//using System.Collections.Generic;
//using static System.NumericsX.OpenStack.Gngine.Gngine;
//using static System.NumericsX.OpenStack.Gngine.Render.R;
//using GlIndex = System.Int32;

//namespace System.NumericsX.OpenStack.Gngine.Render
//{
//    public unsafe struct GuiModelSurface
//    {
//        public Material material;
//        public fixed float color[4];
//        public int firstVert;
//        public int numVerts;
//        public int firstIndex;
//        public int numIndexes;
//    }

//    public unsafe class GuiModel
//    {
//        GuiModelSurface surf;

//        List<GuiModelSurface> surfaces = new();
//        List<GlIndex> indexes = new();
//        List<DrawVert> verts = new();

//        public GuiModel()
//        {
//            indexes.Capacity = 1000;
//            verts.Capacity = 1000;
//        }

//        // Begins collecting draw commands into surfaces
//        public void Clear()
//        {
//            surfaces.Clear();
//            indexes.Clear();
//            verts.Clear();
//            AdvanceSurf();
//        }

//        public void WriteToDemo(VFileDemo demo)
//        {
//            int i, j;

//            var verts = this.verts.Ptr();
//            i = verts.Length; demo.WriteInt(i);
//            for (j = 0; j < i; j++)
//            {
//                demo.WriteVec3(verts[j].xyz);
//                demo.WriteVec2(verts[j].st);
//                demo.WriteVec3(verts[j].normal);
//                demo.WriteVec3(verts[j].tangents0);
//                demo.WriteVec3(verts[j].tangents1);
//                demo.WriteUnsignedChar(verts[j].color0);
//                demo.WriteUnsignedChar(verts[j].color1);
//                demo.WriteUnsignedChar(verts[j].color2);
//                demo.WriteUnsignedChar(verts[j].color3);
//            }

//            var indexes = this.indexes.Ptr();
//            i = indexes.Length; demo.WriteInt(i);
//            for (j = 0; j < i; j++) demo.WriteInt(indexes[j]);

//            var surfaces = this.surfaces.Ptr();
//            i = surfaces.Length; demo.WriteInt(i);
//            for (j = 0; j < i; j++)
//            {
//                ref GuiModelSurface surf = ref surfaces[j];
//                demo.WriteInt((int &)surf.material);
//                demo.WriteFloat(surf.color[0]);
//                demo.WriteFloat(surf.color[1]);
//                demo.WriteFloat(surf.color[2]);
//                demo.WriteFloat(surf.color[3]);
//                demo.WriteInt(surf.firstVert);
//                demo.WriteInt(surf.numVerts);
//                demo.WriteInt(surf.firstIndex);
//                demo.WriteInt(surf.numIndexes);
//                demo.WriteHashString(surf.material.Name);
//            }
//        }
//        public void ReadFromDemo(VFileDemo demo)
//        {
//            int i, j;

//            demo.ReadInt(out i); var verts = this.verts.SetNum(i, false);
//            for (j = 0; j < i; j++)
//            {
//                demo.ReadVec3(out verts[j].xyz);
//                demo.ReadVec2(out verts[j].st);
//                demo.ReadVec3(out verts[j].normal);
//                demo.ReadVec3(out verts[j].tangents0);
//                demo.ReadVec3(out verts[j].tangents1);
//                demo.ReadUnsignedChar(out verts[j].color0);
//                demo.ReadUnsignedChar(out verts[j].color1);
//                demo.ReadUnsignedChar(out verts[j].color2);
//                demo.ReadUnsignedChar(out verts[j].color3);
//            }

//            demo.ReadInt(out i); var indexes = this.indexes.SetNum(i, false);
//#if false
//            for (j = 0; j < i; j++) demo.ReadShort(out indexes[j]);
//#else
//            for (j = 0; j < i; j++) demo.ReadInt(out indexes[j]);
//#endif

