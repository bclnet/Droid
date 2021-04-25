#include <pthread.h>
#include <unistd.h>
#include <VrApi.h>
#include <VrApi_Helpers.h>
#include <android/native_window_jni.h>
#include <sys/prctl.h> // for prctl( PR_SET_NAME )
#include "ovr.h"
#include "app.h"
#include "native-log.h"
#include "lib/utils.h"
#include "lib/vmath.h"

static float vrFOV = 0.0f;
static int QUEST_ID = 0;
// passed in from the Java code
static int NUM_MULTI_SAMPLES = -1;
float SS_MULTIPLIER = -1.0f;
static int DISPLAY_REFRESH = -1;
// Let's go to the maximum!
const int CPU_LEVEL = 4;
const int GPU_LEVEL = 4;

/*
================================================================================
ovrEgl
================================================================================
*/

typedef struct {
    EGLint MajorVersion;
    EGLint MinorVersion;
    EGLDisplay Display;
    EGLConfig Config;
    EGLSurface TinySurface;
    EGLSurface MainSurface;
    EGLContext Context;
} ovrEgl;

static void ovrEgl_Clear(ovrEgl *egl) {
    egl->MajorVersion = 0;
    egl->MinorVersion = 0;
    egl->Display = nullptr;
    egl->Config = nullptr;
    egl->TinySurface = EGL_NO_SURFACE;
    egl->MainSurface = EGL_NO_SURFACE;
    egl->Context = EGL_NO_CONTEXT;
}

static void ovrEgl_CreateContext(ovrEgl *egl, const ovrEgl *shareEgl) {
    if (egl->Display)
        return;

    egl->Display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    eglInitialize(egl->Display, &egl->MajorVersion, &egl->MinorVersion);
    // Do NOT use eglChooseConfig, because the Android EGL code pushes in multisample flags in eglChooseConfig if the user has selected the "force 4x MSAA" option in
    // settings, and that is completely wasted for our warp target.
    const int MAX_CONFIGS = 1024;
    EGLConfig configs[MAX_CONFIGS];
    EGLint numConfigs = 0;
    if (!eglGetConfigs(egl->Display, configs, MAX_CONFIGS, &numConfigs)) {
        ALOGE("eglGetConfigs() failed: %s", EglErrorString(eglGetError()));
        return;
    }
    const EGLint configAttribs[] = {
            EGL_RED_SIZE, 8,
            EGL_GREEN_SIZE, 8,
            EGL_BLUE_SIZE, 8,
            EGL_ALPHA_SIZE, 8, // need alpha for the multi-pass timewarp compositor
            EGL_DEPTH_SIZE, 0,
            EGL_STENCIL_SIZE, 0,
            EGL_SAMPLES, 0,
            EGL_NONE
    };
    egl->Config = 0;
    for (auto i = 0; i < numConfigs; i++) {
        EGLint value = 0;

        eglGetConfigAttrib(egl->Display, configs[i], EGL_RENDERABLE_TYPE, &value);
        if ((value & EGL_OPENGL_ES3_BIT_KHR) != EGL_OPENGL_ES3_BIT_KHR)
            continue;

        // The pbuffer config also needs to be compatible with normal window rendering so it can share textures with the window context.
        eglGetConfigAttrib(egl->Display, configs[i], EGL_SURFACE_TYPE, &value);
        if ((value & (EGL_WINDOW_BIT | EGL_PBUFFER_BIT)) != (EGL_WINDOW_BIT | EGL_PBUFFER_BIT))
            continue;

        auto j = 0;
        for (; configAttribs[j] != EGL_NONE; j += 2) {
            eglGetConfigAttrib(egl->Display, configs[i], configAttribs[j], &value);
            if (value != configAttribs[j + 1])
                break;
        }
        if (configAttribs[j] == EGL_NONE) {
            egl->Config = configs[i];
            break;
        }
    }
    if (!egl->Config) {
        ALOGE("eglChooseConfig() failed: %s", EglErrorString(eglGetError()));
        return;
    }
    EGLint contextAttribs[] = {
            EGL_CONTEXT_CLIENT_VERSION, 3,
            EGL_NONE
    };
    ALOGV("Context = eglCreateContext(Display, Config, EGL_NO_CONTEXT, contextAttribs)");
    egl->Context = eglCreateContext(egl->Display, egl->Config, shareEgl ? shareEgl->Context : EGL_NO_CONTEXT, contextAttribs);
    if (!egl->Context) {
        ALOGE("eglCreateContext() failed: %s", EglErrorString(eglGetError()));
        return;
    }
    const EGLint surfaceAttribs[] = {
            EGL_WIDTH, 16,
            EGL_HEIGHT, 16,
            EGL_NONE
    };
    ALOGV("TinySurface = eglCreatePbufferSurface(Display, Config, surfaceAttribs)");
    egl->TinySurface = eglCreatePbufferSurface(egl->Display, egl->Config, surfaceAttribs);
    if (!egl->TinySurface) {
        ALOGE("eglCreatePbufferSurface() failed: %s", EglErrorString(eglGetError()));
        eglDestroyContext(egl->Display, egl->Context);
        egl->Context = EGL_NO_CONTEXT;
        return;
    }
    ALOGV("eglMakeCurrent(Display, TinySurface, TinySurface, Context)");
    if (!eglMakeCurrent(egl->Display, egl->TinySurface, egl->TinySurface, egl->Context)) {
        ALOGE("eglMakeCurrent() failed: %s", EglErrorString(eglGetError()));
        eglDestroySurface(egl->Display, egl->TinySurface);
        eglDestroyContext(egl->Display, egl->Context);
        egl->Context = EGL_NO_CONTEXT;
        return;
    }
}

static void ovrEgl_DestroyContext(ovrEgl *egl) {
    if (egl->Display) {
        ALOGV("eglMakeCurrent(Display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT)");
        if (!eglMakeCurrent(egl->Display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT))
            ALOGE("eglMakeCurrent() failed: %s", EglErrorString(eglGetError()));
    }
    if (egl->Context) {
        ALOGV("eglDestroyContext(Display, Context)");
        if (!eglDestroyContext(egl->Display, egl->Context))
            ALOGE("eglDestroyContext() failed: %s", EglErrorString(eglGetError()));
        egl->Context = EGL_NO_CONTEXT;
    }
    if (egl->TinySurface) {
        ALOGV("eglDestroySurface(Display, TinySurface)");
        if (!eglDestroySurface(egl->Display, egl->TinySurface))
            ALOGE("eglDestroySurface() failed: %s", EglErrorString(eglGetError()));
        egl->TinySurface = EGL_NO_SURFACE;
    }
    if (egl->Display) {
        ALOGV("eglTerminate(Display)");
        if (!eglTerminate(egl->Display))
            ALOGE("eglTerminate() failed: %s", EglErrorString(eglGetError()));
        egl->Display = nullptr;
    }
}

/*
================================================================================
ovrFramebuffer
================================================================================
*/

static void ovrFramebuffer_Clear(ovrFramebuffer *frameBuffer) {
    frameBuffer->Width = 0;
    frameBuffer->Height = 0;
    frameBuffer->Multisamples = 0;
    frameBuffer->TextureSwapChainLength = 0;
    frameBuffer->ProcessingTextureSwapChainIndex = 0;
    frameBuffer->ReadyTextureSwapChainIndex = 0;
    frameBuffer->ColorTextureSwapChain = nullptr;
    frameBuffer->DepthBuffers = nullptr;
    frameBuffer->FrameBuffers = nullptr;
}

typedef void (GL_APIENTRYP PFNGLRENDERBUFFERSTORAGEMULTISAMPLEEXTPROC)(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height);

typedef void (GL_APIENTRYP PFNGLFRAMEBUFFERTEXTURE2DMULTISAMPLEEXTPROC)(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLsizei samples);

#if !defined(GL_OVR_multiview)

/// static const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_NUM_VIEWS_OVR       = 0x9630;
/// static const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_BASE_VIEW_INDEX_OVR = 0x9632;
/// static const int GL_MAX_VIEWS_OVR                                      = 0x9631;
typedef void(GL_APIENTRY *PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVRPROC)(GLenum target, GLenum attachment, GLuint texture, GLint level, GLint baseViewIndex, GLsizei numViews);

#endif

#if !defined(GL_OVR_multiview_multisampled_render_to_texture)

