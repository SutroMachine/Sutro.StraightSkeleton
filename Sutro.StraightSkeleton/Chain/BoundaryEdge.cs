using g3;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Chain
{
    internal class BoundaryEdge
    {
        public Segment2d Segment { get; }
        public LineLinear2d LineLinear2d { get; }

        public static BoundaryEdge FromVectors(Vector2d begin, Vector2d end)
        {
            return new BoundaryEdge(begin, end);
        }

        public BoundaryEdge(Vector2d begin, Vector2d end)
        {
            Segment = new Segment2d(begin, end);
            LineLinear2d = new LineLinear2d(begin, end);
        }

        public override string ToString()
        {
            return "BoundaryEdge [p1=" + Segment.P0 + ", p2=" + Segment.P1 + "]";
        }
    }
}
