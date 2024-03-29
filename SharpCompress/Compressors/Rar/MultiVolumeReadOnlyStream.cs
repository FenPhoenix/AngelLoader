#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Rar;

namespace SharpCompress.Compressors.Rar;

internal sealed class MultiVolumeReadOnlyStream : Stream
{
    private long currentPosition;
    private long maxPosition;

    private IEnumerator<RarFilePart> filePartEnumerator;
    private Stream currentStream;

    internal MultiVolumeReadOnlyStream(
        IEnumerable<RarFilePart> parts
    )
    {
        filePartEnumerator = parts.GetEnumerator();
        filePartEnumerator.MoveNext();
        InitializeNextFilePart();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            if (filePartEnumerator != null)
            {
                filePartEnumerator.Dispose();
                filePartEnumerator = null;
            }
            currentStream = null;
        }
    }

    private void InitializeNextFilePart()
    {
        maxPosition = filePartEnumerator.Current.FileHeader.CompressedSize;
        currentPosition = 0;
        currentStream = filePartEnumerator.Current.GetCompressedStream();

        CurrentCrc = filePartEnumerator.Current.FileHeader.FileCrc;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalRead = 0;
        var currentOffset = offset;
        var currentCount = count;
        while (currentCount > 0)
        {
            var readSize = currentCount;
            if (currentCount > maxPosition - currentPosition)
            {
                readSize = (int)(maxPosition - currentPosition);
            }

            var read = currentStream.Read(buffer, currentOffset, readSize);
            if (read < 0)
            {
                throw new EndOfStreamException();
            }

            currentPosition += read;
            currentOffset += read;
            currentCount -= read;
            totalRead += read;
            if (
                ((maxPosition - currentPosition) == 0)
                && filePartEnumerator.Current.FileHeader.IsSplitAfter
            )
            {
                if (filePartEnumerator.Current.FileHeader.R4Salt != null)
                {
                    throw new InvalidFormatException(
                        "Sharpcompress currently does not support multi-volume decryption."
                    );
                }
                var fileName = filePartEnumerator.Current.FileHeader.FileName;
                if (!filePartEnumerator.MoveNext())
                {
                    throw new InvalidFormatException(
                        "Multi-part rar file is incomplete.  Entry expects a new volume: "
                            + fileName
                    );
                }
                InitializeNextFilePart();
            }
            else
            {
                break;
            }
        }
        return totalRead;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public uint CurrentCrc { get; private set; }

    public override void Flush() => throw new NotSupportedException();

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
}
