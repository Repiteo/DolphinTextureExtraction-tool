﻿namespace AuroraLib.Common
{
    public static partial class FormatDictionary
    {
        public static bool TryGetValue(string key, out FormatInfo info)
        {
            if (Header.TryGetValue(key, out info))
                return true;

            try
            {
                info = Master.First(x => x.Extension == key);
                return true;
            }
            catch (Exception) { }

            return false;
        }

        public static FormatInfo GetValue(string key)
        {
            if (TryGetValue(key, out FormatInfo info)) return info;
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// Identifies the file format
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static FormatInfo Identify(this Stream stream, string extension = "")
        {
            foreach (var item in Formats)
            {
                try
                {
                    if (item.IsMatch.Invoke(stream, extension))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        return item;
                    }
                }
                catch (Exception t)
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Match error in {item.Class?.Name}, {t}");
                }
                finally
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            return new FormatInfo(stream, extension);
        }

        static FormatDictionary()
        {
            List<FormatInfo> Exten = new List<FormatInfo>();

            foreach (FormatInfo file in Master)
            {
                if (file.Header != null)
                {
                    try
                    {
                        file.Class = Reflection.FileAccess.GetByMagic(file.Header.Magic);
                        file.IsMatch = Reflection.FileAccess.GetInstance(file.Class).IsMatch;
                    }
                    catch (Exception) { }
                    if (file.Header.Magic.Length > 1)
                        Header.Add(file.Header.Magic, file);

                    Formats.Add(file);
                }
                else
                {
                    Exten.Add(file);
                }
            }
            Formats.AddRange(Exten);
        }

        private static List<FormatInfo> Formats = new List<FormatInfo>();

        public static readonly Dictionary<string, FormatInfo> Header = new Dictionary<string, FormatInfo>();
    }
}
