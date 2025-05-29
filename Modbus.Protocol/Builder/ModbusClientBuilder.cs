using Modbus.Protocol.Contracts;

namespace Modbus.Protocol.Builder
{
    public abstract class ModbusClientBuilder<TModbusProtocol> : ModbusClientOptions where TModbusProtocol : IModbusProtocol
    {
        public abstract ModbusClient<TModbusProtocol> Build();
    }
}
