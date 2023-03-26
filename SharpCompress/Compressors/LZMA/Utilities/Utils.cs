using System;
using System.IO;

namespace SharpCompress.Compressors.LZMA.Utilities;

internal enum BlockType : byte
{
    #region Constants

    End = 0,
    Header = 1,
    ArchiveProperties = 2,
    AdditionalStreamsInfo = 3,
    MainStreamsInfo = 4,
    FilesInfo = 5,
    PackInfo = 6,
    UnpackInfo = 7,
    SubStreamsInfo = 8,
    Size = 9,
    Crc = 10,
    Folder = 11,
    CodersUnpackSize = 12,
    NumUnpackStream = 13,
    EmptyStream = 14,
    EmptyFile = 15,
    Anti = 16,
    Name = 17,
    CTime = 18,
    ATime = 19,
    MTime = 20,
    WinAttributes = 21,
    Comment = 22,
    EncodedHeader = 23,
    StartPos = 24,
    Dummy = 25

    #endregion
}

internal static class Utils
{
    internal static DateTime? TranslateTime(long? time)
    {
        if (time.HasValue && time.Value >= 0 && time.Value <= 2650467743999999999) //maximum Windows file time 31.12.9999
        {
            return DateTime.FromFileTimeUtc(time.Value).ToLocalTime();
        }
        return null;
    }

    internal static void ReadExact(this Stream stream, byte[] buffer, int offset, int length)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0 || offset > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (length < 0 || length > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        while (length > 0)
        {
            var fetched = stream.Read(buffer, offset, length);
            if (fetched <= 0)
            {
                throw new EndOfStreamException();
            }

            offset += fetched;
            length -= fetched;
        }
    }
}
