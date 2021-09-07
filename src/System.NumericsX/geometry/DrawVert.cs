using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public class DrawVert
    {
        public unsafe static readonly int SizeOf = Marshal.SizeOf<DrawVert>();
        public Vector3 xyz;
        public Vector2 st;
        public Vector3 normal;
        public Vector3 tangents0;
        public Vector3 tangents1;
        public byte color0;
        public byte color1;
        public byte color2;
        public byte color3;

        public DrawVert Clone()
            => (DrawVert)MemberwiseClone();

        public unsafe float this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < 5);
                fixed (float* p = &xyz.x)
                    return p[index];
            }
        }

        public void Clear()
        {
            xyz.Zero();
            st.Zero();
            normal.Zero();
            tangents0.Zero();
            tangents1.Zero();
            color0 = color1 = color2 = color3 = 0;
        }

        public void Lerp(DrawVert a, DrawVert b, float f)
        {
            xyz = a.xyz + f * (b.xyz - a.xyz);
            st = a.st + f * (b.st - a.st);
        }

        public unsafe void LerpAll(DrawVert a, DrawVert b, float f)
        {
            xyz = a.xyz + f * (b.xyz - a.xyz);
            st = a.st + f * (b.st - a.st);
            normal = a.normal + f * (b.normal - a.normal);
            tangents0 = a.tangents0 + f * (b.tangents0 - a.tangents0);
            tangents1 = a.tangents1 + f * (b.tangents1 - a.tangents1);
            color0 = (byte)(a.color0 + f * (b.color0 - a.color0));
            color1 = (byte)(a.color1 + f * (b.color1 - a.color1));
            color2 = (byte)(a.color2 + f * (b.color2 - a.color2));
            color3 = (byte)(a.color3 + f * (b.color3 - a.color3));
        }

        public void Normalize()
        {
            normal.Normalize();
            tangents1.Cross(normal, tangents0);
            tangents1.Normalize();
            tangents0.Cross(tangents1, normal);
            tangents0.Normalize();
        }

        //public uint Color
        //{
        //    get => color;
        //    set => color = value;
        //}

        public void SetTexCoord(Vector2 st)
        {
            TexCoordS = st.x;
            TexCoordT = st.y;
        }
        public void SetTexCoord(float s, float t)
        {
            TexCoordS = s;
            TexCoordT = t;
        }

        public Vector2 TexCoord
            => new(MathXX.F16toF32(st.x), MathXX.F16toF32(st.y));

        public float TexCoordS
        {
            get => MathXX.F16toF32(st.x);
            set => st.x = MathXX.F32toF16(value);
        }

        public float TexCoordT
        {
            get => MathXX.F16toF32(st.y);
            set => st.y = MathXX.F32toF16(value);
        }
    }

    static class MathXX
    {
        // GPU half-float bit patterns
        static int HF_MANTISSA(ushort x) => x & 1023;
        static int HF_EXP(ushort x) => (x & 32767) >> 10;
        static int HF_SIGN(ushort x) => (x & 32768) != 0 ? -1 : 1;

        public unsafe static float F16toF32(float f) //: opt
        {
            var x = *(ushort*)&f; //: added
            var e = HF_EXP(x);
            var m = HF_MANTISSA(x);
            var s = HF_SIGN(x);
            if (0 < e && e < 31) return s * (float)Math.Pow(2f, e - 15f) * (1 + m / 1024f);
            else if (m == 0) return s * 0f;
            return s * (float)Math.Pow(2f, -14f) * (m / 1024f);
        }

        public static unsafe ushort F32toF16(float a)
        {
            var f = *(uint*)&a;
            var signbit = (f & 0x80000000) >> 16;
            var exponent = ((f & 0x7F800000) >> 23) - 112;
            var mantissa = f & 0x007FFFFF;
            if (exponent <= 0) return 0;
            if (exponent > 30) return (ushort)(signbit | 0x7BFF);
            return (ushort)(signbit | (exponent << 10) | (mantissa >> 13));
        }
    }
}
