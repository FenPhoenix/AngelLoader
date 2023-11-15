using System.IO;
using AL_Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Crypto;

namespace SharpCompress.Archives.Rar;

internal class SeekableFilePart : RarFilePart
{
    private readonly Stream stream;

    internal SeekableFilePart(
        MarkHeader mh,
        FileHeader fh,
        int index,
        Stream stream
    )
        : base(mh, fh, index)
    {
        this.stream = stream;
    }

    internal override Stream GetCompressedStream()
    {
        stream.Position = FileHeader.DataStartPosition;
        if (FileHeader.R4Salt != null)
        {
            ThrowHelper.EncryptionNotSupported();
        }
        return stream;
    }

    internal override string FilePartName => "Unknown Stream - File Entry: " + FileHeader.FileName;
}
