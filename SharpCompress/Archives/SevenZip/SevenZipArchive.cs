using System;
using System.Collections.ObjectModel;
using System.IO;
using SharpCompress.Common.SevenZip;
using SharpCompress.IO;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchive : IDisposable
{
    private bool _disposed;
    private readonly SourceStream _srcStream;

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
                var reader = new ArchiveReader(_context);
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
        _srcStream = new SourceStream(stream);
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
