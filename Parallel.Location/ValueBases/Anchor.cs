namespace Parallel.Location
{
    public class Anchor:IAnchor
    {
        public Anchor(double x, double z, double y, int id)
        {
            X = x;
            Z = z;
            Y = y;
            Id = id;
        }

        public double X { get; }
        public double Z { get; }
        public double Y { get; }
        public int Id { get; set; }
    }
}