using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Data;
using System.Net.Sockets;
using System.Linq;
using System.IO;

namespace TeltonikaParser
{
    /// <summary>
    /// TeltonikaFMXXXX
    /// </summary>
    public class ProtocolTeltonikaFm
    {
        private const int CodecFmxxx = 0x08;

        private const int Acc = 1;
        private const int Door = 2;
        private const int Analog = 4;
        private const int Gsm = 5;
        private const int Speed = 6;
        private const int Voltage = 7;
        private const int Gpspower = 8;
        private const int Temperature = 9;
        private const int Odometer = 16;
        private const int Stop = 20;
        private const int Trip = 28;
        private const int Immobilizer = 29;
        private const int Authorized = 30;
        private const int Greedriving = 31;
        private const int Overspeed = 33;


        private static string ParseBytes(byte[] byteBuffer, int index, int size)
        {
            return BitConverter.ToString(byteBuffer, index, size).Replace("-", string.Empty);
        }

        private static string ParseImei(byte[] byteBuffer, int size)
        {
            int index = 0;
            var result = ParseBytes(byteBuffer, index, 2);
            return result;
        }

        private static bool CheckImei(string data)
        {
            Console.WriteLine(data.Length);
            if (data.Length == 15)
                return true;

            return false;
        }

        private static List<Position> ParsePositions(byte[] byteBuffer, int linesNb)
        {
            int index = 0;
            index += 7;
            uint dataSize = byteBuffer[index];

            index++;
            uint codecId = byteBuffer[index];

            if (codecId == CodecFmxxx)
            {
                index++;
                uint numberOfData = byteBuffer[index];

                Console.WriteLine("{0} {1} {2} ", codecId, numberOfData, dataSize);

                List<Position> result = new List<Position>();

                index++;
                for (int i = 0; i < numberOfData; i++)
                {
                    Position position = new Position();

                    var timestamp = long.Parse(ParseBytes(byteBuffer, index, 8), NumberStyles.HexNumber);
                    index += 8;

                    position.Time = DateTime.Now;

                    var preority = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                    index++;

                    position.Lo = int.Parse(ParseBytes(byteBuffer, index, 4), NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    position.La = int.Parse(ParseBytes(byteBuffer, index, 4), NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    var altitude = short.Parse(ParseBytes(byteBuffer, index, 2), NumberStyles.HexNumber);
                    index += 2;

                    var dir = short.Parse(ParseBytes(byteBuffer, index, 2), NumberStyles.HexNumber);

                    if (dir < 90) position.Direction = 1;
                    else if (dir == 90) position.Direction = 2;
                    else if (dir < 180) position.Direction = 3;
                    else if (dir == 180) position.Direction = 4;
                    else if (dir < 270) position.Direction = 5;
                    else if (dir == 270) position.Direction = 6;
                    else if (dir > 270) position.Direction = 7;
                    index += 2;

                    var satellite = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                    index++;

                    position.Status = satellite >= 3 ? "A" : "L";

                    position.Speed = short.Parse(ParseBytes(byteBuffer, index, 2), NumberStyles.HexNumber);
                    index += 2;

                    int ioEvent = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                    index++;
                    int ioCount = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                    index++;
                    //read 1 byte
                    {
                        int cnt = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                            index++;
                            //Add output status
                            switch (id)
                            {
                                case Acc:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    position.Status += value == 1 ? ",ACC off" : ",ACC on";
                                    index++;
                                    break;
                                }
                                case Door:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    position.Status += value == 1 ? ",door close" : ",door open";
                                    index++;
                                    break;
                                }
                                case Gsm:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    position.Status += string.Format(",GSM {0}", value);
                                    index++;
                                    break;
                                }
                                case Stop:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    position.StopFlag = value == 1;
                                    position.IsStop = value == 1;

                                    index++;
                                    break;
                                }
                                case Immobilizer:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    position.Alarm = value == 0
                                        ? "Activate Anti-carjacking success"
                                        : "Emergency release success";
                                    index++;
                                    break;
                                }
                                case Greedriving:
                                {
                                    var value = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                                    switch (value)
                                    {
                                        case 1:
                                        {
                                            position.Alarm = "Acceleration intense !!";
                                            break;
                                        }
                                        case 2:
                                        {
                                            position.Alarm = "Freinage brusque !!";
                                            break;
                                        }
                                        case 3:
                                        {
                                            position.Alarm = "Virage serré !!";
                                            break;
                                        }
                                        default:
                                            break;
                                    }

                                    index++;
                                    break;
                                }
                                default:
                                {
                                    index++;
                                    break;
                                }
                            }
                        }
                    }

                    //read 2 byte
                    {
                        int cnt = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                            index++;


                            switch (id)
                            {
                                case Analog:
                                {
                                    var value = short.Parse(ParseBytes(byteBuffer, index, 2), NumberStyles.HexNumber);
                                    if (value < 12)
                                        position.Alarm += string.Format("Low voltage", value);
                                    index += 2;
                                    break;
                                }
                                case Speed:
                                {
                                    var value = short.Parse(ParseBytes(byteBuffer, index, 2), NumberStyles.HexNumber);
                                    position.Alarm += string.Format("Speed", value);
                                    index += 2;
                                    break;
                                }
                                default:
                                {
                                    index += 2;
                                    break;
                                }
                            }
                        }
                    }

                    //read 4 byte
                    {
                        int cnt = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                            index++;

                            switch (id)
                            {
                                case Temperature:
                                {
                                    var value = int.Parse(ParseBytes(byteBuffer, index, 4), NumberStyles.HexNumber);
                                    position.Alarm += string.Format("Temperature {0}", value);
                                    index += 4;
                                    break;
                                }
                                case Odometer:
                                {
                                    var value = int.Parse(ParseBytes(byteBuffer, index, 4), NumberStyles.HexNumber);
                                    position.Mileage = value;
                                    index += 4;
                                    break;
                                }
                                default:
                                {
                                    index += 4;
                                    break;
                                }
                            }
                        }
                    }

                    //read 8 byte
                    {
                        int cnt = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(ParseBytes(byteBuffer, index, 1), NumberStyles.HexNumber);
                            index++;

                            var io = long.Parse(ParseBytes(byteBuffer, index, 8), NumberStyles.HexNumber);
                            position.Status += string.Format(",{0} {1}", id, io);
                            index += 8;
                        }
                    }

                    result.Add(position);
                    Console.WriteLine(position.ToString());
                }

                return result;
            }

            return null;
        }

        public static byte[] DealingWithHeartBeat(string data)
        {
            byte[] result = {1};
            if (CheckImei(data))
            {
                return result;
            }

            return null;
        }

        public static string ParseHeartBeatData(byte[] byteBuffer, int size)
        {
            var imei = ParseImei(byteBuffer, size);
            if (CheckImei(imei))
            {
                return imei;
            }
            else
            {
                int index = 0;
                index += 7;
                uint dataSize = byteBuffer[index];

                index++;
                uint codecId = byteBuffer[index];

                if (codecId == CodecFmxxx)
                {
                    index++;
                    uint numberOfData = byteBuffer[index];

                    return numberOfData.ToString();
                }
            }

            return string.Empty;
        }

        public static List<Position> ParseData(byte[] byteBuffer, int size)
        {
            List<Position> result = new List<Position>();
            result = ParsePositions(byteBuffer, size);

            return result;
        }

        public static Position GetGprsPos(string oneLine)
        {
            return null;
        }
    }
}