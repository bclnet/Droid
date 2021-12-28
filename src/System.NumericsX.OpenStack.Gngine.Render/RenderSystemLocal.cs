using System.NumericsX.OpenStack.Gngine.Framework;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    partial class TR
    {

    }


    //xthreadInfo renderThread = { 0 };

    // Most renderer globals are defined here. backend functions should never modify any of these fields, but may read fields that aren't dynamically modified by the frontend.
    //public partial class RenderSystemLocal : IRenderSystem
    //{
    //    // external functions
    //    public void Init();
    //    public void Shutdown();
    //    public void InitOpenGL();
    //    public void ShutdownOpenGL();
    //    public bool IsOpenGLRunning();
    //    public bool IsFullScreen();
    //    public int GetScreenWidth();
    //    public int GetScreenHeight();
    //    public float GetFOV();
    //    public int GetRefresh();

    //    public IRenderWorld AllocRenderWorld();
    //    public void FreeRenderWorld(IRenderWorld rw);
    //    public void BeginLevelLoad();
    //    public void EndLevelLoad();
    //    public bool RegisterFont(string fontName, FontInfoEx font);
    //    public void SetHudOpacity(float opacity);
    //    public void SetColor(Vector4 rgba);
    //    public void SetColor4(float r, float g, float b, float a);
    //    public void DrawStretchPic(DrawVert verts, GlIndex indexes, int vertCount, int indexCount, Material material, bool clip = true, float x = 0f, float y = 0f, float w = 640f, float h = 0f);
    //    public void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material);

    //    public void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material);
    //    public void GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc);
    //    public void GetGLSettings(out int width, out int height);
    //    public void PrintMemInfo(MemInfo mi);

    //    public void DrawSmallChar(int x, int y, int ch, Material material);
    //    public void DrawSmallStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
    //    public void DrawBigChar(int x, int y, int ch, Material material);
    //    public void DrawBigStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
    //    public void WriteDemoPics();
    //    public void DrawDemoPics();
    //    public void BeginFrame(int windowWidth, int windowHeight);
    //    public void EndFrame(int frontEndMsec, int backEndMsec);
    //    public void TakeScreenshot(int width, int height, string fileName, int downSample, RenderView @ref);
    //    public void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false);
    //    public void CaptureRenderToImage(string imageName);
    //    public void CaptureRenderToFile(string fileName, bool fixAlpha);
    //    public void UnCrop();
    //    public bool UploadImage(string imageName, byte[] data, int width, int height);

    //    public void DirectFrameBufferStart();
    //    public void DirectFrameBufferEnd();

    //    // internal functions
    //    RenderSystemLocal();

    //    public void Clear();
    //    public void SetBackEndRenderer();          // sets tr.backEndRenderer based on cvars
    //    public void RenderViewToViewport(RenderView renderView, ScreenRect viewport);

    //    public bool multithreadActive = false;

    //    public bool useSpinLock = true;
    //    public int spinLockDelay = 1000;
    //    public float hudOpacity = 0f;

    //    public bool windowActive = false; // True when the app is at the foreground and not minimised

    //    public volatile bool backendThreadRun = false;
    //    public volatile bool backendFinished = true;
    //    public volatile bool imagesFinished = false;

    //    public volatile bool backendThreadShutdown = false;

    //    public volatile FrameData fdToRender = null;
    //    public volatile int vertListToRender = 0;

    //    // These are set if the backend should save pixels
    //    public volatile RenderCrop pixelsCrop = null;
    //    public volatile byte[] pixels = null;

    //    // For FPS limiting
    //    public uint lastRenderTime = 0;

    //    // The backend task
    //    public void BackendThreadTask();

    //    // The backend thread
    //    public void BackendThread();

    //    // Start (and create) the back thread
    //    public void BackendThreadExecute();

    //    // Wait for backend thread to finish
    //    public void BackendThreadWait();

    //    public void BackendThreadShutdown();

    //    // Call this to render the current command buffer. If you pass is pixels it will block and perform a glReadPixels
    //    public void RenderCommands(RenderCrop pixelsCrop, byte[] pixels);

    //    // Static runner to start thread
    //    public static int BackendThreadRunner(void* localRenderSystem);

    //    // renderer globals
    //    public bool registered;        // cleared at shutdown, set at InitOpenGL

    //    public bool takingScreenshot;

    //    public int frameCount;     // incremented every frame
    //    public int viewCount;      // incremented every view (twice a scene if subviewed) and every R_MarkFragments call

    //    public int staticAllocCount;   // running total of bytes allocated

    //    public float frameShaderTime;  // shader time for all non-world 2D rendering

    //    public int[] viewportOffset = new int[2];  // for doing larger-than-window tiled renderings
    //    public int[] tiledViewport = new int[2];

    //    public Vector4 ambientLightVector;  // used for "ambient bump mapping"

    //    public float sortOffset;               // for determinist sorting of equal sort materials

    //    public List<RenderWorldLocal> worlds;

    //    public RenderWorldLocal primaryWorld;
    //    public RenderView primaryRenderView;
    //    public ViewDef primaryView;
    //    // many console commands need to know which world they should operate on

    //    public Material defaultMaterial;
    //    public Image testImage;
    //    public Cinematic testVideo;
    //    public float testVideoStartTime;

    //    public Image ambientCubeImage;  // hack for testing dependent ambient lighting

    //    public ViewDef viewDef;

    //    public PerformanceCounters pc;                   // performance counters

    //    public DrawSurfsCommand lockSurfacesCmd; // use this when r_lockSurfaces = 1

    //    public ViewEntity identitySpace;     // can use if we don't know viewDef->worldSpace is valid

    //    public RenderCrop[] renderCrops = new RenderCrop[MAX_RENDER_CROPS];
    //    public int currentRenderCrop;

    //    // GUI drawing variables for surface creation
    //    public int guiRecursionLevel;      // to prevent infinite overruns
    //    public GuiModel guiModel;
    //    public GuiModel demoGuiModel;

    //    // DG: remember the original glConfig.vidWidth/Height values that get overwritten in BeginFrame() so they can be reset in EndFrame() (Editors tend to mess up the viewport by using BeginFrame())
    //    public int origWidth;
    //    public int origHeight;
    //}

    public class RenderSystemLocal : IRenderSystem
    {
        public int frameCount => throw new NotImplementedException();

        public ViewDef viewDef => throw new NotImplementedException();

        public Vector4 ambientLightVector => throw new NotImplementedException();

        public bool IsOpenGLRunning => throw new NotImplementedException();

        public bool IsFullScreen => throw new NotImplementedException();

        public int ScreenWidth => throw new NotImplementedException();

        public int ScreenHeight => throw new NotImplementedException();

        public float FOV => throw new NotImplementedException();

        public int Refresh => throw new NotImplementedException();

        public IRenderWorld AllocRenderWorld()
        {
            throw new NotImplementedException();
        }

        public void BeginFrame(int windowWidth, int windowHeight)
        {
            throw new NotImplementedException();
        }

        public void BeginLevelLoad()
        {
            throw new NotImplementedException();
        }

        public void CaptureRenderToFile(string fileName, bool fixAlpha = false)
        {
            throw new NotImplementedException();
        }

        public void CaptureRenderToImage(string imageName)
        {
            throw new NotImplementedException();
        }

        public void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false)
        {
            throw new NotImplementedException();
        }

        public void DirectFrameBufferEnd()
        {
            throw new NotImplementedException();
        }

        public void DirectFrameBufferStart()
        {
            throw new NotImplementedException();
        }

        public void DrawBigChar(int x, int y, int ch, Material material)
        {
            throw new NotImplementedException();
        }

        public void DrawBigStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material)
        {
            throw new NotImplementedException();
        }

        public void DrawDemoPics()
        {
            throw new NotImplementedException();
        }

        public void DrawSmallChar(int x, int y, int ch, Material material)
        {
            throw new NotImplementedException();
        }

        public void DrawSmallStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material)
        {
            throw new NotImplementedException();
        }

        public void DrawStretchPic(DrawVert[] verts, int[] indexes, int vertCount, int indexCount, Material material, bool clip = true, float min_x = 0, float min_y = 0, float max_x = 640, float max_y = 480)
        {
            throw new NotImplementedException();
        }

        public void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material)
        {
            throw new NotImplementedException();
        }

        public void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material)
        {
            throw new NotImplementedException();
        }

        public void EndFrame(out int frontEndMsec, out int backEndMsec)
        {
            throw new NotImplementedException();
        }

        public void EndLevelLoad()
        {
            throw new NotImplementedException();
        }

        public void FreeRenderWorld(IRenderWorld rw)
        {
            throw new NotImplementedException();
        }

        public void GetGLSettings(out int width, out int height)
        {
            throw new NotImplementedException();
        }

        public void GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void InitOpenGL()
        {
            throw new NotImplementedException();
        }

        public void PrintMemInfo(MemInfo mi)
        {
            throw new NotImplementedException();
        }

        public bool RegisterFont(string fontName, FontInfoEx font)
        {
            throw new NotImplementedException();
        }

        public void SetColor(Vector4 rgba)
        {
            throw new NotImplementedException();
        }

        public void SetColor4(float r, float g, float b, float a)
        {
            throw new NotImplementedException();
        }

        public void SetHudOpacity(float opacity)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void ShutdownOpenGL()
        {
            throw new NotImplementedException();
        }

        public void TakeScreenshot(int width, int height, string fileName, int samples, RenderView ref_)
        {
            throw new NotImplementedException();
        }

        public void UnCrop()
        {
            throw new NotImplementedException();
        }

        public bool UploadImage(string imageName, byte[] data, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void WriteDemoPics()
        {
            throw new NotImplementedException();
        }
    }
}