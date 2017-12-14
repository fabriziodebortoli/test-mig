using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Collections;

namespace Microarea.TaskBuilderNet.Core.HashAlgorithms
{
    public static class CrcHelper
    {
        #region internal

        #region reverseBits

        /*internal static byte ReverseBits(byte b)
        {
            byte newValue = 0;
            for (int i = 7; i >= 0; i--)
            {
                newValue |= (byte)((b & 1) << i);
                b >>= 1;
            }
            return newValue;
        }
        internal static ushort ReverseBits(ushort us)
        {
            ushort newValue = 0;
            for (int i = 15; i >= 0; i--)
            {
                newValue |= (ushort)((us & 1) << i);
                us >>= 1;
            }
            return newValue;
        }
        internal static uint ReverseBits(uint ui)
        {
            uint newValue = 0;
            for (int i = 31; i >= 0; i--)
            {
                newValue |= (ui & 1) << i;
                ui >>= 1;
            }
            return newValue;
        }*/

        internal static ulong ReverseBits(ulong ul, int valueLength)
        {
            ulong newValue = 0;

            for (int i = valueLength - 1; i >= 0; i--)
            {
                newValue |= (ul & 1) << i;
                ul >>= 1;
            }

            return newValue;
        }

        #endregion reverseBits

        #region ToBigEndian

        internal static byte[] ToBigEndianBytes(UInt32 value)
        {
            var result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        internal static byte[] ToBigEndianBytes(UInt16 value)
        {
            var result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        internal static byte[] ToBigEndianBytes(UInt64 value)
        {
            var result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        #endregion ToBigEndian

        #region FromBigEndian

        internal static UInt16 FromBigToUInt16(byte[] buffer, int start)
        {
            return (ushort)(buffer[start] << 8 | buffer[start + 1]);
        }

        internal static UInt32 FromBigToUInt32(byte[] buffer, int start)
        {
            return (uint)(buffer[start] << 24 | buffer[start + 1] << 16 | buffer[start + 2] << 8 | buffer[start + 3]);
        }

        internal static UInt64 FromBigToUInt64(byte[] buffer, int start)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)buffer[i]) << (64 - 8 * (i + 1));
            }

            return result;
        }

        #endregion FromBigEndian

        #endregion internal

        #region public

        /// <summary>
        /// Use this method for convert hash from byte array to UInt16 value.
        /// </summary>
        public static ushort ToUInt16(byte[] hash)
        {
            return FromBigToUInt16(hash, 0);
        }

        /// <summary>
        /// Use this method for convert hash from byte array to UInt32 value.
        /// </summary>
        public static uint ToUInt32(byte[] hash)
        {
            return FromBigToUInt32(hash, 0);
        }

        #endregion public

