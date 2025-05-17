using Modbus.Protocol.Contracts;
using Modbus.Protocol.Protocol;

namespace Modbus.Protocol.Tests.Helper
{
    public class TestModbusProtocol : IModbusProtocol
    {
        public event Func<ModbusPDU, Task> OnModbusPDUSend;
        public async Task SendModbusPDUAsync(ModbusPDU pdu, CancellationToken token)
        {
            await (OnModbusPDUSend?.Invoke(pdu) ?? Task.CompletedTask);
        }
        public async Task ReceiveModbusPDUAsync(ModbusPDU pdu)
        {
            OnModbusPDUReceived?.Invoke(this, pdu);
        }

        #region IModbusProtocol
        public bool Connected => true;

        public event EventHandler<ModbusPDU> OnModbusPDUReceived;

        public async Task ConnectAsync(CancellationToken token)
        {
            //Nothing to connect
        }
        public void Dispose()
        {
            //Nothing to dispose
        }
        #endregion
    }
}
