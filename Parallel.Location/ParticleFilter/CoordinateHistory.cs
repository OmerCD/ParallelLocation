using System;

namespace Parallel.Location.ParticleFilter
{
    class CoordinateHistory
    {
        public CoordinateHistory(ICoordinate coordinate, DateTime timeStamp)
        {
            Coordinate = coordinate;
            TimeStamp = timeStamp;
        }

        public ICoordinate Coordinate { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}