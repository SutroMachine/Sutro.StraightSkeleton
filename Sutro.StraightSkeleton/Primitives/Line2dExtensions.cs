using g3;

namespace Sutro.StraightSkeleton.Primitives
{
    internal static class Line2dExtensions
    {
        public static Vector2d Collide(this Line2d ray, LineLinear2d line, double epsilon)
        {
            var collide = LineLinear2d.Collide(CreateLinearForm(ray), line);
            if (collide.Equals(Vector2d.MinValue))
                return Vector2d.MinValue;

            var collideVector = collide - ray.Origin;
            return ray.Direction.Dot(collideVector) < epsilon ? Vector2d.MinValue : collide;
        }

        public static LineLinear2d CreateLinearForm(this Line2d line)
        {
            var x = line.Origin.x;
            var y = line.Origin.y;

            var B = -line.Direction.x;
            var A = line.Direction.y;

            var C = -(A * x + B * y);
            return new LineLinear2d(A, B, C);
        }

        public static bool IsOnLeftSite(this Line2d line, Vector2d point, double epsilon)
        {
            var direction = point - line.Origin;
            return Vector2dExtensions.OrthogonalRight(line.Direction).Dot(direction) < epsilon;
        }

        public static bool IsOnRightSite(this Line2d line, Vector2d point, double epsilon)
        {
            var direction = point - line.Origin;
            return Vector2dExtensions.OrthogonalRight(line.Direction).Dot(direction) > -epsilon;
        }
    }
}
