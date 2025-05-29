using Abaddax.Utilities.Network;
using Modbus.Protocol;
using Modbus.Protocol.Contracts;
using System.Net.Sockets;

namespace Modbus.TCP.Internal
{
    internal sealed class ModbusTcpServerHost : ModbusServerHost<ModbusTcpProtocol>
    {
        readonly TcpListener _listener;
        readonly IModbusServerData _serverData;
        readonly byte _unitIdentifier;
        bool disposedValue;

        private async Task OnClientConnected(TcpClient client, CancellationToken token)
        {
            ModbusServer<ModbusTcpProtocol>? server = null;
            try
            {
                var modbusServerProtocol = new ModbusTcpProtocol(() => client.GetStream())
                {
                    UnitIdentifier = _unitIdentifier,
                };
                server = new ModbusServer<ModbusTcpProtocol>(modbusServerProtocol, _serverData);

                AddConnection(server, token);
            }
            catch (Exception ex)
            {
                server?.Dispose();
            }
        }
        internal ModbusTcpServerHost(TcpListener listener, IModbusServerData serverData, byte unitIdentifier)
        {
            ArgumentNullException.ThrowIfNull(listener);
            ArgumentNullException.ThrowIfNull(serverData);

            _listener = listener;
            _serverData = serverData;
            _unitIdentifier = unitIdentifier;
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();

            _listener.Start();
            //Run in background
            _ = _listener.AcceptTcpClientsAsync(OnClientConnected, CancellationToken);
        }
        public override async Task StopAsync()
        {
            _listener.Stop();
            await base.StopAsync();
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                base.Dispose(disposing);
                _listener.Dispose();
                disposedValue = true;
            }
        }
    }
}
