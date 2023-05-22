using System;
using System.Collections.Generic;

namespace SharpCompress.Common.SevenZip;

internal ref struct CStreamSwitch
{
    private ArchiveReader _archive;
    private bool _needRemove;

    public void Dispose()
    {
        if (_needRemove)
        {
            _needRemove = false;
            _archive.DeleteByteStream();
        }
    }

    public void Set(ArchiveReader archive, byte[] dataVector)
    {
        Dispose();
        _archive = archive;
        _archive.AddByteStream(dataVector, 0, dataVector.Length);
        _needRemove = true;
    }

    public void Set(ArchiveReader archive, List<byte[]> dataVector)
    {
        Dispose();

        byte external = archive.ReadByte();
        if (external != 0)
        {
            int dataIndex = archive.ReadNum();
            if (dataIndex < 0 || dataIndex >= dataVector.Count)
            {
                throw new InvalidOperationException();
            }

            _archive = archive;
            _archive.AddByteStream(dataVector[dataIndex], 0, dataVector[dataIndex].Length);
            _needRemove = true;
        }
    }
}
