using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using Moserware.Algebra;
using Moserware.Numerics;
using Moserware.Security.Cryptography;

namespace Ssss {
    // NOTE: I (Jeff) went out of my way to make this program very similar to B. Poettering's 
    // "ssss-split" and "ssss-combine" programs (down to the command line options). 
    // This was simply to demonstrate that my code produced similar results. I received
    // permission from B. Poettering to release my compatible derivation under the MIT license 
    // (see my License.txt) included with this code for more details.
    internal class Program {
        private const string Version = "0.51 (.net)";

        private static void Main(string[] args) {
            Arguments.ParseArguments(args);

            if (GetProgramNameWithoutExtension().EndsWith("-split", StringComparison.OrdinalIgnoreCase)) {
                if (Arguments.ShowHelp || Arguments.ShowVersion) {
                    Console.WriteLine("Split secrets using Shamir's Secret Sharing Scheme.");
                    Console.WriteLine();
                    Console.WriteLine("ssss-split -t threshold -n shares [-w token] [-s level] [-x] [-q] [-Q] [-D] [-v]");
                }

                ShowVersionAndQuitIfAsked();

                if (Arguments.Threshold < 2) {
                    Fatal("invalid parameters: invalid threshold value");
                }

                if (Arguments.Shares < Arguments.Threshold) {
                    Fatal("invalid parameters: number of shares smaller than threshold");
                }

                if ((Arguments.SecurityLevel > 0) && !IrreduciblePolynomial.IsValidDegree(Arguments.SecurityLevel)) {
                    Fatal("invalid parameters: invalid security level");
                }

                int maxTokenLength = IrreduciblePolynomial.MaxDegree/8;

                if (!String.IsNullOrWhiteSpace(Arguments.Token) && (Arguments.Token.Length > maxTokenLength)) {
                    Fatal("invalid parameters: token too long");
                }

                Split();
            }
            else {
                if (Arguments.ShowHelp || Arguments.ShowVersion) {
                    Console.WriteLine("Combine shares using Shamir's Secret Sharing Scheme.");
                    Console.WriteLine();
                    Console.WriteLine("ssss-combine -t threshold [-x] [-q] [-Q] [-D] [-v]");
                }

                ShowVersionAndQuitIfAsked();

                if (Arguments.Threshold < 2) {
                    Fatal("invalid parameters: invalid threshold value");
                }

                Combine();
            }
        }

        private static void ShowVersionAndQuitIfAsked() {
            if (Arguments.ShowVersion) {
                Console.WriteLine(Version);
                Environment.Exit(0);
            }
        }

        private static void Split() {
            int degree = Arguments.HasSpecifiedSecurityLevel
                             ? Arguments.SecurityLevel
                             : IrreduciblePolynomial.MaxDegree;

            if (!Arguments.QuietMode) {
                Console.Write("Generating shares using a ({0},{1}) scheme with ", Arguments.Threshold, Arguments.Shares);

                if (Arguments.HasSpecifiedSecurityLevel) {
                    Console.Write("a {0} bit", Arguments.SecurityLevel);
                }
                else {
                    Console.Write("dynamic");
                }

                Console.WriteLine(" security level.");

                Console.Error.Write("Enter the secret, ");

                if (Arguments.HexMode) {
                    Console.Error.Write("at most {0} hex digits: ", degree/4);
                }
                else {
                    Console.Error.Write("at most {0} ASCII characters: ", degree/8);
                }
            }

            var secret = String.Empty;

            try {
                secret = ReadLineHidden();
            }
            catch {
                Fatal("I/O error while reading secret");
            }

            int securitySize = Arguments.SecurityLevel;

            if (!Arguments.HasSpecifiedSecurityLevel) {
                securitySize = Arguments.HexMode
                                   ? 4*(((secret.Length + 1) & ~1))
                                   : 8*secret.Length;

                if (!IrreduciblePolynomial.IsValidDegree(securitySize)) {
                    Fatal("security level invalid (secret too long?)");
                }

                if (!Arguments.QuietMode) {
                    Console.Error.WriteLine("Using a {0} bit security level.", securitySize);
                }
            }

            Diffuser diffuser = new NullDiffuser();

            if (Arguments.EnableDiffusion) {
                if (securitySize >= 64) {
                    diffuser = new XteaDiffuser();
                }
                else {
                    Warning("security level too small for the diffusion layer");
                }
            }

            var secretBytes = MakeSecretBytes(secret, Arguments.HexMode, degree);
            
            foreach (var share in SecretSplitter.Split(SecretShareType.Message, secretBytes, Arguments.Threshold, diffuser).GetShares(Arguments.Shares)) {
                Console.WriteLine(share.ToString(SecretShareFormattingOptions.None));
            }
        }

