#nullable disable

using System;
using System.Runtime.CompilerServices;
using SharpCompress.Archives.SevenZip;
using static AL_Common.Common;

namespace SharpCompress.Common.SevenZip;

internal sealed class ArchiveDatabase
{
    internal byte _majorVersion;
    internal byte _minorVersion;
    internal long _startPositionAfterHeader;

    internal readonly ListFast<CFolder> _folders = new(0);
    internal readonly ListFast<long> PackSizes = new(0);
    internal readonly ListFast<int> _numUnpackStreamsVector = new(0);
    internal readonly ListFast<long> UnpackSizes = new(16);
    internal readonly ListFast<SevenZipArchiveEntry> _files = new(0);
    internal readonly ListFast<byte> Header = new(0);
    internal readonly ListFast<ListFast<byte>> DataVector = new(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Clear()
    {
        _majorVersion = 0;
        _minorVersion = 0;
        _startPositionAfterHeader = 0;

        _folders.ClearFast();
        PackSizes.ClearFast();
        _numUnpackStreamsVector.ClearFast();
        UnpackSizes.ClearFast();
        _files.ClearFast();
        Header.ClearFast();
        DataVector.ClearFast();
    }

    private void FillFolderStartFileIndex()
    {
        int folderIndex = 0;
        int indexInFolder = 0;
        for (int i = 0; i < _files.Count; i++)
        {
            SevenZipArchiveEntry file = _files[i];

            bool emptyStream = !file.HasStream;

            if (emptyStream && indexInFolder == 0)
            {
                continue;
            }

            if (indexInFolder == 0)
            {
                // v3.13 incorrectly worked with empty folders
                // v4.07: Loop for skipping empty folders
                for (; ; )
                {
                    if (folderIndex >= _folders.Count)
                    {
                        throw new InvalidOperationException();
                    }

                    if (_numUnpackStreamsVector![folderIndex] != 0)
                    {
                        break;
                    }

                    folderIndex++;
                }
            }

            if (emptyStream)
            {
                continue;
            }

            indexInFolder++;

            if (indexInFolder >= _numUnpackStreamsVector![folderIndex])
            {
                folderIndex++;
                indexInFolder = 0;
            }
        }
    }

    internal void Fill() => FillFolderStartFileIndex();
}
