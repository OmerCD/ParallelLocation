﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ReflectorO
{
    public enum Endianness
    {
        LittleEndian,
        BigEndian,
    }
    public abstract class EndianBitConverter
    {
        private static readonly LittleEndianBitConverter Little = new LittleEndianBitConverter();
        private static readonly BigEndianBitConverter Big = new BigEndianBitConverter();

        public abstract bool IsLittleEndian();

        public abstract Endianness Endianness { get; }

        public static LittleEndianBitConverter LittleEndianBitConverter => EndianBitConverter.Little;

        public static BigEndianBitConverter BigEndianBitConverter => EndianBitConverter.Big;

        public long DoubleToInt64Bits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        public int SingleToInt32Bits(float value)
        {
            return new EndianBitConverter.Int32SingleUnion(value).AsInt32;
        }

        public float Int32BitsToSingle(int value)
        {
            return new EndianBitConverter.Int32SingleUnion(value).AsSingle;
        }

        public bool ToBoolean(byte[] value, int startIndex)
        {
            EndianBitConverter.CheckByteArgument(value, startIndex, 1);
            return BitConverter.ToBoolean(value, startIndex);
        }

        public char ToChar(byte[] value, int startIndex)
        {
            return (char)this.CheckedFromBytes(value, startIndex, 2);
        }

        public double ToDouble(byte[] value, int startIndex)
        {
            return this.Int64BitsToDouble(this.ToInt64(value, startIndex));
        }

        public float ToSingle(byte[] value, int startIndex)
        {
            return this.Int32BitsToSingle(this.ToInt32(value, startIndex));
        }

        public short ToInt16(byte[] value, int startIndex)
        {
            return (short)this.CheckedFromBytes(value, startIndex, 2);
        }

        public int ToInt32(byte[] value, int startIndex)
        {
            return (int)this.CheckedFromBytes(value, startIndex, 4);
        }

        public long ToInt64(byte[] value, int startIndex)
        {
            return this.CheckedFromBytes(value, startIndex, 8);
        }

        public ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)this.CheckedFromBytes(value, startIndex, 2);
        }

        public uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)this.CheckedFromBytes(value, startIndex, 4);
        }

        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong)this.CheckedFromBytes(value, startIndex, 8);
        }

        private static void CheckByteArgument(byte[] value, int startIndex, int bytesRequired)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0 || startIndex > value.Length - bytesRequired)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        private long CheckedFromBytes(byte[] value, int startIndex, int bytesToConvert)
        {
            EndianBitConverter.CheckByteArgument(value, startIndex, bytesToConvert);
            return this.FromBytes(value, startIndex, bytesToConvert);
        }

        protected abstract long FromBytes(byte[] value, int startIndex, int bytesToConvert);

        public static string ToString(byte[] value)
        {
            return BitConverter.ToString(value);
        }

        public static string ToString(byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex);
        }

        public static string ToString(byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length);
        }

        public Decimal ToDecimal(byte[] value, int startIndex)
        {
            int[] bits = new int[4];
            for (int index = 0; index < 4; ++index)
                bits[index] = this.ToInt32(value, startIndex + index * 4);
            return new Decimal(bits);
        }

        public byte[] GetBytes(Decimal value)
        {
            byte[] buffer = new byte[16];
            int[] bits = Decimal.GetBits(value);
            for (int index = 0; index < 4; ++index)
                this.CopyBytesImpl((long)bits[index], 4, buffer, index * 4);
            return buffer;
        }

        public void CopyBytes(Decimal value, byte[] buffer, int index)
        {
            int[] bits = Decimal.GetBits(value);
            for (int index1 = 0; index1 < 4; ++index1)
                this.CopyBytesImpl((long)bits[index1], 4, buffer, index1 * 4 + index);
        }

        private byte[] GetBytes(long value, int bytes)
        {
            byte[] buffer = new byte[bytes];
            this.CopyBytes(value, bytes, buffer, 0);
            return buffer;
        }

        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytes(char value)
        {
            return this.GetBytes((long)value, 2);
        }

        public byte[] GetBytes(double value)
        {
            return this.GetBytes(this.DoubleToInt64Bits(value), 8);
        }

        public byte[] GetBytes(short value)
        {
            return this.GetBytes((long)value, 2);
        }

        public byte[] GetBytes(int value)
        {
            return this.GetBytes((long)value, 4);
        }

        public byte[] GetBytes(long value)
        {
            return this.GetBytes(value, 8);
        }

        public byte[] GetBytes(float value)
        {
            return this.GetBytes((long)this.SingleToInt32Bits(value), 4);
        }

        public byte[] GetBytes(ushort value)
        {
            return this.GetBytes((long)value, 2);
        }

        public byte[] GetBytes(uint value)
        {
            return this.GetBytes((long)value, 4);
        }

        public byte[] GetBytes(ulong value)
        {
            return this.GetBytes((long)value, 8);
        }

        private void CopyBytes(long value, int bytes, byte[] buffer, int index)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Byte array must not be null");
            if (buffer.Length < index + bytes)
                throw new ArgumentOutOfRangeException("Buffer not big enough for value");
            this.CopyBytesImpl(value, bytes, buffer, index);
        }

        protected abstract void CopyBytesImpl(long value, int bytes, byte[] buffer, int index);

        public void CopyBytes(bool value, byte[] buffer, int index)
        {
            this.CopyBytes(value ? 1L : 0L, 1, buffer, index);
        }

        public void CopyBytes(char value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 2, buffer, index);
        }

        public void CopyBytes(double value, byte[] buffer, int index)
        {
            this.CopyBytes(this.DoubleToInt64Bits(value), 8, buffer, index);
        }

        public void CopyBytes(short value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 2, buffer, index);
        }

        public void CopyBytes(int value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 4, buffer, index);
        }

        public void CopyBytes(long value, byte[] buffer, int index)
        {
            this.CopyBytes(value, 8, buffer, index);
        }

        public void CopyBytes(float value, byte[] buffer, int index)
        {
            this.CopyBytes((long)this.SingleToInt32Bits(value), 4, buffer, index);
        }

        public void CopyBytes(ushort value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 2, buffer, index);
        }

        public void CopyBytes(uint value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 4, buffer, index);
        }

        public void CopyBytes(ulong value, byte[] buffer, int index)
        {
            this.CopyBytes((long)value, 8, buffer, index);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Int32SingleUnion
        {
            [FieldOffset(0)]
            private readonly int _i;
            [FieldOffset(0)]
            private readonly float _f;

            internal Int32SingleUnion(int i)
            {
                this._f = 0.0f;
                this._i = i;
            }

            internal Int32SingleUnion(float f)
            {
                this._i = 0;
                this._f = f;
            }

            internal int AsInt32 => this._i;

            internal float AsSingle => this._f;
        }
    }


}
