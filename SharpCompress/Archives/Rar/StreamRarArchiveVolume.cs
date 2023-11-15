using System.Collections.Generic;
using System.IO;
using SharpCompress_7z.Common.Rar;
using SharpCompress_7z.Common.Rar.Headers;
using SharpCompress_7z.IO;
using SharpCompress_7z.Readers;

namespace SharpCompress_7z.Archives.Rar;

internal sealed class StreamRarArchiveVolume : RarVolume
{
    internal StreamRarArchiveVolume(Stream stream, ReaderOptions options, int index = 0)
        : base(StreamingMode.Seekable, stream, options, index) { }

    internal override IEnumerable<RarFilePart> ReadFileParts() => GetVolumeFileParts();

    internal override RarFilePart CreateFilePart(MarkHeader markHeader, FileHeader fileHeader) =>
        new SeekableFilePart(markHeader, fileHeader, Index, Stream, ReaderOptions.Password);
}
