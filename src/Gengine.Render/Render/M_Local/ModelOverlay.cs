using Droid.Core;
using System.Collections.Generic;

namespace Droid.Render
{
    public struct OverlayVertex
    {
        public int vertexNum;
        public float st[2];
    }

    public struct OverlaySurface
    {
        public int surfaceNum;
        public int surfaceId;
        public int numIndexes;
        public GlIndex[] indexes;
        public int numVerts;
        public OverlayVertex[] verts;
    }

    public struct OverlayMaterial
    {
        public Material material;
        public List<OverlaySurface> surfaces;
    }

    public class RenderModelOverlay
    {
        const int MAX_OVERLAY_SURFACES = 16;

        public RenderModelOverlay();

        public static RenderModelOverlay Alloc();
        public static void Free(ref RenderModelOverlay overlay);

        // Projects an overlay onto deformable geometry and can be added to a render entity to allow decals on top of dynamic models.
        // This does not generate tangent vectors, so it can't be used with light interaction shaders. Materials for overlays should always
        // be clamped, because the projected texcoords can run well off the texture since no new clip vertexes are generated.
        public void CreateOverlay(IRenderModel model, Plane[] localTextureAxis, Material material);

        // Creates new model surfaces for baseModel, which should be a static instantiation of a dynamic model.
        public void AddOverlaySurfacesToModel(IRenderModel baseModel);

        // Removes overlay surfaces from the model.
        public static void RemoveOverlaySurfacesFromModel(IRenderModel baseModel);

        public void ReadFromDemoFile(VFileDemo f);
        public void WriteToDemoFile(VFileDemo f);

        List<OverlayMaterial> materials;

        void FreeSurface(ref OverlaySurface surface);
    }
}