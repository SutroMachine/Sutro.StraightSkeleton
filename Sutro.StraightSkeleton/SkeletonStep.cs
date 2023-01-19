using System.Collections.Generic;
using System.Linq;
using g3;
using Sutro.StraightSkeleton.Circular;
using Sutro.StraightSkeleton.Events;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton
{
    internal class SkeletonStep
    {
        public PriorityQueue<SkeletonEvent> EventQueue { get; set; }
        public HashSet<Wavefront> Wavefronts { get; set; }
        public List<Edge> Edges { get; set; }
        public List<BoundaryChain> Boundaries { get; set; }
        public List<Segment2d> RibSegments { get; set; } = new();
        public List<Segment2d> SpineSegments { get; set; } = new();

        private readonly float lineWidth = 0.05f;
        private readonly float pointDiam = 0.15f;

        public void AddEdgesToSvg(SVGWriter writer, ref AxisAlignedBox2d bounds)
        {
            var edgeLineStyle = SVGWriter.Style.Outline("blue", lineWidth);
            var edgePointStyle = SVGWriter.Style.Filled("blue");
            foreach (var edge in Edges)
            {
                bounds.Contain(edge.Begin);
                bounds.Contain(edge.End);
                writer.AddLine(new Segment2d(edge.Begin, edge.End), edgeLineStyle);
                writer.AddCircle(new Circle2d(edge.Begin, 0.15f), edgePointStyle);
            }
        }

        public void ToSvg(string filename)
        {
            var writer = new SVGWriter();

            var bounds = new AxisAlignedBox2d();
            AddEdgesToSvg(writer, ref bounds);
            AddBoundaryEdgeToSvg(writer, ref bounds);

            bounds.Expand(10);

            AddRibSegmentsToSvg(writer);
            AddSpineSegmentsToSvg(writer);

            var activeVertexPointStyle = SVGWriter.Style.Outline("red", lineWidth);

            foreach (var vertex in Wavefronts.SelectMany(list => list.Iterate()))
            {
                if (!bounds.Contains(vertex.Point))
                    continue;
                writer.AddCircle(new Circle2d(vertex.Point, pointDiam * 2), activeVertexPointStyle);
                writer.AddLine(new Segment2d(vertex.Point, vertex.Bisector.PointAt(2)), activeVertexPointStyle);
            }

            var eventQueueLineStyle = SVGWriter.Style.Outline("gray", lineWidth);
            foreach (var skeletonEvent in EventQueue.PeekIterate().Where(e => !e.IsObsolete))
            {
                if (!bounds.Contains(skeletonEvent.V))
                    continue;

                switch (skeletonEvent)
                {
                    case BoundaryEvent boundaryEvent:
                        writer.AddCircle(new Circle2d(boundaryEvent.V, pointDiam * 2), eventQueueLineStyle);
                        writer.AddLine(new Segment2d(boundaryEvent.Parent.Point, boundaryEvent.V), eventQueueLineStyle);
                        break;

                    case SplitEvent splitEvent:
                        writer.AddCircle(new Circle2d(splitEvent.V, pointDiam * 2), eventQueueLineStyle);
                        writer.AddLine(new Segment2d(splitEvent.Parent.Point, splitEvent.V), eventQueueLineStyle);
                        break;
                }
            }

            writer.AddLine(new Segment2d(bounds.GetCorner(0), bounds.GetCorner(1)), SVGWriter.Style.Outline("white", 0));
            writer.AddLine(new Segment2d(bounds.GetCorner(2), bounds.GetCorner(3)), SVGWriter.Style.Outline("white", 0));

            writer.Write(filename);
        }

        public void AddBoundaryEdgeToSvg(SVGWriter writer, ref AxisAlignedBox2d bounds)
        {
            var boundaryEdgeStyle = SVGWriter.Style.Outline("purple", lineWidth);
            var boundaryEdgePointStyle = SVGWriter.Style.Filled("purple");

            foreach (var node in Boundaries.SelectMany(boundary => boundary.EnumerateNodes()))
            {
                bounds.Contain(node.Value);
                writer.AddLine(node.NextEdge.Value.Segment, boundaryEdgeStyle);
                writer.AddCircle(new Circle2d(node.Value, 0.15f), boundaryEdgePointStyle);
            }
        }

        public void AddRibSegmentsToSvg(SVGWriter writer)
        {
            var skeletonLineStyle = SVGWriter.Style.Outline("black", lineWidth * 2);
            foreach (var segment in RibSegments)
            {
                writer.AddLine(segment, skeletonLineStyle);
            }
        }

        public void AddSpineSegmentsToSvg(SVGWriter writer)
        {
            var skeletonLineStyle = SVGWriter.Style.Outline("red", lineWidth * 4);
            foreach (var segment in SpineSegments)
            {
                writer.AddLine(segment, skeletonLineStyle);
            }
        }
    }
}
