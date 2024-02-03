using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using AL_Common;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers.Rar;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader;

public static partial class Utils
{
    internal static ZipArchive GetReadModeZipArchiveCharEnc(string fileName, byte[] buffer)
    {
        // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
        Encoding enc = GetOEMCodePageOrFallback(Encoding.UTF8);
        return new ZipArchive(GetReadModeFileStreamWithCachedBuffer(fileName, buffer), ZipArchiveMode.Read, leaveOpen: false, enc);
    }

    internal static void Update_ExtractToDirectory_Fast(
        this ZipArchive source,
        string destinationDirectoryName,
        IProgress<ProgressPercents> progress)
    {
        ProgressPercents percents = new();

        string path1 = Directory.CreateDirectory(destinationDirectoryName).FullName;

        int length = path1.Length;
        if (length > 0 && path1[length - 1] != Path.DirectorySeparatorChar)
        {
            path1 += Path.DirectorySeparatorChar.ToString();
        }

        byte[] tempBuffer = new byte[StreamCopyBufferSize];

        ReadOnlyCollection<ZipArchiveEntry> entries = source.Entries;
        int entryCount = entries.Count;
        for (int i = 0; i < entryCount; i++)
        {
            ZipArchiveEntry entry = entries[i];
            string fullPath = Path.GetFullPath(Path.Combine(path1, entry.FullName));

            if (!fullPath.StartsWith(path1, StringComparison.OrdinalIgnoreCase))
            {
                ThrowHelper.IOException(
                    "Extracting Zip entry would have resulted in a file outside the specified destination directory.");
            }

            if (Path.GetFileName(fullPath).Length == 0)
            {
                if (entry.Length > 0)
                {
                    ThrowHelper.IOException(
                        "Zip entry name ends in directory separator character but contains data.");
                }
                Directory.CreateDirectory(fullPath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                entry.ExtractToFile_Fast(fullPath, false, tempBuffer);
            }

            percents.SubPercent = GetPercentFromValue_Int(i + 1, entryCount);
            percents.MainPercent = 50 + (percents.SubPercent / 2);
            progress.Report(percents);
        }
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

    internal static void ExtractToFile_Fast(
        this RarArchiveEntry entry,
        string fileName,
        bool overwrite,
        byte[] tempBuffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.None))
        using (Stream source = entry.OpenEntryStream())
        {
            StreamCopyNoAlloc(source, destination, tempBuffer);
        }
        if (entry.LastModifiedTime != null)
        {
            File.SetLastWriteTime(fileName, (DateTime)entry.LastModifiedTime);
        }
    }

    internal static void ExtractToFile_Fast(
        this RarReader reader,
        string fileName,
        bool overwrite,
        byte[] tempBuffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.None))
        using (Stream source = reader.OpenEntryStream())
        {
            StreamCopyNoAlloc(source, destination, tempBuffer);
        }
        DateTime? lastModifiedTime = reader.Entry.LastModifiedTime;
        if (lastModifiedTime != null)
        {
            File.SetLastWriteTime(fileName, (DateTime)lastModifiedTime);
        }
    }
}
