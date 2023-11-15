using SharpCompress_7z.Common;

namespace SharpCompress_7z.Archives;

internal interface IArchiveExtractionListener : IExtractionListener
{
    void EnsureEntriesLoaded();
}
