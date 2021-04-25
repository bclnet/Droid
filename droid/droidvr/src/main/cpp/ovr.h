#ifndef OVRAPP_H
#define OVRAPP_H

#include <VrApi_Input.h>
#include "lib/egl.h"

/*
================================================================================

ovrFramebuffer

================================================================================
*/

typedef struct {
    int Width;
    int Height;
    int Multisamples;
    int TextureSwapChainLength;
    int ProcessingTextureSwapChainIndex;
    int ReadyTextureSwapChainIndex;
    ovrTextureSwapChain *ColorTextureSwapChain;
    GLuint *DepthBuffers;
    GLuint *FrameBuffers;
    bool UseMultiview;
} ovrFramebuffer;

void ovrFramebuffer_SetCurrent(ovrFramebuffer *frameBuffer);

void ovrFramebuffer_Destroy(ovrFramebuffer *frameBuffer);

void ovrFramebuffer_SetNone();

void ovrFramebuffer_Resolve(ovrFramebuffer *frameBuffer);

void ovrFramebuffer_Advance(ovrFramebuffer *frameBuffer);

void ovrFramebuffer_ClearEdgeTexels(ovrFramebuffer *frameBuffer);

/*
================================================================================

ovrRenderer

================================================================================
*/

typedef struct {
    ovrFramebuffer FrameBuffer;
    ovrMatrix4f ProjectionMatrix;
    int NumBuffers;
} ovrRenderer;


void ovrRenderer_Clear(ovrRenderer *renderer);

void ovrRenderer_Create(int width, int height, ovrRenderer *renderer, const ovrJava *java);

void ovrRenderer_Destroy(ovrRenderer *renderer);


/*
================================================================================

appRenderState

================================================================================
*/

typedef struct {
    GLint VertexBuffer;
    GLint IndexBuffer;
    GLint VertexArrayObject;
    GLint Program;
    GLint VertexShader;
    GLint FragmentShader;
} appRenderState;

void getCurrentRenderState(appRenderState *state);

void restoreRenderState(appRenderState *state);

/*
================================================================================

ovrGeometry

================================================================================
*/

typedef struct {
    GLint Index;
    GLint Size;
    GLenum Type;
    GLboolean Normalized;
    GLsizei Stride;
    const GLvoid *Pointer;
} ovrVertexAttribPointer;

#define MAX_VERTEX_ATTRIB_POINTERS        3

typedef struct {
    GLuint VertexBuffer;
    GLuint IndexBuffer;
    GLuint VertexArrayObject;
    int VertexCount;
    int IndexCount;
    ovrVertexAttribPointer VertexAttribs[MAX_VERTEX_ATTRIB_POINTERS];
} ovrGeometry;

/*
================================================================================

ovrProgram

================================================================================
*/

#define MAX_PROGRAM_UNIFORMS    8
#define MAX_PROGRAM_TEXTURES    8

typedef struct {
    GLuint Program;
    GLuint VertexShader;
    GLuint FragmentShader;
    // These will be -1 if not used by the program.
    GLint UniformLocation[MAX_PROGRAM_UNIFORMS];    // ProgramUniforms[].name
    GLint UniformBinding[MAX_PROGRAM_UNIFORMS];    // ProgramUniforms[].name
    GLint Textures[MAX_PROGRAM_TEXTURES];            // Texture%i
} ovrProgram;

/*
================================================================================

ovrScene

================================================================================
*/

typedef struct {
    bool CreatedScene;

    //Proper renderer for stereo rendering to the cylinder layer
    ovrRenderer CylinderRenderer;

    int CylinderWidth;
    int CylinderHeight;
} ovrScene;

void ovrScene_Clear(ovrScene *scene);

void ovrScene_Create(int width, int height, ovrScene *scene, const ovrJava *java);

/*
================================================================================

ovrRenderer

================================================================================
*/

ovrLayerProjection2 ovrRenderer_RenderGroundPlaneToEyeBuffer(ovrRenderer *renderer, const ovrJava *java,
                                                             const ovrScene *scene, const ovrTracking2 *tracking);

ovrLayerProjection2 ovrRenderer_RenderToEyeBuffer(ovrRenderer *renderer, const ovrJava *java,
                                                  const ovrTracking2 *tracking);

ovrLayerCylinder2 BuildCylinderLayer(ovrRenderer *cylinderRenderer,
                                     const int textureWidth, const int textureHeight,
                                     const ovrTracking2 *tracking, float rotateYaw);


#endif // OVRAPP_H