//            demo.ReadInt(out i); var surfaces = this.surfaces.SetNum(i, false);
//            for (j = 0; j < i; j++)
//            {
//                ref GuiModelSurface surf = ref surfaces[j];
//                demo.ReadInt((int &)surf.material);
//                demo.ReadFloat(out surf.color[0]);
//                demo.ReadFloat(out surf.color[1]);
//                demo.ReadFloat(out surf.color[2]);
//                demo.ReadFloat(out surf.color[3]);
//                demo.ReadInt(out surf.firstVert);
//                demo.ReadInt(out surf.numVerts);
//                demo.ReadInt(out surf.firstIndex);
//                demo.ReadInt(out surf.numIndexes);
//                surf.material = declManager.FindMaterial(demo.ReadHashString());
//            }
//        }

//        void EmitSurface(GuiModelSurface surf, float[] modelMatrix, float[] modelViewMatrix, bool depthHack)
//        {
//            SrfTriangles tri;

//            if (surf.numVerts == 0) return;     // nothing in the surface

//            // copy verts and indexes
//            tri = R_ClearedFrameAlloc<SrfTriangles>();

//            tri.numIndexes = surf.numIndexes;
//            tri.numVerts = surf.numVerts;
//            tri.indexes = R_FrameAllocMany<GlIndex>(tri.numIndexes);
//            memcpy(tri.indexes, &indexes[surf.firstIndex], tri.numIndexes * sizeof(tri.indexes[0]));

//            // we might be able to avoid copying these and just let them reference the list vars but some things, like deforms and recursive guis, need to access the verts in cpu space, not just through the vertex range
//            tri.verts = R_FrameAllocMany<DrawVert>(tri.numVerts);
//            memcpy(tri.verts, &verts[surf.firstVert], tri.numVerts * sizeof(tri.verts[0]));

//            // move the verts to the vertex cache
//            tri.ambientCache = vertexCache.AllocFrameTemp(tri.verts, tri.numVerts * sizeof(tri.verts[0]), false);
//            tri.indexCache = vertexCache.AllocFrameTemp(tri.indexes, tri.numIndexes * sizeof(GlIndex), true);

//            RenderEntity renderEntity;
//            memset(&renderEntity, 0, sizeof(renderEntity));
//            memcpy(renderEntity.shaderParms, surf.color, sizeof(surf.color));

//            var guiSpace = (ViewEntity)R_ClearedFrameAlloc(sizeof(ViewEntity));
//            memcpy(guiSpace.modelMatrix, modelMatrix, sizeof(guiSpace.modelMatrix));
//            memcpy(guiSpace.viewMatrix, modelViewMatrix, sizeof(guiSpace.viewMatrix));
//            guiSpace.weaponDepthHack = depthHack;

//            // add the surface, which might recursively create another gui
//            R_AddDrawSurf(tri, guiSpace, &renderEntity, surf.material, tr.viewDef.scissor);
//        }

//        public void EmitToCurrentView(float[] modelMatrix, bool depthHack)
//        {
//            var modelViewMatrix = stackalloc float[48];

//            myGlMultMatrix(modelMatrix, tr.viewDef.worldSpace.eyeViewMatrix[0], modelViewMatrix); // Left Eye
//            myGlMultMatrix(modelMatrix, tr.viewDef.worldSpace.eyeViewMatrix[1], modelViewMatrix + 16); // Right Eye
//            myGlMultMatrix(modelMatrix, tr.viewDef.worldSpace.eyeViewMatrix[2], modelViewMatrix + 32); // Center Eye

//            for (var i = 0; i < surfaces.Count; i++) EmitSurface(surfaces[i], modelMatrix, modelViewMatrix, depthHack);
//        }

//        // Creates a view that covers the screen and emit the surfaces
//        public void EmitFullScreen()
//        {
//            ViewDef viewDef;

//            if (surfaces[0].numVerts == 0) return;

//            viewDef = R_ClearedFrameAlloc<ViewDef>();

