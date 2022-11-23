using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events.Chains
{
    internal class SingleEdgeChain : IChain
    {
        public ChainType ChainType
        {
            get { return ChainType.Split; }
        }

        public Vertex CurrentVertex
        {
            get { return null; }
        }

        public Edge NextEdge
        {
            get { return _oppositeEdge; }
        }

        public Vertex NextVertex
        {
            get { return _nextVertex; }
        }

        public Edge PreviousEdge
        {
            get { return _oppositeEdge; }
        }

        public Vertex PreviousVertex
        {
            get { return _previousVertex; }
        }

        public SingleEdgeChain(Edge oppositeEdge, Vertex nextVertex)
        {
            _oppositeEdge = oppositeEdge;
            _nextVertex = nextVertex;

            // previous vertex for opposite edge event is valid only before
            // processing of multi split event start. We need to store vertex before
            // processing starts.
            _previousVertex = nextVertex.Previous as Vertex;
        }

        private readonly Vertex _nextVertex;
        private readonly Edge _oppositeEdge;
        private readonly Vertex _previousVertex;
    }
}
