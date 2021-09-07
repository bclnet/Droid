using System.Runtime.InteropServices;
using GlIndex = System.Int32;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    // this is used for calculating unsmoothed normals and tangents for deformed models
    public unsafe struct DominantTri
    {
        public GlIndex v2, v3;
        public fixed float normalizationScale[3];
    }
}
