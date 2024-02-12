using System;
using System.Collections.Generic;
using SharpCompress.Common;
using SharpCompress.IO;

namespace SharpCompress.Archives;

public abstract class AbstractArchive<TEntry, TVolume> : IDisposable
    where TEntry : IEntry
    where TVolume : IDisposable
{
    private readonly LazyReadOnlyCollection<TVolume> lazyVolumes;
    private readonly LazyReadOnlyCollection<TEntry> lazyEntries;

    protected OptionsBase OptionsBase { get; }

    private bool disposed;
    protected readonly SourceStream SrcStream;

    internal AbstractArchive(SourceStream srcStream)
    {
        OptionsBase = srcStream.OptionsBase;
        SrcStream = srcStream;
        lazyVolumes = new LazyReadOnlyCollection<TVolume>(LoadVolumes(SrcStream));
        lazyEntries = new LazyReadOnlyCollection<TEntry>(LoadEntries(Volumes));
    }


    /// <summary>
    /// Returns an ReadOnlyCollection of all the RarArchiveEntries across the one or many parts of the RarArchive.
    /// </summary>
    public LazyReadOnlyCollection<TEntry> Entries => lazyEntries;

    /// <summary>
    /// Returns an ReadOnlyCollection of all the RarArchiveVolumes across the one or many parts of the RarArchive.
    /// </summary>
    protected ICollection<TVolume> Volumes => lazyVolumes;

    protected abstract IEnumerable<TVolume> LoadVolumes(SourceStream srcStream);
    protected abstract IEnumerable<TEntry> LoadEntries(IEnumerable<TVolume> volumes);

    public virtual void Dispose()
    {
        if (!disposed)
        {
            lazyVolumes.ForEach(static v => v.Dispose());
            SrcStream?.Dispose();

            disposed = true;
        }
    }

#if false
    private void EnsureEntriesLoaded()
    {
        lazyEntries.EnsureFullyLoaded();
        lazyVolumes.EnsureFullyLoaded();
    }

    /// <summary>
    /// Use this method to extract all entries in an archive in order.
    /// This is primarily for SOLID Rar Archives or 7Zip Archives as they need to be
    /// extracted sequentially for the best performance.
    ///
    /// This method will load all entry information from the archive.
    ///
    /// WARNING: this will reuse the underlying stream for the archive.  Errors may
    /// occur if this is used at the same time as other extraction methods on this instance.
    /// </summary>
    /// <returns></returns>
    public IReader ExtractAllEntries()
    {
        EnsureEntriesLoaded();
        return CreateReaderForSolidExtraction();
    }

    /// <summary>
    /// The total size of the files compressed in the archive.
    /// </summary>
    public long TotalSize =>
        Entries.Aggregate(0L, static (total, cf) => total + cf.CompressedSize);

    /// <summary>
    /// The total size of the files as uncompressed in the archive.
    /// </summary>
    public long TotalUncompressedSize =>
        Entries.Aggregate(0L, static (total, cf) => total + cf.Size);


    /// <summary>
    /// The archive can find all the parts of the archive needed to fully extract the archive.  This forces the parsing of the entire archive.
    /// </summary>
    public bool IsComplete
    {
        get
        {
            EnsureEntriesLoaded();
            return Entries.All(static x => x.IsComplete);
        }
    }
#endif
}
