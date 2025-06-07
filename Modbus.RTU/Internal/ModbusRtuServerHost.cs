using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Contracts;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU.Internal
{
    internal sealed class ModbusRtuServerHost : ModbusServerHost<ModbusRtuProtocol>
    {
        readonly SerialPort _serialPort;
        readonly IModbusServerData _serverData;
        readonly byte _unitIdentifier;
        bool disposedValue;

        internal ModbusRtuServerHost(SerialPort serialPort, IModbusServerData serverData, byte unitIdentifier)
        {
            ArgumentNullException.ThrowIfNull(serialPort);
            ArgumentNullException.ThrowIfNull(serverData);

            _serialPort = serialPort;
            _serverData = serverData;
            _unitIdentifier = unitIdentifier;
        }

        public override async Task StartAsync(CancellationToken token = default)
        {
            if (MaxServerConnections != 1)
                throw new NotSupportedException("MaxServerConnections must always be 1");

            await base.StartAsync(token);

            _serialPort.Open();

            //Create server
            ModbusServer<ModbusRtuProtocol>? server = null;
            try
            {
                var modbusServerProtocol = new ModbusRtuProtocol(_serialPort)
                {
                    UnitIdentifier = _unitIdentifier,
                };
                server = new ModbusServer<ModbusRtuProtocol>(modbusServerProtocol, _serverData);

                AddConnection(server, default);
            }
            catch (Exception ex)
            {
                server?.Dispose();
            }
        }
        public override async Task StopAsync(CancellationToken token = default)
        {
            _serialPort.Close();
            await base.StopAsync(token);
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                base.Dispose(disposing);
                _serialPort.Close();
                disposedValue = true;
            }
        }
    }
}
