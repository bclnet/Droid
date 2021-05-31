using System;

namespace Droid.Core
{
    // plane sides
    public enum PLANESIDE : int
    {
        FRONT = 0,
        BACK = 1,
        ON = 2,
        CROSS = 3,
    }

    // plane types
    public enum PLANETYPE : int
    {
        X = 0,
        Y = 1,
        Z = 2,
        NEGX = 3,
        NEGY = 4,
        NEGZ = 5,
        TRUEAXIAL = 6, // all types < 6 are true axial planes
        ZEROX = 6,
        ZEROY = 7,
        ZEROZ = 8,
        NONAXIAL = 9,
    }

    public struct Plane
    {
        public const float ON_EPSILON = 0.1f;
        public const float DEGENERATE_DIST_EPSILON = 1e-4f;

        public float a;
        public float b;
        public float c;
        public float d;

        //public const int SIDE_FRONT = 0;
        //public const int SIDE_BACK = 1;
        //public const int SIDE_ON = 2;
        //public const int SIDE_CROSS = 3;

        public Plane(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        public Plane(Vector3 normal, float dist)
        {
            this.a = normal.x;
            this.b = normal.y;
            this.c = normal.z;
            this.d = -dist;
        }
        public static implicit operator Plane(Vector3 v) // sets normal and sets Plane::d to zero
            => new()
            {
                a = v.x,
                b = v.y,
                c = v.z,
                d = 0,
            };

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &a)
                    return p[index];
            }
        }

        public static Plane operator -(Plane _)                     // flips plane
            => new(-_.a, -_.b, -_.c, -_.d);
        public static Plane operator +(Plane _, Plane p)   // add plane equations
            => new(_.a + p.a, _.b + p.b, _.c + p.c, _.d + p.d);
        public static Plane operator -(Plane _, Plane p)   // subtract plane equations
            => new(_.a - p.a, _.b - p.b, _.c - p.c, _.d - p.d);
        //public static Plane operator *(Plane _, Matrix3x3 &m );			// Normal *= m

        public bool Compare(Plane p)                      // exact compare, no epsilon
            => a == p.a && b == p.b && c == p.c && d == p.d;
        public bool Compare(Plane p, float epsilon) // compare with epsilon
        {
            if (MathX.Fabs(a - p.a) > epsilon) return false;
            if (MathX.Fabs(b - p.b) > epsilon) return false;
            if (MathX.Fabs(c - p.c) > epsilon) return false;
            if (MathX.Fabs(d - p.d) > epsilon) return false;
            return true;
        }
        public bool Compare(Plane p, float normalEps, float distEps)  // compare with epsilon
        {
            if (MathX.Fabs(d - p.d) > distEps) return false;
            if (!Normal.Compare(p.Normal, normalEps)) return false;
            return true;
        }
        public static bool operator ==(Plane _, Plane p)                   // exact compare, no epsilon
            => _.Compare(p);
        public static bool operator !=(Plane _, Plane p)                   // exact compare, no epsilon
            => !_.Compare(p);
        public override bool Equals(object obj)
            => obj is Plane q && Compare(q);
        public override int GetHashCode()
            => a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ d.GetHashCode();

        public void Zero()                         // zero plane
            => a = b = c = d = 0.0f;

        public void SetNormal(Vector3 normal)      // sets the normal
        {
            a = normal.x;
            b = normal.y;
            c = normal.z;
        }

        public Vector3 Normal
        {   // reference to normal
            get => reinterpret.cast_vec3(this);
            set { }
        }

        public float Normalize(bool fixDegenerate = true)  // only normalizes the plane normal, does not adjust d
        {
            var length = reinterpret.cast_vec3(this).Normalize();
            if (fixDegenerate)
                FixDegenerateNormal();
            return length;
        }

        public bool FixDegenerateNormal()          // fix degenerate normal
            => Normal.FixDegenerateNormal();

        public bool FixDegeneracies(float distEpsilon) // fix degenerate normal and dist
        {
            var fixedNormal = FixDegenerateNormal();
            // only fix dist if the normal was degenerate
            if (fixedNormal)
                if (MathX.Fabs(d - MathX.Rint(d)) < distEpsilon)
                    d = MathX.Rint(d);
            return fixedNormal;
        }

        public float Dist
        {
            get => -d; // returns: -d
            set => d = -value; // sets: d = -dist
        }

        public PLANETYPE Type                      // returns plane type
        {
            get
            {
                if (Normal[0] == 0.0f)
                {
                    if (Normal[1] == 0.0f) return Normal[2] > 0.0f ? PLANETYPE.Z : PLANETYPE.NEGZ;
                    else if (Normal[2] == 0.0f) return Normal[1] > 0.0f ? PLANETYPE.Y : PLANETYPE.NEGY;
                    else return PLANETYPE.ZEROX;
                }
                else if (Normal[1] == 0.0f)
                {
                    if (Normal[2] == 0.0f) return Normal[0] > 0.0f ? PLANETYPE.X : PLANETYPE.NEGX;
                    else return PLANETYPE.ZEROY;
                }
                else if (Normal[2] == 0.0f) return PLANETYPE.ZEROZ;
                else return PLANETYPE.NONAXIAL;
            }
        }

        public bool FromPoints(Vector3 p1, Vector3 p2, Vector3 p3, bool fixDegenerate = true)
        {
            Normal = (p1 - p2).Cross(p3 - p2);
            if (Normalize(fixDegenerate) == 0.0f)
                return false;
            d = -(Normal * p2);
            return true;
        }
        public bool FromVecs(Vector3 dir1, Vector3 dir2, Vector3 p, bool fixDegenerate = true)
        {
            Normal = dir1.Cross(dir2);
            if (Normalize(fixDegenerate) == 0.0f)
                return false;
            d = -(Normal * p);
            return true;
        }

        public void FitThroughPoint(Vector3 p) // assumes normal is valid
             => d = -(Normal * p);

        public bool HeightFit(Vector3[] points, int numPoints)
        {
            int i;
            float sumXX = 0.0f, sumXY = 0.0f, sumXZ = 0.0f;
            float sumYY = 0.0f, sumYZ = 0.0f;
            Vector3 sum = new(), average, dir;

            if (numPoints == 1)
            {
                a = 0.0f;
                b = 0.0f;
                c = 1.0f;
                d = -points[0].z;
                return true;
            }
            if (numPoints == 2)
            {
                dir = points[1] - points[0];
                Normal = dir.Cross(new Vector3(0, 0, 1)).Cross(dir);
                Normalize();
                d = -(Normal * points[0]);
                return true;
            }

            sum.Zero();
            for (i = 0; i < numPoints; i++)
                sum += points[i];
            average = sum / numPoints;

            for (i = 0; i < numPoints; i++)
            {
                dir = points[i] - average;
                sumXX += dir.x * dir.x;
                sumXY += dir.x * dir.y;
                sumXZ += dir.x * dir.z;
                sumYY += dir.y * dir.y;
                sumYZ += dir.y * dir.z;
            }

            Matrix2x2 m = new(sumXX, sumXY, sumXY, sumYY);
            if (!m.InverseSelf())
                return false;

            a = -sumXZ * m[0][0] - sumYZ * m[0][1];
            b = -sumXZ * m[1][0] - sumYZ * m[1][1];
            c = 1.0f;
            Normalize();
            d = -(a * average.x + b * average.y + c * average.z);
            return true;
        }

        public Plane Translate(Vector3 translation)
            => new(a, b, c, d - translation * Normal);
        public Plane TranslateSelf(Vector3 translation)
        {
            d -= translation * Normal;
            return this;
        }

        public Plane Rotate(Vector3 origin, Matrix3x3 axis)
        {
            Plane p = new();
            p.Normal = Normal * axis;
            p.d = d + origin * Normal - origin * p.Normal;
            return p;
        }
        public Plane RotateSelf(Vector3 origin, Matrix3x3 axis)
        {
            d += origin * Normal;
            Normal *= axis;
            d -= origin * Normal;
            return this;
        }

        public float Distance(Vector3 v)
            => a * v.x + b * v.y + c * v.z + d;

        public PLANESIDE Side(Vector3 v, float epsilon = 0.0f)
        {
            var dist = Distance(v);
            if (dist > epsilon) return PLANESIDE.FRONT;
            else if (dist < -epsilon) return PLANESIDE.BACK;
            else return PLANESIDE.ON;
        }

        public bool LineIntersection(Vector3 start, Vector3 end)
        {
            float d1, d2, fraction;

            d1 = Normal * start + d;
            d2 = Normal * end + d;
            if (d1 == d2) return false;
            if (d1 > 0.0f && d2 > 0.0f) return false;
            if (d1 < 0.0f && d2 < 0.0f) return false;
            fraction = d1 / (d1 - d2);
            return fraction >= 0.0f && fraction <= 1.0f;
        }

        // intersection point is start + dir * scale
        public bool RayIntersection(Vector3 start, Vector3 dir, float scale)
        {
            float d1, d2;

            d1 = Normal * start + d;
            d2 = Normal * dir;
            if (d2 == 0.0f)
                return false;
            scale = -(d1 / d2);
            return true;
        }

        public bool PlaneIntersection(Plane plane, Vector3 start, Vector3 dir)
        {
            double n00, n01, n11, det, invDet, f0, f1;

            n00 = Normal.LengthSqr;
            n01 = Normal * plane.Normal;
            n11 = plane.Normal.LengthSqr;
            det = n00 * n11 - n01 * n01;

            if (MathX.Fabs(det) < 1e-6f)
                return false;

            invDet = 1.0f / det;
            f0 = (n01 * plane.d - n11 * d) * invDet;
            f1 = (n01 * d - n00 * plane.d) * invDet;

            dir = Normal.Cross(plane.Normal);
            start = (float)f0 * Normal + (float)f1 * plane.Normal;
            return true;
        }

        public static int Dimension
            => 4;

        public Vector4 ToVec4()
            => reinterpret.cast_vec4(this);

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &a)
                return callback(_);
        }
        public unsafe string ToString(int precision = 2)
        {
            var dimension = Dimension;
            return ToFloatPtr(_ => StringX.FloatArrayToString(_, dimension, precision));
        }

        public static Plane origin = new(0.0f, 0.0f, 0.0f, 0.0f);
    }
}