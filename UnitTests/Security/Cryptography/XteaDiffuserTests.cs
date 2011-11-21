using System.Numerics;
using System.Text;
using Moserware.Numerics;
using Moserware.Security.Cryptography;
using NUnit.Framework;

namespace UnitTests.Security.Cryptography {
    [TestFixture]
    public class XteaDiffuserTests {
        [Test]
        public void TestDiffuser() {
            const string helloWorld = "Hello World!";
            var helloWorldBytes = Encoding.UTF8.GetBytes(helloWorld);
            BigInteger b = helloWorldBytes.ToBigIntegerFromBigEndianUnsignedBytes();
            Diffuser d = new XteaDiffuser();
            var scrambled = d.Scramble(b, helloWorldBytes.Length);
            var unscrambed = d.Unscramble(scrambled, helloWorldBytes.Length);
            var unscrambledBytes = unscrambed.ToUnsignedBigEndianBytes();
            string recovered = Encoding.UTF8.GetString(unscrambledBytes);
            Assert.AreEqual(helloWorld, recovered);
        }
    }
}