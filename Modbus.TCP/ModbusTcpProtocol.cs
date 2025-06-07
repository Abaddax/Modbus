using Abaddax.Modbus.Protocol.Contracts;
using Abaddax.Modbus.Protocol.Protocol;
using Abaddax.Modbus.TCP.Internal;
using Abaddax.Utilities.Collections;
using Abaddax.Utilities.IO;
using System.Buffers.Binary;

namespace Abaddax.Modbus.TCP
{
    public sealed class ModbusTcpProtocol : IModbusProtocol
    {
        readonly Lazy<ListenStream<ModbusTcpListenProtocol>> _stream;

        readonly byte _unitIdentifier = 1;
        readonly DistinctDictionary<Guid, ushort> _transactionIDs = new();

        bool disposedValue;

        private void RemoveTransaction(Guid id)
        {
            lock (_transactionIDs)
            {
                _transactionIDs.Remove(id);
            }
        }
        private void GenerateTransaction(ushort? currentTransactionId, Guid? currentId, out ushort transactionId, out Guid id)
        {
            if (currentTransactionId == 0)
                currentTransactionId = null;
            if (currentId == Guid.Empty)
                currentId = null;
            int retries = 0;
            while (retries++ < 10)
            {
                transactionId = currentTransactionId ?? (ushort)Random.Shared.Next(0, ushort.MaxValue);
                id = currentId ?? Guid.NewGuid();
                lock (_transactionIDs)
                {
                    if (_transactionIDs.TryAdd(id, transactionId))
                        return;
                }
            }
            throw new Exception("Failed to generate transactionId");
        }

        private async Task OnModbusMessageReceived(Exception? readException, ReadOnlyMemory<byte> message, CancellationToken token)
        {
            if (readException != null)
            {
                //TODO
                _stream.Value.Close();
                return;
            }

            var span = message.Span;

            var transactionId = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(0, 2));
            var protocolId = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));
            var messageLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4, 2));
            if (protocolId != 0)
                throw new Exception("Invalid protocol identifier");
            if (span.Length != messageLength + 6 ||
                messageLength < 2)
                throw new Exception("Invalid message length");

            var unitIdentifier = span[6];

            //Not for this device
            if (unitIdentifier != _unitIdentifier)
                return;

            var function = (ModbusFunctionCode)span[7];

            var data = span.Slice(8);

            //Response
            if (_transactionIDs.TryGetKey(transactionId, out var id))
            {
                RemoveTransaction(id);

                var responsePdu = new ModbusPDU()
                {
                    ID = id,
                    FunctionCode = function,
                    Data = data.ToArray()
                };
                OnModbusPDUReceived?.Invoke(this, responsePdu);
            }
            //Request
            else
            {
                GenerateTransaction(transactionId, null, out transactionId, out id);

                var requestPdu = new ModbusPDU()
                {
                    ID = id,
                    FunctionCode = function,
                    Data = data.ToArray()
                };
                OnModbusPDUReceived?.Invoke(this, requestPdu);
            }
        }

        internal ModbusTcpProtocol(Func<Stream> connectFunc)
        {
            ArgumentNullException.ThrowIfNull(connectFunc);

            _stream = new(() =>
            {
                return new ListenStream<ModbusTcpListenProtocol>(connectFunc(), new ModbusTcpListenProtocol());
            });
        }

        public async Task ConnectAsync(CancellationToken token)
        {
            if (Connected)
                return;

            _stream.Value.StartListening(OnModbusMessageReceived);
        }

        public byte UnitIdentifier { get => _unitIdentifier; init => _unitIdentifier = value; }
        public bool Connected => _stream.IsValueCreated ? _stream.Value.Listening : false;

        public event EventHandler<ModbusPDU> OnModbusPDUReceived;

        public async Task SendModbusPDUAsync(ModbusPDU pdu, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(pdu.Data);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pdu.Data.Length, ushort.MaxValue - 2);

            if (!Connected)
                throw new InvalidOperationException("Not connected");

            bool isResponse = _transactionIDs.TryGetValue(pdu.ID, out var transactionId);
            try
            {
                Guid id = pdu.ID;
                if (!isResponse)
                {
                    GenerateTransaction(null, pdu.ID, out transactionId, out id);
                }

                Memory<byte> buffer = new byte[8 + pdu.Data.Length];
                var bufferSpan = buffer.Span;

                BinaryPrimitives.WriteUInt16BigEndian(bufferSpan.Slice(0, 2), transactionId);
                BinaryPrimitives.WriteUInt16BigEndian(bufferSpan.Slice(2, 2), 0);
                BinaryPrimitives.WriteUInt16BigEndian(bufferSpan.Slice(4, 2), (ushort)(2 + pdu.Data.Length));

                bufferSpan[6] = _unitIdentifier;

                bufferSpan[7] = (byte)pdu.FunctionCode;


                pdu.Data.CopyTo(buffer.Slice(8));

                await _stream.Value.WriteAsync(buffer, token);

                //Clear transaction
                if (isResponse)
                    RemoveTransaction(pdu.ID);
            }
            catch (Exception ex)
            {
                RemoveTransaction(pdu.ID);
                throw;
            }
        }

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_stream.IsValueCreated)
                        _stream.Value.StopListening();
                    _transactionIDs.Clear();
                }
                if (_stream.IsValueCreated)
                    _stream.Value.Dispose();
                disposedValue = true;
            }
        }
        ~ModbusTcpProtocol()
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
