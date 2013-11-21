using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Moserware.Security.Cryptography {
    // I designed this to be the simplest OpenPGP (RFC4880) compliant reader and writer that did exactly what I wanted
    // I implemented everything from scratch by just following the RFC. Thus, there shouldn't be any licensing issues
    // to worry about if you want to use it in a derived work.
    public static class OpenPgp {
        public static Stream DecryptSingleFile(Stream encryptedStream, SecureString passphrase) {
            string ignoredFileName;
            return DecryptSingleFile(encryptedStream, passphrase, out ignoredFileName);
        }

        public static Stream DecryptSingleFile(Stream encryptedStream, SecureString passphrase, out string filename) {
            DateTime ignoredDate;
            return DecryptSingleFile(encryptedStream, passphrase, out filename, out ignoredDate);
        }

        public static Stream DecryptSingleFile(Stream encryptedStream, SecureString passphrase, out string filename, out DateTime fileDate) {
            SymmetricAlgorithm symmetricAlgorithm = null;
            
            foreach (var packet in OpenPgpPacketReader.ReadAllPackets(encryptedStream)) {
                var encryptedSessionKey = packet as SymmetricKeyEncryptedSessionKeyOpenPgpPacket;
                if (encryptedSessionKey != null) {
                    symmetricAlgorithm = encryptedSessionKey.GetKeyedSymmetricAlgorithm(passphrase);
                }

                var encryptedIntegrityProtectedData = packet as SymmetricallyEncryptedIntegrityProtectedDataOpenPgpPacket;
                if (encryptedIntegrityProtectedData != null) {
                    foreach (var decryptedPacket in encryptedIntegrityProtectedData.GetDecryptedPackets(symmetricAlgorithm)) {
                        var compressedPacket = decryptedPacket as CompressedDataOpenPgpPacket;
                        if (compressedPacket != null) {
                            Stream literalStream = null;
                            filename = null;
                            fileDate = DateTime.MinValue;

                            foreach (var decompressedPacket in OpenPgpPacketReader.ReadAllPackets(compressedPacket.DecompressedStream)) {
                                var literalData = decompressedPacket as LiteralDataOpenPgpPacket;
                                if (literalData != null) {
                                    filename = literalData.FileName;
                                    fileDate = literalData.FileDate;
                                    literalStream = literalData.LiteralStream;
                                }
                            }

                            return literalStream;
                        }
                    }
                }
            }

            throw new CryptographicException();
        }

        public static Stream EncryptSingleFile(SecureString passphrase, Stream contentsToEncrypt, string fileName) {
            return EncryptSingleFile(passphrase, contentsToEncrypt, fileName, DateTime.UtcNow);
        }

        public static Stream EncryptSingleFile(SecureString passphrase, Stream contentsToEncrypt, string fileName, DateTime fileDateTime) {
            var msOutput = new MemoryStream();
            
            var keyPacket = new SymmetricKeyEncryptedSessionKeyOpenPgpPacket();
            keyPacket.WriteTo(msOutput);

            var literalData = new LiteralDataOpenPgpPacket();
            literalData.FileName = fileName;
            literalData.FileDate = fileDateTime;
            contentsToEncrypt.CopyTo(literalData.LiteralStream);

            var compressedPacket = new CompressedDataOpenPgpPacket();
            literalData.WriteTo(compressedPacket.DecompressedStream);
            
            var symmetricAlgorithm = keyPacket.GetKeyedSymmetricAlgorithm(passphrase);
            var outerPacket = new SymmetricallyEncryptedIntegrityProtectedDataOpenPgpPacket();
            outerPacket.EncryptPackets(symmetricAlgorithm, compressedPacket);

            outerPacket.WriteTo(msOutput);

            msOutput.Position = 0;
            return msOutput;
        }
    }

    internal static class OpenPgpPacketReader {
        private static readonly Dictionary<int, Type> _TagIdToType = new Dictionary<int, Type>();

        static OpenPgpPacketReader() {
            // Use reflection to find them
            var packetType = typeof (OpenPgpPacket);

            foreach(var currentPacketType in typeof(OpenPgpPacketReader).Assembly.GetTypes().Where(t => (t != packetType) && packetType.IsAssignableFrom(t) && !packetType.IsAbstract)) {
                // hacky way to get the id
                var tempPacket = Activator.CreateInstance(currentPacketType) as OpenPgpPacket;
                if(tempPacket == null) {
                    throw new NotImplementedException();
                }
                _TagIdToType[tempPacket.TagId] = currentPacketType;
            }
            
        }

        public static IEnumerable<OpenPgpPacket> ReadAllPackets(Stream inputStream) {
            OpenPgpPacketHeader header;
            while(OpenPgpPacketHeader.TryReadFrom(inputStream, out header)) {
                long currentPosition = inputStream.Position;
                long afterPacket = currentPosition + header.Length;

                Type mappedType;
                if(_TagIdToType.TryGetValue(header.TagId, out mappedType)) {
                    yield return Activator.CreateInstance(mappedType, header, inputStream) as OpenPgpPacket;
                }
                else {
                    yield return new OpenPgpPacket(header, inputStream);
                }

                if (header.Length >= 0) {
                    inputStream.Seek(afterPacket, SeekOrigin.Begin);
                }
            }
        }
    }

    internal class OpenPgpPacket {
        private readonly Stream _InputStream;
        private readonly long _StartOffset = -1;

        protected OpenPgpPacket(int tagId) {
            TagId = tagId;
            Header = new OpenPgpPacketHeader(tagId);
        }

        internal OpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : this(header.TagId) {
            _InputStream = inputStream;
            _StartOffset = inputStream.Position;
            Header = header;
        }

        internal int TagId { get; set; }
        public OpenPgpPacketHeader Header { get; private set; }
        protected long StartOfNextPacketOffset {
            get { return _StartOffset + Header.Length; }
        }

        protected Stream InputStream {
            get { return _InputStream; }
        }

        public virtual void WriteTo(Stream outputStream) {
            Header.WriteTo(outputStream);
        }
    }

    // per RFC4480 section 4.2, defaults to new packet format
    internal class OpenPgpPacketHeader {
        internal const byte NewHeaderHighBits = 0xC0;
        private long _Length;
        protected int _TagId;

        public OpenPgpPacketHeader(int tagId) {
            _TagId = tagId;
        }

        internal OpenPgpPacketHeader(byte firstByte, Stream inputStream) {
            if((firstByte & NewHeaderHighBits) != NewHeaderHighBits) {
                throw new ArgumentException();
            }

            _TagId = firstByte & ~NewHeaderHighBits;

            // See 4.2.2.1: "A one-octet Body Length header encodes a length of 0 to 191 octets."
            int firstLengthByte = inputStream.ReadByte();
            if (firstLengthByte < 0) {
                throw new EndOfStreamException();
            }
            if (firstLengthByte < 192) {
                Length = firstLengthByte;
                return;
            }

            // See 4.2.2.2: "A two-octet Body Length header encodes a length of 192 to 8383 octets."
            if((firstLengthByte >= 192) && (firstLengthByte <= 223)) {
                int secondLengthByte = inputStream.ReadByte();
                if (secondLengthByte < 0) {
                    throw new EndOfStreamException();
                }
                Length = ((firstLengthByte - 192) << 8) + secondLengthByte + 192;
                return;
            }

            // 4.2.2.3: "A five-octet Body Length header consists of a single octet holding the value 255, followed by a four-octet scalar."

            if(firstLengthByte != 255) {
                throw new InvalidDataException();
            }

            Length = OpenPgpScalarNumber.Read(inputStream, 4);
        }

        protected OpenPgpPacketHeader() {}

        public long Length {
            get { return _Length; }
            set {
                if(value < -1) {
                    throw new ArgumentOutOfRangeException();
                }
                _Length = value;
            }
        }

        public virtual int TagId {
            get { return _TagId; }
            set {
                if ((value < 0) || (value > 63)) {
                    throw new ArgumentOutOfRangeException();
                }
                _TagId = value;
            }
        }

        public virtual void WriteTo(Stream outputStream) {
            outputStream.WriteByte((byte)(NewHeaderHighBits | TagId));
            if(Length <= 191) {
                outputStream.WriteByte((byte)Length);
            }
            else if(Length <= 8383) {
                int bodyLenRemainder = (int)(Length - 192);
                byte secondLengthByte = (byte) (bodyLenRemainder & 0xFF);
                bodyLenRemainder >>= 8;
                byte firstLengthByte = (byte) (bodyLenRemainder + 192);
                outputStream.WriteByte(firstLengthByte);
                outputStream.WriteByte(secondLengthByte);
            }
            else {
                outputStream.WriteByte(0xFF);
                OpenPgpScalarNumber.WriteTo((uint)Length, outputStream);
            }
        }

        public static bool TryReadFrom(Stream inputStream, out OpenPgpPacketHeader readPacketHeader) {
            int firstByteRaw = inputStream.ReadByte();
            if(firstByteRaw < 0) {
                readPacketHeader = null;
                return false;
            }

            var firstByte = (byte) firstByteRaw;
            
            if((firstByte & NewHeaderHighBits) == NewHeaderHighBits) {
                readPacketHeader = new OpenPgpPacketHeader(firstByte, inputStream);
                return true;
            }
            if((firstByte & OpenPgpOldFormatPacketHeader.OldHeaderHighBits) == OpenPgpOldFormatPacketHeader.OldHeaderHighBits) {
                readPacketHeader = new OpenPgpOldFormatPacketHeader(firstByte, inputStream);
                return true;
            }
            readPacketHeader = null;
            return false;
        }
    }

    internal class OpenPgpOldFormatPacketHeader : OpenPgpPacketHeader {
        internal const byte OldHeaderHighBits = 0x80;
        private const byte OldHeaderLengthTypeBits = 0x03;

        internal OpenPgpOldFormatPacketHeader(byte firstByte, Stream inputStream) {
            _TagId = (firstByte & ~OldHeaderHighBits) >> 2;

            int lengthType = (firstByte & OldHeaderLengthTypeBits);

            if (lengthType == 3) {
                // 4.2.1 - Indeterminate length
                Length = -1;
                return;
            }

            int firstLengthOctet = inputStream.ReadByte();
            if(firstLengthOctet < 0) {
                throw new EndOfStreamException();
            }

            // See 4.2.1: Old Format Packet Lengths
            if(lengthType == 0) {
                Length = firstLengthOctet;
                return;
            }

            int secondLengthOctet = inputStream.ReadByte();
            if(secondLengthOctet < 0) {
                throw new EndOfStreamException();
            }

            if(lengthType == 1) {
                Length = (firstLengthOctet << 16) | secondLengthOctet;
            }
            else if(lengthType == 2) {
                int thirdLengthOctet = inputStream.ReadByte();
                int fourthLengthOctet = inputStream.ReadByte();
                if(fourthLengthOctet < 0) {
                    throw new EndOfStreamException();
                }
                Length = (firstLengthOctet << 24) | (secondLengthOctet << 16) | (thirdLengthOctet << 8) | fourthLengthOctet;
            }
            else {
                throw new NotSupportedException();
            }
        }

        public override int TagId {
            get { return base.TagId; }
            set {
                if(value >= 16) {
                    throw new ArgumentOutOfRangeException("TagId");
                }
                base.TagId = value;
            }
        }

        public override void WriteTo(Stream outputStream) {
            var firstByte = (byte) (OldHeaderHighBits | (TagId << 2));

            if(Length < 256) {
                outputStream.WriteByte(firstByte);
            }
            else if(Length < 65536) {
                outputStream.WriteByte((byte)(firstByte | 1));
                outputStream.WriteByte((byte)(Length >> 8));
                outputStream.WriteByte((byte)(Length & 0xFF));
            }
            else {
                outputStream.WriteByte((byte)(firstByte | 2));
                outputStream.WriteByte((byte)(Length >> 24));
                outputStream.WriteByte((byte)((Length & 0xFF0000) >> 16));
                outputStream.WriteByte((byte)((Length & 0xFF00) >> 8));
                outputStream.WriteByte((byte)(Length & 0xFF));
            }
        }
    }

    // Section 3.1
    // Stored unsigned in big-endian format
    internal static class OpenPgpScalarNumber {
        public static uint Read(Stream inputStream, int bytesToRead) {
            var bytes = new byte[Marshal.SizeOf(typeof(uint))];
            inputStream.Read(bytes, 0, bytes.Length);
            Array.Reverse(bytes); // big -> little endian
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static void WriteTo(uint number, Stream outputStream) {
            // Natively is little endian, so reverse it to make it big endian
            var bytes = BitConverter.GetBytes(number);
            Array.Reverse(bytes);
            outputStream.Write(bytes, 0, bytes.Length);
        }
    }

    // Section 3.5
    internal class OpenPgpTimeField {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public OpenPgpTimeField(long secondsSinceEpoch) {
            Date = Epoch.AddSeconds(secondsSinceEpoch);
        }

        public OpenPgpTimeField(DateTime date) {
            Date = date;
        }

        public DateTime Date { get; private set; }

        public static OpenPgpTimeField Read(Stream inputStream) {
            return new OpenPgpTimeField(OpenPgpScalarNumber.Read(inputStream, 4));
        }

        public void WriteTo(Stream outputStream) {
            OpenPgpScalarNumber.WriteTo((uint)(Date - Epoch).TotalSeconds, outputStream);
        }
    }

    // Packet types

    // Section 5.3
    internal class SymmetricKeyEncryptedSessionKeyOpenPgpPacket : OpenPgpPacket {
        private const byte ExpectedVersion = 0x04;

        public SymmetricKeyEncryptedSessionKeyOpenPgpPacket() : base(3) {
            Cipher = OpenPgpSymmetricCipher.Default;
            StringToKeySpecifier = new IteratedAndSaltedOpenPgpStringToKeySpecifier();
        }

        public SymmetricKeyEncryptedSessionKeyOpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : base(header, inputStream) {
            int versionNumber = inputStream.ReadByte();
            if(versionNumber != ExpectedVersion) {
                throw new InvalidDataException();
            }

            Cipher = OpenPgpSymmetricCipher.GetCipher(inputStream.ReadByte());
            StringToKeySpecifier = OpenPgpStringToKeySpecifier.ReadFrom(inputStream);
            
            if(inputStream.Position < StartOfNextPacketOffset) {
                throw new NotImplementedException();
            }
        }

        public OpenPgpSymmetricCipher Cipher { get; set; }
        public OpenPgpStringToKeySpecifier StringToKeySpecifier { get; set; }

        public byte[] HashToSessionKey(SecureString passphrase) {
            return StringToKeySpecifier.HashToKey(passphrase, Cipher);
        }

        public SymmetricAlgorithm GetKeyedSymmetricAlgorithm(SecureString passphrase) {
            var key = HashToSessionKey(passphrase);
            var symmetricAlgorithm = Cipher.GetNewSymmetricAlgorithmInstance();
            symmetricAlgorithm.Key = key;
            return symmetricAlgorithm;
        }

        public override void WriteTo(Stream outputStream) {
            Header.Length = 1 /* version */+ Cipher.SubpacketByteLength + StringToKeySpecifier.SubpacketByteLength;
            base.WriteTo(outputStream);
            outputStream.WriteByte(ExpectedVersion);
            outputStream.WriteByte((byte)Cipher.Id);
            StringToKeySpecifier.WriteTo(outputStream);
        }
    }

    // Section 5.13
    internal class SymmetricallyEncryptedIntegrityProtectedDataOpenPgpPacket : OpenPgpPacket {
        private const byte ExpectedVersion = 0x01;

        public SymmetricallyEncryptedIntegrityProtectedDataOpenPgpPacket() : base(18) {
        }

        public SymmetricallyEncryptedIntegrityProtectedDataOpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : base(header, inputStream) {
            var foundByte = inputStream.ReadByte();
            if(foundByte != ExpectedVersion) {
                throw new InvalidDataException();
            }

            int encryptedDataLength = (int) header.Length - 1; // minus version byte
            
            var ms = new MemoryStream(encryptedDataLength);
            inputStream.CopySubsetTo(ms, encryptedDataLength);
            ms.Position = 0;
            EncryptedStream = ms;
        }

        public Stream EncryptedStream { get; private set; }

        // Throw error if not valid
        public IEnumerable<OpenPgpPacket> GetDecryptedPackets(SymmetricAlgorithm keyedAlgorithm) {
            var transform = new OpenPgpCfbTransform(keyedAlgorithm, encrypt:false);
            var decryptor = new CryptoStream(EncryptedStream, transform, CryptoStreamMode.Read);
            var decrypted = new MemoryStream();
            decryptor.CopyTo(decrypted);
            
            // Get rid of the modification detection code, but verify it
            var mdcStart = decrypted.Length - (1 + 1 + 20);
            decrypted.Position = mdcStart;
            
            var mdcStream = new MemoryStream();
            decrypted.CopyTo(mdcStream);
            decrypted.Position = 0;
            decrypted.SetLength(mdcStart + 2);
            var hasher = SHA1.Create();
            var hashIncludingHeaderAndLength = hasher.ComputeHash(transform.PrefixBytes.Concat(decrypted.ToArray()).ToArray());
            decrypted.SetLength(decrypted.Length - 2);
            decrypted.Position = 0;
            // TODO:
            mdcStream.Position = 0;
            var mdc = OpenPgpPacketReader.ReadAllPackets(mdcStream).First() as ModificationDetectionCodeOpenPgpPacket;

            for(int i = 0; i < hashIncludingHeaderAndLength.Length; i++) {
                if(hashIncludingHeaderAndLength[i] != mdc.HashValue[i]) {
                    throw new ModificationDetectedException();
                }
            }

            return OpenPgpPacketReader.ReadAllPackets(decrypted);
        }

        public void EncryptPackets(SymmetricAlgorithm keyedAlgorithm, params OpenPgpPacket[] packetsToEncrypt) {
            var transform = new OpenPgpCfbTransform(keyedAlgorithm, encrypt: true);
            var msEncrypted = new MemoryStream();
            EncryptedStream = msEncrypted;
            var encryptor = new CryptoStream(msEncrypted, transform, CryptoStreamMode.Write);

            // HACK: Write the prefix bytes manually rather than do it all in the transform since we need it for the hash
            encryptor.Write(transform.PrefixBytes, 0, transform.PrefixBytes.Length);

            var hasher = SHA1.Create();
            var hashContentsStream = new MemoryStream();
            var hashStream = new CryptoStream(hashContentsStream, hasher, CryptoStreamMode.Write);

            hashStream.Write(transform.PrefixBytes, 0, transform.PrefixBytes.Length);

            var currentPacketStream = new MemoryStream();

            foreach(var currentPacketToEncrypt in packetsToEncrypt) {
                currentPacketStream.SetLength(0);
                currentPacketToEncrypt.WriteTo(currentPacketStream);
                currentPacketStream.Position = 0;
                currentPacketStream.CopyTo(hashStream);
                currentPacketStream.Position = 0;
                currentPacketStream.CopyTo(encryptor);
            }

            var mdcPacket = new ModificationDetectionCodeOpenPgpPacket();
            mdcPacket.Header.Length = hasher.HashSize/8;
            mdcPacket.Header.WriteTo(hashStream);

            hashStream.FlushFinalBlock();

            var hashContents = hasher.Hash;

            mdcPacket.HashValue = hashContents;
            mdcPacket.WriteTo(encryptor);

            // SECREVIEW: Emit MDC packet
            encryptor.FlushFinalBlock();
            msEncrypted.Position = 0;
        }

        public override void WriteTo(Stream outputStream) {
            var msEncrypted = EncryptedStream as MemoryStream;
            Header.Length = msEncrypted.Length + 1;
            base.WriteTo(outputStream);
            outputStream.WriteByte(ExpectedVersion);
            msEncrypted.Position = 0;
            msEncrypted.CopyTo(outputStream);
        }
    }

    public class ModificationDetectedException : SecurityException {
        public ModificationDetectedException() : base("It appears that the file has been tampered with or the key specified was invalid.") {
            
        }
    }

    internal class ModificationDetectionCodeOpenPgpPacket : OpenPgpPacket {
        public ModificationDetectionCodeOpenPgpPacket() : base(19) {
        }

        public ModificationDetectionCodeOpenPgpPacket(byte[] hashValue) : this() {
            HashValue = hashValue;
        }

        public ModificationDetectionCodeOpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : base(header, inputStream) {
            var sha1Value = new byte[20];
            if(inputStream.Read(sha1Value, 0, 20) != 20) {
                throw new CryptographicException();
            }
            HashValue = sha1Value;
        }

        public byte[] HashValue { get; set; }

        public override void WriteTo(Stream outputStream) {
            Header.Length = HashValue.Length;
            base.WriteTo(outputStream);
            outputStream.Write(HashValue, 0, HashValue.Length);
        }
    }

    internal class CompressedDataOpenPgpPacket : OpenPgpPacket {
        public CompressedDataOpenPgpPacket() : base(8) {
            DecompressedStream = new MemoryStream();
            CompressionAlgorithm = OpenPgpCompressionAlgorithm.DefaultCompressionAlgorithm;
        }
        
        public CompressedDataOpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : base(header, inputStream) {
            int compressionAlgorithmId = inputStream.ReadByte();
            CompressionAlgorithm = OpenPgpCompressionAlgorithm.GetCompressionAlgorithmById(compressionAlgorithmId);
            
            var decompressionStream = CompressionAlgorithm.Decompress(inputStream) as DeflateStream;
            DecompressedStream = new MemoryStream();
            decompressionStream.CopyTo(DecompressedStream);
            DecompressedStream.Position = 0;
        }

        public Stream DecompressedStream { get; set; }
        public OpenPgpCompressionAlgorithm CompressionAlgorithm { get; set; }

        public override void WriteTo(Stream outputStream) {
            var compressedData = new MemoryStream();
            DecompressedStream.Position = 0;
            var compressorStream = CompressionAlgorithm.Compress(compressedData);
            DecompressedStream.CopyTo(compressorStream);
            compressorStream.Close();
            compressedData.Position = 0;
            Header.Length = 1 + compressedData.Length; // algorithm byte + length
            base.WriteTo(outputStream);
            outputStream.WriteByte((byte)CompressionAlgorithm.Id);
            compressedData.CopyTo(outputStream);
        }
    }

    // Section 5.9
    internal class LiteralDataOpenPgpPacket : OpenPgpPacket {
        public LiteralDataOpenPgpPacket() : base(11) {
            LiteralStream = new MemoryStream();
        }

        public LiteralDataOpenPgpPacket(OpenPgpPacketHeader header, Stream inputStream) : base(header, inputStream) {
            var format = (char) inputStream.ReadByte();
            var stringLength = inputStream.ReadByte();
            var stringBytes = inputStream.CopySubsetToBytes(stringLength);
            FileName = Encoding.UTF8.GetString(stringBytes);
            FileDate = OpenPgpTimeField.Read(inputStream).Date;
            LiteralStream = new MemoryStream();
            inputStream.CopySubsetTo(LiteralStream, StartOfNextPacketOffset - inputStream.Position);
            LiteralStream.Position = 0;
        }

        public string FileName { get; set; }
        public DateTime FileDate { get; set; }

        public Stream LiteralStream { get; set; }

        public CompressedDataOpenPgpPacket Compress() {
            var compressed = new CompressedDataOpenPgpPacket();
            WriteTo(compressed.DecompressedStream);
            return compressed;
        }

        public override void WriteTo(Stream outputStream) {
            Header.Length = 0;
            byte formatByte = (byte) 'b'; // binary
            Header.Length++; // format byte size
            var filenameBytes = Encoding.UTF8.GetBytes(FileName);
            Header.Length += 1 + filenameBytes.Length; // size of string
            Header.Length += 4; // sizeof time
            Header.Length += LiteralStream.Length;
            base.WriteTo(outputStream);

            outputStream.WriteByte(formatByte);
            outputStream.WriteByte((byte)filenameBytes.Length);
            outputStream.Write(filenameBytes, 0, filenameBytes.Length);
            new OpenPgpTimeField(FileDate).WriteTo(outputStream);
            LiteralStream.Position = 0;
            LiteralStream.CopyTo(outputStream);
        }
    }

    internal static class StreamExtensions {
        public static void CopySubsetTo(this Stream sourceStream, Stream destinationStream, long lengthToCopy) {
            var bytesLeftToCopy = lengthToCopy;
            var buffer = new byte[4096];

            while(bytesLeftToCopy > 0) {
                int bytesRead = sourceStream.Read(buffer, 0, (int) Math.Min((long) buffer.Length, bytesLeftToCopy));
                if(bytesRead == 0) {
                    break;
                }
                destinationStream.Write(buffer, 0, bytesRead);
                bytesLeftToCopy -= bytesRead;
            }
        }

        public static byte[] CopySubsetToBytes(this Stream inputStream, int lengthToCopy) {
            var msDestination = new MemoryStream(lengthToCopy);
            CopySubsetTo(inputStream, msDestination, lengthToCopy);
            return msDestination.ToArray();
        }
    }

    internal abstract class OpenPgpStringToKeySpecifier {
        // SECREVIEW: Make iterated and salted
        public static OpenPgpStringToKeySpecifier DefaultStringToKeySpecifier { get { return new SaltedOpenPgpStringToKeySpecifier(); } }

        protected OpenPgpStringToKeySpecifier(OpenPgpStringToKeySpecifierId id, Stream inputStream) : this(id) {
            HashAlgorithm = OpenPgpHashAlgorithm.GetHashAlgorithmById(inputStream.ReadByte());
        }

        protected OpenPgpStringToKeySpecifier(OpenPgpStringToKeySpecifierId id) {
            Id = id;
            // SECREVIEW: Able to set this?
            HashAlgorithm = OpenPgpHashAlgorithm.DefaultHashAlgorithm;
        }

        public OpenPgpStringToKeySpecifierId Id { get; private set; }
        public OpenPgpHashAlgorithm HashAlgorithm { get; private set; }
        public static OpenPgpStringToKeySpecifier ReadFrom(Stream inputStream) {
            var id = (OpenPgpStringToKeySpecifierId) inputStream.ReadByte();
            switch(id) {
                case OpenPgpStringToKeySpecifierId.IteratedAndSalted: return new IteratedAndSaltedOpenPgpStringToKeySpecifier(inputStream);
                case OpenPgpStringToKeySpecifierId.Salted: return new SaltedOpenPgpStringToKeySpecifier(inputStream);
                case OpenPgpStringToKeySpecifierId.Simple: return new SimpleOpenPgpStringToKeySpecifier(inputStream);
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual void WriteTo(Stream outputStream) {
            outputStream.WriteByte((byte) Id);
            outputStream.WriteByte((byte)this.HashAlgorithm.Id);
        }

        public virtual long SubpacketByteLength {
            get { return 1+1; } // Id + algorithm type bytes
        }

        public abstract byte[] HashToKey(SecureString passphrase, OpenPgpSymmetricCipher cipher);
    }

    internal class SimpleOpenPgpStringToKeySpecifier : OpenPgpStringToKeySpecifier {
        public SimpleOpenPgpStringToKeySpecifier() : base(OpenPgpStringToKeySpecifierId.Simple) {
        }

        public SimpleOpenPgpStringToKeySpecifier(Stream inputStream) : base(OpenPgpStringToKeySpecifierId.Simple, inputStream) {
            
        }

        public override byte[] HashToKey(SecureString passphrase, OpenPgpSymmetricCipher cipher) {
            return HashAlgorithm.ComputeHash(null, passphrase, cipher.KeySizeInBits / 8);
        }
    }

    internal class SaltedOpenPgpStringToKeySpecifier : OpenPgpStringToKeySpecifier {
        private const int SaltLength = 8;

        public SaltedOpenPgpStringToKeySpecifier() : this(OpenPgpStringToKeySpecifierId.Salted) {
        }

        protected SaltedOpenPgpStringToKeySpecifier(OpenPgpStringToKeySpecifierId id) : base(id) {
            var saltBytes = new byte[SaltLength];
            using(var rand = RandomNumberGenerator.Create()) {
                rand.GetBytes(saltBytes);
            }
            Salt = saltBytes;
        }

        public SaltedOpenPgpStringToKeySpecifier(Stream inputStream) : this(inputStream, OpenPgpStringToKeySpecifierId.Salted) {
        }

        protected SaltedOpenPgpStringToKeySpecifier(Stream inputStream, OpenPgpStringToKeySpecifierId id) : base(id, inputStream) {
            var saltBytes = new byte[SaltLength];
            if(inputStream.Read(saltBytes, 0, saltBytes.Length) <= 0) {
                throw new EndOfStreamException();
            }
            Salt = saltBytes;
        }

        public override void WriteTo(Stream outputStream) {
            base.WriteTo(outputStream);
            outputStream.Write(Salt, 0, Salt.Length);
        }

        public override long SubpacketByteLength {
            get { return base.SubpacketByteLength + SaltLength;}
        }

        public byte[] Salt { get; private set; }

        public override byte[] HashToKey(SecureString passphrase, OpenPgpSymmetricCipher cipher) {
            return HashAlgorithm.ComputeHash(Salt, passphrase, cipher.KeySizeInBits/8);
        }
    }

    internal class IteratedAndSaltedOpenPgpStringToKeySpecifier : SaltedOpenPgpStringToKeySpecifier {
        private const byte DefaultCount = 0xBE; // Using 0xBE => 3,932,160 octets for now since that's what GPG 2.0.17 seems to use
        private readonly byte _CountByte = DefaultCount;

        public IteratedAndSaltedOpenPgpStringToKeySpecifier()
            : base(OpenPgpStringToKeySpecifierId.IteratedAndSalted) {
        }

        public IteratedAndSaltedOpenPgpStringToKeySpecifier(Stream inputStream) : base(inputStream, OpenPgpStringToKeySpecifierId.IteratedAndSalted) {
            _CountByte = (byte)inputStream.ReadByte();
        }
        
        public int Count {
            get {
                // See 3.7.1.3 for explanation
                var c = _CountByte;
                const int expbias = 6;
                return (16 + (c & 15)) << ((c >> 4) + expbias);
            }
        }

        public override void WriteTo(Stream outputStream) {
            base.WriteTo(outputStream);
            outputStream.WriteByte(_CountByte);
        }

        public override long SubpacketByteLength {
            get { return base.SubpacketByteLength + 1; }
        }

        public override byte[] HashToKey(SecureString passphrase, OpenPgpSymmetricCipher cipher) {
            return HashAlgorithm.ComputeHash(Salt, passphrase, cipher.KeySizeInBits / 8, Count);
        }
    }
    
    internal class OpenPgpSymmetricCipher {
        private static readonly Dictionary<int, OpenPgpSymmetricCipher> _IdToCipher = new Dictionary<int, OpenPgpSymmetricCipher>();
        private static readonly Dictionary<string, OpenPgpSymmetricCipher> _NameToCipher = new Dictionary<string, OpenPgpSymmetricCipher>(StringComparer.OrdinalIgnoreCase);

        private readonly Func<SymmetricAlgorithm> _SymmetricAlgorithmFactory;

        static OpenPgpSymmetricCipher() {
            // Section 9.2
            AddCipher(7, "AES128", 128, () => new RijndaelManaged());
            AddCipher(8, "AES192", 192, () => new RijndaelManaged());
            AddCipher(9, "AES256", 256, () => new RijndaelManaged());
        }
        
        private static void AddCipher(int id, string name, int keySize, Func<SymmetricAlgorithm> cipherFactory) {
            var newCipher = new OpenPgpSymmetricCipher(id, name, () => { var sa = cipherFactory(); sa.KeySize = keySize; return sa; });
            _IdToCipher[id] = newCipher;
            _NameToCipher[name] = newCipher;
        }

        public static OpenPgpSymmetricCipher Default {
            get { return GetCipher("AES256"); }
        }

        public static OpenPgpSymmetricCipher GetCipher(int id) {
            return _IdToCipher[id];
        }

        public static OpenPgpSymmetricCipher GetCipher(string name) {
            return _NameToCipher[name];
        }

        public OpenPgpSymmetricCipher(int id, string name, Func<SymmetricAlgorithm> symmetricAlgorithmFactory) {
            _SymmetricAlgorithmFactory = symmetricAlgorithmFactory;
            Id = id;
            Name = name;
        }

        public SymmetricAlgorithm GetNewSymmetricAlgorithmInstance() {
            return _SymmetricAlgorithmFactory();
        }

        public int KeySizeInBits { get { return _SymmetricAlgorithmFactory().KeySize; }}
        public int Id { get; private set; }
        public string Name { get; private set; }

        public long SubpacketByteLength { get { return 1; } }
    }

    internal abstract class OpenPgpCompressionAlgorithm {
        public static readonly OpenPgpCompressionAlgorithm DefaultCompressionAlgorithm = new DeflateOpenPgpCompressionAlgorithm();
        public static OpenPgpCompressionAlgorithm GetCompressionAlgorithmById(int id) {
            if(id != 1) {
                throw new NotSupportedException();
            }

            return DefaultCompressionAlgorithm;
        }

        protected OpenPgpCompressionAlgorithm(int id) {
            Id = id;
        }

        public abstract Stream Compress(Stream decompressedStream);
        public abstract Stream Decompress(Stream compressedStream);
        public int Id { get; private set; }
    }

    internal class DeflateOpenPgpCompressionAlgorithm : OpenPgpCompressionAlgorithm {
        public DeflateOpenPgpCompressionAlgorithm() : base(1) {
            
        }

        public override Stream Compress(Stream decompressedStream) {
            return new DeflateStream(decompressedStream, CompressionMode.Compress, leaveOpen:true);
        }

        public override Stream Decompress(Stream compressedStream) {
            return new DeflateStream(compressedStream, CompressionMode.Decompress, leaveOpen: true);
        }
    }
    
    internal class OpenPgpHashAlgorithm {
        private static readonly Dictionary<int, OpenPgpHashAlgorithm> _IdToAlgorithm = new Dictionary<int, OpenPgpHashAlgorithm>();
        private readonly Func<HashAlgorithm> _HashFactory;

        static OpenPgpHashAlgorithm() {
            Action<int, Func<HashAlgorithm>> add = (id, factory) => _IdToAlgorithm[id] = new OpenPgpHashAlgorithm(id, factory);
            // Section 9.4
            add(2, () => new SHA1CryptoServiceProvider());
            add(8, () => new SHA256CryptoServiceProvider());
            add(9, () => new SHA384CryptoServiceProvider());
            add(10, () => new SHA512CryptoServiceProvider());
        }

        public static OpenPgpHashAlgorithm GetHashAlgorithmById(int id) {
            return _IdToAlgorithm[id];
        }

        public static OpenPgpHashAlgorithm DefaultHashAlgorithm {
            get { return GetHashAlgorithmById(8); } // SHA-256
        }

        public OpenPgpHashAlgorithm(int hashAlgorithmId, Func<HashAlgorithm> hashAlgorithmFactory) {
            Id = hashAlgorithmId;
            _HashFactory = hashAlgorithmFactory;
        }

        public int Id { get; private set; }

        public byte[] ComputeHash(byte[] salt, SecureString passphrase, int outputSizeNeededInBytes, int octetsToHashCount = 0) {
            salt = salt ?? new byte[0];
            var firstHashContext = _HashFactory();
            var hashSizeInOctets = firstHashContext.HashSize/8;

            var hashContexts = new HashAlgorithm[Math.Max((outputSizeNeededInBytes + hashSizeInOctets - 1)/hashSizeInOctets, 1)];
            hashContexts[0] = firstHashContext;

            for(int i = 1; i < hashContexts.Length; i++) {
                hashContexts[i] = _HashFactory();

                // Per 3.7.1.1:  
                // If the hash size is less than the key size, multiple instances of the 
                // hash context are created -- enough to produce the required key data.
                // These instances are preloaded with 0, 1, 2, ... octets of zeros (that
                // is to say, the first instance has no preloading, the second gets
                // preloaded with 1 octet of zero, the third is preloaded with two
                // octets of zeros, and so forth).
                var zeros = new byte[i];
                hashContexts[i].TransformBlock(zeros, 0, zeros.Length, null, 0);
            }

            IntPtr passphrasePointer = IntPtr.Zero;
            var passphraseChars = new char[passphrase.Length];
            byte[] passphraseBytes = null;
            
            try {
                passphrasePointer = Marshal.SecureStringToBSTR(passphrase);
                Marshal.Copy(passphrasePointer, passphraseChars, 0, passphraseChars.Length);
                
                passphraseBytes = Encoding.UTF8.GetBytes(passphraseChars);

                // Zero-out for paranoia reasons
                for (int i = 0; i < passphraseChars.Length; i++) {
                    passphraseChars[i] = '\0';
                }

                var maxOctetsPerIteration = salt.Length + passphraseBytes.Length;
                bool hasDoneFirstIteration = false;

                for (int octectsLeftToHash = octetsToHashCount; (octectsLeftToHash > 0) || !hasDoneFirstIteration; octectsLeftToHash -= maxOctetsPerIteration) {
                    for (int ixHasher = 0; ixHasher < hashContexts.Length; ixHasher++) {
                        var currentHashContext = hashContexts[ixHasher];
                        int currentContextBytesToHash = hasDoneFirstIteration ? Math.Min(octectsLeftToHash, maxOctetsPerIteration) : maxOctetsPerIteration;
                        int saltBytesToHash = Math.Min(currentContextBytesToHash, salt.Length);
                        currentHashContext.TransformBlock(salt, 0, saltBytesToHash, null, 0);
                        currentContextBytesToHash -= saltBytesToHash;
                        currentHashContext.TransformBlock(passphraseBytes, 0, currentContextBytesToHash, null, 0);
                    }

                    hasDoneFirstIteration = true;
                }
            }
            finally {
                Marshal.ZeroFreeBSTR(passphrasePointer);
                if(passphraseBytes != null) {
                    for (int i = 0; i < passphraseBytes.Length; i++) {
                        passphraseBytes[i] = 0;
                    }
                }
            }

            var ms = new MemoryStream();

            // have to have some output buffer, even though it's never used (see HashAlgorithm source for proof)
            var dummyOutputBuffer = new byte[0];
            foreach(var hashContext in hashContexts) {
                hashContext.TransformFinalBlock(dummyOutputBuffer, 0, 0);
                ms.Write(hashContext.Hash, 0, hashContext.Hash.Length);
            }

            ms.SetLength(outputSizeNeededInBytes);
            return ms.ToArray();
        }
    }

    internal class OpenPgpCfbTransform : ICryptoTransform {
        private readonly ICryptoTransform _UnderlyingTransform;
        private readonly byte[] _FeedbackRegister;
        private readonly byte[] _EncryptedFeedbackRegister;
        private int _CurrentEncryptedFeedbackRegisterPosition;
        private bool _IsEncrypt;
        private byte[] _PrefixBytes;
        private int _PrefixBytesUsed;

        public OpenPgpCfbTransform(SymmetricAlgorithm algorithm, bool encrypt) {
            // We use ECB mode as a primitive since we're effectively doing the CFB mode ourself
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            algorithm.IV = new byte[algorithm.IV.Length];
            int feedbackSizeInBytes = algorithm.BlockSize / 8;
            algorithm.FeedbackSize = algorithm.BlockSize;
            _UnderlyingTransform = algorithm.CreateEncryptor();

            _IsEncrypt = encrypt;
            _FeedbackRegister = new byte[feedbackSizeInBytes];
            _EncryptedFeedbackRegister = new byte[feedbackSizeInBytes];
            _CurrentEncryptedFeedbackRegisterPosition = feedbackSizeInBytes;
            _PrefixBytes = new byte[feedbackSizeInBytes + 2]; // 2 bytes repeated for check

            if(encrypt) {
                var rand = RandomNumberGenerator.Create();
                rand.GetBytes(_PrefixBytes);
                // Repeat the bytes
                _PrefixBytes[_PrefixBytes.Length - 2] = _PrefixBytes[_PrefixBytes.Length - 4];
                _PrefixBytes[_PrefixBytes.Length - 1] = _PrefixBytes[_PrefixBytes.Length - 3];
            }
        }

        public bool CanReuseTransform {
            get { return false; }
        }

        public bool CanTransformMultipleBlocks {
            get { return true; }
        }

        public int InputBlockSize {
            get { return _UnderlyingTransform.InputBlockSize; }
        }

        public int OutputBlockSize {
            get { return _UnderlyingTransform.OutputBlockSize; }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            if(_IsEncrypt) {
                for (int ixEncryptedByte = 0; ixEncryptedByte < inputCount; ixEncryptedByte++) {
                    var nextCiphertextByte = (byte)(GetNextEncryptedFeedbackByte() ^ inputBuffer[inputOffset + ixEncryptedByte]);
                    outputBuffer[outputOffset + ixEncryptedByte] = nextCiphertextByte;
                    _FeedbackRegister[_CurrentEncryptedFeedbackRegisterPosition - 1] = nextCiphertextByte;
                }
            }
            else {
                for (int ixDecryptedByte = 0; ixDecryptedByte < inputCount; ixDecryptedByte++) {
                    var ciphertextByte = inputBuffer[inputOffset + ixDecryptedByte];
                    var encryptedByte = GetNextEncryptedFeedbackByte();
                    outputBuffer[outputOffset + ixDecryptedByte] = (byte)(encryptedByte ^ ciphertextByte);
                    _FeedbackRegister[_CurrentEncryptedFeedbackRegisterPosition - 1] = ciphertextByte;
                }

                if (_PrefixBytesUsed < _PrefixBytes.Length) {
                    int headerBytesRemaining = _PrefixBytes.Length - _PrefixBytesUsed;
                    int headerBytesAvailable = Math.Min(headerBytesRemaining, inputCount);
                    for (int ixHeaderByte = 0; ixHeaderByte < headerBytesAvailable; ixHeaderByte++) {
                        _PrefixBytes[_PrefixBytesUsed++] = outputBuffer[outputOffset + ixHeaderByte];
                    }

                    if (_PrefixBytesUsed == _PrefixBytes.Length) {
                        // do simple check
                        if ((_PrefixBytes[_PrefixBytes.Length - 2] != _PrefixBytes[_PrefixBytes.Length - 4])
                           ||
                           (_PrefixBytes[_PrefixBytes.Length - 1] != _PrefixBytes[_PrefixBytes.Length - 3])) {
                            throw new CryptographicException("Simple check failed");
                        }
                    }

                    int remainingBytesTransformed = inputCount - headerBytesAvailable;

                    int ixOutputEnd = (outputOffset + inputCount) - headerBytesAvailable;
                    for (int ixOutputByte = outputOffset; ixOutputByte < ixOutputEnd; ixOutputByte++) {
                        outputBuffer[ixOutputByte] = outputBuffer[ixOutputByte + headerBytesAvailable];
                    }

                    return remainingBytesTransformed;
                }
            }
            
            return inputCount;
        }

        public byte[] PrefixBytes {
            get { return _PrefixBytes; }
        }

        private byte GetNextEncryptedFeedbackByte() {
            if(_CurrentEncryptedFeedbackRegisterPosition >= _EncryptedFeedbackRegister.Length) {
                if (_UnderlyingTransform.TransformBlock(_FeedbackRegister, 0, _FeedbackRegister.Length, _EncryptedFeedbackRegister, 0) != _EncryptedFeedbackRegister.Length) {
                    throw new CryptographicException();
                }
                _CurrentEncryptedFeedbackRegisterPosition = 0;
            }

            return _EncryptedFeedbackRegister[_CurrentEncryptedFeedbackRegisterPosition++];
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            var result = new byte[inputCount];
            int bytesTransformed = TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
            if(bytesTransformed < inputCount) {
                var actualFinalBlock = new byte[bytesTransformed];
                Buffer.BlockCopy(result, 0, actualFinalBlock, 0, bytesTransformed);
                return actualFinalBlock;
            }
            return result;
        }

        public void Dispose() {
            _UnderlyingTransform.Dispose();
        }
    }
    
    // Section 3.7.1
    internal enum OpenPgpStringToKeySpecifierId {
        Simple = 0,
        Salted = 1,
        IteratedAndSalted = 3
    }
}
