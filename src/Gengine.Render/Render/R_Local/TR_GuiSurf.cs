using Droid.Core;
using Droid.UI;
using System;
using static Droid.Core.Lib;

namespace Droid.Render
{
    partial class TRX
    {
        // Calculates two axis for the surface sutch that a point dotted against the axis will give a 0.0 to 1.0 range in S and T when inside the gui surface
        public static void R_SurfaceToTextureAxis(SrfTriangles tri, ref Vector3 origin, Vector3[] axis)
        {
            float[] d0 = new float[5], d1 = new float[5];
            float[] bounds0 = new float[2], bounds1 = new float[2], boundsOrg = new float[2];
            int i, j; float v, area, inva; DrawVert a, b, c;

            // find the bounds of the texture
            bounds0[0] = bounds0[1] = 999999;
            bounds1[0] = bounds1[1] = -999999;
            for (i = 0; i < tri.numVerts; i++)
                for (j = 0; j < 2; j++)
                {
                    v = tri.verts[i].st[j];
                    if (v < bounds0[j]) bounds0[j] = v;
                    if (v > bounds1[j]) bounds1[j] = v;
                }

            // use the floor of the midpoint as the origin of the surface, which will prevent a slight misalignment from throwing it an entire cycle off
            boundsOrg[0] = (float)Math.Floor((bounds0[0] + bounds1[0]) * 0.5f);
            boundsOrg[1] = (float)Math.Floor((bounds0[1] + bounds1[1]) * 0.5f);

            // determine the world S and T vectors from the first drawSurf triangle
            a = tri.verts[tri.indexes[0]]; b = tri.verts[tri.indexes[1]]; c = tri.verts[tri.indexes[2]];

            MathX.VectorSubtract(ref b.xyz, ref a.xyz, d0);
            d0[3] = b.st.x - a.st.x; d0[4] = b.st.y - a.st.y;
            MathX.VectorSubtract(ref c.xyz, ref a.xyz, d1);
            d1[3] = c.st.x - a.st.x; d1[4] = c.st.y - a.st.y;

            area = d0[3] * d1[4] - d0[4] * d1[3];
            if (area == 0.0)
            {
                origin.Zero();
                axis[0].Zero();
                axis[1].Zero();
                axis[2].Zero();
                return; // degenerate
            }
            inva = 1f / area;

            axis[0].x = (d0[0] * d1[4] - d0[4] * d1[0]) * inva;
            axis[0].y = (d0[1] * d1[4] - d0[4] * d1[1]) * inva;
            axis[0].z = (d0[2] * d1[4] - d0[4] * d1[2]) * inva;

            axis[1].x = (d0[3] * d1[0] - d0[0] * d1[3]) * inva;
            axis[1].y = (d0[3] * d1[1] - d0[1] * d1[3]) * inva;
            axis[1].z = (d0[3] * d1[2] - d0[2] * d1[3]) * inva;

            Plane plane = new();
            plane.FromPoints(a.xyz, b.xyz, c.xyz);
            axis[2].x = plane[0]; axis[2].y = plane[1]; axis[2].z = plane[2];

            // take point 0 and project the vectors to the texture origin
            MathX.VectorMA(ref a.xyz, boundsOrg[0] - a.st.x, ref axis[0], ref origin);
            MathX.VectorMA(ref origin, boundsOrg[1] - a.st.y, ref axis[1], ref origin);
        }

        // Create a texture space on the given surface and call the GUI generator to create quads for it.
        public static void R_RenderGuiSurf(IUserInterface gui, DrawSurf drawSurf)
        {
            Vector3 origin = new(); Vector3[] axis = new Vector3[3];

            // for testing the performance hit
            if (r_skipGuiShaders.Integer == 1)
                return;

            // don't allow an infinite recursion loop
            if (tr.guiRecursionLevel == 4)
                return;

            tr.pc.c_guiSurfs++;

            // create the new matrix to draw on this surface
            R_SurfaceToTextureAxis(drawSurf.geoFrontEnd, ref origin, axis);

            float[] guiModelMatrix = new float[16], modelMatrix = new float[16];

            guiModelMatrix[00] = axis[0].x / 640f;
            guiModelMatrix[04] = axis[1].x / 480f;
            guiModelMatrix[08] = axis[2].x;
            guiModelMatrix[12] = origin.x;

            guiModelMatrix[01] = axis[0].y / 640f;
            guiModelMatrix[05] = axis[1].y / 480f;
            guiModelMatrix[09] = axis[2].y;
            guiModelMatrix[13] = origin.y;

            guiModelMatrix[02] = axis[0].z / 640f;
            guiModelMatrix[06] = axis[1].z / 480f;
            guiModelMatrix[10] = axis[2].z;
            guiModelMatrix[14] = origin.z;

            guiModelMatrix[3] = 0f;
            guiModelMatrix[7] = 0f;
            guiModelMatrix[11] = 0f;
            guiModelMatrix[15] = 1f;

            myGlMultMatrix(guiModelMatrix, drawSurf.space.modelMatrix, modelMatrix);

            tr.guiRecursionLevel++;

            // call the gui, which will call the 2D drawing functions
            tr.guiModel.Clear();
            gui.Redraw(tr.viewDef.renderView.time);
            tr.guiModel.EmitToCurrentView(modelMatrix, drawSurf.space.weaponDepthHack);
            tr.guiModel.Clear();

            tr.guiRecursionLevel--;
        }

        // Reloads any guis that have had their file timestamps changed. An optional "all" parameter will cause all models to reload, even if they are not out of date.
        // Should we also reload the map models?
        public static void R_ReloadGuis_f(CmdArgs args)
        {
            if (string.Equals(args[1], "all", System.StringComparison.OrdinalIgnoreCase))
            {
                common.Printf("Reloading all gui files...\n");
                UIX.uiManager.Reload(true);
            }
            else
            {
                common.Printf("Checking for changed gui files...\n");
                UIX.uiManager.Reload(false);
            }
        }

        public static void R_ListGuis_f(CmdArgs args)
            => UIX.uiManager.ListGuis();
    }
}
