#define USE_TRI_DATA_ALLOCATOR
using Droid.Core;
using Droid.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Droid.Render
{
    public static partial class TRX
    {
        // everything that is needed by the backend needs to be double buffered to allow it to run in parallel on a dual cpu machine
        const int SMP_FRAMES = 1;

        const int FALLOFF_TEXTURE_SIZE = 64;

        const float DEFAULT_FOG_DISTANCE = 500f;

        const int FOG_ENTER_SIZE = 64;
        const float FOG_ENTER = (FOG_ENTER_SIZE + 1f) / (FOG_ENTER_SIZE * 2);
        // picky to get the bilerp correct at terminator
    }

    public struct ShaderProgram { }

    // ScreenRect gets carried around with each drawSurf, so it makes sense to keep it compact, instead of just using the idBounds class
    public class ScreenRect
    {
        public short x1, y1, x2, y2;                            // inclusive pixel bounds inside viewport
        public float zmin, zmax;                                // for depth bounds test

        public void Clear();                                // clear to backwards values
        public void AddPoint(float x, float y);         // adds a point
        public void Expand();                               // expand by one pixel each way to fix roundoffs
        public void Intersect(ScreenRect rect);
        public void Union(ScreenRect rect);
        public bool Equals(ScreenRect rect);
        public bool IsEmpty();
    }

    partial class TRX
    {
        public static ScreenRect R_ScreenRectFromViewFrustumBounds(Bounds bounds);
        public static void R_ShowColoredScreenRect(ScreenRect rect, int colorIndex);
    }

    public enum DemoCommand
    {
        DC_BAD,
        DC_RENDERVIEW,
        DC_UPDATE_ENTITYDEF,
        DC_DELETE_ENTITYDEF,
        DC_UPDATE_LIGHTDEF,
        DC_DELETE_LIGHTDEF,
        DC_LOADMAP,
        DC_CROP_RENDER,
        DC_UNCROP_RENDER,
        DC_CAPTURE_RENDER,
        DC_END_FRAME,
        DC_DEFINE_MODEL,
        DC_SET_PORTAL_STATE,
        DC_UPDATE_SOUNDOCCLUSION,
        DC_GUI_MODEL
    }

    // drawSurf_t structures command the back end to render surfaces a given srfTriangles_t may be used with multiple viewEntity_t,
    // as when viewed in a subview or multiple viewport render, or with multiple shaders when skinned, or, possibly with multiple
    // lights, although currently each lighting interaction creates unique srfTriangles_t

    // drawSurf_t are always allocated and freed every frame, they are never cached
    public class DrawSurf
    {
        const int DSF_VIEW_INSIDE_SHADOW = 1;

        public SrfTriangles geoFrontEnd;
        public ViewEntity space;
        public Material material; // may be NULL for shadow volumes
        public float sort;     // material->sort, modified by gui / entity sort offsets
        public float[] shaderRegisters;   // evaluated and adjusted for referenceShaders
        public DrawSurf nextOnLight;    // viewLight chains
        public ScreenRect scissorRect;   // for scissor clipping, local inside renderView viewport
        public int dsFlags;            // DSF_VIEW_INSIDE_SHADOW, etc
        public float[] wobbleTransform = new float[16];
        public int numIndexes;
        // data in vertex object space, not directly readable by the CPU
        public VertCache indexCache;                // int
        public VertCache ambientCache;          // idDrawVert
        public VertCache shadowCache;           // shadowCache_t

        public int numShadowIndexesNoFrontCaps;    // shadow volumes with front caps omitted
        public int numShadowIndexesNoCaps;         // shadow volumes with the front and rear caps omitted
        public int shadowCapPlaneBits;     // bits 0-5 are set when that plane of the interacting light has triangles
    }

    public class ShadowFrustum
    {
        public int numPlanes;      // this is always 6 for now
        public Plane[] planes = new Plane[6];
        // positive sides facing inward plane 5 is always the plane the projection is going to, the other planes are just clip planes all planes are in global coordinates
        public bool makeClippedPlanes;
        // a projected light with a single frustum needs to make sil planes from triangles that clip against side planes, but a point light that has adjacent frustums doesn't need to
    }

    // areas have references to hold all the lights and entities in them
    public class AreaReference
    {
        public AreaReference areaNext;              // chain in the area
        public AreaReference areaPrev;
        public AreaReference ownerNext;             // chain on either the entityDef or lightDef
        public RenderEntityLocal entity;                    // only one of entity / light will be non-NULL
        public RenderLightLocal light;                  // only one of entity / light will be non-NULL
        public PortalArea area;                 // so owners can find all the areas they are in
    }

    // IRenderLight should become the new public interface replacing the qhandle_t to light defs in the idRenderWorld interface
    public interface IRenderLight
    {
        void FreeRenderLight();
        void UpdateRenderLight(RenderLight re, bool forceUpdate = false);
        void GetRenderLight(RenderLight re);
        void ForceUpdate();
        int Index { get; }
    }

    // IRenderEntity should become the new public interface replacing the qhandle_t to entity defs in the idRenderWorld interface
    public interface IRenderEntity
    {
        void FreeRenderEntity();
        void UpdateRenderEntity(RenderEntity re, bool forceUpdate = false);
        void GetRenderEntity(RenderEntity re);
        void ForceUpdate();
        int Index { get; }
        // overlays are extra polygons that deform with animating models for blood and damage marks
        void ProjectOverlay(Plane[] localTextureAxis, Material material);
        void RemoveDecals();
    }

    public class RenderLightLocal : IRenderLight
    {
        public RenderLightLocal();

        public void FreeRenderLight();
        public void UpdateRenderLight(RenderLight re, bool forceUpdate = false);
        public void GetRenderLight(RenderLight re);
        public void ForceUpdate();
        public int Index => 0;
        public RenderLight parms;                    // specification
        public bool lightHasMoved;         // the light has changed its position since it was first added, so the prelight model is not valid
        public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin
        public RenderWorldLocal world;
        public int index;                  // in world lightdefs
        public int areaNum;                // if not -1, we may be able to cull all the light's interactions if !viewDef->connectedAreas[areaNum]
        public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        public bool archived;              // for demo writing

        // derived information
        public Plane[] lightProject = new Plane[4];

        public Material lightShader;          // guaranteed to be valid, even if parms.shader isn't
        public Image falloffImage;

        public Vector3 globalLightOrigin;       // accounting for lightCenter and parallel

        public Plane[] frustum = new Plane[6];             // in global space, positive side facing out, last two are front/back
        public Winding[] frustumWindings = new Winding[6];      // used for culling
        public SrfTriangles[] frustumTris;            // triangulated frustumWindings[]

        public int numShadowFrustums;      // one for projected lights, usually six for point lights
        public ShadowFrustum[] shadowFrustums = new ShadowFrustum[6];

        public int viewCount;              // if == tr.viewCount, the light is on the viewDef->viewLights list
        public ViewLight viewLight;

        public AreaReference references;                // each area the light is present in will have a lightRef
        public Interaction firstInteraction;        // doubly linked list
        public Interaction lastInteraction;

        public DoublePortal foggedPortals;
    }

    public class RenderEntityLocal : IRenderEntity
    {
        public RenderEntityLocal();

        public void FreeRenderEntity();
        public void UpdateRenderEntity(RenderEntity re, bool forceUpdate = false);
        public void GetRenderEntity(RenderEntity re);
        public void ForceUpdate();
        public int Index => 0;

        // overlays are extra polygons that deform with animating models for blood and damage marks
        public void ProjectOverlay(Plane[] localTextureAxis, Material material);
        public void RemoveDecals();

        public RenderEntity parms;

        public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin

        public RenderWorldLocal world;
        public int index;                  // in world entityDefs

        public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        public bool archived;              // for demo writing

        public IRenderModel dynamicModel;            // if parms.model->IsDynamicModel(), this is the generated data
        public int dynamicModelFrameCount; // continuously animating dynamic models will recreate dynamicModel if this doesn't == tr.viewCount
        public IRenderModel cachedDynamicModel;

        public Bounds referenceBounds;       // the local bounds used to place entityRefs, either from parms or a model

        // a viewEntity_t is created whenever a idRenderEntityLocal is considered for inclusion in a given view, even if it turns out to not be visible
        public int viewCount;              // if tr.viewCount == viewCount, viewEntity is valid, but the entity may still be off screen
        public ViewEntity viewEntity;                // in frame temporary memory

        public int visibleCount;
        // if tr.viewCount == visibleCount, at least one ambient surface has actually been added by R_AddAmbientDrawsurfs
        // note that an entity could still be in the view frustum and not be visible due to portal passing

        public RenderModelDecal decals;                 // chain of decals that have been projected on this model
        public RenderModelOverlay overlay;              // blood overlays on animated models

        public AreaReference entityRefs;                // chain of all references
        public Interaction firstInteraction;        // doubly linked list
        public Interaction lastInteraction;

        public bool needsPortalSky;
    }

    // viewLights are allocated on the frame temporary stack memory a viewLight contains everything that the back end needs out of an idRenderLightLocal,
    // which the front end may be modifying simultaniously if running in SMP mode. a viewLight may exist even without any surfaces, and may be relevent for fogging,
    // but should never exist if its volume does not intersect the view frustum
    public class ViewLight
    {
        public ViewLight next;

        // back end should NOT reference the lightDef, because it can change when running SMP
        public RenderLightLocal lightDef;

        // for scissor clipping, local inside renderView viewport scissorRect.Empty() is true if the viewEntity_t was never actually seen through any portals
        public ScreenRect scissorRect;

        // if the view isn't inside the light, we can use the non-reversed shadow drawing, avoiding the draws of the front and rear caps
        public bool viewInsideLight;

        // true if globalLightOrigin is inside the view frustum, even if it may be obscured by geometry.  This allows us to skip shadows from non-visible objects
        public bool viewSeesGlobalLightOrigin;

        // if !viewInsideLight, the corresponding bit for each of the shadowFrustum projection planes that the view is on the negative side of will be set,
        // allowing us to skip drawing the projected caps of shadows if we can't see the face
        public int viewSeesShadowPlaneBits;

        public Vector3 globalLightOrigin;           // global light origin used by backend
        public Plane[] lightProject = new Plane[4];            // light project used by backend
        public Plane fogPlane;                   // fog plane for backend fog volume rendering
        public SrfTriangles[] frustumTris;              // light frustum for backend fog volume rendering
        public Material lightShader;              // light shader used by backend
        public float[] shaderRegisters;           // shader registers used by backend
        public Image falloffImage;              // falloff image used by backend

        public DrawSurf globalShadows;             // shadow everything
        public DrawSurf localInteractions;         // don't get local shadows
        public DrawSurf localShadows;              // don't shadow local Surfaces
        public DrawSurf globalInteractions;        // get shadows from everything
        public DrawSurf translucentInteractions;   // get shadows from everything
    }

    // a viewEntity is created whenever a idRenderEntityLocal is considered for inclusion in the current view, but it may still turn out to be culled.
    // viewEntity are allocated on the frame temporary stack memory a viewEntity contains everything that the back end needs out of a idRenderEntityLocal,
    // which the front end may be modifying simultaniously if running in SMP mode. A single entityDef can generate multiple viewEntity_t in a single frame, as when seen in a mirror
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ViewEntity_Union
    {
        // local coords to left/right/center eye coords
        [FieldOffset(0)] public fixed float eyeViewMatrix0[16];
        [FieldOffset(0)] public fixed float eyeViewMatrix1[16];
        [FieldOffset(0)] public fixed float eyeViewMatrix2[16];
        // Can also be treated as a float[48]
        [FieldOffset(0)] public fixed float viewMatrix[48];
    }
    public class ViewEntity
    {
        public ViewEntity next;

        // back end should NOT reference the entityDef, because it can change when running SMP
        public RenderEntityLocal entityDef;

        // for scissor clipping, local inside renderView viewport scissorRect.Empty() is true if the viewEntity_t was never actually
        // seen through any portals, but was created for shadow casting. a viewEntity can have a non-empty scissorRect, meaning that an area
        // that it is in is visible, and still not be visible.
        public ScreenRect scissorRect;

        public bool weaponDepthHack;
        public float modelDepthHack;

        public float[] modelMatrix = new float[16];      // local coords to global coords

        public ViewEntity_Union u;
    }

    // viewDefs are allocated on the frame temporary stack memory
    public class ViewDef
    {
        const int MAX_CLIP_PLANES = 1;              // we may expand this to six for some subview issues

        // specified in the call to DrawScene()
        public RenderView renderView;

        public float[] projectionMatrix = new float[16];
        public ViewEntity worldSpace; // left, right and untransformed, Eye World space view entities

        public RenderWorldLocal renderWorld;

        public float floatTime;

        public Vector3 initialViewAreaOrigin;

        // Used to find the portalArea that view flooding will take place from. for a normal view, the initialViewOrigin will be renderView.viewOrg,
        // but a mirror may put the projection origin outside of any valid area, or in an unconnected area of the map, so the view
        // area must be based on a point just off the surface of the mirror / subview. It may be possible to get a failed portal pass if the plane of the
        // mirror intersects a portal, and the initialViewAreaOrigin is on a different side than the renderView.viewOrg is.
        public bool isSubview;             // true if this view is not the main view
        public bool isMirror;              // the portal is a mirror, invert the face culling
        public bool isXraySubview;

        public bool isEditor;

        public int numClipPlanes;          // mirrors will often use a single clip plane
        public Plane[] clipPlanes = new Plane[MAX_CLIP_PLANES];        // in world space, the positive side of the plane is the visible side
        public ScreenRect viewport;              // in real pixels and proper Y flip

        // for scissor clipping, local inside renderView viewport subviews may only be rendering part of the main view
        // these are real physical pixel values, possibly scaled and offset from the renderView x/y/width/height
        public ScreenRect scissor;

        public ViewDef superView;              // never go into an infinite subview loop
        public DrawSurf subviewSurface;

        // drawSurfs are the visible surfaces of the viewEntities, sorted by the material sort parameter
        public DrawSurf[] drawSurfs;             // we don't use an List for this, because it is allocated in frame temporary memory and may be resized
        public int numDrawSurfs;
        public int maxDrawSurfs;

        public ViewLight viewLights;           // chain of all viewLights effecting view
        public ViewEntity viewEntitys;         // chain of all viewEntities effecting view, including off screen ones casting shadows

        // we use viewEntities as a check to see if a given view consists solely of 2D rendering, which we can optimize in certain ways.  A 2D view will not have any viewEntities
        public Plane[] frustum = new Plane[5];             // positive sides face outward, [4] is the front clip plane
        public Frustum viewFrustum;

        public int areaNum;                // -1 = not in a valid area

        // An array in frame temporary memory that lists if an area can be reached without crossing a closed door.  This is used to avoid drawing interactions when the light is behind a closed door.
        public bool[] connectedAreas;
    }

    // complex light / surface interactions are broken up into multiple passes of a simple interaction shader
    public class DrawInteraction
    {
        public DrawSurf surf;

        public Image lightImage;
        public Image lightFalloffImage;
        public Image bumpImage;
        public Image diffuseImage;
        public Image specularImage;

        public Vector4 diffuseColor;    // may have a light color baked into it, will be < tr.backEndRendererMaxLight
        public Vector4 specularColor;   // may have a light color baked into it, will be < tr.backEndRendererMaxLight
        public StageVertexColor vertexColor; // applies to both diffuse and specular

        public int ambientLight;   // use tr.ambientNormalMap instead of normalization cube map (not a bool just to avoid an uninitialized memory check of the pad region by valgrind)

        // these are loaded into the vertex program
        public Vector4 localLightOrigin;
        public Vector4 localViewOrigin;
        public Matrix4x4 lightProjection; // S,T,R=Falloff,Q   // in local coordinates, possibly with a texture matrix baked in
        public Vector4[] bumpMatrix = new Vector4[2];
        public Vector4[] diffuseMatrix = new Vector4[2];
        public Vector4[] specularMatrix = new Vector4[2];
    }

    public enum RC
    {
        NOP,
        DRAW_VIEW,
        SET_BUFFER,
        COPY_RENDER,
        SWAP_BUFFERS,        // can't just assume swap at end of list because
        DIRECT_BUFFER_START,
        DIRECT_BUFFER_END
        // of forced list submission before syncs
    }

    public class EmptyCommand
    {
        public RC commandId;
        public EmptyCommand next;
    }

    public class SetBufferCommand : EmptyCommand
    {
        public int buffer;
        public int frameCount;
    }

    public class DrawSurfsCommand : EmptyCommand
    {
        public ViewDef viewDef;
    }

    public class CopyRenderCommand : EmptyCommand
    {
        public int x, y, imageWidth, imageHeight;
        public Image image;
        public int cubeFace;                   // when copying to a cubeMap
    }

    // a request for frame memory will never fail (until malloc fails), but it may force the allocation of a new memory block that will be discontinuous with the existing memory
    public class FrameMemoryBlock
    {
        // this is the inital allocation for max number of drawsurfs in a given view, but it will automatically grow if needed
        const int INITIAL_DRAWSURFS = 0x4000;
        public FrameMemoryBlock next;
        public int size;
        public int used;
        public int poop;           // so that base is 16 byte aligned dynamically allocated as [size]
        public byte base0;
        public byte base1;
        public byte base2;
        public byte base3;
    }

    // all of the information needed by the back end must be contained in a frameData_t.  This entire structure is
    // duplicated so the front and back end can run in parallel on an SMP machine (OBSOLETE: this capability has been removed)
    public class FrameData
    {
        // one or more blocks of memory for all frame temporary allocations
        public FrameMemoryBlock memory;

        // alloc will point somewhere into the memory chain
        public FrameMemoryBlock alloc;

        public SrfTriangles firstDeferredFreeTriSurf;
        public SrfTriangles lastDeferredFreeTriSurf;

        public int memoryHighwater;    // max used on any frame

        // the currently building command list commands can be inserted at the front if needed, as for required dynamically generated textures
        public EmptyCommand cmdHead, cmdTail;     // may be of other command type based on commandId
    }

    unsafe partial class TRX
    {
        //public static readonly FrameData frameData = new();

        public static void R_LockSurfaceScene(ViewDef parms);
        public static void R_ClearCommandChain();
        public static void R_AddDrawViewCmd(ViewDef parms);

        //public static void R_ReloadGuis_f(CmdArgs args);
        //public static void R_ListGuis_f(CmdArgs args);

        public static void* R_GetCommandBuffer(int bytes);

        // this allows a global override of all materials
        public static bool R_GlobalShaderOverride(Material[] shader);

        // this does various checks before calling the idDeclSkin
        public static Material R_RemapShaderBySkin(Material shader, DeclSkin customSkin, Material customShader);
    }

    public struct GLstate
    {
        const int MAX_MULTITEXTURE_UNITS = 8;
        const int MAX_GUI_SURFACES = 1024;      // default size of the drawSurfs list for guis, will be automatically expanded as needed

        public CT faceCulling;
        public int glStateBits;
        public bool forceGlState;      // the next GL_State will ignore glStateBits and set everything
        public int currentTexture;

        public ShaderProgram currentProgram;

        public void Clear()
            => this = new();
    }

    // all state modified by the back end is separated from the front end state
    public class BackEndState
    {
        public int frameCount;     // used to track all images used in a frame
        public ViewDef viewDef;
        public BackEndCounters pc;

        // Current states, for optimizations
        public ViewEntity currentSpace;       // for detecting when a matrix must change
        public ScreenRect currentScissor; // for scissor clipping, local inside renderView viewport
        public bool currentRenderCopied;   // true if any material has already referenced _currentRender

        // our OpenGL state deltas
        public GLstate glState;

        public int c_copyFrameBuffer;
    }

    public class PerformanceCounters
    {
        public int c_sphere_cull_in, c_sphere_cull_clip, c_sphere_cull_out;
        public int c_box_cull_in, c_box_cull_out;
        public int c_createInteractions;   // number of calls to idInteraction::CreateInteraction
        public int c_createLightTris;
        public int c_createShadowVolumes;
        public int c_generateMd5;
        public int c_entityDefCallbacks;
        public int c_alloc, c_free;    // counts for R_StaticAllc/R_StaticFree
        public int c_visibleViewEntities;
        public int c_shadowViewEntities;
        public int c_viewLights;
        public int c_numViews;         // number of total views rendered
        public int c_deformedSurfaces; // idMD5Mesh::GenerateSurface
        public int c_deformedVerts;    // idMD5Mesh::GenerateSurface
        public int c_deformedIndexes;  // idMD5Mesh::GenerateSurface
        public int c_tangentIndexes;   // R_DeriveTangents()
        public int c_entityUpdates, c_lightUpdates, c_entityReferences, c_lightReferences;
        public int c_guiSurfs;
        public int frontEndMsec;       // sum of time in all RE_RenderScene's in a frame
    }

    public class BackEndCounters
    {
        public int c_surfaces;
        public int c_shaders;
        public int c_vertexes;
        public int c_indexes;      // one set per pass
        public int c_totalIndexes; // counting all passes

        public int c_drawElements;
        public int c_drawIndexes;
        public int c_drawVertexes;
        public int c_drawRefIndexes;
        public int c_drawRefVertexes;

        public int c_shadowElements;
        public int c_shadowIndexes;
        public int c_shadowVertexes;

        public int c_vboIndexes;

        public int msec;           // total msec for backend run
    }

    public class RenderCrop
    {
        const int MAX_RENDER_CROPS = 8;

        public int x, y, width, height; // these are in physical, OpenGL Y-at-bottom pixels
    }

    //xthreadInfo renderThread = { 0 };

    // Most renderer globals are defined here. backend functions should never modify any of these fields, but may read fields that aren't dynamically modified by the frontend.
    public partial class RenderSystemLocal : IRenderSystem
    {
        // external functions
        public void Init();
        public void Shutdown();
        public void InitOpenGL();
        public void ShutdownOpenGL();
        public bool IsOpenGLRunning();
        public bool IsFullScreen();
        public int GetScreenWidth();
        public int GetScreenHeight();
        public float GetFOV();
        public int GetRefresh();

        public IRenderWorld AllocRenderWorld();
        public void FreeRenderWorld(IRenderWorld rw);
        public void BeginLevelLoad();
        public void EndLevelLoad();
        public bool RegisterFont(string fontName, FontInfoEx font);
        public void SetHudOpacity(float opacity);
        public void SetColor(Vector4 rgba);
        public void SetColor4(float r, float g, float b, float a);
        public void DrawStretchPic(DrawVert verts, GlIndex indexes, int vertCount, int indexCount, Material material, bool clip = true, float x = 0f, float y = 0f, float w = 640f, float h = 0f);
        public void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material);

        public void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material);
        public void GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc);
        public void GetGLSettings(out int width, out int height);
        public void PrintMemInfo(MemInfo mi);

        public void DrawSmallChar(int x, int y, int ch, Material material);
        public void DrawSmallStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
        public void DrawBigChar(int x, int y, int ch, Material material);
        public void DrawBigStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
        public void WriteDemoPics();
        public void DrawDemoPics();
        public void BeginFrame(int windowWidth, int windowHeight);
        public void EndFrame(int frontEndMsec, int backEndMsec);
        public void TakeScreenshot(int width, int height, string fileName, int downSample, RenderView @ref);
        public void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false);
        public void CaptureRenderToImage(string imageName);
        public void CaptureRenderToFile(string fileName, bool fixAlpha);
        public void UnCrop();
        public bool UploadImage(string imageName, byte[] data, int width, int height);

        public void DirectFrameBufferStart();
        public void DirectFrameBufferEnd();

        // internal functions
        RenderSystemLocal();

        public void Clear();
        public void SetBackEndRenderer();          // sets tr.backEndRenderer based on cvars
        public void RenderViewToViewport(RenderView renderView, ScreenRect viewport);

        public bool multithreadActive = false;

        public bool useSpinLock = true;
        public int spinLockDelay = 1000;
        public float hudOpacity = 0f;

        public bool windowActive = false; // True when the app is at the foreground and not minimised

        public volatile bool backendThreadRun = false;
        public volatile bool backendFinished = true;
        public volatile bool imagesFinished = false;

        public volatile bool backendThreadShutdown = false;

        public volatile FrameData fdToRender = null;
        public volatile int vertListToRender = 0;

        // These are set if the backend should save pixels
        public volatile RenderCrop pixelsCrop = null;
        public volatile byte[] pixels = null;

        // For FPS limiting
        public uint lastRenderTime = 0;

        // The backend task
        public void BackendThreadTask();

        // The backend thread
        public void BackendThread();

        // Start (and create) the back thread
        public void BackendThreadExecute();

        // Wait for backend thread to finish
        public void BackendThreadWait();

        public void BackendThreadShutdown();

        // Call this to render the current command buffer. If you pass is pixels it will block and perform a glReadPixels
        public void RenderCommands(RenderCrop pixelsCrop, byte[] pixels);

        // Static runner to start thread
        public static int BackendThreadRunner(void* localRenderSystem);

        // renderer globals
        public bool registered;        // cleared at shutdown, set at InitOpenGL

        public bool takingScreenshot;

        public int frameCount;     // incremented every frame
        public int viewCount;      // incremented every view (twice a scene if subviewed) and every R_MarkFragments call

        public int staticAllocCount;   // running total of bytes allocated

        public float frameShaderTime;  // shader time for all non-world 2D rendering

        public int[] viewportOffset = new int[2];  // for doing larger-than-window tiled renderings
        public int[] tiledViewport = new int[2];

        public Vector4 ambientLightVector;  // used for "ambient bump mapping"

        public float sortOffset;               // for determinist sorting of equal sort materials

        public List<RenderWorldLocal> worlds;

        public RenderWorldLocal primaryWorld;
        public RenderView primaryRenderView;
        public ViewDef primaryView;
        // many console commands need to know which world they should operate on

        public Material defaultMaterial;
        public Image testImage;
        public Cinematic testVideo;
        public float testVideoStartTime;

        public Image ambientCubeImage;  // hack for testing dependent ambient lighting

        public ViewDef viewDef;

        public PerformanceCounters pc;                   // performance counters

        public DrawSurfsCommand lockSurfacesCmd; // use this when r_lockSurfaces = 1

        public ViewEntity identitySpace;     // can use if we don't know viewDef->worldSpace is valid

        public RenderCrop[] renderCrops = new RenderCrop[MAX_RENDER_CROPS];
        public int currentRenderCrop;

        // GUI drawing variables for surface creation
        public int guiRecursionLevel;      // to prevent infinite overruns
        public GuiModel guiModel;
        public GuiModel demoGuiModel;

        // DG: remember the original glConfig.vidWidth/Height values that get overwritten in BeginFrame() so they can be reset in EndFrame() (Editors tend to mess up the viewport by using BeginFrame())
        public int origWidth;
        public int origHeight;
    }

    static partial class TRX
    {
        //public static readonly BackEndState backEnd;
        //public static readonly RenderSystemLocal tr;
        public static readonly Glconfig glConfig;     // outside of TR since it shouldn't be cleared during ref re-init

        // cvars
        public static readonly CVar r_mode;                   // video mode number
        public static readonly CVar r_displayRefresh;         // optional display refresh rate option for vid mode
        public static readonly CVar r_fullscreen;             // 0 = windowed, 1 = full screen
        public static readonly CVar r_multiSamples;           // number of antialiasing samples

        public static readonly CVar r_ignore;                 // used for random debugging without defining new vars
        public static readonly CVar r_ignore2;                // used for random debugging without defining new vars
        public static readonly CVar r_znear;                  // near Z clip plane

        //extern CVar r_finish;                 // force a call to glFinish() every frame
        //extern CVar r_swapInterval;           // changes the GL swap interval
        //extern CVar r_offsetFactor;           // polygon offset parameter
        //extern CVar r_offsetUnits;            // polygon offset parameter
        //extern CVar r_clear;                  // force screen clear every frame
        //extern CVar r_shadows;                // enable shadows
        //extern CVar r_subviewOnly;            // 1 = don't render main view, allowing subviews to be debugged
        //extern CVar r_lightScale;             // all light intensities are multiplied by this, which is normally 2
        //extern CVar r_flareSize;              // scale the flare deforms from the material def

        public static CVar r_gamma;                  // changes gamma tables
        public static CVar r_brightness;             // changes gamma tables

        public static CVar r_checkBounds;            // compare all surface bounds with precalculated ones

        public static CVar r_usePhong;
        public static CVar r_specularExponent;
        public static CVar r_useLightPortalFlow;     // 1 = do a more precise area reference determination
        public static CVar r_useShadowSurfaceScissor;// 1 = scissor shadows by the scissor rect of the interaction surfaces
        public static CVar r_useConstantMaterials;   // 1 = use pre-calculated material registers if possible
        public static CVar r_useInteractionTable;    // create a full entityDefs * lightDefs table to make finding interactions faster
        public static CVar r_useNodeCommonChildren;  // stop pushing reference bounds early when possible
        public static CVar r_useSilRemap;            // 1 = consider verts with the same XYZ, but different ST the same for shadows
        public static CVar r_useCulling;             // 0 = none, 1 = sphere, 2 = sphere + box
        public static CVar r_useLightCulling;        // 0 = none, 1 = box, 2 = exact clip of polyhedron faces
        public static CVar r_useLightScissors;       // 1 = use custom scissor rectangle for each light
        public static CVar r_useClippedLightScissors;// 0 = full screen when near clipped, 1 = exact when near clipped, 2 = exact always
        public static CVar r_useEntityCulling;       // 0 = none, 1 = box
        public static CVar r_useEntityScissors;      // 1 = use custom scissor rectangle for each entity
        public static CVar r_useInteractionCulling;  // 1 = cull interactions
        public static CVar r_useInteractionScissors; // 1 = use a custom scissor rectangle for each interaction
        public static CVar r_useFrustumFarDistance;  // if != 0 force the view frustum far distance to this distance
        public static CVar r_useShadowCulling;       // try to cull shadows from partially visible lights
        public static CVar r_usePreciseTriangleInteractions; // 1 = do winding clipping to determine if each ambiguous tri should be lit
        public static CVar r_useTurboShadow;         // 1 = use the infinite projection with W technique for dynamic shadows
        public static CVar r_useExternalShadows;     // 1 = skip drawing caps when outside the light volume
        public static CVar r_useOptimizedShadows;    // 1 = use the dmap generated static shadow volumes
        public static CVar r_useShadowProjectedCull; // 1 = discard triangles outside light volume before shadowing
        public static CVar r_useDeferredTangents;    // 1 = don't always calc tangents after deform
        public static CVar r_useCachedDynamicModels; // 1 = cache snapshots of dynamic models
        public static CVar r_useInfiniteFarZ;        // 1 = use the no-far-clip-plane trick
        public static CVar r_useScissor;             // 1 = scissor clip as portals and lights are processed
        public static CVar r_usePortals;             // 1 = use portals to perform area culling, otherwise draw everything
        public static CVar r_useStateCaching;        // avoid redundant state changes in GL_*() calls
        public static CVar r_useEntityCallbacks;     // if 0, issue the callback immediately at update time, rather than defering
        public static CVar r_lightAllBackFaces;      // light all the back faces, even when they would be shadowed

        public static CVar r_skipPostProcess;        // skip all post-process renderings
        public static CVar r_skipSuppress;           // ignore the per-view suppressions
        public static CVar r_skipInteractions;       // skip all light/surface interaction drawing
        public static CVar r_skipFrontEnd;           // bypasses all front end work, but 2D gui rendering still draws
        public static CVar r_skipBackEnd;            // don't draw anything
        public static CVar r_skipCopyTexture;        // do all rendering, but don't actually copyTexSubImage2D
        public static CVar r_skipRender;             // skip 3D rendering, but pass 2D
        public static CVar r_skipTranslucent;        // skip the translucent interaction rendering
        public static CVar r_skipAmbient;            // bypasses all non-interaction drawing
        public static CVar r_skipNewAmbient;         // bypasses all vertex/fragment program ambients
        public static CVar r_skipBlendLights;        // skip all blend lights
        public static CVar r_skipFogLights;          // skip all fog lights
        public static CVar r_skipSubviews;           // 1 = don't render any mirrors / cameras / etc
        public static CVar r_skipGuiShaders;         // 1 = don't render any gui elements on surfaces
        public static CVar r_skipParticles;          // 1 = don't render any particles
        public static CVar r_skipUpdates;            // 1 = don't accept any entity or light updates, making everything static
        public static CVar r_skipDeforms;            // leave all deform materials in their original state
        public static CVar r_skipDynamicTextures;    // don't dynamically create textures
        public static CVar r_skipBump;               // uses a flat surface instead of the bump map
        public static CVar r_skipSpecular;           // use black for specular
        public static CVar r_skipDiffuse;            // use black for diffuse
        public static CVar r_skipOverlays;           // skip overlay surfaces
        public static CVar r_skipROQ;

        public static CVar r_ignoreGLErrors;

        public static CVar r_forceLoadImages;        // draw all images to screen after registration
        public static CVar r_demonstrateBug;         // used during development to show IHV's their problems
        public static CVar r_screenFraction;         // for testing fill rate, the resolution of the entire screen can be changed

        public static CVar r_showUnsmoothedTangents; // highlight geometry rendered with unsmoothed tangents
        public static CVar r_showSilhouette;         // highlight edges that are casting shadow planes
        public static CVar r_showVertexColor;        // draws all triangles with the solid vertex color
        public static CVar r_showUpdates;            // report entity and light updates and ref counts
        public static CVar r_showDemo;               // report reads and writes to the demo file
        public static CVar r_showDynamic;            // report stats on dynamic surface generation
        public static CVar r_showIntensity;          // draw the screen colors based on intensity, red = 0, green = 128, blue = 255
        public static CVar r_showDefs;               // report the number of modeDefs and lightDefs in view
        public static CVar r_showDepth;              // display the contents of the depth buffer and the depth range
        public static CVar r_showTris;               // enables wireframe rendering of the world
        public static CVar r_showSurfaceInfo;        // show surface material name under crosshair
        public static CVar r_showNormals;            // draws wireframe normals
        public static CVar r_showEdges;              // draw the sil edges
        public static CVar r_showViewEntitys;        // displays the bounding boxes of all view models and optionally the index
        public static CVar r_showTexturePolarity;    // shade triangles by texture area polarity
        public static CVar r_showTangentSpace;       // shade triangles by tangent space
        public static CVar r_showDominantTri;        // draw lines from vertexes to center of dominant triangles
        public static CVar r_showTextureVectors;     // draw each triangles texture (tangent) vectors
        public static CVar r_showLights;             // 1 = print light info, 2 = also draw volumes
        public static CVar r_showLightCount;         // colors surfaces based on light count
        public static CVar r_showShadowCount;        // colors screen based on shadow volume depth complexity
        public static CVar r_showLightScissors;      // show light scissor rectangles
        public static CVar r_showEntityScissors;     // show entity scissor rectangles
        public static CVar r_showInteractionFrustums;// show a frustum for each interaction
        public static CVar r_showInteractionScissors;// show screen rectangle which contains the interaction frustum
        public static CVar r_showMemory;             // print frame memory utilization
        public static CVar r_showCull;               // report sphere and box culling stats
        public static CVar r_showInteractions;       // report interaction generation activity
        public static CVar r_showSurfaces;           // report surface/light/shadow counts
        public static CVar r_showPrimitives;         // report vertex/index/draw counts
        public static CVar r_showPortals;            // draw portal outlines in color based on passed / not passed
        public static CVar r_showAlloc;              // report alloc/free counts
        public static CVar r_showSkel;               // draw the skeleton when model animates
        public static CVar r_jointNameScale;         // size of joint names when r_showskel is set to 1
        public static CVar r_jointNameOffset;        // offset of joint names when r_showskel is set to 1

        public static CVar r_testGamma;              // draw a grid pattern to test gamma levels
        public static CVar r_testStepGamma;          // draw a grid pattern to test gamma levels
        public static CVar r_testGammaBias;          // draw a grid pattern to test gamma levels

        public static CVar r_singleLight;            // suppress all but one light
        public static CVar r_singleEntity;           // suppress all but one entity
        public static CVar r_singleArea;             // only draw the portal area the view is actually in
        public static CVar r_singleSurface;          // suppress all but one surface on each entity
        public static CVar r_shadowPolygonOffset;    // bias value added to depth test for stencil shadow drawing
        public static CVar r_shadowPolygonFactor;    // scale value for stencil shadow drawing

        public static CVar r_jitter;                 // randomly subpixel jitter the projection matrix
        public static CVar r_lightSourceRadius;      // for soft-shadow sampling
        public static CVar r_lockSurfaces;
        public static CVar r_orderIndexes;           // perform index reorganization to optimize vertex use

        public static CVar r_debugLineDepthTest;     // perform depth test on debug lines
        public static CVar r_debugLineWidth;         // width of debug lines
        public static CVar r_debugArrowStep;         // step size of arrow cone line rotation in degrees
        public static CVar r_debugPolygonFilled;

        public static CVar r_materialOverride;       // override all materials

        public static CVar r_debugRenderToTexture;

        public static CVar r_multithread;            // enable multithread
        public static CVar r_noLight;                // no lighting
        public static CVar r_useETC1;                // ETC1 compression
        public static CVar r_useETC1Cache;           // use ETC1 cache
        public static CVar r_useIndexVBO;
        public static CVar r_useVertexVBO;
        public static CVar r_maxFps;

        #region GL wrapper/helper functions

        //public static void GL_SelectTexture(int unit);
        public static void GL_CheckErrors();
        //public static void GL_ClearStateDelta();
        //public static void GL_State(int stateVector);
        //public static void GL_Cull(int cullType);

        public const int GLS_SRCBLEND_ZERO = 0x00000001;
        public const int GLS_SRCBLEND_ONE = 0x0;
        public const int GLS_SRCBLEND_DST_COLOR = 0x00000003;
        public const int GLS_SRCBLEND_ONE_MINUS_DST_COLOR = 0x00000004;
        public const int GLS_SRCBLEND_SRC_ALPHA = 0x00000005;
        public const int GLS_SRCBLEND_ONE_MINUS_SRC_ALPHA = 0x00000006;
        public const int GLS_SRCBLEND_DST_ALPHA = 0x00000007;
        public const int GLS_SRCBLEND_ONE_MINUS_DST_ALPHA = 0x00000008;
        public const int GLS_SRCBLEND_ALPHA_SATURATE = 0x00000009;
        public const int GLS_SRCBLEND_BITS = 0x0000000f;

        public const int GLS_DSTBLEND_ZERO = 0x0;
        public const int GLS_DSTBLEND_ONE = 0x00000020;
        public const int GLS_DSTBLEND_SRC_COLOR = 0x00000030;
        public const int GLS_DSTBLEND_ONE_MINUS_SRC_COLOR = 0x00000040;
        public const int GLS_DSTBLEND_SRC_ALPHA = 0x00000050;
        public const int GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA = 0x00000060;
        public const int GLS_DSTBLEND_DST_ALPHA = 0x00000070;
        public const int GLS_DSTBLEND_ONE_MINUS_DST_ALPHA = 0x00000080;
        public const int GLS_DSTBLEND_BITS = 0x000000f0;

        // these masks are the inverse, meaning when set the glColorMask value will be 0, preventing that channel from being written
        public const int GLS_DEPTHMASK = 0x00000100;
        public const int GLS_REDMASK = 0x00000200;
        public const int GLS_GREENMASK = 0x00000400;
        public const int GLS_BLUEMASK = 0x00000800;
        public const int GLS_ALPHAMASK = 0x00001000;
        public const int GLS_COLORMASK = (GLS_REDMASK | GLS_GREENMASK | GLS_BLUEMASK);

        public const int GLS_DEPTHFUNC_ALWAYS = 0x00010000;
        public const int GLS_DEPTHFUNC_EQUAL = 0x00020000;
        public const int GLS_DEPTHFUNC_LESS = 0x0;

        public const int GLS_DEFAULT = GLS_DEPTHFUNC_ALWAYS;

        public static void R_Init();
        public static void R_InitOpenGL();

        //public static void R_DoneFreeType();

        public static void R_SetColorMappings();

        public static void R_ScreenShot_f(CmdArgs args);

        public static bool R_CheckExtension(string name);

        #endregion

        #region IMPLEMENTATION SPECIFIC FUNCTIONS

        public struct GlimpParms
        {
            public int width;
            public int height;
            public bool fullScreen;
            public bool stereo;
            public int displayHz;
            public int multiSamples;
        }

        public static bool GLimp_Init(GlimpParms parms);
        // If the desired mode can't be set satisfactorily, false will be returned. The renderer will then reset the glimpParms to "safe mode" of 640x480 fullscreen and try again.  If that also fails, the error will be fatal.

        public static bool GLimp_SetScreenParms(GlimpParms parms);
        // will set up gl up with the new parms

        public static void GLimp_Shutdown();
        // Destroys the rendering context, closes the window, resets the resolution, and resets the gamma ramps.

        public static void GLimp_SetupFrame(int a);

        public static void GLimp_SwapBuffers();
        // Calls the system specific swapbuffers routine, and may also perform other system specific cvar checks that happen every frame. This will not be called if 'r_drawBuffer GL_FRONT'

        public static void GLimp_SetGamma(ushort[] red, ushort[] green, ushort[] blue);
        // Sets the hardware gamma ramps for gamma and brightness adjustment. These are now taken as 16 bit values, so we can take full advantage of dacs with >8 bits of precision

        const int GRAB_ENABLE = 1 << 0;
        const int GRAB_REENABLE = 1 << 1;
        const int GRAB_HIDECURSOR = 1 << 2;
        const int GRAB_SETSTATE = 1 << 3;

        public static void GLimp_GrabInput(int flags);

        public static void GLimp_WindowActive(bool active);

        #endregion

        #region MAIN

        public static void R_RenderView(ViewDef parms);

        // performs radius cull first, then corner cull
        public static bool R_CullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes);
        public static bool R_RadiusCullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes);
        public static bool R_CornerCullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes);

        public static void R_AxisToModelMatrix(Matrix3x3 axis, Vector3 origin, float[] modelMatrix);

        // note that many of these assume a normalized matrix, and will not work with scaled axis
        public static void R_GlobalPointToLocal(float[] modelMatrix, Vector3 i, out Vector3 o);
        public static void R_GlobalVectorToLocal(float[] modelMatrix, Vector3 i, out Vector3 o);
        public static void R_GlobalPlaneToLocal(float[] modelMatrix, Plane i, out Plane o);
        public static void R_PointTimesMatrix(float[] modelMatrix, Vector4 i, out Vector4 o);
        public static void R_LocalPointToGlobal(float[] modelMatrix, Vector3 i, out Vector3 o);
        public static void R_LocalVectorToGlobal(float[] modelMatrix, Vector3 i, out Vector3 o);
        public static void R_LocalPlaneToGlobal(float[] modelMatrix, Plane i, out Plane o);
        public static void R_TransformEyeZToWin(float src_z, float[] projectionMatrix, out float dst_z);

        public static void R_GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc);

        public static void R_TransformModelToClip(Vector3 src, float[] modelMatrix, float[] projectionMatrix, Plane eye, Plane dst);

        public static void R_TransformClipToDevice(Plane clip, ViewDef view, Vector3 normalized);

        public static void R_TransposeGLMatrix(float[] i, out float[] o);

        public static void R_SetViewMatrix(ViewDef viewDef);

        public static void myGlMultMatrix(float[] a, float[] b, float[] o);

        #endregion

        #region LIGHT

        public static void R_ListRenderLightDefs_f(CmdArgs args);
        public static void R_ListRenderEntityDefs_f(CmdArgs args);

        public static bool R_IssueEntityDefCallback(RenderEntityLocal def);
        public static IRenderModel R_EntityDefDynamicModel(RenderEntityLocal def);

        public static ViewEntity R_SetEntityDefViewEntity(RenderEntityLocal def);
        public static ViewLight R_SetLightDefViewLight(RenderLightLocal def);

        public static void R_AddDrawSurf(SrfTriangles tri, ViewEntity space, IRenderEntity renderEntity, Material shader, ScreenRect scissor);

        public static void R_LinkLightSurf(DrawSurf[] link, SrfTriangles tri, ViewEntity space, RenderLightLocal light, Material shader, ScreenRect scissor, bool viewInsideShadow);

        public static bool R_CreateAmbientCache(SrfTriangles tri, bool needsLighting);
        public static bool R_CreateIndexCache(SrfTriangles tri);
        public static bool R_CreatePrivateShadowCache(SrfTriangles tri);
        public static bool R_CreateVertexProgramShadowCache(SrfTriangles tri);

        #endregion

        #region LIGHTRUN

        public static void R_RegenerateWorld_f(CmdArgs args);

        public static void R_ModulateLights_f(CmdArgs args);

        public static void R_SetLightProject(Plane[] lightProject, Vector3 origin, Vector3 targetPoint, Vector3 rightVector, Vector3 upVector, Vector3 start, Vector3 stop);

        public static void R_AddLightSurfaces();
        public static void R_AddModelSurfaces();
        public static void R_RemoveUnecessaryViewLights();

        public static void R_FreeDerivedData();
        public static void R_ReCreateWorldReferences();

        public static void R_CreateEntityRefs(RenderEntityLocal def);
        public static void R_CreateLightRefs(RenderLightLocal light);

        public static void R_DeriveLightData(RenderLightLocal light);
        public static void R_FreeLightDefDerivedData(RenderLightLocal light);
        public static void R_CheckForEntityDefsUsingModel(RenderModel model);

        public static void R_ClearEntityDefDynamicModel(RenderEntityLocal def);
        public static void R_FreeEntityDefDerivedData(RenderEntityLocal def, bool keepDecals, bool keepCachedDynamicModel);
        public static void R_FreeEntityDefCachedDynamicModel(RenderEntityLocal def);
        public static void R_FreeEntityDefDecals(RenderEntityLocal def);
        public static void R_FreeEntityDefOverlay(RenderEntityLocal def);
        public static void R_FreeEntityDefFadedDecals(RenderEntityLocal def, int time);

        public static void R_CreateLightDefFogPortals(RenderLightLocal ldef);

        // Framebuffer stuff
        public static void R_InitFrameBuffer();
        public static void R_FrameBufferStart();
        public static void R_FrameBufferEnd();

        #endregion

        #region POLYTOPE

        public static SrfTriangles R_PolytopeSurface(int numPlanes, Plane[] planes, Winding[] windings);

        #endregion

        #region RENDER BACKEND
        // NB: Not touching to GLSL shader stuff. This is using classic OGL calls only.

        public static void RB_DrawView(DrawSurfsCommand data);
        public static void RB_RenderView();

        public static void RB_DrawElementsWithCounters(DrawSurf surf);
        public static void RB_DrawShadowElementsWithCounters(DrawSurf surf, int numIndexes);
        public static void RB_SubmittInteraction(DrawInteraction din, Action<DrawInteraction> drawInteraction);
        public static void RB_SetDrawInteraction(ShaderStage surfaceStage, float[] surfaceRegs, Image[] image, Vector4[] matrix, float[] color);
        public static void RB_BindVariableStageImage(TextureStage texture, float[] shaderRegisters);
        public static void RB_BeginDrawingView();
        public static void RB_GetShaderTextureMatrix(float[] shaderRegisters, TextureStage texture, float[] matrix);
        public static void RB_BakeTextureMatrixIntoTexgen(Matrix4x4 lightProject, float[] textureMatrix);

        #endregion

        #region DRAW_GLSL
        // NB: Specific to GLSL shader stuff

        public class ShaderProgram
        {
            uint program;

            uint vertexShader;
            uint fragmentShader;

            int glColor;
            int alphaTest;
            int specularExponent;

            int modelMatrix;

            // New for multiview - The view and projection matrix uniforms
            uint projectionMatrixBinding;
            uint viewMatricesBinding;

            int modelViewMatrix;
            int textureMatrix;
            int localLightOrigin;
            int localViewOrigin;

            int lightProjection;

            int bumpMatrixS;
            int bumpMatrixT;
            int diffuseMatrixS;
            int diffuseMatrixT;
            int specularMatrixS;
            int specularMatrixT;

            int colorModulate;
            int colorAdd;
            int diffuseColor;
            int specularColor;
            int fogColor;

            int fogMatrix;

            int clipPlane;

            /* gl_... */
            int attr_TexCoord;
            int attr_Tangent;
            int attr_Bitangent;
            int attr_Normal;
            int attr_Vertex;
            int attr_Color;

            int[] u_fragmentMap = new int[MAX_FRAGMENT_IMAGES];
            int[] u_fragmentCubeMap = new int[MAX_FRAGMENT_IMAGES];
        }

        void R_ReloadGLSLPrograms_f(CmdArgs args);

        void RB_GLSL_PrepareShaders();
        void RB_GLSL_FillDepthBuffer(DrawSurf[] drawSurfs, int numDrawSurfs);
        void RB_GLSL_DrawInteractions();
        int RB_GLSL_DrawShaderPasses(DrawSurf[] drawSurfs, int numDrawSurfs);
        void RB_GLSL_FogAllLights();

        // TR_STENCILSHADOWS
        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1
        void R_MakeShadowFrustums(RenderLightLocal def);

        public enum ShadowGen
        {
            SG_DYNAMIC,     // use infinite projections
            SG_STATIC,      // clip to bounds
        }
        SrfTriangles R_CreateShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, ShadowGen optimize, SrfCullInfo cullInfo);

        // TR_TURBOSHADOW
        // Fast, non-clipped overshoot shadow volumes
        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1 calling this function may modify "facing" based on culling
        //public static SrfTriangles R_CreateVertexProgramTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);
        ////public static SrfTriangles R_CreateTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);

        #endregion

        #region TRISURF

        public static void R_InitTriSurfData();
        public static void R_ShutdownTriSurfData();
        public static void R_PurgeTriSurfData(FrameData frame);
        public static void R_ShowTriSurfMemory_f(CmdArgs args);

        public static SrfTriangles R_AllocStaticTriSurf();
        public static SrfTriangles R_CopyStaticTriSurf(SrfTriangles tri);
        public static void R_AllocStaticTriSurfVerts(SrfTriangles tri, int numVerts);
        public static void R_AllocStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
        public static void R_AllocStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
        public static void R_AllocStaticTriSurfPlanes(SrfTriangles tri, int numIndexes);
        public static void R_ResizeStaticTriSurfVerts(SrfTriangles tri, int numVerts);
        public static void R_ResizeStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
        public static void R_ResizeStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
        public static void R_ReferenceStaticTriSurfVerts(SrfTriangles tri, SrfTriangles reference);
        public static void R_ReferenceStaticTriSurfIndexes(SrfTriangles tri, SrfTriangles reference);
        public static void R_FreeStaticTriSurfSilIndexes(SrfTriangles tri);
        public static void R_FreeStaticTriSurf(SrfTriangles tri);
        public static void R_FreeStaticTriSurfVertexCaches(SrfTriangles tri);
        public static void R_ReallyFreeStaticTriSurf(SrfTriangles tri);
        public static void R_FreeDeferredTriSurfs(FrameData frame);
        public static int R_TriSurfMemory(SrfTriangles tri);

        public static void R_BoundTriSurf(SrfTriangles tri);
        public static void R_RemoveDuplicatedTriangles(SrfTriangles tri);
        public static void R_CreateSilIndexes(SrfTriangles tri);
        public static void R_RemoveDegenerateTriangles(SrfTriangles tri);
        public static void R_RemoveUnusedVerts(SrfTriangles tri);
        public static void R_RangeCheckIndexes(SrfTriangles tri);
        public static void R_CreateVertexNormals(SrfTriangles tri);    // also called by dmap
        public static void R_DeriveFacePlanes(SrfTriangles tri);       // also called by renderbump
        public static void R_CleanupTriangles(SrfTriangles tri, bool createNormals, bool identifySilEdges, bool useUnsmoothedTangents);
        public static void R_ReverseTriangles(SrfTriangles tri);

        // Only deals with vertexes and indexes, not silhouettes, planes, etc. Does NOT perform a cleanup triangles, so there may be duplicated verts in the result.
        public static SrfTriangles R_MergeSurfaceList(SrfTriangles_t[] surfaces, int numSurfaces);
        public static SrfTriangles R_MergeTriangles(SrfTriangles_t tri1, SrfTriangles tri2);

        // if the deformed verts have significant enough texture coordinate changes to reverse the texture polarity of a triangle, the tangents will be incorrect
        public static void R_DeriveTangents(SrfTriangles tri, bool allocFacePlanes = true);

        // deformable meshes precalculate as much as possible from a base frame, then generate complete srfTriangles_t from just a new set of vertexes
        public class DeformInfo
        {
            public int numSourceVerts;

            // numOutputVerts may be smaller if the input had duplicated or degenerate triangles it will often be larger if the input had mirrored texture seams that needed to be busted for proper tangent spaces
            public int numOutputVerts;
            public DrawVert verts;

            public int numMirroredVerts;
            public int[] mirroredVerts;

            public int numIndexes;
            public GlIndex[] indexes;

            public GlIndex[] silIndexes;

            public int numDupVerts;
            public int[] dupVerts;

            public int numSilEdges;
            public SilEdge[] silEdges;

            public DominantTri[] dominantTris;
        }

        public static DeformInfo R_BuildDeformInfo(int numVerts, DrawVert[] verts, int numIndexes, int[] indexes, bool useUnsmoothedTangents);
        public static void R_FreeDeformInfo(DeformInfo deformInfo);
        public static int R_DeformInfoMemoryUsed(DeformInfo deformInfo);

        #endregion

        #region SUBVIEW

        public static bool R_PreciseCullSurface(DrawSurf drawSurf, Bounds ndcBounds);
        public static bool R_GenerateSubViews();

        #endregion

        #region SCENE GENERATION

        public static void R_InitFrameData();
        public static void R_ShutdownFrameData();
        public static int R_CountFrameData();
        public static void R_ToggleSmpFrame();
        public static T R_FrameAlloc<T>();
        public static T[] R_FrameAllocMany<T>(int count);
        public static T R_ClearedFrameAlloc<T>();
        public static T[] R_ClearedFrameAllocMany<T>(int count);
        public static void R_FrameFree<T>(ref T data);

        public static void R_DirectFrameBufferStart();
        public static void R_DirectFrameBufferEnd();

        public static T R_StaticAlloc<T>();     // just malloc with error checking
        public static T R_ClearedStaticAlloc<T>();  // with memset
        public static void R_StaticFree<T>(ref T data);

        #endregion

        #region TR_BACKEND

        //public static void RB_SetDefaultGLState();
        //public static void RB_ExecuteBackEndCommands(EmptyCommand cmds);

        #endregion

        #region TR_GUISURF

        //public static void R_SurfaceToTextureAxis(SrfTriangles tri, Vector3 origin, Vector3[] axis);
        //public static void R_RenderGuiSurf(IUserInterface gui, DrawSurf drawSurf);

        #endregion

        #region TR_ORDERINDEXES

        public static void R_OrderIndexes(int numIndexes, GlIndex[] indexes);

        #endregion

        #region TR_DEFORM

        public static void R_DeformDrawSurf(DrawSurf drawSurf);

        #endregion

        #region TR_TRACE

        public struct LocalTrace
        {
            public float fraction;
            // only valid if fraction < 1.0
            public Vector3 point;
            public Vector3 normal;
            public int[] indexes;
        }

        //public static LocalTrace R_LocalTrace(Vector3 start, Vector3 end, float radius, SrfTriangles tri);

        #endregion

        #region TR_SHADOWBOUNDS

        public static ScreenRect R_CalcIntersectionScissor(RenderLightLocal lightDef, RenderEntityLocal entityDef, ViewDef viewDef);

        #endregion
    }
}