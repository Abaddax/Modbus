using Abaddax.Modbus.Protocol.Builder.Extensions;
using Abaddax.Modbus.Protocol.Tests.Helper;
using System.Net;
using System.Net.Sockets;

namespace Abaddax.Modbus.TCP.Tests
{
    [NonParallelizable]
    public class ClientServerTests
    {
        const int _port = 10502;

        TcpListener _listener;

        [SetUp]
        public void Setup()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
        }

        [TearDown]
        public void Teardown()
        {
            _listener.Dispose();
        }

        [Test]
        public async Task ShouldReadCoils()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
                .WithServer("127.0.0.1", _port)
                .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    retreiveCoil: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

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
            using var modbusClient = new ModbusTcpClientBuilder()
                 .WithServer("127.0.0.1", _port)
                 .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    retreiveDiscreteInput: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

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
            using var modbusClient = new ModbusTcpClientBuilder()
                .WithServer("127.0.0.1", _port)
                .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    retreiveHoldingRegister: (address) =>
                    {
                        return (short)address;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

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
            using var modbusClient = new ModbusTcpClientBuilder()
                .WithServer("127.0.0.1", _port)
                .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    retreiveInputRegister: (address) =>
                    {
                        return (short)address;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

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
            Dictionary<ushort, bool> values = new();

            using var modbusClient = new ModbusTcpClientBuilder()
               .WithServer("127.0.0.1", _port)
               .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

            await modbusClient.WriteSingleCoilAsync(0x1000, true);
            await modbusClient.WriteSingleCoilAsync(0x1234, false);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.True);
            Assert.That(values[0x1234], Is.False);
        }
        [Test]
        public async Task ShouldWriteSingleRegister()
        {
            Dictionary<ushort, short> values = new();

            using var modbusClient = new ModbusTcpClientBuilder()
               .WithServer("127.0.0.1", _port)
               .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

            await modbusClient.WriteSingleRegisterAsync(0x1000, 0x1234);
            await modbusClient.WriteSingleRegisterAsync(0x1234, -0x1234);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.EqualTo(0x1234));
            Assert.That(values[0x1234], Is.EqualTo(-0x1234));
        }

        [Test]
        public async Task ShouldWriteMultipleCoils()
        {
            Dictionary<ushort, bool> values = new();

            using var modbusClient = new ModbusTcpClientBuilder()
                .WithServer("127.0.0.1", _port)
                .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

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
            Dictionary<ushort, short> values = new();

            using var modbusClient = new ModbusTcpClientBuilder()
               .WithServer("127.0.0.1", _port)
               .Build();
            using var modbusServer = new ModbusTcpServerBuilder()
                .WithMaxServerConnections(1)
                .WithTcpListener(_listener)
                .WithServerData(new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }))
                .Build();

            await modbusServer.StartAsync();
            await modbusClient.ConnectAsync();

            await modbusClient.WriteMultipleRegistersAsync(0x1000, [0, 10, -10, short.MaxValue, short.MinValue]);

            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values[0x1000], Is.EqualTo(0));
            Assert.That(values[0x1001], Is.EqualTo(10));
            Assert.That(values[0x1002], Is.EqualTo(-10));
            Assert.That(values[0x1003], Is.EqualTo(short.MaxValue));
            Assert.That(values[0x1004], Is.EqualTo(short.MinValue));
        }

    }
}
