using g3;
using Sutro.StraightSkeleton.Chain;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events
{
    internal class BoundaryEvent : SkeletonEvent
    {
        public Vertex Parent { get; }

        public BoundaryChain.Edge BoundaryEdge { get; }

        public override bool IsObsolete => Parent.IsProcessed;

        public BoundaryEvent(Vector2d point, double distance, Vertex parent, BoundaryChain.Edge boundaryEdge) : base(point, distance)
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
