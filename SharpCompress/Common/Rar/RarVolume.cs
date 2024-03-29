using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.IO;

namespace SharpCompress.Common.Rar;

/// <summary>
/// A RarArchiveVolume is a single rar file that may or may not be a split RarArchive.  A Rar Archive is one to many Rar Parts
/// </summary>
public abstract class RarVolume : Volume
{
    private readonly RarHeaderFactory _headerFactory;
    private int _maxCompressionAlgorithm;

    internal RarVolume(StreamingMode mode, Stream stream, int index = 0)
        : base(stream) => _headerFactory = new RarHeaderFactory(mode);

#nullable disable
    private ArchiveHeader ArchiveHeader;

#nullable enable

    private StreamingMode Mode => _headerFactory.StreamingMode;

    internal abstract IEnumerable<RarFilePart> ReadFileParts();

    internal abstract RarFilePart CreateFilePart(FileHeader fileHeader);

    internal IEnumerable<RarFilePart> GetVolumeFileParts()
    {
        foreach (var header in _headerFactory.ReadHeaders(Stream))
        {
            switch (header.HeaderType)
            {
                case HeaderType.Mark:

                {
                }
                break;
                case HeaderType.Archive:

                {
                    ArchiveHeader = (ArchiveHeader)header;
                }
                break;
                case HeaderType.File:

                {
                    var fh = (FileHeader)header;
                    if (_maxCompressionAlgorithm < fh.CompressionAlgorithm)
                    {
                        _maxCompressionAlgorithm = fh.CompressionAlgorithm;
                    }

                    yield return CreateFilePart(fh);
                }
                break;
                case HeaderType.Service:

                {
                    var fh = (FileHeader)header;
                    if (fh.FileName == "CMT")
                    {
                        var part = CreateFilePart(fh);
                        var buffer = new byte[fh.CompressedSize];
                        part.GetCompressedStream().Read(buffer, 0, buffer.Length);
                        System.Text.Encoding.UTF8.GetString(
                            buffer,
                            0,
                            buffer.Length - 1
                        );
                    }
                }
                break;
            }
        }
    }

    private void EnsureArchiveHeaderLoaded()
    {
        if (ArchiveHeader is null)
        {
            if (Mode == StreamingMode.Streaming)
            {
                throw new InvalidOperationException(
                    "ArchiveHeader should never been null in a streaming read."
                );
            }

            // we only want to load the archive header to avoid overhead but have to do the nasty thing and reset the stream
            GetVolumeFileParts().First();
            Stream.Position = 0;
        }
    }

    /// <summary>
    /// RarArchive is part of a multi-part archive.
    /// </summary>
    public bool IsMultiVolume
    {
        get
        {
            EnsureArchiveHeaderLoaded();
            return ArchiveHeader.IsVolume;
        }
    }

    /// <summary>
    /// RarArchive is SOLID (this means the Archive saved bytes by reusing information which helps for archives containing many small files).
    /// Currently, SharpCompress cannot decompress SOLID archives.
    /// </summary>
    public bool IsSolidArchive
    {
        get
        {
            EnsureArchiveHeaderLoaded();
            return ArchiveHeader.IsSolid;
        }
    }
}
