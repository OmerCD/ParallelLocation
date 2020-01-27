using System.Collections.Generic;

namespace Parallel.Location
{
    public interface ILocationCalculator
    {
        int MinAnchorCount { get; }
        void SetAnchors(IEnumerable<IAnchor> values);
        void SetAnchors(IAnchor[] values);
        void SetAnchor(int anchorId, IAnchor value);
        IEnumerable<IAnchor> CurrentAnchors { get; }
        ICoordinate GetResult(int id, params IDistance[] distances);
    }
}