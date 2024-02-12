using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;

namespace SharpCompress.Readers;

/// <summary>
/// A generic push reader that reads unseekable comrpessed streams.
/// </summary>
public abstract class AbstractReader<TEntry, TVolume> : IReader
    where TEntry : Entry
    where TVolume : Volume
{
    private bool completed;
    private IEnumerator<TEntry>? entriesForCurrentReadStream;
    private bool wroteCurrentEntry;

    internal AbstractReader(OptionsBase options)
    {
        Options = options;
    }

    internal OptionsBase Options { get; }

    /// <summary>
    /// Current volume that the current entry resides in
    /// </summary>
    protected abstract TVolume Volume { get; }

    /// <summary>
    /// Current file entry
    /// </summary>
    public TEntry Entry => entriesForCurrentReadStream!.Current;

    #region IDisposable Members

    public void Dispose()
    {
        entriesForCurrentReadStream?.Dispose();
        Volume?.Dispose();
    }

    #endregion

    public bool Cancelled { get; }

    public bool MoveToNextEntry()
    {
        if (completed)
        {
            return false;
        }
        if (Cancelled)
        {
            throw new InvalidOperationException("Reader has been cancelled.");
        }
        if (entriesForCurrentReadStream is null)
        {
            return LoadStreamForReading(RequestInitialStream());
        }
        if (!wroteCurrentEntry)
        {
            SkipEntry();
        }
        wroteCurrentEntry = false;
        if (NextEntryForCurrentStream())
        {
            return true;
        }
        completed = true;
        return false;
    }

    private bool LoadStreamForReading(Stream stream)
    {
        entriesForCurrentReadStream?.Dispose();
        if ((stream is null) || (!stream.CanRead))
        {
            throw new MultipartStreamRequiredException(
                "File is split into multiple archives: '"
                    + Entry.Key
                    + "'. A new readable stream is required.  Use Cancel if it was intended."
            );
        }
        entriesForCurrentReadStream = GetEntries(stream).GetEnumerator();
        return entriesForCurrentReadStream.MoveNext();
    }

    protected virtual Stream RequestInitialStream() => Volume.Stream;

    private bool NextEntryForCurrentStream() => entriesForCurrentReadStream!.MoveNext();

    protected abstract IEnumerable<TEntry> GetEntries(Stream stream);

    #region Entry Skip/Write

    private void SkipEntry()
    {
        if (!Entry.IsDirectory)
        {
            Skip();
        }
    }

    private void Skip()
    {
        var part = Entry.Parts.First();

        if (!Entry.IsSolid && Entry.CompressedSize > 0)
        {
            //not solid and has a known compressed size then we can skip raw bytes.
            var rawStream = part.GetRawStream();

            if (rawStream != null)
            {
                var bytesToAdvance = Entry.CompressedSize;
                rawStream.Skip(bytesToAdvance);
                return;
            }
        }
        //don't know the size so we have to try to decompress to skip
        using var s = OpenEntryStream();
        s.SkipEntry();
    }

    public void WriteEntryTo(Stream writableStream)
    {
        if (wroteCurrentEntry)
        {
            throw new ArgumentException("WriteEntryTo or OpenEntryStream can only be called once.");
        }

        if (writableStream is null)
        {
            throw new ArgumentNullException(nameof(writableStream));
        }
        if (!writableStream.CanWrite)
        {
            throw new ArgumentException(
                "A writable Stream was required.  Use Cancel if that was intended."
            );
        }

        Write(writableStream);
        wroteCurrentEntry = true;
    }

    private void Write(Stream writeStream)
    {
        using Stream s = OpenEntryStream();
        s.TransferTo(writeStream);
    }

    public EntryStream OpenEntryStream()
    {
        if (wroteCurrentEntry)
        {
            throw new ArgumentException("WriteEntryTo or OpenEntryStream can only be called once.");
        }
        var stream = GetEntryStream();
        wroteCurrentEntry = true;
        return stream;
    }

    /// <summary>
    /// Retains a reference to the entry stream, so we can check whether it completed later.
    /// </summary>
    protected EntryStream CreateEntryStream(Stream decompressed) =>
        new EntryStream(this, decompressed);

    protected virtual EntryStream GetEntryStream() =>
        CreateEntryStream(Entry.Parts.First().GetCompressedStream());

    #endregion

    IEntry IReader.Entry => Entry;
}
