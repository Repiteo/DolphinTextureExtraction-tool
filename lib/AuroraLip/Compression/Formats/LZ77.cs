﻿using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// Nintendo LZ77 compression algorithm use LZ10
    /// </summary>
    public class LZ77 : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("LZ77");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) && stream.ReadByte() == 16;

        public void Compress(in byte[] source, Stream destination)
        {
            // LZ77 compression can only handle files smaller than 16MB
            if (source.Length > 0xFFFFFF)
            {
                throw new Exception($"{typeof(LZ77)} compression can't be used to compress files larger than {0xFFFFFF:N0} bytes.");
            }
            // Write out the header
            destination.Write(_identifier);
            destination.Write(0x10 | (source.Length << 8));

            LZ10.Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 5;
            int destinationLength = (int)source.ReadUInt24();

            return LZ10.Decompress_ALG(source, destinationLength);
        }
    }
}
