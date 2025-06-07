using Abaddax.Modbus.Protocol;
using Abaddax.Modbus.Protocol.Builder;
using Abaddax.Modbus.RTU.Internal;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU
{
    public sealed class ModbusRtuServerBuilder : ModbusServerBuilder<ModbusRtuProtocol>
    {
        SerialPort _serialPort = new SerialPort();
        public string? PortName { get => _serialPort.PortName; set => _serialPort.PortName = value; }
        public int BaudRate { get => _serialPort.BaudRate; set => _serialPort.BaudRate = value; }
        public Parity Parity { get => _serialPort.Parity; set => _serialPort.Parity = value; }
        public StopBits StopBits { get => _serialPort.StopBits; set => _serialPort.StopBits = value; }

        public ModbusRtuServerBuilder WithPortName(string portName)
        {
            ArgumentException.ThrowIfNullOrEmpty(portName);

            _serialPort.PortName = portName;
            return this;
        }
        public ModbusRtuServerBuilder WithSerialPort(SerialPort serialPort)
        {
            ArgumentNullException.ThrowIfNull(serialPort);

            _serialPort = serialPort;
            return this;
        }

        public ModbusRtuServerBuilder WithBaudRate(int baudRate)
        {
            int[] validBaudRates = [4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600];
            if (!validBaudRates.Contains(baudRate))
                throw new ArgumentException("Baudrate is not valid", nameof(baudRate));

            _serialPort.BaudRate = baudRate;
            return this;
        }
        public ModbusRtuServerBuilder WithParity(Parity parity)
        {
            _serialPort.Parity = parity;
            return this;
        }
        public ModbusRtuServerBuilder WithStopBits(StopBits stopBits)
        {
            _serialPort.StopBits = StopBits;
            return this;
        }


        public override ModbusServerHost<ModbusRtuProtocol> Build()
        {
            if (ServerData == null)
                throw new InvalidOperationException("Server-Data is not specified");
            if (string.IsNullOrEmpty(_serialPort?.PortName))
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