//            // for gui editor
//            if (tr.viewDef == null || !tr.viewDef.isEditor)
//            {
//                viewDef.renderView.x = 0; viewDef.renderView.y = 0;
//                viewDef.renderView.width = R.SCREEN_WIDTH; viewDef.renderView.height = R.SCREEN_HEIGHT;

//                tr.RenderViewToViewport(&viewDef.renderView, &viewDef.viewport);

//                viewDef.scissor.x1 = 0; viewDef.scissor.y1 = 0;
//                viewDef.scissor.x2 = viewDef.viewport.x2 - viewDef.viewport.x1; viewDef.scissor.y2 = viewDef.viewport.y2 - viewDef.viewport.y1;
//            }
//            else
//            {
//                viewDef.renderView.x = tr.viewDef.renderView.x; viewDef.renderView.y = tr.viewDef.renderView.y;
//                viewDef.renderView.width = tr.viewDef.renderView.width; viewDef.renderView.height = tr.viewDef.renderView.height;

//                viewDef.viewport.x1 = tr.viewDef.renderView.x; viewDef.viewport.x2 = tr.viewDef.renderView.x + tr.viewDef.renderView.width;
//                viewDef.viewport.y1 = tr.viewDef.renderView.y; viewDef.viewport.y2 = tr.viewDef.renderView.y + tr.viewDef.renderView.height;

//                viewDef.scissor.x1 = tr.viewDef.scissor.x1; viewDef.scissor.y1 = tr.viewDef.scissor.y1;
//                viewDef.scissor.x2 = tr.viewDef.scissor.x2; viewDef.scissor.y2 = tr.viewDef.scissor.y2;
//            }

//            viewDef.floatTime = tr.frameShaderTime;

//            // qglOrtho( 0, 640, 480, 0, 0, 1 );		// always assume 640x480 virtual coordinates
//            viewDef.projectionMatrix[0] = 2f / 640f;
//            viewDef.projectionMatrix[5] = -2f / 480f;
//            viewDef.projectionMatrix[10] = -2f / 1f;
//            viewDef.projectionMatrix[12] = -1f;
//            viewDef.projectionMatrix[13] = 1f;
//            viewDef.projectionMatrix[14] = -1f;
//            viewDef.projectionMatrix[15] = 1f;

//            for (var i = 0; i < 3; ++i)
//            {
//                viewDef.worldSpace.eyeViewMatrix[i][0] = 1f;
//                viewDef.worldSpace.eyeViewMatrix[i][5] = 1f;
//                viewDef.worldSpace.eyeViewMatrix[i][10] = 1f;
//                viewDef.worldSpace.eyeViewMatrix[i][15] = 1f;
//            }

//            viewDef.worldSpace.viewMatrix[0] = 1f;
//            viewDef.worldSpace.viewMatrix[5] = 1f;
//            viewDef.worldSpace.viewMatrix[10] = 1f;
//            viewDef.worldSpace.viewMatrix[15] = 1f;

//            viewDef.worldSpace.modelMatrix[0] = 1f;
//            viewDef.worldSpace.modelMatrix[5] = 1f;
//            viewDef.worldSpace.modelMatrix[10] = 1f;
//            viewDef.worldSpace.modelMatrix[15] = 1f;

//            viewDef.maxDrawSurfs = surfaces.Count;
//            viewDef.drawSurfs = (DrawSurf)R_FrameAlloc(viewDef.maxDrawSurfs * sizeof(DrawSurf));
//            viewDef.numDrawSurfs = 0;

//            var oldViewDef = tr.viewDef;
//            tr.viewDef = viewDef;

//            // add the surfaces to this view
//            for (var i = 0; i < surfaces.Count; i++) EmitSurface(surfaces[i], viewDef.worldSpace.modelMatrix, viewDef.worldSpace.viewMatrix, false);

//            tr.viewDef = oldViewDef;

//            // add the command to draw this view
//            R_AddDrawViewCmd(viewDef);
//        }

//        void AdvanceSurf()
//        {
//            GuiModelSurface s;

