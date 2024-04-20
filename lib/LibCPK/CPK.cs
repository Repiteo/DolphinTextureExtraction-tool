﻿using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace LibCPK
{
    public class CPK
    {
        public List<FileEntry> fileTable = new List<FileEntry>();
        public Dictionary<string, object> cpkdata = new();
        public UTF utf = new();
        UTF files = new();

        public CPK()
        {
            isUtfEncrypted = false;
        }

        public void ReadCPK(string sPath, Encoding? encoding = null)
        {
            using (Stream stream = File.OpenRead(sPath))
                ReadCPK(stream, encoding);
        }

        public void ReadCPK(Stream stream, Encoding? encoding = null)
        {
            uint Files;
            ushort Align;

            EndianReader br = new EndianReader(stream, true);
            MemoryStream ms;
            EndianReader utfr;

            if (Tools.ReadCString(br, 4) != "CPK ")
            {
                br.Close();
                throw new Exception($"Invalid Identifier, Expected \"CPK \"");
            }

            ReadUTFData(br);

            CPK_packet = utf_packet;

            FileEntry CPAK_entry = new FileEntry
            {
                FileName = "CPK_HDR",
                FileOffsetPos = br.BaseStream.Position + 0x10,
                FileSize = CPK_packet.Length,
                Encrypted = isUtfEncrypted,
                FileType = FileTypeFlag.CPK
            };

            fileTable.Add(CPAK_entry);

            ms = new MemoryStream(utf_packet);
            utfr = new EndianReader(ms, false);

            utf.Clear();
            if (!utf.ReadUTF(utfr, encoding))
            {
                br.Close();
                throw new Exception($"UTF Error utfr:{utfr}, encoding:{encoding}");
            }

            utfr.Close();
            ms.Close();

            cpkdata.Clear();

            try
            {
                for (int i = 0; i < utf.columns.Count; i++)
                {
                    cpkdata.Add(utf.columns[i].name, utf.rows[0].rows[i].GetValue());
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Debug.Print(ex.ToString());
            }

            TocOffset = (ulong)GetColumsData(utf, 0, "TocOffset", E_ColumnDataType.DATA_TYPE_UINT64);
            long TocOffsetPos = GetColumnPostion(utf, 0, "TocOffset");

            EtocOffset = (ulong)GetColumsData(utf, 0, "EtocOffset", E_ColumnDataType.DATA_TYPE_UINT64);
            long ETocOffsetPos = GetColumnPostion(utf, 0, "EtocOffset");

            ItocOffset = (ulong)GetColumsData(utf, 0, "ItocOffset", E_ColumnDataType.DATA_TYPE_UINT64);
            long ITocOffsetPos = GetColumnPostion(utf, 0, "ItocOffset");

            GtocOffset = (ulong)GetColumsData(utf, 0, "GtocOffset", E_ColumnDataType.DATA_TYPE_UINT64);
            long GTocOffsetPos = GetColumnPostion(utf, 0, "GtocOffset");

            ContentOffset = (ulong)GetColumsData(utf, 0, "ContentOffset", E_ColumnDataType.DATA_TYPE_UINT64);
            long ContentOffsetPos = GetColumnPostion(utf, 0, "ContentOffset");
            fileTable.Add(CreateFileEntry("CONTENT_OFFSET", ContentOffset, typeof(ulong), ContentOffsetPos, TOCFlag.CPK, FileTypeFlag.CONTENT, false));

            Files = (uint)GetColumsData(utf, 0, "Files", E_ColumnDataType.DATA_TYPE_UINT32);
            Align = (ushort)GetColumsData(utf, 0, "Align", E_ColumnDataType.DATA_TYPE_USHORT);

            if (TocOffset != 0xFFFFFFFFFFFFFFFF)
            {
                FileEntry entry = CreateFileEntry("TOC_HDR", TocOffset, typeof(ulong), TocOffsetPos, TOCFlag.CPK, FileTypeFlag.HDR, false);
                fileTable.Add(entry);

                if (!ReadTOC(br, TocOffset, ContentOffset, encoding))
                    throw new Exception($"TOC Error TocOffset:{TocOffset}, ContentOffset:{ContentOffset}");
            }

            if (EtocOffset != 0xFFFFFFFFFFFFFFFF)
            {
                FileEntry entry = CreateFileEntry("ETOC_HDR", EtocOffset, typeof(ulong), ETocOffsetPos, TOCFlag.CPK, FileTypeFlag.HDR, false);
                fileTable.Add(entry);

                if (!ReadETOC(br, EtocOffset))
                    throw new Exception($"ETOC Error br:{br}, EtocOffset:{EtocOffset}");
            }

            if (ItocOffset != 0xFFFFFFFFFFFFFFFF)
            {
                FileEntry entry = CreateFileEntry("ITOC_HDR", ItocOffset, typeof(ulong), ITocOffsetPos, TOCFlag.CPK, FileTypeFlag.HDR, false);
                fileTable.Add(entry);

                if (!ReadITOC(br, ItocOffset, ContentOffset, Align))
                    throw new Exception($"ITOC Error br:{br}, ItocOffset:{ItocOffset}, ContentOffset:{ContentOffset}, Align:{Align}");
            }

            if (GtocOffset != 0xFFFFFFFFFFFFFFFF)
            {
                FileEntry entry = CreateFileEntry("GTOC_HDR", GtocOffset, typeof(ulong), GTocOffsetPos, TOCFlag.CPK, FileTypeFlag.HDR, false);
                fileTable.Add(entry);

                if (!ReadGTOC(br, GtocOffset))
                    throw new Exception($"GTOC Error br:{br}, GtocOffset:{GtocOffset}");
            }
            // at this point, we should have all needed file info

            //utf = null;
        }


        FileEntry CreateFileEntry(string FileName, ulong FileOffset, Type FileOffsetType, long FileOffsetPos, TOCFlag TOCName, FileTypeFlag FileType, bool encrypted)
        {
            FileEntry entry = new FileEntry
            {
                FileName = FileName,
                FileOffset = FileOffset,
                FileOffsetType = FileOffsetType,
                FileOffsetPos = FileOffsetPos,
                TOCName = TOCName,
                FileType = FileType,
                Encrypted = encrypted,
                Offset = 0,
            };

            return entry;
        }

        public bool ReadTOC(EndianReader br, ulong TocOffset, ulong ContentOffset, Encoding? encoding = null)
        {
            ulong fTocOffset = TocOffset;
            ulong add_offset = 0;

            if (fTocOffset > (ulong)0x800)
                fTocOffset = (ulong)0x800;


            if (ContentOffset < 0)
                add_offset = fTocOffset;
            else
            {
                if (TocOffset < 0)
                    add_offset = ContentOffset;
                else
                {
                    if (ContentOffset < fTocOffset)
                        add_offset = ContentOffset;
                    else
                        add_offset = fTocOffset;
                }
            }

            br.BaseStream.Seek((long)TocOffset, SeekOrigin.Begin);

            if (Tools.ReadCString(br, 4) != "TOC ")
            {
                br.Close();
                return false;
            }

            ReadUTFData(br);

            // Store unencrypted TOC
            TOC_packet = utf_packet;

            FileEntry toc_entry = fileTable.Where(x => x.FileName.ToString() == "TOC_HDR").Single();
            toc_entry.Encrypted = isUtfEncrypted;
            toc_entry.FileSize = TOC_packet.Length;

            MemoryStream ms = new MemoryStream(utf_packet);
            EndianReader utfr = new EndianReader(ms, false);

            files.Clear();
            if (!files.ReadUTF(utfr, encoding))
            {
                br.Close();
                return false;
            }

            utfr.Close();
            ms.Close();

            FileEntry temp;
            for (int i = 0; i < files.num_rows; i++)
            {
                temp = new FileEntry();

                temp.TOCName = TOCFlag.TOC;

                temp.DirName = (string)GetColumnData(files, i, "DirName");
                temp.FileName = (string)GetColumnData(files, i, "FileName");

                temp.FileSize = GetColumnData(files, i, "FileSize");
                temp.FileSizePos = GetColumnPostion(files, i, "FileSize");
                temp.FileSizeType = GetColumnType(files, i, "FileSize");

                temp.ExtractSize = GetColumnData(files, i, "ExtractSize");
                temp.ExtractSizePos = GetColumnPostion(files, i, "ExtractSize");
                temp.ExtractSizeType = GetColumnType(files, i, "ExtractSize");

                temp.FileOffset = ((ulong)GetColumnData(files, i, "FileOffset") + (ulong)add_offset);
                temp.FileOffsetPos = GetColumnPostion(files, i, "FileOffset");
                temp.FileOffsetType = GetColumnType(files, i, "FileOffset");

                temp.FileType = FileTypeFlag.FILE;

                temp.Offset = add_offset;

                temp.ID = GetColumnData(files, i, "ID");
                temp.UserString = (string)GetColumnData(files, i, "UserString");

                fileTable.Add(temp);
            }

            return true;
        }

        public void WriteCPK(BinaryWriter cpk)
        {
            WritePacket(cpk, "CPK ", 0, CPK_packet);

            cpk.BaseStream.Seek(0x800 - 6, SeekOrigin.Begin);
            cpk.Write(Encoding.ASCII.GetBytes("(c)CRI"));
            if ((TocOffset > 0x800) && TocOffset < 0x8000)
            {
                //部分cpk是从0x2000开始TOC，所以
                //需要计算 cpk padding
                cpk.Write(new byte[TocOffset - 0x800]);
            }
        }

        public void WriteTOC(BinaryWriter cpk)
        {
            WritePacket(cpk, "TOC ", TocOffset, TOC_packet);
        }

        public void WriteITOC(BinaryWriter cpk)
        {
            WritePacket(cpk, "ITOC", ItocOffset, ITOC_packet);
        }

        public void WriteETOC(BinaryWriter cpk)
        {
            WritePacket(cpk, "ETOC", EtocOffset, ETOC_packet);
        }

        public void WriteGTOC(BinaryWriter cpk)
        {
            WritePacket(cpk, "GTOC", GtocOffset, GTOC_packet);
        }

        public void WritePacket(BinaryWriter cpk, string ID, ulong position, byte[] packet)
        {
            if (position != 0xffffffffffffffff)
            {
                cpk.BaseStream.Seek((long)position, SeekOrigin.Begin);
                byte[] encrypted;
                if (isUtfEncrypted == true)
                {
                    encrypted = DecryptUTF(packet); // Yes it says decrypt...
                }
                else
                {
                    encrypted = packet;
                }
                if (encrypted != null)
                {
                    cpk.Write(Encoding.ASCII.GetBytes(ID));
                    cpk.Write((Int32)0xff);
                    cpk.Write((UInt64)encrypted.Length);
                    cpk.Write(encrypted);
                }
            }
        }

        public bool ReadITOC(EndianReader br, ulong startoffset, ulong ContentOffset, ushort Align)
        {
            br.BaseStream.Seek((long)startoffset, SeekOrigin.Begin);

            if (Tools.ReadCString(br, 4) != "ITOC")
            {
                br.Close();
                return false;
            }

            ReadUTFData(br);

            ITOC_packet = utf_packet;

            FileEntry itoc_entry = fileTable.Where(x => x.FileName.ToString() == "ITOC_HDR").Single();
            itoc_entry.Encrypted = isUtfEncrypted;
            itoc_entry.FileSize = ITOC_packet.Length;

            MemoryStream ms = new MemoryStream(utf_packet);
            EndianReader utfr = new EndianReader(ms, false);

            files.Clear();
            if (!files.ReadUTF(utfr))
            {
                br.Close();
                return false;
            }

            utfr.Close();
            ms.Close();

            byte[] DataL = (byte[])GetColumnData(files, 0, "DataL");
            long DataLPos = GetColumnPostion(files, 0, "DataL");

            byte[] DataH = (byte[])GetColumnData(files, 0, "DataH");
            long DataHPos = GetColumnPostion(files, 0, "DataH");

            //MemoryStream ms;
            //EndianReader ir;
            Dictionary<int, uint> SizeTable, CSizeTable;
            Dictionary<int, long> SizePosTable, CSizePosTable;
            Dictionary<int, Type> SizeTypeTable, CSizeTypeTable;

            List<int> IDs = new List<int>();

            SizeTable = new Dictionary<int, uint>();
            SizePosTable = new Dictionary<int, long>();
            SizeTypeTable = new Dictionary<int, Type>();

            CSizeTable = new Dictionary<int, uint>();
            CSizePosTable = new Dictionary<int, long>();
            CSizeTypeTable = new Dictionary<int, Type>();

            ushort ID, size1;
            uint size2;
            long pos;
            Type type;

            if (DataL != null)
            {
                ms = new MemoryStream(DataL);
                utfr = new EndianReader(ms, false);
                UTF utfDataL = new();
                utfDataL.ReadUTF(utfr);

                for (int i = 0; i < utfDataL.num_rows; i++)
                {
                    ID = (ushort)GetColumnData(utfDataL, i, "ID");
                    size1 = (ushort)GetColumnData(utfDataL, i, "FileSize");
                    SizeTable.Add((int)ID, (uint)size1);

                    pos = GetColumnPostion(utfDataL, i, "FileSize");
                    SizePosTable.Add((int)ID, pos + DataLPos);

                    type = GetColumnType(utfDataL, i, "FileSize");
                    SizeTypeTable.Add((int)ID, type);

                    if ((GetColumnData(utfDataL, i, "ExtractSize")) != null)
                    {
                        size1 = (ushort)GetColumnData(utfDataL, i, "ExtractSize");
                        CSizeTable.Add((int)ID, (uint)size1);

                        pos = GetColumnPostion(utfDataL, i, "ExtractSize");
                        CSizePosTable.Add((int)ID, pos + DataLPos);

                        type = GetColumnType(utfDataL, i, "ExtractSize");
                        CSizeTypeTable.Add((int)ID, type);
                    }

                    IDs.Add(ID);
                }
            }

            if (DataH != null)
            {
                ms = new MemoryStream(DataH);
                utfr = new EndianReader(ms, false);
                UTF utfDataH = new();
                utfDataH.ReadUTF(utfr);

                for (int i = 0; i < utfDataH.num_rows; i++)
                {
                    ID = (ushort)GetColumnData(utfDataH, i, "ID");
                    size2 = (uint)GetColumnData(utfDataH, i, "FileSize");
                    SizeTable.Add(ID, size2);

                    pos = GetColumnPostion(utfDataH, i, "FileSize");
                    SizePosTable.Add((int)ID, pos + DataHPos);

                    type = GetColumnType(utfDataH, i, "FileSize");
                    SizeTypeTable.Add((int)ID, type);

                    if ((GetColumnData(utfDataH, i, "ExtractSize")) != null)
                    {
                        size2 = (uint)GetColumnData(utfDataH, i, "ExtractSize");
                        CSizeTable.Add(ID, size2);

                        pos = GetColumnPostion(utfDataH, i, "ExtractSize");
                        CSizePosTable.Add((int)ID, pos + DataHPos);

                        type = GetColumnType(utfDataH, i, "ExtractSize");
                        CSizeTypeTable.Add((int)ID, type);
                    }

                    IDs.Add(ID);
                }
            }

            FileEntry temp;
            //int id = 0;
            uint value = 0, value2 = 0;
            ulong baseoffset = ContentOffset;

            // Seems ITOC can mix up the IDs..... but they'll alwaysy be in order...
            IDs = IDs.OrderBy(x => x).ToList();


            for (int i = 0; i < IDs.Count; i++)
            {
                int id = IDs[i];

                temp = new FileEntry();
                SizeTable.TryGetValue(id, out value);
                CSizeTable.TryGetValue(id, out value2);

                temp.TOCName = TOCFlag.ITOC;

                temp.DirName = string.Empty;
                temp.FileName = id.ToString() + ".bin";

                temp.FileSize = value;
                temp.FileSizePos = SizePosTable[id];
                temp.FileSizeType = SizeTypeTable[id];

                if (CSizeTable.Count > 0 && CSizeTable.ContainsKey(id))
                {
                    temp.ExtractSize = value2;
                    temp.ExtractSizePos = CSizePosTable[id];
                    temp.ExtractSizeType = CSizeTypeTable[id];
                }

                temp.FileType = FileTypeFlag.FILE;


                temp.FileOffset = baseoffset;
                temp.ID = id;
                temp.UserString = string.Empty;

                fileTable.Add(temp);

                if ((value % Align) > 0)
                    baseoffset += value + (Align - (value % Align));
                else
                    baseoffset += value;


                //id++;
            }

            ms.Close();
            utfr.Close();


            return true;
        }

        private void ReadUTFData(EndianReader br)
        {
            isUtfEncrypted = false;
            br.IsLittleEndian = true;

            unk1 = br.ReadInt32();
            utf_size = br.ReadInt64();
            utf_packet = br.ReadBytes((int)utf_size);

            if (utf_packet[0] != 0x40 && utf_packet[1] != 0x55 && utf_packet[2] != 0x54 && utf_packet[3] != 0x46) //@UTF
            {
                utf_packet = DecryptUTF(utf_packet);
                isUtfEncrypted = true;
            }

            br.IsLittleEndian = false;
        }

        public bool ReadGTOC(EndianReader br, ulong startoffset)
        {
            br.BaseStream.Seek((long)startoffset, SeekOrigin.Begin);

            if (Tools.ReadCString(br, 4) != "GTOC")
            {
                br.Close();
                return false;
            }

            ReadUTFData(br);

            GTOC_packet = utf_packet;
            FileEntry gtoc_entry = fileTable.Where(x => x.FileName.ToString() == "GTOC_HDR").Single();
            gtoc_entry.Encrypted = isUtfEncrypted;
            gtoc_entry.FileSize = GTOC_packet.Length;


            return true;
        }

        public bool ReadETOC(EndianReader br, ulong startoffset)
        {
            br.BaseStream.Seek((long)startoffset, SeekOrigin.Begin);

            if (Tools.ReadCString(br, 4) != "ETOC")
            {
                br.Close();
                return false;
            }

            //br.BaseStream.Seek(0xC, SeekOrigin.Current); //skip header data

            ReadUTFData(br);

            ETOC_packet = utf_packet;

            FileEntry etoc_entry = fileTable.Where(x => x.FileName.ToString() == "ETOC_HDR").Single();
            etoc_entry.Encrypted = isUtfEncrypted;
            etoc_entry.FileSize = ETOC_packet.Length;

            MemoryStream ms = new MemoryStream(utf_packet);
            EndianReader utfr = new EndianReader(ms, false);

            files = new UTF();
            if (!files.ReadUTF(utfr))
            {
                br.Close();
                return false;
            }

            utfr.Close();
            ms.Close();

            List<FileEntry> fileEntries = fileTable.Where(x => x.FileType == FileTypeFlag.FILE).ToList();

            for (int i = 0; i < fileEntries.Count; i++)
            {
                fileTable[i].LocalDir = (string)GetColumnData(files, i, "LocalDir");
                var tUpdateDateTime = GetColumnData(files, i, "UpdateDateTime");
                if (tUpdateDateTime == null) tUpdateDateTime = 0;
                fileTable[i].UpdateDateTime = (ulong)tUpdateDateTime;
            }

            return true;
        }

        public byte[] DecryptUTF(byte[] input)
        {
            byte[] result = new byte[input.Length];

            int m, t;
            byte d;

            m = 0x0000655f;
            t = 0x00004115;

            for (int i = 0; i < input.Length; i++)
            {
                d = input[i];
                d = (byte)(d ^ (byte)(m & 0xff));
                result[i] = d;
                m *= t;
            }

            return result;
        }

        public enum E_ColumnDataType
        {
            DATA_TYPE_BYTE = 0,
            DATA_TYPE_USHORT = 1,
            DATA_TYPE_UINT32 = 2,
            DATA_TYPE_UINT64 = 3,
        }

        public object GetColumsData(UTF utf, int row, string Name, E_ColumnDataType type)
        {
            object? Temp;

            try
            {
                Temp = GetColumnData(utf, row, Name);
                if (Temp is ulong)
                {
                    return (ulong)Temp;
                }
                if (Temp is uint)
                {
                    return (uint)Temp;
                }
                if (Temp is ushort)
                {
                    return (ushort)Temp;
                }
                if (Temp is byte)
                {
                    return (byte)Temp;
                }
            }

            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                switch (type)
                {
                    case E_ColumnDataType.DATA_TYPE_BYTE: // byte
                        return (byte)0xFF;
                    case E_ColumnDataType.DATA_TYPE_USHORT: // short
                        return (ushort)0xFFFF;
                    case E_ColumnDataType.DATA_TYPE_UINT32: // int
                        return 0xFFFFFFFF;
                    case E_ColumnDataType.DATA_TYPE_UINT64: // long
                        return 0xFFFFFFFFFFFFFFFF;
                }
            }

            return 0;
        }

        private object GetColumnData(UTF utf, int row, string pName)
        {
            for (int i = 0; i < utf.num_columns; i++)
            {
                int storageFlag = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.STORAGE_MASK;
                int columnType = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.TYPE_MASK;
                if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_CONSTANT)
                {
                    if (utf.columns[i].name == pName)
                    {
                        return utf.columns[i].GetValue();
                    }
                }

                if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_NONE || storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_ZERO)
                {
                    continue;
                }

                if (utf.columns[i].name == pName)
                {
                    return utf.rows[row].rows[i].GetValue();
                }
            }

            throw new KeyNotFoundException();
        }

        public long GetColumnPostion(UTF utf, int row, string pName)
        {
            long result = -1;

            try
            {
                for (int i = 0; i < utf.num_columns; i++)
                {
                    int storageFlag = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.STORAGE_MASK;
                    int columnType = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.TYPE_MASK;
                    if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_CONSTANT)
                    {
                        if (utf.columns[i].name == pName)
                        {
                            result = utf.columns[i].position;
                            break;
                        }
                    }

                    if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_NONE || storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_ZERO)
                    {
                        continue;
                    }

                    if (utf.columns[i].name == pName)
                    {
                        result = utf.rows[row].rows[i].position;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return -1;
            }

            return result;
        }

        public Type GetColumnType(UTF utf, int row, string pName)
        {
            for (int i = 0; i < utf.num_columns; i++)
            {
                int storageFlag = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.STORAGE_MASK;
                int columnType = utf.columns[i].flags & (int)UTF.COLUMN_FLAGS.TYPE_MASK;
                if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_CONSTANT)
                {
                    if (utf.columns[i].name == pName)
                    {
                        return utf.columns[i].GetType();
                    }
                }

                if (storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_NONE || storageFlag == (int)UTF.COLUMN_FLAGS.STORAGE_ZERO)
                {
                    continue;
                }
                if (utf.columns[i].name == pName)
                {
                    return utf.rows[row].rows[i].GetType();
                }
            }

            throw new KeyNotFoundException();
        }

        public void UpdateFileEntry(FileEntry fileEntry)
        {
            if (fileEntry.FileType == FileTypeFlag.FILE || fileEntry.FileType == FileTypeFlag.HDR)
            {
                byte[] updateMe = Array.Empty<byte>();
                switch (fileEntry.TOCName)
                {
                    case TOCFlag.CPK:
                        updateMe = CPK_packet;
                        break;
                    case TOCFlag.TOC:
                        updateMe = TOC_packet;
                        break;
                    case TOCFlag.ITOC:
                        updateMe = ITOC_packet;
                        break;
                    case TOCFlag.ETOC:
                        updateMe = ETOC_packet;
                        break;
                    case TOCFlag.GTOC:
                        updateMe = GTOC_packet;
                        break;
                    default:
                        break;
                }


                //Update ExtractSize
                if (fileEntry.ExtractSizePos > 0)
                    UpdateValue(ref updateMe, fileEntry.ExtractSize, fileEntry.ExtractSizePos, fileEntry.ExtractSizeType);

                //Update FileSize
                if (fileEntry.FileSizePos > 0)
                    UpdateValue(ref updateMe, fileEntry.FileSize, fileEntry.FileSizePos, fileEntry.FileSizeType);

                //Update FileOffset
                if (fileEntry.FileOffsetPos > 0)
                    if (fileEntry.TOCName == TOCFlag.TOC && fileEntry.FileType == FileTypeFlag.FILE)
                    {
                        UpdateValue(ref updateMe, fileEntry.FileOffset - (ulong)fileEntry.Offset, fileEntry.FileOffsetPos, fileEntry.FileOffsetType);
                    }
                    else
                    {
                        UpdateValue(ref updateMe, fileEntry.FileOffset, fileEntry.FileOffsetPos, fileEntry.FileOffsetType);
                    }

                switch (fileEntry.TOCName)
                {
                    case TOCFlag.CPK:
                        CPK_packet = updateMe;
                        break;
                    case TOCFlag.TOC:
                        TOC_packet = updateMe;
                        break;
                    case TOCFlag.ITOC:
                        ITOC_packet = updateMe;
                        break;
                    case TOCFlag.ETOC:
                        updateMe = ETOC_packet;
                        break;
                    case TOCFlag.GTOC:
                        updateMe = GTOC_packet;
                        break;
                    default:
                        break;

                }
            }
        }

        public void UpdateValue(ref byte[] packet, object value, long pos, Type type)
        {
            MemoryStream temp = new MemoryStream();
            temp.Write(packet, 0, packet.Length);

            EndianWriter toc = new EndianWriter(temp, false);
            toc.Seek((int)pos, SeekOrigin.Begin);

            value = Convert.ChangeType(value, type);

            if (type == typeof(Byte))
            {
                toc.Write((Byte)value);
            }
            else if (type == typeof(UInt16))
            {
                toc.Write((UInt16)value);
            }
            else if (type == typeof(UInt32))
            {
                toc.Write((UInt32)value);
            }
            else if (type == typeof(UInt64))
            {
                toc.Write((UInt64)value);
            }
            else if (type == typeof(Single))
            {
                toc.Write((Single)value);
            }
            else
            {
                throw new Exception("Not supported type!");
            }

            toc.Close();

            MemoryStream myStream = (MemoryStream)toc.BaseStream;
            packet = myStream.ToArray();

        }

        public bool isUtfEncrypted { get; set; }
        public int unk1 { get; set; }
        public long utf_size { get; set; }
        public byte[] utf_packet { get; set; } = Array.Empty<byte>();

        public byte[] CPK_packet { get; set; } = Array.Empty<byte>();
        public byte[] TOC_packet { get; set; } = Array.Empty<byte>();
        public byte[] ITOC_packet { get; set; } = Array.Empty<byte>();
        public byte[] ETOC_packet { get; set; } = Array.Empty<byte>();
        public byte[] GTOC_packet { get; set; } = Array.Empty<byte>();

        public ulong TocOffset, EtocOffset, ItocOffset, GtocOffset, ContentOffset;
    }


    [Flags]
    public enum E_StructTypes : int
    {
        DATA_TYPE_UINT8 = 0,
        DATA_TYPE_UINT8_1 = 1,
        DATA_TYPE_UINT16 = 2,
        DATA_TYPE_UINT16_1 = 3,
        DATA_TYPE_UINT32 = 4,
        DATA_TYPE_UINT32_1 = 5,
        DATA_TYPE_UINT64 = 6,
        DATA_TYPE_UINT64_1 = 7,
        DATA_TYPE_FLOAT = 8,
        DATA_TYPE_STRING = 0xA,
        DATA_TYPE_BYTEARRAY = 0xB,
        DATA_TYPE_MASK = 0xf,
        DATA_TYPE_NONE = -1,
    }

    public class UTF
    {
        public enum COLUMN_FLAGS : int
        {
            STORAGE_MASK = 0xf0,
            STORAGE_NONE = 0x00,
            STORAGE_ZERO = 0x10,
            STORAGE_CONSTANT = 0x30,
            STORAGE_PERROW = 0x50,

            TYPE_MASK = 0x0f,
        }



        public List<COLUMN> columns = new();
        public List<ROWS> rows = new();

        public void Clear()
        {
            columns.Clear();
            rows.Clear();
            table_size = 0;
            rows_offset = 0;
            strings_offset = 0;
            data_offset = 0;
            table_name = 0;
            num_columns = 0;
            row_length = 0;
            num_rows = 0;
        }


        public bool ReadUTF(EndianReader br, Encoding? encoding = null)
        {
            long offset = br.BaseStream.Position;

            if (Tools.ReadCString(br, 4) != "@UTF")
            {
                return false;
            }

            table_size = br.ReadInt32();
            rows_offset = br.ReadInt32();
            strings_offset = br.ReadInt32();
            data_offset = br.ReadInt32();

            // CPK Header & UTF Header are ignored, so add 8 to each offset
            rows_offset += (offset + 8);
            strings_offset += (offset + 8);
            data_offset += (offset + 8);

            table_name = br.ReadInt32();
            num_columns = br.ReadInt16();
            row_length = br.ReadInt16();
            num_rows = br.ReadInt32();

            //read Columns
            columns = new List<COLUMN>();
            COLUMN column;

            for (int i = 0; i < num_columns; i++)
            {
                column = new COLUMN();
                column.flags = br.ReadByte();
                if (column.flags == 0)
                {
                    br.BaseStream.Seek(3, SeekOrigin.Current);
                    column.flags = br.ReadByte();
                }

                column.name = Tools.ReadCString(br, -1, (long)(br.ReadInt32() + strings_offset), encoding);
                if ((column.flags & (int)UTF.COLUMN_FLAGS.STORAGE_MASK) == (int)UTF.COLUMN_FLAGS.STORAGE_CONSTANT)
                {
                    column.UpdateTypedData(br, column.flags, strings_offset, data_offset, encoding);
                }
                columns.Add(column);
            }

            //read Rows

            rows = new List<ROWS>();
            ROWS current_entry;
            ROW current_row;
            int storage_flag;

            for (int j = 0; j < num_rows; j++)
            {
                br.BaseStream.Seek(rows_offset + (j * row_length), SeekOrigin.Begin);

                current_entry = new ROWS();

                for (int i = 0; i < num_columns; i++)
                {
                    current_row = new ROW();

                    storage_flag = (columns[i].flags & (int)COLUMN_FLAGS.STORAGE_MASK);

                    if (storage_flag == (int)COLUMN_FLAGS.STORAGE_NONE) // 0x00
                    {
                        current_entry.rows.Add(current_row);
                        continue;
                    }

                    if (storage_flag == (int)COLUMN_FLAGS.STORAGE_ZERO) // 0x10
                    {
                        current_entry.rows.Add(current_row);
                        continue;
                    }

                    if (storage_flag == (int)COLUMN_FLAGS.STORAGE_CONSTANT) // 0x30
                    {
                        current_entry.rows.Add(current_row);
                        continue;
                    }
                    if (storage_flag == (int)COLUMN_FLAGS.STORAGE_PERROW)
                    {
                        // 0x50

                        current_row.type = columns[i].flags & (int)COLUMN_FLAGS.TYPE_MASK;

                        current_row.position = br.BaseStream.Position;

                        current_row.UpdateTypedData(br, columns[i].flags, strings_offset, data_offset, encoding);

                        current_entry.rows.Add(current_row);
                    }
                }

                rows.Add(current_entry);
            }

            return true;
        }

        public int table_size { get; set; }

        public long rows_offset { get; set; }
        public long strings_offset { get; set; }
        public long data_offset { get; set; }
        public int table_name { get; set; }
        public short num_columns { get; set; }
        public short row_length { get; set; }
        public int num_rows { get; set; }
    }


    public class COLUMN : TypeData
    {
        public COLUMN()
        {
        }


        public byte flags { get; set; }
        public string name { get; set; } = string.Empty;


    }


    public class ROWS
    {
        public List<ROW> rows;

        public ROWS()
        {
            rows = new List<ROW>();
        }
    }

    public abstract class TypeData
    {
        public int type = -1;
        public object GetValue()
        {
            switch (this.type)
            {
                case (int)E_StructTypes.DATA_TYPE_UINT8:
                case (int)E_StructTypes.DATA_TYPE_UINT8_1: return this.uint8;

                case (int)E_StructTypes.DATA_TYPE_UINT16:
                case (int)E_StructTypes.DATA_TYPE_UINT16_1: return this.uint16;

                case (int)E_StructTypes.DATA_TYPE_UINT32:
                case (int)E_StructTypes.DATA_TYPE_UINT32_1: return this.uint32;

                case (int)E_StructTypes.DATA_TYPE_UINT64:
                case (int)E_StructTypes.DATA_TYPE_UINT64_1: return this.uint64;

                case (int)E_StructTypes.DATA_TYPE_FLOAT: return this.ufloat;

                case (int)E_StructTypes.DATA_TYPE_STRING: return this.str;

                case (int)E_StructTypes.DATA_TYPE_BYTEARRAY: return this.data;

                default: throw new NullReferenceException();
            }
        }

        public new Type GetType()
        {
            object result = -1;

            switch (this.type)
            {
                case (int)E_StructTypes.DATA_TYPE_UINT8:
                case (int)E_StructTypes.DATA_TYPE_UINT8_1: return this.uint8.GetType();

                case (int)E_StructTypes.DATA_TYPE_UINT16:
                case (int)E_StructTypes.DATA_TYPE_UINT16_1: return this.uint16.GetType();

                case (int)E_StructTypes.DATA_TYPE_UINT32:
                case (int)E_StructTypes.DATA_TYPE_UINT32_1: return this.uint32.GetType();

                case (int)E_StructTypes.DATA_TYPE_UINT64:
                case (int)E_StructTypes.DATA_TYPE_UINT64_1: return this.uint64.GetType();

                case (int)E_StructTypes.DATA_TYPE_FLOAT: return this.ufloat.GetType();

                case (int)E_StructTypes.DATA_TYPE_STRING: return this.str.GetType();

                case (int)E_StructTypes.DATA_TYPE_BYTEARRAY: return this.data.GetType();

                default: throw new NullReferenceException();
            }
        }

        public void UpdateTypedData(EndianReader br, int flags, long strings_offset, long data_offset, Encoding? encoding)
        {
            int type = flags & (int)UTF.COLUMN_FLAGS.TYPE_MASK;
            this.type = type;
            this.position = br.BaseStream.Position;
            switch (type)
            {
                case (int)E_StructTypes.DATA_TYPE_UINT8:
                case (int)E_StructTypes.DATA_TYPE_UINT8_1:
                    this.uint8 = br.ReadByte();
                    break;
                case (int)E_StructTypes.DATA_TYPE_UINT16:
                case (int)E_StructTypes.DATA_TYPE_UINT16_1:
                    this.uint16 = br.ReadUInt16();
                    break;

                case (int)E_StructTypes.DATA_TYPE_UINT32:
                case (int)E_StructTypes.DATA_TYPE_UINT32_1:
                    this.uint32 = br.ReadUInt32();
                    break;

                case (int)E_StructTypes.DATA_TYPE_UINT64:
                case (int)E_StructTypes.DATA_TYPE_UINT64_1:
                    this.uint64 = br.ReadUInt64();

                    break;

                case (int)E_StructTypes.DATA_TYPE_FLOAT:
                    this.ufloat = br.ReadSingle();
                    break;

                case (int)E_StructTypes.DATA_TYPE_STRING:
                    this.str = Tools.ReadCString(br, -1, br.ReadInt32() + strings_offset, encoding);

                    break;

                case (int)E_StructTypes.DATA_TYPE_BYTEARRAY:
                    long position = br.ReadInt32() + data_offset;
                    this.position = position;
                    this.data = Tools.GetData(br, position, br.ReadInt32());
                    break;
            }
        }


        //column based datatypes
        public byte uint8 { get; set; }
        public ushort uint16 { get; set; }
        public uint uint32 { get; set; }
        public ulong uint64 { get; set; }
        public float ufloat { get; set; }
        public string str { get; set; } = string.Empty;
        public byte[] data { get; set; } = Array.Empty<byte>();
        public long position { get; set; }
    }


    public class ROW : TypeData
    {
        public ROW()
        {

        }

    }

    public class FileEntry
    {
        public FileEntry()
        {
            DirName = string.Empty;
            FileName = string.Empty;
            FileSize = null!;
            ExtractSize = null!;
            ID = null!;
            FileSizeType = null!;
            ExtractSizeType = null!;
            FileOffsetType = null!;
            UserString = string.Empty;
            LocalDir = string.Empty;

            FileOffset = 0;
            UpdateDateTime = 0;
        }

        public string DirName { get; set; } // string
        public string FileName { get; set; } // string

        public object FileSize { get; set; }
        public long FileSizePos { get; set; }
        public Type FileSizeType { get; set; }

        public object ExtractSize { get; set; } // int
        public long ExtractSizePos { get; set; }
        public Type ExtractSizeType { get; set; }

        public ulong FileOffset { get; set; }
        public long FileOffsetPos { get; set; }
        public Type FileOffsetType { get; set; }


        public ulong Offset { get; set; }
        public object ID { get; set; } // int
        public string UserString { get; set; } // string
        public ulong UpdateDateTime { get; set; }
        public string LocalDir { get; set; } // string
        public TOCFlag TOCName { get; set; }

        public bool Encrypted { get; set; }

        public FileTypeFlag FileType { get; set; }
    }

    public enum FileTypeFlag
    {
        FILE = default,
        CPK,
        HDR,
        CONTENT,
    }

    public enum TOCFlag
    {
        CPK,
        TOC,
        ITOC,
        ETOC,
        GTOC
    }
}
