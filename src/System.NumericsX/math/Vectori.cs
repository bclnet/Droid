using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3i
    {
        public int x;
        public int y;
        public int z;

        public Vector3i(int xyz)
            => x = y = z = xyz;
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(Vector3i a)
            => this = a;

        public void Zero()
            => x = y = z = 0;

        public unsafe int this[int index]
        {
            get
            {
                fixed (int* p = &x)
                    return p[index];
            }
            set
            {
                fixed (int* p = &x)
                    p[index] = value;
            }
        }

        public static Vector3i operator -(Vector3i _)
            => new(-_.x, -_.y, -_.z);
        public static float operator *(Vector3i _, Vector3i a)
            => _.x * a.x + _.y * a.y + _.z * a.z;
        public static Vector3i operator *(Vector3i _, int a)
            => new(_.x * a, _.y * a, _.z * a);
        public static Vector3i operator /(Vector3i _, int a)
        {
            var inva = 1 / a;
            return new Vector3i(_.x * inva, _.y * inva, _.z * inva);
        }
        public static Vector3i operator +(Vector3i _, Vector3i a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z);
        public static Vector3i operator -(Vector3i _, Vector3i a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z);

        public static Vector3i operator *(int a, Vector3i b)
            => new(b.x * a, b.y * a, b.z * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(Vector3i a)
            => (x == a.x) && (y == a.y) && (z == a.z);
        
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector3i _, Vector3i a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector3i _, Vector3i a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector3i q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        public Vector3i Cross(Vector3i a)
            => new(y * a.z - z * a.y, z * a.x - x * a.z, x * a.y - y * a.x);
        public Vector3i Cross(Vector3i a, Vector3i b)
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

        public void Clamp(Vector3i min, Vector3i max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
            if (z < min.z) z = min.z;
            else if (z > max.z) z = max.z;
        }

        public static int Dimension
            => 3;

        public static Vector3i origin = new(0, 0, 0);
    }
}