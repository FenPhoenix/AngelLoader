using System;
using System.Collections.Generic;
using System.Linq;
using SharpCompress.Common.Rar;
using SharpCompress.IO;
using SharpCompress.Readers;

namespace SharpCompress.Archives;

public abstract class AbstractArchive<TEntry, TVolume> : IDisposable
    where TEntry : IArchiveEntry
    where TVolume : IDisposable
{
    private readonly LazyReadOnlyCollection<TVolume> lazyVolumes;
    private readonly LazyReadOnlyCollection<TEntry> lazyEntries;

    protected ReaderOptions ReaderOptions { get; }

    private bool disposed;
    protected readonly SourceStream SrcStream;

    internal AbstractArchive(SourceStream srcStream)
    {
        ReaderOptions = srcStream.ReaderOptions;
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
    public ICollection<TVolume> Volumes => lazyVolumes;

    /// <summary>
    /// The total size of the files compressed in the archive.
    /// </summary>
    public long TotalSize =>
        Entries.Aggregate(0L, (total, cf) => total + cf.CompressedSize);

    /// <summary>
    /// The total size of the files as uncompressed in the archive.
    /// </summary>
    public long TotalUncompressedSize =>
        Entries.Aggregate(0L, (total, cf) => total + cf.Size);

    protected abstract IEnumerable<TVolume> LoadVolumes(SourceStream srcStream);
    protected abstract IEnumerable<TEntry> LoadEntries(IEnumerable<TVolume> volumes);

    public virtual void Dispose()
    {
        if (!disposed)
        {
            lazyVolumes.ForEach(v => v.Dispose());
            lazyEntries.GetLoaded().Cast<RarEntry>().ForEach(static _ => { });
            SrcStream?.Dispose();

            disposed = true;
        }
    }

    public void EnsureEntriesLoaded()
    {
        lazyEntries.EnsureFullyLoaded();
        lazyVolumes.EnsureFullyLoaded();
    }
}
