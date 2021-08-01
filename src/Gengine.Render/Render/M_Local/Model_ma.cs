using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Gengine.Render
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MaNodeHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string parent;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MaAttribHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        public int size;
    }

    public class MaTransform
    {
        public Vector3 translate;
        public Vector3 rotate;
        public Vector3 scale;
        public MaTransform parent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MaFace
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] edge;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] vertexNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] tVertexNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] vertexColors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public Vector3[] vertexNormals;
    }

    public struct MaMesh
    {
        // Transform to be applied
        public MaTransform transform;

        // Verts
        public int numVertexes;
        public Vector3[] vertexes;
        public int numVertTransforms;
        public Vector4[] vertTransforms;
        public int nextVertTransformIndex;

        // Texture Coordinates
        public int numTVertexes;
        public Vector2[] tvertexes;

        // Edges
        public int numEdges;
        public Vector3[] edges;

        // Colors
        public int numColors;
        public int[] colors;

        // Faces
        public int numFaces;
        public MaFace[] faces;

        // Normals
        public int numNormals;
        public Vector3[] normals;
        public bool normalsParsed;
        public int nextNormal;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MaMaterial
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        public float uOffset, vOffset;     // max lets you offset by material without changing texCoords
        public float uTiling, vTiling;     // multiply tex coords by this
        public float angle;                 // in clockwise radians
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MaObject
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        public int materialRef;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string materialName;

        public MaMesh mesh;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MaFileNode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)] public string path;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MaMaterialNode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;

        public MaMaterialNode child;
        public MaFileNode file;
    }

    public class MaModel
    {
        public DateTime timeStamp;
        public List<MaMaterial> materials;
        public List<MaObject> objects;
        public HashSet<MaTransform> transforms;

        // Material Resolution
        public HashSet<MaFileNode> fileNodes;
        public HashSet<MaMaterialNode> materialNodes;
    }

    public static partial class ModelX
    {
        public static MaModel MA_Load(string fileName) => throw new NotImplementedException();
        public static void MA_Free(ref MaModel ma) => throw new NotImplementedException();
    }
}
