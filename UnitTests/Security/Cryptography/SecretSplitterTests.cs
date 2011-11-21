using System.Linq;
using Moserware.Security.Cryptography;
using NUnit.Framework;

namespace UnitTests.Security.Cryptography {
    [TestFixture]
    public class SecretSplitterTests {
        [Test]
        public void TestSplits() {
            const string helloWorld = "Hello World!";

            // Try it a bunch of times since it's random each time
            for (int i = 0; i < 10; i++) {
                var splits = SecretSplitter.SplitMessage(helloWorld, 2, 3);
                var recovered = SecretCombiner.Combine(splits.Take(2)).RecoveredTextString;
                Assert.AreEqual(helloWorld, recovered);
            }
        }
    }
}