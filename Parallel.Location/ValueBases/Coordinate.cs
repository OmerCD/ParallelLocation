namespace Parallel.Location
{
    public class Coordinate : ICoordinate
    {
        public Coordinate(double x, double z, double y)
        {
            X = x;
            Z = z;
            Y = y;
        }

        public double X { get; }
        public double Z { get; }
        public double Y { get; }
    }
}