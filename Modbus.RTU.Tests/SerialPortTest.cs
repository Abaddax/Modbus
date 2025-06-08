using Abaddax.Modbus.RTU.Tests.Helper;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU.Tests
{
    [NonParallelizable]
    public class SerialPortTest
    {
        [SetUp]
        public void Setup()
        {
            SerialPortPatch.Apply();
        }

        [TearDown]
        public void Teardown()
        {
            SerialPortPatch.Remove();
        }

        [Test]
        public async Task ShouldMockSerialPort()
        {
            using var server = new SerialPort()
            {
                PortName = SerialPortMock.SERVER_PORT
            };
            using var client = new SerialPort()
            {
                PortName = SerialPortMock.CLIENT_PORT
            };

            Assert.That(server.IsOpen, Is.False);
            Assert.That(client.IsOpen, Is.False);

            var connectServerTask = Task.Run(() =>
            {
                server.Open();
            });
            var connectClientTask = Task.Run(() =>
            {
                client.Open();
            });
            await connectServerTask;
            await connectClientTask;

            Assert.That(server.IsOpen, Is.True);
            Assert.That(client.IsOpen, Is.True);

            int serverDataReceived = 0;
            int clientDataReceived = 0;
            server.DataReceived += (s, e) =>
            {
                serverDataReceived += 1;
                return;
            };
            client.DataReceived += (s, e) =>
            {
                clientDataReceived += 1;
                return;
            };

            var sendBuffer = new byte[100];
            var receiveBuffer = new byte[sendBuffer.Length];
            Random.Shared.NextBytes(sendBuffer);

            Assert.That(server.BytesToRead, Is.EqualTo(0));
            Assert.That(server.BytesToWrite, Is.EqualTo(0));
            Assert.That(client.BytesToRead, Is.EqualTo(0));
            Assert.That(client.BytesToWrite, Is.EqualTo(0));

            client.Write(sendBuffer, 0, sendBuffer.Length);

            await Task.Delay(10);

            Assert.That(server.BytesToRead, Is.EqualTo(100));
            Assert.That(client.BytesToWrite, Is.EqualTo(0));
            Assert.That(serverDataReceived, Is.EqualTo(1));

            var read = server.Read(receiveBuffer, 0, receiveBuffer.Length);
            Assert.That(read, Is.EqualTo(100));
            Assert.That(server.BytesToRead, Is.EqualTo(0));
            Assert.That(receiveBuffer, Is.EquivalentTo(sendBuffer));

            //Refresh data
            Random.Shared.NextBytes(sendBuffer);

            server.Write(sendBuffer, 0, sendBuffer.Length);

            await Task.Delay(10);

            Assert.That(client.BytesToRead, Is.EqualTo(100));
            Assert.That(server.BytesToWrite, Is.EqualTo(0));
            Assert.That(clientDataReceived, Is.EqualTo(1));

            read = client.Read(receiveBuffer, 0, 50);
            Assert.That(read, Is.EqualTo(50));
            Assert.That(client.BytesToRead, Is.EqualTo(50));
            Assert.That(receiveBuffer.Take(50), Is.EquivalentTo(sendBuffer.Take(50)));

            client.DiscardInBuffer();
            Assert.That(client.BytesToRead, Is.EqualTo(0));

            client.Close();
            server.Close();


            Assert.That(server.IsOpen, Is.False);
            Assert.That(client.IsOpen, Is.False);
        }

        [Test]
        public async Task ShouldWorkWithModbusRTU()
        {
            EasyModbus.ModbusServer server = new EasyModbus.ModbusServer()
            {
                SerialPort = SerialPortMock.SERVER_PORT
            };
            EasyModbus.ModbusClient client = new EasyModbus.ModbusClient()
            {
                SerialPort = SerialPortMock.CLIENT_PORT
            };
            try
            {
                var connectServerTask = Task.Run(() =>
                {
                    server.Listen();
                });
                var connectClientTask = Task.Run(() =>
                {
                    client.Connect();
                });
                await connectServerTask;
                await connectClientTask;

                //Uses offset 1 for some reason
                server.coils[0x01 + 1] = false;
                server.coils[0x10 + 1] = true;
                server.coils[0x11 + 1] = false;

                var value = client.ReadCoils(0x01, 1);

                Assert.That(value.Length, Is.EqualTo(1));
                Assert.That(value[0], Is.False);

                value = client.ReadCoils(0x10, 2);
                Assert.That(value.Length, Is.EqualTo(2));
                Assert.That(value[0], Is.True);
                Assert.That(value[1], Is.False);
            }
            finally
            {
                server.StopListening();
                client.Disconnect();
            }
        }
    }
}
