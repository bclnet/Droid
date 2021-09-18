using System.NumericsX;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.OpenStack;
using GlIndex = System.Int32;

namespace Gengine.Render
{
    unsafe partial class TRX
    {
        // The ambientCache is on the stack, so we don't want to leave a reference to it that would try to be freed later.  Create the ambientCache immediately.
        static void R_FinishDeform(DrawSurf drawSurf, SrfTriangles newTri, DrawVert[] ac)
        {
            if (newTri == null)
                return;

            // generate current normals, tangents, and bitangents We might want to support the possibility of deform functions generating
            // explicit normals, and we might also want to allow the cached deformInfo optimization for these.
            // FIXME: this doesn't work, because the deformed surface is just the ambient one, and there isn't an opportunity to generate light interactions
            if (drawSurf.material.ReceivesLighting)
            {
                newTri.verts = ac;
                R_DeriveTangents(newTri, false);
                newTri.verts = null;
            }

            newTri.ambientCache = VertexCacheX.AllocFrameTemp(ac, newTri.numVerts * DrawVert.SizeOf, false);
            newTri.indexCache = VertexCacheX.AllocFrameTemp(newTri.indexes, newTri.numIndexes * sizeof(GlIndex), true);

            drawSurf.geoFrontEnd = newTri;
            drawSurf.ambientCache = newTri.ambientCache;
            drawSurf.indexCache = newTri.indexCache;
            drawSurf.numIndexes = newTri.numIndexes;
            drawSurf.numShadowIndexesNoFrontCaps = newTri.numShadowIndexesNoFrontCaps;
            drawSurf.numShadowIndexesNoCaps = newTri.numShadowIndexesNoCaps;
            drawSurf.shadowCapPlaneBits = newTri.shadowCapPlaneBits;
        }

