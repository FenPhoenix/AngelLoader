/*
@SharpCompress: There's plenty of performance optimization we could still do here.
Lots of byte[] allocations, stream recreations, etc. However, we're already pretty fast, so it's not urgent.
*/

using System.IO;
using SharpCompress.Common.SevenZip;
using static AL_Common.Common;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchive
{
    private readonly Stream _srcStream;

    private readonly SevenZipContext _context;

    private ListFast<SevenZipArchiveEntry>? _lazyEntries;
    public ListFast<SevenZipArchiveEntry> Entries
    {
        get
        {
            if (_lazyEntries == null)
            {
                _srcStream.Position = 0;
                new ArchiveReader(_context).ReadDatabase(_srcStream);

                _lazyEntries = _context.ArchiveDatabase._files;
            }

            return _lazyEntries;
        }
    }

    /// <summary>
    /// Saves a bit of time by not reading the entry data, only the count.
    /// </summary>
    /// <returns></returns>
    public int GetEntryCountOnly()
    {
        _srcStream.Position = 0;
        return new ArchiveReader(_context).ReadDatabase(_srcStream, onlyGetEntryCount: true);
    }

    public SevenZipArchive(Stream stream) : this(stream, new SevenZipContext())
    {
    }

    public SevenZipArchive(Stream stream, SevenZipContext context)
    {
        _srcStream = stream;
        _context = context;
    }
}
