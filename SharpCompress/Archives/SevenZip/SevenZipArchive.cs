/*
@SharpCompress: There's plenty of performance optimization we could still do here.
Lots of byte[] allocations, stream recreations, etc. However, we're already pretty fast, so it's not urgent.
*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using SharpCompress.Common.SevenZip;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchive : IDisposable
{
    private bool _disposed;
    private readonly Stream _srcStream;

    private readonly SevenZipContext _context;

    // @SharpCompress: Reuse one list of entries like we do with zips
    private ReadOnlyCollection<SevenZipArchiveEntry>? _lazyEntries;
    public ReadOnlyCollection<SevenZipArchiveEntry> Entries
    {
        get
        {
            if (_lazyEntries == null)
            {
                _srcStream.Position = 0;
                ArchiveReader reader = new(_context);
                reader.Open(_srcStream);
                ArchiveDatabase database = reader.ReadDatabase();

                _lazyEntries = new ReadOnlyCollection<SevenZipArchiveEntry>(database._files);
            }

            return _lazyEntries;
        }
    }

    public SevenZipArchive(Stream stream) : this(stream, new SevenZipContext())
    {
    }

    public SevenZipArchive(Stream stream, SevenZipContext context)
    {
        _srcStream = stream;
        _context = context;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _srcStream.Dispose();
            _disposed = true;
        }
    }
}
