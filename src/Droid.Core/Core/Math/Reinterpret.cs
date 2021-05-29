using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Droid.Core
{
    public static class reinterpret
    {
        public static unsafe int cast_int(float v) => *(int*)&v;
        public static unsafe float cast_float(int v) => *(float*)&v;
        public static unsafe float cast_float(uint v) => *(float*)&v;

        //public static Vector2 cast_vec2(ref Vector3 s) => new(s.x, s.y);
        //public static Vector2 cast_vec2(ref Vector4 s) => new(s.x, s.y);
        //public static Vector3 cast_vec3(ref Vector4 s) => new(s.x, s.y, s.z);
        //public static Vector3 cast_vec3(ref Vector5 s) => new(s.x, s.y, s.z);
        //public static Vector3 cast_vec3(ref Vector6 s, int index) => new(s.p[index], s.p[index + 1], s.p[index + 2]);
        //public static Vector3 cast_vec3(ref VectorX s, int index) => new(s.p[index], s.p[index + 1], s.p[index + 2]);
        //public static Vector6 cast_vec6(ref VectorX s, int index) => new(s.p[index], s.p[index + 1], s.p[index + 2], s.p[index + 3], s.p[index + 4], s.p[index + 5]);

        public static unsafe Vector2 cast_vec2(Vector3 s) => *(Vector2*)&s;
        public static unsafe Vector2 cast_vec2(Vector4 s) => *(Vector2*)&s;
        public static unsafe Vector3 cast_vec3(Vector4 s) => *(Vector3*)&s;
        public static unsafe Vector3 cast_vec3(Vector5 s) => *(Vector3*)&s;
        public static unsafe Vector3 cast_vec3(float* s, int index) => *(Vector3*)&s[index];
        public static unsafe Vector6 cast_vec6(float* s, int index) => *(Vector6*)&s[index];

        public static unsafe Matrix2x2 cast_mat2(float* s)
        {
            var mat = new Vector2[2];
            fixed (void* matp = mat)
                Unsafe.CopyBlock(matp, s, 2U * 2U * sizeof(float));
            return new() { mat = mat };
        }
        public static unsafe Matrix3x3 cast_mat3(float* s)
        {
            var mat = new Vector3[3];
            fixed (void* matp = mat)
                Unsafe.CopyBlock(matp, s, 3U * 3U * sizeof(float));
            return new() { mat = mat };
        }
        public static unsafe Matrix4x4 cast_mat4(float* s)
        {
            var mat = new Vector4[4];
            fixed (void* matp = mat)
                Unsafe.CopyBlock(matp, s, 4U * 4U * sizeof(float));
            return new() { mat = mat };
        }
        public static unsafe Matrix5x5 cast_mat5(float* s)
        {
            var mat = new Vector5[5];
            fixed (void* matp = mat)
                Unsafe.CopyBlock(matp, s, 5U * 5U * sizeof(float));
            return new() { mat = mat };
        }
        public static unsafe Matrix6x6 cast_mat6(float* s)
        {
            var mat = new Vector6[6];
            fixed (void* matp = mat)
                Unsafe.CopyBlock(matp, s, 6U * 6U * sizeof(float));
            return new() { mat = mat };
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct F2ui
        {
            [FieldOffset(0)] public uint i;
            [FieldOffset(0)] public float f;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct F2i
        {
            [FieldOffset(0)] public int i;
            [FieldOffset(0)] public float f;
        }
    }
}