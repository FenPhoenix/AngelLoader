using System.IO;
using SharpCompress_7z.Common.Rar;
using SharpCompress_7z.Common.Rar.Headers;

namespace SharpCompress_7z.Readers.Rar;

internal sealed class NonSeekableStreamFilePart : RarFilePart
{
    internal NonSeekableStreamFilePart(MarkHeader mh, FileHeader fh, int index = 0)
        : base(mh, fh, index) { }

    internal override Stream GetCompressedStream() => FileHeader.PackedStream;

    internal override Stream? GetRawStream() => FileHeader.PackedStream;

    internal override string FilePartName => "Unknown Stream - File Entry: " + FileHeader.FileName;
}
