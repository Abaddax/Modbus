namespace Modbus.Protocol.Builder
{
    public abstract class ModbusClientOptions
    {
        public byte UnitIdentifier { get; set; } = 1;
    }
}
