using Abaddax.Modbus.Protocol.Contracts;
using Abaddax.Modbus.Protocol.Extensions;
using Abaddax.Modbus.Protocol.Protocol;
using Abaddax.Utilities.Threading.Tasks;
using System.Buffers.Binary;

namespace Abaddax.Modbus.Protocol
{
    public sealed class ModbusServer<TModbusProtocol> : IDisposable where TModbusProtocol : IModbusProtocol
    {
        private readonly TModbusProtocol _protocol;
        private readonly IModbusServerData _data;

        private bool _disposedValue = false;

        public bool Running => _protocol.Connected;

        public ModbusServer(TModbusProtocol protocol, IModbusServerData data)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _data = data ?? throw new ArgumentNullException(nameof(protocol));

            _protocol.OnModbusPDUReceived += OnModbusPDUReceived;
        }

        public async Task StartAsync(CancellationToken token = default)
        {
            if (!_protocol.Connected)
                await _protocol.ConnectAsync(token);
        }

        private void OnModbusPDUReceived(object? sender, ModbusPDU pdu)
        {
            if (_disposedValue)
                return;
            //Run in background
            _ = Task.Run(async () =>
            {
                using (CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1000)))
                {
                    await OnModbusPDUReceivedAsync(pdu, tokenSource.Token);
                }
            });
        }
        private async Task OnModbusPDUReceivedAsync(ModbusPDU request, CancellationToken token)
        {
            ModbusPDU response;
            try
            {
                response = request.FunctionCode switch
                {
                    ModbusFunctionCode.ReadCoils => await HandleReadCoilsAsync(request, token),
                    ModbusFunctionCode.ReadDiscreteInputs => await HandleReadDiscreteInputsAsync(request, token),
                    ModbusFunctionCode.ReadHoldingRegisters => await HandleReadHoldingRegistersAsync(request, token),
                    ModbusFunctionCode.ReadInputRegisters => await HandleReadInputRegistersAsync(request, token),
                    ModbusFunctionCode.WriteSingleCoil => await HandleWriteSingleCoilAsync(request, token),
                    ModbusFunctionCode.WriteSingleRegister => await HandleWriteSingleRegisterAsync(request, token),
                    ModbusFunctionCode.WriteMultipleCoils => await HandleWriteMultipleCoilsAsync(request, token),
                    ModbusFunctionCode.WriteMultipleRegisters => await HandleWriteMultipleRegisters(request, token),
                    _ => new ModbusPDU()
                    {
                        FunctionCode = request.FunctionCode | ModbusFunctionCode.Exception,
                        Data = [(byte)ModbusExceptionCode.IllegalFunction],
                    }
                };
            }
            catch (Exception ex)
            {
                response = new ModbusPDU()
                {
                    FunctionCode = request.FunctionCode | ModbusFunctionCode.Exception,
                    Data = [(byte)ModbusExceptionCode.DeviceFailure],
                };
            }
            response.ID = request.ID;
            await _protocol.SendModbusPDUAsync(response, token);
        }

        #region Read Functions
        /// <summary>
        /// Read Coils (Functioncode 0x01) 
        /// </summary>
        private async Task<ModbusPDU> HandleReadCoilsAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var startingAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var quantity = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x07d0);

            var response = await Enumerable.Range(0, quantity)
                .Select(i => _data.RetrieveCoilAsync((ushort)(startingAddress + i), token))
                .AwaitAll(token)
                .ToEnumerableAsync(token);
            var responseBuffer = response
                .ReadBytes()
                .Prepend((byte)0)
                .ToArray();
            //Set byte count
            responseBuffer[0] = (byte)(responseBuffer.Length - 1);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadCoils,
                Data = responseBuffer,
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Read Discrete Inputs (Functioncode 0x02) 
        /// </summary>
        private async Task<ModbusPDU> HandleReadDiscreteInputsAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var startingAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var quantity = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x07d0);

            var response = await Enumerable.Range(0, quantity)
                .Select(i => _data.RetreiveDiscreteInputAsync((ushort)(startingAddress + i), token))
                .AwaitAll(token)
                .ToEnumerableAsync(token);
            var responseBuffer = response
                .ReadBytes()
                .Prepend((byte)0)
                .ToArray();
            //Set byte count
            responseBuffer[0] = (byte)(responseBuffer.Length - 1);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadDiscreteInputs,
                Data = responseBuffer,
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Read Holding Registers (Functioncode 0x03) 
        /// </summary>
        private async Task<ModbusPDU> HandleReadHoldingRegistersAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var startingAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var quantity = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x007d);

            var response = await Enumerable.Range(0, quantity)
                .Select(i => _data.RetreiveHoldingRegister((ushort)(startingAddress + i), token))
                .AwaitAll(token)
                .ToEnumerableAsync(token);
            var responseBuffer = response
                .ReadBytes()
                .Prepend((byte)0)
                .ToArray();
            //Set byte count
            responseBuffer[0] = (byte)(responseBuffer.Length - 1);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadHoldingRegisters,
                Data = responseBuffer,
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Read Input Registers (Functioncode 0x04) 
        /// </summary>
        private async Task<ModbusPDU> HandleReadInputRegistersAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var startingAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var quantity = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(startingAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 0x007d);

            var response = await Enumerable.Range(0, quantity)
                .Select(i => _data.RetreiveInputRegister((ushort)(startingAddress + i), token))
                .AwaitAll(token)
                .ToEnumerableAsync(token);
            var responseBuffer = response
                .ReadBytes()
                .Prepend((byte)0)
                .ToArray();
            //Set byte count
            responseBuffer[0] = (byte)(responseBuffer.Length - 1);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.ReadInputRegisters,
                Data = responseBuffer,
                ID = request.ID,
            };

            return responsePdu;
        }
        #endregion

        #region Write Functions
        /// <summary>
        /// Write Single Coil (Functioncode 0x05) 
        /// </summary>
        private async Task<ModbusPDU> HandleWriteSingleCoilAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var outputAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var outputValue = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);

            var value = outputValue switch
            {
                0xFF00 => true,
                0x0000 => false,
                _ => throw new ArgumentOutOfRangeException(nameof(outputValue)),
            };

            await _data.StoreCoilAsync(outputAddress, value, token);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteSingleCoil,
                Data = request.Data,
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Write Single Register (Functioncode 0x06) 
        /// </summary>
        private async Task<ModbusPDU> HandleWriteSingleRegisterAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfNotEqual(request.Data.Length, 4);

            var outputAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var value = BinaryPrimitives.ReadInt16BigEndian(request.Data.AsSpan(2, 2));

            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);

            await _data.StoreHoldingRegister(outputAddress, value, token);

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteSingleRegister,
                Data = request.Data,
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Write Multiple Coils (Functioncode 0x0F) 
        /// </summary>
        private async Task<ModbusPDU> HandleWriteMultipleCoilsAsync(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.Data.Length, 4);

            var outputAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var outputCount = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));
            var byteCount = request.Data[4];

            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress + outputCount, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputCount, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputCount, 0x07b0);
            ArgumentOutOfRangeException.ThrowIfNotEqual(byteCount, outputCount / 8 + (outputCount % 8 == 0 ? 0 : 1));

            var coils = request.Data
                .Skip(5)
                .Take(byteCount)
                .ReadAsBool()
                .Take(outputCount);

            foreach (var coil in coils.Index())
            {
                await _data.StoreCoilAsync((ushort)(outputAddress + coil.Index), coil.Item, token);
            }

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteMultipleCoils,
                Data = request.Data[0..4],
                ID = request.ID,
            };

            return responsePdu;
        }
        /// <summary>
        /// Write Multiple Registers (Functioncode 0x10) 
        /// </summary>
        private async Task<ModbusPDU> HandleWriteMultipleRegisters(ModbusPDU request, CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            ArgumentNullException.ThrowIfNull(request.Data);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.Data.Length, 4);

            var outputAddress = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(0, 2));
            var outputCount = BinaryPrimitives.ReadUInt16BigEndian(request.Data.AsSpan(2, 2));
            var byteCount = request.Data[4];

            ArgumentOutOfRangeException.ThrowIfLessThan(outputAddress, 0x0000);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputAddress, 0xffff);
            ArgumentOutOfRangeException.ThrowIfLessThan(outputCount, 0x0001);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(outputCount, 0x007b);
            ArgumentOutOfRangeException.ThrowIfNotEqual(byteCount, outputCount * 2);

            var registers = request.Data
                .Skip(5)
                .Take(byteCount)
                .ReadAsShort()
                .Take(outputCount);

            foreach (var register in registers.Index())
            {
                await _data.StoreHoldingRegister((ushort)(outputAddress + register.Index), register.Item, token);
            }

            var responsePdu = new ModbusPDU()
            {
                FunctionCode = ModbusFunctionCode.WriteMultipleRegisters,
                Data = request.Data[0..4],
                ID = request.ID,
            };

            return responsePdu;
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _protocol.OnModbusPDUReceived -= OnModbusPDUReceived;
                }
                _protocol.Dispose();
                _disposedValue = true;
            }
        }
        ~ModbusServer()
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
