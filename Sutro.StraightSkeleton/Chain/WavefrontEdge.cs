using g3;
using Sutro.StraightSkeleton.Primitives;

namespace Sutro.StraightSkeleton.Chain
{
    public class WavefrontEdge
    {
        public readonly Vector2d Norm;

        public WavefrontEdge()
        {
        }

        public WavefrontEdge(Vector2d begin, Vector2d end)
        {
            LineLinear2d = new LineLinear2d(begin, end);
            Norm = (end - begin).Normalized;
        }

        public override string ToString()
        {
            return $"Edge [Norm={Norm}]";
        }

        public LineLinear2d LineLinear2d { get; }
    }
}
