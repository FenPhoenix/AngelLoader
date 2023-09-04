using System;
using static AL_Common.Common;

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

    public void Set(ArchiveReader archive, ListFast<byte> dataVector)
    {
        Dispose();
        _archive = archive;
        _archive.AddByteStream(dataVector.ItemsArray, 0, dataVector.Count);
        _needRemove = true;
    }

    public void Set(ArchiveReader archive, ListFast<ListFast<byte>> dataVector)
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
            _archive.AddByteStream(dataVector[dataIndex].ItemsArray, 0, dataVector[dataIndex].Count);
            _needRemove = true;
        }
    }
}
