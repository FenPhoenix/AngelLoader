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

    private ReadOnlyCollection<SevenZipArchiveEntry>? _lazyEntries;
    public ReadOnlyCollection<SevenZipArchiveEntry> Entries
    {
        get
        {
            if (_lazyEntries == null)
            {
                _srcStream.Position = 0;
                var reader = new ArchiveReader();
                reader.Open(_srcStream);
                ArchiveDatabase database = reader.ReadDatabase();

                _lazyEntries = new ReadOnlyCollection<SevenZipArchiveEntry>(database._files);
            }

            return _lazyEntries;
        }
    }

    public SevenZipArchive(Stream stream) => _srcStream = new SourceStream(stream);

    public void Dispose()
    {
        if (!_disposed)
        {
            _srcStream.Dispose();
            _disposed = true;
        }
    }
}
