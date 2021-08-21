using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// https://benbowen.blog/post/fun_with_makeref/
namespace System.NumericsX.Core
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe static class UnsafeX
    {
        [DllImport("msvcrt.dll", EntryPoint = "memmove", SetLastError = false)] public static unsafe extern void MoveBlock(void* destination, void* source, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", SetLastError = false)] public static unsafe extern void CopyBlock(void* destination, void* source, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memset", SetLastError = false)] public static unsafe extern void InitBlock(void* destination, int c, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcmp", SetLastError = false)] public static unsafe extern int CompareBlock(void* b1, void* b2, int byteCount);

        public static void Swap<T>(ref T a, ref T b)
        {
            var c = a;
            a = b;
            b = c;
        }

        public static T ReadT<T>(byte[] buffer, int offset = 0)
        {
            throw new NotImplementedException();
        }
        public static T ReadTSize<T>(int sizeOf, byte[] buffer, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public static T[] ReadTArray<T>(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteGenericToPtr<T>(IntPtr dest, T value, int sizeOfT) where T : struct
        {
            var bytePtr = (byte*)dest;

            var valueref = __makeref(value);
            var valuePtr = (byte*)*((IntPtr*)&valueref);
            for (var i = 0; i < sizeOfT; ++i)
                bytePtr[i] = valuePtr[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadGenericFromPtr<T>(IntPtr source, int sizeOfT) where T : struct
        {
            var bytePtr = (byte*)source;

            T result = default;
            var resultRef = __makeref(result);
            var resultPtr = (byte*)*((IntPtr*)&resultRef);

            for (var i = 0; i < sizeOfT; ++i)
                resultPtr[i] = bytePtr[i];

            return result;
        }
    }
}