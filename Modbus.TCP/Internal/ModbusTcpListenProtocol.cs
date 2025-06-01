using Abaddax.Utilities.IO;
using System.Buffers.Binary;

namespace Abaddax.Modbus.TCP.Internal
{
    internal sealed class ModbusTcpListenProtocol : IStreamProtocol
    {
        public int FixedHeaderSize => 6;

        public async Task<ReadOnlyMemory<byte>> GetPacketBytesAsync(ReadOnlyMemory<byte> header, Stream stream, CancellationToken token = default)
        {
            if (header.Length != 6)
                throw new Exception("Received invalid header");

            var span = header.Span;

            var transactionId = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(0, 2));
            var protocolId = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));
            var messageLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4, 2));

            if (protocolId != 0)
                throw new Exception("Invalid protocol identifier");

            Memory<byte> message = new byte[6 + messageLength];
            header.CopyTo(message.Slice(0, 6));

            //Read remaining bytes
            await stream.ReadExactlyAsync(message.Slice(6, messageLength), token);

            return message;
        }
    }
}
