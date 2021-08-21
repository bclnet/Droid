using System.Runtime.CompilerServices;
using System.Text;
using static System.NumericsX.Lib;

namespace System.NumericsX.Core
{
    public static class StringX
    {
        #region Color

        static readonly Vector4[] g_color_table =
        {
            new(0f, 0f, 0f, 1f),
            new(1f, 0f, 0f, 1f), // S_COLOR_RED
	        new(0f, 1f, 0f, 1f), // S_COLOR_GREEN
	        new(1f, 1f, 0f, 1f), // S_COLOR_YELLOW
	        new(0f, 0f, 1f, 1f), // S_COLOR_BLUE
	        new(0f, 1f, 1f, 1f), // S_COLOR_CYAN
	        new(1f, 0f, 1f, 1f), // S_COLOR_MAGENTA
	        new(1f, 1f, 1f, 1f), // S_COLOR_WHITE
	        new(0.5f, 0.5f, 0.5f, 1f), // S_COLOR_GRAY
	        new(0f, 0f, 0f, 1f), // S_COLOR_BLACK
	        new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
        };

        public unsafe static bool IsColor(byte* s, void* till)
            => s[0] == '^' && s != till && s[1] != ' ';
        public static bool IsColor(byte[] s, int offset)
            => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(StringBuilder s, int offset)
            => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(string s, int offset)
            => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';

        public static int ColorIndex(int c)
            => c & 15;

        public static Vector4 ColorForIndex(int i)
            => g_color_table[i & 15];

        #endregion

        /// <summary>
        /// Determines whether the specified s is numeric.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>
        ///   <c>true</c> if the specified s is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(string s)
        {
            var i = 0;
            if (s[i] == '-')
                i++;
            var dot = false;
            for (; i < s.Length; i++)
            {
                var c = s[i];
                if (!char.IsDigit(c))
                {
                    if ((c == '.') && !dot)
                    {
                        dot = true;
                        continue;
                    }
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Safe strncpy that ensures a trailing zero
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="destsize">The destsize.</param>
        public static void Copynz(byte[] dest, byte[] src, int destsize)
        {
            if (src != null)
            {
                common.Warning("Str::Copynz: NULL src");
                return;
            }
            if (destsize < 1)
            {
                common.Warning("Str::Copynz: destsize < 1");
                return;
            }
            Unsafe.CopyBlock(ref dest[0], ref src[0], (uint)destsize - 1);
            dest[destsize - 1] = 0;
        }

        public static bool CharIsPrintable(Key key)
        {
            throw new NotImplementedException();
        }
    }
}