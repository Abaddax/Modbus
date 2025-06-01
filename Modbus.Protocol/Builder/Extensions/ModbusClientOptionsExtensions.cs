namespace Abaddax.Modbus.Protocol.Builder.Extensions
{
    public static class ModbusClientOptionsExtensions
    {
        public static TModbusClientOptions WithUnitIdentifier<TModbusClientOptions>(this TModbusClientOptions options, byte unitIdentifier)
            where TModbusClientOptions : ModbusClientOptions
        {
            ArgumentNullException.ThrowIfNull(options);

            ArgumentOutOfRangeException.ThrowIfLessThan(unitIdentifier, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(unitIdentifier, 255);

            options.UnitIdentifier = unitIdentifier;

            return options;
        }
    }
}