typedef void(GL_APIENTRY *PFNGLFRAMEBUFFERTEXTUREMULTISAMPLEMULTIVIEWOVRPROC)(GLenum target, GLenum attachment, GLuint texture, GLint level, GLsizei samples, GLint baseViewIndex, GLsizei numViews);

#endif

static bool ovrFramebuffer_Create(ovrFramebuffer *frameBuffer, const bool useMultiview, const GLenum colorFormat, const int width, const int height, const int multisamples) {
    PFNGLRENDERBUFFERSTORAGEMULTISAMPLEEXTPROC glRenderbufferStorageMultisampleEXT = (PFNGLRENDERBUFFERSTORAGEMULTISAMPLEEXTPROC) eglGetProcAddress("glRenderbufferStorageMultisampleEXT");
    PFNGLFRAMEBUFFERTEXTURE2DMULTISAMPLEEXTPROC glFramebufferTexture2DMultisampleEXT = (PFNGLFRAMEBUFFERTEXTURE2DMULTISAMPLEEXTPROC) eglGetProcAddress("glFramebufferTexture2DMultisampleEXT");

    PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVRPROC glFramebufferTextureMultiviewOVR = (PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVRPROC) eglGetProcAddress("glFramebufferTextureMultiviewOVR");
    PFNGLFRAMEBUFFERTEXTUREMULTISAMPLEMULTIVIEWOVRPROC glFramebufferTextureMultisampleMultiviewOVR = (PFNGLFRAMEBUFFERTEXTUREMULTISAMPLEMULTIVIEWOVRPROC) eglGetProcAddress("glFramebufferTextureMultisampleMultiviewOVR");

    frameBuffer->Width = width;
    frameBuffer->Height = height;
    frameBuffer->Multisamples = multisamples;
    frameBuffer->UseMultiview = useMultiview && glFramebufferTextureMultiviewOVR ? true : false;

    frameBuffer->ColorTextureSwapChain = vrapi_CreateTextureSwapChain3(frameBuffer->UseMultiview ? VRAPI_TEXTURE_TYPE_2D_ARRAY : VRAPI_TEXTURE_TYPE_2D, colorFormat, width, height, 1, 3);
    frameBuffer->TextureSwapChainLength = vrapi_GetTextureSwapChainLength(frameBuffer->ColorTextureSwapChain);
    frameBuffer->DepthBuffers = (GLuint *) malloc(frameBuffer->TextureSwapChainLength * sizeof(GLuint));
    frameBuffer->FrameBuffers = (GLuint *) malloc(frameBuffer->TextureSwapChainLength * sizeof(GLuint));

    ALOGV("frameBuffer->UseMultiview = %d", frameBuffer->UseMultiview);

    for (auto i = 0; i < frameBuffer->TextureSwapChainLength; i++) {
        // create the color buffer texture.
        const GLuint colorTexture = vrapi_GetTextureSwapChainHandle(frameBuffer->ColorTextureSwapChain, i);
        GLenum colorTextureTarget = frameBuffer->UseMultiview ? GL_TEXTURE_2D_ARRAY : GL_TEXTURE_2D;
        GL(glBindTexture(colorTextureTarget, colorTexture));
        GL(glTexParameteri(colorTextureTarget, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER));
        GL(glTexParameteri(colorTextureTarget, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER));
        GLfloat borderColor[] = {0.0f, 0.0f, 0.0f, 0.0f};
        GL(glTexParameterfv(colorTextureTarget, GL_TEXTURE_BORDER_COLOR, borderColor));
        GL(glTexParameteri(colorTextureTarget, GL_TEXTURE_MIN_FILTER, GL_LINEAR));
        GL(glTexParameteri(colorTextureTarget, GL_TEXTURE_MAG_FILTER, GL_LINEAR));
        GL(glBindTexture(colorTextureTarget, 0));

        if (frameBuffer->UseMultiview) {
            // create the depth buffer texture.
            GL(glGenTextures(1, &frameBuffer->DepthBuffers[i]));
            GL(glBindTexture(GL_TEXTURE_2D_ARRAY, frameBuffer->DepthBuffers[i]));
            GL(glTexStorage3D(GL_TEXTURE_2D_ARRAY, 1, GL_DEPTH_COMPONENT24, width, height, 2));
            GL(glBindTexture(GL_TEXTURE_2D_ARRAY, 0));

            // create the frame buffer.
            GL(glGenFramebuffers(1, &frameBuffer->FrameBuffers[i]));
            GL(glBindFramebuffer(GL_FRAMEBUFFER, frameBuffer->FrameBuffers[i]));
            if (multisamples > 1 && glFramebufferTextureMultisampleMultiviewOVR) {
                GL(glFramebufferTextureMultisampleMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, frameBuffer->DepthBuffers[i], 0 /* level */, multisamples /* samples */, 0 /* baseViewIndex */, 2 /* numViews */));
                GL(glFramebufferTextureMultisampleMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, colorTexture, 0 /* level */, multisamples /* samples */, 0 /* baseViewIndex */, 2 /* numViews */));
            } else {
                GL(glFramebufferTextureMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, frameBuffer->DepthBuffers[i], 0 /* level */, 0 /* baseViewIndex */, 2 /* numViews */));
                GL(glFramebufferTextureMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, colorTexture, 0 /* level */, 0 /* baseViewIndex */, 2 /* numViews */));
            }

            GL(GLenum renderFramebufferStatus = glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER));
            GL(glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0));
            if (renderFramebufferStatus != GL_FRAMEBUFFER_COMPLETE) {
                ALOGE("Incomplete frame buffer object: %s", GlFrameBufferStatusString(renderFramebufferStatus));
                return false;
            }
        } else {
            if (multisamples > 1 && glRenderbufferStorageMultisampleEXT && glFramebufferTexture2DMultisampleEXT) {
                // create multisampled depth buffer.
                GL(glGenRenderbuffers(1, &frameBuffer->DepthBuffers[i]));
                GL(glBindRenderbuffer(GL_RENDERBUFFER, frameBuffer->DepthBuffers[i]));
                GL(glRenderbufferStorageMultisampleEXT(GL_RENDERBUFFER, multisamples, GL_DEPTH_COMPONENT24, width, height));
                GL(glBindRenderbuffer(GL_RENDERBUFFER, 0));

                // create the frame buffer.
                // NOTE: glFramebufferTexture2DMultisampleEXT only works with GL_FRAMEBUFFER.
                GL(glGenFramebuffers(1, &frameBuffer->FrameBuffers[i]));
                GL(glBindFramebuffer(GL_FRAMEBUFFER, frameBuffer->FrameBuffers[i]));
                GL(glFramebufferTexture2DMultisampleEXT(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorTexture, 0, multisamples));
                GL(glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, frameBuffer->DepthBuffers[i]));
                GL(GLenum renderFramebufferStatus = glCheckFramebufferStatus(GL_FRAMEBUFFER));
                GL(glBindFramebuffer(GL_FRAMEBUFFER, 0));
                if (renderFramebufferStatus != GL_FRAMEBUFFER_COMPLETE) {
                    ALOGE("Incomplete frame buffer object: %s", GlFrameBufferStatusString(renderFramebufferStatus));
                    return false;
                }
            } else {
                // create depth buffer.
                GL(glGenRenderbuffers(1, &frameBuffer->DepthBuffers[i]));
                GL(glBindRenderbuffer(GL_RENDERBUFFER, frameBuffer->DepthBuffers[i]));
                GL(glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, width, height));
                GL(glBindRenderbuffer(GL_RENDERBUFFER, 0));

                // create the frame buffer.
                GL(glGenFramebuffers(1, &frameBuffer->FrameBuffers[i]));
                GL(glBindFramebuffer(GL_DRAW_FRAMEBUFFER, frameBuffer->FrameBuffers[i]));
                GL(glFramebufferRenderbuffer(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, frameBuffer->DepthBuffers[i]));
                GL(glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorTexture, 0));
                GL(GLenum renderFramebufferStatus = glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER));
                GL(glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0));
                if (renderFramebufferStatus != GL_FRAMEBUFFER_COMPLETE) {
                    ALOGE("Incomplete frame buffer object: %s", GlFrameBufferStatusString(renderFramebufferStatus));
                    return false;
                }
            }
        }
    }
    return true;
}

