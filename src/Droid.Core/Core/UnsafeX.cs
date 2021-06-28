using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Droid.Core
{
    public unsafe delegate T FloatPtr<T>(float* ptr);
    public unsafe delegate void FloatPtr(float* ptr);

    [SuppressUnmanagedCodeSecurity]
    public static class UnsafeX
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
    }
}