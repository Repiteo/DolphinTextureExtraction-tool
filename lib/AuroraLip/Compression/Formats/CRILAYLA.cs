﻿using AuroraLib.Common;
using LibCPK;

namespace AuroraLib.Compression.Formats
{
    public class CRILAYLA : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "CRILAYLA";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && stream.MatchString(magic);

        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(CPK.CompressCRILAYLA(source));
        }

        public byte[] Decompress(Stream source)
            => CPK.DecompressLegacyCRI(source.ToArray());
    }
}
