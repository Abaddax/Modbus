using Abaddax.Modbus.Protocol.Protocol;

namespace Abaddax.Modbus.Protocol.Contracts
{
    public interface IModbusProtocol : IDisposable
    {
        bool Connected { get; }

        event EventHandler<ModbusPDU> OnModbusPDUReceived;

        public Task ConnectAsync(CancellationToken token);

        public Task SendModbusPDUAsync(ModbusPDU pdu, CancellationToken token);
    }
}
