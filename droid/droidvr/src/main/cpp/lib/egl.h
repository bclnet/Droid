#ifndef EGL_H
#define EGL_H

#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>

#if !defined(EGL_OPENGL_ES3_BIT_KHR)
#define EGL_OPENGL_ES3_BIT_KHR        0x0040
#endif

// EXT_texture_border_clamp
#ifndef GL_CLAMP_TO_BORDER
#define GL_CLAMP_TO_BORDER            0x812D
#endif

#ifndef GL_TEXTURE_BORDER_COLOR
#define GL_TEXTURE_BORDER_COLOR        0x1004
#endif

void EglInitExtensions();

const char *EglErrorString(const EGLint error);

const char *GlErrorString(GLenum error);

const char *GlFrameBufferStatusString(GLenum status);

#define CHECK_GL_ERRORS
#ifdef CHECK_GL_ERRORS

#include <android/log.h>

static void GLCheckErrors(int line) {
    for (auto i = 0; i < 10; i++) {
        const GLenum error = glGetError();
        if (error == GL_NO_ERROR)
            break;
        __android_log_print(ANDROID_LOG_ERROR, "TAG1", "GL error on line %d: %s", line, GlErrorString(error));
    }
}

#define GL(func) func; GLCheckErrors( __LINE__ );
#else
#define GL(func) func;
#endif

#endif // EGL_H