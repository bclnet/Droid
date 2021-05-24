namespace Droid.Core
{
    public struct Complex
    {
        public float r;     // real part
        public float i;      // imaginary part

        public Complex(float r, float i)
        {
            this.r = r;
            this.i = i;
        }

        public void Set(float r, float i)
        {
            this.r = r;
            this.i = i;
        }
        public void Zero()
            => r = i = 0.0f;

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &r)
                    return p[index];
            }
            set
            {
                fixed (float* p = &r)
                    p[index] = value;
            }
        }

        public static Complex operator -(Complex _)
            => new(-_.r, -_.i);

        public static Complex operator *(Complex _, Complex a)
            => new(_.r * a.r - _.i * a.i, _.i * a.r + _.r * a.i);
        public static Complex operator /(Complex _, Complex a)
        {
            float s, t;
            if (MathX.Fabs(a.r) >= MathX.Fabs(a.i))
            {
                s = a.i / a.r;
                t = 1.0f / (a.r + s * a.i);
                return new((_.r + s * _.i) * t, (_.i - s * _.r) * t);
            }
            else
            {
                s = a.r / a.i;
                t = 1.0f / (s * a.r + a.i);
                return new((_.r * s + _.i) * t, (_.i * s - _.r) * t);
            }
        }
        public static Complex operator +(Complex _, Complex a)
            => new(_.r + a.r, _.i + a.i);
        public static Complex operator -(Complex _, Complex a)
            => new(_.r - a.r, _.i - a.i);

        public static Complex operator *(Complex _, float a)
            => new(_.r * a, _.i * a);
        public static Complex operator /(Complex _, float a)
        {
            var s = 1.0f / a;
            return new(_.r * s, _.i * s);
        }
        public static Complex operator +(Complex _, float a)
            => new(_.r + a, _.i);
        public static Complex operator -(Complex _, float a)
            => new(_.r - a, _.i);

        public static Complex operator *(float a, Complex b)
            => new(a * b.r, a * b.i);
        public static Complex operator /(float a, Complex b)
        {
            float s, t;
            if (MathX.Fabs(b.r) >= MathX.Fabs(b.i))
            {
                s = b.i / b.r;
                t = a / (b.r + s * b.i);
                return new(t, -s * t);
            }
            else
            {
                s = b.r / b.i;
                t = a / (s * b.r + b.i);
                return new(s * t, -t);
            }
        }
        public static Complex operator +(float a, Complex b)
            => new(a + b.r, b.i);
        public static Complex operator -(float a, Complex b)
            => new(a - b.r, -b.i);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(Complex a)
            => (r == a.r) && (i == a.i);
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(Complex a, float epsilon)
        {
            if (MathX.Fabs(r - a.r) > epsilon) return false;
            if (MathX.Fabs(i - a.i) > epsilon) return false;
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
        public static bool operator ==(Complex _, Complex a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Complex _, Complex a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Complex q && Compare(q);
        public override int GetHashCode()
            => r.GetHashCode() ^ i.GetHashCode();

        public Complex Reciprocal()
        {
            float s, t;
            if (MathX.Fabs(r) >= MathX.Fabs(i))
            {
                s = i / r;
                t = 1.0f / (r + s * i);
                return new(t, -s * t);
            }
            else
            {
                s = r / i;
                t = 1.0f / (s * r + i);
                return new(s * t, -t);
            }
        }
        public Complex Sqrt()
        {
            if (r == 0.0f && i == 0.0f)
                return new(0.0f, 0.0f);
            float w;
            var x = MathX.Fabs(r);
            var y = MathX.Fabs(i);
            if (x >= y)
            {
                w = y / x;
                w = MathX.Sqrt(x) * MathX.Sqrt(0.5f * (1.0f + MathX.Sqrt(1.0f + w * w)));
            }
            else
            {
                w = x / y;
                w = MathX.Sqrt(y) * MathX.Sqrt(0.5f * (w + MathX.Sqrt(1.0f + w * w)));
            }
            if (w == 0.0f) return new(0.0f, 0.0f);
            if (r >= 0.0f) return new(w, 0.5f * i / w);
            else return new(0.5f * y / w, (i >= 0.0f) ? w : -w);
        }
        public float Abs()
        {
            float t;
            var x = MathX.Fabs(r);
            var y = MathX.Fabs(i);
            if (x == 0.0f) return y;
            else if (y == 0.0f) return x;
            else if (x > y) { t = y / x; return x * MathX.Sqrt(1.0f + t * t); }
            else { t = x / y; return y * MathX.Sqrt(1.0f + t * t); }
        }

        public static int Dimension
            => 2;

        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* p = &r)
                return callback(p);
        }
        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(array => StringX.FloatArrayToString(array, Dimension, precision));

        public static Complex origin = new(0.0f, 0.0f);
        //#define zero origin
    }
}