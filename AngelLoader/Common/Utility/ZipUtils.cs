using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using AL_Common.FastZipReader;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers.Rar;

namespace AngelLoader;

public static partial class Utils
{
    internal static ZipArchive GetReadModeZipArchiveCharEnc(string fileName, byte[] buffer)
    {
        // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
        Encoding enc = GetOEMCodePageOrFallback(Encoding.UTF8);
        return new ZipArchive(GetReadModeFileStreamWithCachedBuffer(fileName, buffer), ZipArchiveMode.Read, leaveOpen: false, enc);
    }

    internal static ZipArchiveFast GetReadModeZipArchiveCharEnc_Fast(
        string fileName,
        byte[] buffer,
        ZipContext ctx)
    {
        // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
        Encoding enc = GetOEMCodePageOrFallback(Encoding.UTF8);
        return ZipArchiveFast.Create_General(
            stream: GetReadModeFileStreamWithCachedBuffer(fileName, buffer),
            context: ctx,
            entryNameEncoding: enc);
    }

    internal static void Update_ExtractToDirectory_Fast(
        this ZipArchive source,
        string destinationDirectoryName,
        IProgress<ProgressPercents> progress,
        CancellationToken cancellationToken)
    {
        ProgressPercents percents = new();

        string path1 = Directory.CreateDirectory(destinationDirectoryName).FullName;

        cancellationToken.ThrowIfCancellationRequested();

        int length = path1.Length;
        if (length > 0 && path1[length - 1] != Path.DirectorySeparatorChar)
        {
            path1 += Path.DirectorySeparatorChar.ToString();
        }

        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            ReadOnlyCollection<ZipArchiveEntry> entries = source.Entries;

            cancellationToken.ThrowIfCancellationRequested();

            int entryCount = entries.Count;
            for (int i = 0; i < entryCount; i++)
            {
                ZipArchiveEntry entry = entries[i];
                string fullPath = Path.GetFullPath(Path.Combine(path1, entry.FullName));

                cancellationToken.ThrowIfCancellationRequested();

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

                    cancellationToken.ThrowIfCancellationRequested();

                    Internal_ExtractToFile_Fast(entry, fullPath, false, tempBuffer, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                percents.SubPercent = GetPercentFromValue_Int(i + 1, entryCount);
                percents.MainPercent = 50 + (percents.SubPercent / 2);
                progress.Report(percents);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }

        return;

        static void Internal_ExtractToFile_Fast(
            ZipArchiveEntry entry,
            string fileName,
            bool overwrite,
            byte[] tempBuffer,
            CancellationToken cancellationToken)
        {
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.Read))
            using (Stream source = entry.Open())
            {
                cancellationToken.ThrowIfCancellationRequested();
                Internal_StreamCopyNoAlloc(source, destination, tempBuffer, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            File.SetLastWriteTime(fileName, entry.LastWriteTime.DateTime);

            cancellationToken.ThrowIfCancellationRequested();

            return;

            static void Internal_StreamCopyNoAlloc(Stream source, Stream destination, byte[] buffer, CancellationToken cancellationToken)
            {
                int count;
                while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                {
                    destination.Write(buffer, 0, count);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }
    }

    internal static void ExtractToFile_Fast(
        this ZipArchiveEntry entry,
        string fileName,
        bool overwrite,
        IOBufferPools ioBufferPools)
    {
        byte[] fileStreamBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            using (FileStream destination = GetWriteModeFileStreamWithCachedBuffer(fileName, overwrite, fileStreamBuffer))
            using (Stream source = entry.Open())
            {
                source.CopyTo(destination);
            }
            File.SetLastWriteTime(fileName, entry.LastWriteTime.DateTime);
        }
        finally
        {
            ioBufferPools.FileStream.Return(fileStreamBuffer);
        }
    }

    internal static void ExtractToFile_Fast(
        this RarArchiveEntry entry,
        string fileName,
        bool overwrite,
        IOBufferPools ioBufferPools)
    {
        byte[] fileStreamBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            using (FileStream destination = GetWriteModeFileStreamWithCachedBuffer(fileName, overwrite, fileStreamBuffer))
            using (Stream source = entry.OpenEntryStream())
            {
                source.CopyTo(destination);
            }
            if (entry.LastModifiedTime != null)
            {
                File.SetLastWriteTime(fileName, (DateTime)entry.LastModifiedTime);
            }
        }
        finally
        {
            ioBufferPools.FileStream.Return(fileStreamBuffer);
        }
    }

    internal static void ExtractToFile_Fast(
        this RarReader reader,
        string fileName,
        bool overwrite,
        IOBufferPools ioBufferPools)
    {
        byte[] fileStreamBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            using (FileStream destination = GetWriteModeFileStreamWithCachedBuffer(fileName, overwrite, fileStreamBuffer))
            using (Stream source = reader.OpenEntryStream())
            {
                source.CopyTo(destination);
            }
            DateTime? lastModifiedTime = reader.Entry.LastModifiedTime;
            if (lastModifiedTime != null)
            {
                File.SetLastWriteTime(fileName, (DateTime)lastModifiedTime);
            }
        }
        finally
        {
            ioBufferPools.FileStream.Return(fileStreamBuffer);
        }
    }
}
