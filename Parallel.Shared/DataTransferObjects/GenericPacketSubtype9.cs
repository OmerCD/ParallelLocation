using Parallel.Shared.DataTransferObjects.Attributes;
using ReflectorO.Attributes;
using ReflectorO.CustomParse;

namespace Parallel.Shared.DataTransferObjects
{
    public class GenericPacketSubtype9 //tekfen için mobil
    {
        public byte MessageType { get; set; }
        public byte MessageSubType { get; set; }
        public byte Reserved { get; set; }
        public byte DataSize { get; set; }
        //  public byte Data { get; set; }
        public uint UtcTime { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public uint Altitude { get; set; }
        public ushort MobilId { get; set; }
        public ushort ReaderId { get; set; }
        public byte Power { get; set; }
        public ushort ACCZ { get; set; }
        public uint DataCountNo { get; set; }
        public override string ToString()
        {
            return "MessageSubType 9";
        }
        // private class PowerParser : ICustomParser
        // {
        //     public object Parse(ParseInfo parseInfo)
        //     {
        //         ((GenericPacketSubtype9) parseInfo.ParsedObject).Alarm = parseInfo.Data[0] >> 4;
        //         return (byte)(parseInfo.Data[0] & 0b00001111);
        //     }
        // }
    }
}