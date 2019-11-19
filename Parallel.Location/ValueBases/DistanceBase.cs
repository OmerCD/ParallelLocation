namespace Parallel.Location
{
    public class DistanceBase : IDistance
    {
        public DistanceBase(int fromAnchorId, double distance)
        {
            FromAnchorId = fromAnchorId;
            Distance = distance;
        }

        public int FromAnchorId { get; }
        public double Distance { get; }
    }
}