void ovrFramebuffer_Destroy(ovrFramebuffer *frameBuffer) {
    GL(glDeleteFramebuffers(frameBuffer->TextureSwapChainLength, frameBuffer->FrameBuffers));
    if (frameBuffer->UseMultiview) {
        GL(glDeleteTextures(frameBuffer->TextureSwapChainLength, frameBuffer->DepthBuffers));
    } else {
        GL(glDeleteRenderbuffers(frameBuffer->TextureSwapChainLength, frameBuffer->DepthBuffers));
    }
    vrapi_DestroyTextureSwapChain(frameBuffer->ColorTextureSwapChain);

    free(frameBuffer->DepthBuffers);
    free(frameBuffer->FrameBuffers);

    ovrFramebuffer_Clear(frameBuffer);
}

void ovrFramebuffer_SetCurrent(ovrFramebuffer *frameBuffer) {
    GL(glBindFramebuffer(GL_FRAMEBUFFER, frameBuffer->FrameBuffers[frameBuffer->ProcessingTextureSwapChainIndex]));
}

void ovrFramebuffer_SetNone() {
    GL(glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0));
}

void ovrFramebuffer_Resolve(ovrFramebuffer *frameBuffer) {
    // Discard the depth buffer, so the tiler won't need to write it back out to memory.
    const GLenum depthAttachment[1] = {GL_DEPTH_ATTACHMENT};
    glInvalidateFramebuffer(GL_DRAW_FRAMEBUFFER, 1, depthAttachment);
}

void ovrFramebuffer_Advance(ovrFramebuffer *frameBuffer) {
    // Advance to the next texture from the set.
    frameBuffer->ReadyTextureSwapChainIndex = frameBuffer->ProcessingTextureSwapChainIndex;
    frameBuffer->ProcessingTextureSwapChainIndex = (frameBuffer->ProcessingTextureSwapChainIndex + 1) % frameBuffer->TextureSwapChainLength;
}

void ovrFramebuffer_ClearEdgeTexels(ovrFramebuffer *frameBuffer) {
    GL(glEnable(GL_SCISSOR_TEST));
    GL(glViewport(0, 0, frameBuffer->Width, frameBuffer->Height));

    // Explicitly clear the border texels to black because OpenGL-ES does not support GL_CLAMP_TO_BORDER. Clear to fully opaque black.
    GL(glClearColor(0.0f, 0.0f, 0.0f, 1.0f));

    // bottom
    GL(glScissor(0, 0, frameBuffer->Width, 1));
    GL(glClear(GL_COLOR_BUFFER_BIT));
    // top
    GL(glScissor(0, frameBuffer->Height - 1, frameBuffer->Width, 1));
    GL(glClear(GL_COLOR_BUFFER_BIT));
    // left
    GL(glScissor(0, 0, 1, frameBuffer->Height));
    GL(glClear(GL_COLOR_BUFFER_BIT));
    // right
    GL(glScissor(frameBuffer->Width - 1, 0, 1, frameBuffer->Height));
    GL(glClear(GL_COLOR_BUFFER_BIT));

    GL(glScissor(0, 0, 0, 0));
    GL(glDisable(GL_SCISSOR_TEST));
}

//void GPUWaitSync() { }

/*
================================================================================
ovrRenderer
================================================================================
*/

void ovrRenderer_Clear(ovrRenderer *renderer) {
    ovrFramebuffer_Clear(&renderer->FrameBuffer);
    renderer->ProjectionMatrix = ovrMatrix4f_CreateIdentity();
    renderer->NumBuffers = VRAPI_FRAME_LAYER_EYE_MAX;
}

float X_GetFOV();

void ovrRenderer_Create(int width, int height, ovrRenderer *renderer, const ovrJava *java) {
    renderer->NumBuffers = 1; // Multiview

    // now using a symmetrical render target, based on the horizontal FOV
    X_GetFOV();

    // Create the multi view frame buffer
    ovrFramebuffer_Create(&renderer->FrameBuffer, true, GL_RGBA8, width, height, NUM_MULTI_SAMPLES);

    // setup the projection matrix.
    renderer->ProjectionMatrix = ovrMatrix4f_CreateProjectionFov(vrFOV, vrFOV, 0.0f, 0.0f, 1.0f, 0.0f);
}

void ovrRenderer_Destroy(ovrRenderer *renderer) {
    ovrFramebuffer_Destroy(&renderer->FrameBuffer);
    renderer->ProjectionMatrix = ovrMatrix4f_CreateIdentity();
}


/*
================================================================================
ovrApp
================================================================================
*/

#define MAX_TRACKING_SAMPLES 4

typedef struct {
    ovrJava Java;
    ovrEgl Egl;
    ANativeWindow *NativeWindow;
    bool Resumed;
    ovrMobile *Ovr;
    ovrScene Scene;
//    SDL_mutex *RenderThreadFrameIndex_Mutex;
    long long RenderThreadFrameIndex;
    long long MainThreadFrameIndex;
    double DisplayTime[MAX_TRACKING_SAMPLES];
    ovrTracking2 Tracking[MAX_TRACKING_SAMPLES];
    int SwapInterval;
    int CpuLevel;
    int GpuLevel;
    int MainThreadTid;
    int RenderThreadTid;
    ovrLayer_Union2 Layers[ovrMaxLayerCount];
    int LayerCount;
    ovrRenderer Renderer;
} ovrApp;

static ovrApp _appState;
static ovrJava _java;
static bool _destroyed = false;
static bool _initialised = false;
static bool _shutdown = false;

static char **_argv;
static int _argc = 0;

appClient _appClient;
appInput _appInput;

static void ovrApp_Clear(ovrApp *app) {
    app->Java.Vm = nullptr;
    app->Java.Env = nullptr;
    app->Java.ActivityObject = nullptr;
    app->Ovr = nullptr;
//    app->RenderThreadFrameIndex_Mutex = SDL_CreateMutex();
    app->RenderThreadFrameIndex = 1;
    app->MainThreadFrameIndex = 1;
    memset(app->DisplayTime, 0, MAX_TRACKING_SAMPLES * sizeof(double));
    memset(app->Tracking, 0, MAX_TRACKING_SAMPLES * sizeof(ovrTracking2));
    app->SwapInterval = 1;
    app->CpuLevel = CPU_LEVEL;
    app->GpuLevel = GPU_LEVEL;
    app->MainThreadTid = 0;
    app->RenderThreadTid = 0;

    ovrEgl_Clear(&app->Egl);

    ovrScene_Clear(&app->Scene);
    ovrRenderer_Clear(&app->Renderer);
}

//static void ovrApp_PushBlackFinal(ovrApp *app) {}

static void ovrApp_HandleVrModeChanges(ovrApp *app) {
    if (app->Resumed && app->NativeWindow) {
        if (app->Ovr)
            return;

        auto sJava = _java;
        sJava.Env = nullptr;
        sJava.Vm->AttachCurrentThread(&sJava.Env, nullptr);

        auto parms = vrapi_DefaultModeParms(&sJava);
        // must reset the FLAG_FULLSCREEN window flag when using a SurfaceView
        parms.Flags |= VRAPI_MODE_FLAG_RESET_WINDOW_FULLSCREEN;

        parms.Flags |= VRAPI_MODE_FLAG_NATIVE_WINDOW;
        parms.Display = (size_t) app->Egl.Display;
        parms.WindowSurface = (size_t) app->NativeWindow;
        parms.ShareContext = (size_t) app->Egl.Context;

        ALOGV("eglGetCurrentSurface(EGL_DRAW) = %p", eglGetCurrentSurface(EGL_DRAW));
        ALOGV("vrapi_EnterVrMode()");
        app->Ovr = vrapi_EnterVrMode(&parms);
        ALOGV("eglGetCurrentSurface(EGL_DRAW) = %p", eglGetCurrentSurface(EGL_DRAW));

        // if entering VR mode failed then the ANativeWindow was not valid.
        if (!app->Ovr) {
            ALOGE("Invalid ANativeWindow!");
            app->NativeWindow = nullptr;
            return;
        }

        // set performance parameters once we have entered VR mode and have a valid ovrMobile.
        vrapi_SetClockLevels(app->Ovr, app->CpuLevel, app->GpuLevel);
        ALOGV("vrapi_SetClockLevels(%d, %d)", app->CpuLevel, app->GpuLevel);
        vrapi_SetPerfThread(app->Ovr, VRAPI_PERF_THREAD_TYPE_MAIN, app->MainThreadTid);
        ALOGV("vrapi_SetPerfThread(MAIN, %d)", app->MainThreadTid);
        vrapi_SetPerfThread(app->Ovr, VRAPI_PERF_THREAD_TYPE_RENDERER, app->RenderThreadTid);
        ALOGV("vrapi_SetPerfThread(RENDERER, %d)", app->RenderThreadTid);
        vrapi_SetExtraLatencyMode(app->Ovr, VRAPI_EXTRA_LATENCY_MODE_ON);
    } else {
        if (!app->Ovr)
            return;

        ALOGV("eglGetCurrentSurface(EGL_DRAW) = %p", eglGetCurrentSurface(EGL_DRAW));
        ALOGV("vrapi_LeaveVrMode()");
        vrapi_LeaveVrMode(app->Ovr);
        app->Ovr = nullptr;
        ALOGV("eglGetCurrentSurface(EGL_DRAW) = %p", eglGetCurrentSurface(EGL_DRAW));
    }
}