//            if (surfaces.Count != 0)
//            {
//                s.color[0] = surf.color[0];
//                s.color[1] = surf.color[1];
//                s.color[2] = surf.color[2];
//                s.color[3] = surf.color[3];
//                s.material = surf.material;
//            }
//            else
//            {
//                s.color[0] = 1;
//                s.color[1] = 1;
//                s.color[2] = 1;
//                s.color[3] = 1;
//                s.material = tr.defaultMaterial;
//            }
//            s.numIndexes = 0; s.firstIndex = indexes.Count;
//            s.numVerts = 0; s.firstVert = verts.Count;

//            surfaces.Add(s);
//            this.surf = surfaces[^1];
//        }

//        // these calls are forwarded from the renderer
//        public void SetColor(float r, float g, float b, float a)
//        {
//            if (!glConfig.isInitialized) return;
//            if (r == surf.color[0] && g == surf.color[1] && b == surf.color[2] && a == surf.color[3]) return; // no change

//            if (surf.numVerts != 0) AdvanceSurf();

//            // change the parms
//            surf.color[0] = r;
//            surf.color[1] = g;
//            surf.color[2] = b;
//            surf.color[3] = a;
//        }

//        public void DrawStretchPic(DrawVert[] verts, IList<GlIndex> indexes, int vertCount, int indexCount, Material hShader, bool clip = true, float min_x = 0f, float min_y = 0f, float max_x = 640f, float max_y = 480f)
//        {
//            if (!R.glConfig.isInitialized || verts == null || indexes == null || vertCount == 0 || indexCount == 0 || hShader = null) return;

//            // break the current surface if we are changing to a new material
//            if (hShader != surf.material)
//            {
//                if (surf.numVerts != 0) AdvanceSurf();
//                hShader.EnsureNotPurged();    // in case it was a gui item started before a level change
//                surf.material = hShader;
//            }

//            // add the verts and indexes to the current surface
//            if (clip)
//            {
//                int i, j;

//                // FIXME:	this is grim stuff, and should be rewritten if we have any significant number of guis asking for clipping
//                FixedWinding w = new();
//                for (i = 0; i < indexCount; i += 3)
//                {
//                    w.Clear();
//                    w.AddPoint(new Vector5(verts[indexes[i]].xyz.x, verts[indexes[i]].xyz.y, verts[indexes[i]].xyz.z, verts[indexes[i]].st.x, verts[indexes[i]].st.y));
//                    w.AddPoint(new Vector5(verts[indexes[i + 1]].xyz.x, verts[indexes[i + 1]].xyz.y, verts[indexes[i + 1]].xyz.z, verts[indexes[i + 1]].st.x, verts[indexes[i + 1]].st.y));
//                    w.AddPoint(new Vector5(verts[indexes[i + 2]].xyz.x, verts[indexes[i + 2]].xyz.y, verts[indexes[i + 2]].xyz.z, verts[indexes[i + 2]].st.x, verts[indexes[i + 2]].st.y));

//                    for (j = 0; j < 3; j++) if (w[j].x < min_x || w[j].x > max_x || w[j].y < min_y || w[j].y > max_y) break;
//                    if (j < 3)
//                    {
//                        Plane p = new();
//                        p.Normal.x = 1f; p.Normal.y = p.Normal.z = 0f; p.Dist = min_x; w.ClipInPlace(p);
//                        p.Normal.x = -1f; p.Normal.y = p.Normal.z = 0f; p.Dist = -max_x; w.ClipInPlace(p);
//                        p.Normal.x = p.Normal.z = 0f; p.Normal.y = 1f; p.Dist = min_y; w.ClipInPlace(p);
//                        p.Normal.x = p.Normal.z = 0f; p.Normal.y = -1f; p.Dist = -max_y; w.ClipInPlace(p);
//                    }

