using System.Collections.Generic;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;

namespace SharpCompress.Readers.Rar;

public sealed class RarReaderEntry : RarEntry
{
    internal RarReaderEntry(bool solid, RarFilePart part)
    {
        Part = part;
        IsSolid = solid;
    }

    private readonly RarFilePart Part;

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
