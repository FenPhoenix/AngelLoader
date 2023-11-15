using System.Collections.Generic;
using System.IO;
using SharpCompress_7z.Common.Rar;
using SharpCompress_7z.Common.Rar.Headers;
using SharpCompress_7z.IO;

namespace SharpCompress_7z.Readers.Rar;

public sealed class RarReaderVolume : RarVolume
{
    internal RarReaderVolume(Stream stream, ReaderOptions options, int index = 0)
        : base(StreamingMode.Streaming, stream, options, index) { }

    internal override RarFilePart CreateFilePart(MarkHeader markHeader, FileHeader fileHeader) =>
        new NonSeekableStreamFilePart(markHeader, fileHeader, Index);

    internal override IEnumerable<RarFilePart> ReadFileParts() => GetVolumeFileParts();
}
