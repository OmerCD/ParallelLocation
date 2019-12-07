﻿namespace ReflectorO
{
    public sealed class BigEndianBitConverter : EndianBitConverter
    {
        public override sealed bool IsLittleEndian()
        {
            return false;
        }

        public override sealed Endianness Endianness => Endianness.BigEndian;

        protected override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
        {
            int num = index + bytes - 1;
            for (int index1 = 0; index1 < bytes; ++index1)
            {
                buffer[num - index1] = (byte) ((ulong) value & (ulong) byte.MaxValue);
                value >>= 8;
            }
        }

        protected override long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
        {
            long num = 0;
            for (int index = 0; index < bytesToConvert; ++index)
                num = num << 8 | (long) buffer[startIndex + index];
            return num;
        }
    }
}