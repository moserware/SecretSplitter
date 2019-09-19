﻿using System;
using System.Numerics;
using Moserware.Numerics;

namespace Moserware.Security.Cryptography {
    /// <summary>
    /// Diffuses the bits of the number.
    /// </summary>
    public abstract class Diffuser {
        public virtual BigInteger Scramble(BigInteger input, int rawByteLength) {
            return Scramble(input, rawByteLength);
        }

        protected virtual BigInteger Scramble(BigInteger input) {
            return input;
        }

        public virtual BigInteger Unscramble(BigInteger input, int rawByteLength) {
            return Unscramble(input, rawByteLength);
        }

        protected virtual BigInteger Unscramble(BigInteger input) {
            return input;
        }
    }

    /// <summary>
    /// Performs no diffusion (e.g. is a pass-thru)
    /// </summary>
    public class NullDiffuser : Diffuser {
    }

    // NOTE: In order to achieve full compatibility with B. Poettering's "ssss-split" and
    //       "ssss-combine" programs, I (Jeff) had to look at B. Poettering's source code
    //       to see exactly how he diffused data using the XTEA algorithm. I reproduced the
    //       effective results using my own code below, but still had to look at his code. 
    //       I asked for explicit permission from the author (B. Poettering) to release 
    //       my derived code under the MIT license (instead of GPL) and was generously 
    //       granted permission by him. For more license details, see License.txt included
    //       with this code.
    public class XteaDiffuser : Diffuser     {
        private const int InnerRounds = 32;
        private const int OuterRounds = 40;
        private const UInt32 Delta = 0x9E3779B9;
        private const UInt32 DecodeInitialSum = unchecked(InnerRounds * Delta);

        public override BigInteger Scramble(BigInteger input, int rawByteLength) {

            byte[] integerBytes = GetBigIntegerBytesWithLeastSignificantWordFirstUsing16BitMsbFirstWords(input);
            int integerBytesNeededSize = (rawByteLength + 1) / 2 * 2;
            int padLen = integerBytesNeededSize - integerBytes.Length;
            if (padLen > 0) {
                byte[] padded = new byte[integerBytesNeededSize];
                Array.Copy(integerBytes, padded, integerBytes.Length);
                integerBytes = padded;
            }
            if (rawByteLength % 2 == 1) {
                integerBytes[rawByteLength - 1] = integerBytes[rawByteLength];
            }
            for (int i = 0; i < (OuterRounds * rawByteLength); i += 2) {
                EncodeSlice(integerBytes, i, rawByteLength, EncipherBlock);
            }
            if (rawByteLength % 2 == 1) {
                integerBytes[rawByteLength] = integerBytes[rawByteLength - 1];
                integerBytes[rawByteLength - 1] = 0;
            }

            return GetBigIntegerFromLeastSignificantWordsFirstWith16BitMsbFirstWords(integerBytes);
        }

        public override BigInteger Unscramble(BigInteger input, int rawByteLength) {

            byte[] integerBytes = GetBigIntegerBytesWithLeastSignificantWordFirstUsing16BitMsbFirstWords(input);
            int integerBytesNeededSize = (rawByteLength + 1) / 2 * 2;
            int padLen = integerBytesNeededSize - integerBytes.Length;
            if (padLen > 0)
            {
                byte[] padded = new byte[integerBytesNeededSize];
                Array.Copy(integerBytes, padded, integerBytes.Length);
                integerBytes = padded;
            }
            if (rawByteLength % 2 == 1) {
               integerBytes[rawByteLength - 1] = integerBytes[rawByteLength];
            }
            for (int i = (OuterRounds * rawByteLength) - 2; i >= 0; i -= 2) {
                EncodeSlice(integerBytes, i, rawByteLength, DecipherBlock);
            }
            if (rawByteLength % 2 == 1) {
                integerBytes[rawByteLength] = integerBytes[rawByteLength - 1];
                integerBytes[rawByteLength - 1] = 0;
            }

            return GetBigIntegerFromLeastSignificantWordsFirstWith16BitMsbFirstWords(integerBytes);
        }

