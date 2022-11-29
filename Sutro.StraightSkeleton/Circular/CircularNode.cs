namespace Sutro.StraightSkeleton.Circular
{
    public class CircularNode
    {
        public CircularNode Next { get; set; }
        public CircularNode Previous { get; set; }

        public void AddNext(CircularNode node)
        {
            List.AddNext(this, node);
        }

        public void AddPrevious(CircularNode node)
        {
            List.AddPrevious(this, node);
        }

        public void Remove()
        {
            List.Remove(this);
        }

        internal ICircularList List;
    }
}
