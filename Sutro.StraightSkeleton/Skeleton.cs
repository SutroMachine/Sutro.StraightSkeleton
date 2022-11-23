using System.Collections.Generic;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton
{
    /// <summary> Represents skeleton algorithm results. </summary>
    public class Skeleton
    {
        /// <summary> Distance points from edges. </summary>
        public readonly Dictionary<Vector2d, double> Distances;

        /// <summary> Result of skeleton algorithm for edge. </summary>
        public readonly List<EdgeResult> Edges;

        /// <summary> Creates instance of <see cref="Skeleton"/>. </summary>
        public Skeleton(List<EdgeResult> edges, Dictionary<Vector2d, double> distances)
        {
            Edges = edges;
            Distances = distances;
        }
    }
}
