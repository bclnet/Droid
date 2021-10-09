using Gengine.Render;
using System.NumericsX;
using System.NumericsX.OpenStack;

const int LIGHT_TRIS_DEFERRED = ((SrfTriangles*)-1);
const int LIGHT_CULL_ALL_FRONT = ((byte*)-1);
const float LIGHT_CLIP_EPSILON = 0.1f;

public struct SrfCullInfo
{
    // For each triangle a byte set to 1 if facing the light origin.
    byte* facing;

    // For each vertex a byte with the bits [0-5] set if the vertex is at the back side of the corresponding clip plane.
    // If the 'cullBits' pointer equals LIGHT_CULL_ALL_FRONT all vertices are at the front of all the clip planes.
    byte* cullBits;

    // Clip planes in surface space used to calculate the cull bits.
    Plane localClipPlanes[6];
}

public struct SurfaceInteraction
{
    // if lightTris == LIGHT_TRIS_DEFERRED, then the calculation of the
    // lightTris has been deferred, and must be done if ambientTris is visible
    SrfTriangles lightTris;

    // shadow volume triangle surface
    SrfTriangles shadowTris;

    // so we can check ambientViewCount before adding lightTris, and get at the shared vertex and possibly shadowVertex caches
    SrfTriangles ambientTris;

    Material shader;

    int expCulled;          // only for the experimental shadow buffer renderer

    SrfCullInfo cullInfo;
}

class AreaNumRef
{
    public AreaNumRef next;
    public int areaNum;
}

class Interaction
{
    // this may be 0 if the light and entity do not actually intersect -1 = an untested interaction
    public int numSurfaces;

    // if there is a whole-entity optimized shadow hull, it will
    // be present as a surfaceInteraction_t with a NULL ambientTris, but
    // possibly having a shader to specify the shadow sorting order
    public SurfaceInteraction surfaces;

    // get space from here, if NULL, it is a pre-generated shadow volume from dmap
    public RenderEntityLocal entityDef;
    public RenderLightLocal lightDef;

    public Interaction lightNext;               // for lightDef chains
    public Interaction lightPrev;
    public Interaction entityNext;              // for entityDef chains
    public Interaction entityPrev;

    public Interaction();

    // because these are generated and freed each game tic for active elements all over the world, we use a custom pool allocater to avoid memory allocation overhead and fragmentation
    public static Interaction AllocAndLink(RenderEntityLocal edef, RenderLightLocal ldef);

    // unlinks from the entity and light, frees all surfaceInteractions, and puts it back on the free list
    public void UnlinkAndFree();

    // free the interaction surfaces
    public void FreeSurfaces();

    // makes the interaction empty for when the light and entity do not actually intersect all empty interactions are linked at the end of the light's and entity's interaction list
    public void MakeEmpty();

    // returns true if the interaction is empty
    public bool IsEmpty
        => numSurfaces == 0;

    // returns true if the interaction is not yet completely created
    public bool IsDeferred
        => numSurfaces == -1;

    // returns true if the interaction has shadows
    public bool HasShadows { get; }

    // counts up the memory used by all the surfaceInteractions, which will be used to determine when we need to start purging old interactions
    public int MemoryUsed { get; }

    // makes sure all necessary light surfaces and shadow surfaces are created, and calls R_LinkLightSurf() for each one
    public void AddActiveInteraction();

    enum FrustumState
    {
        FRUSTUM_UNINITIALIZED,
        FRUSTUM_INVALID,
        FRUSTUM_VALID,
        FRUSTUM_VALIDAREAS,
    }
    Frustum frustum;              // frustum which contains the interaction
    AreaNumRef frustumAreas;         // numbers of the areas the frustum touches

    int dynamicModelFrameCount; // so we can tell if a callback model animated

    // actually create the interaction
    void CreateInteraction(RenderModel model);

    // unlink from entity and light lists
    void Unlink();

    // try to determine if the entire interaction, including shadows, is guaranteed
    // to be outside the view frustum
    bool CullInteractionByViewFrustum(Frustum viewFrustum);

    // determine the minimum scissor rect that will include the interaction shadows projected to the bounds of the light
    ScreenRect CalcInteractionScissorRectangle(Frustum viewFrustum);
}

void R_CalcInteractionFacing(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, in SrfCullInfo cullInfo);
void R_CalcInteractionCullBits(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, in SrfCullInfo cullInfo);
void R_FreeInteractionCullInfo(SrfCullInfo cullInfo);

void R_ShowInteractionMemory_f(CmdArgs args);

