using System;
using System.IO;

namespace Pfim
{
    /// <summary>Decodes images into a uniform structure</summary>
    public static class Pfimage
    {
        public static Targa FromFile(string path)
        {
            return FromFile(path, new PfimConfig());
        }

        /// <summary>Constructs an image from a given file</summary>
        public static Targa FromFile(string path, PfimConfig config)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"Image does not exist: {Path.GetFullPath(path)}", path);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, config.BufferSize))
            {
                return FromStream(fs, config);
            }
        }

        public static Targa FromStream(Stream stream)
        {
            return FromStream(stream, new PfimConfig());
        }

        /// <summary>
        /// Create image from stream. Pfim will try to detect the format based on several leading bytes
        /// </summary>
        public static Targa FromStream(Stream stream, PfimConfig config)
        {
            byte[] magic = new byte[4];
            Util.ReadExactly(stream, magic, 0, magic.Length);
            return Targa.CreateWithPartialHeader(stream, config, magic);
        }
    }
}
