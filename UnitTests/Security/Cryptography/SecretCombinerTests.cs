using Moserware.Security.Cryptography;
using NUnit.Framework;

namespace UnitTests.Security.Cryptography {
    [TestFixture]
    public class SecretCombinerTests {
        [Test]
        public void TestCombine2Of3() {
            const string expected = "Hello World!";
            string split1 = "1-f844243438cd26e13e29341a";
            string split2 = "2-57440c4943e5fd3ccbe0d1c8";
            string split3 = "3-cdbbeb9d95024b88675870bb";

            string combined12 = SecretCombiner.Combine(new[] {split1, split2}).RecoveredTextString;
            Assert.AreEqual(expected, combined12);

            string combined23 = SecretCombiner.Combine(new[] {split2, split3}).RecoveredTextString;
            Assert.AreEqual(expected, combined23);
        }

        [Test]
        public void TestCombine3Of5() {
            const string expected = "This is a unit test of (3,5)";

            string split1 = "1-924f9ed00492cb59df69fec1a5e20714364e00fefceb623329bffecd";
            string split2 = "2-7d39255e93654f970e23b020e7a408d89729ea9dac9f9334008b3656";
            string split3 = "3-a7ceeed93a5f4f6ca6fc4174966ee0aa566a04e2aa92f8df555a84bc";
            string split4 = "4-2201295c62d5d4c024340488f355fd75bdca85303f9808ebd8bccc3d";
            string split5 = "5-f8f6e2dbcbefd43b8cebf5dc829f15077c896b4f399563008d6d7ec5";

            string combined123 = SecretCombiner.Combine(new[] {split1, split2, split3}).RecoveredTextString;
            Assert.AreEqual(expected, combined123);

            string combined234 = SecretCombiner.Combine(new[] {split2, split3, split4}).RecoveredTextString;
            Assert.AreEqual(expected, combined234);

            string combined345 = SecretCombiner.Combine(new[] {split3, split4, split5}).RecoveredTextString;
            Assert.AreEqual(expected, combined345);
        }

        [Test] 
        public void DiffuserTests() {
            // Ensure the 64 bit cutoff works

            // this is before the cutoff - 7 bytes
            string combinedHello12 = SecretCombiner.Combine(new[] { "1-c2dd042a1ab153", "2-3d15bce0843163" }).RecoveredTextString;
            Assert.AreEqual("hello12", combinedHello12);

            // this is the cutoff - 8 bytes
            string combinedHello123 = SecretCombiner.Combine(new[] { "1-11feca4d8f9a7665", "2-f95cfd12cbce92fd" }).RecoveredTextString;
            Assert.AreEqual("hello123", combinedHello123);

            // this is after the cutoff - 9 bytes
            string combinedHello1234 = SecretCombiner.Combine(new[] { "1-4676885abc0af6104e", "2-a3b1297294d04fc9c5" }).RecoveredTextString;
            Assert.AreEqual("hello1234", combinedHello1234);
        }
    }
}