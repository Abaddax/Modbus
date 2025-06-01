namespace Abaddax.Modbus.Protocol.Protocol
{
    public enum ModbusFunctionCode
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10,
        //ReadDeviceIdentification = 0x2B,
        //ReadFileRecord = 0x14,

        /// <summary>
        /// Function code + 0x80
        /// </summary>
        Exception = 0x80
    }

}
