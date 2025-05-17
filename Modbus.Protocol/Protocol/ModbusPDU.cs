namespace Modbus.Protocol.Protocol
{
    public record struct ModbusPDU
    {
        public Guid ID { get; set; }
        required public ModbusFunctionCode FunctionCode { get; set; }
        required public byte[] Data { get; set; }
    }
}
