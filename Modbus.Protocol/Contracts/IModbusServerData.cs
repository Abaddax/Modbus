namespace Modbus.Protocol.Contracts
{
    public interface IModbusServerData
    {
        Task<bool> RetrieveCoilAsync(ushort address, CancellationToken token = default);
        Task StoreCoilAsync(ushort address, bool value, CancellationToken token = default);

        Task<bool> RetreiveDiscreteInputAsync(ushort address, CancellationToken token = default);

        Task<short> RetreiveHoldingRegister(ushort address, CancellationToken token = default);
        Task StoreHoldingRegister(ushort address, short value, CancellationToken token = default);

        Task<short> RetreiveInputRegister(ushort address, CancellationToken token = default);
    }
}
