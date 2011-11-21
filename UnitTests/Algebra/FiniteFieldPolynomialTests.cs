using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Moserware.Algebra;
using NUnit.Framework;

namespace UnitTests.Algebra {
    [TestFixture]
    public class FiniteFieldPolynomialTests {
        [Test]
        public void BasicTests() {
            var rijndaelPoly = new IrreduciblePolynomial(8);
            var a = new FiniteFieldPolynomial(rijndaelPoly, 6, 4, 1, 0);
            var b = new FiniteFieldPolynomial(rijndaelPoly, 7, 6, 3, 1);
            var product = a*b;
            // "a" and "b" are inverses, so their product is 1
            Assert.AreEqual(1, (int)product.PolynomialValue);

            var g = new FiniteFieldPolynomial(rijndaelPoly, BigInteger.Parse("0e5", NumberStyles.HexNumber));
            var p = new FiniteFieldPolynomial(rijndaelPoly, BigInteger.One);

            // g is a generator, so we should generate all values except 0
            var vals = new HashSet<BigInteger> {p.PolynomialValue};

            for (int i = 0; i < 255; i++) {
                p = p*g;
                vals.Add(p.PolynomialValue);
            }

            Assert.AreEqual(255, vals.Count);
            Assert.IsTrue(vals.Contains((p*g).PolynomialValue));
        }

        [Test]
        public void TestInverse() {
            var rijndaelPoly = new IrreduciblePolynomial(8);
            var a = new FiniteFieldPolynomial(rijndaelPoly, 6, 4, 1, 0);
            var expectedInverse = new FiniteFieldPolynomial(rijndaelPoly, 7, 6, 3, 1);

            var actualInverse = a.GetInverse();

            Assert.AreEqual(expectedInverse.ToString(), actualInverse.ToString());
            var productSanityCheck = a*actualInverse;
            Assert.IsTrue(productSanityCheck.One.Equals(productSanityCheck));
        }
    }
}