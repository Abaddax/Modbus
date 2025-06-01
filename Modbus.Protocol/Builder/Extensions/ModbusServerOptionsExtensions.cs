using Abaddax.Modbus.Protocol.Contracts;

namespace Abaddax.Modbus.Protocol.Builder.Extensions
{
    public static class ModbusServerOptionsExtensions
    {
        public static TModbusServerOptions WithUnitIdentifier<TModbusServerOptions>(this TModbusServerOptions options, byte unitIdentifier)
            where TModbusServerOptions : ModbusServerOptions
        {
            ArgumentNullException.ThrowIfNull(options);

            ArgumentOutOfRangeException.ThrowIfLessThan(unitIdentifier, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(unitIdentifier, 255);

            options.UnitIdentifier = unitIdentifier;

            return options;
        }

        public static TModbusServerOptions WithMaxServerConnections<TModbusServerOptions>(this TModbusServerOptions options, int maxServerConnections)
            where TModbusServerOptions : ModbusServerOptions
        {
            ArgumentNullException.ThrowIfNull(options);

            options.MaxServerConnections = maxServerConnections;

            return options;
        }

        public static TModbusServerOptions WithServerData<TModbusServerOptions>(this TModbusServerOptions options, IModbusServerData serverData)
            where TModbusServerOptions : ModbusServerOptions
        {
            ArgumentNullException.ThrowIfNull(options);

            ArgumentNullException.ThrowIfNull(serverData);

            options.ServerData = serverData;

            return options;
        }
    }
}
