using Sutro.StraightSkeleton.Circular;

namespace Sutro.StraightSkeleton.Path
{
    internal class FaceNode : PathQueueNode<FaceNode>
    {
        public readonly Vertex Vertex;

        public FaceQueue FaceQueue
        {
            get { return (FaceQueue)List; }
        }

        public bool IsQueueUnconnected
        {
            get { return FaceQueue.IsUnconnected; }
        }

        public FaceNode(Vertex vertex)
        {
            Vertex = vertex;
        }

        public void QueueClose()
        {
            FaceQueue.Close();
        }
    }
}
