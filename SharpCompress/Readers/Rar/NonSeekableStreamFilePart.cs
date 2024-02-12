using System.IO;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;

namespace SharpCompress.Readers.Rar;

internal sealed class NonSeekableStreamFilePart : RarFilePart
{
    internal NonSeekableStreamFilePart(FileHeader fh)
        : base(fh) { }

    internal override Stream GetCompressedStream() => FileHeader.PackedStream;

    internal override Stream? GetRawStream() => FileHeader.PackedStream;
}
