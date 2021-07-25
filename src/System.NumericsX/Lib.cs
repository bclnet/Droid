using System.Buffers.Binary;
using System.Diagnostics;
using System.NumericsX.Core;
using System.NumericsX.Sys;

namespace System.NumericsX
{
    public unsafe delegate T FloatPtr<T>(float* ptr);
    public unsafe delegate void FloatPtr(float* ptr);

    public static class Lib
    {
        public static int frameNumber = 0;
        public static volatile int com_ticNumber;   //: sky (attach)		// 60 hz tics, incremented by async function

        public static ISystem sys = new SystemLocal();
        public static ICommon common;
        public static IConsole console;
        internal static CVarSystemLocal cvarSystemLocal = new(); public static ICVarSystem cvarSystem = cvarSystemLocal;
        internal static CmdSystemLocal cmdSystemLocal = new(); public static ICmdSystem cmdSystem = cmdSystemLocal;
        public static IVFileSystem fileSystem;
        public static ISession session;
        public static IUsercmdGen usercmdGen; //internal static CmdSystemLocal usercmdGenLocal = new(); public static IUsercmdGen usercmdGen = cmdSystemLocal;

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
            //MatrixX.Test();

            // test idPolynomial
            Polynomial.Test();
        }

        static void ShutDown()
        {
            // shut down the SIMD engine
            //SIMD.Shutdown();
        }

        #region Colors

        // color escape character
        public const int C_COLOR_ESCAPE = '^';
        public const int C_COLOR_DEFAULT = '0';
        public const int C_COLOR_RED = '1';
        public const int C_COLOR_GREEN = '2';
        public const int C_COLOR_YELLOW = '3';
        public const int C_COLOR_BLUE = '4';
        public const int C_COLOR_CYAN = '5';
        public const int C_COLOR_MAGENTA = '6';
        public const int C_COLOR_WHITE = '7';
        public const int C_COLOR_GRAY = '8';
        public const int C_COLOR_BLACK = '9';

        // color escape string
        public const string S_COLOR_DEFAULT = "^0";
        public const string S_COLOR_RED = "^1";
        public const string S_COLOR_GREEN = "^2";
        public const string S_COLOR_YELLOW = "^3";
        public const string S_COLOR_BLUE = "^4";
        public const string S_COLOR_CYAN = "^5";
        public const string S_COLOR_MAGENTA = "^6";
        public const string S_COLOR_WHITE = "^7";
        public const string S_COLOR_GRAY = "^8";
        public const string S_COLOR_BLACK = "^9";

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

        #endregion

        #region Endian

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
        public static short BigShort(ref short l)
#if !BIG_ENDIAN
            => l = BinaryPrimitives.ReverseEndianness(l);
#else
        { }
#endif
        public static short LittleShort(short l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleShort(ref short l)
#if !BIG_ENDIAN
        { }
#else
            => l = BinaryPrimitives.ReverseEndianness(l);
#endif
        public static int BigInt(int l)
#if !BIG_ENDIAN
            => BinaryPrimitives.ReverseEndianness(l);
#else
            => l;
#endif
        public static void BigInt(ref int l)
#if !BIG_ENDIAN
            => l = BinaryPrimitives.ReverseEndianness(l);
#else
        { }
#endif
        public static int LittleInt(int l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleInt(ref int l)
#if !BIG_ENDIAN
        { }
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static float BigFloat(float l)
#if !BIG_ENDIAN
            => FloatSwap(l);
#else
	        => l;
#endif
        public static void BigFloat(ref float l)
#if !BIG_ENDIAN
            => l = FloatSwap(l);
#else
        { }
#endif
        public static float LittleFloat(float l)
#if !BIG_ENDIAN
            => l;
#else
	        => FloatSwap(l);
#endif
        public static void LittleFloat(ref float l)
#if !BIG_ENDIAN
        { }
#else
	        => l = FloatSwap(l);
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

        public static void LittleVector3(ref Vector3 l)
        {
#if !BIG_ENDIAN
#else
            l.x = FloatSwap(l.x); l.y = FloatSwap(l.y); l.z = FloatSwap(l.z);
#endif
        }

        #endregion
    }
}