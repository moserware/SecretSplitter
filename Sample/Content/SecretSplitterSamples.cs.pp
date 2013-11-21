using Moserware.Security.Cryptography;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace $rootnamespace$ {
    public static class SecretSplitterSamples {
        public static void RunSamples() {
            // Let's run a few examples. Simply call this method from your project.
            // Feel free to follow along in the debugger.
            // It's important to note that this sample file shows several different scenarios.
            // Pick the one that best matches your needs and delete the others

            SplitSimpleMessage();
            SplitFile();

            // For more details on the theory behind this program, see
            // http://www.moserware.com/2011/11/life-death-and-splitting-secrets.html
        }

        private static void SplitSimpleMessage() {
            // In this example, we take a very simple message (a string) and split it directly.
            // You can do this for messages below ~1250 characters. However, keep in mind
            // that the size of the message is directly proportional to the size of *each* share/split.

            const string secretMessage = "Hello World!";

            // The threshold is exactly how many shares need to be combined to reconstruct the secret.
            // You must have exactly this many: no more, no less
            const int threshold = 3;

            // The total shares is the number of shares to generate. You can generate as many as you'd like.
            // Any combination of the threshold (3 in this example) can be used to reconstruct the secret;
            const int totalShares = 5;
            var shares = SecretSplitter.SplitMessage(secretMessage, threshold, totalShares);

            // Note that every time you run this example you'll get different shares. That's because there is
            // a randomized component to the share generation process.

            Debug.Assert(shares.Length == 5);

            // Notice that *any* combination of three shares works to reconstruct the secret message
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[1], shares[2] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[1], shares[3] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[1], shares[4] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[2], shares[3] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[2], shares[4] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[0], shares[3], shares[4] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[1], shares[2], shares[3] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[1], shares[2], shares[4] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[1], shares[3], shares[4] }).RecoveredTextString == secretMessage);
            Debug.Assert(SecretCombiner.Combine(new[] { shares[2], shares[3], shares[4] }).RecoveredTextString == secretMessage);

            // However, having less shares doesn't work
            Debug.Assert(SecretCombiner.Combine(shares.Take(threshold - 1)).RecoveredTextString != secretMessage);

            // Having more shares doesn't work either
            Debug.Assert(SecretCombiner.Combine(shares.Take(threshold + 1)).RecoveredTextString != secretMessage);
        }
         
        private static void SplitFile() {
            // If your message is above 1K, you should use the hybrid approach of splitting the master secret
            // (which is the encryption key) and then use that to encrypt the file using OpenPGP and AES.

            // Here's an example of how to do this hybrid approach:

            // First, get a master secret of whatever size you want. 128 bits is plenty big and is a nice
            // balance of share sizes and security. However, to be fun, let's be super paranoid and go with
            // 256 bits (at the cost of bigger shares!)
            var masterPassword = HexadecimalPasswordGenerator.GeneratePasswordOfBitSize(bitSize: 256);
            var masterPasswordBytes = SecretEncoder.ParseHexString(masterPassword);

            // As mentioned above, the threshold is the total number of shares that need to come together
            // to unlock the secret.
            const int threshold = 3;

            // Now we create a class to help us encrypt everything else:
            var splitSecret = SecretSplitter.SplitFile(masterPasswordBytes, threshold);

            // We can generate as many shares as we'd like knowing that 3 of them need to come together
            // to reconstruct the secret.
            const int totalShares = 5;
            var shares = splitSecret.GetShares(totalShares);

            // The textual representation is what you'd typically distribute:
            var sharesText = shares.Select(s => s.ToString()).ToList();

            // Remember that the shares are just mechanisms to distribute the master password. You still
            // need to distribute the encrypted file

            // Normally, you'd probably use the simpler method of
            // splitSecret.EncryptFile(inputPath, outputPath);

            // But this sample will use the more generic stream based version to keep everything in memory

            // Let's get a spot to store the encrypted output
            Stream encryptedStream;

            // First, let's make up a sample message of the numbers 1-1000
            // As mentioned before, normally you'd just use a simple file name with EncryptFile or an 
            // existing stream:

            // The OpenPGP format stores the name of the file, so we provide that here. It can be whatever
            // you want
            const string fileNameInsideEncryptedContainer = "numbers.txt";

            using (var inputStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(inputStream)) {
                // Generate the numbers 1..1000 each on a single line
                for (int i = 1; i <= 1000; i++) {
                    streamWriter.WriteLine(i);
                }

                // we're done writing
                streamWriter.Flush();

                // Note that the size is bigger than 1250 bytes (the limit of splitting the message itself):
                Debug.Assert(inputStream.Length > 1250);

                // Now we can use the input. Reset its position to the start of the stream:
                inputStream.Position = 0;

                // Finally, encrypt the file. Save it with the filename metadata:
                encryptedStream = splitSecret.Encrypt(inputStream, fileNameInsideEncryptedContainer);
            }

            // We can save the contents of outputStream wherever we'd like. It's encrypted.

            // Let's go ahead and decrypt it.

            // First, take the threshold number of shares to recover the master secret:
            var combinedSecret = SecretCombiner.Combine(sharesText.Take(threshold));

            // This metadata is present inside the encrypted file
            string decryptedFileName;
            DateTime decryptedFileDateTime;

            using (var decryptedStream = combinedSecret.Decrypt(encryptedStream, out decryptedFileName, out decryptedFileDateTime))
            using (var decryptedStreamReader = new StreamReader(decryptedStream)) {
                // For fun, verify the decrypted file is what we expect:

                Debug.Assert(decryptedFileName == fileNameInsideEncryptedContainer);
                for (int expectedNumber = 1; expectedNumber <= 1000; expectedNumber++) {
                    var currentLineText = decryptedStreamReader.ReadLine();
                    var currentLineNumber = Int32.Parse(currentLineText);
                    Debug.Assert(currentLineNumber == expectedNumber);
                }

                // Nothing left:
                Debug.Assert(decryptedStreamReader.EndOfStream);
            }
        }
    }
}
