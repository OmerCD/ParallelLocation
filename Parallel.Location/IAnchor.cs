﻿﻿namespace Parallel.Location
{
    public interface IAnchor : ICoordinate, IIntId
    {
        double MaxReadDistance { get; set; }
    }
}