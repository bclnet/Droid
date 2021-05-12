using System;
using System.Diagnostics;
using System.Numerics;

namespace Droid.Core
{
    // token types
    public static class VectorX
    {
        public static float LengthSqr(this Vector3 s) => (s.X * s.X + s.Y * s.Y + s.Z * s.Z);
    }
}