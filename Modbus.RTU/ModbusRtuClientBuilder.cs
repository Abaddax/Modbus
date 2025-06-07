using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Builder;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU
{
    public sealed class ModbusRtuClientBuilder : ModbusClientBuilder<ModbusRtuProtocol>
    {
        SerialPort? _serialPort = null;

        public ModbusRtuClientBuilder WithSerialPort(string portName)
        {
            ArgumentException.ThrowIfNullOrEmpty(portName);

            _serialPort ??= new SerialPort();
            _serialPort.PortName = portName;
            return this;
        }

        public override ModbusClient<ModbusRtuProtocol> Build()
        {
            if (_serialPort == null)
                throw new InvalidOperationException("Serial-Port is not specified");
            var modbusClientProtocol = new ModbusRtuProtocol(_serialPort)
            {
                UnitIdentifier = UnitIdentifier
            };

            return new ModbusClient<ModbusRtuProtocol>(modbusClientProtocol);
        }
    }
}
