using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Moserware.Security.Cryptography {
    public class SecretEncoder {
        public static byte[] EncodeString(string s) {
            return Encoding.UTF8.GetBytes(s);
        }
        
        public static string DecodeString(byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        public static bool TryParseHexString(string s, out byte[] encodedBytes) {
            var m = Regex.Match(s, "^([0-9a-fA-F]{2})+$");
            if (!m.Success) {
                encodedBytes = null;
                return false;
            }

            var byteStrings = m.Groups[1].Captures.OfType<Capture>().Select(g => g.Value);
            encodedBytes = byteStrings.Select(b => byte.Parse(b, NumberStyles.HexNumber)).ToArray();
            return true;
        }

        public static byte[] ParseHexString(string s) {
            byte[] result;
            if (!TryParseHexString(s, out result)) {
                throw new ArgumentException();
            }

            return result;
        }

        public static string ToHexString(byte[] bytes) {
            return String.Join("", bytes.Select(b => b.ToString("x2")));
        }
    }
}