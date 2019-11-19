using System.Collections.Generic;

namespace Parallel.Location
{
    public interface ILocationCalculator
    {
        int MinAnchorCount { get; }
        void SetAnchors(IEnumerable<IAnchor> values);
        void SetAnchors(IAnchor[] values);
        ICoordinate GetResult(params IDistance[] distances);
    }
}