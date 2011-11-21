using System.Linq;
using System.Text;

namespace Moserware {
    internal static class ByteExtensions {
        public static string ToHexString(this byte[] bytes) {
            var sb = new StringBuilder();
            foreach (var currentByte in bytes) {
                sb.Append(currentByte.ToString("x2"));
            }

            return sb.ToString();
        }

        public static byte[] ConcatZeroByte(this byte[] bytes) {
            return bytes.Concat(new byte[] {0x00}).ToArray();
        }

        public static byte[] Reverse(this byte[] bytes) {
            var reversed = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++) {
                reversed[(bytes.Length - i) - 1] = bytes[i];
            }

            return reversed;
        }
    }
}
