using System;
using System.Collections.Generic;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.IO;
using SharpCompress.Readers;

namespace SharpCompress.Archives;

public abstract class AbstractArchive<TEntry, TVolume> : IArchive, IArchiveExtractionListener
    where TEntry : IArchiveEntry
    where TVolume : IDisposable
{
    private readonly LazyReadOnlyCollection<TVolume> lazyVolumes;
    private readonly LazyReadOnlyCollection<TEntry> lazyEntries;

    public event EventHandler<ArchiveExtractionEventArgs<IArchiveEntry>>? EntryExtractionBegin;
    public event EventHandler<ArchiveExtractionEventArgs<IArchiveEntry>>? EntryExtractionEnd;

    public event EventHandler<CompressedBytesReadEventArgs>? CompressedBytesRead;
    public event EventHandler<FilePartExtractionBeginEventArgs>? FilePartExtractionBegin;

    protected ReaderOptions ReaderOptions { get; }

    private bool disposed;
    protected SourceStream SrcStream;

    internal AbstractArchive(ArchiveType type, SourceStream srcStream)
    {
        Type = type;
        ReaderOptions = srcStream.ReaderOptions;
        SrcStream = srcStream;
        lazyVolumes = new LazyReadOnlyCollection<TVolume>(LoadVolumes(SrcStream));
        lazyEntries = new LazyReadOnlyCollection<TEntry>(LoadEntries(Volumes));
    }

    public ArchiveType Type { get; }

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
    public virtual long TotalSize =>
        Entries.Aggregate(0L, (total, cf) => total + cf.CompressedSize);

    /// <summary>
    /// The total size of the files as uncompressed in the archive.
    /// </summary>
    public virtual long TotalUncompressSize =>
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

    void IArchiveExtractionListener.EnsureEntriesLoaded()
    {
        lazyEntries.EnsureFullyLoaded();
        lazyVolumes.EnsureFullyLoaded();
    }

    void IExtractionListener.FireCompressedBytesRead(
        long currentPartCompressedBytes,
        long compressedReadBytes
    ) =>
        CompressedBytesRead?.Invoke(
            this,
            new CompressedBytesReadEventArgs(
                currentFilePartCompressedBytesRead: currentPartCompressedBytes,
                compressedBytesRead: compressedReadBytes
            )
        );

    void IExtractionListener.FireFilePartExtractionBegin(
        string name,
        long size,
        long compressedSize
    ) =>
        FilePartExtractionBegin?.Invoke(
            this,
            new FilePartExtractionBeginEventArgs(
                compressedSize: compressedSize,
                size: size,
                name: name
            )
        );

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
        ((IArchiveExtractionListener)this).EnsureEntriesLoaded();
        return CreateReaderForSolidExtraction();
    }

    protected abstract IReader CreateReaderForSolidExtraction();

    /// <summary>
    /// Archive is SOLID (this means the Archive saved bytes by reusing information which helps for archives containing many small files).
    /// </summary>
    public virtual bool IsSolid => false;

    /// <summary>
    /// The archive can find all the parts of the archive needed to fully extract the archive.  This forces the parsing of the entire archive.
    /// </summary>
    public bool IsComplete
    {
        get
        {
            ((IArchiveExtractionListener)this).EnsureEntriesLoaded();
            return Entries.All(x => x.IsComplete);
        }
    }
}
