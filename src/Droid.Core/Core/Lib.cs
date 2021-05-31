using System;
using System.Buffers.Binary;
using System.Diagnostics;

namespace Droid.Core
{
    public static class Lib
    {
        public static ISystem sys;
        //public static Common      common;
        public static ICVarSystem cvarSystem;
        public static IVFileSystem fileSystem;
        public static int frameNumber = 0;

        public static Action<string> FatalError;
        public static Action<string> Error;
        public static Action<string> Warning;
        public static Action<string> Printf;

        static unsafe void Init()
        {
            Debug.Assert(sizeof(bool) == 1);
            Debug.Assert(sizeof(float) == sizeof(int));
            Debug.Assert(sizeof(Vector3) == 3 * sizeof(float));

            // initialize generic SIMD implementation
            //SIMD.Init();

            // initialize math
            MathX.Init();

            // test idMatX
            //MathX.Test();

            // test idPolynomial
            Polynomial.Test();
        }

        static void ShutDown()
        {
            // shut down the SIMD engine
            //SIMD.Shutdown();
        }

        // basic colors
        public static readonly Vector4 colorBlack = new(0.00f, 0.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorWhite = new(1.00f, 1.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorRed = new(1.00f, 0.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorGreen = new(0.00f, 1.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorBlue = new(0.00f, 0.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorYellow = new(1.00f, 1.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorMagenta = new(1.00f, 0.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorCyan = new(0.00f, 1.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorOrange = new(1.00f, 0.50f, 0.00f, 1.00f);
        public static readonly Vector4 colorPurple = new(0.60f, 0.00f, 0.60f, 1.00f);
        public static readonly Vector4 colorPink = new(0.73f, 0.40f, 0.48f, 1.00f);
        public static readonly Vector4 colorBrown = new(0.40f, 0.35f, 0.08f, 1.00f);
        public static readonly Vector4 colorLtGrey = new(0.75f, 0.75f, 0.75f, 1.00f);
        public static readonly Vector4 colorMdGrey = new(0.50f, 0.50f, 0.50f, 1.00f);
        public static readonly Vector4 colorDkGrey = new(0.25f, 0.25f, 0.25f, 1.00f);

        public static readonly uint[] colorMask = new[] { 255U, 0U };

        static byte ColorFloatToByte(float c)
            => (byte)(((uint)(c * 255.0f)) & colorMask[MathX.FLOATSIGNBITSET(c) ? 1 : 0]);

        // wrapper to Common functions
        //public static void Error(string fmt);
        //public static void Warning(string fmt);

        // packs color floats in the range [0,1] into an integer
        public static uint PackColor(ref Vector3 color)
        {
            uint dx, dy, dz;

            dx = ColorFloatToByte(color.x);
            dy = ColorFloatToByte(color.y);
            dz = ColorFloatToByte(color.z);
#if !BIG_ENDIAN
            return (dx << 0) | (dy << 8) | (dz << 16);
#else
            return (dy << 16) | (dz << 8) | (dx << 0);
#endif
        }
        public static uint PackColor(ref Vector4 color)
        {
            uint dw, dx, dy, dz;

            dx = ColorFloatToByte(color.x);
            dy = ColorFloatToByte(color.y);
            dz = ColorFloatToByte(color.z);
            dw = ColorFloatToByte(color.w);
#if !BIG_ENDIAN
            return (dx << 0) | (dy << 8) | (dz << 16) | (dw << 24);
#else
            return (dx << 24) | (dy << 16) | (dz << 8) | (dw << 0);
#endif
        }

        public static void UnpackColor(uint color, ref Vector3 unpackedColor)
        {
#if !BIG_ENDIAN
            unpackedColor.Set(((color >> 0) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f));
#else
            unpackedColor.Set(((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 0) & 255) * (1.0f / 255.0f));
#endif
        }

        public static void UnpackColor(uint color, ref Vector4 unpackedColor)
        {
#if !BIG_ENDIAN
            unpackedColor.Set(((color >> 0) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 24) & 255) * (1.0f / 255.0f));
#else
            unpackedColor.Set(((color >> 24) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 0) & 255) * (1.0f / 255.0f));
#endif
        }

        static float FloatSwap(float f)
        {
            var dat = new reinterpret.F2ui { f = f };
            dat.u = BinaryPrimitives.ReverseEndianness(dat.u);
            return dat.f;
        }

        static unsafe void RevBytesSwap(void* bp, int elsize, int elcount)
        {
            byte* p, q;

            p = (byte*)bp;
            if (elsize == 2)
            {
                q = p + 1;
                while (elcount-- != 0)
                {
                    *p ^= *q;
                    *q ^= *p;
                    *p ^= *q;
                    p += 2;
                    q += 2;
                }
                return;
            }
            while (elcount-- != 0)
            {
                q = p + elsize - 1;
                while (p < q)
                {
                    *p ^= *q;
                    *q ^= *p;
                    *p ^= *q;
                    ++p;
                    --q;
                }
                p += elsize >> 1;
            }
        }

        static unsafe void RevBitFieldSwap(void* bp, int elsize)
        {
            int i; byte* p; byte t, v;

            LittleRevBytes(bp, elsize, 1);

            p = (byte*)bp;
            while (elsize-- != 0)
            {
                v = *p;
                t = 0;
                for (i = 7; i != 0; i--)
                {
                    t <<= 1;
                    v >>= 1;
                    t |= (byte)(v & 1);
                }
                *p++ = t;
            }
        }

        // little/big endian conversion
        public static bool BigEndian
#if !BIG_ENDIAN
            => false;
#else
	        => true;
#endif
        public static short BigShort(short l)
#if !BIG_ENDIAN
            => BinaryPrimitives.ReverseEndianness(l);
#else
            => l;
#endif
        public static short LittleShort(short l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static int BigInt(int l)
#if !BIG_ENDIAN
            => BinaryPrimitives.ReverseEndianness(l);
#else
            => l;
#endif
        public static int LittleInt(int l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static float BigFloat(float l)
#if !BIG_ENDIAN
            => FloatSwap(l);
#else
	        => l;
#endif
        public static float LittleFloat(float l)
#if !BIG_ENDIAN
            => l;
#else
	        => FloatSwap(l);
#endif
        public static unsafe void BigRevBytes(void* bp, int elsize, int elcount)
#if !BIG_ENDIAN
           => RevBytesSwap(bp, elsize, elcount);
#else
            { }
#endif

        public static unsafe void LittleRevBytes(void* bp, int elsize, int elcount)
#if !BIG_ENDIAN
        { }
#else
	        => RevBytesSwap(bp, elsize, elcount);
#endif
        public static unsafe void LittleBitField(void* bp, int elsize)
#if !BIG_ENDIAN
        { }
#else
            => RevBitFieldSwap(bp, elsize);
#endif

        //// color escape character
        //internal const int C_COLOR_ESCAPE = '^';
        //internal const int C_COLOR_DEFAULT = '0';
        //internal const int C_COLOR_RED = '1';
        //internal const int C_COLOR_GREEN = '2';
        //internal const int C_COLOR_YELLOW = '3';
        //internal const int C_COLOR_BLUE = '4';
        //internal const int C_COLOR_CYAN = '5';
        //internal const int C_COLOR_MAGENTA = '6';
        //internal const int C_COLOR_WHITE = '7';
        //internal const int C_COLOR_GRAY = '8';
        //internal const int C_COLOR_BLACK = '9';

        //// color escape string
        //internal const string S_COLOR_DEFAULT = "^0";
        //internal const string S_COLOR_RED = "^1";
        //internal const string S_COLOR_GREEN = "^2";
        //internal const string S_COLOR_YELLOW = "^3";
        //internal const string S_COLOR_BLUE = "^4";
        //internal const string S_COLOR_CYAN = "^5";
        //internal const string S_COLOR_MAGENTA = "^6";
        //internal const string S_COLOR_WHITE = "^7";
        //internal const string S_COLOR_GRAY = "^8";
        //internal const string S_COLOR_BLACK = "^9";
    }
}