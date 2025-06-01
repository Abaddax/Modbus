using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Builder;
using Abaddax.Modbus.TCP.Internal;
using System.Net;
using System.Net.Sockets;

namespace Abaddax.Modbus.TCP
{
    public sealed class ModbusTcpServerBuilder : ModbusServerBuilder<ModbusTcpProtocol>
    {
        TcpListener? _listener = null;

        public ModbusTcpServerBuilder WithEndpoint(IPAddress localaddr, int port)
        {
            ArgumentNullException.ThrowIfNull(localaddr);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(port, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65535);

            _listener = new TcpListener(localaddr, port);
            return this;
        }
        public ModbusTcpServerBuilder WithEndpoint(IPEndPoint localEP)
        {
            ArgumentNullException.ThrowIfNull(localEP);

            _listener = new TcpListener(localEP);
            return this;
        }
        public ModbusTcpServerBuilder WithTcpListener(TcpListener listener)
        {
            ArgumentNullException.ThrowIfNull(listener);

            _listener = listener;
            return this;
        }

        public override ModbusServerHost<ModbusTcpProtocol> Build()
        {
            if (ServerData == null)
                throw new InvalidOperationException("Server-Data is not specified");
            if (_listener == null)
                throw new InvalidOperationException("Server-Listener is not specified");

            return new ModbusTcpServerHost(_listener, ServerData, UnitIdentifier)
            {
                MaxServerConnection = MaxServerConnections
            };
        }

    }
}
