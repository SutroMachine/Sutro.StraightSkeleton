using System;
using System.Collections.Generic;

namespace Sutro.StraightSkeleton.Chain
{
    public class Chain<TNode, TEdge>
    {
        private readonly Func<TNode, TNode, TEdge> _edgeFactory;

        public class Edge
        {
            public Edge(TEdge value)
            {
                Value = value;
            }

            public TEdge Value { get; set; }
            public Node PreviousNode { get; set; }
            public Node NextNode { get; set; }

            public Edge NextEdge => NextNode.NextEdge;
            public Edge PreviousEdge => PreviousNode.PreviousEdge;

            public IEnumerable<Edge> EnumerateEdges()
            {
                var current = this;
                while (current.NextEdge != this)
                {
                    yield return current;
                    current = current.NextEdge;
                }
            }
        }

        public class Node
        {
            public Node(TNode value)
            {
                Value = value;
            }

            public Chain<TNode, TEdge> Chain { get; set; }

            public Edge NextEdge { get; set; }
            public Edge PreviousEdge { get; set; }

            public Node NextNode => NextEdge.NextNode;
            public Node PreviousNode => PreviousEdge.PreviousNode;

            public TNode Value { get; set; }

            public IEnumerable<Node> EnumerateNodes()
            {
                var current = this;
                while (current.NextNode != this)
                {
                    yield return current;
                    current = current.NextNode;
                }
            }
        }

        public Node First { get; private set; }
        public Node Last { get; private set; }

        public Chain(IEnumerable<TNode> nodes, Func<TNode, TNode, TEdge> edgeFactory, bool close = false)
        {
            var enumerator = nodes.GetEnumerator();
            First = new Node(enumerator.Current);
            enumerator.MoveNext();
            var currentNode = First;
            foreach (var node in nodes)
            {
                // TODO: Make properties private so only the Chain can set next/previous properties
                Last = new Node(node);
                var newEdge = new Edge(edgeFactory(currentNode.Value, Last.Value))
                {
                    PreviousNode = currentNode,
                    NextNode = Last
                };

                currentNode.NextEdge = newEdge;
                Last.PreviousEdge = newEdge;

                currentNode = Last;
            }

            if (close)
            {
                var finalEdge = new Edge(edgeFactory(currentNode.Value, First.Value))
                {
                    PreviousNode = currentNode,
                    NextNode = First,
                };

                currentNode.NextEdge = finalEdge;
                First.PreviousEdge = finalEdge;

                Last = First;
            }

            _edgeFactory = edgeFactory;
        }

        public bool IsClosed()
        {
            return First.PreviousEdge != null;
        }

        public void AddAfterLast(TNode node)
        {
            if (IsClosed())
                throw new Exception();

            var newNode = new Node(node);
            var newEdge = new Edge(_edgeFactory(Last.Value, newNode.Value))
            {
                PreviousNode = Last,
                NextNode = newNode
            };

            Last.NextEdge = newEdge;
            newNode.PreviousEdge = newEdge;

            Last = newNode;
        }

        public IEnumerable<Node> EnumerateNodes()
        {
            var current = First;
            yield return First;
            while (current.NextNode != null && current.NextNode != First)
            {
                yield return current.NextNode;
                current = current.NextNode;
            }
        }

        public void AddBeforeFirst(TNode node)
        {
            if (IsClosed())
                throw new Exception();

            var newNode = new Node(node);
            var newEdge = new Edge(_edgeFactory(newNode.Value, First.Value))
            {
                PreviousNode = newNode,
                NextNode = First
            };

            First.PreviousEdge = newEdge;
            newNode.NextEdge = newEdge;

            First = newNode;
        }

        public IEnumerable<Edge> EnumerateEdges()
        {
            var current = First.NextEdge;
            yield return First.NextEdge;
            while (current.NextEdge != null && current.NextEdge != First.NextEdge)
            {
                yield return current.NextEdge;
                current = current.NextEdge;
            }
        }
    }
}
