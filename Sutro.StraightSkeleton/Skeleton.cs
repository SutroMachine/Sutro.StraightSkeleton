using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using g3;

namespace Sutro.StraightSkeleton
{
    /// <summary> Represents skeleton algorithm results. </summary>
    public class Skeleton
    {
        /// <summary> Distance points from edges. </summary>
        public ReadOnlyDictionary<Vector2d, double> Distances { get; }

        /// <summary> Result of skeleton algorithm for edge. </summary>
        public ReadOnlyCollection<EdgeResult> Edges { get; }

        /// <summary> Creates instance of <see cref="Skeleton"/>. </summary>
        public Skeleton(IList<EdgeResult> edges, Dictionary<Vector2d, double> distances)
        {
            Edges = new ReadOnlyCollection<EdgeResult>(edges);
            Distances = new ReadOnlyDictionary<Vector2d, double>(distances);
        }
    }
}
