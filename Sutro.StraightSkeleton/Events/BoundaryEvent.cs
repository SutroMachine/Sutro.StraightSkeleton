using g3;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events
{
    internal class BoundaryEvent : SkeletonEvent
    {
        public readonly Vertex Parent;

        public override bool IsObsolete => false;

        public BoundaryEvent(Vector2d point, double distance, Vertex parent) : base(point, distance)
        {

        }

    }
}
