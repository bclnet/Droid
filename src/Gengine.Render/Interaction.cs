#define LIGHT_TRIS_DEFERRED			((srfTriangles_t *)-1)
#define LIGHT_CULL_ALL_FRONT		((byte *)-1)
#define LIGHT_CLIP_EPSILON			0.1f


typedef struct {
    // For each triangle a byte set to 1 if facing the light origin.
byte* facing;

// For each vertex a byte with the bits [0-5] set if the
// vertex is at the back side of the corresponding clip plane.
// If the 'cullBits' pointer equals LIGHT_CULL_ALL_FRONT all
// vertices are at the front of all the clip planes.
byte* cullBits;

// Clip planes in surface space used to calculate the cull bits.
idPlane localClipPlanes[6];
} srfCullInfo_t;


typedef struct {
    // if lightTris == LIGHT_TRIS_DEFERRED, then the calculation of the
    // lightTris has been deferred, and must be done if ambientTris is visible
srfTriangles_t* lightTris;

// shadow volume triangle surface
srfTriangles_t* shadowTris;

// so we can check ambientViewCount before adding lightTris, and get
// at the shared vertex and possibly shadowVertex caches
srfTriangles_t* ambientTris;

const idMaterial* shader;

int expCulled;          // only for the experimental shadow buffer renderer

srfCullInfo_t cullInfo;
} surfaceInteraction_t;


typedef struct areaNumRef_s
{
    struct areaNumRef_s *	next;
	int areaNum;
}
areaNumRef_t;


class idRenderEntityLocal;
class idRenderLightLocal;

class idInteraction
{
    public:
	// this may be 0 if the light and entity do not actually intersect
	// -1 = an untested interaction
	int numSurfaces;

    // if there is a whole-entity optimized shadow hull, it will
    // be present as a surfaceInteraction_t with a NULL ambientTris, but
    // possibly having a shader to specify the shadow sorting order
    surfaceInteraction_t* surfaces;

    // get space from here, if NULL, it is a pre-generated shadow volume from dmap
    idRenderEntityLocal* entityDef;
    idRenderLightLocal* lightDef;

    idInteraction* lightNext;               // for lightDef chains
    idInteraction* lightPrev;
    idInteraction* entityNext;              // for entityDef chains
    idInteraction* entityPrev;

    public:
	idInteraction(void );

    // because these are generated and freed each game tic for active elements all
    // over the world, we use a custom pool allocater to avoid memory allocation overhead
    // and fragmentation
    static idInteraction* AllocAndLink(idRenderEntityLocal* edef, idRenderLightLocal* ldef);

    // unlinks from the entity and light, frees all surfaceInteractions,
    // and puts it back on the free list
    void UnlinkAndFree(void );

    // free the interaction surfaces
    void FreeSurfaces(void );

    // makes the interaction empty for when the light and entity do not actually intersect
    // all empty interactions are linked at the end of the light's and entity's interaction list
    void MakeEmpty(void );

    // returns true if the interaction is empty
    bool IsEmpty(void ) const {
		return (numSurfaces == 0 );
	}

// returns true if the interaction is not yet completely created
bool IsDeferred(void ) const {
		return ( numSurfaces == -1 );
	}

	// returns true if the interaction has shadows
	bool HasShadows(void ) const;

// counts up the memory used by all the surfaceInteractions, which
// will be used to determine when we need to start purging old interactions
int MemoryUsed(void );

// makes sure all necessary light surfaces and shadow surfaces are created, and
// calls R_LinkLightSurf() for each one
void AddActiveInteraction(void );

private:
	enum {
    FRUSTUM_UNINITIALIZED,
    FRUSTUM_INVALID,
    FRUSTUM_VALID,
    FRUSTUM_VALIDAREAS,
}
frustumState;
idFrustum frustum;              // frustum which contains the interaction
areaNumRef_t* frustumAreas;         // numbers of the areas the frustum touches

int dynamicModelFrameCount;	// so we can tell if a callback model animated

private:
	// actually create the interaction
	void CreateInteraction( const idRenderModel* model);

// unlink from entity and light lists
void Unlink(void );

// try to determine if the entire interaction, including shadows, is guaranteed
// to be outside the view frustum
bool CullInteractionByViewFrustum( const idFrustum &viewFrustum );

// determine the minimum scissor rect that will include the interaction shadows
// projected to the bounds of the light
idScreenRect CalcInteractionScissorRectangle( const idFrustum &viewFrustum );
};


void R_CalcInteractionFacing( const idRenderEntityLocal* ent, const srfTriangles_t* tri, const idRenderLightLocal* light, srfCullInfo_t &cullInfo );
void R_CalcInteractionCullBits( const idRenderEntityLocal* ent, const srfTriangles_t* tri, const idRenderLightLocal* light, srfCullInfo_t &cullInfo );
void R_FreeInteractionCullInfo(srfCullInfo_t &cullInfo );

void R_ShowInteractionMemory_f( const idCmdArgs &args );