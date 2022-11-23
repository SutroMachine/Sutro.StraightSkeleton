using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events.Chains
{
    internal class SplitChain : IChain
    {
        public ChainType ChainType
        {
            get { return ChainType.Split; }
        }

        public Vertex CurrentVertex
        {
            get { return _splitEvent.Parent; }
        }

        public Edge NextEdge
        {
            get { return _splitEvent.Parent.NextEdge; }
        }

        public Vertex NextVertex
        {
            get { return _splitEvent.Parent.Next as Vertex; }
        }

        public Edge OppositeEdge
        {
            get
            {
                if (!(_splitEvent is VertexSplitEvent))
                    return _splitEvent.OppositeEdge;

                return null;
            }
        }

        public Edge PreviousEdge
        {
            get { return _splitEvent.Parent.PreviousEdge; }
        }

        public Vertex PreviousVertex
        {
            get { return _splitEvent.Parent.Previous as Vertex; }
        }

        public SplitChain(SplitEvent @event)
        {
            _splitEvent = @event;
        }

        private readonly SplitEvent _splitEvent;
    }
}
