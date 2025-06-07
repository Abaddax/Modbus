using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Builder;
using Abaddax.Modbus.RTU.Internal;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU
{
    public sealed class ModbusRtuServerBuilder : ModbusServerBuilder<ModbusRtuProtocol>
    {
        SerialPort? _serialPort = null;

        public ModbusRtuServerBuilder WithSerialPort(string portName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(portName);

            _serialPort ??= new SerialPort();
            _serialPort.PortName = portName;
            return this;
        }

        public override ModbusServerHost<ModbusRtuProtocol> Build()
        {
            if (ServerData == null)
                throw new InvalidOperationException("Server-Data is not specified");
            if (_serialPort == null)
                throw new InvalidOperationException("Serial-Port is not specified");
            if (MaxServerConnections != 1)
                throw new NotSupportedException("MaxServerConnections must always be 1");

            return new ModbusRtuServerHost(_serialPort, ServerData, UnitIdentifier)
            {
                MaxServerConnections = 1
            };
        }

    }
}
