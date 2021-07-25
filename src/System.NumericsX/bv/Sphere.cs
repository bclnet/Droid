namespace System.NumericsX
{
    public struct Sphere
    {
        Vector3 origin;
        float radius;

        //public Sphere() { }
        public Sphere(Vector3 point)
        {
            origin = point;
            radius = 0f;
        }
        public Sphere(Vector3 point, float r)
        {
            origin = point;
            radius = r;
        }

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* p = &origin.x)
                    return p[index];
            }
        }
        public static Sphere operator +(Sphere _, Vector3 t)              // returns tranlated sphere
            => new(_.origin + t, _.radius);
        //public static Sphere operator +(Sphere _, Sphere s)
        //{
        //}

        public bool Compare(Sphere a)                          // exact compare, no epsilon
            => origin.Compare(a.origin) && radius == a.radius;
        public bool Compare(Sphere a, float epsilon)    // compare with epsilon
            => origin.Compare(a.origin, epsilon) && MathX.Fabs(radius - a.radius) <= epsilon;
        public static bool operator ==(Sphere _, Sphere a)                      // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Sphere _, Sphere a)                      // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Sphere q && Compare(q);
        public override int GetHashCode()
            => origin.GetHashCode() ^ radius.GetHashCode();

        public void Clear()                                   // inside out sphere
        {
            origin.Zero();
            radius = -1f;
        }
        public void Zero()                                    // single point at origin
        {
            origin.Zero();
            radius = 0f;
        }

        // origin of sphere
        public Vector3 Origin
        {
            get => origin;
            set => origin = value;
        }

        // sphere radius
        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public bool IsCleared                       // returns true if sphere is inside out
            => radius < 0f;

        public bool AddPoint(Vector3 p)                    // add the point, returns true if the sphere expanded
        {
            if (radius < 0f)
            {
                origin = p;
                radius = 0f;
                return true;
            }
            else
            {
                var r = (p - origin).LengthSqr;
                if (r > radius * radius)
                {
                    r = MathX.Sqrt(r);
                    origin += (p - origin) * 0.5f * (1f - radius / r);
                    radius += 0.5f * (r - radius);
                    return true;
                }
                return false;
            }
        }
        public bool AddSphere(Sphere s)                    // add the sphere, returns true if the sphere expanded
        {
            if (radius < 0f)
            {
                origin = s.origin;
                radius = s.radius;
                return true;
            }
            else
            {
                var r = (s.origin - origin).LengthSqr;
                if (r > (radius + s.radius) * (radius + s.radius))
                {
                    r = MathX.Sqrt(r);
                    origin += (s.origin - origin) * 0.5f * (1f - radius / (r + s.radius));
                    radius += 0.5f * ((r + s.radius) - radius);
                    return true;
                }
                return false;
            }
        }
        public Sphere Expand(float d)                    // return bounds expanded in all directions with the given value
            => new(origin, radius + d);
        public Sphere ExpandSelf(float d)                 // expand bounds in all directions with the given value
        {
            radius += d;
            return this;
        }
        public Sphere Translate(Vector3 translation)
            => new(origin + translation, radius);
        public Sphere TranslateSelf(Vector3 translation)
        {
            origin += translation;
            return this;
        }

        public float PlaneDistance(Plane plane)
        {
            var d = plane.Distance(origin);
            if (d > radius) return d - radius;
            if (d < -radius) return d + radius;
            return 0f;
        }

        public PLANESIDE PlaneSide(Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var d = plane.Distance(origin);
            if (d > radius + epsilon) return PLANESIDE.FRONT;
            if (d < -radius - epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        public bool ContainsPoint(Vector3 p)           // includes touching
            => (p - origin).LengthSqr <= radius * radius;

        public bool IntersectsSphere(Sphere s) // includes touching
        {
            var r = s.radius + radius;
            return (s.origin - origin).LengthSqr <= r * r;
        }

        // Returns true if the line intersects the sphere between the start and end point.
        public bool LineIntersection(Vector3 start, Vector3 end)
        {
            var s = start - origin;
            var e = end - origin;
            var r = e - s;
            var a = -s * r;
            if (a <= 0) return s * s < radius * radius;
            else if (a >= r * r) return e * e < radius * radius;
            else
            {
                r = s + (a / (r * r)) * r;
                return r * r < radius * radius;
            }
        }

        // Returns true if the ray intersects the sphere.
        // The ray can intersect the sphere in both directions from the start point.
        // If start is inside the sphere then scale1< 0 and scale2> 0.
        // intersection points are (start + dir * scale1) and (start + dir * scale2)
        public bool RayIntersection(Vector3 start, Vector3 dir, out float scale1, out float scale2)
        {
            var p = start - origin;
            var a = dir * dir; //: double
            var b = dir * p; //: double
            var c = p * p - radius * radius; //: double
            var d = b * b - c * a; //: double

            if (d < 0f)
            {
                scale1 = scale2 = default;
                return false;
            }

            var sqrtd = MathX.Sqrt(d); //: double
            a = 1f / a;

            scale1 = (-b + sqrtd) * a;
            scale2 = (-b - sqrtd) * a;

            return true;
        }

        // Tight sphere for a point set.
        public void FromPoints(Vector3[] points, int numPoints)
        {
            //Vector3 mins, maxs;
            SIMD.Processor.MinMax(out var mins, out var maxs, points, numPoints);

            var origin = (mins + maxs) * 0.5f;

            var radiusSqr = 0f;
            for (var i = 0; i < numPoints; i++)
            {
                var dist = (points[i] - origin).LengthSqr;
                if (dist > radiusSqr)
                    radiusSqr = dist;
            }
            radius = MathX.Sqrt(radiusSqr);
        }

        // Most tight sphere for a translation.
        public void FromPointTranslation(Vector3 point, Vector3 translation)
        {
            origin = point + 0.5f * translation;
            radius = MathX.Sqrt(0.5f * translation.LengthSqr);
        }

        public void FromSphereTranslation(Sphere sphere, Vector3 start, Vector3 translation)
        {
            origin = start + sphere.origin + 0.5f * translation;
            radius = MathX.Sqrt(0.5f * translation.LengthSqr) + sphere.radius;
        }

        // Most tight sphere for a rotation.
        public void FromPointRotation(Vector3 point, Rotation rotation)
        {
            var end = rotation * point;
            origin = (point + end) * 0.5f;
            radius = MathX.Sqrt(0.5f * (end - point).LengthSqr);
        }

        public void FromSphereRotation(Sphere sphere, Vector3 start, Rotation rotation)
        {
            var end = rotation * sphere.origin;
            origin = start + (sphere.origin + end) * 0.5f;
            radius = MathX.Sqrt(0.5f * (end - sphere.origin).LengthSqr) + sphere.radius;
        }

        public void AxisProjection(Vector3 dir, out float min, out float max)
        {
            var d = dir * origin;
            min = d - radius;
            max = d + radius;
        }

        static readonly Sphere zero = new(Vector3.origin, 0f);
    }
}