//                    var numVerts = this.verts.Count;
//                    var verts2 = this.verts.SetNum(numVerts + w.NumPoints, false);
//                    for (j = 0; j < w.NumPoints; j++)
//                    {
//                        ref DrawVert dv = ref verts2[numVerts + j];
//                        dv.xyz.x = w[j].x; dv.xyz.y = w[j].y; dv.xyz.z = w[j].z;
//                        dv.st.x = w[j].s; dv.st.y = w[j].t;
//                        dv.normal.Set(0, 0, 1);
//                        dv.tangents0.Set(1, 0, 0);
//                        dv.tangents1.Set(0, 1, 0);
//                    }
//                    surf.numVerts += w.NumPoints;

//                    for (j = 2; j < w.NumPoints; j++)
//                    {
//                        this.indexes.Add(numVerts - surf.firstVert);
//                        this.indexes.Add(numVerts + j - 1 - surf.firstVert);
//                        this.indexes.Add(numVerts + j - surf.firstVert);
//                        surf.numIndexes += 3;
//                    }
//                }
//            }
//            else
//            {
//                var numVerts = this.verts.Count;
//                var numIndexes = indexes.Count;

//                this.verts.AssureSize(numVerts + vertCount);
//                this.indexes.AssureSize(numIndexes + indexCount);

//                surf.numVerts += vertCount;
//                surf.numIndexes += indexCount;

//                for (var i = 0; i < indexCount; i++) this.indexes[numIndexes + i] = numVerts + indexes[i] - surf.firstVert;

//                memcpy(&this.verts[numVerts], verts, vertCount * sizeof(verts[0]));
//            }
//        }

//        // x/y/w/h are in the 0,0 to 640,480 range
//        public void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material hShader)
//        {
//            if (!glConfig.isInitialized || hShader == null) return;

//            var verts = stackalloc DrawVert[4];
//            var indexes = stackalloc GlIndex[6];

//            // clip to edges, because the pic may be going into a guiShader instead of full screen
//            if (x < 0) { s1 += (s2 - s1) * -x / w; w += x; x = 0; }
//            if (y < 0) { t1 += (t2 - t1) * -y / h; h += y; y = 0; }
//            if (x + w > 640) { s2 -= (s2 - s1) * (x + w - 640) / w; w = 640 - x; }
//            if (y + h > 480) { t2 -= (t2 - t1) * (y + h - 480) / h; h = 480 - y; }

//            if (w <= 0 || h <= 0) return;     // completely clipped away

//            indexes[0] = 3; indexes[1] = 0; indexes[2] = 2; indexes[3] = 2; indexes[4] = 0; indexes[5] = 1;
//            verts[0].xyz.x = x; verts[0].xyz.y = y; verts[0].xyz.z = 0;
//            verts[0].st.x = s1; verts[0].st.y = t1;
//            verts[0].normal.x = 0; verts[0].normal.y = 0; verts[0].normal.z = 1;
//            verts[0].tangents0.x = 1; verts[0].tangents0.y = 0; verts[0].tangents0.z = 0;
//            verts[0].tangents1.x = 0; verts[0].tangents1.y = 1; verts[0].tangents1.z = 0;
//            verts[1].xyz.x = x + w; verts[1].xyz.y = y; verts[1].xyz.z = 0;
//            verts[1].st.x = s2; verts[1].st.y = t1;
//            verts[1].normal.x = 0; verts[1].normal.y = 0; verts[1].normal.z = 1;
//            verts[1].tangents0.x = 1; verts[1].tangents0.y = 0; verts[1].tangents0.z = 0;
//            verts[1].tangents1.x = 0; verts[1].tangents1.y = 1; verts[1].tangents1.z = 0;
//            verts[2].xyz.x = x + w; verts[2].xyz.y = y + h; verts[2].xyz.z = 0;
//            verts[2].st.x = s2; verts[2].st.y = t2;
//            verts[2].normal.x = 0; verts[2].normal.y = 0; verts[2].normal.z = 1;
//            verts[2].tangents0.x = 1; verts[2].tangents0.y = 0; verts[2].tangents0.z = 0;
//            verts[2].tangents1.x = 0; verts[2].tangents1.y = 1; verts[2].tangents1.z = 0;
//            verts[3].xyz.x = x; verts[3].xyz.y = y + h; verts[3].xyz.z = 0;
//            verts[3].st.x = s1; verts[3].st.y = t2;
//            verts[3].normal.x = 0; verts[3].normal.y = 0; verts[3].normal.z = 1;
//            verts[3].tangents0.x = 1; verts[3].tangents0.y = 0; verts[3].tangents0.z = 0;
//            verts[3].tangents1.x = 0; verts[3].tangents1.y = 1; verts[3].tangents1.z = 0;

