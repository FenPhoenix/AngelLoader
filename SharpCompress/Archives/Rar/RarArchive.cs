using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Compressors.Rar;
using SharpCompress.IO;

namespace SharpCompress.Archives.Rar;

public sealed class RarArchive : AbstractArchive<RarArchiveEntry, RarVolume>
{
    internal Lazy<IRarUnpack> UnpackV2017 { get; } =
        new Lazy<IRarUnpack>(static () => new Compressors.Rar.UnpackV2017.Unpack());
    internal Lazy<IRarUnpack> UnpackV1 { get; } =
        new Lazy<IRarUnpack>(static () => new Compressors.Rar.UnpackV1.Unpack());

    /// <summary>
    /// Constructor with a SourceStream able to handle FileInfo and Streams.
    /// </summary>
    /// <param name="srcStream"></param>
    private RarArchive(SourceStream srcStream)
        : base(srcStream) { }

    protected override IEnumerable<RarArchiveEntry> LoadEntries(IEnumerable<RarVolume> volumes) =>
        RarArchiveEntryFactory.GetEntries(this, volumes);

    protected override IEnumerable<RarVolume> LoadVolumes(SourceStream srcStream)
    {
        SrcStream.LoadAllParts(); //request all streams
        var streams = SrcStream.Streams.ToArray();
        var idx = 0;
        if (streams.Length > 1 && IsRarFile(streams[1])) //test part 2 - true = multipart not split
        {
            SrcStream.IsVolumes = true;
            streams[1].Position = 0;
            SrcStream.Position = 0;

            return srcStream.Streams.Select(
                a => new StreamRarArchiveVolume(a, idx++)
            );
        }
        else //split mode or single file
        {
            return new StreamRarArchiveVolume(SrcStream, idx).AsEnumerable();
        }
    }

    public bool IsSolid => Volumes.First().IsSolidArchive;

    #region Creation
    /// <summary>
    /// Constructor with a FileInfo object to an existing file.
    /// </summary>
    /// <param name="filePath"></param>
    public static RarArchive Open(string filePath)
    {
        filePath.CheckNotNullOrEmpty(nameof(filePath));
        var fileInfo = new FileInfo(filePath);
        return new RarArchive(
            new SourceStream(
                fileInfo,
                i => RarArchiveVolumeFactory.GetFilePart(i, fileInfo)
            )
        );
    }

    /// <summary>
    /// Takes a seekable Stream as a source
    /// </summary>
    /// <param name="stream"></param>
    public static RarArchive Open(Stream stream)
    {
        stream.CheckNotNull(nameof(stream));
        return new RarArchive(new SourceStream(stream, static _ => null));
    }

    public static bool IsRarFile(Stream stream)
    {
        try
        {
            MarkHeader.Read(stream, false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
