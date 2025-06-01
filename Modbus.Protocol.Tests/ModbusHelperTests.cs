using Abaddax.Modbus.Protocol.Extensions;

namespace Abaddax.Modbus.Protocol.Tests
{
    public class ModbusHelperTests
    {
        [Test]
        public void ShouldConvertRegisterBytes()
        {
            var registers = new short[] { 0x1234, 0x5678 };

            var bytes = registers.GetRegisterBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(4));
            Assert.That(bytes[0], Is.EqualTo(0x12));
            Assert.That(bytes[1], Is.EqualTo(0x34));
            Assert.That(bytes[2], Is.EqualTo(0x56));
            Assert.That(bytes[3], Is.EqualTo(0x78));

            var registers2 = bytes.GetRegisters().ToArray();
            Assert.That(registers2, Is.EquivalentTo(registers));
        }

        [Test]
        public void ShouldConvertBool()
        {
            var values = new bool[] {
                true, false, false, true, true, true, false, false,
                true, false, true, false, true, false,
            };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(2));
            Assert.That(bytes[0], Is.EqualTo(0b0011_1001));
            Assert.That(bytes[1], Is.EqualTo(0b0001_0101));

            var values2 = bytes.ReadAsBool().ToArray();
            Assert.That(values2, Is.EquivalentTo(values.Concat([false, false])));
        }
        [Test]
        public void ShouldConvertShort()
        {
            var values = new short[] { short.MaxValue, short.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(4));
            Assert.That(bytes[0], Is.EqualTo(0b0111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b1000_0000));
            Assert.That(bytes[3], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsShort().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertUShort()
        {
            var values = new ushort[] { ushort.MaxValue, ushort.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(4));
            Assert.That(bytes[0], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[3], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsUShort().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertInt()
        {
            var values = new int[] { int.MaxValue, int.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(8));
            Assert.That(bytes[0], Is.EqualTo(0b0111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[3], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[4], Is.EqualTo(0b1000_0000));
            Assert.That(bytes[5], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[6], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[7], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsInt().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertUInt()
        {
            var values = new uint[] { uint.MaxValue, uint.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(8));
            Assert.That(bytes[0], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[3], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[4], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[5], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[6], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[7], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsUInt().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertLong()
        {
            var values = new long[] { long.MaxValue, long.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(16));
            Assert.That(bytes[0], Is.EqualTo(0b0111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[3], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[4], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[5], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[6], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[7], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[8], Is.EqualTo(0b1000_0000));
            Assert.That(bytes[9], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[10], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[11], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[12], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[13], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[14], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[15], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsLong().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertULong()
        {
            var values = new ulong[] { ulong.MaxValue, ulong.MinValue };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(16));
            Assert.That(bytes[0], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[1], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[2], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[3], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[4], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[5], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[6], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[7], Is.EqualTo(0b1111_1111));
            Assert.That(bytes[8], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[9], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[10], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[11], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[12], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[13], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[14], Is.EqualTo(0b0000_0000));
            Assert.That(bytes[15], Is.EqualTo(0b0000_0000));

            var values2 = bytes.ReadAsULong().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertFloat()
        {
            var values = new float[] { 0, 10, -10, float.MinValue, float.MaxValue, float.Pi, float.NegativeInfinity, float.PositiveInfinity };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(4 * values.Length));

            var values2 = bytes.ReadAsFloat().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }
        [Test]
        public void ShouldConvertDouble()
        {
            var values = new double[] { 0, 10, -10, double.MinValue, double.MaxValue, double.Pi, double.NegativeInfinity, double.PositiveInfinity };

            var bytes = values.ReadBytes().ToArray();

            Assert.That(bytes.Length, Is.EqualTo(8 * values.Length));

            var values2 = bytes.ReadAsDouble().ToArray();
            Assert.That(values2, Is.EquivalentTo(values));
        }

    }
}
