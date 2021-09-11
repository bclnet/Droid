using System.Diagnostics;

namespace System.NumericsX
{
    public struct Bounds
    {
        Vector3[] b;

        public Bounds(Bounds a)
        {
            b = (Vector3[])a.b.Clone();
        }
        //internal Bounds()
        //{
        //    b = new Vector3[2];
        //}
        public Bounds(Vector3 mins, Vector3 maxs)
        {
            b = new Vector3[2];
            b[0] = mins;
            b[1] = maxs;
        }

        public Bounds(Vector3 point)
        {
            b = new Vector3[2];
            b[0] = point;
            b[1] = point;
        }

        public ref Vector3 this[int index]
            => ref b[index];

        public static Bounds operator +(Bounds _, Vector3 t)                // returns translated bounds
            => new(_.b[0] + t, _.b[1] + t);
        public static Bounds operator *(Bounds _, Matrix3x3 r)              // returns rotated bounds
        {
            Bounds bounds = new();
            bounds.FromTransformedBounds(_, Vector3.origin, r);
            return bounds;
        }
        public static Bounds operator +(Bounds _, Bounds a)
        {
            Bounds newBounds = new(_);
            newBounds.AddBounds(a);
            return newBounds;
        }
        public static Bounds operator -(Bounds _, Bounds a)
        {
            Debug.Assert(
                _.b[1].x - _.b[0].x > a.b[1].x - a.b[0].x &&
                _.b[1].y - _.b[0].y > a.b[1].y - a.b[0].y &&
                _.b[1].z - _.b[0].z > a.b[1].z - a.b[0].z);
            return new(
                new Vector3(_.b[0].x + a.b[1].x, _.b[0].y + a.b[1].y, _.b[0].z + a.b[1].z),
                new Vector3(_.b[1].x + a.b[0].x, _.b[1].y + a.b[0].y, _.b[1].z + a.b[0].z));
        }

        public bool Compare(Bounds a)                          // exact compare, no epsilon
            => b[0].Compare(a.b[0]) && b[1].Compare(a.b[1]);
        public bool Compare(Bounds a, float epsilon)   // compare with epsilon
            => b[0].Compare(a.b[0], epsilon) && b[1].Compare(a.b[1], epsilon);
        public static bool operator ==(Bounds _, Bounds a)                      // exact compare, no epsilon
            => _.Compare(a);
        public static bool operator !=(Bounds _, Bounds a)                      // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Bounds q && Compare(q);
        public override int GetHashCode()
            => b.GetHashCode();

        public void Clear()                                    // inside out bounds
        {
            b[0].x = b[0].y = b[0].z = MathX.INFINITY;
            b[1].x = b[1].y = b[1].z = -MathX.INFINITY;
        }
        public void Zero()                                 // single point at origin
            =>
            b[0].x = b[0].y = b[0].z =
            b[1].x = b[1].y = b[1].z = 0;

        public Vector3 Center                      // returns center of bounds
            => new((b[1].x + b[0].x) * 0.5f, (b[1].y + b[0].y) * 0.5f, (b[1].z + b[0].z) * 0.5f);

