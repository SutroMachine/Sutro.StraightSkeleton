using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using Sutro.StraightSkeleton.Circular;
using Sutro.StraightSkeleton.Events;
using Sutro.StraightSkeleton.Events.Chains;
using Sutro.StraightSkeleton.Path;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton
{
    public class ExternalSkeletonBuilder : SkeletonBuilderBase
    {
        protected override Line2d CalcInitialBisector(Vector2d point, Edge edgeBefore, Edge edgeAfter)
        {
            // Reverse direction to make the ray external instead of internal
            return new Line2d(point, -CalcVectorBisector(edgeBefore.Norm, edgeAfter.Norm));
        }

        public Skeleton Build(GeneralPolygon2d gpoly, GeneralPolygon2d boundary)
        {
            return Build(gpoly);
        }
    }

    public class SkeletonBuilder : SkeletonBuilderBase
    {
        protected override Line2d CalcInitialBisector(Vector2d point, Edge edgeBefore, Edge edgeAfter)
        {
            return new Line2d(point, CalcVectorBisector(edgeBefore.Norm, edgeAfter.Norm));
        }
    }

    /// <summary>
    ///     Straight skeleton algorithm implementation. Base on highly modified Petr
    ///     Felkel and Stepan Obdrzalek algorithm.
    /// </summary>
    /// <remarks>
    ///     This is .NET adopted port of java implementation from kendzi-straight-skeleton library.
    /// </remarks>
    ///
    public abstract class SkeletonBuilderBase
    {
        internal readonly List<CircularList<BoundaryEdge>> _boundaryEdgeLoops = new List<CircularList<BoundaryEdge>>();

        public SkeletonBuilderBase AddBoundary(Polygon2d boundary)
        {
            var boundaryLoop = new CircularList<BoundaryEdge>();

            var size = boundary.VertexCount;
            for (var i = 0; i < size; i++)
            {
                var j = (i + 1) % size;
                boundaryLoop.AddLast(new BoundaryEdge(boundary[i], boundary[j]));
            }
            _boundaryEdgeLoops.Add(boundaryLoop);
            return this;
        }

        /// <summary> Creates straight skeleton for given polygon. </summary>
        public Skeleton Build(List<Vector2d> points)
        {
            return Build(new GeneralPolygon2d(new Polygon2d(points)));
        }

        public Skeleton Build(Polygon2d polygon)
        {
            return Build(new GeneralPolygon2d(polygon));
        }

        /// <summary> Creates straight skeleton for given polygon with holes. </summary>
        public Skeleton Build(List<Vector2d> outer, List<List<Vector2d>> holes)
        {
            var gpoly = new GeneralPolygon2d(new Polygon2d(outer));

            if (holes != null)
            {
                foreach (var hole in holes)
                {
                    gpoly.AddHole(new Polygon2d(hole), bCheckContainment: false, bCheckOrientation: false);
                }
            }
            return Build(gpoly);
        }

        /// <summary> Creates straight skeleton for given polygon with holes. </summary>
        public Skeleton Build(GeneralPolygon2d gpoly, string svgPrefix = "")
        {
            ValidateGeneralPolygon(gpoly);
            gpoly.EnforceCounterClockwise();

            var queue = new PriorityQueue<SkeletonEvent>(3, new SkeletonEventDistanceComparer());
            var sLav = new HashSet<CircularList<Vertex>>();
            var faces = new List<FaceQueue>();
            var edges = new List<Edge>();

            var step = new SkeletonStep()
            {
                EventQueue = queue,
                ActiveVertices = sLav,
                Edges = edges,
                Boundaries = _boundaryEdgeLoops,
                Segments = new List<Segment2d>(),
            };

            InitSlav(gpoly.Outer, sLav, edges, faces);

            foreach (var inner in gpoly.Holes)
            {
                InitSlav(inner, sLav, edges, faces);
            }

            InitEvents(sLav, queue, edges);

            int stepCount = 0;
            step.ToSvg($"{svgPrefix}-{stepCount}.svg");

            var leftBoundaryFaceIntersections = new Dictionary<FaceQueue, BoundaryEvent>();
            var rightBoundaryFaceIntersections = new Dictionary<FaceQueue, BoundaryEvent>();

            var count = 0;
            while (!queue.Empty)
            {
                // start processing skeleton level
                count = AssertMaxNumberOfInteraction(count);
                var levelHeight = queue.Peek().Distance;
                foreach (var levelEvent in LoadAndGroupLevelEvents(queue))
                {
                    // event is outdated some of parent vertex was processed before
                    if (levelEvent.IsObsolete)
                        continue;

                    switch (levelEvent)
                    {
                        case EdgeEvent edgeEvent:
                            throw new InvalidOperationException("All edge@events should be converted to " +
                                                                "MultiEdgeEvents for given level");
                        case SplitEvent splitEvent:
                            throw new InvalidOperationException("All split events should be converted to " +
                                                                "MultiSplitEvents for given level");

                        case MultiSplitEvent multiSplitEvent:
                            MultiSplitEvent(multiSplitEvent, sLav, queue, edges, step.Segments);
                            break;

                        case BoundaryEvent boundaryEvent:
                            if (boundaryEvent.Parent.IsProcessed)
                                break;
                            boundaryEvent.Parent.IsProcessed = true;
                            var vertex = new Vertex(boundaryEvent.V, 0, new Line2d(), null, null);
                            step.Segments.Add(new Segment2d(boundaryEvent.Parent.Point, boundaryEvent.V));

                            leftBoundaryFaceIntersections[boundaryEvent.Parent.RightFace.FaceQueue] = boundaryEvent;
                            rightBoundaryFaceIntersections[boundaryEvent.Parent.LeftFace.FaceQueue] = boundaryEvent;

                            //boundaryEvent.Parent.RightFace.AddPush(new FaceNode(vertex));
                            //boundaryEvent.Parent.LeftFace.AddPush(new FaceNode(vertex));
                            break;

                        case PickEvent pickEvent:
                            PickEvent(pickEvent);
                            break;

                        case MultiEdgeEvent multiEdgeEvent:
                            MultiEdgeEvent(multiEdgeEvent, queue, edges, step.Segments);
                            break;

                        default:
                            throw new InvalidOperationException("Unknown event type: " + levelEvent.GetType());
                    }
                }

                ProcessTwoNodeLavs(sLav);
                RemoveEventsUnderHeight(queue, levelHeight);
                RemoveEmptyLav(sLav);

                ++stepCount;
                step.ToSvg($"{svgPrefix}-{stepCount}.svg");
            }

            AddBoundaryEdgesToFaces(faces, leftBoundaryFaceIntersections, rightBoundaryFaceIntersections);

            var skeleton = AddFacesToOutput(faces);

            var finalWriter = new SVGWriter();
            var bounds = new AxisAlignedBox2d();

            step.AddEdgesToSvg(finalWriter, ref bounds);
            step.AddBoundaryEdgeToSvg(finalWriter, ref bounds);
            step.AddSegmentsToSvg(finalWriter);

            skeleton.AddToSVG(finalWriter, ref bounds);
            finalWriter.Write($"{svgPrefix}-FINAL.svg");

            // Clean up for next usage
            _boundaryEdgeLoops.Clear();

            return skeleton;
        }

        private void AddBoundaryEdgesToFaces(List<FaceQueue> faces, Dictionary<FaceQueue, BoundaryEvent> leftBoundaryFaceIntersections, Dictionary<FaceQueue, BoundaryEvent> rightBoundaryFaceIntersections)
        {
            foreach (var face in faces.Where(f => leftBoundaryFaceIntersections.ContainsKey(f) && rightBoundaryFaceIntersections.ContainsKey(f)))
            {
                var left = leftBoundaryFaceIntersections[face];
                var right = rightBoundaryFaceIntersections[face];

                var node = face.First;
                while (node.Next != null)
                {
                    node = node.Next;
                }

                var faceNode = node as FaceNode;
                if (faceNode != null && right.Parent != faceNode.Vertex)
                    throw new Exception();

                var newFaceNode = new FaceNode(new Vertex(right.V, 0, new Line2d(), null, null));
                faceNode.AddPush(newFaceNode);
                faceNode = newFaceNode;
                var boundaryEdge = right.BoundaryEdge;

                while (left.BoundaryEdge != boundaryEdge)
                {
                    newFaceNode = new FaceNode(new Vertex(boundaryEdge.Segment.P0, 0, new Line2d(), null, null));
                    faceNode.AddPush(newFaceNode);
                    faceNode = newFaceNode;

                    boundaryEdge = boundaryEdge.Previous as BoundaryEdge;
                }

                faceNode.AddPush(new FaceNode(new Vertex(left.V, 0, new Line2d(), null, null)));
            }
        }

        internal static bool EdgeBehindBisector(Line2d bisector, LineLinear2d edge)
        {
            // Simple intersection test between the bisector starting at V and the
            // whole line containing the currently tested line segment ei rejects
            // the line segments laying "behind" the vertex V
            return bisector.Collide(edge, SplitEpsilon) == Vector2d.MinValue;
        }

        /// <summary>
        ///     Check if given point is on one of edge bisectors. If so this is vertex
        ///     split event. This event need two opposite edges to process but second
        ///     (next) edge can be take from edges list and it is next edge on list.
        /// </summary>
        /// <param name="point">Point of event.</param>
        /// <param name="edge">candidate for opposite edge.</param>
        /// <returns>previous opposite edge if it is vertex split event.</returns>
        protected static Edge VertexOpositeEdge(Vector2d point, Edge edge)
        {
            if (edge.BisectorNext.WhichSide(point, SplitEpsilon) == 0)
                return edge;

            if (edge.BisectorPrevious.WhichSide(point, SplitEpsilon) == 0)
                return edge.Previous as Edge;

            return null;
        }

        // Error epsilon.
        private const double SplitEpsilon = 1E-6;

        private static void AddEventToGroup(HashSet<Vertex> parentGroup, SkeletonEvent skeletonEvent)
        {
            if (skeletonEvent is SplitEvent splitEvent)
            {
                parentGroup.Add(splitEvent.Parent);
            }
            else if (skeletonEvent is EdgeEvent edgeEvent)
            {
                parentGroup.Add(edgeEvent.PreviousVertex);
                parentGroup.Add(edgeEvent.NextVertex);
            }
            else if (skeletonEvent is BoundaryEvent boundaryEvent)
            {
                parentGroup.Add(boundaryEvent.Parent);
            }
        }

        private static void AddFaceBack(Vertex newVertex, Vertex va, Vertex vb)
        {
            var fn = new FaceNode(newVertex);
            va.RightFace.AddPush(fn);
            FaceQueueUtil.ConnectQueues(fn, vb.LeftFace);
        }

        private static void AddFaceLeft(Vertex newVertex, Vertex va)
        {
            var fn = new FaceNode(newVertex);
            va.LeftFace.AddPush(fn);
            newVertex.LeftFace = fn;
        }

        private static void AddFaceRight(Vertex newVertex, Vertex vb)
        {
            var fn = new FaceNode(newVertex);
            vb.RightFace.AddPush(fn);
            newVertex.RightFace = fn;
        }

        private static Skeleton AddFacesToOutput(List<FaceQueue> faces)
        {
            var edgeOutputs = new List<EdgeResult>();
            var distances = new Dictionary<Vector2d, double>();
            foreach (var face in faces)
            {
                if (face.Size > 0)
                {
                    var faceList = new Polygon2d();
                    foreach (var vertex in face.Iterate().Select(fn => fn.Vertex))
                    {
                        faceList.AppendVertex(vertex.Point);
                        if (!distances.ContainsKey(vertex.Point))
                            distances.Add(vertex.Point, vertex.Distance);
                    }
                    edgeOutputs.Add(new EdgeResult(face.Edge, faceList));
                }
            }
            return new Skeleton(edgeOutputs, distances);
        }

        private static void AddMultiBackFaces(List<EdgeEvent> edgeList, Vertex edgeVertex)
        {
            foreach (var edgeEvent in edgeList)
            {
                var leftVertex = edgeEvent.PreviousVertex;
                leftVertex.IsProcessed = true;
                LavUtil.RemoveFromLav(leftVertex);

                var rightVertex = edgeEvent.NextVertex;
                rightVertex.IsProcessed = true;
                LavUtil.RemoveFromLav(rightVertex);

                AddFaceBack(edgeVertex, leftVertex, rightVertex);
            }
        }

        private static FaceNode AddSplitFaces(FaceNode lastFaceNode, IChain chainBegin,
            IChain chainEnd, Vertex newVertex)
        {
            if (chainBegin is SingleEdgeChain)
            {
                // When chain is generated by opposite edge we need to share face
                // between two chains. Number of that chains shares is always odd.

                // right face
                if (lastFaceNode == null)
                {
                    // Vertex generated by opposite edge share three faces, but
                    // vertex can store only left and right face. So we need to
                    // create vertex clone to store additional back face.
                    var beginVertex = CreateOppositeEdgeVertex(newVertex);

                    // same face in two vertex, original and in opposite edge clone
                    newVertex.RightFace = beginVertex.RightFace;
                    lastFaceNode = beginVertex.LeftFace;
                }
                else
                {
                    // face queue exist simply assign it to new node
                    if (newVertex.RightFace != null)
                        throw new InvalidOperationException("newVertex.RightFace should be null");

                    newVertex.RightFace = lastFaceNode;
                    lastFaceNode = null;
                }
            }
            else
            {
                var beginVertex = chainBegin.CurrentVertex;
                // right face
                AddFaceRight(newVertex, beginVertex);
            }

            if (chainEnd is SingleEdgeChain)
            {
                // left face
                if (lastFaceNode == null)
                {
                    // Vertex generated by opposite edge share three faces, but
                    // vertex can store only left and right face. So we need to
                    // create vertex clone to store additional back face.
                    var endVertex = CreateOppositeEdgeVertex(newVertex);

                    // same face in two vertex, original and in opposite edge clone
                    newVertex.LeftFace = endVertex.LeftFace;
                    lastFaceNode = endVertex.LeftFace;
                }
                else
                {
                    // face queue exist simply assign it to new node
                    if (newVertex.LeftFace != null)
                        throw new InvalidOperationException("newVertex.LeftFace should be null.");
                    newVertex.LeftFace = lastFaceNode;

                    lastFaceNode = null;
                }
            }
            else
            {
                var endVertex = chainEnd.CurrentVertex;
                // left face
                AddFaceLeft(newVertex, endVertex);
            }
            return lastFaceNode;
        }

        private static int AssertMaxNumberOfInteraction(int count)
        {
            count++;
            if (count > 10000)
                throw new InvalidOperationException("Too many interaction: bug?");
            return count;
        }

        protected static Line2d CalcBisector(Vector2d p, Edge e1, Edge e2)
        {
            return new Line2d(p, CalcVectorBisector(e1.Norm, e2.Norm));
        }

        private static SplitCandidate CalcCandidatePointForSplit(Vertex vertex, Edge edge)
        {
            var vertexEdge = ChoseLessParallelVertexEdge(vertex, edge);
            if (vertexEdge == null)
                return null;

            var vertexEdteNormNegate = vertexEdge.Norm;
            var edgesBisector = CalcVectorBisector(vertexEdteNormNegate, edge.Norm);
            var edgesCollide = vertexEdge.LineLinear2d.Collide(edge.LineLinear2d);

            // Check should be performed to exclude the case when one of the
            // line segments starting at V is parallel to ei.
            if (edgesCollide == Vector2d.MinValue)
            {
                throw new InvalidOperationException("Ups this should not happen");
            }

            var edgesBisectorLine = new Line2d(edgesCollide, edgesBisector).CreateLinearForm();

            // Compute the coordinates of the candidate point Bi as the intersection
            // between the bisector at V and the axis of the angle between one of
            // the edges starting at V and the tested line segment ei
            var candidatePoint = vertex.Bisector.Collide(edgesBisectorLine, SplitEpsilon);

            if (candidatePoint == Vector2d.MinValue)
                return null;

            if (edge.BisectorPrevious.IsOnRightSite(candidatePoint, SplitEpsilon)
                && edge.BisectorNext.IsOnLeftSite(candidatePoint, SplitEpsilon))
            {
                var distance = CalcDistance(candidatePoint, edge);

                if (edge.BisectorPrevious.IsOnLeftSite(candidatePoint, SplitEpsilon))
                    return new SplitCandidate(candidatePoint, distance, null, edge.Begin);
                if (edge.BisectorNext.IsOnRightSite(candidatePoint, SplitEpsilon))
                    return new SplitCandidate(candidatePoint, distance, null, edge.Begin);

                return new SplitCandidate(candidatePoint, distance, edge, Vector2d.MinValue);
            }
            return null;
        }

        private static double CalcDistance(Vector2d intersect, Edge currentEdge)
        {
            var edge = currentEdge.End - currentEdge.Begin;
            var vector = intersect - currentEdge.Begin;

            var pointOnVector = VectorExtensions.OrthogonalProjection(edge, vector);
            return vector.Distance(pointOnVector);
        }

        private static List<SplitCandidate> CalcOppositeEdges(Vertex vertex, List<Edge> edges)
        {
            var ret = new List<SplitCandidate>();
            foreach (var edgeEntry in edges)
            {
                var edge = edgeEntry.LineLinear2d;
                // check if edge is behind bisector
                if (EdgeBehindBisector(vertex.Bisector, edge))
                    continue;

                // compute the coordinates of the candidate point Bi
                var candidatePoint = CalcCandidatePointForSplit(vertex, edgeEntry);
                if (candidatePoint != null)
                    ret.Add(candidatePoint);
            }
            ret.Sort(new SplitCandidateComparer());
            return ret;
        }

        private IEnumerable<BoundaryEvent> ComputeBoundaryEvents(Vertex vertex)
        {
            foreach (var loop in _boundaryEdgeLoops)
            {
                foreach (var boundaryEdge in loop.Iterate())
                {
                    // check if edge is behind bisector
                    if (EdgeBehindBisector(vertex.Bisector, boundaryEdge.LineLinear2d))
                        continue;

                    // compute the coordinates of the intersection point
                    var intersection = new IntrLine2Segment2(vertex.Bisector, boundaryEdge.Segment);
                    intersection.Compute();

                    if (intersection.Result == IntersectionResult.Intersects)
                    {
                        yield return new BoundaryEvent(intersection.Point, intersection.Parameter + vertex.Distance, vertex, boundaryEdge);
                    }
                }
            }
        }

        protected static Vector2d CalcVectorBisector(Vector2d norm1, Vector2d norm2)
        {
            return VectorExtensions.BisectorNormalized(norm1, norm2);
        }

        private static Vertex ChooseOppositeEdgeLav(List<Vertex> edgeLavs, Edge oppositeEdge, Vector2d center)
        {
            if (!edgeLavs.Any())
                return null;

            if (edgeLavs.Count == 1)
                return edgeLavs[0];

            var edgeStart = oppositeEdge.Begin;
            var edgeNorm = oppositeEdge.Norm;
            var centerVector = center - edgeStart;
            var centerDot = edgeNorm.Dot(centerVector);
            foreach (var end in edgeLavs)
            {
                var begin = end.Previous as Vertex;

                var beginVector = begin.Point - edgeStart;
                var endVector = end.Point - edgeStart;

                var beginDot = edgeNorm.Dot(beginVector);
                var endDot = edgeNorm.Dot(endVector);

                // Make projection of center, begin and end into edge. Begin and end
                // are vertex chosen by opposite edge (then point to opposite edge).
                // Chose lav only when center is between begin and end. Only one lav
                // should meet criteria.
                if (beginDot < centerDot && centerDot < endDot ||
                    beginDot > centerDot && centerDot > endDot)
                    return end;
            }

            // Additional check if center is inside lav
            foreach (var end in edgeLavs)
            {
                var size = end.List.Size;
                var points = new List<Vector2d>(size);
                var next = end;
                for (var i = 0; i < size; i++)
                {
                    points.Add(next.Point);
                    next = next.Next as Vertex;
                }

                var poly = new Polygon2d(points);

                if (poly.Contains(center))
                    return end;
            }
            throw new InvalidOperationException("Could not find lav for opposite edge, it could be correct " +
                                                "but need some test data to check.");
        }

        private static Edge ChoseLessParallelVertexEdge(Vertex vertex, Edge edge)
        {
            var edgeA = vertex.PreviousEdge;
            var edgeB = vertex.NextEdge;

            var vertexEdge = edgeA;

            var edgeADot = Math.Abs(edge.Norm.Dot(edgeA.Norm));
            var edgeBDot = Math.Abs(edge.Norm.Dot(edgeB.Norm));

            // both lines are parallel to given edge
            if (edgeADot + edgeBDot >= 2 - SplitEpsilon)
                return null;

            // Simple check should be performed to exclude the case when one of
            // the line segments starting at V (vertex) is parallel to e_i
            // (edge) we always chose edge which is less parallel.
            if (edgeADot > edgeBDot)
                vertexEdge = edgeB;

            return vertexEdge;
        }

        /// <summary>
        ///     Calculate two new edge events for given vertex. events are generated
        ///     using current, previous and next vertex in current lav. When two edge
        ///     events are generated distance from source is check. To queue is added
        ///     only closer event or both if they have the same distance.
        /// </summary>
        private static double ComputeCloserEdgeEvent(Vertex vertex, PriorityQueue<SkeletonEvent> queue)
        {
            var nextVertex = vertex.Next as Vertex;
            var previousVertex = vertex.Previous as Vertex;

            var point = vertex.Point;

            // We need to chose closer edge event. When two evens appear in epsilon
            // we take both. They will create single MultiEdgeEvent.
            var point1 = ComputeIntersectionBisectors(vertex, nextVertex);
            var point2 = ComputeIntersectionBisectors(previousVertex, vertex);

            if (point1 == Vector2d.MinValue && point2 == Vector2d.MinValue)
                return -1;

            var distance1 = double.MaxValue;
            var distance2 = double.MaxValue;

            if (point1 != Vector2d.MinValue)
                distance1 = point.DistanceSquared(point1);
            if (point2 != Vector2d.MinValue)
                distance2 = point.DistanceSquared(point2);

            if (Math.Abs(distance1 - SplitEpsilon) < distance2)
                queue.Add(CreateEdgeEvent(point1, vertex, nextVertex));
            if (Math.Abs(distance2 - SplitEpsilon) < distance1)
                queue.Add(CreateEdgeEvent(point2, previousVertex, vertex));

            return distance1 < distance2 ? distance1 : distance2;
        }

        private static void ComputeEdgeEvents(Vertex previousVertex, Vertex nextVertex,
            PriorityQueue<SkeletonEvent> queue)
        {
            var point = ComputeIntersectionBisectors(previousVertex, nextVertex);
            if (point != Vector2d.MinValue)
                queue.Add(CreateEdgeEvent(point, previousVertex, nextVertex));
        }

        private void ComputeEvents(Vertex vertex, PriorityQueue<SkeletonEvent> queue, List<Edge> edges)
        {
            var distanceSquared = ComputeCloserEdgeEvent(vertex, queue);
            queue.AddRange(ComputeSplitEvents(vertex, edges, distanceSquared));
            queue.AddRange(ComputeBoundaryEvents(vertex));
        }

        private static Vector2d ComputeIntersectionBisectors(Vertex vertexPrevious, Vertex vertexNext)
        {
            var intersection = new IntrLine2Line2(vertexPrevious.Bisector, vertexNext.Bisector);
            intersection.Compute();

            if (intersection.Result == IntersectionResult.Intersects &&
                intersection.Segment1Parameter > 0 &&
                intersection.Segment2Parameter > 0 &&
                intersection.Point != vertexPrevious.Point &&
                intersection.Point != vertexNext.Point)
            {
                return intersection.Point;
            }

            return Vector2d.MinValue;
        }

        private static IEnumerable<SkeletonEvent> ComputeSplitEvents(Vertex vertex, List<Edge> edges,
            double distanceSquared)
        {
            var source = vertex.Point;
            var oppositeEdges = CalcOppositeEdges(vertex, edges);

            // check if it is vertex split event
            foreach (var oppositeEdge in oppositeEdges)
            {
                var point = oppositeEdge.Point;

                if (Math.Abs(distanceSquared - (-1)) > SplitEpsilon &&
                    source.DistanceSquared(point) > distanceSquared + SplitEpsilon)
                {
                    // Current split event distance from source of event is
                    // greater then for edge event. Split event can be reject.
                    // Distance from source is not the same as distance for
                    // edge. Two events can have the same distance to edge but
                    // they will be in different distance form its source.
                    // Unnecessary events should be reject otherwise they cause
                    // problems for degenerate cases.
                    continue;
                }

                // check if it is vertex split event
                if (oppositeEdge.OppositePoint != Vector2d.MinValue)
                {
                    // some of vertex event can share the same opposite point
                    yield return new VertexSplitEvent(point, oppositeEdge.Distance, vertex);
                    continue;
                }
                yield return new SplitEvent(point, oppositeEdge.Distance, vertex, oppositeEdge.OppositeEdge);
            }
        }

        private static void CorrectBisectorDirection(Line2d bisector, Vertex beginNextVertex,
            Vertex endPreviousVertex, Edge beginEdge, Edge endEdge)
        {
            // New bisector for vertex is created using connected edges. For
            // parallel edges numerical error may appear and direction of created
            // bisector is wrong. It for parallel edges direction of edge need to be
            // corrected using location of vertex.
            var beginEdge2 = beginNextVertex.PreviousEdge;
            var endEdge2 = endPreviousVertex.NextEdge;

            //if (beginEdge != beginEdge2 || endEdge != endEdge2)
            //    throw new InvalidOperationException();

            // Check if edges are parallel and in opposite direction to each other.
            if (beginEdge.Norm.Dot(endEdge.Norm) < -0.97)
            {
                var n1 = VectorExtensions.FromTo(endPreviousVertex.Point, bisector.Origin).Normalized;
                var n2 = VectorExtensions.FromTo(bisector.Origin, beginNextVertex.Point).Normalized;
                var bisectorPrediction = CalcVectorBisector(n1, n2);

                // Bisector is calculated in opposite direction to edges and center.
                if (bisector.Direction.Dot(bisectorPrediction) < 0)
                    bisector.Direction *= -1;
            }
        }

        /// <summary>
        ///     Create chains of events from cluster. Cluster is set of events which meet
        ///     in the same result point. Try to connect all event which share the same
        ///     vertex into chain. events in chain are sorted. If events don't share
        ///     vertex, returned chains contains only one event.
        /// </summary>
        /// <param name="cluster">Set of event which meet in the same result point</param>
        /// <returns>chains of events</returns>
        private static List<IChain> CreateChains(List<SkeletonEvent> cluster)
        {
            var edgeCluster = new HashSet<EdgeEvent>();
            var splitCluster = new List<SplitEvent>();
            var vertexEventsParents = new HashSet<Vertex>();

            foreach (var skeletonEvent in cluster)
            {
                switch (skeletonEvent)
                {
                    case EdgeEvent edgeEvent:
                        edgeCluster.Add(edgeEvent);
                        break;

                    case VertexSplitEvent _:
                        // It will be processed in next loop to find unique split
                        // events for one parent.
                        break;

                    case SplitEvent splitEvent:
                        // If vertex and split event exist for the same parent
                        // vertex and at the same level always prefer split.
                        vertexEventsParents.Add(splitEvent.Parent);
                        splitCluster.Add(splitEvent);
                        break;

                    default:
                        break;
                }
            }

            foreach (var skeletonEvent in cluster)
            {
                if (skeletonEvent is VertexSplitEvent vertexSplitEvent &&
                    !vertexEventsParents.Contains(vertexSplitEvent.Parent))
                {
                    // It can be created multiple vertex events for one parent.
                    // Its is caused because two edges share one vertex and new
                    //event will be added to both of them. When processing we
                    // need always group them into one per vertex. Always prefer
                    // split events over vertex events.
                    vertexEventsParents.Add(vertexSplitEvent.Parent);
                    splitCluster.Add(vertexSplitEvent);
                }
            }

            var edgeChains = new List<EdgeChain>();

            // We need to find all connected edge events, and create chains from
            // them. Two event are assumed as connected if next parent of one
            // event is equal to previous parent of second event.
            while (edgeCluster.Count > 0)
                edgeChains.Add(new EdgeChain(CreateEdgeChain(edgeCluster)));

            var chains = new List<IChain>(edgeChains.Count);
            foreach (var edgeChain in edgeChains)
                chains.Add(edgeChain);

            while (splitCluster.Any())
            {
                var split = splitCluster[0];
                splitCluster.RemoveAt(0);

                // check if chain is split type
                if (edgeChains.Any(chain => IsInEdgeChain(split, chain)))
                {
                    continue;
                }

                // split event is not part of any edge chain, it should be added as
                // a single element chain.
                chains.Add(new SplitChain(split));
            }

            // Return list of chains with type. Possible types are edge chain,
            // closed edge chain, split chain. Closed edge chain will produce pick
            //event. Always it can exist only one closed edge chain for point
            // cluster.
            return chains;
        }

        private static List<EdgeEvent> CreateEdgeChain(HashSet<EdgeEvent> unprocessedEdgeEvents)
        {
            var seed = unprocessedEdgeEvents.First();
            unprocessedEdgeEvents.Remove(seed);

            // Find all successors of edge event
            var successorEdges = new List<EdgeEvent>();
            var endVertex = seed.NextVertex;

            while (unprocessedEdgeEvents.Count > 0)
            {
                var nextEdge = unprocessedEdgeEvents.FirstOrDefault(e => e.PreviousVertex == endVertex);
                if (nextEdge == null)
                {
                    break;
                }
                unprocessedEdgeEvents.Remove(nextEdge);
                successorEdges.Add(nextEdge);
                endVertex = nextEdge.NextVertex;
            }

            // Find all predecessors of edge event
            var predecessorEdges = new List<EdgeEvent>();
            var beginVertex = seed.PreviousVertex;

            while (unprocessedEdgeEvents.Count > 0)
            {
                var previousEdge = unprocessedEdgeEvents.FirstOrDefault(e => e.NextVertex == beginVertex);
                if (previousEdge == null)
                {
                    break;
                }
                unprocessedEdgeEvents.Remove(previousEdge);
                predecessorEdges.Add(previousEdge);
                beginVertex = previousEdge.PreviousVertex;
            }

            // Combine into a single list
            var edgeList = new List<EdgeEvent>(predecessorEdges.Count + 1 + successorEdges.Count);
            for (int i = predecessorEdges.Count - 1; i >= 0; i--)
            {
                edgeList.Add(predecessorEdges[i]);
            }
            edgeList.Add(seed);
            edgeList.AddRange(successorEdges);

            return edgeList;
        }

        private static SkeletonEvent CreateEdgeEvent(Vector2d point, Vertex previousVertex, Vertex nextVertex)
        {
            return new EdgeEvent(point, CalcDistance(point, previousVertex.NextEdge), previousVertex, nextVertex);
        }

        private static SkeletonEvent CreateLevelEvent(Vector2d @eventCenter, double distance,
            List<SkeletonEvent> @eventCluster)
        {
            var chains = CreateChains(eventCluster);

            if (chains.Count == 1)
            {
                var chain = chains[0];
                if (chain.ChainType == ChainType.ClosedEdge)
                    return new PickEvent(eventCenter, distance, (EdgeChain)chain);
                if (chain.ChainType == ChainType.Edge)
                    return new MultiEdgeEvent(eventCenter, distance, (EdgeChain)chain);
                if (chain.ChainType == ChainType.Split)
                    return new MultiSplitEvent(eventCenter, distance, chains);
            }

            if (chains.Any(chain => chain.ChainType == ChainType.ClosedEdge))
                throw new InvalidOperationException("Found closed chain of events for single point, " +
                                                    "but found more then one chain");
            return new MultiSplitEvent(eventCenter, distance, chains);
        }

        private static Vertex CreateMultiSplitVertex(Edge nextEdge, Edge previousEdge, Vector2d center, double distance)
        {
            var bisector = CalcBisector(center, previousEdge, nextEdge);
            // edges are mirrored for event
            return new Vertex(center, distance, bisector, previousEdge, nextEdge);
        }

        private static void CreateOppositeEdgeChains(HashSet<CircularList<Vertex>> sLav,
            List<IChain> chains, Vector2d center)
        {
            // Add chain created from opposite edge, this chain have to be
            // calculated during processing @event because lav could change during
            // processing another @events on the same level
            var oppositeEdges = new HashSet<Edge>();

            var oppositeEdgeChains = new List<IChain>();
            var chainsForRemoval = new List<IChain>();

            foreach (var chain in chains)
            {
                // add opposite edges as chain parts
                if (chain is SplitChain splitChain)
                {
                    var oppositeEdge = splitChain.OppositeEdge;
                    if (oppositeEdge != null && !oppositeEdges.Contains(oppositeEdge))
                    {
                        // find lav vertex for opposite edge
                        var nextVertex = FindOppositeEdgeLav(sLav, oppositeEdge, center);
                        if (nextVertex != null)
                            oppositeEdgeChains.Add(new SingleEdgeChain(oppositeEdge, nextVertex));
                        else
                        {
                            FindOppositeEdgeLav(sLav, oppositeEdge, center);
                            chainsForRemoval.Add(chain);
                        }
                        oppositeEdges.Add(oppositeEdge);
                    }
                }
            }

            // if opposite edge can't be found in active lavs then split chain with
            // that edge should be removed
            foreach (var chain in chainsForRemoval)
                chains.Remove(chain);

            chains.AddRange(oppositeEdgeChains);
        }

        private static Vertex CreateOppositeEdgeVertex(Vertex newVertex)
        {
            // When opposite edge is processed we need to create copy of vertex to
            // use in opposite face. When opposite edge chain occur vertex is shared
            // by additional output face.
            var vertex = new Vertex(newVertex.Point, newVertex.Distance, newVertex.Bisector,
                newVertex.PreviousEdge, newVertex.NextEdge);

            // create new empty node queue
            var fn = new FaceNode(vertex);
            vertex.LeftFace = fn;
            vertex.RightFace = fn;

            // add one node for queue to present opposite site of edge split@event
            var rightFace = new FaceQueue();
            rightFace.AddFirst(fn);

            return vertex;
        }

        private static List<Vertex> FindEdgeLavs(HashSet<CircularList<Vertex>> sLav, Edge oppositeEdge,
            CircularList<Vertex> skippedLav)
        {
            var edgeLavs = new List<Vertex>();
            foreach (var lav in sLav)
            {
                if (lav == skippedLav)
                    continue;

                var vertexInLav = GetEdgeInLav(lav, oppositeEdge);
                if (vertexInLav != null)
                    edgeLavs.Add(vertexInLav);
            }
            return edgeLavs;
        }

        private static Vertex FindOppositeEdgeLav(HashSet<CircularList<Vertex>> sLav,
            Edge oppositeEdge, Vector2d center)
        {
            var edgeLavs = FindEdgeLavs(sLav, oppositeEdge, null);
            return ChooseOppositeEdgeLav(edgeLavs, oppositeEdge, center);
        }

        /// <summary>
        ///     Take next lav vertex _AFTER_ given edge, find vertex is always on RIGHT
        ///     site of edge.
        /// </summary>
        private static Vertex GetEdgeInLav(CircularList<Vertex> lav, Edge oppositeEdge)
        {
            foreach (var node in lav.Iterate())
                if (oppositeEdge == node.PreviousEdge ||
                    oppositeEdge == node.Previous.Next)
                    return node;

            return null;
        }

        private static List<SkeletonEvent> GroupLevelEvents(List<SkeletonEvent> levelEvents)
        {
            var ret = new List<SkeletonEvent>();

            var parentGroup = new HashSet<Vertex>();

            while (levelEvents.Count > 0)
            {
                parentGroup.Clear();

                var @event = levelEvents[0];
                levelEvents.RemoveAt(0);
                var @eventCenter = @event.V;
                var distance = @event.Distance;

                if (@event is BoundaryEvent boundaryEvent)
                {
                    // TODO: Add smarter handling for boundary events
                    ret.Add(@event);
                    continue;
                }

                AddEventToGroup(parentGroup, @event);

                var cluster = new List<SkeletonEvent> { @event };

                for (var j = 0; j < levelEvents.Count; j++)
                {
                    var test = levelEvents[j];

                    if (IsEventInGroup(parentGroup, test) || eventCenter.Distance(test.V) < SplitEpsilon)
                    {
                        // Because of numerical errors split event and edge event
                        // can appear in slight different point. Epsilon can be
                        // apply to level but event point can move rapidly even for
                        // little changes in level. If two events for the same level
                        // share the same parent, they should be merge together.

                        var item = levelEvents[j];
                        levelEvents.RemoveAt(j);
                        cluster.Add(item);
                        AddEventToGroup(parentGroup, test);
                        j--;
                    }
                }

                // More then one event share the same result point, we need to
                // create new level event.
                ret.Add(CreateLevelEvent(eventCenter, distance, cluster));
            }
            return ret;
        }

        private void InitEvents(HashSet<CircularList<Vertex>> sLav,
            PriorityQueue<SkeletonEvent> queue, List<Edge> edges)
        {
            foreach (var lav in sLav)
            {
                foreach (var vertex in lav.Iterate())
                    queue.AddRange(ComputeSplitEvents(vertex, edges, -1));
            }

            foreach (var lav in sLav)
            {
                foreach (var vertex in lav.Iterate())
                {
                    queue.AddRange(ComputeBoundaryEvents(vertex));
                }
            }

            foreach (var lav in sLav)
            {
                foreach (var vertex in lav.Iterate())
                {
                    var nextVertex = vertex.Next as Vertex;
                    ComputeEdgeEvents(vertex, nextVertex, queue);
                }
            }
        }

        private void InitSlav(Polygon2d polygon, HashSet<CircularList<Vertex>> sLav,
            List<Edge> edges, List<FaceQueue> faces)
        {
            var edgesList = new CircularList<Edge>();

            var size = polygon.VertexCount;
            for (var i = 0; i < size; i++)
            {
                var j = (i + 1) % size;
                edgesList.AddLast(new Edge(polygon[i], polygon[j]));
            }

            foreach (var edge in edgesList.Iterate())
            {
                var nextEdge = edge.Next as Edge;
                var bisector = CalcInitialBisector(edge.End, edge, nextEdge);

                edge.BisectorNext = bisector;
                nextEdge.BisectorPrevious = bisector;
                edges.Add(edge);
            }

            var lav = new CircularList<Vertex>();
            sLav.Add(lav);

            foreach (var edge in edgesList.Iterate())
            {
                var nextEdge = edge.Next as Edge;
                var vertex = new Vertex(edge.End, 0, edge.BisectorNext, edge, nextEdge);
                lav.AddLast(vertex);
            }

            foreach (var vertex in lav.Iterate())
            {
                var next = vertex.Next as Vertex;
                // create face on right side of vertex
                var rightFace = new FaceNode(vertex);

                var faceQueue = new FaceQueue();
                faceQueue.Edge = (vertex.NextEdge);

                faceQueue.AddFirst(rightFace);
                faces.Add(faceQueue);
                vertex.RightFace = rightFace;

                // create face on left side of next vertex
                var leftFace = new FaceNode(next);
                rightFace.AddPush(leftFace);
                next.LeftFace = leftFace;
            }
        }

        protected abstract Line2d CalcInitialBisector(Vector2d point, Edge edgeBefore, Edge edgeAfter);

        private static bool IsEventInGroup(HashSet<Vertex> parentGroup, SkeletonEvent @event)
        {
            return @event switch
            {
                SplitEvent splitEvent => parentGroup.Contains(splitEvent.Parent),
                BoundaryEvent boundaryEvent => parentGroup.Contains(boundaryEvent.Parent),
                EdgeEvent edgeEvent => parentGroup.Contains(edgeEvent.PreviousVertex) || parentGroup.Contains(edgeEvent.NextVertex),
                _ => false,
            };
        }

        private static bool IsInEdgeChain(SplitEvent split, EdgeChain chain)
        {
            var splitParent = split.Parent;
            var edgeList = chain.EdgeList;
            return edgeList.Any(edgeEvent => edgeEvent.PreviousVertex == splitParent ||
                edgeEvent.NextVertex == splitParent);
        }

        private static List<SkeletonEvent> LoadAndGroupLevelEvents(PriorityQueue<SkeletonEvent> queue)
        {
            var levelEvents = LoadLevelEvents(queue);
            return GroupLevelEvents(levelEvents);
        }

        /// <summary> Loads all not obsolete event which are on one level. As level height is taken epsilon. </summary>
        private static List<SkeletonEvent> LoadLevelEvents(PriorityQueue<SkeletonEvent> queue)
        {
            var level = new List<SkeletonEvent>();
            SkeletonEvent levelStart;
            // skip all obsolete events in level
            do
            {
                levelStart = queue.Empty ? null : queue.Next();
            }
            while (levelStart != null && levelStart.IsObsolete);

            // all events obsolete
            if (levelStart == null || levelStart.IsObsolete)
                return level;

            var levelStartHeight = levelStart.Distance;

            level.Add(levelStart);

            SkeletonEvent skeletonEevent;
            while ((skeletonEevent = queue.Peek()) != null &&
                Math.Abs(skeletonEevent.Distance - levelStartHeight) < SplitEpsilon)
            {
                var nextLevelEvent = queue.Next();
                if (!nextLevelEvent.IsObsolete)
                    level.Add(nextLevelEvent);
            }
            return level;
        }

        private void MultiEdgeEvent(MultiEdgeEvent @event,
            PriorityQueue<SkeletonEvent> queue, List<Edge> edges, List<Segment2d> segments)
        {
            var center = @event.V;
            var edgeList = @event.Chain.EdgeList;

            var previousVertex = @event.Chain.PreviousVertex;
            previousVertex.IsProcessed = true;

            var nextVertex = @event.Chain.NextVertex;
            nextVertex.IsProcessed = true;

            var bisector = CalcInitialBisector(center, previousVertex.PreviousEdge, nextVertex.NextEdge);
            var edgeVertex = new Vertex(center, @event.Distance, bisector, previousVertex.PreviousEdge,
                nextVertex.NextEdge);

            var consumedVertices = new List<Vertex> { previousVertex };
            consumedVertices.AddRange(@event.Chain.EdgeList.Select(edge => edge.NextVertex));

            foreach (var v in consumedVertices)
            {
                segments.Add(new Segment2d(v.Point, center));
            }

            // left face
            AddFaceLeft(edgeVertex, previousVertex);

            // right face
            AddFaceRight(edgeVertex, nextVertex);

            previousVertex.AddPrevious(edgeVertex);

            // back faces
            AddMultiBackFaces(edgeList, edgeVertex);

            ComputeEvents(edgeVertex, queue, edges);
        }

        private void MultiSplitEvent(MultiSplitEvent @event, HashSet<CircularList<Vertex>> sLav,
            PriorityQueue<SkeletonEvent> queue, List<Edge> edges, List<Segment2d> segments)
        {
            var chains = @event.Chains;
            var center = @event.V;

            segments.Add(new Segment2d(@event.Chains[0].CurrentVertex.Point, center));

            CreateOppositeEdgeChains(sLav, chains, center);

            chains.Sort(new ChainComparer(center));

            // face node for split@event is shared between two chains
            FaceNode lastFaceNode = null;

            // connect all edges into new bisectors and lavs
            var edgeListSize = chains.Count;
            for (var i = 0; i < edgeListSize; i++)
            {
                var chainBegin = chains[i];
                var chainEnd = chains[(i + 1) % edgeListSize];

                var newVertex = CreateMultiSplitVertex(chainBegin.NextEdge, chainEnd.PreviousEdge,
                    center, @event.Distance);

                var beginNextVertex = chainBegin.NextVertex;
                var endPreviousVertex = chainEnd.PreviousVertex;

                CorrectBisectorDirection(newVertex.Bisector, beginNextVertex, endPreviousVertex,
                    chainBegin.NextEdge,
                    chainEnd.PreviousEdge);

                if (LavUtil.IsSameLav(beginNextVertex, endPreviousVertex))
                {
                    // if vertex are in same lav we need to cut part of lav in the
                    //  middle of vertex and create new lav from that points
                    var lavPart = LavUtil.CutLavPart(beginNextVertex, endPreviousVertex);

                    var lav = new CircularList<Vertex>();
                    sLav.Add(lav);
                    lav.AddLast(newVertex);
                    foreach (var vertex in lavPart)
                        lav.AddLast(vertex);
                }
                else
                {
                    //if vertex are in different lavs we need to merge them into one.
                    LavUtil.MergeBeforeBaseVertex(beginNextVertex, endPreviousVertex);
                    endPreviousVertex.AddNext(newVertex);
                }

                ComputeEvents(newVertex, queue, edges);
                lastFaceNode = AddSplitFaces(lastFaceNode, chainBegin, chainEnd, newVertex);
            }

            // remove all centers of@events from lav
            edgeListSize = chains.Count;
            for (var i = 0; i < edgeListSize; i++)
            {
                var chainBegin = chains[i];
                var chainEnd = chains[(i + 1) % edgeListSize];

                LavUtil.RemoveFromLav(chainBegin.CurrentVertex);
                LavUtil.RemoveFromLav(chainEnd.CurrentVertex);

                if (chainBegin.CurrentVertex != null)
                    chainBegin.CurrentVertex.IsProcessed = true;
                if (chainEnd.CurrentVertex != null)
                    chainEnd.CurrentVertex.IsProcessed = true;
            }
        }

        private static void PickEvent(PickEvent @event)
        {
            var center = @event.V;
            var edgeList = @event.Chain.EdgeList;

            // lav will be removed so it is final vertex.
            AddMultiBackFaces(edgeList, new Vertex(center, @event.Distance,
                new Line2d(Vector2d.MinValue, Vector2d.MinValue), null, null)
            { IsProcessed = true });
        }

        private static void ProcessTwoNodeLavs(HashSet<CircularList<Vertex>> sLav)
        {
            foreach (var lav in sLav)
            {
                if (lav.Size == 2)
                {
                    var first = lav.First();
                    var last = first.Next as Vertex;

                    FaceQueueUtil.ConnectQueues(first.LeftFace, last.RightFace);
                    FaceQueueUtil.ConnectQueues(first.RightFace, last.LeftFace);

                    first.IsProcessed = true;
                    last.IsProcessed = true;

                    LavUtil.RemoveFromLav(first);
                    LavUtil.RemoveFromLav(last);
                }
            }
        }

        private static void RemoveEmptyLav(HashSet<CircularList<Vertex>> sLav)
        {
            sLav.RemoveWhere(circularList => circularList.Size == 0);
        }

        private static void RemoveEventsUnderHeight(PriorityQueue<SkeletonEvent> queue,
            double levelHeight)
        {
            while (!queue.Empty)
            {
                if (queue.Peek().Distance > levelHeight + SplitEpsilon)
                    break;
                queue.Next();
            }
        }

        private static void ValidateGeneralPolygon(GeneralPolygon2d gpolygon)
        {
            ValidatePolygon(gpolygon.Outer);
            foreach (var hole in gpolygon.Holes)
            {
                ValidatePolygon(hole);
            }
        }

        private static void ValidatePolygon(Polygon2d polygon)
        {
            if (polygon == null)
                throw new ArgumentException("polygon can't be null");

            if (polygon[0].Equals(polygon[polygon.VertexCount - 1]))
                throw new ArgumentException("polygon can't start and end with the same point");
        }

        #region Nested classes

        private sealed class ChainComparer : IComparer<IChain>
        {
            public ChainComparer(Vector2d center)
            {
                _center = center;
            }

            public int Compare(IChain x, IChain y)
            {
                if (x == y)
                    return 0;

                var angle1 = Angle(_center, x.PreviousEdge.Begin);
                var angle2 = Angle(_center, y.PreviousEdge.Begin);

                return angle1 > angle2 ? 1 : -1;
            }

            private readonly Vector2d _center;

            private static double Angle(Vector2d p0, Vector2d p1)
            {
                var dx = p1.x - p0.x;
                var dy = p1.y - p0.y;
                return Math.Atan2(dy, dx);
            }
        }

        private sealed class SkeletonEventDistanceComparer : IComparer<SkeletonEvent>
        {
            public int Compare(SkeletonEvent left, SkeletonEvent right)
            {
                return left.Distance.CompareTo(right.Distance);
            }
        };

        private sealed class SplitCandidate
        {
            public readonly double Distance;
            public readonly Edge OppositeEdge;
            public readonly Vector2d OppositePoint;
            public readonly Vector2d Point;

            public SplitCandidate(Vector2d point, double distance, Edge oppositeEdge, Vector2d oppositePoint)
            {
                Point = point;
                Distance = distance;
                OppositeEdge = oppositeEdge;
                OppositePoint = oppositePoint;
            }
        }

        private sealed class SplitCandidateComparer : IComparer<SplitCandidate>
        {
            public int Compare(SplitCandidate left, SplitCandidate right)
            {
                return left.Distance.CompareTo(right.Distance);
            }
        }

        #endregion Nested classes
    }
}
