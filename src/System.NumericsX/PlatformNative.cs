using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public static partial class Platform
    {
        [Flags]
        public enum CPUID
        {
            NONE = 0x00000,
            UNSUPPORTED = 0x00001,    // unsupported (386/486)
            GENERIC = 0x00002,    // unrecognized processor
            MMX = 0x00010,    // Multi Media Extensions
			_3DNOW = 0x00020,  // 3DNow!
            SSE = 0x00040,    // Streaming SIMD Extensions
            SSE2 = 0x00080,   // Streaming SIMD Extensions 2
            SSE3 = 0x00100,   // Streaming SIMD Extentions 3 aka Prescott's New Instructions
            ALTIVEC = 0x00200 // AltiVec
        }

        // returns a selection of the CPUID_* flags
        public static GetProcessorIdDelegate GetProcessorId;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate CPUID GetProcessorIdDelegate();

        // sets the FPU precision
        public static FPU_SetPrecisionDelegate FPU_SetPrecision;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void FPU_SetPrecisionDelegate();

        // sets Flush-To-Zero mode
        public static FPU_SetFTZDelegate FPU_SetFTZ;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void FPU_SetFTZDelegate(bool enable);

        // sets Denormals-Are-Zero mode
        public static FPU_SetDAZDelegate FPU_SetDAZ;[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void FPU_SetDAZDelegate(bool enable);
    }
}