namespace Parallel.Shared.DataTransferObjects
{
    public class SRfGenericPacket
    {
        public byte MessageType { get; set; }
        public byte MessageSubType { get; set; }
        public byte Reserved { get; set; }
        public byte DataSize { get; set; }
        public SMobileConfig Data { get; set; }
        public ushort Source { get; set; }
    }
}