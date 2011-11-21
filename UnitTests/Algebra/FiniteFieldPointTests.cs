using System.Globalization;
using System.Numerics;
using Moserware.Algebra;
using NUnit.Framework;

namespace UnitTests.Algebra {
    [TestFixture]
    public class FiniteFieldPointTests {
        [Test]
        public void ParseTests() {
            const string toParse = "2-ef3305516d35b906812e181cb72e42967b1bc3c1791363";
            var p = FiniteFieldPoint.Parse(toParse);
            Assert.AreEqual(new BigInteger(2), p.X.PolynomialValue);
            var expectedY = BigInteger.Parse("0ef3305516d35b906812e181cb72e42967b1bc3c1791363", NumberStyles.HexNumber);
            Assert.AreEqual(expectedY, p.Y.PolynomialValue);
        }
    }
}