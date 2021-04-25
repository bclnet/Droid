#include <VrApi.h>
#include <VrApi_Helpers.h>
#include "lib/vmath.h"
#include "ovr.h"
#include "app.h"

/*
================================================================================
ovrScene
================================================================================
*/

void ovrScene_Clear(ovrScene *scene) {
    scene->CreatedScene = false;
    ovrRenderer_Clear(&scene->CylinderRenderer);
    scene->CylinderWidth = 0;
    scene->CylinderHeight = 0;
}

void ovrScene_Create(int width, int height, ovrScene *scene, const ovrJava *java) {
    if (scene->CreatedScene)
        return;

    // create cylinder renderer
    scene->CylinderWidth = width;
    scene->CylinderHeight = height;
    ovrRenderer_Create(width, height, &scene->CylinderRenderer, java);
    scene->CreatedScene = true;
}

void ovrScene_Destroy(ovrScene *scene) {
    ovrRenderer_Destroy(&scene->CylinderRenderer);
    scene->CreatedScene = false;
}

/*
================================================================================
ovrRenderer
================================================================================
*/

// assumes landscape cylinder shape.
static ovrMatrix4f CylinderModelMatrix(const int texWidth, const int texHeight, const ovrVector3f translation, const float rotateYaw, const float rotatePitch, const float radius, const float density) {
    const auto scaleMatrix = ovrMatrix4f_CreateScale(radius, radius * (float) texHeight * VRAPI_PI / density, radius);
    const auto transMatrix = ovrMatrix4f_CreateTranslation(translation.x, translation.y, translation.z);
    const auto rotXMatrix = ovrMatrix4f_CreateRotation(rotateYaw, 0.0f, 0.0f);
    const auto rotYMatrix = ovrMatrix4f_CreateRotation(0.0f, rotatePitch, 0.0f);

    const auto m0 = ovrMatrix4f_Multiply(&transMatrix, &scaleMatrix);
    const auto m1 = ovrMatrix4f_Multiply(&rotXMatrix, &m0);
    const auto m2 = ovrMatrix4f_Multiply(&rotYMatrix, &m1);

    return m2;
}

extern float SS_MULTIPLIER;

ovrLayerCylinder2 BuildCylinderLayer(ovrRenderer *cylinderRenderer, const int texWidth, const int texHeight, const ovrTracking2 *tracking, float rotatePitch) {
    auto layer = vrapi_DefaultLayerCylinder2();

    const auto fadeLevel = 1.0f;
    layer.Header.ColorScale.x = layer.Header.ColorScale.y = layer.Header.ColorScale.z = layer.Header.ColorScale.w = fadeLevel;

    //TODO: Alpha issues!
    //layer.Header.SrcBlend = VRAPI_FRAME_LAYER_BLEND_SRC_ALPHA;
    //layer.Header.DstBlend = VRAPI_FRAME_LAYER_BLEND_ONE_MINUS_SRC_ALPHA;
    //layer.Header.Flags = VRAPI_FRAME_LAYER_FLAG_CLIP_TO_TEXTURE_RECT;

    layer.HeadPose = tracking->HeadPose;

    const auto density = 4500.0f;
    const auto radius = 6.0f;
    const ovrVector3f translation = {0.0f, 0.0f, -4.0f / SS_MULTIPLIER};

    auto cylinderTransform = CylinderModelMatrix(texWidth, texHeight, translation, rotatePitch, M_DEG2RAD(_appClient.screenYaw), radius, density);

    const auto circScale = density * 0.5f / texWidth;
    const auto circBias = -circScale * (0.5f * (1.0f - 1.0f / circScale));

    auto cylinderFrameBuffer = &cylinderRenderer->FrameBuffer;

    for (auto eye = 0; eye < VRAPI_FRAME_LAYER_EYE_MAX; eye++) {
        auto modelViewMatrix = ovrMatrix4f_Multiply(&tracking->Eye[eye].ViewMatrix, &cylinderTransform);
        layer.Textures[eye].TexCoordsFromTanAngles = ovrMatrix4f_Inverse(&modelViewMatrix);
        layer.Textures[eye].ColorSwapChain = cylinderFrameBuffer->ColorTextureSwapChain;
        layer.Textures[eye].SwapChainIndex = cylinderFrameBuffer->ReadyTextureSwapChainIndex;

        // texcoord scale and bias is just a representation of the aspect ratio. The positioning of the cylinder is handled entirely by the TexCoordsFromTanAngles matrix.
        const auto texScaleX = circScale;
        const auto texBiasX = circBias;
        const auto texScaleY = -0.5f;
        const auto texBiasY = texScaleY * (0.5f * (1.0f - (1.0f / texScaleY)));

        layer.Textures[eye].TextureMatrix.M[0][0] = texScaleX;
        layer.Textures[eye].TextureMatrix.M[0][2] = texBiasX;
        layer.Textures[eye].TextureMatrix.M[1][1] = texScaleY;
        layer.Textures[eye].TextureMatrix.M[1][2] = -texBiasY;

        layer.Textures[eye].TextureRect.width = 1.0f;
        layer.Textures[eye].TextureRect.height = 1.0f;
    }
    return layer;
}
