using Abaddax.Modbus.Protocol.Contracts;

namespace Abaddax.Modbus.Protocol.Builder
{
    public abstract class ModbusClientBuilder<TModbusProtocol> : ModbusClientOptions where TModbusProtocol : IModbusProtocol
    {
        public abstract ModbusClient<TModbusProtocol> Build();
    }
}
