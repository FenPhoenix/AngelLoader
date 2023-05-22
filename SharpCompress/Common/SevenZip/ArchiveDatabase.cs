#nullable disable

using System;
using System.Collections.Generic;
using SharpCompress.Archives.SevenZip;

namespace SharpCompress.Common.SevenZip;

internal sealed class ArchiveDatabase
{
    internal byte _majorVersion;
    internal byte _minorVersion;
    internal long _startPositionAfterHeader;

    internal List<CFolder> _folders = new();
    internal List<int> _numUnpackStreamsVector;
    internal List<SevenZipArchiveEntry> _files = new();

    internal void Clear()
    {
        _folders.Clear();
        _numUnpackStreamsVector = null!;
        _files.Clear();
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

    public void Fill() => FillFolderStartFileIndex();
}
