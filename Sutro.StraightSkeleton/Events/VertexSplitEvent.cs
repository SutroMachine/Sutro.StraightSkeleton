using System;
using Sutro.StraightSkeleton.Circular;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Events
{
    internal class VertexSplitEvent : SplitEvent
    {
        public VertexSplitEvent(Vector2d point, double distance, Vertex parent) :
            base(point, distance, parent, null)
        {
        }

        public override String ToString()
        {
            return "VertexSplitEvent [V=" + V + ", Parent=" +
                   (Parent != null ? Parent.Point.ToString() : "null")
                   + ", Distance=" + Distance + "]";
        }
    }
}