        public float GetRadius()                       // returns the radius relative to the bounds origin
        {
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                var b0 = (float)MathX.Fabs(b[0][i]);
                var b1 = (float)MathX.Fabs(b[1][i]);
                if (b0 > b1) total += b0 * b0;
                else total += b1 * b1;
            }
            return MathX.Sqrt(total);
        }
        public float GetRadius(Vector3 center)     // returns the radius relative to the given center
        {
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                var b0 = (float)MathX.Fabs(center[i] - b[0][i]);
                var b1 = (float)MathX.Fabs(b[1][i] - center[i]);
                if (b0 > b1) total += b0 * b0;
                else total += b1 * b1;
            }
            return MathX.Sqrt(total);
        }

        public float GetVolume()                       // returns the volume of the bounds
            => b[0].x >= b[1].x || b[0].y >= b[1].y || b[0].z >= b[1].z ? 0f
            : (b[1].x - b[0].x) * (b[1].y - b[0].y) * (b[1].z - b[0].z);

        public bool IsCleared                        // returns true if bounds are inside out
            => b[0].x > b[1].x;

        public bool AddPoint(Vector3 v)                    // add the point, returns true if the bounds expanded
        {
            var expanded = false;
            if (v.x < b[0].x) { b[0].x = v.x; expanded = true; }
            if (v.x > b[1].x) { b[1].x = v.x; expanded = true; }
            if (v.y < b[0].y) { b[0].y = v.y; expanded = true; }
            if (v.y > b[1].y) { b[1].y = v.y; expanded = true; }
            if (v.z < b[0].z) { b[0].z = v.z; expanded = true; }
            if (v.z > b[1].z) { b[1].z = v.z; expanded = true; }
            return expanded;
        }

        public bool AddBounds(Bounds a)                    // add the bounds, returns true if the bounds expanded
        {
            var expanded = false;
            if (a.b[0].x < b[0].x) { b[0].x = a.b[0].x; expanded = true; }
            if (a.b[0].y < b[0].y) { b[0].y = a.b[0].y; expanded = true; }
            if (a.b[0].z < b[0].z) { b[0].z = a.b[0].z; expanded = true; }
            if (a.b[1].x > b[1].x) { b[1].x = a.b[1].x; expanded = true; }
            if (a.b[1].y > b[1].y) { b[1].y = a.b[1].y; expanded = true; }
            if (a.b[1].z > b[1].z) { b[1].z = a.b[1].z; expanded = true; }
            return expanded;
        }

        public Bounds Intersect(Bounds a)          // return intersection of this bounds with the given bounds
        {
            Bounds n = new();
            n.b[0].x = a.b[0].x > b[0].x ? a.b[0].x : b[0].x;
            n.b[0].y = a.b[0].y > b[0].y ? a.b[0].y : b[0].y;
            n.b[0].z = a.b[0].z > b[0].z ? a.b[0].z : b[0].z;
            n.b[1].x = a.b[1].x < b[1].x ? a.b[1].x : b[1].x;
            n.b[1].y = a.b[1].y < b[1].y ? a.b[1].y : b[1].y;
            n.b[1].z = a.b[1].z < b[1].z ? a.b[1].z : b[1].z;
            return n;
        }
        public Bounds IntersectSelf(Bounds a)              // intersect this bounds with the given bounds
        {
            if (a.b[0].x > b[0].x) b[0].x = a.b[0].x;
            if (a.b[0].y > b[0].y) b[0].y = a.b[0].y;
            if (a.b[0].z > b[0].z) b[0].z = a.b[0].z;
            if (a.b[1].x < b[1].x) b[1].x = a.b[1].x;
            if (a.b[1].y < b[1].y) b[1].y = a.b[1].y;
            if (a.b[1].z < b[1].z) b[1].z = a.b[1].z;
            return this;
        }

        public Bounds Expand(float d)                  // return bounds expanded in all directions with the given value
            => new(
            new Vector3(b[0].x - d, b[0].y - d, b[0].z - d),
            new Vector3(b[1].x + d, b[1].y + d, b[1].z + d));
        public Bounds ExpandSelf(float d)                  // expand bounds in all directions with the given value
        {
            b[0].x -= d; b[0].y -= d; b[0].z -= d;
            b[1].x += d; b[1].y += d; b[1].z += d;
            return this;
        }

        public Bounds Translate(Vector3 translation)   // return translated bounds
            => new(b[0] + translation, b[1] + translation);
        public Bounds TranslateSelf(Vector3 translation)       // translate this bounds
        {
            b[0] += translation;
            b[1] += translation;
            return this;
        }

        public Bounds Rotate(Matrix3x3 rotation)           // return rotated bounds
        {
            Bounds bounds = new();
            bounds.FromTransformedBounds(this, Vector3.origin, rotation);
            return bounds;
        }
        public Bounds RotateSelf(Matrix3x3 rotation)           // rotate this bounds
        {
            FromTransformedBounds(this, Vector3.origin, rotation);
            return this;
        }

        public float PlaneDistance(Plane plane)
        {
            var center = (b[0] + b[1]) * 0.5f;

            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs((b[1].x - center.x) * pn.x) +
                MathX.Fabs((b[1].y - center.y) * pn.y) +
                MathX.Fabs((b[1].z - center.z) * pn.z); //: opt

            if (d1 - d2 > 0f) return d1 - d2;
            if (d1 + d2 < 0f) return d1 + d2;
            return 0f;
        }

        public PLANESIDE PlaneSide(Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var center = (b[0] + b[1]) * 0.5f;

            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs((b[1].x - center.x) * pn[0]) +
                MathX.Fabs((b[1].y - center.y) * pn[1]) +
                MathX.Fabs((b[1].z - center.z) * pn[2]);

            if (d1 - d2 > epsilon) return PLANESIDE.FRONT;
            if (d1 + d2 < -epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        public bool ContainsPoint(Vector3 p)           // includes touching
            =>
            p.x >= b[0].x && p.y >= b[0].y && p.z >= b[0].z &&
            p.x <= b[1].x && p.y <= b[1].y && p.z <= b[1].z; //: opt

        public bool IntersectsBounds(Bounds a) // includes touching
            =>
            a.b[1].x >= b[0].x && a.b[1].y >= b[0].y && a.b[1].z >= b[0].z &&
            a.b[0].x <= b[1].x && a.b[0].y <= b[1].y && a.b[0].z <= b[1].z; //: opt

        // Returns true if the line intersects the bounds between the start and end point.
        public bool LineIntersection(Vector3 start, Vector3 end)
        {
            var center = (b[0] + b[1]) * 0.5f;
            var extents = b[1] - center;
            var lineDir = 0.5f * (end - start);
            var lineCenter = start + lineDir;
            var dir = lineCenter - center;

            var ld_x = MathX.Fabs(lineDir.x); if (MathX.Fabs(dir.x) > extents.x + ld_x) return false;
            var ld_y = MathX.Fabs(lineDir.y); if (MathX.Fabs(dir.y) > extents.y + ld_y) return false;
            var ld_z = MathX.Fabs(lineDir.z); if (MathX.Fabs(dir.z) > extents.z + ld_z) return false;

            var cross = lineDir.Cross(dir);
            if (MathX.Fabs(cross.x) > extents.y * ld_z + extents.z * ld_y) return false;
            if (MathX.Fabs(cross.y) > extents.x * ld_z + extents.z * ld_x) return false;
            if (MathX.Fabs(cross.z) > extents.x * ld_y + extents.y * ld_x) return false;
            return true;
        }

        // Returns true if the ray intersects the bounds.
        // The ray can intersect the bounds in both directions from the start point.
        // If start is inside the bounds it is considered an intersection with scale = 0
        // intersection point is start + dir * scale
        public bool RayIntersection(Vector3 start, Vector3 dir, ref float scale)
        {
            var ax0 = -1;
            int inside = 0, side;
            for (var i = 0; i < 3; i++)
            {
                if (start[i] < b[0][i]) side = 0;
                else if (start[i] > b[1][i]) side = 1;
                else
                {
                    inside++;
                    continue;
                }
                if (dir[i] == 0f)
                    continue;
                var f = start[i] - b[side][i];
                if (ax0 < 0 || MathX.Fabs(f) > MathX.Fabs(scale * dir[i]))
                {
                    scale = -(f / dir[i]);
                    ax0 = i;
                }
            }

            if (ax0 < 0)
            {
                scale = 0f;
                // return true if the start point is inside the bounds
                return inside == 3;
            }

            var ax1 = (ax0 + 1) % 3;
            var ax2 = (ax0 + 2) % 3;
            Vector3 hit = new();
            hit[ax1] = start[ax1] + scale * dir[ax1];
            hit[ax2] = start[ax2] + scale * dir[ax2];
            return
                hit[ax1] >= b[0][ax1] && hit[ax1] <= b[1][ax1] &&
                hit[ax2] >= b[0][ax2] && hit[ax2] <= b[1][ax2];
        }

        // most tight bounds for the given transformed bounds
        public void FromTransformedBounds(Bounds bounds, Vector3 origin, Matrix3x3 axis)
        {
            var center = (bounds[0] + bounds[1]) * 0.5f;
            var extents = bounds[1] - center;

            var rotatedExtents = new Vector3
            {
                x = MathX.Fabs(extents.x * axis[0].x) +
                    MathX.Fabs(extents.y * axis[1].x) +
                    MathX.Fabs(extents.z * axis[2].x),
                y = MathX.Fabs(extents.x * axis[0].y) +
                    MathX.Fabs(extents.y * axis[1].y) +
                    MathX.Fabs(extents.z * axis[2].y),
                z = MathX.Fabs(extents.x * axis[0].z) +
                    MathX.Fabs(extents.y * axis[1].z) +
                    MathX.Fabs(extents.z * axis[2].z),
            }; //: unroll

            center = origin + center * axis;
            b[0] = center - rotatedExtents;
            b[1] = center + rotatedExtents;
        }

        // most tight bounds for a point set
        public unsafe void FromPoints(Vector3[] points, int numPoints)
        {
            fixed (Vector3* pointsF = points)
                ISimd.Processor.MinMax(out b[0], out b[1], pointsF, numPoints);
        }

        // Most tight bounds for the translational movement of the given point.
        public void FromPointTranslation(Vector3 point, Vector3 translation)
        {
            if (translation.x < 0f) { b[0].x = point.x + translation.x; b[1].x = point.x; }
            else { b[0].x = point.y; b[1].x = point.x + translation.x; } //: unroll
            if (translation.y < 0f) { b[0].y = point.y + translation.y; b[1].y = point.y; }
            else { b[0].y = point.y; b[1].y = point.y + translation.y; } //: unroll
            if (translation.z < 0f) { b[0].z = point.z + translation.z; b[1].z = point.z; }
            else { b[0].z = point.z; b[1].z = point.z + translation.z; } //: unroll
        }

        // Most tight bounds for the translational movement of the given bounds.
        public void FromBoundsTranslation(Bounds bounds, Vector3 origin, Matrix3x3 axis, Vector3 translation)
        {
            if (axis.IsRotated())
                FromTransformedBounds(bounds, origin, axis);
            else
            {
                b[0] = bounds[0] + origin;
                b[1] = bounds[1] + origin;
            }
            if (translation.x < 0f) b[0].x += translation.x;
            else b[1].x += translation.x; //: unroll
            if (translation.y < 0f) b[0].y += translation.y;
            else b[1].y += translation.y; //: unroll
            if (translation.z < 0f) b[0].z += translation.z;
            else b[1].z += translation.z; //: unroll
        }

        // only for rotations < 180 degrees
        Bounds BoundsForPointRotation(Vector3 start, Rotation rotation)
        {
            var end = start * rotation;
            var axis = rotation.Vec;
            var origin = rotation.Origin + axis * (axis * (start - rotation.Origin));
            var radiusSqr = (start - origin).LengthSqr;
            var v1 = (start - origin).Cross(axis);
            var v2 = (end - origin).Cross(axis);

            Bounds bounds = new();
            for (var i = 0; i < 3; i++)
            {
                // if the derivative changes sign along this axis during the rotation from start to end
                if ((v1[i] > 0f && v2[i] < 0f) || (v1[i] < 0f && v2[i] > 0f))
                {
                    if ((0.5f * (start[i] + end[i]) - origin[i]) > 0f)
                    {
                        bounds[0][i] = Math.Min(start[i], end[i]);
                        bounds[1][i] = origin[i] + MathX.Sqrt(radiusSqr * (1f - axis[i] * axis[i]));
                    }
                    else
                    {
                        bounds[0][i] = origin[i] - MathX.Sqrt(radiusSqr * (1f - axis[i] * axis[i]));
                        bounds[1][i] = Math.Max(start[i], end[i]);
                    }
                }

                else if (start[i] > end[i])
                {
                    bounds[0][i] = end[i];
                    bounds[1][i] = start[i];
                }
                else
                {
                    bounds[0][i] = start[i];
                    bounds[1][i] = end[i];
                }
            }

            return bounds;
        }

        // Most tight bounds for the rotational movement of the given point.
        public void FromPointRotation(Vector3 point, Rotation rotation)
        {
            if (MathX.Fabs(rotation.Angle) < 180f)
                this = BoundsForPointRotation(point, rotation);
            else
            {
                var radius = (point - rotation.Origin).Length;
                // FIXME: these bounds are usually way larger
                b[0].Set(-radius, -radius, -radius);
                b[1].Set(radius, radius, radius);
            }
        }

        // Most tight bounds for the rotational movement of the given bounds.
        public void FromBoundsRotation(Bounds bounds, Vector3 origin, Matrix3x3 axis, Rotation rotation)
        {
            if (MathX.Fabs(rotation.Angle) < 180f)
            {
                this = BoundsForPointRotation(bounds[0] * axis + origin, rotation);
                for (var i = 1; i < 8; i++)
                {
                    var point = new Vector3
                    {
                        x = bounds[(i ^ (i >> 1)) & 1].z,
                        y = bounds[(i >> 1) & 1].y,
                        z = bounds[(i >> 2) & 1].z,
                    };
                    this += BoundsForPointRotation(point * axis + origin, rotation);
                }
            }
            else
            {
                var point = (bounds[1] - bounds[0]) * 0.5f;
                var radius = (bounds[1] - point).Length + (point - rotation.Origin).Length;
                // FIXME: these bounds are usually way larger
                b[0].Set(-radius, -radius, -radius);
                b[1].Set(radius, radius, radius);
            }
        }

        public void ToPoints(Vector3[] points)
        {
            for (var i = 0; i < 8; i++)
            {
                points[i].x = b[(i ^ (i >> 1)) & 1].x;
                points[i].y = b[(i >> 1) & 1].y;
                points[i].z = b[(i >> 2) & 1].z;
            }
        }

        public Sphere ToSphere()
        {
            Sphere sphere = new();
            sphere.Origin = (b[0] + b[1]) * 0.5f;
            sphere.Radius = (b[1] - sphere.Origin).Length;
            return sphere;
        }

        public void AxisProjection(Vector3 dir, out float min, out float max)
        {
            var center = (b[0] + b[1]) * 0.5f;
            var extents = b[1] - center;

            var d1 = dir * center;
            var d2 =
                MathX.Fabs(extents.x * dir.x) +
                MathX.Fabs(extents.y * dir.y) +
                MathX.Fabs(extents.z * dir.z);

            min = d1 - d2;
            max = d1 + d2;
        }

        public void AxisProjection(Vector3 origin, Matrix3x3 axis, Vector3 dir, out float min, out float max)
        {
            var center = (b[0] + b[1]) * 0.5f;
            var extents = b[1] - center;
            center = origin + center * axis;

            var d1 = dir * center;
            var d2 =
                MathX.Fabs(extents.x * (dir * axis[0])) +
                MathX.Fabs(extents.y * (dir * axis[1])) +
                MathX.Fabs(extents.z * (dir * axis[2]));

            min = d1 - d2;
            max = d1 + d2;
        }

        public static readonly Bounds zero = new(Vector3.origin, Vector3.origin);
    }
}