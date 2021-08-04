using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public static class reinterpret
    {
        public static unsafe int cast_int(float v) => *(int*)&v;
        public static unsafe float cast_float(int v) => *(float*)&v;
        public static unsafe float cast_float(uint v) => *(float*)&v;

        public static unsafe Vector2 cast_vec2(Vector3 s) => *(Vector2*)&s;
        public static unsafe Vector2 cast_vec2(Vector4 s) => *(Vector2*)&s;
        
        public static unsafe Vector3 cast_vec3(Vector4 s) => *(Vector3*)&s;
        public static unsafe Vector3 cast_vec3(Vector5 s) => *(Vector3*)&s;
        public static unsafe Vector3 cast_vec3(Plane s) => *(Vector3*)&s;
        public static unsafe Vector3 cast_vec3(float* s, int index) => *(Vector3*)&s[index];

        public static unsafe Vector4 cast_vec4(Plane s) => *(Vector4*)&s;

        public static unsafe Vector5 cast_vec5(Vector3 s) => *(Vector5*)&s;

        public static unsafe Vector6 cast_vec6(float* s, int index) => *(Vector6*)&s[index];

        [StructLayout(LayoutKind.Explicit)]
        internal struct F2ui
        {
            [FieldOffset(0)] public float f;
            [FieldOffset(0)] public uint u;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MatrixX2
        {
            [FieldOffset(0)] public MatrixX x;
            [FieldOffset(0)] public Matrix2x2 x2;
            [FieldOffset(0)] public Matrix3x3 x3;
            [FieldOffset(0)] public Matrix4x4 x4;
            [FieldOffset(0)] public Matrix5x5 x5;
            [FieldOffset(0)] public Matrix6x6 x6;
        }
    }
}