float X_GetFOV() {
    vrFOV = vrapi_GetSystemPropertyInt(&_appState.Java, VRAPI_SYS_PROP_SUGGESTED_EYE_FOV_DEGREES_Y);
    return vrFOV;
}

int X_GetRefreshRate() {
    return _initialised ? vrapi_GetSystemPropertyInt(&_appState.Java, VRAPI_SYS_PROP_DISPLAY_REFRESH_RATE) : 60;
}

/*
================================================================================
ovrMessageQueue
================================================================================
*/

typedef enum {
    MQ_WAIT_NONE,        // don't wait
    MQ_WAIT_RECEIVED,    // wait until the consumer thread has received the message
    MQ_WAIT_PROCESSED    // wait until the consumer thread has processed the message
} ovrMQWait;

#define MAX_MESSAGE_PARMS    8
#define MAX_MESSAGES        1024

typedef struct {
    int Id;
    ovrMQWait Wait;
    long long Parms[MAX_MESSAGE_PARMS];
} ovrMessage;

static void ovrMessage_Init(ovrMessage *message, const int id, const int wait) {
    message->Id = id;
    message->Wait = (ovrMQWait) wait;
    memset(message->Parms, 0, sizeof(message->Parms));
}

static void ovrMessage_SetPointerParm(ovrMessage *message, int index, void *ptr) { *(void **) &message->Parms[index] = ptr; }

static void *ovrMessage_GetPointerParm(ovrMessage *message, int index) { return *(void **) &message->Parms[index]; }

static void ovrMessage_SetIntegerParm(ovrMessage *message, int index, int value) { message->Parms[index] = value; }

static int ovrMessage_GetIntegerParm(ovrMessage *message, int index) { return (int) message->Parms[index]; }

static void ovrMessage_SetFloatParm(ovrMessage *message, int index, float value) { *(float *) &message->Parms[index] = value; }

static float ovrMessage_GetFloatParm(ovrMessage *message, int index) { return *(float *) &message->Parms[index]; }

// cyclic queue with messages.
typedef struct {
    ovrMessage Messages[MAX_MESSAGES];
    volatile int Head;    // dequeue at the head
    volatile int Tail;    // enqueue at the tail
    ovrMQWait Wait;
    volatile bool EnabledFlag;
    volatile bool PostedFlag;
    volatile bool ReceivedFlag;
    volatile bool ProcessedFlag;
    pthread_mutex_t Mutex;
    pthread_cond_t PostedCondition;
    pthread_cond_t ReceivedCondition;
    pthread_cond_t ProcessedCondition;
} ovrMessageQueue;

static void ovrMessageQueue_Create(ovrMessageQueue *messageQueue) {
    messageQueue->Head = 0;
    messageQueue->Tail = 0;
    messageQueue->Wait = MQ_WAIT_NONE;
    messageQueue->EnabledFlag = false;
    messageQueue->PostedFlag = false;
    messageQueue->ReceivedFlag = false;
    messageQueue->ProcessedFlag = false;

    pthread_mutexattr_t attr;
    pthread_mutexattr_init(&attr);
    pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_ERRORCHECK);
    pthread_mutex_init(&messageQueue->Mutex, &attr);
    pthread_mutexattr_destroy(&attr);
    pthread_cond_init(&messageQueue->PostedCondition, nullptr);
    pthread_cond_init(&messageQueue->ReceivedCondition, nullptr);
    pthread_cond_init(&messageQueue->ProcessedCondition, nullptr);
}

static void ovrMessageQueue_Destroy(ovrMessageQueue *messageQueue) {
    pthread_mutex_destroy(&messageQueue->Mutex);
    pthread_cond_destroy(&messageQueue->PostedCondition);
    pthread_cond_destroy(&messageQueue->ReceivedCondition);
    pthread_cond_destroy(&messageQueue->ProcessedCondition);
}

static void ovrMessageQueue_Enable(ovrMessageQueue *messageQueue, const bool set) {
    messageQueue->EnabledFlag = set;
}

static void ovrMessageQueue_PostMessage(ovrMessageQueue *messageQueue, const ovrMessage *message) {
    if (!messageQueue->EnabledFlag)
        return;
    while (messageQueue->Tail - messageQueue->Head >= MAX_MESSAGES)
        usleep(1000);
    pthread_mutex_lock(&messageQueue->Mutex);
    messageQueue->Messages[messageQueue->Tail & (MAX_MESSAGES - 1)] = *message;
    messageQueue->Tail++;
    messageQueue->PostedFlag = true;
    pthread_cond_broadcast(&messageQueue->PostedCondition);
    if (message->Wait == MQ_WAIT_RECEIVED) {
        while (!messageQueue->ReceivedFlag)
            pthread_cond_wait(&messageQueue->ReceivedCondition, &messageQueue->Mutex);
        messageQueue->ReceivedFlag = false;
    } else if (message->Wait == MQ_WAIT_PROCESSED) {
        while (!messageQueue->ProcessedFlag)
            pthread_cond_wait(&messageQueue->ProcessedCondition, &messageQueue->Mutex);
        messageQueue->ProcessedFlag = false;
    }
    pthread_mutex_unlock(&messageQueue->Mutex);
}

static void ovrMessageQueue_SleepUntilMessage(ovrMessageQueue *messageQueue) {
    if (messageQueue->Wait == MQ_WAIT_PROCESSED) {
        messageQueue->ProcessedFlag = true;
        pthread_cond_broadcast(&messageQueue->ProcessedCondition);
        messageQueue->Wait = MQ_WAIT_NONE;
    }
    pthread_mutex_lock(&messageQueue->Mutex);
    if (messageQueue->Tail > messageQueue->Head) {
        pthread_mutex_unlock(&messageQueue->Mutex);
        return;
    }
    while (!messageQueue->PostedFlag)
        pthread_cond_wait(&messageQueue->PostedCondition, &messageQueue->Mutex);
    messageQueue->PostedFlag = false;
    pthread_mutex_unlock(&messageQueue->Mutex);
}

static bool ovrMessageQueue_GetNextMessage(ovrMessageQueue *messageQueue, ovrMessage *message, bool waitForMessages) {
    if (messageQueue->Wait == MQ_WAIT_PROCESSED) {
        messageQueue->ProcessedFlag = true;
        pthread_cond_broadcast(&messageQueue->ProcessedCondition);
        messageQueue->Wait = MQ_WAIT_NONE;
    }
    if (waitForMessages)
        ovrMessageQueue_SleepUntilMessage(messageQueue);
    pthread_mutex_lock(&messageQueue->Mutex);
    if (messageQueue->Tail <= messageQueue->Head) {
        pthread_mutex_unlock(&messageQueue->Mutex);
        return false;
    }
    *message = messageQueue->Messages[messageQueue->Head & (MAX_MESSAGES - 1)];
    messageQueue->Head++;
    pthread_mutex_unlock(&messageQueue->Mutex);
    if (message->Wait == MQ_WAIT_RECEIVED) {
        messageQueue->ReceivedFlag = true;
        pthread_cond_broadcast(&messageQueue->ReceivedCondition);
    } else if (message->Wait == MQ_WAIT_PROCESSED)
        messageQueue->Wait = MQ_WAIT_PROCESSED;
    return true;
}


/*
================================================================================
Vec
================================================================================
*/

#ifndef VEC_EPSILON
#define VEC_EPSILON 0.001f
#endif

