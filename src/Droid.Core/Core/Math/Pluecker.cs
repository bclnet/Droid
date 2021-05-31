using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Droid.Core
{
    public class Pluecker
    {
        internal float[] p = new float[6];

        //public Pluecker() { }
        public unsafe Pluecker(float[] a)
        {
            fixed (float* p = this.p, a_ = a)
                Unsafe.CopyBlock(p, a_, 6U * sizeof(float));
        }
        public Pluecker(Vector3 start, Vector3 end)
            => FromLine(start, end);
        public Pluecker(float a1, float a2, float a3, float a4, float a5, float a6)
        {
            p[0] = a1;
            p[1] = a2;
            p[2] = a3;
            p[3] = a4;
            p[4] = a5;
            p[5] = a6;
        }

        public float this[int index]
            => p[index];

        public static Pluecker operator -(Pluecker _)                                          // flips the direction
                => new(-_.p[0], -_.p[1], -_.p[2], -_.p[3], -_.p[4], -_.p[5]);
        public static Pluecker operator *(Pluecker _, float a)
            => new(_.p[0] * a, _.p[1] * a, _.p[2] * a, _.p[3] * a, _.p[4] * a, _.p[5] * a);
        public static Pluecker operator /(Pluecker _, float a)
        {
            Debug.Assert(a != 0f);
            var inva = 1f / a;
            return new(_.p[0] * inva, _.p[1] * inva, _.p[2] * inva, _.p[3] * inva, _.p[4] * inva, _.p[5] * inva);
        }
        public static float operator *(Pluecker _, Pluecker a)                     // permuted inner product
            => _.p[0] * a.p[4] + _.p[1] * a.p[5] + _.p[2] * a.p[3] + _.p[4] * a.p[0] + _.p[5] * a.p[1] + _.p[3] * a.p[2];
        public static Pluecker operator -(Pluecker _, Pluecker a)
            => new(_.p[0] - a[0], _.p[1] - a[1], _.p[2] - a[2], _.p[3] - a[3], _.p[4] - a[4], _.p[5] - a[5]);
        public static Pluecker operator +(Pluecker _, Pluecker a)
            => new(_.p[0] + a[0], _.p[1] + a[1], _.p[2] + a[2], _.p[3] + a[3], _.p[4] + a[4], _.p[5] + a[5]);


        public bool Compare(Pluecker a)                      // exact compare, no epsilon
            => (p[0] == a[0]) && (p[1] == a[1]) && (p[2] == a[2]) &&
            (p[3] == a[3]) && (p[4] == a[4]) && (p[5] == a[5]);
        public bool Compare(Pluecker a, float epsilon) // compare with epsilon
        {
            if (MathX.Fabs(p[0] - a[0]) > epsilon) return false;
            if (MathX.Fabs(p[1] - a[1]) > epsilon) return false;
            if (MathX.Fabs(p[2] - a[2]) > epsilon) return false;
            if (MathX.Fabs(p[3] - a[3]) > epsilon) return false;
            if (MathX.Fabs(p[4] - a[4]) > epsilon) return false;
            if (MathX.Fabs(p[5] - a[5]) > epsilon) return false;
            return true;
        }
        public static bool operator ==(Pluecker _, Pluecker a)                 // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Pluecker _, Pluecker a)                 // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Pluecker q && Compare(q);
        public override int GetHashCode()
            => p[0].GetHashCode();

        public void Set(float a1, float a2, float a3, float a4, float a5, float a6)
        {
            p[0] = a1;
            p[1] = a2;
            p[2] = a3;
            p[3] = a4;
            p[4] = a5;
            p[5] = a6;
        }

        public void Zero()
           => p[0] = p[1] = p[2] = p[3] = p[4] = p[5] = 0f;

        public void FromLine(Vector3 start, Vector3 end)           // pluecker from line
        {
            p[0] = start.x * end.y - end.x * start.y;
            p[1] = start.x * end.z - end.x * start.z;
            p[2] = start.x - end.x;
            p[3] = start.y * end.z - end.y * start.z;
            p[4] = start.z - end.z;
            p[5] = end.y - start.y;
        }

        public void FromRay(Vector3 start, Vector3 dir)            // pluecker from ray
        {
            p[0] = start.x * dir.y - dir.x * start.y;
            p[1] = start.x * dir.z - dir.x * start.z;
            p[2] = -dir.x;
            p[3] = start.y * dir.z - dir.y * start.z;
            p[4] = -dir.z;
            p[5] = dir.y;
        }

        /// <summary>
        /// pluecker coordinate for the intersection of two planes
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        public bool FromPlanes(Plane p1, Plane p2)         // pluecker from intersection of planes
        {
            p[0] = -(p1.c * -p2.d - p2.c * -p1.d);
            p[1] = -(p2.b * -p1.d - p1.b * -p2.d);
            p[2] = p1.b * p2.c - p2.b * p1.c;

            p[3] = -(p1.a * -p2.d - p2.a * -p1.d);
            p[4] = p1.a * p2.b - p2.a * p1.b;
            p[5] = p1.a * p2.c - p2.a * p1.c;

            return p[2] != 0f || p[5] != 0f || p[4] != 0f;
        }

        public bool ToLine(out Vector3 start, out Vector3 end)                 // pluecker to line
        {
            Vector3 dir1, dir2; float d;

            dir1.x = p[3];
            dir1.y = -p[1];
            dir1.z = p[0];

            dir2.x = -p[2];
            dir2.y = p[5];
            dir2.z = -p[4];

            d = dir2 * dir2;
            if (d == 0f)
            {
                start = end = default;
                return false; // pluecker coordinate does not represent a line
            }

            start = dir2.Cross(dir1) * (1f / d);
            end = start + dir2;
            return true;
        }

        public bool ToRay(out Vector3 start, out Vector3 dir)                  // pluecker to ray
        {
            Vector3 dir1; float d;

            dir1.x = p[3];
            dir1.y = -p[1];
            dir1.z = p[0];

            dir.x = -p[2];
            dir.y = p[5];
            dir.z = -p[4];

            d = dir * dir;
            if (d == 0f)
            {
                start = default;
                return false; // pluecker coordinate does not represent a line
            }

            start = dir.Cross(dir1) * (1f / d);
            return true;
        }

        public void ToDir(out Vector3 dir)                                 // pluecker to direction
        {
            dir.x = -p[2];
            dir.y = p[5];
            dir.z = -p[4];
        }

        public float PermutedInnerProduct(Pluecker a)          // pluecker permuted inner product
         => p[0] * a.p[4] + p[1] * a.p[5] + p[2] * a.p[3] + p[4] * a.p[0] + p[5] * a.p[1] + p[3] * a.p[2];

        /// <summary>
        /// calculates square of shortest distance between the two 3D lines represented by their pluecker coordinates
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public float Distance3DSqr(Pluecker a)                 // pluecker line distance
        {
            float d, s; Vector3 dir;

            dir.x = -a.p[5] * p[4] - a.p[4] * -p[5];
            dir.y = a.p[4] * p[2] - a.p[2] * p[4];
            dir.z = a.p[2] * -p[5] - -a.p[5] * p[2];
            if (dir.x == 0f && dir.y == 0f && dir.z == 0f)
                return -1f;   // FIXME: implement for parallel lines
            d = a.p[4] * (p[2] * dir.y - -p[5] * dir.x) +
                a.p[5] * (p[2] * dir.z - p[4] * dir.x) +
                a.p[2] * (-p[5] * dir.z - p[4] * dir.y);
            s = PermutedInnerProduct(a) / d;
            return dir * dir * (s * s);
        }

        public float Length                                      // pluecker length
            => (float)MathX.Sqrt(p[5] * p[5] + p[4] * p[4] + p[2] * p[2]);

        public float LengthSqr                                   // pluecker squared length
            => p[5] * p[5] + p[4] * p[4] + p[2] * p[2];

        public Pluecker Normalize()                                    // pluecker normalize
        {
            var d = LengthSqr;
            if (d == 0f)
                return this; // pluecker coordinate does not represent a line
            d = MathX.InvSqrt(d);
            return new(p[0] * d, p[1] * d, p[2] * d, p[3] * d, p[4] * d, p[5] * d);
        }

        public float NormalizeSelf()                                      // pluecker normalize
        {
            var l = LengthSqr;
            if (l == 0f)
                return l; // pluecker coordinate does not represent a line
            var d = MathX.InvSqrt(l);
            p[0] *= d;
            p[1] *= d;
            p[2] *= d;
            p[3] *= d;
            p[4] *= d;
            p[5] *= d;
            return d * l;
        }

        public static int Dimension
            => 6;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = this.p)
                return callback(_);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(_ => StringX.FloatArrayToString(_, Dimension, precision));

        public static Pluecker origin = new(0f, 0f, 0f, 0f, 0f, 0f);
    }
}