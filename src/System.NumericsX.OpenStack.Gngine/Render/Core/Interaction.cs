using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public partial class Interaction
    {
        internal static readonly SrfTriangles LIGHT_TRIS_DEFERRED = new();
        internal static readonly byte[] LIGHT_CULL_ALL_FRONT = Array.Empty<byte>();
        const float LIGHT_CLIP_EPSILON = 0.1f;
    }

    public unsafe struct SrfCullInfo
    {
        // For each triangle a byte set to 1 if facing the light origin.
        public byte* facing;
        // For each vertex a byte with the bits [0-5] set if the vertex is at the back side of the corresponding clip plane. If the 'cullBits' pointer equals LIGHT_CULL_ALL_FRONT all vertices are at the front of all the clip planes.
        public byte[] cullBits;
        // Clip planes in surface space used to calculate the cull bits.
        public Plane[] localClipPlanes;
    }

    public class SurfaceInteraction
    {
        // if lightTris == LIGHT_TRIS_DEFERRED, then the calculation of the lightTris has been deferred, and must be done if ambientTris is visible
        public SrfTriangles lightTris;
        // shadow volume triangle surface
        public SrfTriangles shadowTris;
        // so we can check ambientViewCount before adding lightTris, and get at the shared vertex and possibly shadowVertex caches
        public SrfTriangles ambientTris;
        public Material shader;
        public int expCulled;          // only for the experimental shadow buffer renderer
        public SrfCullInfo cullInfo;
    }

    class AreaNumRef
    {
        public AreaNumRef next;
        public int areaNum;
    }

    partial class Interaction
    {
        // this may be 0 if the light and entity do not actually intersect -1 = an untested interaction
        public int numSurfaces;

        // if there is a whole-entity optimized shadow hull, it will
        // be present as a surfaceInteraction_t with a null ambientTris, but
        // possibly having a shader to specify the shadow sorting order
        public SurfaceInteraction[] surfaces;

        // get space from here, if null, it is a pre-generated shadow volume from dmap
        public IRenderEntity entityDef;
        public IRenderLight lightDef;

        public Interaction lightNext;               // for lightDef chains
        public Interaction lightPrev;
        public Interaction entityNext;              // for entityDef chains
        public Interaction entityPrev;

        public Interaction()
        {
            numSurfaces = 0;
            surfaces = null;
            entityDef = null;
            lightDef = null;
            lightNext = null;
            lightPrev = null;
            entityNext = null;
            entityPrev = null;
            dynamicModelFrameCount = 0;
            frustumState = FrustumState.FRUSTUM_UNINITIALIZED;
            frustumAreas = null;
        }

        // because these are generated and freed each game tic for active elements all over the world, we use a custom pool allocater to avoid memory allocation overhead and fragmentation
        public static Interaction AllocAndLink(IRenderEntity edef, IRenderLight ldef)
        {
            if (edef == null || ldef == null) common.Error("Interaction::AllocAndLink: null parm");

            var renderWorld = edef.world;

            var interaction = renderWorld.interactionAllocator.Alloc();

            // link and initialize
            interaction.dynamicModelFrameCount = 0;

            interaction.lightDef = ldef;
            interaction.entityDef = edef;

            interaction.numSurfaces = -1;      // not checked yet
            interaction.surfaces = null;

            interaction.frustumState = FrustumState.FRUSTUM_UNINITIALIZED;
            interaction.frustumAreas = null;

            // link at the start of the entity's list
            interaction.lightNext = ldef.firstInteraction;
            interaction.lightPrev = null;
            ldef.firstInteraction = interaction;
            if (interaction.lightNext != null) interaction.lightNext.lightPrev = interaction;
            else ldef.lastInteraction = interaction;

            // link at the start of the light's list
            interaction.entityNext = edef.firstInteraction;
            interaction.entityPrev = null;
            edef.firstInteraction = interaction;
            if (interaction.entityNext != null) interaction.entityNext.entityPrev = interaction;
            else edef.lastInteraction = interaction;

            // update the interaction table
            if (renderWorld.interactionTable)
            {
                var index = ldef.index * renderWorld.interactionTableWidth + edef.index;
                if (renderWorld.interactionTable[index] != null) common.Error("Interaction::AllocAndLink: non null table entry");
                renderWorld.interactionTable[index] = interaction;
            }

            return interaction;
        }

        // unlinks from the entity and light, frees all surfaceInteractions, and puts it back on the free list
        // Removes links and puts it back on the free list.
        public void UnlinkAndFree()
        {
            // clear the table pointer
            var renderWorld = this.lightDef.world;
            if (renderWorld.interactionTable)
            {
                var index = this.lightDef.index * renderWorld.interactionTableWidth + this.entityDef.index;
                if (renderWorld.interactionTable[index] != this) common.Error("Interaction::UnlinkAndFree: interactionTable wasn't set");
                renderWorld.interactionTable[index] = null;
            }

            Unlink();

            FreeSurfaces();

            // free the interaction area references
            AreaNumRef area, nextArea;
            for (area = frustumAreas; area != null; area = nextArea) { nextArea = area.next; renderWorld.areaNumRefAllocator.Free(area); }

            // put it back on the free list
            renderWorld.interactionAllocator.Free(this);
        }

        // free the interaction surfaces
        // Frees the surfaces, but leaves the interaction linked in, so it will be regenerated automatically
        public void FreeSurfaces()
        {
            if (this.surfaces != null)
            {
                for (var i = 0; i < this.numSurfaces; i++)
                {
                    var sint = this.surfaces[i];
                    if (sint.lightTris != null)
                    {
                        if (sint.lightTris != LIGHT_TRIS_DEFERRED) R_FreeStaticTriSurf(sint.lightTris);
                        sint.lightTris = null;
                    }
                    if (sint.shadowTris != null)
                    {
                        // if it doesn't have an entityDef, it is part of a prelight model, not a generated interaction
                        if (this.entityDef != null) { R_FreeStaticTriSurf(sint.shadowTris); sint.shadowTris = null; }
                    }
                    R_FreeInteractionCullInfo(sint.cullInfo);
                }

                R_StaticFree(this.surfaces);
                this.surfaces = null;
            }
            this.numSurfaces = -1;
        }

        // makes the interaction empty for when the light and entity do not actually intersect all empty interactions are linked at the end of the light's and entity's interaction list
        // Makes the interaction empty and links it at the end of the entity's and light's interaction lists.
        public void MakeEmpty()
        {
            // an empty interaction has no surfaces
            numSurfaces = 0;

            Unlink();

            // relink at the end of the entity's list
            this.entityNext = null;
            this.entityPrev = this.entityDef.lastInteraction;
            this.entityDef.lastInteraction = this;
            if (this.entityPrev != null) this.entityPrev.entityNext = this;
            else this.entityDef.firstInteraction = this;

            // relink at the end of the light's list
            this.lightNext = null;
            this.lightPrev = this.lightDef.lastInteraction;
            this.lightDef.lastInteraction = this;
            if (this.lightPrev != null) this.lightPrev.lightNext = this;
            else this.lightDef.firstInteraction = this;
        }

        // returns true if the interaction is empty
        public bool IsEmpty
            => numSurfaces == 0;

        // returns true if the interaction is not yet completely created
        public bool IsDeferred
            => numSurfaces == -1;

        // returns true if the interaction has shadows
        public bool HasShadows
            => !lightDef.parms.noShadows && !entityDef.parms.noShadow && lightDef.lightShader.LightCastsShadows;

        // counts up the memory used by all the surfaceInteractions, which will be used to determine when we need to start purging old interactions
        // Counts up the memory used by all the surfaceInteractions, which will be used to determine when we need to start purging old interactions.
        public int MemoryUsed
        {
            get
            {
                var total = 0;
                for (var i = 0; i < numSurfaces; i++)
                {
                    var inter = surfaces[i];
                    total += R_TriSurfMemory(inter.lightTris);
                    total += R_TriSurfMemory(inter.shadowTris);
                }
                return total;
            }
        }

        // If we know that we are "off to the side" of an infinite shadow volume, we can draw it without caps in zpass mode
        static bool R_PotentiallyInsideInfiniteShadow(SrfTriangles occluder, in Vector3 localView, in Vector3 localLight)
        {
            Bounds exp;

            // expand the bounds to account for the near clip plane, because the view could be mathematically outside, but if the near clip plane chops a volume edge, the zpass rendering would fail.
            float znear = R.r_znear.Float;
            if (tr.viewDef.renderView.cramZNear) znear *= 0.25f;
            float stretch = znear * 2;  // in theory, should vary with FOV
            exp[0][0] = occluder.bounds[0][0] - stretch;
            exp[0][1] = occluder.bounds[0][1] - stretch;
            exp[0][2] = occluder.bounds[0][2] - stretch;
            exp[1][0] = occluder.bounds[1][0] + stretch;
            exp[1][1] = occluder.bounds[1][1] + stretch;
            exp[1][2] = occluder.bounds[1][2] + stretch;

            if (exp.ContainsPoint(localView)) return true;
            if (exp.ContainsPoint(localLight)) return true;

            // if the ray from localLight to localView intersects a face of the expanded bounds, we will be inside the projection
            var ray = localView - localLight;

            // intersect the ray from the view to the light with the near side of the bounds
            for (var axis = 0; axis < 3; axis++)
            {
                float d, frac; Vector3 hit;

                if (localLight[axis] < exp[0][axis])
                {
                    if (localView[axis] < exp[0][axis]) continue;
                    d = exp[0][axis] - localLight[axis];
                    frac = d / ray[axis];
                    hit = localLight + frac * ray;
                    hit[axis] = exp[0][axis];
                }
                else if (localLight[axis] > exp[1][axis])
                {
                    if (localView[axis] > exp[1][axis]) continue;
                    d = exp[1][axis] - localLight[axis];
                    frac = d / ray[axis];
                    hit = localLight + frac * ray;
                    hit[axis] = exp[1][axis];
                }
                else continue;

                if (exp.ContainsPoint(hit)) return true;
            }

            // the view is definitely not inside the projected shadow
            return false;
        }


        // makes sure all necessary light surfaces and shadow surfaces are created, and calls R_LinkLightSurf() for each one
        // If the model doesn't have any surfaces that need interactions with this type of light, it can be skipped, but we might need to instantiate the dynamic model to find out
        public void AddActiveInteraction()
        {
            ViewLight vLight;
            ViewEntity vEntity;
            ScreenRect shadowScissor, lightScissor;
            Vector3 localLightOrigin, localViewOrigin;

            vLight = lightDef.viewLight;
            vEntity = entityDef.viewEntity;

            // do not waste time culling the interaction frustum if there will be no shadows
            // use the entity scissor rectangle
            if (!HasShadows) shadowScissor = vEntity.scissorRect;  // culling does not seem to be worth it for static world models
                                                                   // use the light scissor rectangle
            else if (entityDef.parms.hModel.IsStaticWorldModel) shadowScissor = vLight.scissorRect;
            else
            {
                // try to cull the interaction this will also cull the case where the light origin is inside the view frustum and the entity bounds are outside the view frustum
                if (CullInteractionByViewFrustum(tr.viewDef.viewFrustum)) return;
                // calculate the shadow scissor rectangle
                shadowScissor = CalcInteractionScissorRectangle(tr.viewDef.viewFrustum);
            }

            // get out before making the dynamic model if the shadow scissor rectangle is empty
            if (shadowScissor.IsEmpty) return;

            // We will need the dynamic surface created to make interactions, even if the model itself wasn't visible.  This just returns a cached value after it has been generated once in the view.
            var model = R_EntityDefDynamicModel(entityDef);
            if (model == null || model.NumSurfaces <= 0) return;

            // the dynamic model may have changed since we built the surface list
            if (!IsDeferred && entityDef.dynamicModelFrameCount != dynamicModelFrameCount) FreeSurfaces();
            dynamicModelFrameCount = entityDef.dynamicModelFrameCount;

            // actually create the interaction if needed, building light and shadow surfaces as needed
            if (IsDeferred) CreateInteraction(model);

            R_GlobalPointToLocal(vEntity.modelMatrix, lightDef.globalLightOrigin, out localLightOrigin);
            R_GlobalPointToLocal(vEntity.modelMatrix, tr.viewDef.renderView.vieworg, out localViewOrigin);

            // calculate the scissor as the intersection of the light and model rects
            // this is used for light triangles, but not for shadow triangles
            lightScissor = vLight.scissorRect;
            lightScissor.Intersect(vEntity.scissorRect);

            var lightScissorsEmpty = lightScissor.IsEmpty;

            // for each surface of this entity / light interaction
            for (var i = 0; i < numSurfaces; i++)
            {
                var sint = surfaces[i];

                // see if the base surface is visible, we may still need to add shadows even if empty
                if (!lightScissorsEmpty && sint.ambientTris != null && sint.ambientTris.ambientViewCount == tr.viewCount)
                {
                    // make sure we have created this interaction, which may have been deferred on a previous use that only needed the shadow
                    if (sint.lightTris == LIGHT_TRIS_DEFERRED)
                    {
                        sint.lightTris = R_CreateLightTris(vEntity.entityDef, sint.ambientTris, vLight.lightDef, sint.shader, sint.cullInfo);
                        R_FreeInteractionCullInfo(sint.cullInfo);
                    }

                    var lightTris = sint.lightTris;
                    if (lightTris != null)
                    {
                        // try to cull before adding FIXME: this may not be worthwhile. We have already done culling on the ambient, but individual surfaces may still be cropped somewhat more
                        if (!R_CullLocalBox(lightTris.bounds, vEntity.modelMatrix, 5, tr.viewDef.frustum))
                        {
                            // make sure the original surface has its ambient cache created
                            if (!R_CreateAmbientCache(sint.ambientTris, sint.shader.ReceivesLighting)) continue; // skip if we were out of vertex memory
                                                                                                                 // reference the original surface's ambient cache
                                                                                                                 // GAB NOTE: we are in cache "reuse" mode
                            lightTris.ambientCache = sint.ambientTris.ambientCache;

                            // Even if we reuse the original surface ambient cache, we nevertheless need to compute a local index cache
                            if (!R_CreateIndexCache(lightTris)) continue; // skip if we were out of vertex memory

                            // touch the ambient surface so it won't get purged
                            vertexCache.Touch(lightTris.ambientCache);
                            vertexCache.Touch(lightTris.indexCache);

                            // add the surface to the light list

                            var shader = sint.shader;
                            R_GlobalShaderOverride(shader);

                            // there will only be localSurfaces if the light casts shadows and there are surfaces with NOSELFSHADOW
                            if (sint.shader.Coverage == MC.TRANSLUCENT) R_LinkLightSurf(ref vLight.translucentInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                            else if (!lightDef.parms.noShadows && sint.shader.TestMaterialFlag(MF.NOSELFSHADOW)) R_LinkLightSurf(ref vLight.localInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                            else R_LinkLightSurf(ref vLight.globalInteractions, lightTris, vEntity, lightDef, shader, lightScissor, false);
                        }
                    }
                }

                var shadowTris = sint.shadowTris;
                // the shadows will always have to be added, unless we can tell they are from a surface in an unconnected area
                if (shadowTris != null)
                {
                    // check for view specific shadow suppression (player shadows, etc)
                    if (!r_skipSuppress.Bool)
                    {
                        if (entityDef.parms.suppressShadowInViewID != 0 && entityDef.parms.suppressShadowInViewID == tr.viewDef.renderView.viewID) continue;
                        if (entityDef.parms.suppressShadowInLightID != 0 && entityDef.parms.suppressShadowInLightID == lightDef.parms.lightId) continue;
                    }

                    // cull static shadows that have a non-empty bounds. dynamic shadows that use the turboshadow code will not have valid bounds, because the perspective projection extends them to infinity
                    if (r_useShadowCulling.Bool && !shadowTris.bounds.IsCleared && R_CullLocalBox(shadowTris.bounds, vEntity.modelMatrix, 5, tr.viewDef.frustum)) continue;

                    // If the tri have shadowVertexes (eg. precomputed shadows)
                    if (shadowTris.shadowVertexes != null)
                    {
                        // Create its shadow cache
                        if (!R_CreatePrivateShadowCache(shadowTris)) continue; // skip if we were out of vertex memory
                                                                               // And its index cache
                        if (!R_CreateIndexCache(shadowTris)) continue; // skip if we were out of vertex memory
                    }
                    // Otherwise this is dynamic shadows
                    else
                    {
                        // Make sure the original surface has its shadow cache created
                        if (!R_CreateVertexProgramShadowCache(sint.ambientTris)) continue; // skip if we were out of vertex memory
                                                                                           // reference the original surface's shadow cache. GAB NOTE: we are in cache "reuse" mode
                        shadowTris.shadowCache = sint.ambientTris.shadowCache;

                        // Even if we reuse the original surface shadow cache, we nevertheless need to compute a local index cache
                        if (!R_CreateIndexCache(shadowTris)) continue; // skip if we were out of vertex memory
                    }

                    // In the end, touch the shadow surface so it won't get purged
                    vertexCache.Touch(shadowTris.shadowCache);
                    vertexCache.Touch(shadowTris.indexCache);

                    // see if we can avoid using the shadow volume caps
                    var inside = R_PotentiallyInsideInfiniteShadow(sint.ambientTris, localViewOrigin, localLightOrigin);

                    if (sint.shader.TestMaterialFlag(MF.NOSELFSHADOW)) R_LinkLightSurf(ref vLight.localShadows, shadowTris, vEntity, lightDef, null, shadowScissor, inside);
                    else R_LinkLightSurf(ref vLight.globalShadows, shadowTris, vEntity, lightDef, null, shadowScissor, inside);
                }
            }
        }

        enum FrustumState
        {
            FRUSTUM_UNINITIALIZED,
            FRUSTUM_INVALID,
            FRUSTUM_VALID,
            FRUSTUM_VALIDAREAS,
        }
        FrustumState frustumState;
        Frustum frustum;              // frustum which contains the interaction
        AreaNumRef frustumAreas;         // numbers of the areas the frustum touches

        int dynamicModelFrameCount; // so we can tell if a callback model animated

        // actually create the interaction
        // Called when a entityDef and a lightDef are both present in a portalArea, and might be visible.Performs cull checking before doing the expensive computations.
        // References tr.viewCount so lighting surfaces will only be created if the ambient surface is visible, otherwise it will be marked as deferred.
        // The results of this are cached and valid until the light or entity change.
        void CreateInteraction(RenderModel model)
        {
            Material lightShader = lightDef.lightShader;
            Material shader;
            bool interactionGenerated;
            Bounds bounds;

            tr.pc.c_createInteractions++;

            bounds = model.Bounds(&entityDef.parms);

            // if it doesn't contact the light frustum, none of the surfaces will
            if (R_CullLocalBox(bounds, entityDef.modelMatrix, 6, lightDef.frustum)) { MakeEmpty(); return; }

            // use the turbo shadow path
            ShadowGen shadowGen = SG_DYNAMIC;

            // really large models, like outside terrain meshes, should use the more exactly culled static shadow path instead of the turbo shadow path. FIXME: this is a HACK, we should probably have a material flag.
            if (bounds[1].x - bounds[0].x > 3000) shadowGen = SG_STATIC;

            // create slots for each of the model's surfaces
            numSurfaces = model.NumSurfaces();
            surfaces = R_ClearedStaticAllocMany<SurfaceInteraction>(numSurfaces);

            interactionGenerated = false;

            // check each surface in the model
            for (var c = 0; c < model.NumSurfaces; c++)
            {
                ModelSurface surf;
                SrfTriangles tri;

                surf = model.Surface(c);

                tri = surf.geometry;
                if (tri == null) continue;

                // determine the shader for this surface, possibly by skinning
                shader = surf.shader;
                shader = R_RemapShaderBySkin(shader, entityDef.parms.customSkin, entityDef.parms.customShader);
                if (shader == null) continue;

                // try to cull each surface
                if (R_CullLocalBox(tri.bounds, entityDef.modelMatrix, 6, lightDef.frustum)) continue;

                var sint = surfaces[c];
                sint.shader = shader;

                // save the ambient tri pointer so we can reject lightTri interactions when the ambient surface isn't in view, and we can get shared vertex and shadow data from the source surface
                sint.ambientTris = tri;

                // "invisible ink" lights and shaders
                if (shader.Spectrum != lightShader.Spectrum) continue;

                // generate a lighted surface and add it
                if (shader.ReceivesLighting)
                {
                    sint.lightTris = tri.ambientViewCount == tr.viewCount
                        // this will be calculated when sint.ambientTris is actually in view
                        ? R_CreateLightTris(entityDef, tri, lightDef, shader, sint.cullInfo)
                        : LIGHT_TRIS_DEFERRED;
                    interactionGenerated = true;
                }

                // if the interaction has shadows and this surface casts a shadow
                if (HasShadows && shader.SurfaceCastsShadow && tri.silEdges != null)
                {
                    // if the light has an optimized shadow volume, don't create shadows for any models that are part of the base areas
                    if (lightDef.parms.prelightModel == null || !model.IsStaticWorldModel() || !r_useOptimizedShadows.Bool)
                    {
                        // this is the only place during gameplay (outside the utilities) that R_CreateShadowVolume() is called
                        sint.shadowTris = R_CreateShadowVolume(entityDef, tri, lightDef, shadowGen, sint.cullInfo);
                        if (sint.shadowTris != null)
                            if (shader.Coverage != MC.OPAQUE || (!r_skipSuppress.Bool && entityDef.parms.suppressSurfaceInViewID))
                            {
                                // if any surface is a shadow-casting perforated or translucent surface, or the base surface is suppressed in the view (world weapon shadows) we can't use
                                // the external shadow optimizations because we can see through some of the faces
                                sint.shadowTris.numShadowIndexesNoCaps = sint.shadowTris.numIndexes;
                                sint.shadowTris.numShadowIndexesNoFrontCaps = sint.shadowTris.numIndexes;
                            }
                        interactionGenerated = true;
                    }
                }

                // free the cull information when it's no longer needed
                if (sint.lightTris != LIGHT_TRIS_DEFERRED) R_FreeInteractionCullInfo(sint.cullInfo);
            }

            // if none of the surfaces generated anything, don't even bother checking?
            if (!interactionGenerated) MakeEmpty();
        }

        // unlink from entity and light lists
        void Unlink()
        {
            // unlink from the entity's list
            if (this.entityPrev != null) this.entityPrev.entityNext = this.entityNext;
            else this.entityDef.firstInteraction = this.entityNext;
            if (this.entityNext != null) this.entityNext.entityPrev = this.entityPrev;
            else this.entityDef.lastInteraction = this.entityPrev;
            this.entityNext = this.entityPrev = null;

            // unlink from the light's list
            if (this.lightPrev != null) this.lightPrev.lightNext = this.lightNext;
            else this.lightDef.firstInteraction = this.lightNext;
            if (this.lightNext != null) this.lightNext.lightPrev = this.lightPrev;
            else this.lightDef.lastInteraction = this.lightPrev;
            this.lightNext = this.lightPrev = null;
        }

        // try to determine if the entire interaction, including shadows, is guaranteed to be outside the view frustum
        static Vector4[] CullInteractionByViewFrustum_colors = { colorRed, colorGreen, colorBlue, colorYellow, colorMagenta, colorCyan, colorWhite, colorPurple };
        bool CullInteractionByViewFrustum(Frustum viewFrustum)
        {
            if (!r_useInteractionCulling.Bool) return false;
            if (frustumState == FrustumState.FRUSTUM_INVALID) return false;
            if (frustumState == FrustumState.FRUSTUM_UNINITIALIZED)
            {
                frustum.FromProjection(new Box(entityDef.referenceBounds, entityDef.parms.origin, entityDef.parms.axis), lightDef.globalLightOrigin, MAX_WORLD_SIZE);
                if (!frustum.IsValid) { frustumState = FrustumState.FRUSTUM_INVALID; return false; }
                frustum.ConstrainToBox(lightDef.parms.pointLight
                    ? new Box(lightDef.parms.origin, lightDef.parms.lightRadius, lightDef.parms.axis)
                    : new Box(lightDef.frustumTris.bounds));
                frustumState = FrustumState.FRUSTUM_VALID;
            }
            if (!viewFrustum.IntersectsFrustum(frustum)) return true;
            if (r_showInteractionFrustums.Integer != 0)
            {
                tr.viewDef.renderWorld.DebugFrustum(CullInteractionByViewFrustum_colors[lightDef.index & 7], frustum, r_showInteractionFrustums.Integer > 1);
                if (r_showInteractionFrustums.Integer > 2) tr.viewDef.renderWorld.DebugBox(colorWhite, new Box(entityDef.referenceBounds, entityDef.parms.origin, entityDef.parms.axis));
            }
            return false;
        }

        // determine the minimum scissor rect that will include the interaction shadows projected to the bounds of the light
        ScreenRect CalcInteractionScissorRectangle(Frustum viewFrustum)
        {
            Bounds projectionBounds = default; ScreenRect portalRect = default, scissorRect;

            if (r_useInteractionScissors.Integer == 0) return lightDef.viewLight.scissorRect;

            // this is the code from Cass at nvidia, it is more precise, but slower
            if (r_useInteractionScissors.Integer < 0) return R_CalcIntersectionScissor(lightDef, entityDef, tr.viewDef);

            // frustum must be initialized and valid
            if (frustumState == FrustumState.FRUSTUM_UNINITIALIZED || frustumState == FrustumState.FRUSTUM_INVALID) return lightDef.viewLight.scissorRect;

            // calculate scissors for the portals through which the interaction is visible
            if (r_useInteractionScissors.Integer > 1)
            {
                AreaNumRef area;
                if (frustumState == FrustumState.FRUSTUM_VALID)
                {
                    // retrieve all the areas the interaction frustum touches
                    for (var r = entityDef.entityRefs; r != null; r = r.ownerNext)
                    {
                        area = entityDef.world.areaNumRefAllocator.Alloc();
                        area.areaNum = r.area.areaNum;
                        area.next = frustumAreas;
                        frustumAreas = area;
                    }
                    frustumAreas = tr.viewDef.renderWorld.FloodFrustumAreas(frustum, frustumAreas);
                    frustumState = FrustumState.FRUSTUM_VALIDAREAS;
                }

                portalRect.Clear();
                for (area = frustumAreas; area != null; area = area.next) portalRect.Union(entityDef.world.GetAreaScreenRect(area.areaNum));
                portalRect.Intersect(lightDef.viewLight.scissorRect);
            }
            else portalRect = lightDef.viewLight.scissorRect;

            // early out if the interaction is not visible through any portals
            if (portalRect.IsEmpty) return portalRect;

            // calculate bounds of the interaction frustum projected into the view frustum
            viewFrustum.ClippedProjectionBounds(frustum, lightDef.parms.pointLight
                ? new Box(lightDef.parms.origin, lightDef.parms.lightRadius, lightDef.parms.axis)
                : new Box(lightDef.frustumTris.bounds), projectionBounds);

            if (projectionBounds.IsCleared) return portalRect;

            // derive a scissor rectangle from the projection bounds
            scissorRect = R_ScreenRectFromViewFrustumBounds(projectionBounds);

            // intersect with the portal crossing scissor rectangle
            scissorRect.Intersect(portalRect);

            if (r_showInteractionScissors.Integer > 0) R_ShowColoredScreenRect(scissorRect, lightDef.index);

            return scissorRect;
        }
    }

    unsafe partial class TR
    {
        // Determines which triangles of the surface are facing towards the light origin.
        // The facing array should be allocated with one extra index than the number of surface triangles, which will be used to handle dangling edge silhouettes.
        static void R_CalcInteractionFacing(IRenderEntity ent, SrfTriangles tri, IRenderLight light, ref SrfCullInfo cullInfo)
        {
            if (cullInfo.facing != null) return;

            R_GlobalPointToLocal(ent.modelMatrix, light.globalLightOrigin, out var localLightOrigin);

            var numFaces = tri.numIndexes / 3;

            if (tri.facePlanes == null || !tri.facePlanesCalculated) R_DeriveFacePlanes(tri);

            cullInfo.facing = (byte*)R_StaticAlloc((numFaces + 1) * sizeof(byte));

            // calculate back face culling
            var planeSide = stackalloc float[numFaces + floatX.ALLOC16]; planeSide = (float*)_alloca16(planeSide);

            // exact geometric cull against face
            Simd.Dotcp(planeSide, localLightOrigin, tri.facePlanes, numFaces);
            Simd.CmpGE(cullInfo.facing, planeSide, 0.0f, numFaces);

            cullInfo.facing[numFaces] = 1;  // for dangling edges to reference
        }

        // We want to cull a little on the sloppy side, because the pre-clipping of geometry to the lights in dmap will give many cases that are right
        // at the border we throw things out on the border, because if any one vertex is clearly inside, the entire triangle will be accepted.
        static void R_CalcInteractionCullBits(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, ref SrfCullInfo cullInfo)
        {
            int i, frontBits;

            if (cullInfo.cullBits != null) return;

            frontBits = 0;

            // cull the triangle surface bounding box
            for (i = 0; i < 6; i++)
            {
                R_GlobalPlaneToLocal(ent.modelMatrix, -light.frustum[i], cullInfo.localClipPlanes[i]);

                // get front bits for the whole surface
                if (tri.bounds.PlaneDistance(cullInfo.localClipPlanes[i]) >= LIGHT_CLIP_EPSILON) frontBits |= 1 << i;
            }

            // if the surface is completely inside the light frustum
            if (frontBits == ((1 << 6) - 1)) { cullInfo.cullBits = LIGHT_CULL_ALL_FRONT; return; }

            cullInfo.cullBits = (byte*)R_StaticAlloc(tri.numVerts * sizeof(byte));
            Simd.Memset(cullInfo.cullBits, 0, tri.numVerts * sizeof(byte);

            var planeSide = stackalloc float[tri.numVerts + floatX.ALLOC16]; planeSide = (float*)_alloca16(planeSide);

            for (i = 0; i < 6; i++)
            {
                // if completely infront of this clipping plane
                if ((frontBits & (1 << i)) != 0) continue;
                Simd.Dotpd(planeSide, cullInfo.localClipPlanes[i], tri.verts, tri.numVerts);
                Simd.CmpLT(cullInfo.cullBits, i, planeSide, LIGHT_CLIP_EPSILON, tri.numVerts);
            }
        }

        static void R_FreeInteractionCullInfo(SrfCullInfo cullInfo)
        {
            if (cullInfo.facing != null)
            {
                R_StaticFree(cullInfo.facing);
                cullInfo.facing = null;
            }
            if (cullInfo.cullBits != null)
            {
                if (cullInfo.cullBits != LIGHT_CULL_ALL_FRONT) R_StaticFree(cullInfo.cullBits);
                cullInfo.cullBits = null;
            }
        }

        struct ClipTri
        {
            const int MAX_CLIPPED_POINTS = 20;
            public int numVerts;
            public Vector3 verts = new Vector3[MAX_CLIPPED_POINTS];
        }

        // Clips a triangle from one buffer to another, setting edge flags The returned buffer may be the same as inNum if no clipping is done If entirely clipped away, clipTris[returned].numVerts == 0
        // I have some worries about edge flag cases when polygons are clipped multiple times near the epsilon.
        static int R_ChopWinding(ClipTri* clipTris, int inNum, in Plane plane)
        {
            ClipTri i, o;
            var dists = stackalloc float[MAX_CLIPPED_POINTS];
            var sides = stackalloc int[MAX_CLIPPED_POINTS];
            var counts = stackalloc int[3];
            float dot;
            int i2, j;
            Vector3 mid;
            bool front;

            i = &clipTris[inNum];
            o = &clipTris[inNum ^ 1];
            counts[0] = counts[1] = counts[2] = 0;

            // determine sides for each point
            front = false;
            for (i2 = 0; i2 < i.numVerts; i2++)
            {
                dot = i.verts[i2] * plane.Normal + plane[3];
                dists[i2] = dot;
                if (dot < LIGHT_CLIP_EPSILON) sides[i2] = SIDE_BACK; // slop onto the back
                else { sides[i2] = SIDE_FRONT; if (dot > LIGHT_CLIP_EPSILON) front = true; }
                counts[sides[i2]]++;
            }

            // if none in front, it is completely clipped away
            if (!front) { i.numVerts = 0; return inNum; }
            if (!counts[SIDE_BACK]) return inNum;       // inout stays the same

            // avoid wrapping checks by duplicating first value to end
            sides[i] = sides[0];
            dists[i] = dists[0];
            i.verts[i.numVerts] = i.verts[0];

            o.numVerts = 0;
            for (i = 0; i < i.numVerts; i++)
            {
                ref Vector3 p1 = ref i.verts[i];
                if (sides[i] == SIDE_FRONT) { o.verts[o.numVerts] = p1; o.numVerts++; }
                if (sides[i + 1] == sides[i]) continue;

                // generate a split point
                ref Vector3 p2 = ref i.verts[i + 1];

                dot = dists[i] / (dists[i] - dists[i + 1]);
                for (j = 0; j < 3; j++) mid[j] = p1[j] + dot * (p2[j] - p1[j]);

                o.verts[o.numVerts] = mid;

                o.numVerts++;
            }

            return inNum ^ 1;
        }

        // Returns false if nothing is left after clipping
        static bool R_ClipTriangleToLight(in Vector3 a, in Vector3 b, in Vector3 c, int planeBits, Plane[] frustum)
        {
            int i, p; var pingPong = stackalloc ClipTri[2];

            pingPong[0].numVerts = 3;
            pingPong[0].verts[0] = a;
            pingPong[0].verts[1] = b;
            pingPong[0].verts[2] = c;

            p = 0;
            for (i = 0; i < 6; i++)
                if ((planeBits & (1 << i)) != 0)
                {
                    p = R_ChopWinding(pingPong, p, frustum[i]);
                    if (pingPong[p].numVerts < 1) return false;
                }

            return true;
        }

        // The resulting surface will be a subset of the original triangles, it will never clip triangles, but it may cull on a per-triangle basis.
        static SrfTriangles R_CreateLightTris(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, Material shader, in SrfCullInfo cullInfo)
        {
            int i, numIndexes;
            GlIndex[] indexes;
            SrfTriangles newTri;
            int c_backfaced, c_distance;
            Bounds bounds;
            bool includeBackFaces, faceNum;

            tr.pc.c_createLightTris++;
            c_backfaced = 0;
            c_distance = 0;

            numIndexes = 0;
            indexes = null;

            // it is debatable if non-shadowing lights should light back faces. we aren't at the moment
            includeBackFaces = r_lightAllBackFaces.Bool || light.lightShader.LightEffectsBackSides || shader.ReceivesLightingOnBackSides || ent.parms.noSelfShadow || ent.parms.noShadow;

            // allocate a new surface for the lit triangles
            newTri = R_AllocStaticTriSurf();

            // save a reference to the original surface
            newTri.ambientSurface = tri;

            // the light surface references the verts of the ambient surface
            newTri.numVerts = tri.numVerts;
            R_ReferenceStaticTriSurfVerts(newTri, tri);

            // calculate cull information
            if (!includeBackFaces) R_CalcInteractionFacing(ent, tri, light, cullInfo);
            R_CalcInteractionCullBits(ent, tri, light, cullInfo);

            // if the surface is completely inside the light frustum
            if (cullInfo.cullBits == Interaction.LIGHT_CULL_ALL_FRONT)
            {
                // if we aren't self shadowing, let back facing triangles get through so the smooth shaded bump maps light all the way around
                if (includeBackFaces)
                {
                    // the whole surface is lit so the light surface just references the indexes of the ambient surface
                    R_ReferenceStaticTriSurfIndexes(newTri, tri);
                    numIndexes = tri.numIndexes;
                    bounds = tri.bounds;
                }
                else
                {
                    // the light tris indexes are going to be a subset of the original indexes so we generally allocate too much memory here but we decrease the memory block when the number of indexes is known
                    R_AllocStaticTriSurfIndexes(newTri, tri.numIndexes);

                    // back face cull the individual triangles
                    indexes = newTri.indexes;
                    var facing = cullInfo.facing;
                    for (faceNum = i = 0; i < tri.numIndexes; i += 3, faceNum++)
                    {
                        if (facing[faceNum] != 0) { c_backfaced++; continue; }
                        indexes[numIndexes + 0] = tri.indexes[i + 0];
                        indexes[numIndexes + 1] = tri.indexes[i + 1];
                        indexes[numIndexes + 2] = tri.indexes[i + 2];
                        numIndexes += 3;
                    }

                    // get bounds for the surface
                    Simd.MinMaxdi(bounds[0], bounds[1], tri.verts, indexes, numIndexes);

                    // decrease the size of the memory block to the size of the number of used indexes
                    R_ResizeStaticTriSurfIndexes(newTri, numIndexes);
                }
            }
            else
            {
                // the light tris indexes are going to be a subset of the original indexes so we generally
                // allocate too much memory here but we decrease the memory block when the number of indexes is known
                R_AllocStaticTriSurfIndexes(newTri, tri.numIndexes);

                // cull individual triangles
                indexes = newTri.indexes;
                var facing = cullInfo.facing;
                var cullBits = cullInfo.cullBits;
                for (faceNum = i = 0; i < tri.numIndexes; i += 3, faceNum++)
                {
                    int i1, i2, i3;

                    // if we aren't self shadowing, let back facing triangles get through so the smooth shaded bump maps light all the way around
                    if (!includeBackFaces)
                        if (facing[faceNum] == 0) { c_backfaced++; continue; } // back face cull

                    i1 = tri.indexes[i + 0];
                    i2 = tri.indexes[i + 1];
                    i3 = tri.indexes[i + 2];

                    // fast cull outside the frustum. if all three points are off one plane side, it definately isn't visible
                    if (cullBits[i1] != 0 & cullBits[i2] != 0 & cullBits[i3] != 0) { c_distance++; continue; }

                    if (r_usePreciseTriangleInteractions.Bool)
                        // do a precise clipped cull if none of the points is completely inside the frustum. note that we do not actually use the clipped triangle, which would have Z fighting issues.
                        if (cullBits[i1] != 0 && cullBits[i2] != 0 && cullBits[i3] != 0)
                        {
                            var cull = cullBits[i1] | cullBits[i2] | cullBits[i3];
                            if (!R_ClipTriangleToLight(tri.verts[i1].xyz, tri.verts[i2].xyz, tri.verts[i3].xyz, cull, cullInfo.localClipPlanes)) continue;
                        }

                    // add to the list
                    indexes[numIndexes + 0] = i1;
                    indexes[numIndexes + 1] = i2;
                    indexes[numIndexes + 2] = i3;
                    numIndexes += 3;
                }

                // get bounds for the surface
                Simd.MinMaxdi(bounds[0], bounds[1], tri.verts, indexes, numIndexes);

                // decrease the size of the memory block to the size of the number of used indexes
                R_ResizeStaticTriSurfIndexes(newTri, numIndexes);
            }

            if (numIndexes == 0) { R_ReallyFreeStaticTriSurf(newTri); return null; }
            newTri.numIndexes = numIndexes;
            newTri.bounds = bounds;
            return newTri;
        }

        static void R_ShowInteractionMemory_f(CmdArgs args)
        {
            int total = 0, entities = 0,
                interactions = 0, deferredInteractions = 0, emptyInteractions = 0,
                lightTris = 0, lightTriVerts = 0, lightTriIndexes = 0,
                shadowTris = 0, shadowTriVerts = 0, shadowTriIndexes = 0;

            for (var i = 0; i < tr.primaryWorld.entityDefs.Num(); i++)
            {
                var def = tr.primaryWorld.entityDefs[i];
                if (def == null) continue;
                if (def.firstInteraction == null) continue;
                entities++;

                for (var inter = def.firstInteraction; inter != null; inter = inter.entityNext)
                {
                    interactions++;
                    total += inter.MemoryUsed();

                    if (inter.IsDeferred) { deferredInteractions++; continue; }
                    if (inter.IsEmpty) { emptyInteractions++; continue; }

                    for (var j = 0; j < inter.numSurfaces; j++)
                    {
                        var srf = inter.surfaces[j];
                        if (srf.lightTris && srf.lightTris != Interaction.LIGHT_TRIS_DEFERRED)
                        {
                            lightTris++;
                            lightTriVerts += srf.lightTris.numVerts;
                            lightTriIndexes += srf.lightTris.numIndexes;
                        }
                        if (srf.shadowTris)
                        {
                            shadowTris++;
                            shadowTriVerts += srf.shadowTris.numVerts;
                            shadowTriIndexes += srf.shadowTris.numIndexes;
                        }
                    }
                }
            }

            common.Printf($"{entities} entities with {interactions} total interactions totalling {total / 1024}k\n");
            common.Printf($"{deferredInteractions} deferred interactions, {emptyInteractions} empty interactions\n");
            common.Printf($"{lightTriIndexes,5} indexes {lightTriVerts,5} verts in {lightTris,5} light tris\n");
            common.Printf($"{shadowTriIndexes,5} indexes {shadowTriVerts,5} verts in {shadowTris,5} shadow tris\n");
        }
    }
}