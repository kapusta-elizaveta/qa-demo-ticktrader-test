using System;
using System.IO;
using System.Globalization;

namespace WebCore
{
    public class FileUtils
    {
        private FileInfo _f;

        public FileUtils(string filePath, string fileName)
        {
            FileName = fileName;
            FilePath = filePath;
            _f = new FileInfo(Path.Combine(filePath, fileName));
        }

        public string FileName { get; }

        public string FilePath { get; }

        public string Extension => _f.Extension;

        public string FullPath => _f.FullName;
        
        // Return a string describing the value as a file size.
        // For example, 1.23 MB.
        public string Size
        {
            get
            {
                var ci = CultureInfo.InvariantCulture;
                Double bytes = _f.Length;
                if (bytes > 1024 && bytes < 1048576) {
                    return (bytes / 1024).ToString("F02", ci) + " KB";
                }

                if (bytes >= 1048576 && bytes <= 1073741824) {
                    return (bytes / 1048576).ToString("F02", ci) + " MB";
                }

                if (bytes >= 1073741824) {
                    return (bytes / 1073741824).ToString("F02", ci) + " GB";
                }
                return bytes + " B";
            }
        }
    }
}