using System;
using g3;
using Sutro.StraightSkeleton.Path;

namespace Sutro.StraightSkeleton.Chain
{
    internal class WavefrontVertex
    {
        public Vector2d Point { get; set; }
        public Line2d Bisector { get; set; }
        public double Distance { get; }
        public bool IsProcessed { get; set; }

        public FaceNode LeftFace { get; set; }
        public FaceNode RightFace { get; set; }

        public WavefrontVertex(Vector2d point, double distance)
        {
            Point = point;
            Distance = Math.Round(distance, RoundDigitCount);
            IsProcessed = false;
        }

        public override string ToString()
        {
            return $"Vertex [v= {Point}, IsProcessed={IsProcessed}, Bisector={Bisector}";
        }

        private const int RoundDigitCount = 5;
    }
}
