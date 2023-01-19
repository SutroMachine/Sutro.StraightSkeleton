using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        /// <summary> Central spine of the skeleton. </summary>
        public CurveCollection Spine { get; } = new();

        /// <summary> Seed for creating offsets from the skeleton. </summary>
        public OffsetSeed OffsetSeed { get; internal set; }

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

        public void AddSpineSegments(List<Segment2d> segments)
        {
            // Ideally we wouldn't have to reconstruct connectivity since it's
            // known during the skeleton generation steps, but as a quick-and-dirty
            // approach we can just use DGraph2 functionality to identify curves/loops
            // and connected segments.

            var graph = new DGraph2();

            // DGraph doesn't have a convenient way to check if a Vector2d is already in the graph,
            // so we'll track it in a dictionary as we construct the graph. To avoid issues
            // with comparing floating-point numbers, we'll generate a hash for each vertex based on
            // rounded x & y values, so near-identical vectors have the same hash.

            // Dictionary of (VertexHash, VertexId)
            var vids = new Dictionary<int, int>();

            foreach (var segment in segments)
            {
                int vid0 = GetOrAddVertex(graph, vids, segment.P0);
                int vid1 = GetOrAddVertex(graph, vids, segment.P1);
                graph.AppendEdge(vid0, vid1);
            }

            Spine.Add(DGraph2Util.ExtractCurves(graph));
        }

        private int GetOrAddVertex(DGraph2 graph, Dictionary<int, int> vids, Vector2d vertex)
        {
            var vertexHash = GetVector2dHash(vertex);

            int vid;
            if (!vids.TryGetValue(vertexHash, out vid))
            {
                vid = graph.AppendVertex(vertex);
                vids.Add(vertexHash, vid);
            }
            return vid;
        }

        private const int _vectorHashScale = 1000;

        private int GetVector2dHash(Vector2d vector)
        {
            return HashCode.Combine((int)vector.x * _vectorHashScale, (int)vector.y * _vectorHashScale);
        }
    }
}
