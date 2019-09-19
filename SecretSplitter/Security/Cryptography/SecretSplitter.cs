using Moserware.Algebra;
using Moserware.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security;
using System.Security.Cryptography;

namespace Moserware.Security.Cryptography {
    public static class SecretSplitter {
        private static readonly Diffuser DefaultDiffuser = new SsssDiffuser();

        public static string[] SplitMessage(string secretMessage, int threshold, int totalShares) {
            return SplitMessage(secretMessage, threshold, totalShares, DefaultDiffuser);
        }

        // Keeping the methods that take a Diffuser as private to simplify API for now
        private static string[] SplitMessage(string secret, int threshold, int totalShares, Diffuser diffuser) {
            return
                Split(SecretShareType.Message, SecretEncoder.EncodeString(secret), threshold, diffuser)
                .GetShares(totalShares)
                .Select(share => share.ToString())
                .ToArray();
        }
        
        public static SplitSecret SplitMessage(byte[] secretMessage, int threshold) {
            return Split(SecretShareType.Message, secretMessage, threshold, DefaultDiffuser);
        }

        public static SplitSecret SplitFile(byte[] masterKey, int threshold) {
            return Split(SecretShareType.File, masterKey, threshold);
        }
        
        private static SplitSecret Split(SecretShareType shareType, byte[] secret, int threshold) {
            return Split(shareType, secret, threshold, DefaultDiffuser);
        }

        public static SplitSecret Split(SecretShareType shareType, byte[] secret, int threshold, Diffuser diffuser) {
            var irreduciblePolynomial = IrreduciblePolynomial.CreateOfByteSize(secret.Length);
            var rawSecret = secret.ToBigIntegerFromBigEndianUnsignedBytes();
            var diffusedSecret = diffuser.Scramble(rawSecret, secret.Length);
            var secretCoefficient = new FiniteFieldPolynomial(irreduciblePolynomial, diffusedSecret);

            var allCoefficients = new[] { secretCoefficient }
                .Concat(
                    GetRandomPolynomials(
                        irreduciblePolynomial,
                        threshold - 1)
                )
                .ToArray();

            var passPhrase = new SecureString();

            try {
                foreach (var currentChar in secret.ToHexString()) {
                    passPhrase.AppendChar(currentChar);
                }
            }
            catch {
                passPhrase = null;
            }

            if((passPhrase == null) || (passPhrase.Length == 0)) {
                passPhrase = null;
            }

            return new SplitSecret(shareType, threshold, irreduciblePolynomial, allCoefficients, passPhrase);
        }

        private static IEnumerable<FiniteFieldPolynomial> GetRandomPolynomials(IrreduciblePolynomial irreduciblePolynomial, int total) {
            var rng = RandomNumberGenerator.Create();

            for (int i = 0; i < total; i++) {
                var randomCoefficientBytes = new byte[irreduciblePolynomial.SizeInBytes];
                rng.GetBytes(randomCoefficientBytes);
                yield return new FiniteFieldPolynomial(irreduciblePolynomial, randomCoefficientBytes.ToBigIntegerFromLittleEndianUnsignedBytes());
            }
        }
    }

    /// <summary>
    /// Represents a secret that has been split.
    /// </summary>
    public class SplitSecret {
        private readonly IrreduciblePolynomial _IrreduciblePolynomial;
        private readonly FiniteFieldPolynomial[] _AllCoefficients;
        private readonly SecretShareType _ShareType;
        private readonly SecureString _PassPhrase;

        public SplitSecret(SecretShareType shareType, int threshold, IrreduciblePolynomial irreduciblePolynomial, FiniteFieldPolynomial[] allCoefficients, SecureString passPhrase = null) {
            _ShareType = shareType;
            Threshold = threshold;
            _IrreduciblePolynomial = irreduciblePolynomial;
            _AllCoefficients = allCoefficients;
            _PassPhrase = passPhrase;
        }
        
        public SecretShare GetShare(int n) {
            var xPoly = new FiniteFieldPolynomial(_IrreduciblePolynomial, new BigInteger(n));
            var y = FiniteFieldPolynomial.EvaluateAt(n, _AllCoefficients);
            return new SecretShare(_ShareType, new FiniteFieldPoint(xPoly, y));
        }
        
        public IEnumerable<SecretShare> GetShares(int totalShares) {
            return Enumerable.Range(1, totalShares).Select(GetShare);
        }

        public int Threshold { get; private set; }

        internal FiniteFieldPolynomial[] AllCoefficients {
            get { return _AllCoefficients; }
        }

        public Stream Encrypt(Stream inputStream, string fileName) {
            return Encrypt(inputStream, fileName, DateTime.UtcNow);
        }

        public Stream Encrypt(Stream inputStream, string fileName, DateTime fileDateTime) {
            if((_ShareType != SecretShareType.File) || (_PassPhrase == null)) {
                throw new InvalidOperationException("Cannot encrypt file unless share is file type");
            }

            return OpenPgp.EncryptSingleFile(_PassPhrase, inputStream, fileName, fileDateTime);
        }

        public void EncryptFile(string plaintextInputPath, string encryptedOutputPath) {
            if (File.Exists(encryptedOutputPath)) {
                File.Delete(encryptedOutputPath);
            }

            using(var plaintextStream = File.OpenRead(plaintextInputPath)) 
            using(var encryptedOutputStream = File.OpenWrite(encryptedOutputPath)) {
                var lastWriteDate = File.GetLastWriteTimeUtc(plaintextInputPath);
                var encryptedMemoryStream = Encrypt(plaintextStream, Path.GetFileName(plaintextInputPath), lastWriteDate);
                encryptedMemoryStream.CopyTo(encryptedOutputStream);
            }
        }
    }

    // Simple base for all exceptions
    public class SecretSplitterException : Exception {
        public SecretSplitterException(string message)
            : base(message) {
        }
    }
}

namespace Moserware.Security.Cryptography.Versioning {
    public static class VersionInfo {
        public const string CurrentVersionString = "0.20";
        public static Version CurrentVersion = new Version(CurrentVersionString);
    }
}
