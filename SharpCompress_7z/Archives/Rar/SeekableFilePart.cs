using System.IO;
using SharpCompress_7z.Common.Rar;
using SharpCompress_7z.Common.Rar.Headers;

namespace SharpCompress_7z.Archives.Rar;

internal class SeekableFilePart : RarFilePart
{
    private readonly Stream stream;
    private readonly string? password;

    internal SeekableFilePart(
        MarkHeader mh,
        FileHeader fh,
        int index,
        Stream stream,
        string? password
    )
        : base(mh, fh, index)
    {
        this.stream = stream;
        this.password = password;
    }

    internal override Stream GetCompressedStream()
    {
        stream.Position = FileHeader.DataStartPosition;
        if (FileHeader.R4Salt != null)
        {
            return new RarCryptoWrapper(stream, password!, FileHeader.R4Salt);
        }
        return stream;
    }

    internal override string FilePartName => "Unknown Stream - File Entry: " + FileHeader.FileName;
}
