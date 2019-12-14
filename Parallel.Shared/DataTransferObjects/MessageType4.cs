using System;
using Parallel.Shared.DataTransferObjects.Attributes;
using ReflectorO;
using ReflectorO.Attributes;
using ReflectorO.CustomParse;

namespace Parallel.Shared.DataTransferObjects
{
    public class MessageType4
    {
        public byte MessageType { get; set; }
        public byte DataSize { get; set; }
        public ushort Reserved { get; set; }
        public ushort MobilNodeId { get; set; }
        public ushort ACCZ { get; set; } //11111 alarm durumu
        public byte LinkQuality { get; set; }
        [CustomParser(typeof(PowerParser))] public byte Power { get; set; }
        public ushort ReaderNodeId { get; set; }
        public uint DataCountNo { get; set; } //1-6 arasında bir rakam olmalı
        public int ImMobilityTime { get; set; }
        [NotParsed] public int Alarm { get; set; }

        public override string ToString()
        {
            return "Message Type 4";
        }

        private class PowerParser : ICustomParser
        {
            public object Parse(ParseInfo parseInfo)
            {
                var power = (byte) (parseInfo.Data[0] & 0b00001111);
                if (power != 8)
                {
                    ((MessageType4) parseInfo.ParsedObject).Alarm = (parseInfo.Data[0] >> 4);
                }

                return power;
            }
        }
    }
}