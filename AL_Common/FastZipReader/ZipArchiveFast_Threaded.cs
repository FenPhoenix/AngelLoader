// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System;
using System.IO;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.FastZipReader.ZipArchiveFast_Common;

namespace AL_Common.FastZipReader;

public sealed class ZipArchiveFast_Threaded : IDisposable
{
    private bool _isDisposed;
    private readonly Stream? _backingStream;

    private readonly Stream _archiveStream;
    private readonly long ArchiveStreamLength;

    private readonly ZipContext_Threaded _context;

    private readonly bool _disposeContext;

    public ZipArchiveFast_Threaded(
        Stream stream) :
        this(
            stream: stream,
            context: new ZipContext_Threaded(),
            disposeContext: true)
    {
    }

    [PublicAPI]
    public ZipArchiveFast_Threaded(
        Stream stream,
        ZipContext_Threaded context) :
        this(
            stream: stream,
            context: context,
            disposeContext: false)
    {
    }

    [PublicAPI]
    private ZipArchiveFast_Threaded(
        Stream stream,
        ZipContext_Threaded context,
        bool disposeContext)
    {
        _disposeContext = disposeContext;

        if (stream == null) throw new ArgumentNullException(nameof(stream));

        _context = context;

        // Fen's note: Inlined Init() for nullable detection purposes...
        #region Init

        Stream? extraTempStream = null;

        try
        {
            _backingStream = null;

            if (!stream.CanRead)
            {
                ThrowHelper.ReadModeCapabilities();
            }
            if (!stream.CanSeek)
            {
                _backingStream = stream;
                extraTempStream = stream = new MemoryStream();
                _backingStream.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            _archiveStream = stream;
            ArchiveStreamLength = _archiveStream.Length;

            context.ArchiveSubReadStream.SetSuperStream(_archiveStream);
        }
        catch
        {
            extraTempStream?.Dispose();
            throw;
        }

        #endregion
    }

    private Stream OpenEntry(ZipArchiveFastEntry entry)
    {
        ThrowIfDisposed();

        if (!IsOpenable(entry, _archiveStream, ArchiveStreamLength, _context.BinaryReadBuffer, out string message))
        {
            ThrowHelper.InvalidData(message);
        }

        // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true

        // @MT_TASK: Zip context (threaded mode) used field: ArchiveSubReadStream
        _context.ArchiveSubReadStream.Set((long)entry.StoredOffsetOfCompressedData!, entry.CompressedLength);

        return GetDataDecompressor(entry, _context.ArchiveSubReadStream);
    }

    public void ExtractToFile_Fast(
        ZipArchiveFastEntry entry,
        string fileName,
        bool overwrite,
        byte[] tempBuffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.None))
        using (Stream source = OpenEntry(entry))
        {
            StreamCopyNoAlloc(source, destination, tempBuffer);
        }
        File.SetLastWriteTime(fileName, ZipHelpers.ZipTimeToDateTime(entry.LastWriteTime));
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(ZipArchiveFast).ToString());
        }
    }

    #region Dispose

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _archiveStream.Dispose();
            // @MT_TASK: Zip context (threaded mode) used field: ArchiveSubReadStream (Dispose)
            _context.ArchiveSubReadStream.SetSuperStream(null);
            _backingStream?.Dispose();

            // @MT_TASK: Zip context (threaded mode) dispose - make sure this doesn't cause problems either
            if (_disposeContext) _context.Dispose();

            _isDisposed = true;
        }
    }

    public void Dispose() => Dispose(true);

    #endregion
}
