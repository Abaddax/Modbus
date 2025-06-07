using Abaddax.Modbus.Protocol.Contracts;
using Abaddax.Modbus.Protocol.Extensions;
using Abaddax.Modbus.Protocol.Protocol;
using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace Abaddax.Modbus.Protocol
{
    public sealed class ModbusClient<TModbusProtocol> : IDisposable where TModbusProtocol : IModbusProtocol
    {
        private readonly TModbusProtocol _protocol;
        private readonly ConcurrentDictionary<Guid, ModbusPDU?> _receivedPDUs = new();

        private bool _disposedValue = false;

        public bool Connected => _protocol.Connected;

        public ModbusClient(TModbusProtocol protocol)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

            _protocol.OnModbusPDUReceived += (sender, pdu) =>
                {
                    _receivedPDUs[pdu.ID] = pdu;
                };
        }

        public async Task ConnectAsync(CancellationToken token = default)
        {
            if (!_protocol.Connected)
                await _protocol.ConnectAsync(token);
        }

        private async Task<ModbusPDU> SendReceiveAsync(ModbusPDU request, CancellationToken token)
        {
            if (!_protocol.Connected)
                await _protocol.ConnectAsync(token);

            do
            {
                token.ThrowIfCancellationRequested();
                request.ID = Guid.NewGuid();
            }
            while (!_receivedPDUs.TryAdd(request.ID, null));

            await _protocol.SendModbusPDUAsync(request, token);

            //Wait for response
            ModbusPDU? response = null;
            while (_receivedPDUs.TryGetValue(request.ID, out response))
            {
                token.ThrowIfCancellationRequested();
                if (response != null)
                    break;
                await Task.Delay(10, token);
            }

            if (!_receivedPDUs.TryRemove(request.ID, out response) || response == null)
                throw new Exception("Unable to receive response");

            if (response.Value.Data == null || response.Value.Data.Length == 0)
                throw new Exception("Received empty response");

            if (response.Value.FunctionCode == request.FunctionCode)
            {
                return response.Value;
            }
            else if ((int)response.Value.FunctionCode == (int)(request.FunctionCode | ModbusFunctionCode.Exception))
            {
                var errorCode = (ModbusExceptionCode)response.Value.Data[0];
                throw new ModbusException(errorCode);
            }
            else
            {
                throw new Exception("Unable to parse response");
            }
        }

        #region Read Functions
        /// <summary>
        /// Read Coils (Functioncode 0x01) 
        /// </summary>
        /// <param name="startingAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="quantity">0x0001 (1) to 0x07d0 (2000)</param>
        public async Task<bool[]> ReadCoilsAsync(int startingAddress, int quantity, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x07d0);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)startingAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)quantity);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadCoils,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (responsePdu.Data[0] + 1 != responsePdu.Data.Length)
                throw new Exception("Received invalid length payload");

            return responsePdu.Data
                .Skip(1)
                .ReadAsBool()
                .Take(quantity)
                .ToArray();
        }
        /// <summary>
        /// Read Discrete Inputs (Functioncode 0x02) 
        /// </summary>
        /// <param name="startingAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="quantity">0x0001 (1) to 0x07d0 (2000)</param>
        public async Task<bool[]> ReadDiscreteInputsAsync(int startingAddress, int quantity, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x07d0);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)startingAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)quantity);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadDiscreteInputs,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (responsePdu.Data[0] + 1 != responsePdu.Data.Length)
                throw new Exception("Received invalid length payload");

            return responsePdu.Data
                .Skip(1)
                .ReadAsBool()
                .Take(quantity)
                .ToArray();
        }
        /// <summary>
        /// Read Holding Registers (Functioncode 0x03) 
        /// </summary>
        /// <param name="startingAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="quantity">0x0001 (1) to 0x007d (125)</param>
        public async Task<short[]> ReadHoldingRegistersAsync(int startingAddress, int quantity, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x007d);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)startingAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)quantity);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadHoldingRegisters,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (responsePdu.Data[0] + 1 != responsePdu.Data.Length)
                throw new Exception("Received invalid length payload");

            //TODO
            return responsePdu.Data
                .Skip(1)
                .ReadAsShort()
                .Take(quantity)
                .ToArray();
        }
        /// <summary>
        /// Read Input Registers (Functioncode 0x04) 
        /// </summary>
        /// <param name="startingAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="quantity">0x0001 (1) to 0x007d (125)</param>
        public async Task<short[]> ReadInputRegistersAsync(int startingAddress, int quantity, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x007d);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)startingAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)quantity);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadInputRegisters,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (responsePdu.Data[0] + 1 != responsePdu.Data.Length)
                throw new Exception("Received invalid length payload");

            //TODO
            return responsePdu.Data
                .Skip(1)
                .ReadAsShort()
                .Take(quantity)
                .ToArray();
        }
        #endregion

        #region Write Functions
        /// <summary>
        /// Write Single Coil (Functioncode 0x05) 
        /// </summary>
        /// <param name="outputAddress">0x0000 (0) to 0xffff (65535)</param>
        public async Task WriteSingleCoilAsync(int outputAddress, bool value, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);

            ushort forcevalue = (ushort)(value ? 0xFF00 : 0x0000);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)outputAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), forcevalue);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteSingleCoil,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (!requestPdu.Data.SequenceEqual(requestPdu.Data))
                throw new Exception("Received invalid payload");

            return;
        }
        /// <summary>
        /// Write Single Register (Functioncode 0x06) 
        /// </summary>
        /// <param name="outputAddress">0x0000 (0) to 0xffff (65535)</param>
        public async Task WriteSingleRegisterAsync(int outputAddress, short value, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);

            var requestBuffer = new byte[4];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)outputAddress);
            BinaryPrimitives.WriteInt16BigEndian(requestBuffer.AsSpan(2, 2), value);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteSingleRegister,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (!requestPdu.Data.SequenceEqual(requestPdu.Data))
                throw new Exception("Received invalid payload");

            return;
        }
        /// <summary>
        /// Write Multiple Coils (Functioncode 0x0F) 
        /// </summary>
        /// <param name="outputAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="values">Length of 0x0001 (1) to 0x07b0 (1968)</param>
        public async Task WriteMultipleCoilsAsync(int outputAddress, bool[] values, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);
            ArgumentNullException.ThrowIfNull(values);
            ArgumentOutOfRangeException.ThrowIfLessThan(values.Length, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(values.Length, 0x07b0);

            var bytes = values.ReadBytes().ToArray();

            var requestBuffer = new byte[5 + bytes.Length];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)outputAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)values.Length);
            requestBuffer[4] = (byte)bytes.Length;
            Buffer.BlockCopy(bytes, 0, requestBuffer, 5, bytes.Length);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteMultipleCoils,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (!requestPdu.Data.Take(4).SequenceEqual(requestPdu.Data.Take(4)))
                throw new Exception("Received invalid payload");

            return;
        }
        /// <summary>
        /// Write Multiple Registers (Functioncode 0x10) 
        /// </summary>
        /// <param name="outputAddress">0x0000 (0) to 0xffff (65535)</param>
        /// <param name="values">Length of 0x0001 (1) to 0x007b (123)</param>
        public async Task WriteMultipleRegistersAsync(int outputAddress, short[] values, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);
            ArgumentNullException.ThrowIfNull(values);
            ArgumentOutOfRangeException.ThrowIfLessThan(values.Length, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(values.Length, 0x007b);

            var bytes = values.ReadBytes().ToArray();

            var requestBuffer = new byte[5 + bytes.Length];

            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(0, 2), (ushort)outputAddress);
            BinaryPrimitives.WriteUInt16BigEndian(requestBuffer.AsSpan(2, 2), (ushort)values.Length);
            requestBuffer[4] = (byte)bytes.Length;
            Buffer.BlockCopy(bytes, 0, requestBuffer, 5, bytes.Length);

            var requestPdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteMultipleRegisters,
                Data = requestBuffer
            };
            var responsePdu = await SendReceiveAsync(requestPdu, token);

            if (!requestPdu.Data.Take(4).SequenceEqual(requestPdu.Data.Take(4)))
                throw new Exception("Received invalid payload");

            return;
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _receivedPDUs.Clear();
                }
                _protocol.Dispose();
                _disposedValue = true;
            }
        }
        ~ModbusClient()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
