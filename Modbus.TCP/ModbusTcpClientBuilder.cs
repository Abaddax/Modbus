using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Builder;
using System.Net.Sockets;

namespace Abaddax.Modbus.TCP
{
    public sealed class ModbusTcpClientBuilder : ModbusClientBuilder<ModbusTcpProtocol>
    {
        Func<Stream>? _connectFunc = null;

        public ModbusTcpClientBuilder WithServer(string ip, int port)
        {
            ArgumentException.ThrowIfNullOrEmpty(ip);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(port, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65535);
            _connectFunc = () =>
            {
                var client = new TcpClient(ip, port);
                return client.GetStream();
            };
            return this;
        }
        public ModbusTcpClientBuilder WithServerConnection(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            _connectFunc = () =>
            {
                return stream;
            };
            return this;
        }

        public override ModbusClient<ModbusTcpProtocol> Build()
        {
            if (_connectFunc == null)
                throw new InvalidOperationException("Server is not specified");
            var modbusClientProtocol = new ModbusTcpProtocol(_connectFunc)
            {
                UnitIdentifier = UnitIdentifier
            };

            return new ModbusClient<ModbusTcpProtocol>(modbusClientProtocol);
        }
    }
}
