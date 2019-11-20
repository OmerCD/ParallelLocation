using System;
using System.Linq;
using SocketCommunication.Enums;
using SocketCommunication.Interfaces;
using SocketCommunication.Models;

namespace SocketCommunication.BusinessLogic
{
    public class Parser : IParser
    {

        private static readonly int packet_header_pre_online = 252;
        private static readonly int packet_header_post_online = 253;
        private static int packet_header_pre = 250;
        private static int packet_header_post = 251;

        public bool Parse(byte[] receivedData, int receivedDataLength, out IParsedData parsedData)
        {
            var index = 0;
            //packet baslangici 250 ise 1 252 ise 2 degeri alir.
            var p = 0;

            var buffer = new byte[8200];
            var bufferIndex = 0;

            //bitis paketinin indeksi
            var t = 0;

            for (var i = 0; i < receivedDataLength; i++)
            {
                if (receivedData.Take(5).Any(s => s == packet_header_pre)) { i = 5; p = 1; }

                else if (receivedData.Skip(5).Take(5).Any(s => s == packet_header_pre_online)) { i = 10; p = 2; }

                if (p == 1)
                {
                    if (receivedData[i] == packet_header_post)
                    {
                        t++;
                        if (t == 5)
                        {

                        }
                        else
                        {
                            buffer[bufferIndex] = receivedData[i];
                            bufferIndex++;
                        }
                    }
                    t = 0;
                    buffer[bufferIndex] = receivedData[i];
                    bufferIndex++;
                }

                else
                {

                }

            }







            parsedData = new ParsedData
            {
                MessageType = GetMessageType(receivedData[index]),
                DataCount = receivedData[index + 1]
            };
            index = 9;
            for (var i = 0; i < parsedData.DataCount; i++)
            {
                parsedData.MobileData.Add(new MobileData
                {
                    MobileNodeId = SwapByteOrder(BitConverter.ToUInt16(receivedData, index)),
                    Distance = SwapByteOrder(BitConverter.ToUInt16(receivedData, index + 2)),
                    Power = Convert.ToByte(receivedData[index + 5] & 0x0f),
                    ReaderNodeId = SwapByteOrder(BitConverter.ToUInt16(receivedData, index + 6))
                });
                index += 12;
            }
            return true;
        }
        private MessageType GetMessageType(byte b)
        {
            return Enum.IsDefined(typeof(MessageType), (int)b) ? (MessageType)Enum.Parse(typeof(MessageType), b.ToString()) : MessageType.Undefined;
        }

        private ushort SwapByteOrder(ushort value)
        {
            return (ushort)((0x00FF) & (value >> 8) | (0xFF00) & (value << 8));
        }

    }
}
