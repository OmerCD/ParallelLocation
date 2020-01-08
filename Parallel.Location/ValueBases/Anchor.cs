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

        public Anchor()
        {
            
        }
        public double X { get; set; }
        public double Z { get; set; }
        public double Y { get; set; }
        public int Id { get; set; }
    }
}