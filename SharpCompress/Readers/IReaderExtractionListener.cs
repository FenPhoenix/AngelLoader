using SharpCompress_7z.Common;
using SharpCompress_7z.Common.Rar;

namespace SharpCompress_7z.Readers;

public interface IReaderExtractionListener : IExtractionListener
{
    void FireEntryExtractionProgress(RarEntry entry, long sizeTransferred, int iterations);
}
