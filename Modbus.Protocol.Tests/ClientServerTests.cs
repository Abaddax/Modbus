using Modbus.Protocol.Protocol;
using Modbus.Protocol.Tests.Helper;

namespace Modbus.Protocol.Tests
{
    public class ClientServerTests
    {
        [Test]
        public async Task ShouldReadCoils()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveCoil: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }));

            var value = await modbusClient.ReadCoilsAsync(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = await modbusClient.ReadCoilsAsync(0x1234, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadDiscreteInputs()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveDiscreteInput: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }));

            var value = await modbusClient.ReadDiscreteInputsAsync(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = await modbusClient.ReadDiscreteInputsAsync(0x1234, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadHoldingRegisters()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveHoldingRegister: (address) =>
                    {
                        return (short)address;
                    }));

            var value = await modbusClient.ReadHoldingRegistersAsync(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(0x1000));

            value = await modbusClient.ReadHoldingRegistersAsync(0x1234, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x1234 + i));
            }
        }
        [Test]
        public async Task ShouldReadInputRegisters()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveInputRegister: (address) =>
                    {
                        return (short)address;
                    }));

            var value = await modbusClient.ReadInputRegistersAsync(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(0x1000));

            value = await modbusClient.ReadInputRegistersAsync(0x1234, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x1234 + i));
            }
        }

        [Test]
        public async Task ShouldWriteSingleCoil()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            Dictionary<ushort, bool> values = new();
            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusClient.WriteSingleCoilAsync(0x1000, true);
            await modbusClient.WriteSingleCoilAsync(0x1234, false);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.True);
            Assert.That(values[0x1234], Is.False);
        }
        [Test]
        public async Task ShouldWriteSingleRegister()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            Dictionary<ushort, short> values = new();
            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusClient.WriteSingleRegisterAsync(0x1000, 0x1234);
            await modbusClient.WriteSingleRegisterAsync(0x1234, -0x1234);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.EqualTo(0x1234));
            Assert.That(values[0x1234], Is.EqualTo(-0x1234));
        }

        [Test]
        public async Task ShouldWriteMultipleCoils()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            Dictionary<ushort, bool> values = new();
            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusClient.WriteMultipleCoilsAsync(0x1000, [true, false, false, true, false, true]);

            Assert.That(values.Count, Is.EqualTo(6));
            Assert.That(values[0x1000], Is.True);
            Assert.That(values[0x1001], Is.False);
            Assert.That(values[0x1002], Is.False);
            Assert.That(values[0x1003], Is.True);
            Assert.That(values[0x1004], Is.False);
            Assert.That(values[0x1005], Is.True);
        }
        [Test]
        public async Task ShouldWriteMultipleRegisters()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            Dictionary<ushort, short> values = new();
            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusClient.WriteMultipleRegistersAsync(0x1000, [0, 10, -10, short.MaxValue, short.MinValue]);

            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values[0x1000], Is.EqualTo(0));
            Assert.That(values[0x1001], Is.EqualTo(10));
            Assert.That(values[0x1002], Is.EqualTo(-10));
            Assert.That(values[0x1003], Is.EqualTo(short.MaxValue));
            Assert.That(values[0x1004], Is.EqualTo(short.MinValue));
        }

        [Test]
        public async Task ShouldThrowOnError()
        {
            using var modbusClientProtocol = new TestModbusProtocol();
            using var modbusServerProtocol = new TestModbusProtocol();

            //Wire up
            modbusClientProtocol.OnModbusPDUSend += modbusServerProtocol.ReceiveModbusPDUAsync;
            modbusServerProtocol.OnModbusPDUSend += modbusClientProtocol.ReceiveModbusPDUAsync;

            Dictionary<ushort, short> values = new();
            using var modbusClient = new ModbusClient<TestModbusProtocol>(modbusClientProtocol);
            using var modbusServer = new ModbusServer<TestModbusProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveCoil: (address) =>
                    {
                        throw new Exception("TEST");
                    }));

            var exception = Assert.ThrowsAsync<ModbusException>(async () => await modbusClient.ReadCoilsAsync(0x1000, 1));

            Assert.That(exception.ExceptionCode, Is.EqualTo(ModbusExceptionCode.DeviceFailure));
        }

    }
}
