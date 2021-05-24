using System.Runtime.InteropServices;

namespace Droid
{
    public static class U
    {
        [DllImport("msvcrt.dll", SetLastError = false)] public static unsafe extern void memset(void* dest, int c, int byteCount);
        [DllImport("msvcrt.dll", SetLastError = false)] public static unsafe extern void memcpy(void* dest, void* src, int count);

        //public static void Swap<T>(ref T a, ref T b)
        //{
        //    var c = a;
        //    a = b;
        //    b = c;
        //}
    }
}