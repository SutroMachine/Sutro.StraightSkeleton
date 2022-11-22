using System;
using Sutro.StraightSkeleton.Path;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Circular
{
    internal class Vertex : CircularNode
    {
        const int RoundDigitCount = 5;

        public Vector2d Point;
        public readonly double Distance;
        public readonly LineParametric2d Bisector;

        public readonly Edge NextEdge;
        public readonly Edge PreviousEdge;
        
        public FaceNode LeftFace;
        public FaceNode RightFace;

        public bool IsProcessed;

        public Vertex(Vector2d point, double distance, LineParametric2d bisector, 
            Edge previousEdge, Edge nextEdge)
        {
            Point = point;
            Distance = Math.Round(distance, RoundDigitCount);
            Bisector = bisector;
            PreviousEdge = previousEdge;
            NextEdge = nextEdge;

            IsProcessed = false;
        }

        public override string ToString()
        {
            return "Vertex [v=" + Point + ", IsProcessed=" + IsProcessed + 
                ", Bisector=" + Bisector + ", PreviousEdge=" + PreviousEdge + 
                ", NextEdge=" + NextEdge;
        }
    }
}