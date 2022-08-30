﻿using AuroraLip.Common;
using LibCPK;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    public class Unpack : ScanBase
    {
        public Unpack(string scanDirectory, string saveDirectory, Options options = null) : base(scanDirectory, saveDirectory, options)
        {
        }

        public static void StartScan(string meindirectory, string savedirectory, Options options)
            => StartScan_Async(meindirectory, savedirectory, options).Wait();

        public static async Task StartScan_Async(string meindirectory, string savedirectory, Options options)
        {
            Unpack Extractor = new Unpack(meindirectory, savedirectory, options);
            await Task.Run(() => Extractor.StartScan());
            return;
        }

        public void StartScan()
        {
            Scan(new DirectoryInfo(ScanDirectory));
            Log.Dispose();
        }

        protected override void Scan(FileInfo file)
        {
            Stream stream = new FileStream(file.FullName, FileMode.Open);
            FormatInfo FFormat = GetFormatTypee(stream, file.Extension);
            string subdirectory = GetDirectoryWithoutExtension(file.FullName.Replace(ScanDirectory + Path.DirectorySeparatorChar, ""));

#if !DEBUG
            try
            {
#endif
                switch (FFormat.Typ)
                {
                    case FormatType.Unknown:
                        if (!TryForce(stream, subdirectory, FFormat))
                            AddResultUnknown(stream, FFormat, subdirectory + file.Extension);

                        break;
                    case FormatType.Archive:
                        switch (FFormat.Extension.ToLower())
                        {
                            case "cpk":
                                stream.Close();
                                scanCPK(file.FullName, subdirectory);
                                break;
                            default:
                                if (!TryExtract(stream, subdirectory, FFormat))
                                {
                                    Log.Write(FileAction.Unsupported, subdirectory + file.Extension + $" ~{MathEx.SizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()}");
                                    Save(stream, subdirectory, FFormat);
                                }
                                break;
                        }
                        break;
                    default:
                        break;
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + file.Extension);
            }
#endif
            stream.Close();
        }

        protected override void Scan(Stream stream, string subdirectory, in string Extension = "")
        {
            FormatInfo FFormat = GetFormatTypee(stream, Extension);
            subdirectory = GetDirectoryWithoutExtension(subdirectory);

#if !DEBUG
            try
            {
#endif
                switch (FFormat.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(stream, subdirectory, FFormat))
                            break;

                        AddResultUnknown(stream, FFormat, subdirectory + Extension);
                        Save(stream, subdirectory, FFormat);
                        break;
                    case FormatType.Archive:
                        if (!TryExtract(stream, subdirectory, FFormat))
                        {
                            Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{MathEx.SizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()}");
                            Save(stream, subdirectory, FFormat);
                        }
                        break;
                    default:
                        Save(stream, subdirectory, FFormat);
                        break;
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + Extension);
                Save(stream, subdirectory);
            }
#endif
            stream.Close();
        }

        private void scanCPK(string file, string subdirectory)
        {
            CPK CpkContent = new CPK();
            CpkContent.ReadCPK(file, Encoding.UTF8);

            BinaryReader CPKReader = new BinaryReader(File.OpenRead(file));

            foreach (var entries in CpkContent.fileTable)
            {
                try
                {
                    if (CpkDecompressEntrie(CpkContent, CPKReader, entries, out byte[] chunk))
                    {
                        MemoryStream CpkContentStream = new MemoryStream(chunk);
                        Scan(CpkContentStream, Path.Combine(subdirectory, Path.GetFileNameWithoutExtension(entries.FileName.ToString())));
                        CpkContentStream.Dispose();
                    }
                }
                catch (Exception) { }
            }
            CPKReader.Close();
        }

        private void AddResultUnknown(Stream stream, FormatInfo FormatTypee, in string file)
        {
            if (FormatTypee.Header == null || FormatTypee.Header?.Magic.Length <= 3)
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Bytes32:[{string.Join(",", stream.Read(32))}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Magic:[{FormatTypee.Header.Magic}] Bytes:[{string.Join(",", FormatTypee.Header.Bytes)}] Offset:{FormatTypee.Header.Offset}");
            }
        }

    }
}
