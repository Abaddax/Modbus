namespace Abaddax.Modbus.Protocol.Protocol
{
    public enum ModbusExceptionCode
    {
        IllegalFunction = 0x01,
        IllegalData = 0x02,
        IllegalDataValue = 0x03,
        DeviceFailure = 0x04,
        Acknowledge = 0x05,
        DeviceBusy = 0x06,
        NegativeAcknowledge = 0x07,
        MemoryParityError = 0x08,
        GatewayProblem = 0x0A,
        GatewayException = 0x0B
    }
}
