using SocketCommunication.Interfaces;

namespace SocketCommunication.Models
{
    public class MobileData:IMobileData
    {
        public ushort MobileNodeId { get; set; }
        public byte Power { get; set; }
        public uint ReaderNodeId { get; set; }
        public ushort Distance { get; set; }
    }
}