        private static byte[] MakeSecretBytes(string secret, bool useHexEncoding, int degree) {
            if (useHexEncoding) {
                if (secret.Length > (degree/4)) {
                    Fatal("input string too long");
                }

                if (secret.Length < (degree/4)) {
                    Warning("input string too short, adding null padding on the left");
                }

                return BigInteger.Parse(secret, NumberStyles.HexNumber).ToUnsignedBigEndianBytes();
            }
            else {
                if (secret.Length > (degree/8)) {
                    Fatal("input string too long");
                }

                foreach (char c in secret) {
                    if ((c < 32) || (c >= 127)) {
                        Warning("binary data detected, use -x mode instead");
                        break;
                    }
                }

                return SecretEncoder.EncodeString(secret);
            }
        }

        private static void Combine() {
            if (!Arguments.QuietMode) {
                Console.WriteLine("Enter {0} shares separated by newlines:", Arguments.Threshold);
            }

            int polynomialDegree = -1;

            var totalShares = new List<string>();

            for (int i = 0; i < Arguments.Threshold; i++) {
                if (!Arguments.QuietMode) {
                    Console.Write("Share [{0}/{1}]: ", i + 1, Arguments.Threshold);
                }

                string currentShare = String.Empty;

                try {
                    currentShare = Console.ReadLine();
                }
                catch {
                    Fatal("I/O error while reading shares");
                }

                SecretShare share;
                
                if (!SecretShare.TryParse(currentShare, out share)) {
                    Fatal("invalid syntax");
                }

                if (polynomialDegree < 0) {
                    polynomialDegree = share.Point.Y.PrimePolynomial.Degree;
                    if(!IrreduciblePolynomial.IsValidDegree(polynomialDegree)) {
                        Fatal("share has illegal length");
                    }
                }
                else if (polynomialDegree != share.Point.Y.PrimePolynomial.Degree) {
                    Fatal("shares have different security levels");
                }

                totalShares.Add(currentShare);
            }

            Diffuser diffuser = new NullDiffuser();

            if (Arguments.EnableDiffusion) {
                if (polynomialDegree >= 64) {
                    diffuser = new XteaDiffuser();
                }
                else {
                    Warning("security level too small for the diffusion layer");
                }
            }

            var recoveredSecret = SecretCombiner.Combine(totalShares, diffuser);
            
            if (!Arguments.QuietMode) {
                Console.Error.Write("Resulting secret: ");
            }
            
            Console.Error.WriteLine(Arguments.HexMode ? recoveredSecret.RecoveredHexString : recoveredSecret.RecoveredTextString);
        }

        private static void Warning(string message) {
            if (!Arguments.ExtraQuietMode) {
                Console.Error.WriteLine("WARNING: {0}", message);
            }
        }

        private static void Fatal(string message) {
            Console.Error.WriteLine("FATAL: {0}", message);
            Environment.Exit(1);
        }

        private static string ReadLineHidden() {
            var sb = new StringBuilder();
            while (true) {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter) {
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace) {
                    if (sb.Length > 0) {
                        sb.Length--;
                    }
                }

                sb.Append(keyInfo.KeyChar);
            }

            return sb.ToString();
        }

        private static string GetProgramNameWithoutExtension() {
            var manifestName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            return Path.GetFileNameWithoutExtension(manifestName);
        }
    }
}