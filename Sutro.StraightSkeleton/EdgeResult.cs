using System.Collections.Generic;
using Sutro.StraightSkeleton.Circular;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton
{
    public class EdgeResult
    {
        public readonly Edge Edge;
        public readonly List<Vector2d> Polygon;

        public EdgeResult(Edge edge, List<Vector2d> polygon)
        {
            Edge = edge;
            Polygon = polygon;
        }
    }
}
