using System.Collections.Generic;
using SharpCompress_7z.Common;
using SharpCompress_7z.Common.Rar;
using SharpCompress_7z.Common.Rar.Headers;

namespace SharpCompress_7z.Readers.Rar;

public sealed class RarReaderEntry : RarEntry
{
    internal RarReaderEntry(bool solid, RarFilePart part)
    {
        Part = part;
        IsSolid = solid;
    }

    internal RarFilePart Part { get; }

    internal override IEnumerable<FilePart> Parts => Part.AsEnumerable<FilePart>();

    internal override FileHeader FileHeader => Part.FileHeader;

    /// <summary>
    /// The compressed file size
    /// </summary>
    public override long CompressedSize => Part.FileHeader.CompressedSize;

    /// <summary>
    /// The uncompressed file size
    /// </summary>
    public override long Size => Part.FileHeader.UncompressedSize;
}
