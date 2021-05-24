//#define VECX_SIMD
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Droid.Core
{
    public unsafe delegate T FloatPtr<T>(float* ptr);

    public static partial class VectorX_
    {
        public const float EPSILON = 0.001f;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public void Set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public void Zero()
            => x = y = 0.0f;

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &x)
                    return p[index];
            }
            set
            {
                fixed (float* p = &x)
                    p[index] = value;
            }
        }
        public static Vector2 operator -(Vector2 _)
            => new(-_.x, -_.y);
        public static float operator *(Vector2 _, Vector2 a)
            => _.x * a.x + _.y * a.y;
        public static Vector2 operator *(Vector2 _, float a)
            => new(_.x * a, _.y * a);
        public static Vector2 operator /(Vector2 _, float a)
        { var inva = 1.0f / a; return new(_.x * inva, _.y * inva); }
        public static Vector2 operator +(Vector2 _, Vector2 a)
            => new(_.x + a.x, _.y + a.y);
        public static Vector2 operator -(Vector2 _, Vector2 a)
            => new(_.x - a.x, _.y - a.y);

        public static Vector2 operator *(float a, Vector2 b)
            => new(b.x * a, b.y * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(Vector2 a)
            => (x == a.x) && (y == a.y);
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(Vector2 a, float epsilon)
        {
            if (MathX.Fabs(x - a.x) > epsilon) return false;
            if (MathX.Fabs(y - a.y) > epsilon) return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector2 _, Vector2 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector2 _, Vector2 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector2 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode();

        public float Length
            => (float)MathX.Sqrt(x * x + y * y);
        public float LengthFast
        {
            get
            {
                var sqrLength = x * x + y * y;
                return sqrLength * MathX.RSqrt(sqrLength);
            }
        }
        public float LengthSqr
            => x * x + y * y;
        /// <summary>
        /// returns length
        /// </summary>
        /// <returns></returns>
        public float Normalize()
        {
            var sqrLength = x * x + y * y;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// returns length
        /// </summary>
        /// <returns></returns>
        public float NormalizeFast()
        {
            var lengthSqr = x * x + y * y;
            var invLength = MathX.RSqrt(lengthSqr);
            x *= invLength;
            y *= invLength;
            return invLength * lengthSqr;
        }
        /// <summary>
        /// cap length
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public Vector2 Truncate(float length)
        {
            if (length == 0)
                Zero();
            else
            {
                var length2 = LengthSqr;
                if (length2 > length * length)
                {
                    var ilength = length * MathX.InvSqrt(length2);
                    x *= ilength;
                    y *= ilength;
                }
            }
            return this;
        }
        public void Clamp(Vector2 min, Vector2 max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
        }
        /// <summary>
        /// snap to closest integer value
        /// </summary>
        public void Snap()
        {
            x = (float)Math.Floor(x + 0.5f);
            y = (float)Math.Floor(y + 0.5f);
        }
        /// <summary>
        /// snap towards integer (floor)
        /// </summary>
        public void SnapInt()
        {
            x = (int)x;
            y = (int)y;
        }

        public static int Dimension
            => 2;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(Vector2 v1, Vector2 v2, float l)
        {
            if (l <= 0.0f) this = v1;
            else if (l >= 1.0f) this = v2;
            else this = v1 + l * (v2 - v1);
        }

        public static Vector2 origin = new(0.0f, 0.0f);
        //#define zero origin
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float xyz)
            => x = y = z = xyz;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Zero()
            => x = y = z = 0.0f;

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &x)
                    return p[index];
            }
            set
            {
                fixed (float* p = &x)
                    p[index] = value;
            }
        }

        public static Vector3 operator -(Vector3 _)
            => new(-_.x, -_.y, -_.z);
        public static float operator *(Vector3 _, Vector3 a)
            => _.x * a.x + _.y * a.y + _.z * a.z;
        public static Vector3 operator *(Vector3 _, float a)
            => new(_.x * a, _.y * a, _.z * a);
        public static Vector3 operator /(Vector3 _, float a)
        {
            var inva = 1.0f / a;
            return new Vector3(_.x * inva, _.y * inva, _.z * inva);
        }
        public static Vector3 operator +(Vector3 _, Vector3 a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z);
        public static Vector3 operator -(Vector3 _, Vector3 a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z);

        public static Vector3 operator *(float a, Vector3 b)
            => new(b.x * a, b.y * a, b.z * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(Vector3 a)
            => (x == a.x) && (y == a.y) && (z == a.z);
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(Vector3 a, float epsilon)
        {
            if (MathX.Fabs(x - a.x) > epsilon) return false;
            else if (MathX.Fabs(y - a.y) > epsilon) return false;
            else if (MathX.Fabs(z - a.z) > epsilon) return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector3 _, Vector3 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector3 _, Vector3 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector3 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        /// <summary>
        /// fix degenerate axial cases
        /// </summary>
        /// <returns></returns>
        public bool FixDegenerateNormal()
        {
            if (x == 0.0f)
            {
                if (y == 0.0f)
                {
                    if (z > 0.0f) { if (z != 1.0f) { z = 1.0f; return true; } }
                    else { if (z != -1.0f) { z = -1.0f; return true; } }
                    return false;
                }
                else if (z == 0.0f)
                {
                    if (y > 0.0f) { if (y != 1.0f) { y = 1.0f; return true; } }
                    else { if (y != -1.0f) { y = -1.0f; return true; } }
                    return false;
                }
            }
            else if (y == 0.0f)
            {
                if (z == 0.0f)
                {
                    if (x > 0.0f) { if (x != 1.0f) { x = 1.0f; return true; } }
                    else { if (x != -1.0f) { x = -1.0f; return true; } }
                    return false;
                }
            }
            if (MathX.Fabs(x) == 1.0f)
            {
                if (y != 0.0f || z != 0.0f) { y = z = 0.0f; return true; }
                return false;
            }
            else if (MathX.Fabs(y) == 1.0f)
            {
                if (x != 0.0f || z != 0.0f) { x = z = 0.0f; return true; }
                return false;
            }
            else if (MathX.Fabs(z) == 1.0f)
            {
                if (x != 0.0f || y != 0.0f) { x = y = 0.0f; return true; }
                return false;
            }
            return false;
        }
        /// <summary>
        /// change tiny numbers to zero
        /// </summary>
        /// <returns></returns>
        public bool FixDenormals()
        {
            var denormal = false;
            if (Math.Abs(x) < 1e-30f) { x = 0.0f; denormal = true; }
            if (Math.Abs(y) < 1e-30f) { y = 0.0f; denormal = true; }
            if (Math.Abs(z) < 1e-30f) { z = 0.0f; denormal = true; }
            return denormal;
        }

        public Vector3 Cross(Vector3 a)
            => new(y * a.z - z * a.y, z * a.x - x * a.z, x * a.y - y * a.x);
        public Vector3 Cross(Vector3 a, Vector3 b)
        {
            x = a.y * b.z - a.z * b.y;
            y = a.z * b.x - a.x * b.z;
            z = a.x * b.y - a.y * b.x;
            return this;
        }
        public float Length
            => (float)MathX.Sqrt(x * x + y * y + z * z);
        public float LengthSqr
            => x * x + y * y + z * z;
        public float LengthFast
        {
            get
            {
                var sqrLength = x * x + y * y + z * z;
                return sqrLength * MathX.RSqrt(sqrLength);
            }
        }

        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        public float Normalize()
        {
            var sqrLength = x * x + y * y + z * z;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        public float NormalizeFast()
        {
            var sqrLength = x * x + y * y + z * z;
            var invLength = MathX.RSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// cap length
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public Vector3 Truncate(float length)
        {
            if (length == 0)
                Zero();
            else
            {
                var length2 = LengthSqr;
                if (length2 > length * length)
                {
                    var ilength = length * MathX.InvSqrt(length2);
                    x *= ilength;
                    y *= ilength;
                    z *= ilength;
                }
            }
            return this;
        }
        public void Clamp(Vector3 min, Vector3 max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
            if (z < min.z) z = min.z;
            else if (z > max.z) z = max.z;
        }
        /// <summary>
        /// snap to closest integer value
        /// </summary>
        public void Snap()
        {
            x = (float)Math.Floor(x + 0.5f);
            y = (float)Math.Floor(y + 0.5f);
            z = (float)Math.Floor(z + 0.5f);
        }
        /// <summary>
        /// snap towards integer (floor)
        /// </summary>
        public void SnapInt()
        {
            x = (int)x;
            y = (int)y;
            z = (int)z;
        }

        public static int Dimension
            => 3;

        public float ToYaw()
        {
            float yaw;
            if ((y == 0.0f) && (x == 0.0f))
                yaw = 0.0f;
            else
            {
                yaw = MathX.RAD2DEG((float)Math.Atan2(y, x));
                if (yaw < 0.0f)
                    yaw += 360.0f;
            }
            return yaw;
        }
        public float ToPitch()
        {
            float forward, pitch;
            if ((x == 0.0f) && (y == 0.0f))
                pitch = z > 0.0f ? 90.0f : 270.0f;
            else
            {
                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = MathX.RAD2DEG((float)Math.Atan2(z, forward));
                if (pitch < 0.0f)
                    pitch += 360.0f;
            }
            return pitch;
        }

        public Angles ToAngles()
        {
            float forward, yaw, pitch;
            if ((x == 0.0f) && (y == 0.0f))
            {
                yaw = 0.0f;
                pitch = z > 0.0f ? 90.0f : 270.0f;
            }
            else
            {
                yaw = MathX.RAD2DEG((float)Math.Atan2(y, x));
                if (yaw < 0.0f)
                    yaw += 360.0f;

                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = MathX.RAD2DEG((float)Math.Atan2(z, forward));
                if (pitch < 0.0f)
                    pitch += 360.0f;
            }

            return new Angles(-pitch, yaw, 0.0f);
        }

        public Polar3 ToPolar()
        {
            float forward, yaw, pitch;
            if ((x == 0.0f) && (y == 0.0f))
            {
                yaw = 0.0f;
                pitch = z > 0.0f ? 90.0f : 270.0f;
            }
            else
            {
                yaw = MathX.RAD2DEG((float)Math.Atan2(y, x));
                if (yaw < 0.0f)
                    yaw += 360.0f;

                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = MathX.RAD2DEG((float)Math.Atan2(z, forward));
                if (pitch < 0.0f)
                    pitch += 360.0f;
            }
            return new Polar3(MathX.Sqrt(x * x + y * y + z * z), yaw, -pitch);
        }
        /// <summary>
        /// vector should be normalized
        /// </summary>
        /// <returns></returns>
        public Matrix3x3 ToMatrix3x3()
        {
            var mat = new Matrix3x3();
            mat[0] = this;
            var d = x * x + y * y;
            if (d == 0)
            {
                mat[1].x = 1.0f;
                mat[1].y = 0.0f;
                mat[1].z = 0.0f;
            }
            else
            {
                d = MathX.InvSqrt(d);
                mat[1].x = -y * d;
                mat[1].y = x * d;
                mat[1].z = 0.0f;
            }
            mat[2] = Cross(mat[1]);
            return mat;
        }

        public Vector2 ToVec2()
            => reinterpret.cast_vec2(this);
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        /// <summary>
        /// vector should be normalized
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="down">Down.</param>
        public void NormalVectors(out Vector3 left, out Vector3 down)
        {
            var d = x * x + y * y;
            if (d == 0)
            {
                left.x = 1;
                left.y = 0;
                left.z = 0;
            }
            else
            {
                d = MathX.InvSqrt(d);
                left.x = -y * d;
                left.y = x * d;
                left.z = 0;
            }
            down = left.Cross(this);
        }
        public void OrthogonalBasis(out Vector3 left, out Vector3 up)
        {
            float l, s;
            if (MathX.Fabs(z) > 0.7f)
            {
                l = y * y + z * z;
                s = MathX.InvSqrt(l);
                up.x = 0;
                up.y = z * s;
                up.z = -y * s;
                left.x = l * s;
                left.y = -x * up[2];
                left.z = x * up[1];
            }
            else
            {
                l = x * x + y * y;
                s = MathX.InvSqrt(l);
                left.x = -y * s;
                left.y = x * s;
                left.z = 0;
                up.x = -z * left[1];
                up.y = z * left[0];
                up.z = l * s;
            }
        }

        public void ProjectOntoPlane(Vector3 normal, float overBounce = 1.0f)
        {
            var backoff = this * normal;
            if (overBounce != 1.0)
            {
                if (backoff < 0) backoff *= overBounce;
                else backoff /= overBounce;
            }
            this -= backoff * normal;
        }
        public bool ProjectAlongPlane(Vector3 normal, float epsilon, float overBounce = 1.0f)
        {
            var cross = Cross(normal).Cross(this);
            // normalize so a fixed epsilon can be used
            cross.Normalize();
            var len = normal * cross;
            if (MathX.Fabs(len) < epsilon)
                return false;
            cross *= overBounce * (normal * this) / len;
            this -= cross;
            return true;
        }
        /// <summary>
        /// Projects the z component onto a sphere.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public void ProjectSelfOntoSphere(float radius)
        {
            var rsqr = radius * radius;
            var len = Length;
            z = len < rsqr * 0.5f
                ? (float)Math.Sqrt(rsqr - len)
                : rsqr / (2.0f * (float)Math.Sqrt(len));
        }

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(Vector3 v1, Vector3 v2, float l)
        {
            if (l <= 0.0f) this = v1;
            else if (l >= 1.0f) this = v2;
            else this = v1 + l * (v2 - v1);
        }
        const float LERP_DELTA = 1e-6f;
        /// <summary>
        /// Spherical linear interpolation from v1 to v2.
        /// Vectors are expected to be normalized.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void SLerp(Vector3 v1, Vector3 v2, float l)
        {
            float omega, cosom, sinom, scale0, scale1;
            if (l <= 0.0f) { this = v1; return; }
            else if (l >= 1.0f) { this = v2; return; }

            cosom = v1 * v2;
            if ((1.0f - cosom) > LERP_DELTA)
            {
                omega = (float)Math.Acos(cosom);
                sinom = (float)Math.Sin(omega);
                scale0 = (float)Math.Sin((1.0f - l) * omega) / sinom;
                scale1 = (float)Math.Sin(l * omega) / sinom;
            }
            else
            {
                scale0 = 1.0f - l;
                scale1 = l;
            }

            this = v1 * scale0 + v2 * scale1;
        }

        public static Vector3 origin = new(0.0f, 0.0f, 0.0f);
        //#define zero origin
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public void Zero()
            => x = y = z = w = 0.0f;

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &x)
                    return p[index];
            }
            set
            {
                fixed (float* p = &x)
                    p[index] = value;
            }
        }

        public static Vector4 operator -(Vector4 _)
            => new(-_.x, -_.y, -_.z, -_.w);
        public static float operator *(Vector4 _, Vector4 a)
            => _.x * a.x + _.y * a.y + _.z * a.z + _.w * a.w;
        public static Vector4 operator *(Vector4 _, float a)
            => new(_.x * a, _.y * a, _.z * a, _.w * a);
        public static Vector4 operator /(Vector4 _, float a)
        {
            var inva = 1.0f / a;
            return new(_.x * inva, _.y * inva, _.z * inva, _.w * inva);
        }
        public static Vector4 operator +(Vector4 _, Vector4 a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z, _.w + a.w);
        public static Vector4 operator -(Vector4 _, Vector4 a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z, _.w - a.w);

        public static Vector4 operator *(float a, Vector4 b)
            => new(b.x * a, b.y * a, b.z * a, b.w * a);

        public bool Compare(Vector4 a)                          // exact compare, no epsilon
            => (x == a.x) && (y == a.y) && (z == a.z) && (w == a.w);
        public bool Compare(Vector4 a, float epsilon)     // compare with epsilon
        {
            if (MathX.Fabs(x - a.x) > epsilon) return false;
            else if (MathX.Fabs(y - a.y) > epsilon) return false;
            else if (MathX.Fabs(z - a.z) > epsilon) return false;
            else if (MathX.Fabs(w - a.w) > epsilon) return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector4 _, Vector4 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector4 _, Vector4 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector4 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();

        public float Length
            => (float)MathX.Sqrt(x * x + y * y + z * z + w * w);
        public float LengthSqr
            => x * x + y * y + z * z + w * w;

        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        public float Normalize()
        {
            var sqrLength = x * x + y * y + z * z + w * w;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        public float NormalizeFast()
        {
            var sqrLength = x * x + y * y + z * z + w * w;
            var invLength = MathX.RSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
            return invLength * sqrLength;
        }

        public static int Dimension
            => 4;

        public Vector2 ToVec2()
            => reinterpret.cast_vec2(this);
        public Vector3 ToVec3()
            => reinterpret.cast_vec3(this);
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(Vector4 v1, Vector4 v2, float l)
        {
            if (l <= 0.0f) this = v1;
            else if (l >= 1.0f) this = v2;
            else this = v1 + l * (v2 - v1);
        }

        public static Vector4 origin = new(0.0f, 0.0f, 0.0f, 0.0f);
        //#define zero origin
    }

    public struct Vector5
    {
        public float x;
        public float y;
        public float z;
        public float s;
        public float t;

        public Vector5(Vector3 xyz, Vector2 st)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
            s = st.x;
            t = st.y;
        }
        public Vector5(float x, float y, float z, float s, float t)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.s = s;
            this.t = t;
        }

        public float this[int index]
        {
            get => index switch
            {
                0 => x,
                1 => y,
                2 => z,
                3 => s,
                4 => t,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
            set
            {
                switch (index)
                {
                    case 0: x = value; return;
                    case 1: y = value; return;
                    case 2: z = value; return;
                    case 3: s = value; return;
                    case 4: t = value; return;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public static int Dimension
            => 5;

        public Vector3 ToVec3()
            => reinterpret.cast_vec3(this);
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &x)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        public void Lerp(Vector5 v1, Vector5 v2, float l)
        {
            if (l <= 0.0f) this = v1;
            else if (l >= 1.0f) this = v2;
            else
            {
                x = v1.x + l * (v2.x - v1.x);
                y = v1.y + l * (v2.y - v1.y);
                z = v1.z + l * (v2.z - v1.z);
                s = v1.s + l * (v2.s - v1.s);
                t = v1.t + l * (v2.t - v1.t);
            }
        }
        public static Vector5 origin = new(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        //#define zero origin
    }

    public unsafe struct Vector6
    {
        public Vector6(float[] a)
        {
            p[0] = a[0];
            p[1] = a[1];
            p[2] = a[2];
            p[3] = a[3];
            p[4] = a[4];
            p[5] = a[5];
        }
        public Vector6(float a1, float a2, float a3, float a4, float a5, float a6)
        {
            p[0] = a1;
            p[1] = a2;
            p[2] = a3;
            p[3] = a4;
            p[4] = a5;
            p[5] = a6;
        }

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
            => p[0] = p[1] = p[2] = p[3] = p[4] = p[5] = 0.0f;

        public float this[int index]
        {
            get => p[index];
            set => p[index] = value;
        }

        public static Vector6 operator -(Vector6 _)
            => new(-_.p[0], -_.p[1], -_.p[2], -_.p[3], -_.p[4], -_.p[5]);

        public static Vector6 operator *(Vector6 _, float a)
            => new(_.p[0] * a, _.p[1] * a, _.p[2] * a, _.p[3] * a, _.p[4] * a, _.p[5] * a);
        public static Vector6 operator /(Vector6 _, float a)
        {
            Debug.Assert(a != 0.0f);
            var inva = 1.0f / a;
            return new Vector6(_.p[0] * inva, _.p[1] * inva, _.p[2] * inva, _.p[3] * inva, _.p[4] * inva, _.p[5] * inva);
        }
        public static float operator *(Vector6 _, Vector6 a)
            => _.p[0] * a[0] + _.p[1] * a[1] + _.p[2] * a[2] + _.p[3] * a[3] + _.p[4] * a[4] + _.p[5] * a[5];
        public static Vector6 operator -(Vector6 _, Vector6 a)
            => new(_.p[0] - a[0], _.p[1] - a[1], _.p[2] - a[2], _.p[3] - a[3], _.p[4] - a[4], _.p[5] - a[5]);
        public static Vector6 operator +(Vector6 _, Vector6 a)
            => new(_.p[0] + a[0], _.p[1] + a[1], _.p[2] + a[2], _.p[3] + a[3], _.p[4] + a[4], _.p[5] + a[5]);

        public static Vector6 operator *(float a, Vector6 b)
            => b * a;

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(Vector6 a)
            => (p[0] == a[0]) && (p[1] == a[1]) && (p[2] == a[2]) && (p[3] == a[3]) && (p[4] == a[4]) && (p[5] == a[5]);
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(Vector6 a, float epsilon)
        {
            if (MathX.Fabs(p[0] - a[0]) > epsilon) return false;
            if (MathX.Fabs(p[1] - a[1]) > epsilon) return false;
            if (MathX.Fabs(p[2] - a[2]) > epsilon) return false;
            if (MathX.Fabs(p[3] - a[3]) > epsilon) return false;
            if (MathX.Fabs(p[4] - a[4]) > epsilon) return false;
            if (MathX.Fabs(p[5] - a[5]) > epsilon) return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector6 _, Vector6 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector6 _, Vector6 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector6 q && Compare(q);
        public override int GetHashCode()
            => p[0].GetHashCode();

        public float Length
            => (float)MathX.Sqrt(p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5]);
        public float LengthSqr
            => p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        public float Normalize()
        {
            var sqrLength = p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
            var invLength = MathX.InvSqrt(sqrLength);
            p[0] *= invLength;
            p[1] *= invLength;
            p[2] *= invLength;
            p[3] *= invLength;
            p[4] *= invLength;
            p[5] *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        public float NormalizeFast()
        {
            var sqrLength = p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
            var invLength = MathX.RSqrt(sqrLength);
            p[0] *= invLength;
            p[1] *= invLength;
            p[2] *= invLength;
            p[3] *= invLength;
            p[4] *= invLength;
            p[5] *= invLength;
            return invLength * sqrLength;
        }

        public static int Dimension
            => 6;

        public Vector3 SubVec3(int index)
        {
            fixed (float* p = this.p)
                return reinterpret.cast_vec3(p, index * 3);
        }
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* array = p)
                return callback(array);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        internal fixed float p[6];

        public static Vector6 origin = new(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        //#define zero origin
        public static Vector6 infinity = new(MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY);
    }

    public struct VectorX
    {
        const int VECX_MAX_TEMP = 1024;
        static unsafe int VECX_QUAD(int x) => ((x + 3) & ~3) * sizeof(float);
        void VECX_CLEAREND() { var s = size; while (s < ((s + 3) & ~3)) p[pi + s++] = 0.0f; }
        internal static float[] VECX_ALLOCA(int n) => new float[VECX_QUAD(n)];

        //public VectorX()
        //{
        //    size = alloced = 0;
        //    p = null;
        //}
        public VectorX(int length)
        {
            size = alloced = 0;
            p = null; pi = 0;
            SetSize(length);
        }
        public VectorX(int length, float[] data)
        {
            size = alloced = 0;
            p = null; pi = 0;
            SetData(length, data);
        }

        public float this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < size);
                return p[pi + index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < size);
                p[pi + index] = value;
            }
        }
        public static VectorX operator -(VectorX _)
        {
            var m = new VectorX();
            m.SetTempSize(_.size);
            for (var i = 0; i < _.size; i++)
                m.p[m.pi + i] = -_.p[_.pi + i];
            return m;
        }
        public static VectorX operator *(VectorX _, float a)
        {
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            SIMDProcessor.Mul16(m.p, _.p, a, _.size);
#else
            for (var i = 0; i < _.size; i++)
                m.p[m.pi + i] = _.p[_.pi + i] * a;
#endif
            return m;
        }
        public static VectorX operator /(VectorX _, float a)
        {
            Debug.Assert(a != 0.0f);
            return _ * (1.0f / a);
        }

        public static float operator *(VectorX _, VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var sum = 0.0f;
            for (var i = 0; i < _.size; i++)
                sum += _.p[_.pi + i] * a.p[a.pi + i];
            return sum;
        }
        public static VectorX operator -(VectorX _, VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            SIMDProcessor.Sub16(m.p, _.p, a.p, _.size);
#else
            for (var i = 0; i < _.size; i++)
                m.p[m.pi + i] = _.p[_.pi + i] - a.p[a.pi + i];
#endif
            return m;
        }
        public static VectorX operator +(VectorX _, VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            SIMDProcessor.Add16(m.p, _.p, a.p, _.size);
#else
            for (var i = 0; i < _.size; i++)
                m.p[m.pi + i] = _.p[_.pi + i] + a.p[a.pi + i];
#endif
            return m;
        }

        public static VectorX operator *(float a, VectorX b)
            => b * a;

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(VectorX a)
        {
            Debug.Assert(size == a.size);
            for (var i = 0; i < size; i++)
                if (p[pi + i] != a.p[a.pi + i])
                    return false;
            return true;
        }
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(VectorX a, float epsilon)
        {
            Debug.Assert(size == a.size);
            for (var i = 0; i < size; i++)
                if (MathX.Fabs(p[pi + i] - a.p[a.pi + i]) > epsilon)
                    return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(VectorX _, VectorX a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(VectorX _, VectorX a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is VectorX q && Compare(q);
        public override int GetHashCode()
            => p.GetHashCode();

        public void SetSize(int size)
        {
            var alloc = (size + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                p = new float[alloc];
                pi = 0;
                alloced = alloc;
            }
            this.size = size;
            VECX_CLEAREND();
        }
        public void ChangeSize(int size, bool makeZero = false)
        {
            var alloc = (size + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                var oldVec = p;
                p = new float[alloc];
                alloced = alloc;
                if (oldVec != null)
                    for (var i = 0; i < this.size; i++)
                        p[i] = oldVec[pi + i];
                pi = 0;
                // zero any new elements
                if (makeZero)
                    for (var i = size; i < size; i++)
                        p[i] = 0.0f;
            }
            this.size = size;
            VECX_CLEAREND();
        }
        public int Size
            => size;
        public void SetData(int length, float[] data, int index = 0)
        {
            //Debug.Assert((((uintptr_t)data) & 15) == 0); // data must be 16 byte aligned
            p = data;
            pi = index;
            size = length;
            alloced = -1;
            VECX_CLEAREND();
        }

        public void Zero()
        {
#if VECX_SIMD
            SIMDProcessor.Zero16(p, size);
#else
            Array.Clear(p, pi, size);
#endif
        }
        public void Zero(int length)
        {
            SetSize(length);
#if VECX_SIMD
            SIMDProcessor.Zero16(p, length);
#else
            Array.Clear(p, pi, size);
#endif
        }
        public void Random(int seed, float l = 0.0f, float u = 1.0f)
        {
            var rnd = new Random(seed);
            var c = u - l;
            for (var i = 0; i < size; i++)
                p[pi + i] = l + rnd.RandomFloat() * c;
        }
        public void Random(int length, int seed, float l = 0.0f, float u = 1.0f)
        {
            var rnd = new Random(seed);
            SetSize(length);
            var c = u - l;
            for (var i = 0; i < size; i++)
                p[pi + i] = l + rnd.RandomFloat() * c;
        }
        public void Negate()
        {
#if VECX_SIMD
            SIMDProcessor.Negate16(p, size);
#else
            for (var i = 0; i < size; i++)
                p[pi + i] = -p[pi + i];
#endif
        }
        public void Clamp(float min, float max)
        {
            for (var i = 0; i < size; i++)
            {
                if (p[pi + i] < min) p[pi + i] = min;
                else if (p[pi + i] > max) p[pi + i] = max;
            }
        }
        public VectorX SwapElements(int e1, int e2)
        {
            var tmp = p[pi + e1];
            p[pi + e1] = p[pi + e2];
            p[pi + e2] = tmp;
            return this;
        }

        public float Length
        {
            get
            {
                var sum = 0.0f;
                for (var i = 0; i < size; i++)
                    sum += p[pi + i] * p[pi + i];
                return MathX.Sqrt(sum);
            }
        }
        public float LengthSqr
        {
            get
            {
                var sum = 0.0f;
                for (var i = 0; i < size; i++)
                    sum += p[pi + i] * p[pi + i];
                return sum;
            }
        }
        public VectorX Normalize()
        {
            int i;
            var m = new VectorX();
            m.SetTempSize(size);
            var sum = 0.0f;
            for (i = 0; i < size; i++)
                sum += p[pi + i] * p[pi + i];
            var invSqrt = MathX.InvSqrt(sum);
            for (i = 0; i < size; i++)
                m.p[pi + i] = p[pi + i] * invSqrt;
            return m;
        }
        public float NormalizeSelf()
        {
            int i;
            var sum = 0.0f;
            for (i = 0; i < size; i++)
                sum += p[pi + i] * p[pi + i];
            var invSqrt = MathX.InvSqrt(sum);
            for (i = 0; i < size; i++)
                p[pi + i] *= invSqrt;
            return invSqrt * sum;
        }

        public int Dimension
            => size;

        public unsafe Vector3 SubVec3(int index)
        {
            Debug.Assert(index >= 0 && index * 3 + 3 <= size);
            fixed (float* p = this.p)
                return reinterpret.cast_vec3(p, pi + index * 3);
        }
        public unsafe Vector6 SubVec6(int index)
        {
            Debug.Assert(index >= 0 && index * 6 + 6 <= size);
            fixed (float* p = this.p)
                return reinterpret.cast_vec6(p, pi + index * 6);
        }
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = p)
                return callback(_ + pi);
        }
        public unsafe string ToString(int precision = 2)
        {
            var dimension = Dimension;
            return ToFloatPtr(array => StringX.FloatArrayToString(array, dimension, precision));
        }

        int size;                   // size of the vector
        int alloced;                // if -1 p points to data set with SetData
        internal float[] p;                       // memory the vector is stored
        internal int pi;

        static float[] temp = new float[VECX_MAX_TEMP + 4];   // used to store intermediate results
        static int tempPtr = 0; // (float *) ( ( (intptr_t)temp + 15 ) & ~15 );              // pointer to 16 byte aligned temporary memory
        static int tempIndex = 0;               // index into memory pool, wraps around

        internal unsafe void SetTempSize(int size)
        {
            this.size = size;
            alloced = (size + 3) & ~3;
            Debug.Assert(alloced < VECX_MAX_TEMP);
            if (tempIndex + alloced > VECX_MAX_TEMP)
                tempIndex = 0;
            p = temp;
            fixed (float* p = &temp[0])
                tempPtr = (int)((ulong)p + 15) & ~15;
            pi = tempPtr + tempIndex;
            tempIndex += alloced;
            VECX_CLEAREND();
        }
    }

    public struct Polar3
    {
        public float radius, theta, phi;

        public Polar3(float radius, float theta, float phi)
        {
            Debug.Assert(radius > 0);
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;
        }

        public void Set(float radius, float theta, float phi)
        {
            Debug.Assert(radius > 0);
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;
        }

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &radius)
                    return p[index];
            }
        }

        public static Polar3 operator -(Polar3 _) => new(_.radius, -_.theta, -_.phi);

        public Vector3 ToVec3()
        {
            MathX.SinCos(phi, out var sp, out var cp);
            MathX.SinCos(theta, out var st, out var ct);
            return new Vector3(cp * radius * ct, cp * radius * st, radius * sp);
        }
    }
}