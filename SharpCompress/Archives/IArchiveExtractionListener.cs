using SharpCompress.Common;

namespace SharpCompress.Archives;

internal interface IArchiveExtractionListener : IExtractionListener
{
    void EnsureEntriesLoaded();
}
