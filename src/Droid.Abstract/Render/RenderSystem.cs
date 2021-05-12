using Droid.Framework;
using System;
using System.Numerics;

namespace Droid.Render
{
    // Contains variables specific to the OpenGL configuration being run right now.
    // These are constant once the OpenGL subsystem is initialized.
    class glconfig
    {
        public string renderer_string;
        public string vendor_string;
        public string version_string;
        public string extensions_string;

        public int maxTextureSize;          // queried from GL
        public int maxTextureUnits;
        public float maxTextureAnisotropy;

        public int colorBits, depthBits, stencilBits;

        public bool anisotropicAvailable;

        public bool npotAvailable;

        public bool depthStencilAvailable;

        public int vidWidth, vidHeight; // passed to R_BeginFrame

        public int vidWidthReal, vidHeightReal; // The real resolution of the screen, uses framebuffer if not the same as vidWidth

        public int displayFrequency;

        public bool isFullscreen;

        public bool isInitialized;
    }

    public struct glyphInfo
    {
        public int height;          // number of scan lines
        public int top;         // top of glyph in buffer
        public int bottom;          // bottom of glyph in buffer
        public int pitch;           // width for copying
        public int xSkip;           // x adjustment
        public int imageWidth;      // width of actual image
        public int imageHeight; // height of actual image
        public float s;             // x offset in image where glyph starts
        public float t;             // y offset in image where glyph starts
        public float s2;
        public float t2;
        public Material glyph;          // shader with the glyph
        public string shaderName;
    }

    public class fontInfo
    {
        public glyphInfo[] glyphs = new glyphInfo[R.GLYPHS_PER_FONT];
        public float glyphScale;
        public string name;
    }

    public struct fontInfoEx
    {
        public fontInfo fontInfoSmall;
        public fontInfo fontInfoMedium;
        public fontInfo fontInfoLarge;
        public int maxHeight;
        public int maxWidth;
        public int maxHeightSmall;
        public int maxWidthSmall;
        public int maxHeightMedium;
        public int maxWidthMedium;
        public int maxHeightLarge;
        public int maxWidthLarge;
        public string name;
    }

    public static partial class R
    {
        // font support
        public const int GLYPH_START = 0;
        public const int GLYPH_END = 255;
        public const int GLYPH_CHARSTART = 32;
        public const int GLYPH_CHAREND = 127;
        public const int GLYPHS_PER_FONT = GLYPH_END - GLYPH_START + 1;

        public const int SMALLCHAR_WIDTH = 8;
        public const int SMALLCHAR_HEIGHT = 16;
        public const int BIGCHAR_WIDTH = 16;
        public const int BIGCHAR_HEIGHT = 16;

        // all drawing is done to a 640 x 480 virtual screen size and will be automatically scaled to the real resolution
        public const int SCREEN_WIDTH = 640;
        public const int SCREEN_HEIGHT = 480;

        //
        // functions mainly intended for editor and dmap integration
        //

        // returns the frustum planes in world space
        public static void RenderLightFrustum(renderLight renderLight, Plane[] lightFrustum) => throw new NotImplementedException();

        // for use by dmap to do the carving-on-light-boundaries and for the editor for display
        public static void LightProjectionMatrix(Vector3 origin, Plane rearPlane, Vector4[] mat) => throw new NotImplementedException();

        // used by the view shot taker
        public static void ScreenshotFilename(out int lastNumber, string base_, string fileName) => throw new NotImplementedException();
    }

    public interface RenderSystem
    {
        // set up cvars and basic data structures, but don't init OpenGL, so it can also be used for dedicated servers
        void Init();

        // only called before quitting
        void Shutdown();

        void InitOpenGL();

        void ShutdownOpenGL();

        bool IsOpenGLRunning();

        bool IsFullScreen();
        int GetScreenWidth();
        int GetScreenHeight();
        float GetFOV();
        int GetRefresh();

        // allocate a renderWorld to be used for drawing
        RenderWorld AllocRenderWorld();
        void FreeRenderWorld(ref RenderWorld rw);

        // All data that will be used in a level should be registered before rendering any frames to prevent disk hits,
        // but they can still be registered at a later time if necessary.
        void BeginLevelLoad();
        void EndLevelLoad();

        // font support
        bool RegisterFont(string fontName, fontInfoEx font);

        // GUI drawing just involves shader parameter setting and axial image subsections
        void SetColor(Vector4 rgba);
        void SetColor4(float r, float g, float b, float a);
        void SetHudOpacity(float opacity);

        void DrawStretchPic(DrawVert verts, glIndex indexes, int vertCount, int indexCount, Material material,
                                            bool clip = true, float min_x = 0.0f, float min_y = 0.0f, float max_x = 640.0f, float max_y = 480.0f);
        void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material);

        void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material);
        void GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc);
        void GetGLSettings(out int width, out int height);
        void PrintMemInfo(MemInfo mi);

        void DrawSmallChar(int x, int y, int ch, Material material);
        void DrawSmallStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
        void DrawBigChar(int x, int y, int ch, Material material);
        void DrawBigStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);

        // dump all 2D drawing so far this frame to the demo file
        void WriteDemoPics();

        // draw the 2D pics that were saved out with the current demo frame
        void DrawDemoPics();

        // FIXME: add an interface for arbitrary point/texcoord drawing

        // a frame cam consist of 2D drawing and potentially multiple 3D scenes window sizes are needed to convert SCREEN_WIDTH / SCREEN_HEIGHT values
        void BeginFrame(int windowWidth, int windowHeight);

        // if the pointers are not NULL, timing info will be returned
        void EndFrame(out int frontEndMsec, out int backEndMsec);

        // Will automatically tile render large screen shots if necessary
        // Samples is the number of jittered frames for anti-aliasing
        // If ref == NULL, session->updateScreen will be used
        // This will perform swapbuffers, so it is NOT an approppriate way to generate image files that happen during gameplay, as for savegame
        // markers.  Use WriteRender() instead.
        void TakeScreenshot(int width, int height, string fileName, int samples, renderView ref_);

        // the render output can be cropped down to a subset of the real screen, as for save-game reviews and split-screen multiplayer.  Users of the renderer
        // will not know the actual pixel size of the area they are rendering to

        // the x,y,width,height values are in virtual SCREEN_WIDTH / SCREEN_HEIGHT coordinates

        // to render to a texture, first set the crop size with makePowerOfTwo = true, then perform all desired rendering, then capture to an image
        // if the specified physical dimensions are larger than the current cropped region, they will be cut down to fit
        void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false);
        void CaptureRenderToImage(string imageName);
        // fixAlpha will set all the alpha channel values to 0xff, which allows screen captures
        // to use the default tga loading code without having dimmed down areas in many places
        void CaptureRenderToFile(string fileName, bool fixAlpha = false);
        void UnCrop();

        // the image has to be already loaded ( most straightforward way would be through a FindMaterial )
        // texture filter / mipmapping / repeat won't be modified by the upload
        // returns false if the image wasn't found
        bool UploadImage(string imageName, byte[] data, int width, int height);

        void DirectFrameBufferStart();
        void DirectFrameBufferEnd();
    }
}