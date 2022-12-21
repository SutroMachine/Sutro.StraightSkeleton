using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using Sutro.StraightSkeleton.Circular;
using Sutro.StraightSkeleton.Path;

namespace Sutro.StraightSkeleton
{
    public class OffsetSeed
    {
        private List<Cell> _cells;

        public OffsetSeed(List<List<Cell>> groupedCells)
        {
        }

        public OffsetSeed(List<Cell> cells)
        {
            _cells = cells;
        }

        public double MaxDistance => _cells.Max(c => c.MaxDistance);

        public List<Segment2d> MakeOffset(double distance)
        {
            var segments = new List<Segment2d>();

            // TODO: Add grouping
            foreach (var cell in _cells)
            {
                if (cell.MaxDistance > distance)
                {
                    segments.Add(new Segment2d(cell.GetStart(distance), cell.GetEnd(distance)));
                }
            }

            return segments;
        }

        public class Cell
        {
            private CellEdge edgesStart;
            private CellEdge edgesEnd;
            public double MaxDistance { get; }

            public Cell(CellEdge edgesStart, CellEdge edgesEnd)
            {
                this.edgesStart = edgesStart;
                this.edgesEnd = edgesEnd;
                MaxDistance = edgesStart.MaxDistance;

                if (!MathUtil.EpsilonEqual(edgesStart.MaxDistance, edgesEnd.MaxDistance, 1e-6))
                {
                    throw new Exception();
                }
            }

            public Vector2d GetStart(double distance)
            {
                return edgesStart.GetPointAt(distance);
            }

            public Vector2d GetEnd(double distance)
            {
                return edgesEnd.GetPointAt(distance);
            }
        }

        private static List<List<Edge>> GroupEdges(List<Edge> edges)
        {
            var unprocessedEdges = new HashSet<Edge>(edges);
            var groups = new List<List<Edge>>();

            while (unprocessedEdges.Count > 0)
            {
                var edge = unprocessedEdges.First();

                unprocessedEdges.Remove(edge);
                groups.Add(new List<Edge>() { edge });

                while (edge.Next != groups[^1][0])
                {
                    edge = unprocessedEdges.First(e => e == edge.Next);
                    groups[^1].Add(edge);
                    unprocessedEdges.Remove(edge);
                }
            }
            return groups;
        }

        internal static OffsetSeed FromFaceQueues(List<FaceQueue> faceQueues)
        {
            // TODO: if edges aren't all from a single polygon (holes, multiple polygons, etc.)
            // they need to be grouped here! This assumes we're getting them in the correct order for a single chain

            var cells = new List<Cell>();

            foreach (var queue in faceQueues)
            {
                var faceNode = queue.First as FaceNode;

                // Find last node reversed
                var lastNodeReverse = queue.First;
                while (lastNodeReverse.Previous != null && lastNodeReverse.Previous != queue.First)
                    lastNodeReverse = lastNodeReverse.Previous;

                // Find last node forward
                var lastNodeForward = queue.First;
                while (lastNodeForward.Next != null && lastNodeForward.Next != queue.First)
                    lastNodeForward = lastNodeForward.Next;

                if (lastNodeForward.Next == null)
                {
                    // Close the path queue
                    lastNodeForward.Next = lastNodeReverse;
                }

                if (lastNodeReverse.Previous == null)
                {
                    // Close the path queue
                    lastNodeReverse.Previous = lastNodeForward;
                }

                var edgesEnd = PropagateForward(faceNode.Next as FaceNode);
                var edgesStart = PropagateBackward(faceNode);
                cells.Add(new Cell(edgesStart, edgesEnd));
            }

            return new OffsetSeed(cells);
        }

        private static CellEdge PropagateForward(FaceNode faceNode)
        {
            var points = new List<(Vector2d, double)>();

            points.Add((faceNode.Vertex.Point, faceNode.Vertex.Distance));

            var currentNode = faceNode;
            while (currentNode.Next != null && (currentNode.Next as FaceNode).Vertex.Distance > currentNode.Vertex.Distance)
            {
                currentNode = currentNode.Next as FaceNode;
                points.Add((currentNode.Vertex.Point, currentNode.Vertex.Distance));
            }
            return new CellEdge(points);
        }

        private static CellEdge PropagateBackward(FaceNode faceNode)
        {
            var points = new List<(Vector2d, double)>();

            points.Add((faceNode.Vertex.Point, faceNode.Vertex.Distance));

            var currentNode = faceNode;
            while (currentNode.Previous != null && (currentNode.Previous as FaceNode).Vertex.Distance > currentNode.Vertex.Distance)
            {
                currentNode = currentNode.Previous as FaceNode;
                points.Add((currentNode.Vertex.Point, currentNode.Vertex.Distance));
            }
            return new CellEdge(points);
        }
    }

    public class CellEdge
    {
        private List<(Vector2d, double)> points;

        public CellEdge(List<(Vector2d, double)> points)
        {
            this.points = points;
            MaxDistance = points[^1].Item2;
        }

        public double MaxDistance { get; }

        public Vector2d GetPointAt(double distance)
        {
            // Should use binary search here
            int i = 0;
            for (; i < points.Count - 1; i++)
            {
                if (points[i].Item2 <= distance && points[i + 1].Item2 >= distance)
                    break;
            }

            // Cache line here?

            var seg = new Segment2d(points[i].Item1, points[i + 1].Item1);
            return seg.PointBetween((distance - points[i].Item2) / (points[i + 1].Item2 - points[i].Item2));
        }
    }
}