        // Assuming all the triangles for this shader are independant quads, rebuild them as forward facing sprites
        static void R_AutospriteDeform(DrawSurf surf)
        {
            int i; float radius;
            Vector3 mid, delta, left, up;
            SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            if ((tri.numVerts & 3) != 0)
            {
                common.Warning("R_AutospriteDeform: shader had odd vertex count");
                return;
            }
            if (tri.numIndexes != (tri.numVerts >> 2) * 6)
            {
                common.Warning("R_AutospriteDeform: autosprite had odd index count");
                return;
            }

            R_GlobalVectorToLocal(surf.space.modelMatrix, tr.viewDef.renderView.viewaxis[1], out var leftDir);
            R_GlobalVectorToLocal(surf.space.modelMatrix, tr.viewDef.renderView.viewaxis[2], out var upDir);

            if (tr.viewDef.isMirror)
                leftDir = Vector3.origin - leftDir;

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = R_FrameAllocMany<GlIndex>(newTri.numIndexes);

            var ac = stackalloc DrawVert[newTri.numVerts];
            var vt = tri.verts;
            for (i = 0; i < tri.numVerts; i += 4)
            {
                // find the midpoint
                mid.x = 0.25f * (vt[i + 0].xyz.x + vt[i + 1].xyz.x + vt[i + 2].xyz.x + vt[i + 3].xyz.x);
                mid.y = 0.25f * (vt[i + 0].xyz.y + vt[i + 1].xyz.y + vt[i + 2].xyz.y + vt[i + 3].xyz.y);
                mid.z = 0.25f * (vt[i + 0].xyz.z + vt[i + 1].xyz.z + vt[i + 2].xyz.z + vt[i + 3].xyz.z);

                delta = vt[i + 0].xyz - mid;
                radius = delta.Length * 0.707f;        // / sqrt(2)

                left = leftDir * radius;
                up = upDir * radius;

                ac[i + 0].xyz = mid + left + up;
                ac[i + 0].st.x = 0; ac[i + 0].st.y = 0;
                ac[i + 1].xyz = mid - left + up;
                ac[i + 1].st.x = 1; ac[i + 1].st.y = 0;
                ac[i + 2].xyz = mid - left - up;
                ac[i + 2].st.x = 1; ac[i + 2].st.y = 1;
                ac[i + 3].xyz = mid + left - up;
                ac[i + 3].st.x = 0; ac[i + 3].st.y = 1;

                newTri.indexes[6 * (i >> 2) + 0] = i;
                newTri.indexes[6 * (i >> 2) + 1] = i + 1;
                newTri.indexes[6 * (i >> 2) + 2] = i + 2;

                newTri.indexes[6 * (i >> 2) + 3] = i;
                newTri.indexes[6 * (i >> 2) + 4] = i + 2;
                newTri.indexes[6 * (i >> 2) + 5] = i + 3;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        // will pivot a rectangular quad along the center of its long axis
        // Note that a geometric tube with even quite a few sides tube will almost certainly render much faster than this, so this should only be for faked volumetric tubes.
        // Make sure this is used with twosided translucent shaders, because the exact side order may not be correct.
        static (int x, int y)[] edgeVerts = {
            (0, 1),
            (1, 2),
            (2, 0),
            (3, 4),
            (4, 5),
            (5, 3)
        };

        static void R_TubeDeform(DrawSurf surf)
        {
            int i, j;
            int indexes;
            SrfTriangles tri;

            tri = surf.geoFrontEnd;

            if ((tri.numVerts & 3) != 0)
                common.Error("R_AutospriteDeform: shader had odd vertex count");
            if (tri.numIndexes != (tri.numVerts >> 2) * 6)
                common.Error("R_AutospriteDeform: autosprite had odd index count");

            // we need the view direction to project the minor axis of the tube as the view changes
            R_GlobalPointToLocal(surf.space.modelMatrix, tr.viewDef.renderView.vieworg, out var localView);

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            var newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = R_FrameAllocMany<GlIndex>(newTri.numIndexes);
            fixed (void* d = newTri.indexes, s = tri.indexes)
                Unsafe.CopyBlock(d, s, (uint)(newTri.numIndexes * sizeof(GlIndex)));

            var ac = stackalloc DrawVert[newTri.numVerts];

            // this is a lot of work for two triangles... we could precalculate a lot if it is an issue, but it would mess up the shader abstraction
            for (i = 0, indexes = 0; i < tri.numVerts; i += 4, indexes += 6)
            {
                float lengths[2];
                int nums[2];
                idVec3 mid[2];
                idVec3 major, minor;
                const idDrawVert* v1, *v2;

                // identify the two shortest edges out of the six defined by the indexes
                nums[0] = nums[1] = 0;
                lengths[0] = lengths[1] = 999999;

                for (j = 0; j < 6; j++)
                {
                    float l;

                    v1 = &tri.verts[tri.indexes[i + edgeVerts[j][0]]];
                    v2 = &tri.verts[tri.indexes[i + edgeVerts[j][1]]];

                    l = (v1.xyz - v2.xyz).Length();
                    if (l < lengths[0])
                    {
                        nums[1] = nums[0];
                        lengths[1] = lengths[0];
                        nums[0] = j;
                        lengths[0] = l;
                    }
                    else if (l < lengths[1])
                    {
                        nums[1] = j;
                        lengths[1] = l;
                    }
                }

                // find the midpoints of the two short edges, which will give us the major axis in object coordinates
                for (j = 0; j < 2; j++)
                {
                    v1 = &tri.verts[tri.indexes[i + edgeVerts[nums[j]][0]]];
                    v2 = &tri.verts[tri.indexes[i + edgeVerts[nums[j]][1]]];

                    mid[j][0] = 0.5 * (v1.xyz[0] + v2.xyz[0]);
                    mid[j][1] = 0.5 * (v1.xyz[1] + v2.xyz[1]);
                    mid[j][2] = 0.5 * (v1.xyz[2] + v2.xyz[2]);
                }

                // find the vector of the major axis
                major = mid[1] - mid[0];

                // re-project the points
                for (j = 0; j < 2; j++)
                {
                    float l;
                    int i1 = tri.indexes[i + edgeVerts[nums[j]][0]];
                    int i2 = tri.indexes[i + edgeVerts[nums[j]][1]];

                    idDrawVert* av1 = &ac[i1];
                    idDrawVert* av2 = &ac[i2];

                    *av1 = *(idDrawVert*)&tri.verts[i1];
                    *av2 = *(idDrawVert*)&tri.verts[i2];

                    l = 0.5 * lengths[j];

                    // cross this with the view direction to get minor axis
                    idVec3 dir = mid[j] - localView;
                    minor.Cross(major, dir);
                    minor.Normalize();

                    if (j)
                    {
                        av1.xyz = mid[j] - l * minor;
                        av2.xyz = mid[j] + l * minor;
                    }
                    else
                    {
                        av1.xyz = mid[j] + l * minor;
                        av2.xyz = mid[j] - l * minor;
                    }
                }
            }

            R_FinishDeform(surf, newTri, ac);
        }

        const int MAX_TRI_WINDING_INDEXES = 16;
        static int R_WindingFromTriangles(SrfTriangles tri, GlIndex[] indexes)
        {
            int i, j, k, l;

            indexes[0] = tri.indexes[0];
            int numIndexes = 1;
            int numTris = tri.numIndexes / 3;

            do
            {
                // find an edge that goes from the current index to another index that isn't already used, and isn't an internal edge
                for (i = 0; i < numTris; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (tri.indexes[i * 3 + j] != indexes[numIndexes - 1])
                            continue;
                        int next = tri.indexes[i * 3 + (j + 1) % 3];

                        // make sure it isn't already used
                        if (numIndexes == 1)
                        {
                            if (next == indexes[0])
                                continue;
                        }
                        else
                        {
                            for (k = 1; k < numIndexes; k++)
                                if (indexes[k] == next)
                                    break;
                            if (k != numIndexes)
                                continue;
                        }

                        // make sure it isn't an interior edge
                        for (k = 0; k < numTris; k++)
                        {
                            if (k == i)
                                continue;
                            for (l = 0; l < 3; l++)
                            {
                                int a, b;

                                a = tri.indexes[k * 3 + l];
                                if (a != next)
                                    continue;
                                b = tri.indexes[k * 3 + (l + 1) % 3];
                                if (b != indexes[numIndexes - 1])
                                    continue;

                                // this is an interior edge
                                break;
                            }
                            if (l != 3)
                                break;
                        }
                        if (k != numTris)
                            continue;

                        // add this to the list
                        indexes[numIndexes] = next;
                        numIndexes++;
                        break;
                    }
                    if (j != 3)
                        break;
                }
                if (numIndexes == tri.numVerts)
                    break;
            } while (i != numTris);

            return numIndexes;
        }

        /*
        static void R_FlareDeform( drawSurf_t *surf ) {
            const srfTriangles_t *tri;
            srfTriangles_t		*newTri;
            idPlane	plane;
            float	dot;
            idVec3	localViewer;
            int		j;

            tri = surf.geo;

            if ( tri.numVerts != 4 || tri.numIndexes != 6 ) {
                //FIXME: temp hack for flares on tripleted models
                common.Warning( "R_FlareDeform: not a single quad" );
                return;
            }

            // this srfTriangles_t and all its indexes and caches are in frame
            // memory, and will be automatically disposed of
            newTri = (srfTriangles_t *)R_ClearedFrameAlloc( sizeof( *newTri ) );
            newTri.numVerts = 4;
            newTri.numIndexes = 2*3;
            newTri.indexes = (glIndex_t *)R_FrameAlloc( newTri.numIndexes * sizeof( newTri.indexes[0] ) );

            idDrawVert *ac = (idDrawVert *)_alloca16( newTri.numVerts * sizeof( idDrawVert ) );

            // find the plane
            plane.FromPoints( tri.verts[tri.indexes[0]].xyz, tri.verts[tri.indexes[1]].xyz, tri.verts[tri.indexes[2]].xyz );

            // if viewer is behind the plane, draw nothing
            R_GlobalPointToLocal( surf.space.modelMatrix, tr.viewDef.renderView.vieworg, localViewer );
            float distFromPlane = localViewer * plane.Normal() + plane[3];
            if ( distFromPlane <= 0 ) {
                newTri.numIndexes = 0;
                surf.geo = newTri;
                return;
            }

            idVec3	center;
            center = tri.verts[0].xyz;
            for ( j = 1 ; j < tri.numVerts ; j++ ) {
                center += tri.verts[j].xyz;
            }
            center *= 1.0/tri.numVerts;

            idVec3	dir = localViewer - center;
            dir.Normalize();

            dot = dir * plane.Normal();

            // set vertex colors based on plane angle
            int	color = (int)(dot * 8 * 256);
            if ( color > 255 ) {
                color = 255;
            }
            for ( j = 0 ; j < newTri.numVerts ; j++ ) {
                ac[j].color[0] =
                ac[j].color[1] =
                ac[j].color[2] = color;
                ac[j].color[3] = 255;
            }

            float	spread = surf.shaderRegisters[ surf.material.GetDeformRegister(0) ] * r_flareSize.GetFloat();
            idVec3	edgeDir[4][3];
            glIndex_t		indexes[MAX_TRI_WINDING_INDEXES];
            int		numIndexes = R_WindingFromTriangles( tri, indexes );

            surf.material = declManager.FindMaterial( "textures/smf/anamorphicFlare" );

            // only deal with quads
            if ( numIndexes != 4 ) {
                return;
            }

            // compute centroid
            idVec3 centroid, toeye, forward, up, left;
            centroid.Set( 0, 0, 0 );
            for ( int i = 0; i < 4; i++ ) {
                centroid += tri.verts[ indexes[i] ].xyz;
            }
            centroid /= 4;

            // compute basis vectors
            up.Set( 0, 0, 1 );

            toeye = centroid - localViewer;
            toeye.Normalize();
            left = toeye.Cross( up );
            up = left.Cross( toeye );

            left = left * 40 * 6;
            up = up * 40;

            // compute flares
            struct flare_t {
                float	angle;
                float	length;
            };

            static flare_t flares[] = {
                { 0, 100 },
                { 90, 100 }
            };

            for ( int i = 0; i < 4; i++ ) {
                memset( ac + i, 0, sizeof( ac[i] ) );
            }

            ac[0].xyz = centroid - left;
            ac[0].st[0] = 0; ac[0].st[1] = 0;

            ac[1].xyz = centroid + up;
            ac[1].st[0] = 1; ac[1].st[1] = 0;

            ac[2].xyz = centroid + left;
            ac[2].st[0] = 1; ac[2].st[1] = 1;

            ac[3].xyz = centroid - up;
            ac[3].st[0] = 0; ac[3].st[1] = 1;

            // setup colors
            for ( j = 0 ; j < newTri.numVerts ; j++ ) {
                ac[j].color[0] =
                ac[j].color[1] =
                ac[j].color[2] = 255;
                ac[j].color[3] = 255;
            }

            // setup indexes
            static glIndex_t	triIndexes[2*3] = {
                0,1,2,  0,2,3
            };

            memcpy( newTri.indexes, triIndexes, sizeof( triIndexes ) );

            R_FinishDeform( surf, newTri, ac );
        }
        */


#if true
        static GlIndex[] R_FlareDeform_triIndexes = {
            0,4,5,  0,5,6, 0,6,7, 0,7,1, 1,7,8, 1,8,9,
            15,4,0, 15,0,3, 3,0,1, 3,1,2, 2,1,9, 2,9,10,
            14,15,3, 14,3,13, 13,3,2, 13,2,12, 12,2,11, 11,2,10
        };
#else
        newTri.numIndexes = 12;
        static GlIndex[] R_FlareDeform_triIndexes = {
            0,1,2, 0,2,3, 0,4,5,0,5,6
        };
#endif
        static void R_FlareDeform(DrawSurf surf)
        {
            int j; float dot;
            SrfTriangles tri, newTri; Plane plane = new();

            tri = surf.geoFrontEnd;

            if (tri.numVerts != 4 || tri.numIndexes != 6)
            {
                // FIXME: temp hack for flares on tripleted models
                common.DPrintf("R_FlareDeform: not a single quad\n");
                return;
            }

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = 16;
            newTri.numIndexes = 18 * 3;
            newTri.indexes = R_FrameAllocMany<GlIndex>(newTri.numIndexes);

            var ac = stackalloc DrawVert[newTri.numVerts];

            // find the plane
            if (!plane.FromPoints(tri.verts[tri.indexes[0]].xyz, tri.verts[tri.indexes[1]].xyz, tri.verts[tri.indexes[2]].xyz))
            {
                common.Warning("R_FlareDeform: plane.FromPoints failed");
                return;
            }

            // if viewer is behind the plane, draw nothing
            R_GlobalPointToLocal(surf.space.modelMatrix, tr.viewDef.renderView.vieworg, out var localViewer);
            var distFromPlane = localViewer * plane.Normal + plane[3];
            if (distFromPlane <= 0)
            {
                newTri.numIndexes = 0;
                surf.geoFrontEnd = newTri;
                return;
            }

            var center = tri.verts[0].xyz;
            for (j = 1; j < tri.numVerts; j++)
                center += tri.verts[j].xyz;
            center *= 1f / tri.numVerts;

            var dir = localViewer - center;
            dir.Normalize();

            dot = dir * plane.Normal;

            // set vertex colors based on plane angle
            var color = (int)(dot * 8 * 256);
            if (color > 255)
                color = 255;
            for (j = 0; j < newTri.numVerts; j++)
            {
                ac[j].color[0] = ac[j].color[1] = ac[j].color[2] = color;
                ac[j].color[3] = 255;
            }

            var spread = surf.shaderRegisters[surf.material.GetDeformRegister(0)] * r_flareSize.Float;
            Vector3[][] edgeDir[4][3];
            GlIndex[] indexes = new GlIndex[MAX_TRI_WINDING_INDEXES];
            var numIndexes = R_WindingFromTriangles(tri, indexes);

            // only deal with quads
            if (numIndexes != 4)
                return;

            int i;
            // calculate vector directions
            for (i = 0; i < 4; i++)
            {
                ac[i].xyz = tri.verts[indexes[i]].xyz;
                ac[i].st.x = ac[i].st.y = 0.5f;

                var toEye = tri.verts[indexes[i]].xyz - localViewer;
                toEye.Normalize();

                var d1 = tri.verts[indexes[(i + 1) % 4]].xyz - localViewer;
                d1.Normalize();
                edgeDir[i][1].Cross(toEye, d1);
                edgeDir[i][1].Normalize();
                edgeDir[i][1] = Vector3.origin - edgeDir[i][1];

                var d2 = tri.verts[indexes[(i + 3) % 4]].xyz - localViewer;
                d2.Normalize();
                edgeDir[i][0].Cross(toEye, d2);
                edgeDir[i][0].Normalize();

                edgeDir[i][2] = edgeDir[i][0] + edgeDir[i][1];
                edgeDir[i][2].Normalize();
            }

            // build all the points
            ac[4].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0][0];
            ac[4].st.x = 0f; ac[4].st.y = 0.5f;

            ac[5].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0][2];
            ac[5].st.x = 0f; ac[5].st.y = 0f;

