using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Compressors.Rar;

namespace SharpCompress.Readers.Rar;

/// <summary>
/// This class faciliates Reading a Rar Archive in a non-seekable forward-only manner
/// </summary>
public abstract class RarReader : AbstractReader<RarReaderEntry, RarVolume>
{
    private RarVolume? volume;

    private readonly Lazy<IRarUnpack> UnpackV2017 =
        new Lazy<IRarUnpack>(static () => new Compressors.Rar.UnpackV2017.Unpack());

    private readonly Lazy<IRarUnpack> UnpackV1 =
        new Lazy<IRarUnpack>(static () => new Compressors.Rar.UnpackV1.Unpack());

    internal RarReader() { }

    protected abstract void ValidateArchive(RarVolume archive);

    protected override RarVolume Volume => volume!;

    /// <summary>
    /// Opens a RarReader for Non-seeking usage with a single volume
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static RarReader Open(Stream stream)
    {
        stream.CheckNotNull(nameof(stream));
        return new SingleVolumeRarReader(stream);
    }

    protected override IEnumerable<RarReaderEntry> GetEntries(Stream stream)
    {
        volume = new RarReaderVolume(stream);
        foreach (var fp in volume.ReadFileParts())
        {
            ValidateArchive(volume);
            yield return new RarReaderEntry(volume.IsSolidArchive, fp);
        }
    }

    private IEnumerable<FilePart> CreateFilePartEnumerableForCurrentEntry() =>
        Entry.Parts;

    protected override EntryStream GetEntryStream()
    {
        var stream = new MultiVolumeReadOnlyStream(
            CreateFilePartEnumerableForCurrentEntry().Cast<RarFilePart>()
        );
        if (Entry.IsRarV3)
        {
            return CreateEntryStream(new RarCrcStream(UnpackV1.Value, Entry.FileHeader, stream));
        }
        return CreateEntryStream(new RarCrcStream(UnpackV2017.Value, Entry.FileHeader, stream));
    }
}
