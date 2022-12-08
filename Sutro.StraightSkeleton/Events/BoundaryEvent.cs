using g3;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events
{
    internal class BoundaryEvent : SkeletonEvent
    {
        public Vertex Parent { get; }

        public override bool IsObsolete => Parent.IsProcessed;

        public BoundaryEvent(Vector2d point, double distance, Vertex parent) : base(point, distance)
        {
            Parent = parent;
        }

        public override string ToString()
        {
            return "BoundaryIntersection [V=" + V + ", Distance=" + Distance + "]";
        }
    }
}
