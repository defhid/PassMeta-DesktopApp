/* MIT License

Copyright (c) 2018 differentrain

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace AvaloniaGif.Caching;

/// <summary>
/// Represents the class which provides a implementation of the xxHash64 algorithm.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public sealed class XXHash64 : HashAlgorithm
{
    private const ulong PRIME64_1 = 11400714785074694791UL;
    private const ulong PRIME64_2 = 14029467366897019727UL;
    private const ulong PRIME64_3 = 1609587929392839161UL;
    private const ulong PRIME64_4 = 9650029242287828579UL;
    private const ulong PRIME64_5 = 2870177450012600261UL;

    private static readonly Func<byte[], int, uint> FuncGetLittleEndianUInt32;
    private static readonly Func<byte[], int, ulong> FuncGetLittleEndianUInt64;
    private static readonly Func<ulong, ulong> FuncGetFinalHashUInt64;

    private ulong _seed64;

    private ulong _acc641;
    private ulong _acc642;
    private ulong _acc643;
    private ulong _acc644;
    private ulong _hash64;

    private int _remainingLength;
    private long _totalLength;
    private int _currentIndex;
    private byte[] _currentArray;

    static XXHash64()
    {
        if (BitConverter.IsLittleEndian)
        {
            FuncGetLittleEndianUInt32 = (x, i) =>
            {
                unsafe
                {
                    fixed (byte* array = x)
                    {
                        return *(uint*)(array + i);
                    }
                }
            };
            FuncGetLittleEndianUInt64 = (x, i) =>
            {
                unsafe
                {
                    fixed (byte* array = x)
                    {
                        return *(ulong*)(array + i);
                    }
                }
            };
            FuncGetFinalHashUInt64 = i =>
                (i & 0x00000000000000FFUL) << 56 | (i & 0x000000000000FF00UL) << 40 | (i & 0x0000000000FF0000UL) << 24 |
                (i & 0x00000000FF000000UL) << 8 | (i & 0x000000FF00000000UL) >> 8 | (i & 0x0000FF0000000000UL) >> 24 |
                (i & 0x00FF000000000000UL) >> 40 | (i & 0xFF00000000000000UL) >> 56;
        }
        else
        {
            FuncGetLittleEndianUInt32 = (x, i) =>
            {
                unsafe
                {
                    fixed (byte* array = x)
                    {
                        return (uint)(array[i++] | (array[i++] << 8) | (array[i++] << 16) | (array[i] << 24));
                    }
                }
            };
            FuncGetLittleEndianUInt64 = (x, i) =>
            {
                unsafe
                {
                    fixed (byte* array = x)
                    {
                        return array[i++] | ((ulong)array[i++] << 8) | ((ulong)array[i++] << 16) |
                               ((ulong)array[i++] << 24) | ((ulong)array[i++] << 32) | ((ulong)array[i++] << 40) |
                               ((ulong)array[i++] << 48) | ((ulong)array[i] << 56);
                    }
                }
            };
            FuncGetFinalHashUInt64 = i => i;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XXHash64"/> class by default seed(0).
    /// </summary>
    public XXHash64() => Initialize(0);

    /// <summary>
    /// Gets the <see cref="ulong"/> value of the computed hash code.
    /// </summary>
    /// <exception cref="InvalidOperationException">Computation has not yet completed.</exception>
    public ulong HashUInt64 =>
        State == 0 ? _hash64 : throw new InvalidOperationException("Computation has not yet completed.");

    /// <summary>
    /// Initializes this instance for new hash computing.
    /// </summary>
    public override void Initialize()
    {
        _acc641 = _seed64 + PRIME64_1 + PRIME64_2;
        _acc642 = _seed64 + PRIME64_2;
        _acc643 = _seed64 + 0;
        _acc644 = _seed64 - PRIME64_1;
    }

    /// <summary>
    /// Routes data written to the object into the hash algorithm for computing the hash.
    /// </summary>
    /// <param name="array">The input to compute the hash code for.</param>
    /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
    /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        State = 1;
        var size = cbSize - ibStart;
        _remainingLength = size & 31;
        if (cbSize >= 32)
        {
            var limit = size - _remainingLength;
            do
            {
                _acc641 = Round64(_acc641, FuncGetLittleEndianUInt64(array, ibStart));
                ibStart += 8;
                _acc642 = Round64(_acc642, FuncGetLittleEndianUInt64(array, ibStart));
                ibStart += 8;
                _acc643 = Round64(_acc643, FuncGetLittleEndianUInt64(array, ibStart));
                ibStart += 8;
                _acc644 = Round64(_acc644, FuncGetLittleEndianUInt64(array, ibStart));
                ibStart += 8;
            } while (ibStart < limit);
        }

        _totalLength += cbSize;
        if (_remainingLength != 0)
        {
            _currentArray = array;
            _currentIndex = ibStart;
        }
    }

    /// <summary>
    /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    protected override byte[] HashFinal()
    {
        if (_totalLength >= 32)
        {
            _hash64 = RotateLeft64_1(_acc641) + RotateLeft64_7(_acc642) + RotateLeft64_12(_acc643) +
                      RotateLeft64_18(_acc644);

            _hash64 = MergeRound64(_hash64, _acc641);
            _hash64 = MergeRound64(_hash64, _acc642);
            _hash64 = MergeRound64(_hash64, _acc643);
            _hash64 = MergeRound64(_hash64, _acc644);
        }
        else
        {
            _hash64 = _seed64 + PRIME64_5;
        }

        _hash64 += (ulong)_totalLength;

        while (_remainingLength >= 8)
        {
            _hash64 = RotateLeft64_27(_hash64 ^ Round64(0, FuncGetLittleEndianUInt64(_currentArray, _currentIndex))) *
                PRIME64_1 + PRIME64_4;
            _currentIndex += 8;
            _remainingLength -= 8;
        }

        while (_remainingLength >= 4)
        {
            _hash64 = RotateLeft64_23(_hash64 ^ (FuncGetLittleEndianUInt32(_currentArray, _currentIndex) * PRIME64_1)) *
                PRIME64_2 + PRIME64_3;

            _currentIndex += 4;
            _remainingLength -= 4;
        }

        unsafe
        {
            fixed (byte* arrayPtr = _currentArray)
            {
                while (_remainingLength-- >= 1)
                {
                    _hash64 = RotateLeft64_11(_hash64 ^ (arrayPtr[_currentIndex++] * PRIME64_5)) * PRIME64_1;
                }
            }
        }

        _hash64 = (_hash64 ^ (_hash64 >> 33)) * PRIME64_2;
        _hash64 = (_hash64 ^ (_hash64 >> 29)) * PRIME64_3;
        _hash64 ^= _hash64 >> 32;

        _totalLength = State = 0;
        return BitConverter.GetBytes(FuncGetFinalHashUInt64(_hash64));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MergeRound64(ulong input, ulong value) => (input ^ Round64(0, value)) * PRIME64_1 + PRIME64_4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Round64(ulong input, ulong value) => RotateLeft64_31(input + (value * PRIME64_2)) * PRIME64_1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_1(ulong value) => (value << 1) | (value >> 63); // _ACC64_1

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_7(ulong value) => (value << 7) | (value >> 57); //  _ACC64_2

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_11(ulong value) => (value << 11) | (value >> 53);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_12(ulong value) => (value << 12) | (value >> 52); // _ACC64_3

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_18(ulong value) => (value << 18) | (value >> 46); // _ACC64_4

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_23(ulong value) => (value << 23) | (value >> 41);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_27(ulong value) => (value << 27) | (value >> 37);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64_31(ulong value) => (value << 31) | (value >> 33);

    private void Initialize(ulong seed)
    {
        HashSizeValue = 64;
        _seed64 = seed;
        Initialize();
    }
}