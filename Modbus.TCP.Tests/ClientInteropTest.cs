namespace Modbus.TCP.Tests
{
    public class ClientInteropTest
    {
        EasyModbus.ModbusServer _server;

        [SetUp]
        public void Setup()
        {
            _server = new EasyModbus.ModbusServer();
            _server.Listen();
        }

        [TearDown]
        public void Teardown()
        {
            _server.StopListening();
        }

        [Test]
        public async Task ShouldReadCoils()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
                .WithServer("127.0.0.1", 502)
                .Build();

            await modbusClient.ConnectAsync();

            //Uses offset 1 for some reason
            _server.coils[0x01 + 1] = false;
            _server.coils[0x10 + 1] = true;
            _server.coils[0x11 + 1] = false;

            var value = await modbusClient.ReadCoilsAsync(0x01, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = await modbusClient.ReadCoilsAsync(0x10, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadDiscreteInputs()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
              .WithServer("127.0.0.1", 502)
              .Build();

            await modbusClient.ConnectAsync();

            //Uses offset 1 for some reason
            _server.discreteInputs[0x01 + 1] = false;
            _server.discreteInputs[0x10 + 1] = true;
            _server.discreteInputs[0x11 + 1] = false;

            var value = await modbusClient.ReadDiscreteInputsAsync(0x01, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.False);

            value = await modbusClient.ReadDiscreteInputsAsync(0x10, 2);
            Assert.That(value.Length, Is.EqualTo(2));
            Assert.That(value[0], Is.True);
            Assert.That(value[1], Is.False);
        }
        [Test]
        public async Task ShouldReadHoldingRegisters()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
               .WithServer("127.0.0.1", 502)
               .Build();

            await modbusClient.ConnectAsync();

            //Uses offset 1 for some reason
            _server.holdingRegisters[0x01 + 1] = -0x1234;
            for (int i = 0; i < 10; i++)
            {
                _server.holdingRegisters[0x10 + 1 + i] = (short)(0x10 + i);
            }

            var value = await modbusClient.ReadHoldingRegistersAsync(0x01, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(-0x1234));

            value = await modbusClient.ReadHoldingRegistersAsync(0x10, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x10 + i));
            }
        }
        [Test]
        public async Task ShouldReadInputRegisters()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
              .WithServer("127.0.0.1", 502)
              .Build();

            await modbusClient.ConnectAsync();

            //Uses offset 1 for some reason
            _server.inputRegisters[0x01 + 1] = -0x1234;
            for (int i = 0; i < 10; i++)
            {
                _server.inputRegisters[0x10 + 1 + i] = (short)(0x10 + i);
            }

            var value = await modbusClient.ReadInputRegistersAsync(0x01, 1);

            Assert.That(value.Length, Is.EqualTo(1));
            Assert.That(value[0], Is.EqualTo(-0x1234));

            value = await modbusClient.ReadInputRegistersAsync(0x10, 10);
            Assert.That(value.Length, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(value[i], Is.EqualTo(0x10 + i));
            }
        }

        [Test]
        public async Task ShouldWriteSingleCoil()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
              .WithServer("127.0.0.1", 502)
              .Build();

            await modbusClient.ConnectAsync();

            await modbusClient.WriteSingleCoilAsync(0x01, true);
            await modbusClient.WriteSingleCoilAsync(0x10, false);
            await modbusClient.WriteSingleCoilAsync(0x11, true);

            Assert.That(_server.coils[0x01 + 1], Is.True);
            Assert.That(_server.coils[0x10 + 1], Is.False);
            Assert.That(_server.coils[0x11 + 1], Is.True);
        }
        [Test]
        public async Task ShouldWriteSingleRegister()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
               .WithServer("127.0.0.1", 502)
               .Build();

            await modbusClient.ConnectAsync();

            await modbusClient.WriteSingleRegisterAsync(0x01, 0x1234);
            await modbusClient.WriteSingleRegisterAsync(0x10, -0x1234);

            Assert.That(_server.holdingRegisters[0x01 + 1], Is.EqualTo(0x1234));
            Assert.That(_server.holdingRegisters[0x10 + 1], Is.EqualTo(-0x1234));
        }
        [Test]
        public async Task ShouldWriteMultipleCoils()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
              .WithServer("127.0.0.1", 502)
              .Build();

            await modbusClient.ConnectAsync();

            await modbusClient.WriteMultipleCoilsAsync(0x01, [true, false, false, true, false, true]);

            Assert.That(_server.coils[0x01 + 1], Is.True);
            Assert.That(_server.coils[0x02 + 1], Is.False);
            Assert.That(_server.coils[0x03 + 1], Is.False);
            Assert.That(_server.coils[0x04 + 1], Is.True);
            Assert.That(_server.coils[0x05 + 1], Is.False);
            Assert.That(_server.coils[0x06 + 1], Is.True);
        }
        [Test]
        public async Task ShouldWriteMultipleRegisters()
        {
            using var modbusClient = new ModbusTcpClientBuilder()
              .WithServer("127.0.0.1", 502)
              .Build();

            await modbusClient.ConnectAsync();

            await modbusClient.WriteMultipleRegistersAsync(0x01, [0, 10, -10, short.MaxValue, short.MinValue]);

            Assert.That(_server.holdingRegisters[0x01 + 1], Is.EqualTo(0));
            Assert.That(_server.holdingRegisters[0x02 + 1], Is.EqualTo(10));
            Assert.That(_server.holdingRegisters[0x03 + 1], Is.EqualTo(-10));
            Assert.That(_server.holdingRegisters[0x04 + 1], Is.EqualTo(short.MaxValue));
            Assert.That(_server.holdingRegisters[0x05 + 1], Is.EqualTo(short.MinValue));
        }
    }
}
