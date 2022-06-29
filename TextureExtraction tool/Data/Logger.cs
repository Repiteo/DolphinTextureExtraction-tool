﻿using System;
using System.IO;

namespace DolphinTextureExtraction_tool
{
    public enum FileAction
    {
        Unknown = -2, Unsupported = -1, Extract
    }

    internal class ScanLogger : IDisposable
    {

        public string FullPath { get; private set; }

        readonly StreamWriter LogFile;

        public ScanLogger(string directory)
        {
            Directory.CreateDirectory(directory);
            GenerateLogFullPath(directory);
            LogFile = new StreamWriter(FullPath, false);
            WriteHeader();
        }

        private void GenerateLogFullPath(string directory)
        {
            string basename = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            if (File.Exists(FullPath = Path.ChangeExtension(Path.Combine(directory, basename), "log")))
            {
                int i = 2;
                while (File.Exists(FullPath = Path.ChangeExtension(Path.Combine(directory, basename + "_" + i), "log")))
                {
                    i++;
                }
            }
        }

        public void WriteLine(string value)
        {
            LogFile.WriteLine(value);
        }

        private void WriteHeader()
        {
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}  {DateTime.Now.ToString()}");
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.Flush();
        }

        public void WriteFoot(TextureExtractor.Result result)
        {
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.WriteLine($"~END  {DateTime.Now.ToString()}");
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.WriteLine($"Extracted textures: {result.Extracted}");
            LogFile.WriteLine($"Unsupported files: {result.Unsupported}");
            LogFile.WriteLine($"Unknown files: {result.Unknown}");
            LogFile.WriteLine($"Extraction rate: ~{result.ExtractionRate}%");
            LogFile.WriteLine($"Scan time: {Math.Round(result.TotalTime.TotalSeconds, 2)}s");
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.Flush();
        }

        public void WriteEX(Exception ex, string strMessage = "")
        {
            LogFile.WriteLine("".PadLeft(64, '-'));
            LogFile.WriteLine($"Error!!!... {strMessage} {ex?.Message}");
            LogFile.WriteLine($"{ex?.Source}:{ex?.StackTrace}");
            LogFile.WriteLine("".PadLeft(64, '-'));
            Console.WriteLine("".PadLeft(64, '-'));
            Console.WriteLine($"Error!!!... {strMessage} {ex?.Message}");
            Console.WriteLine($"{ex?.Source}:{ex?.StackTrace}");
            Console.WriteLine("".PadLeft(64, '-'));
        }

        public void Write(FileAction action, string file, string value)
        {
            switch (action)
            {
                case FileAction.Unknown:
                    LogFile.Write("Unknown:");
                    break;
                case FileAction.Unsupported:
                    LogFile.Write("Unsupported:");
                    break;
                case FileAction.Extract:
                    LogFile.Write("Extract:");
                    break;
            }
            LogFile.Write($"\"~{file}\"\n");
            LogFile.WriteLine($" {value}");
            LogFile.Flush();
        }

        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LogFile.Dispose();
                }
                disposedValue = true;
            }
        }
        ~ScanLogger()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