//            DrawStretchPic(&verts[0], &indexes[0], 4, 6, hShader, false, 0f, 0f, 640f, 480f);
//        }

//        // x/y/w/h are in the 0,0 to 640,480 range
//        public void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material)
//        {
//            var tempVerts = stackalloc DrawVert[3];
//            var tempIndexes = stackalloc GlIndex[3];
//            var vertCount = 3;
//            var indexCount = 3;

//            if (!R.glConfig.isInitialized || material == null) return;

//            tempIndexes[0] = 1; tempIndexes[1] = 0; tempIndexes[2] = 2;
//            tempVerts[0].xyz.x = p1.x; tempVerts[0].xyz.y = p1.y; tempVerts[0].xyz.z = 0;
//            tempVerts[0].st.x = t1.x; tempVerts[0].st.y = t1.y;
//            tempVerts[0].normal.x = 0; tempVerts[0].normal.y = 0; tempVerts[0].normal.z = 1;
//            tempVerts[0].tangents0.x = 1; tempVerts[0].tangents0.y = 0; tempVerts[0].tangents0.z = 0;
//            tempVerts[0].tangents1.x = 0; tempVerts[0].tangents1.y = 1; tempVerts[0].tangents1.z = 0;
//            tempVerts[1].xyz.x = p2.x; tempVerts[1].xyz.y = p2.y; tempVerts[1].xyz.z = 0;
//            tempVerts[1].st.x = t2.x; tempVerts[1].st.y = t2.y;
//            tempVerts[1].normal.x = 0;
//            tempVerts[1].normal.y = 0;
//            tempVerts[1].normal.z = 1;
//            tempVerts[1].tangents0.x = 1; tempVerts[1].tangents0.y = 0; tempVerts[1].tangents0.z = 0;
//            tempVerts[1].tangents1.x = 0; tempVerts[1].tangents1.y = 1; tempVerts[1].tangents1.z = 0;
//            tempVerts[2].xyz.x = p3.x; tempVerts[2].xyz.y = p3.y; tempVerts[2].xyz.z = 0;
//            tempVerts[2].st.x = t3.x; tempVerts[2].st.y = t3.y;
//            tempVerts[2].normal.x = 0; tempVerts[2].normal.y = 0; tempVerts[2].normal.z = 1;
//            tempVerts[2].tangents0.x = 1; tempVerts[2].tangents0.y = 0; tempVerts[2].tangents0.z = 0;
//            tempVerts[2].tangents1.x = 0; tempVerts[2].tangents1.y = 1; tempVerts[2].tangents1.z = 0;

//            // break the current surface if we are changing to a new material
//            if (material != surf.material)
//            {
//                if (surf.numVerts != 0) AdvanceSurf();
//                material.EnsureNotPurged();   // in case it was a gui item started before a level change
//                surf.material = material;
//            }

//            var numVerts = verts.Count;
//            var numIndexes = indexes.Count;

//            verts.AssureSize(numVerts + vertCount);
//            indexes.AssureSize(numIndexes + indexCount);

//            surf.numVerts += vertCount;
//            surf.numIndexes += indexCount;

//            for (var i = 0; i < indexCount; i++) indexes[numIndexes + i] = numVerts + tempIndexes[i] - surf.firstVert;

//            memcpy(&verts[numVerts], tempVerts, vertCount * sizeof(verts[0]));
//        }
//    }
//}
