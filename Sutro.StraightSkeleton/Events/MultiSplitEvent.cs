using System.Collections.Generic;
using Sutro.StraightSkeleton.Events.Chains;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Events
{
    internal class MultiSplitEvent : SkeletonEvent
    {
        public readonly List<IChain> Chains;

        public override bool IsObsolete
        {
            get { return false; }
        }

        public MultiSplitEvent(Vector2d point, double distance, List<IChain> chains)
            : base(point, distance)
        {
            Chains = chains;
        }
    }
}
