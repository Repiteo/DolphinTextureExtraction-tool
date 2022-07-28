﻿using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    public class TEX : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Extension => ".tex";

        public TEX() { }

        public TEX(Stream stream) : base(stream) { }

        public TEX(string filepath) : base(filepath) { }

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (!extension.ToLower().StartsWith(".tex"))
                return false;

            uint Tex_Format = stream.ReadUInt32(Endian.Big);

            if (!TEX_ImageFormat.ContainsKey(Tex_Format))
                return false;

            uint ImageWidth = stream.ReadUInt32(Endian.Big);
            uint ImageHeight = stream.ReadUInt32(Endian.Big);
            return ImageWidth > 1 && ImageWidth <= 1024 && ImageHeight >= 1 && ImageHeight <= 1024 && GetCalculatedDataSize(TEX_ImageFormat[Tex_Format], (int)ImageWidth, (int)ImageHeight) < stream.Length;
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, in extension);

        protected override void Read(Stream stream)
        {
            uint Tex_Format = stream.ReadUInt32(Endian.Big);

            GXImageFormat Format = TEX_ImageFormat[Tex_Format];
            uint ImageWidth = stream.ReadUInt32(Endian.Big);
            uint ImageHeight = stream.ReadUInt32(Endian.Big);
            uint Size = stream.ReadUInt32(Endian.Big);
            uint Unknown = stream.ReadUInt32(Endian.Big);
            uint MipMaps = stream.ReadUInt32(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big);
            uint Unknown3 = stream.ReadUInt32(Endian.Big);

            TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, (int)ImageWidth, (int)ImageHeight)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = 0
            };
            Add(current);
        }

        static Dictionary<uint, GXImageFormat> TEX_ImageFormat = new Dictionary<uint, GXImageFormat>
        {
            { 0x00, GXImageFormat.RGBA32 },
            { 0x4b, GXImageFormat.IA8 },
            { 0x4c, GXImageFormat.CMPR }
        };

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}