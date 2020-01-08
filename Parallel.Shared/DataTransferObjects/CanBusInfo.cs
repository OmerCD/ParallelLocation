using ReflectorO.Attributes;

namespace Parallel.Shared.DataTransferObjects
{
    public class CanBusInfo
    {
        public byte MessageType { get; set; }
        public byte Sequence { get; set; }
        public byte NumIOData { get; set; }
        public uint SystemTime { get; set; }
        [Array(88)]
        public byte[] IOBuffer { get; set; } = new byte[88];
    }
}