            ac[6].xyz = tri.verts[indexes[0]].xyz + spread * edgeDir[0][1];
            ac[6].st.x = 0.5f; ac[6].st.y = 0f;

            ac[7].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1][0];
            ac[7].st.x = 0.5f; ac[7].st.y = 0f;

            ac[8].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1][2];
            ac[8].st.x = 1f; ac[8].st.y = 0f;

            ac[9].xyz = tri.verts[indexes[1]].xyz + spread * edgeDir[1][1];
            ac[9].st.x = 1f; ac[9].st.y = 0.5f;

            ac[10].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2][0];
            ac[10].st.x = 1f; ac[10].st.y = 0.5f;

            ac[11].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2][2];
            ac[11].st.x = 1f; ac[11].st.y = 1f;

            ac[12].xyz = tri.verts[indexes[2]].xyz + spread * edgeDir[2][1];
            ac[12].st.x = 0.5f; ac[12].st.y = 1f;

            ac[13].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3][0];
            ac[13].st.x = 0.5f; ac[13].st.y = 1f;

            ac[14].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3][2];
            ac[14].st.x = 0f; ac[14].st.y = 1f;

            ac[15].xyz = tri.verts[indexes[3]].xyz + spread * edgeDir[3][1];
            ac[15].st.x = 0f; ac[15].st.y = 0.5f;

            for (i = 4; i < 16; i++)
            {
                dir = ac[i].xyz - localViewer;
                var len = dir.Normalize();
                var ang = dir * plane.Normal;
                // ac[i].xyz -= dir * spread * 2;
                var newLen = -(distFromPlane / ang);
                if (newLen > 0 && newLen < len)
                    ac[i].xyz = localViewer + dir * newLen;
                ac[i].st.x = 0f; ac[i].st.y = 0.5f;
            }

            fixed (void* d = newTri.indexes, s = triIndexes)
                Unsafe.CopyBlock(d, s, sizeof(triIndexes));

            R_FinishDeform(surf, newTri, ac);
        }

        // Expands the surface along it's normals by a shader amount
        static void R_ExpandDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this srfTriangles_t and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts];

            var dist = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            for (i = 0; i < tri.numVerts; i++)
            {
                ac[i] = tri.verts[i];
                ac[i].xyz = tri.verts[i].xyz + tri.verts[i].normal * dist;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        // Moves the surface along the X axis, mostly just for demoing the deforms
        static void R_MoveDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this SrfTriangles and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts];

            var dist = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            for (i = 0; i < tri.numVerts; i++)
            {
                ac[i] = tri.verts[i];
                ac[i].xyz[0] += dist;
            }

            R_FinishDeform(surf, newTri, ac);
        }

        //=====================================================================================

        // Turbulently deforms the XYZ, S, and T values
        static void R_TurbulentDeform(DrawSurf surf)
        {
            int i; SrfTriangles tri, newTri;

            tri = surf.geoFrontEnd;

            // this SrfTriangles and all its indexes and caches are in frame memory, and will be automatically disposed of
            newTri = R_ClearedFrameAlloc<SrfTriangles>();
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            newTri.indexes = tri.indexes;

            var ac = stackalloc DrawVert[newTri.numVerts];

            idDeclTable* table = (idDeclTable*)surf.material.GetDeformDecl();
            float range = surf.shaderRegisters[surf.material.GetDeformRegister(0)];
            float timeOfs = surf.shaderRegisters[surf.material.GetDeformRegister(1)];
            float domain = surf.shaderRegisters[surf.material.GetDeformRegister(2)];
            float tOfs = 0.5;

            for (i = 0; i < tri.numVerts; i++)
            {
                float f = tri.verts[i].xyz[0] * 0.003 + tri.verts[i].xyz[1] * 0.007 + tri.verts[i].xyz[2] * 0.011;

                f = timeOfs + domain * f;
                f += timeOfs;

                ac[i] = *(idDrawVert*)&tri.verts[i];

                ac[i].st[0] += range * table.TableLookup(f);
                ac[i].st[1] += range * table.TableLookup(f + tOfs);
            }

            R_FinishDeform(surf, newTri, ac);
        }

        //=====================================================================================

        /*
        =====================
        AddTriangleToIsland_r

        =====================
        */
