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

        public ReadOnlyCollection<Line2d> UnboundedRays { get; }

        /// <summary> Creates instance of <see cref="Skeleton"/>. </summary>
        public Skeleton(IList<EdgeResult> edges, Dictionary<Vector2d, double> distances, IList<Line2d> unboundedRays = null)
        {
            Edges = new ReadOnlyCollection<EdgeResult>(edges);
            Distances = new ReadOnlyDictionary<Vector2d, double>(distances);
            UnboundedRays = new ReadOnlyCollection<Line2d>(unboundedRays ?? new List<Line2d>());
        }

        public void AddToSvg(SVGWriter writer)
        {
            var polygons = new HashSet<Polygon2d>();
            Edges.Select(edge => polygons.Add(edge.Polygon)).ToList();

            var polyStyle = SVGWriter.Style.Outline("blue", 0.1f);
            var polyCircleStyle = SVGWriter.Style.Filled("blue");

            var points = new HashSet<Vector2d>();
            foreach (var polygon in polygons)
            {
                for (int i = 0; i < polygon.VertexCount; i++)
                {
                    Vector2d begin = polygon[i];
                    Vector2d end = polygon[(i + 1) % polygon.VertexCount];

                    // Skip segments on original edge
                    if (Distances[begin] == 0 && Distances[end] == 0)
                        continue;

                    if (Distances[begin] >= 0 && !points.Contains(begin))
                    {
                        points.Add(begin);
                        writer.AddCircle(new Circle2d(begin, 0.2f), polyCircleStyle);
                    }

                    if (Distances[end] >= 0 && !points.Contains(end))
                    {
                        points.Add(end);
                        writer.AddCircle(new Circle2d(end, 0.2f), polyCircleStyle);
                    }

                    writer.AddLine(new Segment2d(begin, end), polyStyle);
                }
            }

            //var rayStyle = SVGWriter.Style.Outline("green", 0.1f);
            //var rayCircleStyle = SVGWriter.Style.Filled("green");

            //foreach (var ray in UnboundedRays)
            //{
            //    if (Distances[ray.Origin] >= 0)
            //    {
            //        writer.AddCircle(new Circle2d(ray.Origin, 0.2f), rayCircleStyle);
            //    }

            //    writer.AddLine(new Segment2d(ray.Origin, ray.Origin + 20 * ray.Direction), rayStyle);
            //}

            var edgeStyle = SVGWriter.Style.Outline("black", 0.1f);
            var edgeCircleStyle = SVGWriter.Style.Filled("black");

            foreach (var edge in Edges)
            {
                writer.AddLine(new Segment2d(edge.Edge.Begin, edge.Edge.End), edgeStyle);
                writer.AddCircle(new Circle2d(edge.Edge.Begin, 0.3f), edgeCircleStyle);
            }

        }
    }
}
