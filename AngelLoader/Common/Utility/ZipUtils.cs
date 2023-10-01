using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using static AL_Common.Common;

namespace AngelLoader;

public static partial class Utils
{
    internal static ZipArchive GetReadModeZipArchiveCharEnc(string fileName, byte[] buffer)
    {
        // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
        Encoding enc;
        try
        {
            enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch
        {
            enc = Encoding.UTF8;
        }

        return new ZipArchive(GetReadModeFileStreamWithCachedBuffer(fileName, buffer), ZipArchiveMode.Read, leaveOpen: false, enc);
    }

    internal static void ExtractToFile_Fast(
        this ZipArchiveEntry entry,
        string fileName,
        bool overwrite,
        byte[] tempBuffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.None))
        using (Stream source = entry.Open())
        {
            StreamCopyNoAlloc(source, destination, tempBuffer);
        }
        File.SetLastWriteTime(fileName, entry.LastWriteTime.DateTime);
    }
}
