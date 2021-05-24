using System;

namespace Droid.Core
{
    public class Plane
    {
        public const float ON_EPSILON = 0.1f;
        public const float DEGENERATE_DIST_EPSILON = 1e-4f;

        public const int SIDE_FRONT = 0;
        public const int SIDE_BACK = 1;
        public const int SIDE_ON = 2;
        public const int SIDE_CROSS = 3;

        // plane sides					
        public const int PLANESIDE_FRONT = 0;
        public const int PLANESIDE_BACK = 1;
        public const int PLANESIDE_ON = 2;
        public const int PLANESIDE_CROSS = 3;

        // plane types					
        public const int PLANETYPE_X = 0;
        public const int PLANETYPE_Y = 1;
        public const int PLANETYPE_Z = 2;
        public const int PLANETYPE_NEGX = 3;
        public const int PLANETYPE_NEGY = 4;
        public const int PLANETYPE_NEGZ = 5;
        public const int PLANETYPE_TRUEAXIAL = 6; // all types < 6 are true axial planes
        public const int PLANETYPE_ZEROX = 6;
        public const int PLANETYPE_ZEROY = 7;
        public const int PLANETYPE_ZEROZ = 8;
        public const int PLANETYPE_NONAXIAL = 9;

        //public Plane();
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

        public float this[int index]
            => index switch
            {
                0 => a,
                1 => b,
                2 => c,
                3 => d,
                _ => throw new ArgumentOutOfRangeException(),
            };

        public static Plane operator -(Plane _)                     // flips plane
            => new(-_.a, -_.b, -_.c, -_.d);
        //public static Plane operator=(Plane _, Vector3 v);          // sets normal and sets idPlane::d to zero
        public static Plane operator +(Plane _, Plane p)   // add plane equations
            => new(_.a + p.a, _.b + p.b, _.c + p.c, _.d + p.d);
        public static Plane operator -(Plane _, Plane p)   // subtract plane equations
            => new(_.a - p.a, _.b - p.b, _.c - p.c, _.d - p.d);
        //public static Plane operator *(Plane _, Matrix3x3 &m );			// Normal() *= m

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
            if (!Normal().Compare(p.Normal(), normalEps)) return false;
            return true;
        }
        public static bool operator ==(Plane _, Plane p)                   // exact compare, no epsilon
            => _.Compare(p);
        public static bool operator !=(Plane _, Plane p)                   // exact compare, no epsilon
            => !_.Compare(p);

        public void Zero()                         // zero plane
            => a = b = c = d = 0.0f;
        public void SetNormal(Vector3 normal)      // sets the normal
        {
            a = normal.x;
            b = normal.y;
            c = normal.z;
        }
        public Vector3 Normal()                    // reference to normal
            => MathX.reinterpret_cast_vec3(this);
        public float Normalize(bool fixDegenerate = true)  // only normalizes the plane normal, does not adjust d
        {
            var length = MathX.reinterpret_cast_vec3(this).Normalize();
            if (fixDegenerate)
                FixDegenerateNormal();
            return length;
        }
        public bool FixDegenerateNormal()          // fix degenerate normal
            => Normal().FixDegenerateNormal();
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
        public int Type();                      // returns plane type

        public bool FromPoints(Vector3 p1, Vector3 p2, Vector3 p3, bool fixDegenerate = true)
        {
            Normal() = (p1 - p2).Cross(p3 - p2);
            if (Normalize(fixDegenerate) == 0.0f)
                return false;
            d = -(Normal() * p2);
            return true;
        }
        public bool FromVecs(Vector3 dir1, Vector3 dir2, Vector3 p, bool fixDegenerate = true)
        {
            Normal() = dir1.Cross(dir2);
            if (Normalize(fixDegenerate) == 0.0f)
                return false;
            d = -(Normal() * p);
            return true;
        }

        public void FitThroughPoint(Vector3 p) // assumes normal is valid
             => d = -(Normal() * p);
        public bool HeightFit(Vector3[] points, int numPoints);
        public Plane Translate(Vector3 translation)
            => new(a, b, c, d - translation * Normal());
        public Plane TranslateSelf(Vector3 translation)
        {
            d -= translation * Normal();
            return this;
        }
        public Plane Rotate(Vector3 origin, Matrix3x3 axis)
        {
            var p = new Plane();
            p.Normal() = Normal() * axis;
            p.d = d + origin * Normal() - origin * p.Normal();
            return p;
        }
        public Plane RotateSelf(Vector3 origin, Matrix3x3 axis)
        {
            d += origin * Normal();
            Normal() *= axis;
            d -= origin * Normal();
            return *this;
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

            d1 = Normal() * start + d;
            d2 = Normal() * end + d;
            if (d1 == d2)
            {
                return false;
            }
            if (d1 > 0.0f && d2 > 0.0f)
            {
                return false;
            }
            if (d1 < 0.0f && d2 < 0.0f)
            {
                return false;
            }
            fraction = (d1 / (d1 - d2));
            return (fraction >= 0.0f && fraction <= 1.0f);
        }
        // intersection point is start + dir * scale
        public bool RayIntersection(Vector3 start, Vector3 dir, float scale)
        {
            float d1, d2;

            d1 = Normal() * start + d;
            d2 = Normal() * dir;
            if (d2 == 0.0f)
            {
                return false;
            }
            scale = -(d1 / d2);
            return true;
        }
        public bool PlaneIntersection(Plane plane, Vector3 start, Vector3 dir);

        public int Dimension
            => 4;

        public Vector4 ToVec4()
            => MathX.reinterpret_cast_vec4(this);
        public IntPtr ToFloatPtr() => throw new NotSupportedException();
        public string ToString(int precision = 2) => throw new NotImplementedException();

        float a;
        float b;
        float c;
        float d;

        public static Plane origin;
        //#define zero origin
    }
}