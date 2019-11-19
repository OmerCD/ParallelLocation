namespace Parallel.Location
{
    public interface IDistance
    {
        int FromAnchorId { get; }
        double Distance { get; }
    }
}