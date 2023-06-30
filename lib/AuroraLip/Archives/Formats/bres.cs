﻿using AuroraLib.Common;
using AuroraLib.Common.Struct;

//https://wiki.tockdom.com/wiki/BRRES_(File_Format)
namespace AuroraLib.Archives.Formats
{
    public class Bres : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("bres");

        #region Fields and Properties

        public Endian ByteOrder { get; set; }

        #endregion Fields and Properties

        public Bres()
        { }

        public Bres(string filename) : base(filename)
        {
        }

        public Bres(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            Header header = new(stream);
            if (header.Magic != _identifier)
                throw new InvalidIdentifierException(header.Magic, _identifier);
            ByteOrder = header.BOM;
            stream.Seek(header.RootOffset, SeekOrigin.Begin);
            //root sections
            if (!stream.MatchString("root"))
                throw new InvalidIdentifierException("root");
            uint RootSize = stream.ReadUInt32(ByteOrder);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            //Index Group
            ReadIndex(stream, (int)(stream.Position + RootSize - 8), Root);

            //is brtex & brplt pair
            if (Root.Items.Count == 1 && Root.ItemExists("Textures(NW4R)"))
            {
                //try to request an external file.
                string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".brplt");
                try
                {
                    reference_stream = FileRequest.Invoke(datname);
                    Bres plt = new(reference_stream, FullPath);
                    foreach (var item in plt.Root.Items)
                    {
                        Root.Items.Add(item.Key, item.Value);
                    }
                }
                catch (Exception)
                { }
            }
        }

        private void ReadIndex(Stream stream, in int EndOfRoot, ArchiveDirectory ParentDirectory)
        {
            //Index Group
            long StartOfGroup = stream.Position;
            uint GroupSize = stream.ReadUInt32(ByteOrder);
            uint Groups = stream.ReadUInt32(ByteOrder);

            IndexGroup[] groups = stream.For((int)Groups + 1, s => s.Read<IndexGroup>(ByteOrder));

            foreach (IndexGroup group in groups)
            {
                if (group.NamePointer != 0)
                {
                    stream.Seek(StartOfGroup + group.NamePointer, SeekOrigin.Begin);
                    string Name = stream.ReadString();

                    if (group.DataPointer != 0)
                    {
                        stream.Seek(StartOfGroup + group.DataPointer, SeekOrigin.Begin);
                        if (StartOfGroup + group.DataPointer >= EndOfRoot)
                        {
                            ArchiveFile Sub = new() { Name = Name, Parent = ParentDirectory, OwnerArchive = this };
                            string Magic = stream.ReadString(4);
                            uint FileSize = stream.ReadUInt32(ByteOrder);
                            stream.Position -= 8;
                            if (Magic != "RASD" && FileSize <= stream.Length - stream.Position)
                            {
                                Sub.FileData = new ArchiveFile.ArchiveFileStream(stream, FileSize) { Parent = Sub };
                                if (ParentDirectory.Items.ContainsKey(Sub.Name))
                                {
                                    for (int n = 1; true; n++)
                                    {
                                        if (!ParentDirectory.Items.ContainsKey($"{Sub.Name}_{n}"))
                                        {
                                            Sub.Name = $"{Sub.Name}_{n}";
                                            break;
                                        }
                                    }
                                }
                                ParentDirectory.Items.Add(Sub.Name, Sub);
                            }
                        }
                        else
                        {
                            ArchiveDirectory Sub = new(this, ParentDirectory) { Name = Name };
                            ReadIndex(stream, EndOfRoot, Sub);
                            if (ParentDirectory.Items.ContainsKey(Sub.Name))
                            {
                                for (int n = 1; true; n++)
                                {
                                    if (!ParentDirectory.Items.ContainsKey($"{Sub.Name}_{n}"))
                                    {
                                        Sub.Name = $"{Sub.Name}_{n}";
                                        break;
                                    }
                                }
                            }
                            ParentDirectory.Items.Add(Sub.Name, Sub);
                        }
                    }
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private Stream reference_stream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                reference_stream?.Dispose();
            }
        }

        public unsafe struct Header
        {
            public Identifier32 Magic;
            public Endian BOM;
            public ushort Version;
            public uint Length;
            public ushort RootOffset;
            public ushort Sections;

            public Header(Stream stream)
            {
                Magic = stream.Read<Identifier32>();
                BOM = stream.ReadBOM();
                Version = stream.ReadUInt16(BOM);
                Length = stream.ReadUInt32(BOM);
                RootOffset = stream.ReadUInt16(BOM);
                Sections = stream.ReadUInt16(BOM);
            }
        }

        public struct IndexGroup
        {
            public ushort GroupID;
            public ushort Unknown;
            public ushort LeftIndex;
            public ushort RightIndex;
            public uint NamePointer;
            public uint DataPointer;
        }
    }
}
