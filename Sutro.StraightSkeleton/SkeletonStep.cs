using System;
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
        public HashSet<CircularList<Vertex>> ActiveVertices { get; set; }
        public List<Edge> Edges { get; set; }
        public List<CircularList<BoundaryEdge>> Boundaries { get; set; }
        public List<Segment2d> Segments { get; set; }

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

            AddSegmentsToSvg(writer);

            var activeVertexPointStyle = SVGWriter.Style.Outline("red", lineWidth);

            foreach (var vertex in ActiveVertices.SelectMany(list => list.Iterate()))
            {
                writer.AddCircle(new Circle2d(vertex.Point, pointDiam * 2), activeVertexPointStyle);
                writer.AddLine(new Segment2d(vertex.Point, vertex.Bisector.PointAt(2)), activeVertexPointStyle);
            }

            var eventQueueLineStyle = SVGWriter.Style.Outline("gray", lineWidth);
            foreach (var skeletonEvent in EventQueue.PeekIterate().Where(e => !e.IsObsolete))
            {
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

            AddEdgesToSvg(writer, ref bounds);
            AddBoundaryEdgeToSvg(writer, ref bounds);

            bounds.Expand(10);
            writer.AddLine(new Segment2d(bounds.GetCorner(0), bounds.GetCorner(1)), SVGWriter.Style.Outline("white", 0));
            writer.AddLine(new Segment2d(bounds.GetCorner(2), bounds.GetCorner(3)), SVGWriter.Style.Outline("white", 0));

            writer.Write(filename);
        }

        public void AddBoundaryEdgeToSvg(SVGWriter writer, ref AxisAlignedBox2d bounds)
        {
            var boundaryEdgeStyle = SVGWriter.Style.Outline("purple", lineWidth);
            var boundaryEdgePointStyle = SVGWriter.Style.Filled("purple");

            foreach (var edge in Boundaries.SelectMany(boundary => boundary.Iterate().Select(e => e.Segment)))
            {
                bounds.Contain(edge.P0);
                bounds.Contain(edge.P1);
                writer.AddLine(edge, boundaryEdgeStyle);
                writer.AddCircle(new Circle2d(edge.P0, 0.15f), boundaryEdgePointStyle);
            }
        }

        public void AddSegmentsToSvg(SVGWriter writer)
        {
            var skeletonLineStyle = SVGWriter.Style.Outline("black", lineWidth * 2);
            foreach (var segment in Segments)
            {
                writer.AddLine(segment, skeletonLineStyle);
            }
        }
    }
}
