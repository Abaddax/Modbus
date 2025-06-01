using Abaddax.Modbus.Protocol.Protocol;

namespace Abaddax.Modbus.Protocol
{
    public class ModbusException : Exception
    {
        private readonly ModbusExceptionCode _exceptionCode;

        public ModbusExceptionCode ExceptionCode => _exceptionCode;

        public ModbusException(ModbusExceptionCode exceptionCode)
        {
            _exceptionCode = exceptionCode;
        }

        public override string Message => $"Receive Modbus-Exception-Code: {_exceptionCode}";
    }
}
