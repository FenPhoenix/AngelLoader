using System.Collections.Generic;
using System.IO;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;

namespace SharpCompress.Readers.Rar;

public sealed class RarReaderVolume : RarVolume
{
    internal RarReaderVolume(Stream stream, int index = 0)
        : base(StreamingMode.Streaming, stream, index) { }

    internal override RarFilePart CreateFilePart(MarkHeader markHeader, FileHeader fileHeader) =>
        new NonSeekableStreamFilePart(fileHeader, Index);

    internal override IEnumerable<RarFilePart> ReadFileParts() => GetVolumeFileParts();
}
