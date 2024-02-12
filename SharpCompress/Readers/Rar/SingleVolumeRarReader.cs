using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Rar;

namespace SharpCompress.Readers.Rar;

internal sealed class SingleVolumeRarReader : RarReader
{
    private readonly Stream stream;

    internal SingleVolumeRarReader(Stream stream, OptionsBase options)
        : base(options) => this.stream = stream;

    protected override void ValidateArchive(RarVolume archive)
    {
        if (archive.IsMultiVolume)
        {
            throw new MultiVolumeExtractionException("Streamed archive is a Multi-volume archive.  Use different RarReader method to extract.");
        }
    }

    protected override Stream RequestInitialStream() => stream;
}
