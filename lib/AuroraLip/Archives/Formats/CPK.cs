﻿using AuroraLib.Common;
using AuroraLib.Common.Struct;
using System.Text;

namespace AuroraLib.Archives.Formats
{
    public class CPK : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("CPK ");

        public readonly LibCPK.CPK CpkContent;

        public CPK()
        {
            CpkContent = new LibCPK.CPK();
        }

        public CPK(string filename) : base(filename)
        {
        }

        public CPK(Stream stream, string fullpath = null) : base(stream, fullpath)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            CpkContent.ReadCPK(stream, Encoding.UTF8);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            foreach (var entrie in CpkContent.fileTable)
            {
                if (entrie.FileType != LibCPK.FileTypeFlag.FILE)
                    continue;

                ArchiveDirectory dir;
                if (String.IsNullOrWhiteSpace(entrie.DirName))
                    dir = Root;
                else
                {
                    if (!Root.Items.ContainsKey(entrie.DirName))
                        Root.Items.Add(entrie.DirName, new ArchiveDirectory(this, Root));

                    dir = (ArchiveDirectory)Root.Items[entrie.DirName];
                }

                // important files are available multiple times.
                if (!dir.Items.ContainsKey($"{entrie.ID}{entrie.FileName}"))
                {
                    dir.AddArchiveFile(stream, UInt32.Parse(entrie.FileSize.ToString()), (long)entrie.FileOffset, $"{entrie.ID}{entrie.FileName}");
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
