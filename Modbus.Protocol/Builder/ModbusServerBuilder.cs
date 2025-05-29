using Modbus.Protocol.Contracts;

namespace Modbus.Protocol.Builder
{
    public abstract class ModbusServerBuilder<TModbusProtocol> : ModbusServerOptions where TModbusProtocol : IModbusProtocol
    {
        public abstract ModbusServerHost<TModbusProtocol> Build();
    }
}