        public static ulong FromBigEndian(byte[] hashBytes, int hashSize)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)hashBytes[i]) << (64 - 8 * (i + 1));
            }

            return result;
        }

    }

    public class Parameters
    {
        public Parameters(int hashSize, ulong poly, ulong init, bool refIn, bool refOut, ulong xorOut, ulong check)
        {
            Check = check;
            Init = init;
            Poly = poly;
            RefIn = refIn;
            RefOut = refOut;
            XorOut = xorOut;
            HashSize = hashSize;
        }

        /// <summary>
        /// This field is not strictly part of the definition, and, in
        /// the event of an inconsistency between this field and the other
        /// field, the other fields take precedence.This field is a check
        /// value that can be used as a weak validator of implementations of
        /// the algorithm.The field contains the checksum obtained when the
        /// ASCII string "123456789" is fed through the specified algorithm
        /// (i.e. 313233... (hexadecimal)).
        /// </summary>
        public ulong Check { get; private set; }

        /// <summary>
        /// This is hash size.
        /// </summary>
        public int HashSize { get; private set; }

        /// <summary>
        /// This parameter specifies the initial value of the register
        /// when the algorithm starts.This is the value that is to be assigned
        /// to the register in the direct table algorithm. In the table
        /// algorithm, we may think of the register always commencing with the
        /// value zero, and this value being XORed into the register after the
        /// N'th bit iteration. This parameter should be specified as a
        /// hexadecimal number.
        /// </summary>
        public ulong Init { get; private set; }

        /// <summary>
        /// This parameter is the poly. This is a binary value that
        /// should be specified as a hexadecimal number.The top bit of the
        /// poly should be omitted.For example, if the poly is 10110, you
        /// should specify 06. An important aspect of this parameter is that it
        /// represents the unreflected poly; the bottom bit of this parameter
        /// is always the LSB of the divisor during the division regardless of
        /// whether the algorithm being modelled is reflected.
        /// </summary>
        public ulong Poly { get; private set; }

        /// <summary>
        /// This is a boolean parameter. If it is FALSE, input bytes are
        /// processed with bit 7 being treated as the most significant bit
        /// (MSB) and bit 0 being treated as the least significant bit.If this
        /// parameter is FALSE, each byte is reflected before being processed.
        /// </summary>
        public bool RefIn { get; private set; }

        /// <summary>
        /// This is a boolean parameter. If it is set to FALSE, the
        /// final value in the register is fed into the XOROUT stage directly,
        /// otherwise, if this parameter is TRUE, the final register value is
        /// reflected first.
        /// </summary>
        public bool RefOut { get; private set; }

        /// <summary>
        /// This is an W-bit value that should be specified as a
        /// hexadecimal number.It is XORed to the final register value (after
        /// the REFOUT) stage before the value is returned as the official
        /// checksum.
        /// </summary>
        public ulong XorOut { get; private set; }

        /*
         Parameters for CRC-32 alghoritms
         CRC-32         --> new Parameters( 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xCBF43926)     
         CRC-32/BZIP2   --> new Parameters( 32, 0x04C11DB7, 0xFFFFFFFF, false, false, 0xFFFFFFFF, 0xFC891918)   
         CRC-32C        --> new Parameters( 32, 0x1EDC6F41, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xE3069283)     
         CRC-32D        --> new Parameters( 32, 0xA833982B, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0x87315576)     
         CRC-32/JAMCRC  --> new Parameters( 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0x00000000, 0x340BC6D9)     
         CRC-32/MPEG-2  --> new Parameters( 32, 0x04C11DB7, 0xFFFFFFFF, false, false, 0x00000000, 0x0376E6E7)   
         CRC-32/POSIX   --> new Parameters( 32, 0x04C11DB7, 0x00000000, false, false, 0xFFFFFFFF, 0x765E7680)   
         CRC-32Q        --> new Parameters( 32, 0x814141AB, 0x00000000, false, false, 0x00000000, 0x3010BF7F)   
         CRC-32/XFER    --> new Parameters( 32, 0x000000AF, 0x00000000, false, false, 0x00000000, 0xBD0BE338)  
         */
    }

    public class CRC : HashAlgorithm
    {
        private readonly ulong _mask;

        private readonly ulong[] _table = new ulong[256];

        private ulong _currentValue;

        public CRC(Parameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Parameters = parameters;

            _mask = ulong.MaxValue >> (64 - HashSize);

            Init();
        }

        new public byte[] ComputeHash(Stream inputStream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = inputStream.Read(buffer, 0, 4096)) > 0)
            {
                HashCore(buffer, 0, bytesRead);
            }

            return HashFinal();
        }

        public override int HashSize { get { return Parameters.HashSize; } }

        public Parameters Parameters { get; private set; }

        public UInt64[] GetTable()
        {
            var res = new UInt64[_table.Length];
            Array.Copy(_table, res, _table.Length);
            return res;
        }

        public override void Initialize()
        {
            _currentValue = Parameters.RefOut ? CrcHelper.ReverseBits(Parameters.Init, HashSize) : Parameters.Init;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _currentValue = ComputeCrc(_currentValue, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            return CrcHelper.ToBigEndianBytes(_currentValue ^ Parameters.XorOut);
        }

        private void Init()
        {
            CreateTable();
            Initialize();
        }

        #region Main functions

        private ulong ComputeCrc(ulong init, byte[] data, int offset, int length)
        {
            ulong crc = init;

            if (Parameters.RefOut)
            {
                for (int i = offset; i < offset + length; i++)
                {
                    crc = (_table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8));
                    crc &= _mask;
                }
            }
            else
            {
                int toRight = (HashSize - 8);
                toRight = toRight < 0 ? 0 : toRight;
                for (int i = offset; i < offset + length; i++)
                {
                    crc = (_table[((crc >> toRight) ^ data[i]) & 0xFF] ^ (crc << 8));
                    crc &= _mask;
                }
            }

            return crc;
        }

        private void CreateTable()
        {
            for (int i = 0; i < _table.Length; i++)
                _table[i] = CreateTableEntry(i);
        }

        private ulong CreateTableEntry(int index)
        {
            ulong r = (ulong)index;

            if (Parameters.RefIn)
                r = CrcHelper.ReverseBits(r, HashSize);
            else if (HashSize > 8)
                r <<= (HashSize - 8);

            ulong lastBit = (1ul << (HashSize - 1));

            for (int i = 0; i < 8; i++)
            {
                if ((r & lastBit) != 0)
                    r = ((r << 1) ^ Parameters.Poly);
                else
                    r <<= 1;
            }

            if (Parameters.RefIn)
                r = CrcHelper.ReverseBits(r, HashSize);

            return r & _mask;
        }

        #endregion Main functions

        #region Test functions

        public bool IsRight()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("123456789");

            var hashBytes = ComputeHash(bytes, 0, bytes.Length);

            var hash = CrcHelper.FromBigEndian(hashBytes, HashSize);

            if (hash != Parameters.Check)
                throw new Exception("Algo check failure!");

            return hash == Parameters.Check;
        }

        #endregion Test functions
    }

}
