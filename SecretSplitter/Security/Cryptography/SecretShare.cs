using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Moserware.Algebra;

namespace Moserware.Security.Cryptography {
    /// <summary>
    /// Stores the secret shares/pieces given to others.
    /// </summary>
    public class SecretShare {
        internal const string RegexPattern = @"((?<checksum>([0-9a-fA-F]{2}))(?<shareType>[0-9a-fA-F])-)?" + FiniteFieldPoint.RegexPattern;
        private static readonly Regex _Parser = new Regex(RegexPattern);

        public SecretShare(SecretShareType shareType, FiniteFieldPoint point, string checksum = null) {
            ShareType = shareType;
            Point = point;
            Checksum = checksum;
        }
        
        public SecretShareType ShareType { get; private set; }

        // SECREVIEW: Store the point in ProtectedMemory?
        public FiniteFieldPoint Point { get; private set; }
        public string Checksum { get; private set; }

        public bool HasValidChecksum {
            get { 
                var expectedChecksum = ToString().Substring(0, SimpleChecksum.ChecksumSizeInNibbles);
                return String.Equals(expectedChecksum, Checksum, StringComparison.OrdinalIgnoreCase);
            }
        }

        internal string ParsedValue { get; private set; }

        public override string ToString() {
            return ToString(SecretShareFormattingOptions.IncludeChecksum);
        }

        public string ToString(SecretShareFormattingOptions options) {
            var result = Point.ToString();

            if((options & SecretShareFormattingOptions.IncludeChecksum) == SecretShareFormattingOptions.IncludeChecksum) {
                var prefix = ((int) ShareType).ToString("x") + "-";
                result = prefix + result;
                var checksum = SimpleChecksum.ComputeChecksum(result);
                return checksum + result;
            }

            return result;
        }
        
        public static bool TryParse(string s, out SecretShare share) {
            var match = _Parser.Match(s);

            FiniteFieldPoint point;

            if(!FiniteFieldPoint.TryParse(match, out point)) {
                share = null;
                return false;
            }

            if(!match.Success) {
                share = null;
                return false;
            }

            var rawShareType = match.Groups["shareType"].Value;
            var shareType = String.IsNullOrWhiteSpace(rawShareType) ? "0" : rawShareType;

            int shareTypeVal;

            if(!Int32.TryParse(shareType, NumberStyles.Integer, CultureInfo.InvariantCulture, out shareTypeVal)) {
                share = null;
                return false;
            }

            var checksum = match.Groups["checksum"].Value ?? "";
            share = new SecretShare((SecretShareType) shareTypeVal, point, checksum);
            share.ParsedValue = s;
            return true;
        }

        public static SecretShare Parse(string s) {
            SecretShare share;
            if(TryParse(s, out share)) {
                return share;
            }
            
            throw new InvalidChecksumShareException(s);
        }
    }

    public enum SecretShareType {
        Unknown = 0,
        Message = 1,
        File = 2
    }

    [Flags]
    public enum SecretShareFormattingOptions {
        None = 0,
        IncludeChecksum = 1
    }

    internal static class SimpleChecksum {
        public const int ChecksumSizeInBytes = 1;
        public const int ChecksumSizeInNibbles = ChecksumSizeInBytes * 2;

        public static readonly string Pattern = @"[0-9a-fA-f]{" + ChecksumSizeInNibbles + "}";
        public static string ComputeChecksum(string toHash) {
            return ComputeChecksum(Encoding.UTF8.GetBytes(toHash));
        }

        // In case you're wondering, I use SHA-1 here instead of something like SHA-256 because it's just a simple checksum
        // where only the first byte is used. SHA-1 utilities are much more available than SHA-256 ones.
        public static string ComputeChecksum(byte[] bytes) {
            return String.Join("", new SHA1Managed().ComputeHash(bytes).Take(ChecksumSizeInBytes).Select(b => b.ToString("x2")));
        }
    }
}