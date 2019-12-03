﻿namespace ReflectorO
{
    public sealed class LittleEndianBitConverter : EndianBitConverter
    {
        public override sealed bool IsLittleEndian()
        {
            return true;
        }

        public override sealed Endianness Endianness => Endianness.LittleEndian;

        protected override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
        {
            for (int index1 = 0; index1 < bytes; ++index1)
            {
                buffer[index1 + index] = (byte) ((ulong) value & (ulong) byte.MaxValue);
                value >>= 8;
            }
        }

        protected override long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
        {
            long num = 0;
            for (int index = 0; index < bytesToConvert; ++index)
                num = num << 8 | (long) buffer[startIndex + bytesToConvert - 1 - index];
            return num;
        }
    }
}