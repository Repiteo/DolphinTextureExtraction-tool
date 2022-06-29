﻿using System;

namespace DolphinTextureExtraction_tool
{
    public enum FileTyp
    { Unknown = -1, Archive, Texture, Audio, Model, Video, Text, Font, Layout, Animation, Executable, Else }


    public class Filetyp : IEquatable<string>, IEquatable<Filetyp>
    {
        public readonly string Extension;

        public readonly Header Header;

        public readonly FileTyp Typ;

        public readonly string Description;

        public Filetyp(string extension, FileTyp typ, string description = "")
        {
            Extension = extension;
            Header = null;
            Typ = typ;
            Description = description;
        }

        public Filetyp(string extension, Header header, FileTyp typ, string description = "")
        {
            Extension = extension;
            Header = header;
            Typ = typ;
            Description = description;
        }

        public string GetFullDescription()
        {
            string FullDescription = "";
            if (Header != null && Header.MagicASKI.Length > 2)
            {
                FullDescription += Header.MagicASKI + " ";
            }

            if (Description != "")
            {
                FullDescription += Description + " ";
            }

            if (Typ != FileTyp.Else)
            {
                FullDescription += Typ.ToString();
            }
            return FullDescription;
        }

        public bool Equals(string Extension)
        {
            return this.Extension.ToLower() == Extension.ToLower();
        }

        public bool Equals(Filetyp other)
        {
            return this.Extension.ToLower() == other.Extension.ToLower() || this.Header.Equals(other);
        }
    }
}
