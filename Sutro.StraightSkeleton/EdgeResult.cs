using g3;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton
{
    public class EdgeResult
    {
        public readonly Edge Edge;
        public readonly Polygon2d Polygon;

        public EdgeResult(Edge edge, Polygon2d polygon)
        {
            Edge = edge;
            Polygon = polygon;
        }
    }
}
