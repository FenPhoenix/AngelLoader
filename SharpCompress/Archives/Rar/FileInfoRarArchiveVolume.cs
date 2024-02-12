using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;

namespace SharpCompress.Archives.Rar;

/// <summary>
/// A rar part based on a FileInfo object
/// </summary>
internal abstract class FileInfoRarArchiveVolume : RarVolume
{
    internal FileInfoRarArchiveVolume(FileInfo fileInfo, OptionsBase options, int index = 0)
        : base(StreamingMode.Seekable, fileInfo.OpenRead(), FixOptions(options), index)
    {
        FileInfo = fileInfo;
        FileParts = GetVolumeFileParts().ToArray().ToReadOnly();
    }

    private static OptionsBase FixOptions(OptionsBase options)
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
