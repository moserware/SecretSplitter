using System;
using System.Numerics;
using System.Text;

namespace Moserware.Numerics {
    public static class BigIntegerExtensions {
        private static BigInteger GetBit(int bit) {
            return BigInteger.One << bit;
        }

        public static BigInteger SetBit(this BigInteger n, int bit) {
            n = n | GetBit(bit);
            return n;
        }

        public static bool TestBit(this BigInteger n, int bit) {
            return (n & GetBit(bit)) != 0;
        }

        public static int GetBitLength(this BigInteger n) {
            var remainder = n;
            int bits = 0;
            while (remainder > 0) {
                remainder = remainder >> 1;
                bits++;
            }

            return bits;
        }

        public static string ToPolynomialString(this BigInteger n) {
            var sb = new StringBuilder();
            for (int i = n.GetBitLength(); i >= 0; i--) {
                if (n.TestBit(i)) {
                    if (sb.Length > 0) {
                        sb.Append(" + ");
                    }

                    sb.Append((i > 0) ? "x" : "1");

                    if (i > 1) {
                        sb.Append("^");
                        sb.Append(i);
                    }
                }
            }

            if (sb.Length == 0) {
                sb.Append("0");
            }

            return sb.ToString();
        }

        public static byte[] ToUnsignedLittleEndianBytes(this BigInteger n) {
            var byteArray = n.ToByteArray();
            if ((byteArray.Length > 1) && (byteArray[byteArray.Length - 1] == 0x00)) {
                var byteArrayMissingEnd = new byte[byteArray.Length - 1];
                Array.Copy(byteArray, byteArrayMissingEnd, byteArrayMissingEnd.Length);
                return byteArrayMissingEnd;
            }
            return byteArray;
        }

        public static byte[] ToUnsignedBigEndianBytes(this BigInteger n) {
            var bytes = n.ToUnsignedLittleEndianBytes();
            Array.Reverse(bytes);
            return bytes;
        }

        public static BigInteger ToBigIntegerFromLittleEndianUnsignedBytes(this byte[] bytes) {
            // always assume unsigned (positive) by appending a 0 high order byte
            return new BigInteger(bytes.ConcatZeroByte());
        }

        public static BigInteger ToBigIntegerFromBigEndianUnsignedBytes(this byte[] bytes) {
            var littleEndianBytes = bytes.Reverse();
            return littleEndianBytes.ToBigIntegerFromLittleEndianUnsignedBytes();
        }
    }
}