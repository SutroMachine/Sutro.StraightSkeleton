using g3;

namespace Sutro.StraightSkeleton.Primitives
{
    internal static class VectorExtensions
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
}
