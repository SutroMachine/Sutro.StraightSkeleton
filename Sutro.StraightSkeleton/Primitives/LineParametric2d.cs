using g3;

namespace Sutro.StraightSkeleton.Primitives
{
    /// <summary>
    ///     Geometry line in parametric form:
    ///     x = x_A + t * u_x;
    ///     y = y_A + t * u_y;
    ///     where t in R
    ///     <see href="http://en.wikipedia.org/wiki/Linear_equation" />
    /// </summary>
    internal struct LineParametric2d
    {
        public static readonly LineParametric2d Empty = new LineParametric2d(Vector2d.MinValue, Vector2d.MinValue);
        public Vector2d A;
        public Vector2d U;

        public LineParametric2d(Vector2d pA, Vector2d pU)
        {
            A = pA;
            U = pU;
        }

        public static Vector2d Collide(LineParametric2d ray, LineLinear2d line, double epsilon)
        {
            var collide = LineLinear2d.Collide(ray.CreateLinearForm(), line);
            if (collide.Equals(Vector2d.MinValue))
                return Vector2d.MinValue;

            var collideVector = collide - ray.A;
            return ray.U.Dot(collideVector) < epsilon ? Vector2d.MinValue : collide;
        }

        public LineLinear2d CreateLinearForm()
        {
            var x = this.A.x;
            var y = this.A.y;

            var B = -U.x;
            var A = U.y;

            var C = -(A * x + B * y);
            return new LineLinear2d(A, B, C);
        }

        public bool IsOnLeftSite(Vector2d point, double epsilon)
        {
            var direction = point - A;
            return PrimitiveUtils.OrthogonalRight(U).Dot(direction) < epsilon;
        }

        public bool IsOnRightSite(Vector2d point, double epsilon)
        {
            var direction = point - A;
            return PrimitiveUtils.OrthogonalRight(U).Dot(direction) > -epsilon;
        }
    }
}
