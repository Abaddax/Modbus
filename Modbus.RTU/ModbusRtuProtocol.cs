using Abaddax.Modbus.Protocol.Contracts;
using Abaddax.Modbus.Protocol.Protocol;
using Abaddax.Modbus.RTU.Internal;
using System.Buffers.Binary;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU
{
    public sealed class ModbusRtuProtocol : IModbusProtocol
    {
        readonly SerialPort _serialPort;
        private bool disposedValue;

        readonly byte _unitIdentifier = 1;
        readonly MemoryStream _frameBuffer = new();

        Guid? _currentTransactionID = null;
        DateTime? _dateTimeLastRead = null;

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            TimeSpan characterTime = TimeSpan.FromMilliseconds(1000d / _serialPort.BaudRate * 11);
            lock (_serialPort)
            {
                if (_serialPort.BytesToRead <= 0)
                    return;
                do
                {
                    if (_serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    _dateTimeLastRead = null;
                    byte[] buffer = new byte[_serialPort.BytesToRead];
                    var read = _serialPort.Read(buffer, 0, buffer.Length);
                    if (read < 0)
                    {
                        //Failed to read
                        _serialPort.Close();
                        return;
                    }
                    _frameBuffer.Write(buffer, 0, read);
                }
                //Wait for frame end (3.5 * characterTime)
                while (_dateTimeLastRead != null && DateTime.UtcNow - _dateTimeLastRead > characterTime * 4);

                var message = _frameBuffer.ToArray();
                _frameBuffer.Seek(0, SeekOrigin.Begin);
                _dateTimeLastRead = null;

                OnModbusMessageReceived(message);
            }
        }
        private void OnModbusMessageReceived(byte[] message)
        {
            var span = message.AsSpan();

            var address = span[0];
            if (address != _unitIdentifier)
                return;

            //Not for this device
            var function = (ModbusFunctionCode)span[1];

            //Check crc
            var crc = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(span.Length - 2, 2));
            var calculatedCrc = CRCHelper.CalculateCRC16(span.Slice(0, span.Length - 2));
            if (crc != calculatedCrc)
            {
                //TODO
                throw new Exception("CRC failure");
            }

            Guid transactionId = Guid.NewGuid();
            lock (_serialPort)
            {
                //Request
                if (_currentTransactionID == null)
                {
                    _currentTransactionID = transactionId;
                }
                //Response
                else
                {
                    transactionId = _currentTransactionID.Value;
                    //Remove
                    _currentTransactionID = null;
                }
            }
            var pdu = new ModbusPDU()
            {
                ID = transactionId,
                FunctionCode = function,
                Data = span.Slice(2, span.Length - 4).ToArray()
            };
            OnModbusPDUReceived?.Invoke(this, pdu);
        }

        public ModbusRtuProtocol(SerialPort serialPort)
        {
            ArgumentNullException.ThrowIfNull(serialPort);
            _serialPort = serialPort;

            _serialPort.DataReceived += OnSerialDataReceived;
        }

        public async Task ConnectAsync(CancellationToken token)
        {
            if (!Connected)
                _serialPort.Open();

            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        public byte UnitIdentifier { get => _unitIdentifier; init => _unitIdentifier = value; }
        public bool Connected => _serialPort.IsOpen;

        public event EventHandler<ModbusPDU> OnModbusPDUReceived;

        public async Task SendModbusPDUAsync(ModbusPDU pdu, CancellationToken token)
        {
            var buffer = new byte[4 + pdu.Data.Length];

            buffer[0] = _unitIdentifier;
            buffer[1] = (byte)pdu.FunctionCode;
            Buffer.BlockCopy(pdu.Data, 0, buffer, 2, pdu.Data.Length);
            var crc = CRCHelper.CalculateCRC16(buffer.AsSpan(0, pdu.Data.Length + 2));
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(pdu.Data.Length + 2), crc);

        SEND:
            lock (_serialPort)
            {
                //Request
                if (_currentTransactionID == null)
                    _currentTransactionID = pdu.ID;
                //Response
                else if (_currentTransactionID == pdu.ID)
                    _currentTransactionID = null;
                //Other pending transaction
                else
                    goto WAIT;

                _serialPort.Write(buffer, 0, buffer.Length);
                return;
            }
        WAIT:
            await Task.Delay(10, token);
            goto SEND;
        }

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _serialPort.DataReceived -= OnSerialDataReceived;
                    _serialPort.Close();
                }
                _frameBuffer.Dispose();
                _serialPort.Dispose();
                disposedValue = true;
            }
        }
        ~ModbusRtuProtocol()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Static

        #endregion

    }
}