        // The whole point of the diffuser is to diffuse bits, that's why we'll pick least significant words
        // with most significant word bits. This alone does some diffusion.
        private static byte[] GetBigIntegerBytesWithLeastSignificantWordFirstUsing16BitMsbFirstWords(BigInteger input) {
            byte[] bigEndianBytes = input.ToUnsignedBigEndianBytes();
            bool isOddNumberOfBytes = bigEndianBytes.Length % 2 != 0;
            if (isOddNumberOfBytes) {
                // make sure it's even
                byte[] newBigEndianBytes = new byte[bigEndianBytes.Length + 1];

                // Since it's big endian, we need to shift it right by a byte and fill MSB with zeroes.
                Array.Copy(bigEndianBytes, 0, newBigEndianBytes, 1, bigEndianBytes.Length);
                bigEndianBytes = newBigEndianBytes;
            }

            byte[] result = new byte[bigEndianBytes.Length];

            int ixResultByte = 0;

            for (int ixWord = bigEndianBytes.Length - 2; ixWord >= 0; ixWord -= 2) {
                for (int wordByteOffset = 0; wordByteOffset < 2; wordByteOffset++) {
                    // need to flip individual bytes
                    result[ixResultByte++] = bigEndianBytes[ixWord + wordByteOffset];
                }
            }

            return result;
        }

        private static BigInteger GetBigIntegerFromLeastSignificantWordsFirstWith16BitMsbFirstWords(byte[] wordBytes) {

            byte[] bigEndianBytes = new byte[wordBytes.Length];

            int ixResult = 0;

            for (int ixWord = wordBytes.Length - 2; ixWord >= 0; ixWord -= 2) {
                for (int ixByteInWord = 0; ixByteInWord < 2; ixByteInWord++) {
                    bigEndianBytes[ixResult++] = wordBytes[ixWord + ixByteInWord];
                }
            }

            var result = bigEndianBytes.ToBigIntegerFromBigEndianUnsignedBytes();
            return result;
        }

        private static void EncipherBlock(UInt32[] v) {
            UInt32 sum = 0;

            for (int i = 0; i < InnerRounds; i++) {
                v[0] += (((v[1] << 4) ^ (v[1] >> 5)) + v[1]) ^ sum;
                sum += Delta;
                v[1] += (((v[0] << 4) ^ (v[0] >> 5)) + v[0]) ^ sum;
            }
        }

        private static void DecipherBlock(UInt32[] v) {
            UInt32 sum = DecodeInitialSum;

            for (int i = 0; i < InnerRounds; i++) {
                v[1] -= (((v[0] << 4) ^ (v[0] >> 5)) + v[0]) ^ sum;
                sum -= Delta;
                v[0] -= (((v[1] << 4) ^ (v[1] >> 5)) + v[1]) ^ sum;
            }
        }

        private static void EncodeSlice(byte[] data, int idx, int len, Action<UInt32[]> processBlock) {
            UInt32[] v = new UInt32[2];
            const int wordsPerBlock = 2;

            // Pack
            for (int i = 0; i < wordsPerBlock; i++) {
                v[i] = ((UInt32)data[(idx + (4 * i)) % len]) << 24 |
                       ((UInt32)data[(idx + (4 * i) + 1) % len]) << 16 |
                       ((UInt32)data[(idx + (4 * i) + 2) % len]) << 8 |
                       ((UInt32)data[(idx + (4 * i) + 3) % len]);
            }

            // Process
            processBlock(v);

            // Unpack
            for (int i = 0; i < wordsPerBlock; i++) {
                data[(idx + (4 * i) + 0) % len] = (byte)(v[i] >> 24);
                data[(idx + (4 * i) + 1) % len] = (byte)((v[i] >> 16) & 0xff);
                data[(idx + (4 * i) + 2) % len] = (byte)((v[i] >> 8) & 0xff);
                data[(idx + (4 * i) + 3) % len] = (byte)(v[i] & 0xff);
            }
        }
    }

    // Mimics ssss-split by only diffusing if 64 bits or larger
    public class SsssDiffuser : Diffuser {
        private static readonly XteaDiffuser _XteaDiffuser = new XteaDiffuser();
        private const int _ByteCutoff = 64/8;

        public override BigInteger Scramble(BigInteger input, int rawByteLength) {
            if(rawByteLength < _ByteCutoff) {
                return input;
            }

            return _XteaDiffuser.Scramble(input, rawByteLength);
        }

        public override BigInteger  Unscramble(BigInteger input, int rawByteLength) {
            if(rawByteLength < _ByteCutoff) {
                return input;
            }

            return _XteaDiffuser.Unscramble(input, rawByteLength);
        }
    }
}