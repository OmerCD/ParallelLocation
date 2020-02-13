using System;
using System.Collections.Generic;
using ReflectorO;

namespace QueueManagement
{
    public class MessageType4AlarmDetector : IAlarmPackageDetector
    {
        private readonly HashSet<ushort> _acczList = new HashSet<ushort>
            {11109, 11110, 11111, 11112, 11113, 11114, 11115, 11116, 11117, 11118, 11119, 11120, 11124};

        public bool IsAlarmPackage(byte[] package)
        {
            // 7-8: accz ushort
            // 10 : power byte
            var accz = EndianBitConverter.LittleEndianBitConverter.ToUInt16(package, 7);
            var power = Convert.ToByte(package[10] >> 4);

            return power != 8 && _acczList.Contains(accz);
        }

        public bool IsPackageSatisfies(byte[] package)
        {
            return package[0] == 4;
        }
    }
}