static ovrVector3f Vec_NormalizeVec(ovrVector3f vec) {
    // note: leave w-component untouched
    auto xxyyzz = vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
    auto invLength = 1.0f / sqrtf(xxyyzz);
    ovrVector3f r;
    r.x = vec.x * invLength;
    r.y = vec.y * invLength;
    r.z = vec.z * invLength;
    return r;
}

void Vec_NormalizeAngles(vec3_t angles) {
    while (angles[0] >= 90) angles[0] -= 180;
    while (angles[1] >= 180) angles[1] -= 360;
    while (angles[2] >= 180) angles[2] -= 360;
    while (angles[0] < -90) angles[0] += 180;
    while (angles[1] < -180) angles[1] += 360;
    while (angles[2] < -180) angles[2] += 360;
}

void Vec_GetAnglesFromVectors(const ovrVector3f forward, const ovrVector3f right, const ovrVector3f up, vec3_t angles) {
    float sr, sp, sy, cr, cp, cy;

    sp = -forward.z;

    auto cp_x_cy = forward.x;
    auto cp_x_sy = forward.y;
    auto cp_x_sr = -right.z;
    auto cp_x_cr = up.z;

    auto yaw = atan2(cp_x_sy, cp_x_cy);
    auto roll = atan2(cp_x_sr, cp_x_cr);

    cy = cos(yaw);
    sy = sin(yaw);
    cr = cos(roll);
    sr = sin(roll);

    if (fabs(cy) > VEC_EPSILON) cp = cp_x_cy / cy;
    else if (fabs(sy) > VEC_EPSILON) cp = cp_x_sy / sy;
    else if (fabs(sr) > VEC_EPSILON) cp = cp_x_sr / sr;
    else if (fabs(cr) > VEC_EPSILON) cp = cp_x_cr / cr;
    else cp = cos(asin(sp));

    auto pitch = atan2(sp, cp);

    angles[0] = pitch / (M_PI * 2.f / 360.f);
    angles[1] = yaw / (M_PI * 2.f / 360.f);
    angles[2] = roll / (M_PI * 2.f / 360.f);

    Vec_NormalizeAngles(angles);
}

void Vec_QuatToYawPitchRoll(ovrQuatf q, vec3_t rotation, vec3_t out) {
    auto mat = ovrMatrix4f_CreateFromQuaternion(&q);
    if (rotation[0] != 0.0f || rotation[1] != 0.0f || rotation[2] != 0.0f) {
        auto rot = ovrMatrix4f_CreateRotation(M_DEG2RAD(rotation[0]), M_DEG2RAD(rotation[1]), M_DEG2RAD(rotation[2]));
        mat = ovrMatrix4f_Multiply(&mat, &rot);
    }

    ovrVector4f v1 = {0, 0, -1, 0};
    ovrVector4f v2 = {1, 0, 0, 0};
    ovrVector4f v3 = {0, 1, 0, 0};

    auto forwardInVRSpace = ovrVector4f_MultiplyMatrix4f(&mat, &v1);
    auto rightInVRSpace = ovrVector4f_MultiplyMatrix4f(&mat, &v2);
    auto upInVRSpace = ovrVector4f_MultiplyMatrix4f(&mat, &v3);

    ovrVector3f forward = {-forwardInVRSpace.z, -forwardInVRSpace.x, forwardInVRSpace.y};
    ovrVector3f right = {-rightInVRSpace.z, -rightInVRSpace.x, rightInVRSpace.y};
    ovrVector3f up = {-upInVRSpace.z, -upInVRSpace.x, upInVRSpace.y};

    auto forwardNormal = Vec_NormalizeVec(forward);
    auto rightNormal = Vec_NormalizeVec(right);
    auto upNormal = Vec_NormalizeVec(up);

    Vec_GetAnglesFromVectors(forwardNormal, rightNormal, upNormal, out);
}

void Vec_UpdateHMDOrientation() {
    // position
    M_Vector3Sub(_appClient.hmdposition_last, _appClient.hmdposition, _appClient.hmdposition_delta);
    // keep this for our records
    M_Vector3Cpy(_appClient.hmdposition, _appClient.hmdposition_last);
}

void Vec_SetHMDPosition(float x, float y, float z, float yaw) {
    M_Vector3Set(_appClient.hmdposition, x, y, z);
}

void Vec_SetHMDOrientation(float x, float y, float z, float w) {
    M_Vector4Set(_appClient.hmdorientation_quat, x, y, z, w);
}

void Vec_SetHMDTranslation(float x, float y, float z) {
    M_Vector3Set(_appClient.hmdtranslation, x, y, z);
}

/*
================================================================================
ovrAppThread
================================================================================
*/

enum {
    MESSAGE_ON_CREATE,
    MESSAGE_ON_START,
    MESSAGE_ON_RESUME,
    MESSAGE_ON_PAUSE,
    MESSAGE_ON_STOP,
    MESSAGE_ON_DESTROY,
    MESSAGE_ON_SURFACE_CREATED,
    MESSAGE_ON_SURFACE_DESTROYED
};

typedef struct {
    JavaVM *JavaVm;
    jobject ActivityObject;
    jclass ActivityClass;
    pthread_t Thread;
    ovrMessageQueue MessageQueue;
    ANativeWindow *NativeWindow;
} ovrAppThread;

static ovrAppThread *_appThread = nullptr;
static long _renderThreadCPUTime = 0L;

bool App_ProcessMessageQueue() {
    for (;;) {
        ovrMessage message;
        const bool waitForMessages = !_appState.Ovr && !_destroyed;
        if (!ovrMessageQueue_GetNextMessage(&_appThread->MessageQueue, &message, waitForMessages))
            break;

        switch (message.Id) {
            case MESSAGE_ON_CREATE:
                break;
            case MESSAGE_ON_START:
                if (!_initialised) {
                    // set command line arguments here
                    if (_argc != 0) {}
                    else {
                        int argc = 1;
                        char *argv[] = {"app"};
                    }
                    _initialised = true;
                }
                break;
            case MESSAGE_ON_RESUME:
                _appState.Resumed = true; // if we get here, then user has opted not to quit
                break;
            case MESSAGE_ON_PAUSE:
                _appState.Resumed = false;
                break;
            case MESSAGE_ON_STOP:
                break;
            case MESSAGE_ON_DESTROY: {
                _appState.NativeWindow = nullptr;
                _destroyed = true;
                _shutdown = true;
                break;
            }
            case MESSAGE_ON_SURFACE_CREATED:
                _appState.NativeWindow = (ANativeWindow *) ovrMessage_GetPointerParm(&message, 0);
                break;
            case MESSAGE_ON_SURFACE_DESTROYED:
                _appState.NativeWindow = nullptr;
                break;
        }

        ovrApp_HandleVrModeChanges(&_appState);
    }

    return true;
}

void App_Init() {
    // initialise all our variables
    _appClient.screenYaw = 0.0f;
    _appClient.visible_hud = true;
    _appInput.remote_movementSideways = 0.0f;
    _appInput.remote_movementForward = 0.0f;
    _appInput.remote_movementUp = 0.0f;
    _appInput.positional_movementSideways = 0.0f;
    _appInput.positional_movementForward = 0.0f;
    _appInput.snapTurn = 0.0f;

    // init randomiser
    srand(time(nullptr));

    _shutdown = false;
}

bool forceVirtualScreen = false;
bool inMenu = false;
bool inGameGuiActive = false;
bool objectiveSystemActive = false;
bool inCinematic = false;
bool loading = false;

void App_SetScreenLayer(int screen) {
    inMenu = screen & 0x1;
    inGameGuiActive = !!(screen & 0x2);
    objectiveSystemActive = !!(screen & 0x4);
    inCinematic = !!(screen & 0x8);
    loading = !!(screen & 0x10);
}

bool App_UseScreenLayer() {
    return inMenu || forceVirtualScreen || inCinematic || loading;
}

void App_PrepareEyeBuffer() {
    auto renderer = App_UseScreenLayer() ? &_appState.Scene.CylinderRenderer : &_appState.Renderer;

    auto frameBuffer = &renderer->FrameBuffer;
    ovrFramebuffer_SetCurrent(frameBuffer);

    _renderThreadCPUTime = GetTimeInMilliSeconds();

    GL(glEnable(GL_SCISSOR_TEST));
    GL(glDepthMask(GL_TRUE));
    GL(glEnable(GL_DEPTH_TEST));
    GL(glDepthFunc(GL_LEQUAL));

    //Weusing the size of the render target
    GL(glViewport(0, 0, frameBuffer->Width, frameBuffer->Height));
    GL(glScissor(0, 0, frameBuffer->Width, frameBuffer->Height));

    GL(glClearColor(0.0f, 0.0f, 0.0f, 1.0f));
    GL(glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT));
    GL(glDisable(GL_SCISSOR_TEST));
}