#define MAX_EYEBALL_TRIS	10
#define MAX_EYEBALL_ISLANDS	6

        typedef struct {

    int tris[MAX_EYEBALL_TRIS];
        int numTris;
        idBounds bounds;
        idVec3 mid;
    }
    eyeIsland_t;

static void AddTriangleToIsland_r( const srfTriangles_t* tri, int triangleNum, bool* usedList, eyeIsland_t* island)
    {
        int a, b, c;

        usedList[triangleNum] = true;

        // add to the current island
        if (island.numTris == MAX_EYEBALL_TRIS)
        {
            common.Error("MAX_EYEBALL_TRIS");
        }
        island.tris[island.numTris] = triangleNum;
        island.numTris++;

        // recurse into all neighbors
        a = tri.indexes[triangleNum * 3];
        b = tri.indexes[triangleNum * 3 + 1];
        c = tri.indexes[triangleNum * 3 + 2];

        island.bounds.AddPoint(tri.verts[a].xyz);
        island.bounds.AddPoint(tri.verts[b].xyz);
        island.bounds.AddPoint(tri.verts[c].xyz);

        int numTri = tri.numIndexes / 3;
        for (int i = 0; i < numTri; i++)
        {
            if (usedList[i])
            {
                continue;
            }
            if (tri.indexes[i * 3 + 0] == a
                    || tri.indexes[i * 3 + 1] == a
                    || tri.indexes[i * 3 + 2] == a
                    || tri.indexes[i * 3 + 0] == b
                    || tri.indexes[i * 3 + 1] == b
                    || tri.indexes[i * 3 + 2] == b
                    || tri.indexes[i * 3 + 0] == c
                    || tri.indexes[i * 3 + 1] == c
                    || tri.indexes[i * 3 + 2] == c)
            {
                AddTriangleToIsland_r(tri, i, usedList, island);
            }
        }
    }

    /*
    =====================
    R_EyeballDeform

    Each eyeball surface should have an separate upright triangle behind it, long end
    pointing out the eye, and another single triangle in front of the eye for the focus point.
    =====================
    */
    static void R_EyeballDeform(drawSurf_t* surf)
    {
        int i, j, k;
        const srfTriangles_t* tri;
        srfTriangles_t* newTri;
        eyeIsland_t islands[MAX_EYEBALL_ISLANDS];
        int numIslands;
        bool triUsed[MAX_EYEBALL_ISLANDS * MAX_EYEBALL_TRIS];

        tri = surf.geoFrontEnd;

        // separate all the triangles into islands
        int numTri = tri.numIndexes / 3;
        if (numTri > MAX_EYEBALL_ISLANDS * MAX_EYEBALL_TRIS)
        {
            common.Printf("R_EyeballDeform: too many triangles in surface");
            return;
        }
        memset(triUsed, 0, sizeof(triUsed));

        for (numIslands = 0; numIslands < MAX_EYEBALL_ISLANDS; numIslands++)
        {
            islands[numIslands].numTris = 0;
            islands[numIslands].bounds.Clear();
            for (i = 0; i < numTri; i++)
            {
                if (!triUsed[i])
                {
                    AddTriangleToIsland_r(tri, i, triUsed, &islands[numIslands]);
                    break;
                }
            }
            if (i == numTri)
            {
                break;
            }
        }

        // assume we always have two eyes, two origins, and two targets
        if (numIslands != 3)
        {
            common.Printf("R_EyeballDeform: %i triangle islands\n", numIslands);
            return;
        }

        // this srfTriangles_t and all its indexes and caches are in frame
        // memory, and will be automatically disposed of

        // the surface cannot have more indexes or verts than the original
        newTri = (srfTriangles_t*)R_ClearedFrameAlloc(sizeof( *newTri) );
        memset(newTri, 0, sizeof( *newTri) );
        newTri.numVerts = tri.numVerts;
        newTri.numIndexes = tri.numIndexes;
        newTri.indexes = (glIndex_t*)R_FrameAlloc(tri.numIndexes * sizeof(newTri.indexes[0]));
        idDrawVert* ac = (idDrawVert*)_alloca16(tri.numVerts * sizeof(idDrawVert));

        newTri.numIndexes = 0;

        // decide which islands are the eyes and points
        for (i = 0; i < numIslands; i++)
        {
            islands[i].mid = islands[i].bounds.GetCenter();
        }

        for (i = 0; i < numIslands; i++)
        {
            eyeIsland_t* island = &islands[i];

            if (island.numTris == 1)
            {
                continue;
            }

            // the closest single triangle point will be the eye origin
            // and the next-to-farthest will be the focal point
            idVec3 origin, focus;
            int originIsland = 0;
            float dist[MAX_EYEBALL_ISLANDS];
            int sortOrder[MAX_EYEBALL_ISLANDS];

            for (j = 0; j < numIslands; j++)
            {
                idVec3 dir = islands[j].mid - island.mid;
                dist[j] = dir.Length();
                sortOrder[j] = j;
                for (k = j - 1; k >= 0; k--)
                {
                    if (dist[k] > dist[k + 1])
                    {
                        int temp = sortOrder[k];
                        sortOrder[k] = sortOrder[k + 1];
                        sortOrder[k + 1] = temp;
                        float ftemp = dist[k];
                        dist[k] = dist[k + 1];
                        dist[k + 1] = ftemp;
                    }
                }
            }

            originIsland = sortOrder[1];
            origin = islands[originIsland].mid;

            focus = islands[sortOrder[2]].mid;

            // determine the projection directions based on the origin island triangle
            idVec3 dir = focus - origin;
            dir.Normalize();

            const idVec3 &p1 = tri.verts[tri.indexes[islands[originIsland].tris[0] + 0]].xyz;
            const idVec3 &p2 = tri.verts[tri.indexes[islands[originIsland].tris[0] + 1]].xyz;
            const idVec3 &p3 = tri.verts[tri.indexes[islands[originIsland].tris[0] + 2]].xyz;

            idVec3 v1 = p2 - p1;
            v1.Normalize();
            idVec3 v2 = p3 - p1;
            v2.Normalize();

            // texVec[0] will be the normal to the origin triangle
            idVec3 texVec[2];

            texVec[0].Cross(v1, v2);

            texVec[1].Cross(texVec[0], dir);

            for (j = 0; j < 2; j++)
            {
                texVec[j] -= dir * (texVec[j] * dir);
                texVec[j].Normalize();
            }

            // emit these triangles, generating the projected texcoords

            for (j = 0; j < islands[i].numTris; j++)
            {
                for (k = 0; k < 3; k++)
                {
                    int index = islands[i].tris[j] * 3;

                    index = tri.indexes[index + k];
                    newTri.indexes[newTri.numIndexes++] = index;

                    ac[index].xyz = tri.verts[index].xyz;

                    idVec3 local = tri.verts[index].xyz - origin;

                    ac[index].st[0] = 0.5 + local * texVec[0];
                    ac[index].st[1] = 0.5 + local * texVec[1];
                }
            }
        }

        R_FinishDeform(surf, newTri, ac);
    }

    //==========================================================================================


    /*
    =====================
    R_ParticleDeform

    Emit particles from the surface instead of drawing it
    =====================
    */
    static void R_ParticleDeform(drawSurf_t* surf, bool useArea)
    {
        const struct renderEntity_s * renderEntity = &surf.space.entityDef.parms;
    const struct viewDef_s * viewDef = tr.viewDef;
    const idDeclParticle* particleSystem = (idDeclParticle*)surf.material.GetDeformDecl();

if (r_skipParticles.GetBool())
{
    return;
}

#if 0
	if ( renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] &&
	        viewDef.renderView.time*0.001 >= renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] ) {
		// the entire system has faded out
		return NULL;
	}
