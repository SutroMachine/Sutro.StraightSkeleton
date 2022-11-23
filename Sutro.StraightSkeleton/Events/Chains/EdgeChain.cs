using System.Collections.Generic;
using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events.Chains
{
    internal class EdgeChain : IChain
    {
        public ChainType ChainType
        {
            get { return _closed ? ChainType.ClosedEdge : ChainType.Edge; }
        }

        public Vertex CurrentVertex
        {
            get { return null; }
        }

        public List<EdgeEvent> EdgeList { get; private set; }

        public Edge NextEdge
        {
            get { return EdgeList[EdgeList.Count - 1].NextVertex.NextEdge; }
        }

        public Vertex NextVertex
        {
            get { return EdgeList[EdgeList.Count - 1].NextVertex; }
        }

        public Edge PreviousEdge
        {
            get { return EdgeList[0].PreviousVertex.PreviousEdge; }
        }

        public Vertex PreviousVertex
        {
            get { return EdgeList[0].PreviousVertex; }
        }

        public EdgeChain(List<EdgeEvent> edgeList)
        {
            EdgeList = edgeList;
            _closed = PreviousVertex == NextVertex;
        }

        private readonly bool _closed;
    }
}