void App_FinishEyeBuffer() {
    _renderThreadCPUTime = GetTimeInMilliSeconds() - _renderThreadCPUTime;
    ALOGI("RENDER THREAD TOTAL CPU TIME: %ld", _renderThreadCPUTime);

    GLCheckErrors(__LINE__);

    auto renderer = App_UseScreenLayer() ? &_appState.Scene.CylinderRenderer : &_appState.Renderer;

    auto frameBuffer = &renderer->FrameBuffer;

    // clear edge to prevent smearing
    ovrFramebuffer_ClearEdgeTexels(frameBuffer);
    ovrFramebuffer_Resolve(frameBuffer);
    ovrFramebuffer_Advance(frameBuffer);

    ovrFramebuffer_SetNone();
}

void App_Shutdown() {
//    SDL_DestroyMutex(_appState.RenderThreadFrameIndex_Mutex);
    ovrRenderer_Destroy(&_appState.Renderer);
    ovrEgl_DestroyContext(&_appState.Egl);
    _java.Vm->DetachCurrentThread();
    vrapi_Shutdown();
}

// Called before SDL_main() to initialize JNI bindings in SDL library
//extern void SDL_Android_Init(JNIEnv *env, jclass cls);

// called on the main thread before the rendering thread is started
void App_DeactivateContext() {
    eglMakeCurrent(_appState.Egl.Display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
}

// called by the rendering thread to take charge of the context
void App_ActivateContext() {
    eglMakeCurrent(_appState.Egl.Display, _appState.Egl.TinySurface, _appState.Egl.TinySurface, _appState.Egl.Context);
    _appState.RenderThreadTid = gettid();
}

// 0 = left, 1 = right
float _appHaptics_channel[2][2] = {{0.0f, 0.0f},
                                   {0.0f, 0.0f}};

void App_SetHaptics(int channel, float low, float high) {
    _appHaptics_channel[channel][0] = low;
    _appHaptics_channel[channel][1] = high;
}

void App_ProcessHaptics() {
    float beat;
    bool enable;
    for (auto h = 0; h < 2; ++h) {
        beat = fabs(_appHaptics_channel[h][0] - _appHaptics_channel[h][1]) / 65535;
        vrapi_SetHapticVibrationSimple(_appState.Ovr, _appInput.controllerIDs[1 - h], beat > 0.0f ? beat : 0.0f);
    }
}

void App_ShowLoadingIcon() {
    auto frameFlags = 0;
    frameFlags |= VRAPI_FRAME_FLAG_FLUSH;

    auto blackLayer = vrapi_DefaultLayerBlackProjection2();
    blackLayer.Header.Flags |= VRAPI_FRAME_LAYER_FLAG_INHIBIT_SRGB_FRAMEBUFFER;

    ovrLayerLoadingIcon2 iconLayer = vrapi_DefaultLayerLoadingIcon2();
    iconLayer.Header.Flags |= VRAPI_FRAME_LAYER_FLAG_INHIBIT_SRGB_FRAMEBUFFER;

    const ovrLayerHeader2 *layers[] = {&blackLayer.Header, &iconLayer.Header};

    ovrSubmitFrameDescription2 frameDesc = {};
    {
//        SDL_LockMutex(_appState.RenderThreadFrameIndex_Mutex);
        frameDesc.Flags = frameFlags;
        frameDesc.SwapInterval = 1;
        frameDesc.FrameIndex = _appState.RenderThreadFrameIndex;
        frameDesc.DisplayTime = _appState.DisplayTime[_appState.RenderThreadFrameIndex % MAX_TRACKING_SAMPLES];
        frameDesc.LayerCount = 2;
        frameDesc.Layers = layers;
        _appState.RenderThreadFrameIndex++;
//        SDL_UnlockMutex(_appState.RenderThreadFrameIndex_Mutex);
    }
    vrapi_SubmitFrame2(_appState.Ovr, &frameDesc);
}

void App_GetHMDOrientation() {
    // update the main thread frame index in a thread safe way
    {
//        SDL_LockMutex(_appState.RenderThreadFrameIndex_Mutex);
        _appState.MainThreadFrameIndex = _appState.RenderThreadFrameIndex + 1;
//        SDL_UnlockMutex(_appState.RenderThreadFrameIndex_Mutex);
    }

    _appState.DisplayTime[_appState.MainThreadFrameIndex % MAX_TRACKING_SAMPLES] = vrapi_GetPredictedDisplayTime(_appState.Ovr, _appState.MainThreadFrameIndex);

    auto tracking = &_appState.Tracking[_appState.MainThreadFrameIndex % MAX_TRACKING_SAMPLES];
    *tracking = vrapi_GetPredictedTracking2(_appState.Ovr, _appState.DisplayTime[_appState.MainThreadFrameIndex % MAX_TRACKING_SAMPLES]);

    // Don't update game with tracking if we are in big screen mode
    // Do pass the stuff but block at my end (if big screen prompt is needed)
    const auto quatHmd = tracking->HeadPose.Pose.Orientation;
    const auto positionHmd = tracking->HeadPose.Pose.Position;
//    const auto translationHmd = tracking->HeadPose.Pose.Translation;
    vec3_t rotation = {0};
    Vec_QuatToYawPitchRoll(quatHmd, rotation, _appClient.hmdorientation_temp);
    Vec_SetHMDPosition(positionHmd.x, positionHmd.y, positionHmd.z, 0);
    Vec_SetHMDOrientation(quatHmd.x, quatHmd.y, quatHmd.z, quatHmd.w);
    //Vec_SetHMDTranslation(translationHmd.x, translationHmd.y, translationHmd.z);
    Vec_UpdateHMDOrientation();
}

void Input_AcquireTrackedRemotesData(ovrMobile *Ovr, double displayTime);

void Input_HandleDefault(int controlScheme, int switchSticks,
                         ovrInputStateGamepad *footTrackingNew, ovrInputStateGamepad *footTrackingOld,
                         ovrInputStateTrackedRemote *dominantTrackedRemoteNew, ovrInputStateTrackedRemote *dominantTrackedRemoteOld,
                         ovrTracking *dominantTracking,
                         ovrInputStateTrackedRemote *offTrackedRemoteNew, ovrInputStateTrackedRemote *offTrackedRemoteOld,
                         ovrTracking *offTracking,
                         int domButton1, int domButton2, int offButton1, int offButton2);

void App_GetTrackedRemotesOrientation(int controlScheme, int switchSticks) {
    // get info for tracked remotes
    Input_AcquireTrackedRemotesData(_appState.Ovr, _appState.DisplayTime[_appState.MainThreadFrameIndex % MAX_TRACKING_SAMPLES]);

    // call additional control schemes here
    if (controlScheme == 0)
        Input_HandleDefault(controlScheme, switchSticks,
                            &_appInput.footTrackedRemoteState_new, &_appInput.footTrackedRemoteState_old,
                            &_appInput.rightTrackedRemoteState_new, &_appInput.rightTrackedRemoteState_old,
                            &_appInput.rightRemoteTracking_new,
                            &_appInput.leftTrackedRemoteState_new, &_appInput.leftTrackedRemoteState_old,
                            &_appInput.leftRemoteTracking_new,
                            ovrButton_A, ovrButton_B, ovrButton_X, ovrButton_Y);
    else
        Input_HandleDefault(controlScheme, switchSticks,
                            &_appInput.footTrackedRemoteState_new, &_appInput.footTrackedRemoteState_old,
                            &_appInput.leftTrackedRemoteState_new, &_appInput.leftTrackedRemoteState_old,
                            &_appInput.leftRemoteTracking_new,
                            &_appInput.rightTrackedRemoteState_new, &_appInput.rightTrackedRemoteState_old,
                            &_appInput.rightRemoteTracking_new,
                            ovrButton_X, ovrButton_Y, ovrButton_A, ovrButton_B);
}

// all the stuff we want to do each frame
void App_FrameSetup(int controlScheme, int switchSticks, int refreshRate) {
    ALOGV("RefreshRate = %i", refreshRate);

    //Use floor based tracking space
    vrapi_SetTrackingSpace(_appState.Ovr, VRAPI_TRACKING_SPACE_LOCAL_FLOOR);

    auto device = vrapi_GetSystemPropertyInt(&_java, VRAPI_SYS_PROP_DEVICE_TYPE);
    switch (device) {
        case VRAPI_DEVICE_TYPE_OCULUSQUEST:
            // force 60hz for Quest 1
            vrapi_SetDisplayRefreshRate(_appState.Ovr, 60);
            break;
        case VRAPI_DEVICE_TYPE_OCULUSQUEST2:
            vrapi_SetDisplayRefreshRate(_appState.Ovr, refreshRate);
            break;
    }

    if (!App_UseScreenLayer())
        _appClient.screenYaw = _appClient.hmdorientation_temp[M_YAW];

    App_ProcessHaptics();
    App_GetHMDOrientation();
    App_GetTrackedRemotesOrientation(controlScheme, switchSticks);
}

void App_SubmitFrame() {
    ovrSubmitFrameDescription2 frameDesc = {0};

    long long renderThreadFrameIndex;
    {
//        SDL_LockMutex(_appState.RenderThreadFrameIndex_Mutex);
        renderThreadFrameIndex = _appState.RenderThreadFrameIndex;
//        SDL_UnlockMutex(_appState.RenderThreadFrameIndex_Mutex);
    }

    if (!App_UseScreenLayer()) {
        auto layer = vrapi_DefaultLayerProjection2();
        layer.HeadPose = _appState.Tracking[renderThreadFrameIndex % MAX_TRACKING_SAMPLES].HeadPose;
        for (auto eye = 0; eye < VRAPI_FRAME_LAYER_EYE_MAX; eye++) {
            auto frameBuffer = &_appState.Renderer.FrameBuffer;
            layer.Textures[eye].ColorSwapChain = frameBuffer->ColorTextureSwapChain;
            layer.Textures[eye].SwapChainIndex = frameBuffer->ReadyTextureSwapChainIndex;

            ovrMatrix4f projectionMatrix;
            projectionMatrix = ovrMatrix4f_CreateProjectionFov(vrFOV, vrFOV, 0.0f, 0.0f, 0.1f, 0.0f);

            layer.Textures[eye].TexCoordsFromTanAngles = ovrMatrix4f_TanAngleMatrixFromProjection(&projectionMatrix);

            layer.Textures[eye].TextureRect.x = 0;
            layer.Textures[eye].TextureRect.y = 0;
            layer.Textures[eye].TextureRect.width = 1.0f;
            layer.Textures[eye].TextureRect.height = 1.0f;
        }
        layer.Header.Flags |= VRAPI_FRAME_LAYER_FLAG_CHROMATIC_ABERRATION_CORRECTION;

        // set up the description for this frame.
        const ovrLayerHeader2 *layers[] = {&layer.Header};

        frameDesc.Flags = 0;
        frameDesc.SwapInterval = _appState.SwapInterval;
        frameDesc.FrameIndex = renderThreadFrameIndex;
        frameDesc.DisplayTime = _appState.DisplayTime[renderThreadFrameIndex % MAX_TRACKING_SAMPLES];
        frameDesc.LayerCount = 1;
        frameDesc.Layers = layers;

        // hand over the eye images to the time warp.
        vrapi_SubmitFrame2(_appState.Ovr, &frameDesc);

    } else {
        // set-up the compositor layers for this frame. NOTE: Multiple independent layers are allowed, but they need to be added in a depth consistent order.
        memset(_appState.Layers, 0, sizeof(ovrLayer_Union2) * ovrMaxLayerCount);
        _appState.LayerCount = 0;

        // add a simple cylindrical layer
        _appState.Layers[_appState.LayerCount++].Cylinder = BuildCylinderLayer(&_appState.Scene.CylinderRenderer, _appState.Scene.CylinderWidth, _appState.Scene.CylinderHeight, &_appState.Tracking[renderThreadFrameIndex % MAX_TRACKING_SAMPLES], M_DEG2RAD(_appClient.playerYaw));

        // compose the layers for this frame.
        const ovrLayerHeader2 *layerHeaders[ovrMaxLayerCount] = {0};
        for (auto i = 0; i < _appState.LayerCount; i++)
            layerHeaders[i] = &_appState.Layers[i].Header;


        // set up the description for this frame.
        frameDesc.Flags = 0;
        frameDesc.SwapInterval = _appState.SwapInterval;
        frameDesc.FrameIndex = renderThreadFrameIndex;
        frameDesc.DisplayTime = _appState.DisplayTime[renderThreadFrameIndex % MAX_TRACKING_SAMPLES];
        frameDesc.LayerCount = _appState.LayerCount;
        frameDesc.Layers = layerHeaders;

        // hand over the eye images to the time warp.
        vrapi_SubmitFrame2(_appState.Ovr, &frameDesc);
    }

    {
//        SDL_LockMutex(_appState.RenderThreadFrameIndex_Mutex);
        _appState.RenderThreadFrameIndex++;
//        SDL_UnlockMutex(_appState.RenderThreadFrameIndex_Mutex);
    }
}

void JNI_Shutdown();

void AppMain(int argc, char **argv);

void *AppThreadFunction(void *parm) {
    _appThread = (ovrAppThread *) parm;

    _java.Vm = _appThread->JavaVm;
    _java.Vm->AttachCurrentThread(&_java.Env, nullptr);
    _java.ActivityObject = _appThread->ActivityObject;

    jclass cls = _java.Env->GetObjectClass(_java.ActivityObject);

    // This interface could expand with ABI negotiation, callbacks, etc.
//    SDL_Android_Init(_java.Env, cls);
//    SDL_SetMainReady();

    // note that AttachCurrentThread will reset the thread name.
    prctl(PR_SET_NAME, (long) "App::Main", 0, 0, 0);

    _initialised = false;

    const auto initParms = vrapi_DefaultInitParms(&_java);
    auto initResult = vrapi_Initialize(&initParms);
    if (initResult != VRAPI_INITIALIZE_SUCCESS)
        exit(0); // if intialization failed, vrapi_* function calls will not be available.

    App_Init();

    ovrApp_Clear(&_appState);
    _appState.Java = _java;

    // This app will handle android gamepad events itself.
    vrapi_SetPropertyInt(&_appState.Java, VRAPI_EAT_NATIVE_GAMEPAD_EVENTS, 0);

    _appState.CpuLevel = CPU_LEVEL;
    _appState.GpuLevel = GPU_LEVEL;
    _appState.MainThreadTid = gettid();

    ovrEgl_CreateContext(&_appState.Egl, nullptr);

    EglInitExtensions();

    chdir("/sdcard/Droid");

    // This app will handle android gamepad events itself.
    vrapi_SetPropertyInt(&_appState.Java, VRAPI_EAT_NATIVE_GAMEPAD_EVENTS, 0);

    //Set device defaults
    if (vrapi_GetSystemPropertyInt(&_java, VRAPI_SYS_PROP_DEVICE_TYPE) == VRAPI_DEVICE_TYPE_OCULUSQUEST) {
        QUEST_ID = 1;
        DISPLAY_REFRESH = 60; // Fixed to 60 for oculus 1
        if (SS_MULTIPLIER == -1.0f)
            SS_MULTIPLIER = 1.0f;
        if (NUM_MULTI_SAMPLES == -1)
            NUM_MULTI_SAMPLES = 1;
    } else if (vrapi_GetSystemPropertyInt(&_java, VRAPI_SYS_PROP_DEVICE_TYPE) == VRAPI_DEVICE_TYPE_OCULUSQUEST2) {
        QUEST_ID = 2;
        if (SS_MULTIPLIER == -1.0f)
            SS_MULTIPLIER = 1.1f;
        if (NUM_MULTI_SAMPLES == -1)
            NUM_MULTI_SAMPLES = 2;
    } else {
        ALOGE("Don't know what headset this is!? abort");
        return nullptr;
    }

    // using a symmetrical render target
    _appClient.height = _appClient.width = (int) (vrapi_GetSystemPropertyInt(&_java, VRAPI_SYS_PROP_SUGGESTED_EYE_TEXTURE_WIDTH) * SS_MULTIPLIER);

    // first handle any messages in the queue
    while (!_appState.Ovr)
        App_ProcessMessageQueue();

    ovrRenderer_Create(_appClient.width, _appClient.height, &_appState.Renderer, &_java);

    if (!_appState.Ovr)
        return nullptr;

    // create the scene if not yet created.
    ovrScene_Create(_appClient.width, _appClient.height, &_appState.Scene, &_java);

    // run loading loop until we are ready to start App
    while (!_destroyed && !_initialised) {
        App_ProcessMessageQueue();
        App_GetHMDOrientation();
        App_ShowLoadingIcon();
    }

    // should now be all set up and ready - start the App main loop
    AppMain(_argc, _argv);

    // take the context back
    App_ActivateContext();

    // we are done, shutdown cleanly
    App_Shutdown();

    // ask Java to shut down
    JNI_Shutdown();

    return nullptr;
}

static void ovrAppThread_Create(ovrAppThread *appThread, JNIEnv *env, jobject activityObject, jclass activityClass) {
    env->GetJavaVM(&appThread->JavaVm);
    appThread->ActivityObject = env->NewGlobalRef(activityObject);
    appThread->ActivityClass = (jclass) env->NewGlobalRef(activityClass);
    appThread->Thread = 0;
    appThread->NativeWindow = nullptr;
    ovrMessageQueue_Create(&appThread->MessageQueue);

    const int createError = pthread_create(&appThread->Thread, nullptr, AppThreadFunction, appThread);
    if (createError)
        ALOGE("pthread_create returned %i", createError);
}

static void ovrAppThread_Destroy(ovrAppThread *appThread, JNIEnv *env) {
    pthread_join(appThread->Thread, nullptr);
    env->DeleteGlobalRef(appThread->ActivityObject);
    env->DeleteGlobalRef(appThread->ActivityClass);
    ovrMessageQueue_Destroy(&appThread->MessageQueue);
}


/*
================================================================================
Activity lifecycle
================================================================================
*/

static jmethodID _android_shutdown;
static JavaVM *_jVM;
static jobject _shutdownCallbackObj = nullptr;

jint JNI_OnLoad(JavaVM *vm, void *reserved) {
    ALOGV("DROIDVR_OnLoad");
    JNIEnv *env;
    _jVM = vm;
    if (_jVM->GetEnv((void **) &env, JNI_VERSION_1_4) != JNI_OK) {
        ALOGE("Failed DROIDVR_OnLoad");
        return -1;
    }
    return JNI_VERSION_1_4;
}

void JNI_Shutdown() {
    ALOGV("DROIDVR_Shutdown");
    JNIEnv *env;
    if (_jVM->GetEnv((void **) &env, JNI_VERSION_1_4) < 0)
        _jVM->AttachCurrentThread(&env, nullptr);
    return env->CallVoidMethod(_shutdownCallbackObj, _android_shutdown);
}

extern "C" JNIEXPORT jlong JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onCreate(JNIEnv *env, jclass activityClass, jobject activity, jstring commandLineParams, jlong refresh, jfloat ss, jlong msaa) {
    ALOGV("vr::onCreate()");

    jboolean iscopy;
    const auto arg = env->GetStringUTFChars(commandLineParams, &iscopy);

    auto cmdLine = arg && strlen(arg) ? strdup(arg) : nullptr;
    env->ReleaseStringUTFChars(commandLineParams, arg);
    ALOGV("Command line %s", cmdLine);
    _argv = (char **) malloc(sizeof(char *) * 255);
    _argc = ParseCommandLine(strdup(cmdLine), _argv);

    if (ss != -1.0f)
        SS_MULTIPLIER = ss;
    if (msaa != -1)
        NUM_MULTI_SAMPLES = msaa;
    if (refresh != -1)
        DISPLAY_REFRESH = refresh;

    auto appThread = (ovrAppThread *) malloc(sizeof(ovrAppThread));
    ovrAppThread_Create(appThread, env, activity, activityClass);

    ovrMessageQueue_Enable(&appThread->MessageQueue, true);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_CREATE, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);

    return (jlong) ((size_t) appThread);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onStart(JNIEnv *env, jclass obj, jlong handle, jobject obj1) {
    ALOGV("ovr::onStart()");
    _shutdownCallbackObj = (jobject) env->NewGlobalRef(obj1);
    auto callbackClass = env->GetObjectClass(_shutdownCallbackObj);
    _android_shutdown = env->GetMethodID(callbackClass, "shutdown", "()V");

    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_START, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onResume(JNIEnv *env, jclass obj, jlong handle) {
    ALOGV("ovr::onResume()");
    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_RESUME, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onPause(JNIEnv *env, jclass obj, jlong handle) {
    ALOGV("ovr::onPause()");
    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_PAUSE, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onStop(JNIEnv *env, jclass obj, jlong handle) {
    ALOGV("ovr::onStop()");
    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_STOP, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onDestroy(JNIEnv *env, jclass obj, jlong handle) {
    ALOGV("ovr::onDestroy()");
    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_DESTROY, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
    ovrMessageQueue_Enable(&appThread->MessageQueue, false);

    ovrAppThread_Destroy(appThread, env);
    free(appThread);
}

/*
================================================================================
Surface lifecycle
================================================================================
*/

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onSurfaceCreated(JNIEnv *env, jclass obj, jlong handle, jobject surface) {
    ALOGV("ovr::onSurfaceCreated()");
    auto appThread = (ovrAppThread *) ((size_t) handle);

    // An app that is relaunched after pressing the home button gets an initial surface with the wrong orientation even though android:screenOrientation="landscape" is set in the
    // manifest. The choreographer callback will also never be called for this surface because the surface is immediately replaced with a new surface with the correct orientation.
    auto newNativeWindow = ANativeWindow_fromSurface(env, surface);
    if (ANativeWindow_getWidth(newNativeWindow) < ANativeWindow_getHeight(newNativeWindow))
        ALOGE("Surface not in landscape mode!");

    ALOGV("NativeWindow = ANativeWindow_fromSurface(env, surface)");
    appThread->NativeWindow = newNativeWindow;
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_SURFACE_CREATED, MQ_WAIT_PROCESSED);
    ovrMessage_SetPointerParm(&message, 0, appThread->NativeWindow);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onSurfaceChanged(JNIEnv *env, jclass obj, jlong handle, jobject surface) {
    ALOGV("ovr::onSurfaceChanged()");
    auto appThread = (ovrAppThread *) ((size_t) handle);

    // An app that is relaunched after pressing the home button gets an initial surface with the wrong orientation even though android:screenOrientation="landscape" is set in the
    // manifest. The choreographer callback will also never be called for this surface because the surface is immediately replaced with a new surface with the correct orientation.
    ANativeWindow *newNativeWindow = ANativeWindow_fromSurface(env, surface);
    if (ANativeWindow_getWidth(newNativeWindow) < ANativeWindow_getHeight(newNativeWindow))
        ALOGE("Surface not in landscape mode!");

    if (newNativeWindow != appThread->NativeWindow) {
        if (appThread->NativeWindow) {
            ovrMessage message;
            ovrMessage_Init(&message, MESSAGE_ON_SURFACE_DESTROYED, MQ_WAIT_PROCESSED);
            ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
            ALOGV("ANativeWindow_release(NativeWindow)");
            ANativeWindow_release(appThread->NativeWindow);
            appThread->NativeWindow = nullptr;
        }
        if (newNativeWindow) {
            ALOGV("NativeWindow = ANativeWindow_fromSurface(env, surface)");
            appThread->NativeWindow = newNativeWindow;
            ovrMessage message;
            ovrMessage_Init(&message, MESSAGE_ON_SURFACE_CREATED, MQ_WAIT_PROCESSED);
            ovrMessage_SetPointerParm(&message, 0, appThread->NativeWindow);
            ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
        }
    } else if (newNativeWindow) {
        ANativeWindow_release(newNativeWindow);
    }
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidvr_OVRActivityKt_onSurfaceDestroyed(JNIEnv *env, jclass obj, jlong handle) {
    ALOGV("ovr::onSurfaceDestroyed()");
    auto appThread = (ovrAppThread *) ((size_t) handle);
    ovrMessage message;
    ovrMessage_Init(&message, MESSAGE_ON_SURFACE_DESTROYED, MQ_WAIT_PROCESSED);
    ovrMessageQueue_PostMessage(&appThread->MessageQueue, &message);
    ALOGV("ANativeWindow_release(NativeWindow)");
    ANativeWindow_release(appThread->NativeWindow);
    appThread->NativeWindow = nullptr;
}

