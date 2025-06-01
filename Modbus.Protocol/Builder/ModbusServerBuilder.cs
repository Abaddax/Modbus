using Abaddax.Modbus.Protocol.Contracts;

namespace Abaddax.Modbus.Protocol.Builder
{
    public abstract class ModbusServerBuilder<TModbusProtocol> : ModbusServerOptions where TModbusProtocol : IModbusProtocol
    {
        public abstract ModbusServerHost<TModbusProtocol> Build();
    }
}
