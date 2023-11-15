using SharpCompress.Common;
using SharpCompress.Common.Rar;

namespace SharpCompress.Readers;

public interface IReaderExtractionListener : IExtractionListener
{
    void FireEntryExtractionProgress(RarEntry entry, long sizeTransferred, int iterations);
}
