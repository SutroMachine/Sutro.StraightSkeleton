﻿using System;
using g3;

namespace Sutro.StraightSkeleton.Events
{
    internal abstract class SkeletonEvent
    {
        public Vector2d V;

        public double Distance { get; protected set; }
        public abstract bool IsObsolete { get; }

        public override String ToString()
        {
            return "IntersectEntry [V=" + V + ", Distance=" + Distance + "]";
        }

        protected SkeletonEvent(Vector2d point, double distance)
        {
            V = point;
            Distance = distance;
        }
    }
}
