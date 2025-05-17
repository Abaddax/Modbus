using System.Buffers.Binary;

namespace Modbus.Protocol.Extensions
{
    public static class ModbusHelper
    {
        public static IEnumerable<byte> GetRegisterBytes(this IEnumerable<short> registers) => registers.ReadBytes();
        public static IEnumerable<short> GetRegisters(this IEnumerable<byte> data) => data.ReadAsShort();

        #region ReadAs
        /// <summary>
        /// 1 byte -> 8 bool
        /// </summary>
        public static IEnumerable<bool> ReadAsBool(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                yield return (value & 0b00000001) >> 0 == 1;
                yield return (value & 0b00000010) >> 1 == 1;
                yield return (value & 0b00000100) >> 2 == 1;
                yield return (value & 0b00001000) >> 3 == 1;
                yield return (value & 0b00010000) >> 4 == 1;
                yield return (value & 0b00100000) >> 5 == 1;
                yield return (value & 0b01000000) >> 6 == 1;
                yield return (value & 0b10000000) >> 7 == 1;
            }
        }

        /// <summary>
        /// 2 byte -> 1 short
        /// </summary>
        public static IEnumerable<short> ReadAsShort(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 2 != 0)
                {
                    lastByte = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte, value.Item };
                yield return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
        }
        /// <summary>
        /// 2 byte -> 1 ushort
        /// </summary>
        public static IEnumerable<ushort> ReadAsUShort(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte = 0;
            foreach (var value in data.Index())
            {
                if (value.Index % 2 != 1)
                {
                    lastByte = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte, value.Item };
                yield return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
        }

        /// <summary>
        /// 4 byte -> 1 int
        /// </summary>
        public static IEnumerable<int> ReadAsInt(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 4 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, value.Item };
                yield return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
        }
        /// <summary>
        /// 4 byte -> 1 uint
        /// </summary>
        public static IEnumerable<uint> ReadAsUInt(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 4 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, value.Item };
                yield return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
        }

        /// <summary>
        /// 8 byte -> 1 long
        /// </summary>
        public static IEnumerable<long> ReadAsLong(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0, lastByte3 = 0, lastByte4 = 0, lastByte5 = 0, lastByte6 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 8 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = lastByte3;
                    lastByte3 = lastByte4;
                    lastByte4 = lastByte5;
                    lastByte5 = lastByte6;
                    lastByte6 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, lastByte3, lastByte4, lastByte5, lastByte6, value.Item };
                yield return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
        }
        /// <summary>
        /// 8 byte -> 1 ulong
        /// </summary>
        public static IEnumerable<ulong> ReadAsULong(this IEnumerable<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0, lastByte3 = 0, lastByte4 = 0, lastByte5 = 0, lastByte6 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 8 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = lastByte3;
                    lastByte3 = lastByte4;
                    lastByte4 = lastByte5;
                    lastByte5 = lastByte6;
                    lastByte6 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, lastByte3, lastByte4, lastByte5, lastByte6, value.Item };
                yield return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
        }

        /// <summary>
        /// 4 byte -> 1 float
        /// </summary>
        public static IEnumerable<float> ReadAsFloat(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 4 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, value.Item };
                yield return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
        }
        /// <summary>
        /// 8 byte -> 1 double
        /// </summary>
        public static IEnumerable<double> ReadAsDouble(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte lastByte0 = 0, lastByte1 = 0, lastByte2 = 0, lastByte3 = 0, lastByte4 = 0, lastByte5 = 0, lastByte6 = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 8 != 0)
                {
                    lastByte0 = lastByte1;
                    lastByte1 = lastByte2;
                    lastByte2 = lastByte3;
                    lastByte3 = lastByte4;
                    lastByte4 = lastByte5;
                    lastByte5 = lastByte6;
                    lastByte6 = value.Item;
                    continue;
                }

                Span<byte> buffer = stackalloc byte[] { lastByte0, lastByte1, lastByte2, lastByte3, lastByte4, lastByte5, lastByte6, value.Item };
                yield return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
        }
        #endregion

        #region ReadBytes
        /// <summary>
        /// 8 bool -> 1 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<bool> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool remaining = false;
            byte current = 0;
            foreach (var value in data.Index())
            {
                if ((value.Index + 1) % 8 != 0)
                {
                    if (value.Item)
                        current |= (byte)(1 << value.Index % 8);
                    remaining = true;
                    continue;
                }
                yield return current;
                remaining = false;
                current = 0;
            }
            if (remaining)
                yield return current;
        }

        /// <summary>
        /// 1 short -> 2 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<short> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[2];
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                yield return byte0;
                yield return byte1;
            }
        }
        /// <summary>
        /// 1 ushort -> 2 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<ushort> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                yield return byte0;
                yield return byte1;
            }
        }

        /// <summary>
        /// 1 int -> 4 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<int> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[4];
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
            }
        }
        /// <summary>
        /// 1 uint -> 4 byte 
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<uint> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
            }
        }

        /// <summary>
        ///  1 long -> 8 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<long> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[8];
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                var byte4 = buffer[4];
                var byte5 = buffer[5];
                var byte6 = buffer[6];
                var byte7 = buffer[7];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
                yield return byte4;
                yield return byte5;
                yield return byte6;
                yield return byte7;
            }
        }
        /// <summary>
        /// 1 ulong -> 8 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<ulong> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[8];
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                var byte4 = buffer[4];
                var byte5 = buffer[5];
                var byte6 = buffer[6];
                var byte7 = buffer[7];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
                yield return byte4;
                yield return byte5;
                yield return byte6;
                yield return byte7;
            }
        }

        /// <summary>
        /// 1 float -> 4 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<float> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[4];
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
            }
        }
        /// <summary>
        /// 1 double -> 8 byte
        /// </summary>
        public static IEnumerable<byte> ReadBytes(this IEnumerable<double> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var value in data)
            {
                Span<byte> buffer = stackalloc byte[8];
                BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
                var byte0 = buffer[0];
                var byte1 = buffer[1];
                var byte2 = buffer[2];
                var byte3 = buffer[3];
                var byte4 = buffer[4];
                var byte5 = buffer[5];
                var byte6 = buffer[6];
                var byte7 = buffer[7];
                yield return byte0;
                yield return byte1;
                yield return byte2;
                yield return byte3;
                yield return byte4;
                yield return byte5;
                yield return byte6;
                yield return byte7;
            }
        }
        #endregion

        public static IEnumerable<T> ToEnumerable<T>(this T value)
        {
            yield return value;
        }
        public static IEnumerable<T> ToEnumerable<T>(this T? value) where T : struct
        {
            if (value != null)
                yield return value.Value;
        }
    }
}
