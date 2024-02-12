using System.IO;

namespace SharpCompress.Common;

public abstract class FilePart
{
    internal abstract Stream GetCompressedStream();
    internal abstract Stream? GetRawStream();
}
