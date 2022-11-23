using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Events.Chains
{
    internal interface IChain
    {
        ChainType ChainType { get; }
        Vertex CurrentVertex { get; }
        Edge NextEdge { get; }
        Vertex NextVertex { get; }
        Edge PreviousEdge { get; }
        Vertex PreviousVertex { get; }
    }
}