#endif

//
// calculate the area of all the triangles
//
int numSourceTris = surf.geoFrontEnd.numIndexes / 3;
float totalArea = 0;
float* sourceTriAreas = NULL;
const srfTriangles_t* srcTri = surf.geoFrontEnd;

if (useArea)
{
    sourceTriAreas = (float*)_alloca(sizeof( *sourceTriAreas) * numSourceTris );
    int triNum = 0;
    for (int i = 0; i < srcTri.numIndexes; i += 3, triNum++)
    {
        float area;
        area = idWinding::TriangleArea(srcTri.verts[srcTri.indexes[i]].xyz, srcTri.verts[srcTri.indexes[i + 1]].xyz, srcTri.verts[srcTri.indexes[i + 2]].xyz);
        sourceTriAreas[triNum] = totalArea;
        totalArea += area;
    }
}

//
// create the particles almost exactly the way idRenderModelPrt does
//
particleGen_t g;

g.renderEnt = renderEntity;
g.renderView = &viewDef.renderView;
g.origin.Zero();
g.axis = mat3_identity;

for (int currentTri = 0; currentTri < ((useArea) ? 1 : numSourceTris); currentTri++)
{

    for (int stageNum = 0; stageNum < particleSystem.stages.Num(); stageNum++)
    {
        idParticleStage* stage = particleSystem.stages[stageNum];

        if (!stage.material)
        {
            continue;
        }
        if (!stage.cycleMsec)
        {
            continue;
        }
        if (stage.hidden)
        {       // just for gui particle editor use
            continue;
        }

        // we interpret stage.totalParticles as "particles per map square area"
        // so the systems look the same on different size surfaces
        int totalParticles = (useArea) ? stage.totalParticles * totalArea / 4096.0 : (stage.totalParticles);

        int count = totalParticles * stage.NumQuadsPerParticle();

        // allocate a srfTriangles in temp memory that can hold all the particles
        srfTriangles_t* tri;

        tri = (srfTriangles_t*)R_ClearedFrameAlloc(sizeof( *tri) );
tri.numVerts = 4 * count;
tri.numIndexes = 6 * count;
tri.verts = (idDrawVert*)R_FrameAlloc(tri.numVerts * sizeof(tri.verts[0]));
tri.indexes = (glIndex_t*)R_FrameAlloc(tri.numIndexes * sizeof(tri.indexes[0]));

// just always draw the particles
tri.bounds = stage.bounds;

tri.numVerts = 0;

idRandom steppingRandom, steppingRandom2;

int stageAge = g.renderView.time + renderEntity.shaderParms[SHADERPARM_TIMEOFFSET] * 1000 - stage.timeOffset * 1000;
int stageCycle = stageAge / stage.cycleMsec;

// some particles will be in this cycle, some will be in the previous cycle
steppingRandom.SetSeed(((stageCycle << 10) & idRandom::MAX_RAND) ^ (int)(renderEntity.shaderParms[SHADERPARM_DIVERSITY] * idRandom::MAX_RAND));
steppingRandom2.SetSeed((((stageCycle - 1) << 10) & idRandom::MAX_RAND) ^ (int)(renderEntity.shaderParms[SHADERPARM_DIVERSITY] * idRandom::MAX_RAND));

for (int index = 0; index < totalParticles; index++)
{
    g.index = index;

    // bump the random
    steppingRandom.RandomInt();
    steppingRandom2.RandomInt();

    // calculate local age for this index
    int bunchOffset = stage.particleLife * 1000 * stage.spawnBunching * index / totalParticles;

    int particleAge = stageAge - bunchOffset;
    int particleCycle = particleAge / stage.cycleMsec;
    if (particleCycle < 0)
    {
        // before the particleSystem spawned
        continue;
    }
    if (stage.cycles && particleCycle >= stage.cycles)
    {
        // cycled systems will only run cycle times
        continue;
    }

    if (particleCycle == stageCycle)
    {
        g.random = steppingRandom;
    }
    else
    {
        g.random = steppingRandom2;
    }

    int inCycleTime = particleAge - particleCycle * stage.cycleMsec;

    if (renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] &&
            g.renderView.time - inCycleTime >= renderEntity.shaderParms[SHADERPARM_PARTICLE_STOPTIME] * 1000)
    {
        // don't fire any more particles
        continue;
    }

    // supress particles before or after the age clamp
    g.frac = (float)inCycleTime / (stage.particleLife * 1000);
    if (g.frac < 0)
    {
        // yet to be spawned
        continue;
    }
    if (g.frac > 1.0)
    {
        // this particle is in the deadTime band
        continue;
    }

    //---------------
    // locate the particle origin and axis somewhere on the surface
    //---------------

    int pointTri = currentTri;

    if (useArea)
    {
        // select a triangle based on an even area distribution
        pointTri = idBinSearch_LessEqual<float>(sourceTriAreas, numSourceTris, g.random.RandomFloat() * totalArea);
    }

    // now pick a random point inside pointTri
    const idDrawVert* v1 = &srcTri.verts[srcTri.indexes[pointTri * 3 + 0]];
    const idDrawVert* v2 = &srcTri.verts[srcTri.indexes[pointTri * 3 + 1]];
    const idDrawVert* v3 = &srcTri.verts[srcTri.indexes[pointTri * 3 + 2]];

    float f1 = g.random.RandomFloat();
    float f2 = g.random.RandomFloat();
    float f3 = g.random.RandomFloat();

    float ft = 1.0f / (f1 + f2 + f3 + 0.0001f);

    f1 *= ft;
    f2 *= ft;
    f3 *= ft;

    g.origin = v1.xyz * f1 + v2.xyz * f2 + v3.xyz * f3;
    g.axis[0] = v1.tangents[0] * f1 + v2.tangents[0] * f2 + v3.tangents[0] * f3;
    g.axis[1] = v1.tangents[1] * f1 + v2.tangents[1] * f2 + v3.tangents[1] * f3;
    g.axis[2] = v1.normal * f1 + v2.normal * f2 + v3.normal * f3;

    //-----------------------

    // this is needed so aimed particles can calculate origins at different times
    g.originalRandom = g.random;

    g.age = g.frac * stage.particleLife;

    // if the particle doesn't get drawn because it is faded out or beyond a kill region,
    // don't increment the verts
    tri.numVerts += stage.CreateParticle(&g, tri.verts + tri.numVerts);
}

if (tri.numVerts > 0)
{
    // build the index list
    int indexes = 0;
    for (int i = 0; i < tri.numVerts; i += 4)
    {
        tri.indexes[indexes + 0] = i;
        tri.indexes[indexes + 1] = i + 2;
        tri.indexes[indexes + 2] = i + 3;
        tri.indexes[indexes + 3] = i;
        tri.indexes[indexes + 4] = i + 3;
        tri.indexes[indexes + 5] = i + 1;
        indexes += 6;
    }
    tri.numIndexes = indexes;
    tri.ambientCache = vertexCache.AllocFrameTemp(tri.verts, tri.numVerts * sizeof(idDrawVert), false);
    tri.indexCache = vertexCache.AllocFrameTemp(tri.indexes, tri.numIndexes * sizeof(glIndex_t), true);

    // add the drawsurf
    R_AddDrawSurf(tri, surf.space, renderEntity, stage.material, surf.scissorRect);
}
		}
	}
}

//========================================================================================

/*
=================
R_DeformDrawSurf
=================
*/
void R_DeformDrawSurf(drawSurf_t* drawSurf)
{
    if (!drawSurf.material)
    {
        return;
    }

    if (r_skipDeforms.GetBool())
    {
        return;
    }
    switch (drawSurf.material.Deform())
    {
        case DFRM_NONE:
            return;
        case DFRM_SPRITE:
            R_AutospriteDeform(drawSurf);
            break;
        case DFRM_TUBE:
            R_TubeDeform(drawSurf);
            break;
        case DFRM_FLARE:
            R_FlareDeform(drawSurf);
            break;
        case DFRM_EXPAND:
            R_ExpandDeform(drawSurf);
            break;
        case DFRM_MOVE:
            R_MoveDeform(drawSurf);
            break;
        case DFRM_TURB:
            R_TurbulentDeform(drawSurf);
            break;
        case DFRM_EYEBALL:
            R_EyeballDeform(drawSurf);
            break;
        case DFRM_PARTICLE:
            R_ParticleDeform(drawSurf, true);
            break;
        case DFRM_PARTICLE2:
            R_ParticleDeform(drawSurf, false);
            break;
    }
}
