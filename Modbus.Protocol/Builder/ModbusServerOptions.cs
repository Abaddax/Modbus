using Abaddax.Modbus.Protocol.Contracts;

namespace Abaddax.Modbus.Protocol.Builder
{
    public abstract class ModbusServerOptions
    {
        public byte UnitIdentifier { get; set; } = 1;
        public int MaxServerConnections { get; set; } = -1;
        public IModbusServerData? ServerData { get; set; }
    }
}
