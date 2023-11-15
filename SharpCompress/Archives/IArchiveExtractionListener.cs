using SharpCompress.Common;

namespace SharpCompress.Archives;

internal interface IArchiveExtractionListener
{
    void EnsureEntriesLoaded();
}
