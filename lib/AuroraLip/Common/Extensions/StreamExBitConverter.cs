﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Common
{
    public static partial class StreamEx
    {
        #region Read

        /// <summary>
        /// Returns a 8-bit unsigned integer, read from one byte at the current position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadUInt8(this Stream stream)
            => (byte)stream.ReadByte();

        /// <summary>
        /// Returns a 8-bit signed integer, read from one byte at the current position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadInt8(this Stream stream)
            => (sbyte)stream.ReadByte();

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 16-bit unsigned integer formed by two bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16(this Stream stream, Endian order = Endian.Little)
            => stream.Read<ushort>(order);

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 16-bit signed integer formed by two bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(this Stream stream, Endian order = Endian.Little)
            => stream.Read<short>(order);

        /// <summary>
        /// Returns a 24-bit unsigned integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 24-bit unsigned integer formed by three bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt24 ReadUInt24(this Stream stream, Endian order = Endian.Little)
            => stream.Read<UInt24>(order);

        /// <summary>
        /// Returns a 24-bit signed integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 24-bit signed integer formed by three bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int24 ReadInt24(this Stream stream, Endian order = Endian.Little)
            => stream.Read<Int24>(order);

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 32-bit unsigned integer formed by four bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(this Stream stream, Endian order = Endian.Little)
            => stream.Read<uint>(order);

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 32-bit signed integer formed by four bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(this Stream stream, Endian order = Endian.Little)
            => stream.Read<int>(order);

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 64-bit unsigned integer formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64(this Stream stream, Endian order = Endian.Little)
            => stream.Read<ulong>(order);

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 64-bit signed integer formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(this Stream stream, Endian order = Endian.Little)
            => stream.Read<long>(order);

        /// <summary>
        /// Returns a single-precision floating point number converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A single-precision floating point number formed by from bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadSingle(this Stream stream, Endian order = Endian.Little)
            => stream.Read<float>(order);

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A double-precision floating point number formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDouble(this Stream stream, Endian order = Endian.Little)
            => stream.Read<double>(order);

        #endregion Read

        #region Write

        /// <summary>
        /// Writes the specified 16-bit unsigned integer value as an block of two bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, ushort value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 16-bit signed integer value as an block of two bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, short value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 24-bit unsigned integer value as an block of three bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, UInt24 value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 24-bit signed integer value as an block of three bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, Int24 value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 32-bit unsigned integer value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, uint value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 32-bit signed integer value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, int value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 64-bit unsigned integer value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, ulong value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified 64-bit signed integer value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, long value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified single-precision floating point number value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, float value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        /// <summary>
        /// Writes the specified double-precision floating point number value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, double value, Endian order = Endian.Little)
            => stream.WriteObjekt(value, order);

        #endregion Write

        /// <summary>
        /// Reads the byte order mark (BOM) and returns the endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The endianness represented by the BOM</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Endian ReadBOM(this Stream stream)
            => stream.Read<Endian>();

        /// <summary>
        /// Writes the Byte order mark (BOM) to the Stream
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The endianness represented by the BOM</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, Endian order)
            => stream.WriteObjekt(order);
    }
}
