using Droid.Core;
using System.Runtime.InteropServices;

namespace Droid.Render
{
    static class ModelXMd3
    {
        public const int MD3_IDENT = ('3' << 24) + ('P' << 16) + ('D' << 8) + 'I';
        public const int MD3_VERSION = 15;

        // surface geometry should not exceed these limits
        public const int SHADER_MAX_VERTEXES = 1000;
        public const int SHADER_MAX_INDEXES = 6 * SHADER_MAX_VERTEXES;

        // limits
        public const int MD3_MAX_LODS = 4;
        public const int MD3_MAX_TRIANGLES = 8192; // per surface
        public const int MD3_MAX_VERTS = 4096; // per surface
        public const int MD3_MAX_SHADERS = 256;        // per surface
        public const int MD3_MAX_FRAMES = 1024;    // per model
        public const int MD3_MAX_SURFACES = 32;        // per model
        public const int MD3_MAX_TAGS = 16;        // per frame
        public const int MAX_MD3PATH = 64;		// from quake3

        // vertex scales
        public const float MD3_XYZ_SCALE = 1f / 64;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Md3Frame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Vector3[] bounds;
        public Vector3 localOrigin;
        public float radius;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string name;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Md3Tag
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // tag name
        public Vector3 origin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public Vector3[] axis;
    }

    /*
    ** Md3Surface
    **
    ** CHUNK			SIZE
    ** header			sizeof( md3Surface_t )
    ** shaders			sizeof( md3Shader_t ) * numShaders
    ** triangles[0]		sizeof( md3Triangle_t ) * numTriangles
    ** st				sizeof( md3St_t ) * numVerts
    ** XyzNormals		sizeof( md3XyzNormal_t ) * numVerts * numFrames
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Md3Surface
    {
        public static readonly int SizeOf = (int)Marshal.OffsetOf(typeof(Md3Surface), "e_lfanew");

        public int ident;              //

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // polyset name

        public int flags;
        public int numFrames;          // all surfaces in a model should have the same

        public int numShaders;         // all surfaces in a model should have the same
        public int numVerts;

        public int numTriangles;
        public int ofsTriangles;

        public int ofsShaders;         // offset from start of md3Surface_t
        public int ofsSt;              // texture coords are common for all frames
        public int ofsXyzNormals;      // numVerts * numFrames

        public int ofsEnd;             // next surface follows

        // data
        public Md3Shader[] shaders;
        public Md3Triangle[] tris;
        public Md3St[] sts;
        public Md3XyzNormal[] xyzs;
        
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Md3Shader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name;
        public Material shader;         // for in-game use
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Md3Triangle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] indexes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Md3St
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] st;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Md3XyzNormal
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public short[] xyz;
        public short normal;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Md3Header
    {
        public static readonly int SizeOf = (int)Marshal.OffsetOf(typeof(Md3Surface), "frames");

        public int ident;
        public int version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // model name

        public int flags;

        public int numFrames;
        public int numTags;
        public int numSurfaces;

        public int numSkins;

        public int ofsFrames;          // offset for first frame
        public int ofsTags;            // numFrames * numTags
        public int ofsSurfaces;        // first surface, others follow

        public int ofsEnd;             // end of file

        // data
        public Md3Frame[] frames;
        public Md3Tag[] tags;
        public Md3Surface[] surfaces;
    }
}