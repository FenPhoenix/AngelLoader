using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;

namespace SharpCompress.Archives.Rar;

internal sealed class StreamRarArchiveVolume : RarVolume
{
    internal StreamRarArchiveVolume(Stream stream, OptionsBase options, int index = 0)
        : base(StreamingMode.Seekable, stream, options, index) { }

    internal override IEnumerable<RarFilePart> ReadFileParts() => GetVolumeFileParts();

    internal override RarFilePart CreateFilePart(MarkHeader markHeader, FileHeader fileHeader) =>
        new SeekableFilePart(fileHeader, Stream);
}
