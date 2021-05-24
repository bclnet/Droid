using System;
using System.Diagnostics;

namespace Droid.Core
{
    public struct Rotation
    {
        public Rotation(Vector3 rotationOrigin, Vector3 rotationVec, float rotationAngle)
        {
            origin = rotationOrigin;
            vec = rotationVec;
            angle = rotationAngle;
            axis = default;
            axisValid = false;
        }

        public void Set(Vector3 rotationOrigin, Vector3 rotationVec, float rotationAngle)
        {
            origin = rotationOrigin;
            vec = rotationVec;
            angle = rotationAngle;
            axisValid = false;
        }
        public void SetOrigin(Vector3 rotationOrigin)
            => origin = rotationOrigin;
        /// <summary>
        /// has to be normalized
        /// </summary>
        /// <param name="rotationVec">The rotation vec.</param>
        public void SetVec(Vector3 rotationVec)
        {
            vec = rotationVec;
            axisValid = false;
        }
        /// <summary>
        /// has to be normalized
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public void SetVec(float x, float y, float z)
        {
            vec.x = x;
            vec.y = y;
            vec.z = z;
            axisValid = false;
        }
        public void SetAngle(float rotationAngle)
        {
            angle = rotationAngle;
            axisValid = false;
        }
        public void Scale(float s)
        {
            angle *= s;
            axisValid = false;
        }
        public void ReCalculateMatrix()
        {
            axisValid = false;
            ToMat3();
        }
        public Vector3 GetOrigin()
            => origin;
        public Vector3 GetVec()
            => vec;
        public float GetAngle()
            => angle;

        /// <summary>
        /// flips rotation
        /// </summary>
        /// <param name="_">The .</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Rotation operator -(Rotation _)
            => new(_.origin, _.vec, -_.angle);
        /// <summary>
        /// scale rotation
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="s">The s.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Rotation operator *(Rotation _, float s)
            => new(_.origin, _.vec, _.angle * s);
        /// <summary>
        /// scale rotation
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="s">The s.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Rotation operator /(Rotation _, float s)
        {
            Debug.Assert(s != 0.0f);
            return new Rotation(_.origin, _.vec, _.angle / s);
        }
        /// <summary>
        /// rotate vector
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector3 operator *(Rotation _, Vector3 v)
        {
            if (!_.axisValid)
                _.ToMat3();
            return (v - _.origin) * _.axis + _.origin;
        }

        /// <summary>
        /// scale rotation
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Rotation operator *(float s, Rotation r)
            => r * s;
        /// <summary>
        /// rotate vector
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector3 operator *(Vector3 v, Rotation r)
            => r * v;

        public Angles ToAngles()
            => ToMat3().ToAngles();
        public Quat ToQuat()
        {
            var a = angle * (MathX.M_DEG2RAD * 0.5f);
            MathX.SinCos(a, out var s, out var c);
            return new Quat(vec.x * s, vec.y * s, vec.z * s, c);
        }
        public Matrix3x3 ToMat3()
        {
            if (axisValid)
                return axis;

            var a = angle * (MathX.M_DEG2RAD * 0.5f);
            MathX.SinCos(a, out var s, out var c);

            float x = vec.x * s, y = vec.y * s, z = vec.z * s;
            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = c * x2, wy = c * y2, wz = c * z2;

            axis.mat0.x = 1.0f - (yy + zz); axis.mat0.y = xy - wz; axis.mat0.z = xz + wy;
            axis.mat1.x = xy + wz; axis.mat1.y = 1.0f - (xx + zz); axis.mat1.z = yz - wx;
            axis.mat2.x = xz - wy; axis.mat2.y = yz + wx; axis.mat2.z = 1.0f - (xx + yy);

            axisValid = true;

            return axis;
        }
        public Matrix4x4 ToMat4()
            => ToMat3().ToMat4();
        public Vector3 ToAngularVelocity()
            => vec * MathX.DEG2RAD(angle);

        public void RotatePoint(ref Vector3 point)
        {
            if (!axisValid)
                ToMat3();
            point = (point - origin) * axis + origin;
        }

        public void Normalize180()
        {
            angle -= (float)Math.Floor(angle / 360.0f) * 360.0f;
            if (angle > 180.0f) angle -= 360.0f;
            else if (angle < -180.0f) angle += 360.0f;
        }
        public void Normalize360()
        {
            angle -= (float)Math.Floor(angle / 360.0f) * 360.0f;
            if (angle > 360.0f) angle -= 360.0f;
            else if (angle < 0.0f) angle += 360.0f;
        }

        internal Vector3 origin;         // origin of rotation
        internal Vector3 vec;            // normalized vector to rotate around
        internal float angle;            // angle of rotation in degrees
        internal Matrix3x3 axis;          // rotation axis
        internal bool axisValid;     // true if rotation axis is valid
    }
}