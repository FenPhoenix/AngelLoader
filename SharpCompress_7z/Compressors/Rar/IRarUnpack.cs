using System.IO;
using SharpCompress_7z.Common.Rar.Headers;

namespace SharpCompress_7z.Compressors.Rar;

internal interface IRarUnpack
{
    void DoUnpack(FileHeader fileHeader, Stream readStream, Stream writeStream);
    void DoUnpack();

    // eg u/i pause/resume button

    long DestSize { get; }
    int Char { get; }
    int PpmEscChar { get; set; }
}
