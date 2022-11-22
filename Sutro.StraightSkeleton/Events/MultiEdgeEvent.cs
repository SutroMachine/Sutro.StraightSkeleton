using Sutro.StraightSkeleton.Events.Chains;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Events
{
    internal class MultiEdgeEvent : SkeletonEvent
    {
        public readonly EdgeChain Chain;

        public override bool IsObsolete { get { return false; } }

        public MultiEdgeEvent(Vector2d point, double distance, EdgeChain chain) : base(point, distance)
        {
            Chain = chain;
        }
    }
}
