namespace SocketCommunication.Interfaces
{
    public interface IMobileData
    {
        ushort MobileNodeId { get; set; }
        byte Power { get; set; }
        uint ReaderNodeId { get; set; }
        ushort Distance { get; set; }
    }
}
