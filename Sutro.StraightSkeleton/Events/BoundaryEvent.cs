using g3;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events
{
    internal class BoundaryEvent : SkeletonEvent
    {
        public Vertex Parent { get; }

        public BoundaryEdge BoundaryEdge { get; }

        public override bool IsObsolete => Parent.IsProcessed;

        public BoundaryEvent(Vector2d point, double distance, Vertex parent, BoundaryEdge boundaryEdge) : base(point, distance)
        {
            Parent = parent;
            BoundaryEdge = boundaryEdge;
        }

        public override string ToString()
        {
            return "BoundaryIntersection [V=" + V + ", Distance=" + Distance + "]";
        }
    }
}
