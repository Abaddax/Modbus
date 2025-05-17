using Modbus.Protocol;
using Modbus.Protocol.Tests.Helper;
using System.Net;
using System.Net.Sockets;

namespace Modbus.TCP.Tests
{
    public class ServerInteropTest
    {
        TcpListener _listener;
        EasyModbus.ModbusClient _client;

        [SetUp]
        public void Setup()
        {
            _client = new EasyModbus.ModbusClient("127.0.0.1", 502);
            _listener = new TcpListener(IPAddress.Loopback, 502);
            _listener.Start();
        }

        [TearDown]
        public void Teardown()
        {
            _client.Disconnect();
            _listener.Dispose();
        }

        [Test]
        public async Task ShouldReadCoils()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveCoil: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }));

            await modbusServer.StartAsync();

            var value = _client.ReadCoils(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = _client.ReadCoils(0x1234, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadDiscreteInputs()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveDiscreteInput: (address) =>
                    {
                        if (address == 0x1234)
                            return true;
                        return false;
                    }));

            await modbusServer.StartAsync();

            var value = _client.ReadDiscreteInputs(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = _client.ReadDiscreteInputs(0x1234, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadHoldingRegisters()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveHoldingRegister: (address) =>
                    {
                        return (short)address;
                    }));

            await modbusServer.StartAsync();

            var value = _client.ReadHoldingRegisters(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(0x1000));

            value = _client.ReadHoldingRegisters(0x1234, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x1234 + i));
            }
        }
        [Test]
        public async Task ShouldReadInputRegisters()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    retreiveInputRegister: (address) =>
                    {
                        return (short)address;
                    }));

            await modbusServer.StartAsync();

            var value = _client.ReadInputRegisters(0x1000, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(0x1000));

            value = _client.ReadInputRegisters(0x1234, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x1234 + i));
            }
        }

        [Test]
        public async Task ShouldWriteSingleCoil()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            Dictionary<ushort, bool> values = new();
            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusServer.StartAsync();

            _client.WriteSingleCoil(0x1000, true);
            _client.WriteSingleCoil(0x1234, false);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.True);
            Assert.That(values[0x1234], Is.False);
        }
        [Test]
        public async Task ShouldWriteSingleRegister()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            Dictionary<ushort, short> values = new();
            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusServer.StartAsync();

            _client.WriteSingleRegister(0x1000, 0x1234);
            _client.WriteSingleRegister(0x1234, -0x1234);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[0x1000], Is.EqualTo(0x1234));
            Assert.That(values[0x1234], Is.EqualTo(-0x1234));
        }

        [Test]
        public async Task ShouldWriteMultipleCoils()
        {
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            Dictionary<ushort, bool> values = new();
            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeCoil: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusServer.StartAsync();

            _client.WriteMultipleCoils(0x1000, [true, false, false, true, false, true]);

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
            var serverTask = _listener.AcceptTcpClientAsync();
            _client.Connect();
            using var server = await serverTask;

            using var modbusServerProtocol = new ModbusTCPProtocol(server.GetStream());

            Dictionary<ushort, short> values = new();
            using var modbusServer = new ModbusServer<ModbusTCPProtocol>(modbusServerProtocol,
                new TestModbusServerData(
                    storeHoldingRegister: (address, value) =>
                    {
                        values[address] = value;
                    }));

            await modbusServer.StartAsync();

            _client.WriteMultipleRegisters(0x1000, [0, 10, -10, short.MaxValue, short.MinValue]);

            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values[0x1000], Is.EqualTo(0));
            Assert.That(values[0x1001], Is.EqualTo(10));
            Assert.That(values[0x1002], Is.EqualTo(-10));
            Assert.That(values[0x1003], Is.EqualTo(short.MaxValue));
            Assert.That(values[0x1004], Is.EqualTo(short.MinValue));
        }
    }
}
