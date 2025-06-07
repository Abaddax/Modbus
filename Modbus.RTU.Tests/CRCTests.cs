using Abaddax.Modbus.RTU.Internal;

namespace Abaddax.Modbus.RTU.Tests
{
    public class CRCTests
    {
        [Test]
        //b
        [TestCase("04-01-00-0A-00-0D", 0xDD98)]
        [TestCase("04-01-02-0A-11", 0xB350)]
        [TestCase("04-02-00-0A-00-0D", 0x9998)]
        [TestCase("04-02-02-0A-11", 0xB314)]
        [TestCase("01-03-00-00-00-02", 0xC40B)]
        [TestCase("01-03-04-00-06-00-05", 0xDA31)]
        [TestCase("01-04-00-00-00-02", 0x71CB)]
        [TestCase("01-04-04-00-06-00-05", 0xDB86)]
        [TestCase("11-05-00-AC-FF-00", 0x4E8B)]
        [TestCase("11-06-00-01-00-03", 0x9A9B)]
        [TestCase("11-0F-00-13-00-0A-02-CD-01", 0xBF0B)]
        [TestCase("11-10-00-01-00-02-04-00-0A-01-02", 0xC6F0)]
        public void ShouldCalculateCRC16(string inputHex, int expected)
        {
            byte[] input = Convert.FromHexString(inputHex.Replace("-", ""));
            var crc = CRCHelper.CalculateCRC16(input);

            Assert.That(crc, Is.EqualTo((ushort)expected));
        }
    }
}
