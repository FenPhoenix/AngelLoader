using System.IO;
using AL_Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;

namespace SharpCompress.Archives.Rar;

internal class SeekableFilePart : RarFilePart
{
    private readonly Stream stream;

    internal SeekableFilePart(FileHeader fh,
        Stream stream
    )
        : base(fh)
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
}
