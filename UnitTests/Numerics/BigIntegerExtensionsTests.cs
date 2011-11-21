using System.Numerics;
using Moserware.Numerics;
using NUnit.Framework;

namespace UnitTests.Numerics {
    [TestFixture]
    public class BigIntegerExtensionsTests {
        [Test]
        public void ToUnsignedBytesLeastSignificantByteFirstTest() {
            var zeroBytes = BigInteger.Zero.ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[1], zeroBytes);

            var firstByteCutoff = new BigInteger(127).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x7F }, firstByteCutoff);

            var n128 = new BigInteger(128).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x80 }, n128);

            var n130 = new BigInteger(130).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x82 }, n130);

            var n255 = new BigInteger(255).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0xFF }, n255);

            var n256 = new BigInteger(256).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x01 }, n256);

            var n257 = new BigInteger(257).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x01 }, n257);

            var n258 = new BigInteger(258).ToUnsignedLittleEndianBytes();
            CollectionAssert.AreEqual(new byte[] { 0x02, 0x01 }, n258);
        }
    }
}
