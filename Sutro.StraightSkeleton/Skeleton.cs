using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using g3;
using gs;

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

        public void AddToSVG(SVGWriter writer, ref AxisAlignedBox2d bounds)
        {
            foreach (var edge in Edges)
            {
                bounds.Contain(edge.Edge.Begin);
                bounds.Contain(edge.Edge.End);

                foreach (var gp in ClipperUtil.ComputeOffsetPolygon(edge.Polygon, -0.5, true))
                {
                    writer.AddPolygon(gp, SVGWriter.Style.Filled("gray", opacity: 0.25f));
                }
            }

            bounds.Expand(10);
            writer.AddLine(new Segment2d(bounds.GetCorner(0), bounds.GetCorner(1)), SVGWriter.Style.Outline("white", 0));
            writer.AddLine(new Segment2d(bounds.GetCorner(2), bounds.GetCorner(3)), SVGWriter.Style.Outline("white", 0));
        }
    }
}
