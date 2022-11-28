using System;
using System.Collections.Generic;
using g3;

namespace Sutro.StraightSkeleton.Primitives
{
    internal static class Vector2dExtensions
    {
        #region Vector specific

        public static Vector2d BisectorNormalized(Vector2d norm1, Vector2d norm2)
        {
            var e1v = OrthogonalLeft(norm1);
            var e2v = OrthogonalLeft(norm2);

            // 90 - 180 || 180 - 270
            if (norm1.Dot(norm2) > 0)
                return e1v + e2v;

            // 0 - 180
            var ret = new Vector2d(norm1);
            ret *= -1;
            ret += norm2;

            // 270 - 360
            if (e1v.Dot(norm2) < 0)
                ret *= -1;

            return ret;
        }

        public static Vector2d FromTo(Vector2d begin, Vector2d end)
        {
            return new Vector2d(end.x - begin.x, end.y - begin.y);
        }

        public static Vector2d OrthogonalLeft(Vector2d v)
        {
            return new Vector2d(-v.y, v.x);
        }

        /// <summary>
        ///     <see href="http://en.wikipedia.org/wiki/Vector_projection" />
        /// </summary>
        public static Vector2d OrthogonalProjection(Vector2d unitVector, Vector2d vectorToProject)
        {
            var n = new Vector2d(unitVector).Normalized;

            var px = vectorToProject.x;
            var py = vectorToProject.y;

            var ax = n.x;
            var ay = n.y;

            return new Vector2d(px * ax * ax + py * ax * ay, px * ax * ay + py * ay * ay);
        }

        public static Vector2d OrthogonalRight(Vector2d v)
        {
            return new Vector2d(v.y, -v.x);
        }

        #endregion Vector specific
    }

    public class IntersectPoints
    {
        /// <summary> Intersection point or begin of intersection segment. </summary>
        public readonly Vector2d Intersect;

        /// <summary> Intersection end. </summary>
        public readonly Vector2d IntersectEnd;

        public IntersectPoints(Vector2d intersect, Vector2d intersectEnd)
        {
            Intersect = intersect;
            IntersectEnd = intersectEnd;
        }

        public IntersectPoints(Vector2d intersect)
            : this(intersect, Vector2d.MinValue)
        {
        }

        public IntersectPoints()
            : this(Vector2d.MinValue, Vector2d.MinValue)
        {
        }
    }

    internal static class RayExtensions
    {
        /// <summary>
        ///     Calculate intersection points for rays. It can return more then one
        ///     intersection point when rays overlaps.
        ///     <see href="http://geomalgorithms.com/a05-_intersect-1.html" />
        ///     <see href="http://softsurfer.com/Archive/algorithm_0102/algorithm_0102.htm" />
        /// </summary>
        /// <returns>class with intersection points. It never return null.</returns>
        public static IntersectPoints IntersectRays2D(Line2d r1, Line2d r2)
        {
            var s1p0 = r1.Origin;
            var s1p1 = r1.Origin + r1.Direction;

            var s2p0 = r2.Origin;

            var u = r1.Direction;
            var v = r2.Direction;

            var w = s1p0 - s2p0;
            var d = u.DotPerp(v);

            // test if they are parallel (includes either being a point)
            if (Math.Abs(d) < SmallNum)
            {
                // they are NOT collinear
                // S1 and S2 are parallel
                if (u.DotPerp(w) != 0 || v.DotPerp(w) != 0)
                    return Empty;

                // they are collinear or degenerate
                // check if they are degenerate points
                var du = u.Dot(u);
                var dv = v.Dot(v);
                if (du == 0 && dv == 0)
                {
                    // both segments are points
                    if (s1p0 != s2p0)
                        return Empty;

                    // they are the same point
                    return new IntersectPoints(s1p0);
                }
                if (du == 0)
                {
                    // S1 is a single point
                    if (!InCollinearRay(s1p0, s2p0, v))
                        return Empty;

                    return new IntersectPoints(s1p0);
                }
                if (dv == 0)
                {
                    // S2 a single point
                    if (!InCollinearRay(s2p0, s1p0, u))
                        return Empty;

                    return new IntersectPoints(s2p0);
                }
                // they are collinear segments - get overlap (or not)
                double t0, t1;
                // endpoints of S1 in eqn for S2
                var w2 = s1p1 - s2p0;
                if (v.x != 0)
                {
                    t0 = w.x / v.x;
                    t1 = w2.x / v.x;
                }
                else
                {
                    t0 = w.y / v.y;
                    t1 = w2.y / v.y;
                }
                if (t0 > t1)
                {
                    // must have t0 smaller than t1
                    var t = t0;
                    t0 = t1;
                    t1 = t; // swap if not
                }
                if (t1 < 0)
                    // NO overlap
                    return Empty;

                // clip to min 0
                t0 = t0 < 0 ? 0 : t0;

                if (t0 == t1)
                {
                    // intersect is a point
                    var I0 = new Vector2d(v);
                    I0 *= t0;
                    I0 += s2p0;

                    return new IntersectPoints(I0);
                }

                // they overlap in a valid subsegment

                // I0 = S2_P0 + t0 * v;
                var I_0 = new Vector2d(v);
                I_0 *= t0;
                I_0 += s2p0;

                // I1 = S2_P0 + t1 * v;
                var I1 = new Vector2d(v);
                I1 *= t1;
                I1 += s2p0;

                return new IntersectPoints(I_0, I1);
            }

            // the segments are skew and may intersect in a point
            // get the intersect parameter for S1
            var sI = v.DotPerp(w) / d;
            if (sI < 0 /* || sI > 1 */)
                return Empty;

            // get the intersect parameter for S2
            var tI = u.DotPerp(w) / d;
            if (tI < 0 /* || tI > 1 */)
                return Empty;

            // I0 = S1_P0 + sI * u; // compute S1 intersect point
            var IO = new Vector2d(u);
            IO *= sI;
            IO += s1p0;

            return new IntersectPoints(IO);
        }

        public static bool IsPointOnRay(Vector2d point, Line2d ray, double epsilon)
        {
            var rayDirection = new Vector2d(ray.Direction).Normalized;
            // test if point is on ray
            var pointVector = point - ray.Origin;

            var dot = rayDirection.Dot(pointVector);

            if (dot < epsilon)
                return false;

            var x = rayDirection.x;
            rayDirection.x = rayDirection.y;
            rayDirection.y = -x;

            dot = rayDirection.Dot(pointVector);

            return -epsilon < dot && dot < epsilon;
        }

        /// <summary> Error epsilon. Anything that avoids division. </summary>
        private const double SmallNum = 0.00000001;

        /// <summary> Return value if there is no intersection. </summary>
        private static readonly IntersectPoints Empty = new IntersectPoints();

        private static bool InCollinearRay(Vector2d p, Vector2d rayStart, Vector2d rayDirection)
        {
            // test if point is on ray
            var collideVector = p - rayStart;
            var dot = rayDirection.Dot(collideVector);

            return !(dot < 0);
        }
    }
}
