using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Droid
{
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.CompilerServices.Unsafe/src/System.Runtime.CompilerServices.Unsafe.il
    public static class U
    {
        //[DllImport("msvcrt.dll", SetLastError = false)] public static unsafe extern void memset(void* dest, int c, int byteCount);
        //[DllImport("msvcrt.dll", SetLastError = false)] public static unsafe extern void memcpy(void* dest, void* src, int count);

        //public static unsafe T MarshalT<T>(void* data)
        //    => Marshal.PtrToStructure<T>(new IntPtr(data));

        public static unsafe T[] MarshalTArray<T>(void* data, int count)
        {
            var result = new T[count];
            var hresult = GCHandle.Alloc(result, GCHandleType.Pinned);
            Unsafe.CopyBlock((void*)hresult.AddrOfPinnedObject(), data, (uint)count);
            //memcpy((void*)hresult.AddrOfPinnedObject(), data, count);
            hresult.Free();
            return result;
        }

        //public static void Swap<T>(ref T a, ref T b)
        //{
        //    var c = a;
        //    a = b;
        //    b = c;
        //}
    }
}