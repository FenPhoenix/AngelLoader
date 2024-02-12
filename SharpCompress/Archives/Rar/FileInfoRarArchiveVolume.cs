using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;
using SharpCompress.Readers;

namespace SharpCompress.Archives.Rar;

/// <summary>
/// A rar part based on a FileInfo object
/// </summary>
internal abstract class FileInfoRarArchiveVolume : RarVolume
{
    internal FileInfoRarArchiveVolume(FileInfo fileInfo, ReaderOptions options, int index = 0)
        : base(StreamingMode.Seekable, fileInfo.OpenRead(), FixOptions(options), index)
    {
        FileInfo = fileInfo;
        FileParts = GetVolumeFileParts().ToArray().ToReadOnly();
    }

    private static ReaderOptions FixOptions(ReaderOptions options)
    {
        //make sure we're closing streams with fileinfo
        options.LeaveStreamOpen = false;
        return options;
    }

    private ReadOnlyCollection<RarFilePart> FileParts { get; }

    private FileInfo FileInfo { get; }

    internal override RarFilePart CreateFilePart(MarkHeader markHeader, FileHeader fileHeader) =>
        new FileInfoRarFilePart(this, markHeader, fileHeader, FileInfo);

    internal override IEnumerable<RarFilePart> ReadFileParts() => FileParts;
}
