using System.Collections.Generic;
using System.Linq;

namespace Parallel.Shared.DataTransferObjects.Client
{
    public class LocationInfo
    {
        public LocationInfo(int tagId, double x, double z, double y, IEnumerable<AnchorInfo> anchorInfos)
        {
            TagId = tagId;
            X = x;
            Z = z;
            Y = y;
            AnchorInfos = anchorInfos as AnchorInfo[] ?? anchorInfos.ToArray();
        }

        public int TagId { get; set; }
        public double X { get; set; }
        public double Z { get; set; }
        public double Y { get; set; }
        public AnchorInfo[] AnchorInfos { get; set; }
    }

    public class AnchorInfo
    {
        public double X { get; set; }
        public double Z { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public string Name { get; set; }
    }
}