using Modbus.Protocol.Contracts;

namespace Modbus.Protocol.Tests.Helper
{
    public class TestModbusServerData : IModbusServerData
    {
        Func<ushort, bool>? _retreiveDiscreteInput;
        Func<ushort, bool>? _retreiveCoil;
        Func<ushort, short>? _retreiveHoldingRegister;
        Func<ushort, short>? _retreiveInputRegister;
        Action<ushort, bool>? _storeCoil;
        Action<ushort, short>? _storeHoldingRegister;

        public TestModbusServerData(
            Func<ushort, bool>? retreiveDiscreteInput = null,
            Func<ushort, bool>? retreiveCoil = null,
            Func<ushort, short>? retreiveHoldingRegister = null,
            Func<ushort, short>? retreiveInputRegister = null,
            Action<ushort, bool>? storeCoil = null,
            Action<ushort, short>? storeHoldingRegister = null)
        {
            _retreiveDiscreteInput = retreiveDiscreteInput;
            _retreiveCoil = retreiveCoil;
            _retreiveHoldingRegister = retreiveHoldingRegister;
            _retreiveInputRegister = retreiveInputRegister;
            _storeCoil = storeCoil;
            _storeHoldingRegister = storeHoldingRegister;
        }

        public async Task<bool> RetrieveCoilAsync(ushort address, CancellationToken token = default)
        {
            var value = _retreiveCoil?.Invoke(address);
            return value ?? false;
        }
        public async Task<bool> RetreiveDiscreteInputAsync(ushort address, CancellationToken token = default)
        {
            var value = _retreiveDiscreteInput?.Invoke(address);
            return value ?? false;
        }
        public async Task<short> RetreiveHoldingRegister(ushort address, CancellationToken token = default)
        {
            var value = _retreiveHoldingRegister?.Invoke(address);
            return value ?? 0;
        }
        public async Task<short> RetreiveInputRegister(ushort address, CancellationToken token = default)
        {
            var value = _retreiveInputRegister?.Invoke(address);
            return value ?? 0;
        }

        public async Task StoreCoilAsync(ushort address, bool value, CancellationToken token = default)
        {
            _storeCoil?.Invoke(address, value);
        }
        public async Task StoreHoldingRegister(ushort address, short value, CancellationToken token = default)
        {
            _storeHoldingRegister?.Invoke(address, value);
        }
    }
}
