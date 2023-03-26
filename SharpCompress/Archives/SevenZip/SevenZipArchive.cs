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
                var database = reader.ReadDatabase();

                var entries = new SevenZipArchiveEntry[database._files.Count];
                for (var i = 0; i < database._files.Count; i++)
                {
                    CFileItem file = database._files[i];
                    entries[i] = new SevenZipArchiveEntry(file);
                }

                _lazyEntries = new ReadOnlyCollection<SevenZipArchiveEntry>(entries);
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
