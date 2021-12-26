#define USE_TRI_DATA_ALLOCATOR
using System.Runtime.InteropServices;
// using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{

    public class RenderLightLocal : IRenderLight
    {
        public RenderLightLocal() => throw new NotImplementedException();

        public void FreeRenderLight() => throw new NotImplementedException();
        public void UpdateRenderLight(RenderLight re, bool forceUpdate = false) => throw new NotImplementedException();
        public void GetRenderLight(RenderLight re) => throw new NotImplementedException();
        public void ForceUpdate() => throw new NotImplementedException();
        public int Index => 0;

        //public RenderLight parms;                    // specification
        //public bool lightHasMoved;         // the light has changed its position since it was first added, so the prelight model is not valid
        //public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin
        //public RenderWorldLocal world;
        //public int index;                  // in world lightdefs
        //public int areaNum;                // if not -1, we may be able to cull all the light's interactions if !viewDef->connectedAreas[areaNum]
        //public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        //public bool archived;              // for demo writing

        //// derived information
        //public Plane[] lightProject = new Plane[4];

        //public Material lightShader;          // guaranteed to be valid, even if parms.shader isn't
        //public Image falloffImage;

        //public Vector3 globalLightOrigin;       // accounting for lightCenter and parallel

        //public Plane[] frustum = new Plane[6];             // in global space, positive side facing out, last two are front/back
        //public Winding[] frustumWindings = new Winding[6];      // used for culling
        //public SrfTriangles frustumTris;            // triangulated frustumWindings[]

        //public int numShadowFrustums;      // one for projected lights, usually six for point lights
        //public ShadowFrustum[] shadowFrustums = new ShadowFrustum[6];

        //public int viewCount;              // if == tr.viewCount, the light is on the viewDef->viewLights list
        //public ViewLight viewLight;

        //public AreaReference references;                // each area the light is present in will have a lightRef
        //public Interaction firstInteraction;        // doubly linked list
        //public Interaction lastInteraction;

        //public DoublePortal foggedPortals;
    }

    public class RenderEntityLocal : IRenderEntity
    {
        public RenderEntityLocal() => throw new NotImplementedException();

        public void FreeRenderEntity() => throw new NotImplementedException();
        public void UpdateRenderEntity(RenderEntity re, bool forceUpdate = false) => throw new NotImplementedException();
        public void GetRenderEntity(RenderEntity re) => throw new NotImplementedException();
        public void ForceUpdate() => throw new NotImplementedException();
        public int Index => 0;

        // overlays are extra polygons that deform with animating models for blood and damage marks
        public void ProjectOverlay(Plane[] localTextureAxis, Material material) => throw new NotImplementedException();
        public void RemoveDecals() => throw new NotImplementedException();

        //public RenderEntity parms;

        //public float[] modelMatrix = new float[16];      // this is just a rearrangement of parms.axis and parms.origin

        //public RenderWorldLocal world;
        //public int index;                  // in world entityDefs

        //public int lastModifiedFrameNum;   // to determine if it is constantly changing, and should go in the dynamic frame memory, or kept in the cached memory
        //public bool archived;              // for demo writing

        //public IRenderModel dynamicModel;            // if parms.model->IsDynamicModel(), this is the generated data
        //public int dynamicModelFrameCount; // continuously animating dynamic models will recreate dynamicModel if this doesn't == tr.viewCount
        //public IRenderModel cachedDynamicModel;

        //public Bounds referenceBounds;       // the local bounds used to place entityRefs, either from parms or a model

        //// a viewEntity_t is created whenever a idRenderEntityLocal is considered for inclusion in a given view, even if it turns out to not be visible
        //public int viewCount;              // if tr.viewCount == viewCount, viewEntity is valid, but the entity may still be off screen
        //public ViewEntity viewEntity;                // in frame temporary memory

        //public int visibleCount;
        //// if tr.viewCount == visibleCount, at least one ambient surface has actually been added by R_AddAmbientDrawsurfs
        //// note that an entity could still be in the view frustum and not be visible due to portal passing

        //public RenderModelDecal decals;                 // chain of decals that have been projected on this model
        //public RenderModelOverlay overlay;              // blood overlays on animated models

        //public AreaReference entityRefs;                // chain of all references
        //public Interaction firstInteraction;        // doubly linked list
        //public Interaction lastInteraction;

        //public bool needsPortalSky;
    }


    //unsafe partial class TR
    //{
    //    public static void R_LockSurfaceScene(ViewDef parms);
    //    public static void R_ClearCommandChain();
    //    public static void R_AddDrawViewCmd(ViewDef parms);

    //    //public static void R_ReloadGuis_f(CmdArgs args);
    //    //public static void R_ListGuis_f(CmdArgs args);

    //    public static void* R_GetCommandBuffer(int bytes);

    //    // this allows a global override of all materials
    //    public static bool R_GlobalShaderOverride(Material shader);

    //    // this does various checks before calling the idDeclSkin
    //    public static Material R_RemapShaderBySkin(Material shader, DeclSkin customSkin, Material customShader);
    //}


    //        #region MAIN

    //        #region PROJECTION_

    //        // This uses the "infinite far z" trick
    //        void R_SetupProjection_()
    //        {
    //            float xmin, xmax, ymin, ymax;
    //            float width, height;
    //            float zNear;
    //            float zFar;
    //            float jitterx, jittery;
    //            static RandomX random = new();

    //            // random jittering is usefull when multiple frames are going to be blended together for motion blurred anti-aliasing
    //            if (r_jitter.Bool) { jitterx = random.RandomFloat(); jittery = random.RandomFloat(); }
    //            else jitterx = jittery = 0;

    //            // set up projection matrix
    //#if Z_HACK
    //	zNear = 8;
    //#else
    //            zNear = r_znear.Float;
    //#endif

    //            if (tr.viewDef.renderView.cramZNear) zNear *= 0.25f;

    //            zFar = 4000;

    //            ymax = (float)(zNear * Math.Tan(tr.viewDef.renderView.fov_y * MathX.PI / 360f));
    //            ymin = -ymax;

    //            xmax = (float)(zNear * Math.Tan(tr.viewDef.renderView.fov_x * MathX.PI / 360f));
    //            xmin = -xmax;

    //            width = xmax - xmin;
    //            height = ymax - ymin;

    //            jitterx = jitterx * width / (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1 + 1);
    //            xmin += jitterx;
    //            xmax += jitterx;
    //            jittery = jittery * height / (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1 + 1);
    //            ymin += jittery;
    //            ymax += jittery;

    //            tr.viewDef.projectionMatrix[0] = 2 * zNear / width;
    //            tr.viewDef.projectionMatrix[4] = 0;
    //            tr.viewDef.projectionMatrix[8] = (xmax + xmin) / width;    // normally 0
    //            tr.viewDef.projectionMatrix[12] = 0;

    //            tr.viewDef.projectionMatrix[1] = 0;
    //            tr.viewDef.projectionMatrix[5] = 2 * zNear / height;
    //            tr.viewDef.projectionMatrix[9] = (ymax + ymin) / height;   // normally 0
    //            tr.viewDef.projectionMatrix[13] = 0;

    //            // this is the far-plane-at-infinity formulation, and crunches the Z range slightly so w=0 vertexes do not rasterize right at the wraparound point
    //            tr.viewDef.projectionMatrix[2] = 0;
    //            tr.viewDef.projectionMatrix[6] = 0;
    //#if Z_HACK
    //	tr.viewDef.projectionMatrix[10] = (-zFar-zNear)/(zFar-zNear);//-0.999f;
    //	tr.viewDef.projectionMatrix[14] = -2f*zFar*zNear/(zFar-zNear);
    //#else
    //            tr.viewDef.projectionMatrix[10] = -0.999f;
    //            tr.viewDef.projectionMatrix[14] = -2f * zNear;
    //#endif

    //            tr.viewDef.projectionMatrix[3] = 0;
    //            tr.viewDef.projectionMatrix[7] = 0;
    //            tr.viewDef.projectionMatrix[11] = -1;
    //            tr.viewDef.projectionMatrix[15] = 0;
    //        }

    //        // Setup that culling frustum planes for the current view
    //        // FIXME: derive from modelview matrix times projection matrix
    //        static void R_SetupViewFrustum_()
    //        {
    //            var ang = MathX.DEG2RAD(tr.viewDef.renderView.fov_x) * 0.5f;
    //            MathX.SinCos(ang, out var xs, out var xc);

    //            tr.viewDef.frustum[0] = xs * tr.viewDef.renderView.viewaxis[0] + xc * tr.viewDef.renderView.viewaxis[1];
    //            tr.viewDef.frustum[1] = xs * tr.viewDef.renderView.viewaxis[0] - xc * tr.viewDef.renderView.viewaxis[1];

    //            ang = MathX.DEG2RAD(tr.viewDef.renderView.fov_y) * 0.5f;
    //            MathX.SinCos(ang, out xs, out xc);

    //            tr.viewDef.frustum[2] = xs * tr.viewDef.renderView.viewaxis[0] + xc * tr.viewDef.renderView.viewaxis[2];
    //            tr.viewDef.frustum[3] = xs * tr.viewDef.renderView.viewaxis[0] - xc * tr.viewDef.renderView.viewaxis[2];

    //            // plane four is the front clipping plane
    //            tr.viewDef.frustum[4] = /* vec3_origin - */ tr.viewDef.renderView.viewaxis[0];

    //            for (var i = 0; i < 5; i++)
    //            {
    //                // flip direction so positive side faces out (FIXME: globally unify this)
    //                tr.viewDef.frustum[i] = -tr.viewDef.frustum[i].Normal;
    //                tr.viewDef.frustum[i].d = -(tr.viewDef.renderView.vieworg * tr.viewDef.frustum[i].Normal);
    //            }

    //            // eventually, plane five will be the rear clipping plane for fog
    //            float dNear, dFar, dLeft, dUp;

    //            dNear = r_znear.Float;
    //            if (tr.viewDef.renderView.cramZNear) dNear *= 0.25f;

    //            dFar = MAX_WORLD_SIZE;
    //            dLeft = (float)(dFar * Math.Tan(MathX.DEG2RAD(tr.viewDef.renderView.fov_x * 0.5f)));
    //            dUp = (float)(dFar * Math.Tan(MathX.DEG2RAD(tr.viewDef.renderView.fov_y * 0.5f)));
    //            tr.viewDef.viewFrustum.SetOrigin(tr.viewDef.renderView.vieworg);
    //            tr.viewDef.viewFrustum.SetAxis(tr.viewDef.renderView.viewaxis);
    //            tr.viewDef.viewFrustum.SetSize(dNear, dFar, dLeft, dUp);
    //        }

    //        static void R_ConstrainViewFrustum_()
    //        {
    //            Bounds bounds = default;

    //            // constrain the view frustum to the total bounds of all visible lights and visible entities
    //            bounds.Clear();
    //            for (ViewLight vLight = tr.viewDef.viewLights; vLight != null; vLight = vLight.next) bounds.AddBounds(vLight.lightDef.frustumTris.bounds);
    //            for (ViewEntity vEntity = tr.viewDef.viewEntitys; vEntity != null; vEntity = vEntity.next) bounds.AddBounds(vEntity.entityDef.referenceBounds);
    //            tr.viewDef.viewFrustum.ConstrainToBounds(bounds);

    //            if (r_useFrustumFarDistance.Float > 0f) tr.viewDef.viewFrustum.MoveFarDistance(r_useFrustumFarDistance.Float);
    //        }

    //        #endregion

    //        #region DRAWSURF SORTING

    //        static int R_QsortSurfaces_(DrawSurf a, DrawSurf b)
    //            => a.sort < b.sort ? -1 : a.sort > b.sort ? 1 : 0;

    //        static void R_SortDrawSurfs_()
    //        {
    //            // sort the drawsurfs by sort type, then orientation, then shader
    //            qsort(tr.viewDef.drawSurfs, tr.viewDef.numDrawSurfs, sizeof(tr.viewDef.drawSurfs[0]), R_QsortSurfaces_);
    //        }

    //        #endregion

    //        // A view may be either the actual camera view, a mirror / remote location, or a 3D view on a gui surface.
    //        // Parms will typically be allocated with R_FrameAlloc
    //        public static void R_RenderView(ViewDef parms)
    //        {
    //            ViewDef oldView;

    //            if (parms.renderView.width <= 0 || parms.renderView.height <= 0) return;

    //            tr.viewCount++;

    //            // save view in case we are a subview
    //            oldView = tr.viewDef;

    //            tr.viewDef = parms;

    //            tr.sortOffset = 0;

    //            // set the matrix for world space to eye space
    //            R_SetViewMatrix(tr.viewDef);

    //            // the four sides of the view frustum are needed for culling and portal visibility
    //            R_SetupViewFrustum_();

    //            // we need to set the projection matrix before doing portal-to-screen scissor box calculations
    //            R_SetupProjection_();

    //            // identify all the visible portalAreas, and the entityDefs and lightDefs that are in them and pass culling.
    //            ((RenderWorldLocal)parms.renderWorld).FindViewLightsAndEntities();

    //            // constrain the view frustum to the view lights and entities
    //            R_ConstrainViewFrustum_();

    //            // make sure that interactions exist for all light / entity combinations that are visible add any pre-generated light shadows, and calculate the light shader values
    //            R_AddLightSurfaces();

    //            // adds ambient surfaces and create any necessary interaction surfaces to add to the light lists
    //            R_AddModelSurfaces();

    //            // any viewLight that didn't have visible surfaces can have it's shadows removed
    //            R_RemoveUnecessaryViewLights();

    //            // sort all the ambient surfaces for translucency ordering
    //            R_SortDrawSurfs_();

    //            // generate any subviews (mirrors, cameras, etc) before adding this view
    //            if (R_GenerateSubViews())
    //                // if we are debugging subviews, allow the skipping of the main view draw
    //                if (R.r_subviewOnly.Bool) return;

    //            // write everything needed to the demo file
    //            if (session.writeDemo != null) ((RenderWorldLocal)parms.renderWorld).WriteVisibleDefs(tr.viewDef);

    //            // add the rendering commands for this viewDef
    //            R_AddDrawViewCmd(parms);

    //            // restore view in case we are a subview
    //            tr.viewDef = oldView;
    //        }

    //        // performs radius cull first, then corner cull
    //        // Performs quick test before expensive test
    //        // Returns true if the box is outside the given global frustum, (positive sides are out)
    //        public static bool R_CullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
    //        {
    //            if (R_RadiusCullLocalBox(bounds, modelMatrix, numPlanes, planes)) return true;
    //            return R_CornerCullLocalBox(bounds, modelMatrix, numPlanes, planes);
    //        }

    //        // A fast, conservative center-to-corner culling test
    //        // Returns true if the box is outside the given global frustum, (positive sides are out)
    //        public static bool R_RadiusCullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
    //        {
    //            if (r_useCulling.Integer == 0) return false;

    //            // transform the surface bounds into world space
    //            var localOrigin = (bounds[0] + bounds[1]) * 0.5f;

    //            R_LocalPointToGlobal(modelMatrix, localOrigin, out var worldOrigin);

    //            var worldRadius = (bounds[0] - localOrigin).Length;   // FIXME: won't be correct for scaled objects

    //            float d; Plane frust;
    //            for (var i = 0; i < numPlanes; i++)
    //            {
    //                frust = planes[i];
    //                d = frust.Distance(worldOrigin);
    //                if (d > worldRadius) return true;    // culled
    //            }

    //            return false;       // no culled
    //        }
    //        // Tests all corners against the frustum.
    //        // Can still generate a few false positives when the box is outside a corner.
    //        // Returns true if the box is outside the given global frustum, (positive sides are out)
    //        public static bool R_CornerCullLocalBox(Bounds bounds, float[] modelMatrix, int numPlanes, Plane[] planes)
    //        {
    //            int i, j;
    //            var transformed = stackalloc Vector3[8];
    //            var dists = stackalloc float[8];
    //            Vector3 v; Plane frust;

    //            // we can disable box culling for experimental timing purposes
    //            if (r_useCulling.Integer < 2) return false;

    //            // transform into world space
    //            for (i = 0; i < 8; i++)
    //            {
    //                v.x = bounds[i & 1].x;
    //                v.y = bounds[(i >> 1) & 1].y;
    //                v.z = bounds[(i >> 2) & 1].z;
    //                R_LocalPointToGlobal(modelMatrix, v, out transformed[i]);
    //            }

    //            // check against frustum planes
    //            for (i = 0; i < numPlanes; i++)
    //            {
    //                frust = planes[i];
    //                for (j = 0; j < 8; j++)
    //                {
    //                    dists[j] = frust.Distance(transformed[j]);
    //                    if (dists[j] < 0) break;
    //                }
    //                // all points were behind one of the planes
    //                if (j == 8) { tr.pc.c_box_cull_out++; return true; }
    //            }

    //            tr.pc.c_box_cull_in++;

    //            return false;       // not culled
    //        }

    //        public static void R_AxisToModelMatrix(Matrix3x3 axis, Vector3 origin, float* modelMatrix)
    //        {
    //            modelMatrix[0] = axis[0].x;
    //            modelMatrix[4] = axis[1].x;
    //            modelMatrix[8] = axis[2].x;
    //            modelMatrix[12] = origin.x;

    //            modelMatrix[1] = axis[0].y;
    //            modelMatrix[5] = axis[1].y;
    //            modelMatrix[9] = axis[2].y;
    //            modelMatrix[13] = origin.y;

    //            modelMatrix[2] = axis[0].z;
    //            modelMatrix[6] = axis[1].z;
    //            modelMatrix[10] = axis[2].z;
    //            modelMatrix[14] = origin.z;

    //            modelMatrix[3] = 0f;
    //            modelMatrix[7] = 0f;
    //            modelMatrix[11] = 0f;
    //            modelMatrix[15] = 1f;
    //        }

    //        // note that many of these assume a normalized matrix, and will not work with scaled axis
    //        public static void R_GlobalPointToLocal(float* modelMatrix, Vector3 i, out Vector3 o)
    //        {
    //            Vector3 temp = default;
    //            MathX.VectorSubtract(i, modelMatrix.AsSpan(12), ref temp);

    //            o.x = MathX.DotProduct(temp, modelMatrix.AsSpan(0));
    //            o.y = MathX.DotProduct(temp, modelMatrix.AsSpan(4));
    //            o.z = MathX.DotProduct(temp, modelMatrix.AsSpan(8));
    //        }

    //        public static void R_GlobalVectorToLocal(float* modelMatrix, Vector3 i, out Vector3 o)
    //        {
    //            o.x = MathX.DotProduct(i, modelMatrix.AsSpan(0));
    //            o.y = MathX.DotProduct(i, modelMatrix.AsSpan(4));
    //            o.z = MathX.DotProduct(i, modelMatrix.AsSpan(8));
    //        }
    //        public static void R_GlobalPlaneToLocal(float* modelMatrix, Plane i, out Plane o)
    //        {
    //            o.a = MathX.DotProduct(i, modelMatrix.AsSpan(0));
    //            o.b = MathX.DotProduct(i, modelMatrix.AsSpan(4));
    //            o.c = MathX.DotProduct(i, modelMatrix.AsSpan(8));
    //            o.d = i.d + modelMatrix[12] * i.a + modelMatrix[13] * i.b + modelMatrix[14] * i.c;
    //        }
    //        public static void R_PointTimesMatrix(float[] modelMatrix, Vector4 i, out Vector4 o)
    //        {
    //            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8] + modelMatrix[12];
    //            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9] + modelMatrix[13];
    //            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10] + modelMatrix[14];
    //            o.w = i.x * modelMatrix[3] + i.y * modelMatrix[7] + i.z * modelMatrix[11] + modelMatrix[15];
    //        }
    //        // FIXME: these assume no skewing or scaling transforms
    //        public static void R_LocalPointToGlobal(float[] modelMatrix, Vector3 i, out Vector3 o)
    //        {
    //#if __GNUC__ && __SSE2__
    //            __m128 m0, m1, m2, m3;
    //            __m128 in0, in1, in2;
    //            float i0, i1, i2;
    //            i0 = in[0];
    //            i1 = in[1];
    //            i2 = in[2];

    //            m0 = _mm_loadu_ps(&modelMatrix[0]);
    //            m1 = _mm_loadu_ps(&modelMatrix[4]);
    //            m2 = _mm_loadu_ps(&modelMatrix[8]);
    //            m3 = _mm_loadu_ps(&modelMatrix[12]);

    //            in0 = _mm_load1_ps(&i0);
    //            in1 = _mm_load1_ps(&i1);
    //            in2 = _mm_load1_ps(&i2);

    //            m0 = _mm_mul_ps(m0, in0);
    //            m1 = _mm_mul_ps(m1, in1);
    //            m2 = _mm_mul_ps(m2, in2);

    //            m0 = _mm_add_ps(m0, m1);
    //            m0 = _mm_add_ps(m0, m2);
    //            m0 = _mm_add_ps(m0, m3);

    //            _mm_store_ss(&out[0], m0);
    //            m1 = (__m128)_mm_shuffle_epi32((__m128i)m0, 0x55);
    //            _mm_store_ss(&out[1], m1);
    //            m2 = _mm_movehl_ps(m2, m0);
    //            _mm_store_ss(&out[2], m2);
    //#else
    //            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8] + modelMatrix[12];
    //            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9] + modelMatrix[13];
    //            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10] + modelMatrix[14];
    //#endif
    //        }
    //        public static void R_LocalVectorToGlobal(float[] modelMatrix, Vector3 i, out Vector3 o)
    //        {
    //            o.x = i.x * modelMatrix[0] + i.y * modelMatrix[4] + i.z * modelMatrix[8];
    //            o.y = i.x * modelMatrix[1] + i.y * modelMatrix[5] + i.z * modelMatrix[9];
    //            o.z = i.x * modelMatrix[2] + i.y * modelMatrix[6] + i.z * modelMatrix[10];
    //        }
    //        public static void R_LocalPlaneToGlobal(float[] modelMatrix, Plane i, out Plane o)
    //        {
    //            R_LocalVectorToGlobal(modelMatrix, i.Normal, o.Normal);

    //            var offset = modelMatrix[12] * o.a + modelMatrix[13] * o.b + modelMatrix[14] * o.c;
    //            o.d = i.d - offset;
    //        }
    //        // transform Z in eye coordinates to window coordinates
    //        public static void R_TransformEyeZToWin(float src_z, float[] projectionMatrix, out float dst_z)
    //        {
    //            float clip_z, clip_w;

    //            // projection
    //            clip_z = src_z * projectionMatrix[2 + 2 * 4] + projectionMatrix[2 + 3 * 4];
    //            clip_w = src_z * projectionMatrix[3 + 2 * 4] + projectionMatrix[3 + 3 * 4];

    //            if (clip_w <= 0f) dst_z = 0f; // clamp to near plane
    //            else { dst_z = clip_z / clip_w; dst_z = dst_z * 0.5f + 0.5f; } // convert to window coords
    //        }

    //        // -1 to 1 range in x, y, and z
    //        public static void R_GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc)
    //        {
    //            int i;
    //            Plane view = default;
    //            Plane clip = default;

    //            // _D3XP added work on primaryView when no viewDef
    //            if (tr.viewDef == null)
    //            {
    //                for (i = 0; i < 4; i++)
    //                    view[i] =
    //                        global[0] * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 0 * 4] +
    //                        global[1] * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 1 * 4] +
    //                        global[2] * tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 2 * 4] +
    //                        tr.primaryView.worldSpace.u.eyeViewMatrix2[i + 3 * 4];

    //                for (i = 0; i < 4; i++)
    //                    clip[i] =
    //                        view[0] * tr.primaryView.projectionMatrix[i + 0 * 4] +
    //                        view[1] * tr.primaryView.projectionMatrix[i + 1 * 4] +
    //                        view[2] * tr.primaryView.projectionMatrix[i + 2 * 4] +
    //                        view[3] * tr.primaryView.projectionMatrix[i + 3 * 4];
    //            }
    //            else
    //            {
    //                for (i = 0; i < 4; i++)
    //                    view[i] =
    //                        global[0] * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 0 * 4] +
    //                        global[1] * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 1 * 4] +
    //                        global[2] * tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 2 * 4] +
    //                        tr.viewDef.worldSpace.u.eyeViewMatrix2[i + 3 * 4];

    //                for (i = 0; i < 4; i++)
    //                    clip[i] =
    //                        view[0] * tr.viewDef.projectionMatrix[i + 0 * 4] +
    //                        view[1] * tr.viewDef.projectionMatrix[i + 1 * 4] +
    //                        view[2] * tr.viewDef.projectionMatrix[i + 2 * 4] +
    //                        view[3] * tr.viewDef.projectionMatrix[i + 3 * 4];
    //            }

    //            ndc[0] = clip[0] / clip[3];
    //            ndc[1] = clip[1] / clip[3];
    //            ndc[2] = (clip[2] + clip[3]) / (2 * clip[3]);
    //        }

    //        public static void R_TransformModelToClip(Vector3 src, float[] modelMatrix, float[] projectionMatrix, out Plane eye, out Plane dst)
    //        {
    //            int i; eye = default; dst = default;

    //            for (i = 0; i < 4; i++)
    //                eye[i] =
    //                    src.x * modelMatrix[i + 0 * 4] +
    //                    src.y * modelMatrix[i + 1 * 4] +
    //                    src.z * modelMatrix[i + 2 * 4] +
    //                    1 * modelMatrix[i + 3 * 4];

    //            for (i = 0; i < 4; i++)
    //                dst[i] =
    //                    eye.a * projectionMatrix[i + 0 * 4] +
    //                    eye.b * projectionMatrix[i + 1 * 4] +
    //                    eye.c * projectionMatrix[i + 2 * 4] +
    //                    eye.d * projectionMatrix[i + 3 * 4];
    //        }

    //        // Clip to normalized device coordinates
    //        public static void R_TransformClipToDevice(Plane clip, ViewDef view, out Vector3 normalized)
    //        {
    //            normalized.x = clip.a / clip.d;
    //            normalized.y = clip.b / clip.d;
    //            normalized.z = clip.c / clip.d;
    //        }

    //        public static void R_TransposeGLMatrix(float[] i, ref float[] o) //: o = new float[16];
    //        {
    //            int i2, j;

    //            for (i2 = 0; i2 < 4; i2++)
    //                for (j = 0; j < 4; j++)
    //                    o[i2 * 4 + j] = i[j * 4 + i2];
    //        }

    //        // Sets up the world to view matrix for a given viewParm
    //        static float[] R_SetViewMatrix_flipMatrix = { // convert from our coordinate system (looking down X) to OpenGL's coordinate system (looking down -Z)
    //		    0, 0, -1, 0,
    //            -1, 0, 0, 0,
    //            0, 1, 0, 0,
    //            0, 0, 0, 1
    //        };
    //        public static void R_SetViewMatrix(ViewDef viewDef)
    //        {
    //            Vector3 origin;
    //            ViewEntity world;
    //            var viewerMatrix = stackalloc float[16];
    //            {
    //                world = viewDef.worldSpace;

    //                memset(world, 0, sizeof(*world));

    //                // the model matrix is an identity
    //                world.modelMatrix[0 * 4 + 0] = 1;
    //                world.modelMatrix[1 * 4 + 1] = 1;
    //                world.modelMatrix[2 * 4 + 2] = 1;

    //                for (var eye = 0; eye <= 2; ++eye)
    //                {
    //                    // transform by the camera placement
    //                    origin = viewDef.renderView.vieworg;

    //                    if (eye < 2 && !Doom3Quest_useScreenLayer && !viewDef.renderView.forceMono)
    //                        origin += (eye == 0 ? 1f : -1f) * viewDef.renderView.viewaxis[1] *
    //                                  (cvarSystem.GetCVarFloat("vr_ipd") / 2f) *
    //                                  (100f / 2.54f * cvarSystem.GetCVarFloat("vr_scale"));

    //                    viewerMatrix[0] = viewDef.renderView.viewaxis[0].x;
    //                    viewerMatrix[4] = viewDef.renderView.viewaxis[0].y;
    //                    viewerMatrix[8] = viewDef.renderView.viewaxis[0].z;
    //                    viewerMatrix[12] = -origin.x * viewerMatrix[0] + -origin.y * viewerMatrix[4] + -origin.z * viewerMatrix[8];

    //                    viewerMatrix[1] = viewDef.renderView.viewaxis[1].x;
    //                    viewerMatrix[5] = viewDef.renderView.viewaxis[1].y;
    //                    viewerMatrix[9] = viewDef.renderView.viewaxis[1].z;
    //                    viewerMatrix[13] = -origin.x * viewerMatrix[1] + -origin.y * viewerMatrix[5] + -origin.z * viewerMatrix[9];

    //                    viewerMatrix[2] = viewDef.renderView.viewaxis[2].x;
    //                    viewerMatrix[6] = viewDef.renderView.viewaxis[2].y;
    //                    viewerMatrix[10] = viewDef.renderView.viewaxis[2].z;
    //                    viewerMatrix[14] = -origin.x * viewerMatrix[2] + -origin.y * viewerMatrix[6] + -origin.z * viewerMatrix[10];

    //                    viewerMatrix[3] = 0f;
    //                    viewerMatrix[7] = 0f;
    //                    viewerMatrix[11] = 0f;
    //                    viewerMatrix[15] = 1f;

    //                    // convert from our coordinate system (looking down X) to OpenGL's coordinate system (looking down -Z)
    //                    myGlMultMatrix(viewerMatrix, R_SetViewMatrix_flipMatrix, world.u.eyeViewMatrix[eye]);
    //                }
    //            }
    //        }

    //        public static void myGlMultMatrix(float[] a, float[] b, float[] o)
    //        {
    //            o[0 * 4 + 0] = a[0 * 4 + 0] * b[0 * 4 + 0] + a[0 * 4 + 1] * b[1 * 4 + 0] + a[0 * 4 + 2] * b[2 * 4 + 0] + a[0 * 4 + 3] * b[3 * 4 + 0];
    //            o[0 * 4 + 1] = a[0 * 4 + 0] * b[0 * 4 + 1] + a[0 * 4 + 1] * b[1 * 4 + 1] + a[0 * 4 + 2] * b[2 * 4 + 1] + a[0 * 4 + 3] * b[3 * 4 + 1];
    //            o[0 * 4 + 2] = a[0 * 4 + 0] * b[0 * 4 + 2] + a[0 * 4 + 1] * b[1 * 4 + 2] + a[0 * 4 + 2] * b[2 * 4 + 2] + a[0 * 4 + 3] * b[3 * 4 + 2];
    //            o[0 * 4 + 3] = a[0 * 4 + 0] * b[0 * 4 + 3] + a[0 * 4 + 1] * b[1 * 4 + 3] + a[0 * 4 + 2] * b[2 * 4 + 3] + a[0 * 4 + 3] * b[3 * 4 + 3];
    //            o[1 * 4 + 0] = a[1 * 4 + 0] * b[0 * 4 + 0] + a[1 * 4 + 1] * b[1 * 4 + 0] + a[1 * 4 + 2] * b[2 * 4 + 0] + a[1 * 4 + 3] * b[3 * 4 + 0];
    //            o[1 * 4 + 1] = a[1 * 4 + 0] * b[0 * 4 + 1] + a[1 * 4 + 1] * b[1 * 4 + 1] + a[1 * 4 + 2] * b[2 * 4 + 1] + a[1 * 4 + 3] * b[3 * 4 + 1];
    //            o[1 * 4 + 2] = a[1 * 4 + 0] * b[0 * 4 + 2] + a[1 * 4 + 1] * b[1 * 4 + 2] + a[1 * 4 + 2] * b[2 * 4 + 2] + a[1 * 4 + 3] * b[3 * 4 + 2];
    //            o[1 * 4 + 3] = a[1 * 4 + 0] * b[0 * 4 + 3] + a[1 * 4 + 1] * b[1 * 4 + 3] + a[1 * 4 + 2] * b[2 * 4 + 3] + a[1 * 4 + 3] * b[3 * 4 + 3];
    //            o[2 * 4 + 0] = a[2 * 4 + 0] * b[0 * 4 + 0] + a[2 * 4 + 1] * b[1 * 4 + 0] + a[2 * 4 + 2] * b[2 * 4 + 0] + a[2 * 4 + 3] * b[3 * 4 + 0];
    //            o[2 * 4 + 1] = a[2 * 4 + 0] * b[0 * 4 + 1] + a[2 * 4 + 1] * b[1 * 4 + 1] + a[2 * 4 + 2] * b[2 * 4 + 1] + a[2 * 4 + 3] * b[3 * 4 + 1];
    //            o[2 * 4 + 2] = a[2 * 4 + 0] * b[0 * 4 + 2] + a[2 * 4 + 1] * b[1 * 4 + 2] + a[2 * 4 + 2] * b[2 * 4 + 2] + a[2 * 4 + 3] * b[3 * 4 + 2];
    //            o[2 * 4 + 3] = a[2 * 4 + 0] * b[0 * 4 + 3] + a[2 * 4 + 1] * b[1 * 4 + 3] + a[2 * 4 + 2] * b[2 * 4 + 3] + a[2 * 4 + 3] * b[3 * 4 + 3];
    //            o[3 * 4 + 0] = a[3 * 4 + 0] * b[0 * 4 + 0] + a[3 * 4 + 1] * b[1 * 4 + 0] + a[3 * 4 + 2] * b[2 * 4 + 0] + a[3 * 4 + 3] * b[3 * 4 + 0];
    //            o[3 * 4 + 1] = a[3 * 4 + 0] * b[0 * 4 + 1] + a[3 * 4 + 1] * b[1 * 4 + 1] + a[3 * 4 + 2] * b[2 * 4 + 1] + a[3 * 4 + 3] * b[3 * 4 + 1];
    //            o[3 * 4 + 2] = a[3 * 4 + 0] * b[0 * 4 + 2] + a[3 * 4 + 1] * b[1 * 4 + 2] + a[3 * 4 + 2] * b[2 * 4 + 2] + a[3 * 4 + 3] * b[3 * 4 + 2];
    //            o[3 * 4 + 3] = a[3 * 4 + 0] * b[0 * 4 + 3] + a[3 * 4 + 1] * b[1 * 4 + 3] + a[3 * 4 + 2] * b[2 * 4 + 3] + a[3 * 4 + 3] * b[3 * 4 + 3];
    //        }

    //        #endregion

    //        #region LIGHT (TR_Light.cs)

    //        public static void R_ListRenderLightDefs_f(CmdArgs args);
    //        public static void R_ListRenderEntityDefs_f(CmdArgs args);

    //        //public static bool R_IssueEntityDefCallback(RenderEntityLocal def);
    //        //public static IRenderModel R_EntityDefDynamicModel(RenderEntityLocal def);

    //        //public static ViewEntity R_SetEntityDefViewEntity(RenderEntityLocal def);
    //        //public static ViewLight R_SetLightDefViewLight(RenderLightLocal def);

    //        //public static void R_AddDrawSurf(SrfTriangles tri, ViewEntity space, RenderEntity renderEntity, Material shader, ScreenRect scissor);

    //        //public static void R_LinkLightSurf(ref DrawSurf link, SrfTriangles tri, ViewEntity space, RenderLightLocal light, Material shader, ScreenRect scissor, bool viewInsideShadow);

    //        //public static bool R_CreateAmbientCache(SrfTriangles tri, bool needsLighting);
    //        //public static bool R_CreateIndexCache(SrfTriangles tri);
    //        //public static bool R_CreatePrivateShadowCache(SrfTriangles tri);
    //        //public static bool R_CreateVertexProgramShadowCache(SrfTriangles tri);

    //        #endregion

    //        #region LIGHTRUN (TR_LightRun.cs)

    //        //public static void R_RegenerateWorld_f(CmdArgs args);

    //        //public static void R_ModulateLights_f(CmdArgs args);

    //        //public static void R_SetLightProject(Plane[] lightProject, in Vector3 origin, in Vector3 targetPoint, in Vector3 rightVector, in Vector3 upVector, in Vector3 start, in Vector3 stop);

    //        //public static void R_AddLightSurfaces();
    //        //public static void R_AddModelSurfaces();
    //        //public static void R_RemoveUnecessaryViewLights();

    //        //public static void R_FreeDerivedData();
    //        //public static void R_ReCreateWorldReferences();

    //        //public static void R_CreateEntityRefs(RenderEntityLocal def);
    //        //public static void R_CreateLightRefs(RenderLightLocal light);

    //        //public static void R_DeriveLightData(RenderLightLocal light);
    //        //public static void R_FreeLightDefDerivedData(RenderLightLocal light);
    //        //public static void R_CheckForEntityDefsUsingModel(IRenderModel model);

    //        //public static void R_ClearEntityDefDynamicModel(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefDerivedData(RenderEntityLocal def, bool keepDecals, bool keepCachedDynamicModel);
    //        public static void R_FreeEntityDefCachedDynamicModel(RenderEntityLocal def) => throw new NotImplementedException();
    //        //public static void R_FreeEntityDefDecals(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefOverlay(RenderEntityLocal def);
    //        //public static void R_FreeEntityDefFadedDecals(RenderEntityLocal def, int time);

    //        //public static void R_CreateLightDefFogPortals(RenderLightLocal ldef);

    //        // Framebuffer stuff
    //        public static void R_InitFrameBuffer();
    //        public static void R_FrameBufferStart();
    //        public static void R_FrameBufferEnd();

    //        #endregion

    //        #region POLYTOPE (TR_Polytop.cs)

    //        //public static SrfTriangles R_PolytopeSurface(int numPlanes, Plane[] planes, Winding[] windings);

    //        #endregion

    //        #region RENDER BACKEND (TR_Render.cs)
    //        // NB: Not touching to GLSL shader stuff. This is using classic OGL calls only.

    //        //public static void RB_DrawView(DrawSurfsCommand data);
    //        public static void RB_RenderView();

    //        //public static void RB_DrawElementsWithCounters(DrawSurf surf);
    //        //public static void RB_DrawShadowElementsWithCounters(DrawSurf surf, int numIndexes);
    //        //public static void RB_SubmitInteraction(DrawInteraction din, Action<DrawInteraction> drawInteraction);
    //        //public static void RB_SetDrawInteraction(ShaderStage surfaceStage, float* surfaceRegs, ref Image image, Vector4[] matrix, float[] color);
    //        //public static void RB_BindVariableStageImage(TextureStage texture, float* shaderRegisters);
    //        //public static void RB_BeginDrawingView();
    //        //public static void RB_GetShaderTextureMatrix(float* shaderRegisters, TextureStage texture, float[] matrix);
    //        public static void RB_BakeTextureMatrixIntoTexgen(Matrix4x4 lightProject, float[] textureMatrix);

    //        #endregion

    //        void R_ReloadGLSLPrograms_f(CmdArgs args);

    //        void RB_GLSL_PrepareShaders();
    //        void RB_GLSL_FillDepthBuffer(DrawSurf[] drawSurfs, int numDrawSurfs);
    //        void RB_GLSL_DrawInteractions();
    //        int RB_GLSL_DrawShaderPasses(DrawSurf[] drawSurfs, int numDrawSurfs);
    //        void RB_GLSL_FogAllLights();

    //        // TR_STENCILSHADOWS
    //        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1
    //        void R_MakeShadowFrustums(RenderLightLocal def);

    //        public enum ShadowGen
    //        {
    //            SG_DYNAMIC,     // use infinite projections
    //            SG_STATIC,      // clip to bounds
    //        }
    //        SrfTriangles R_CreateShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, ShadowGen optimize, SrfCullInfo cullInfo);

    //        // TR_TURBOSHADOW
    //        // Fast, non-clipped overshoot shadow volumes
    //        // "facing" should have one more element than tri->numIndexes / 3, which should be set to 1 calling this function may modify "facing" based on culling
    //        //public static SrfTriangles R_CreateVertexProgramTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);
    //        ////public static SrfTriangles R_CreateTurboShadowVolume(RenderEntityLocal ent, SrfTriangles tri, RenderLightLocal light, SrfCullInfo cullInfo);

    //        #region TRISURF (TR_TriSurf.cs)

    //        //public static void R_InitTriSurfData();
    //        //public static void R_ShutdownTriSurfData();
    //        //public static void R_PurgeTriSurfData(FrameData frame);
    //        //public static void R_ShowTriSurfMemory_f(CmdArgs args);

    //        //public static SrfTriangles R_AllocStaticTriSurf();
    //        //public static SrfTriangles R_CopyStaticTriSurf(SrfTriangles tri);
    //        //public static void R_AllocStaticTriSurfVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_AllocStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
    //        //public static void R_AllocStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_AllocStaticTriSurfPlanes(SrfTriangles tri, int numIndexes);
    //        //public static void R_ResizeStaticTriSurfVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_ResizeStaticTriSurfIndexes(SrfTriangles tri, int numIndexes);
    //        //public static void R_ResizeStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts);
    //        //public static void R_ReferenceStaticTriSurfVerts(SrfTriangles tri, SrfTriangles reference);
    //        //public static void R_ReferenceStaticTriSurfIndexes(SrfTriangles tri, SrfTriangles reference);
    //        //public static void R_FreeStaticTriSurfSilIndexes(SrfTriangles tri);
    //        //public static void R_FreeStaticTriSurf(SrfTriangles tri);
    //        //public static void R_FreeStaticTriSurfVertexCaches(SrfTriangles tri);
    //        //public static void R_ReallyFreeStaticTriSurf(SrfTriangles tri);
    //        //public static void R_FreeDeferredTriSurfs(FrameData frame);
    //        //public static int R_TriSurfMemory(SrfTriangles tri);

    //        //public static void R_BoundTriSurf(SrfTriangles tri);
    //        //public static void R_RemoveDuplicatedTriangles(SrfTriangles tri);
    //        //public static void R_CreateSilIndexes(SrfTriangles tri);
    //        //public static void R_RemoveDegenerateTriangles(SrfTriangles tri);
    //        //public static void R_RemoveUnusedVerts(SrfTriangles tri);
    //        //public static void R_RangeCheckIndexes(SrfTriangles tri);
    //        //public static void R_CreateVertexNormals(SrfTriangles tri);    // also called by dmap
    //        //public static void R_DeriveFacePlanes(SrfTriangles tri);       // also called by renderbump
    //        //public static void R_CleanupTriangles(SrfTriangles tri, bool createNormals, bool identifySilEdges, bool useUnsmoothedTangents);
    //        //public static void R_ReverseTriangles(SrfTriangles tri);

    //        // Only deals with vertexes and indexes, not silhouettes, planes, etc. Does NOT perform a cleanup triangles, so there may be duplicated verts in the result.
    //        //public static SrfTriangles R_MergeSurfaceList(SrfTriangles[] surfaces, int numSurfaces);
    //        //public static SrfTriangles R_MergeTriangles(SrfTriangles tri1, SrfTriangles tri2);

    //        // if the deformed verts have significant enough texture coordinate changes to reverse the texture polarity of a triangle, the tangents will be incorrect
    //        //public static void R_DeriveTangents(SrfTriangles tri, bool allocFacePlanes = true);

    //        // deformable meshes precalculate as much as possible from a base frame, then generate complete srfTriangles_t from just a new set of vertexes
    //        public class DeformInfo
    //        {
    //            public int numSourceVerts;

    //            // numOutputVerts may be smaller if the input had duplicated or degenerate triangles it will often be larger if the input had mirrored texture seams that needed to be busted for proper tangent spaces
    //            public int numOutputVerts;
    //            public DrawVert[] verts;

    //            public int numMirroredVerts;
    //            public int[] mirroredVerts;

    //            public int numIndexes;
    //            public GlIndex[] indexes;

    //            public GlIndex[] silIndexes;

    //            public int numDupVerts;
    //            public int[] dupVerts;

    //            public int numSilEdges;
    //            public SilEdge[] silEdges;

    //            public DominantTri[] dominantTris;
    //        }

    //        //public static DeformInfo R_BuildDeformInfo(int numVerts, DrawVert[] verts, int numIndexes, int[] indexes, bool useUnsmoothedTangents);
    //        //public static void R_FreeDeformInfo(DeformInfo deformInfo);
    //        //public static int R_DeformInfoMemoryUsed(DeformInfo deformInfo);

    //        #endregion

    //        #region SUBVIEW (TR_SubView.cs)

    //        //public static bool R_PreciseCullSurface(DrawSurf drawSurf, Bounds ndcBounds);
    //        //public static bool R_GenerateSubViews();

    //        #endregion

    //        #region SCENE GENERATION

    //        const int NUM_FRAME_DATA = 2;
    //        static FrameData[] smpFrameData = new FrameData[NUM_FRAME_DATA];
    //        static volatile uint smpFrame;
    //        const int MEMORY_BLOCK_SIZE = 0x100000;

    //        public static void R_InitFrameData()
    //        {
    //            int size;
    //            FrameData frame;
    //            FrameMemoryBlock block;

    //            R_ShutdownFrameData();

    //            for (var n = 0; n < NUM_FRAME_DATA; n++)
    //            {
    //                smpFrameData[n] = new FrameData();
    //                frame = smpFrameData[n];
    //                size = MEMORY_BLOCK_SIZE;
    //                block = (FrameMemoryBlock)Mem_Alloc(size + sizeof(block));
    //                if (block == null) common.FatalError("R_InitFrameData: Mem_Alloc() failed");
    //                block.size = size;
    //                block.used = 0;
    //                block.next = null;
    //                frame.memory = block;
    //                frame.memoryHighwater = 0;
    //            }

    //            smpFrame = 0;
    //            frameData = smpFrameData[0];

    //            R_ToggleSmpFrame();
    //        }

    //        public static void R_ShutdownFrameData()
    //        {
    //            FrameData frame;
    //            FrameMemoryBlock block;

    //            for (var n = 0; n < NUM_FRAME_DATA; n++)
    //            {
    //                // free any current data
    //                frame = smpFrameData[n];
    //                if (frame == null) continue;

    //                R_FreeDeferredTriSurfs(frame);

    //                FrameMemoryBlock nextBlock;
    //                for (block = frame.memory; block != null; block = nextBlock)
    //                {
    //                    nextBlock = block.next;
    //                    Mem_Free(block);
    //                }
    //                Mem_Free(frame);
    //                smpFrameData[n] = null;
    //            }
    //            frameData = null;
    //        }

    //        public static int R_CountFrameData()
    //        {
    //            FrameData frame;
    //            FrameMemoryBlock block;
    //            int count;

    //            count = 0;
    //            frame = frameData;
    //            for (block = frame.memory; block != null; block = block.next)
    //            {
    //                count += block.used;
    //                if (block == frame.alloc) break;
    //            }

    //            // note if this is a new highwater mark
    //            if (count > frame.memoryHighwater) frame.memoryHighwater = count;

    //            return count;
    //        }

    //        public static void R_ToggleSmpFrame()
    //        {
    //            if (r_lockSurfaces.Bool) return;

    //            smpFrame++;
    //            frameData = smpFrameData[smpFrame % NUM_FRAME_DATA];

    //            R_FreeDeferredTriSurfs(frameData);

    //            // clear frame-temporary data
    //            FrameData frame;
    //            FrameMemoryBlock block;

    //            // update the highwater mark
    //            R_CountFrameData();

    //            frame = frameData;

    //            // reset the memory allocation to the first block
    //            frame.alloc = frame.memory;

    //            // clear all the blocks
    //            for (block = frame.memory; block != null; block = block.next) block.used = 0;

    //            R_ClearCommandChain();
    //        }

    //        // This data will be automatically freed when the current frame's back end completes.
    //        // This should only be called by the front end. The back end shouldn't need to allocate memory.
    //        // If we passed smpFrame in, the back end could alloc memory, because it will always be a different frameData than the front end is using.
    //        // All temporary data, like dynamic tesselations and local spaces are allocated here.
    //        // The memory will not move, but it may not be contiguous with previous allocations even from this frame.
    //        // The memory is NOT zero filled.
    //        // Should part of this be inlined in a macro?
    //        public static T R_FrameAlloc<T>()
    //        {
    //            FrameData frame;
    //            FrameMemoryBlock block;
    //            byte[] buf;

    //            bytes = (bytes + 16) & ~15;
    //            // see if it can be satisfied in the current block
    //            frame = frameData;
    //            block = frame.alloc;

    //            if (block.size - block.used >= bytes)
    //            {
    //                buf = block.base_ + block.used;
    //                block.used += bytes;
    //                return buf;
    //            }

    //            // advance to the next memory block if available
    //            block = block.next;
    //            // create a new block if we are at the end of the chain
    //            if (block == null)
    //            {
    //                int size;

    //                size = MEMORY_BLOCK_SIZE;
    //                block = (FrameMemoryBlock)Mem_Alloc(size + sizeof(block));
    //                if (block == null) common.FatalError("R_FrameAlloc: Mem_Alloc() failed");
    //                block.size = size;
    //                block.used = 0;
    //                block.next = null;
    //                frame.alloc.next = block;
    //            }

    //            // we could fix this if we needed to...
    //            if (bytes > block.size) common.FatalError($"R_FrameAlloc of {bytes} exceeded MEMORY_BLOCK_SIZE");

    //            frame.alloc = block;

    //            block.used = bytes;

    //            return block.base;
    //        }

    //        public static T[] R_FrameAllocMany<T>(int count);

    //        public static T R_ClearedFrameAlloc<T>();

    //        public static T[] R_ClearedFrameAllocMany<T>(int count);

    //        // This does nothing at all, as the frame data is reused every frame and can only be stack allocated.
    //        // The only reason for it's existance is so functions that can use either static or frame memory can set function pointers to both alloc and free.
    //        public static void R_FrameFree<T>(ref T data) { }

    //        public static void R_DirectFrameBufferStart();

    //        public static void R_DirectFrameBufferEnd();

    //        public static T R_StaticAlloc<T>()     // just malloc with error checking
    //        {
    //            void* buf;

    //            tr.pc.c_alloc++;

    //            tr.staticAllocCount += bytes;

    //            buf = Mem_Alloc(bytes);

    //            // don't exit on failure on zero length allocations since the old code didn't
    //            if (buf == null && (bytes != 0)) common.FatalError($"R_StaticAlloc failed on {bytes} bytes");
    //            return buf;
    //        }

    //        public static T R_ClearedStaticAlloc<T>()  // with memset
    //        {
    //            void* buf;

    //            buf = R_StaticAlloc(bytes);
    //            Simd.Memset(buf, 0, bytes);
    //            return buf;
    //        }

    //        public static T[] R_ClearedStaticAllocMany<T>(int count)  // with memset
    //        {
    //            void* buf;

    //            buf = R_StaticAlloc(bytes);
    //            Simd.Memset(buf, 0, bytes);
    //            return buf;
    //        }

    //        public static void R_StaticFree<T>(ref T data)
    //        {
    //            tr.pc.c_free++;
    //            Mem_Free(data);
    //        }

    //        #endregion

    //        #region TR_BACKEND (TR_Backend.cs)

    //        //public static void RB_SetDefaultGLState();
    //        //public static void RB_ExecuteBackEndCommands(EmptyCommand cmds);

    //        #endregion

    //        #region TR_GUISURF (TR_GuiSurf.cs)

    //        //public static void R_SurfaceToTextureAxis(SrfTriangles tri, Vector3 origin, Vector3[] axis);
    //        //public static void R_RenderGuiSurf(IUserInterface gui, DrawSurf drawSurf);

    //        #endregion

    //        #region TR_ORDERINDEXES (TR_OrderIndexes.cs)

    //        //public static void R_OrderIndexes(int numIndexes, GlIndex[] indexes);

    //        #endregion

    //        #region TR_DEFORM (TR_Deform.cs)

    //        //public static void R_DeformDrawSurf(DrawSurf drawSurf);

    //        #endregion

    //        #region TR_TRACE (TR_Trace.cs)

    //        //public struct LocalTrace
    //        //{
    //        //    public float fraction;
    //        //    // only valid if fraction < 1.0
    //        //    public Vector3 point;
    //        //    public Vector3 normal;
    //        //    public int[] indexes;
    //        //}

    //        //public static LocalTrace R_LocalTrace(Vector3 start, Vector3 end, float radius, SrfTriangles tri);

    //        #endregion

    //        #region TR_SHADOWBOUNDS (TR_ShadowBounds.cs)

    //        //public static ScreenRect R_CalcIntersectionScissor(RenderLightLocal lightDef, RenderEntityLocal entityDef, ViewDef viewDef);

    //        #endregion
}
