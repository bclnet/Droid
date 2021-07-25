using Droid.Core;
using System.Runtime.InteropServices;

namespace Droid.Render
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DecalProjectionInfo
	{
		public Vector3 projectionOrigin;
		public Bounds projectionBounds;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public Plane[] boundingPlanes;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Plane[] fadePlanes;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Plane[] textureAxis;
		public Material material;
		public bool parallel;
		public float fadeDepth;
		public int startTime;
		public bool force;
	}

	public class RenderModelDecal
	{
		const int NUM_DECAL_BOUNDING_PLANES = 6;

		public RenderModelDecal();

		public static RenderModelDecal Alloc();
		public static void Free(ref RenderModelDecal decal);

		// Creates decal projection info.
		public static bool CreateProjectionInfo(DecalProjectionInfo info, FixedWinding winding, Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime);

		// Transform the projection info from global space to local.
		public static void GlobalProjectionInfoToLocal(DecalProjectionInfo localInfo, DecalProjectionInfo info, Vector3 origin, Matrix3x3 axis);

		// Creates a deal on the given model.
		public void CreateDecal(IRenderModel model, DecalProjectionInfo localInfo);

		// Remove decals that are completely faded away.
		public static RenderModelDecal RemoveFadedDecals(RenderModelDecal decals, int time);

		// Updates the vertex colors, removing any faded indexes, then copy the verts to temporary vertex cache and adds a drawSurf.
		public void AddDecalDrawSurf(ViewEntity space);

		// Returns the next decal in the chain.
		public RenderModelDecal Next()
			=> nextDecal;

		public void ReadFromDemoFile(VFileDemo f);
		public void WriteToDemoFile(VFileDemo f);

		const int MAX_DECAL_VERTS = 40;
		const int MAX_DECAL_INDEXES = 60;

		Material material;
		SrfTriangles tri;
		DrawVert[] verts = new DrawVert[MAX_DECAL_VERTS];
		float[] vertDepthFade = new float[MAX_DECAL_VERTS];
		GlIndex[] indexes = new GlIndex[MAX_DECAL_INDEXES];
		int[] indexStartTime = new int[MAX_DECAL_INDEXES];
		RenderModelDecal nextDecal;

		// Adds the winding triangles to the appropriate decal in the chain, creating a new one if necessary.
		void AddWinding(Winding w, Material decalMaterial, Plane[] fadePlanes, float fadeDepth, int startTime);

		// Adds depth faded triangles for the winding to the appropriate decal in the chain, creating a new one if necessary.
		// The part of the winding at the front side of both fade planes is not faded. The parts at the back sides of the fade planes are faded with the given depth.
		void AddDepthFadedWinding(Winding w, Material decalMaterial, Plane[] fadePlanes, float fadeDepth, int startTime);
	}
}