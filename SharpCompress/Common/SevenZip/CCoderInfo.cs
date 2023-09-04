#nullable disable

using SharpCompress.Archives.SevenZip;

namespace SharpCompress.Common.SevenZip;

internal sealed class CCoderInfo
{
    internal ulong _methodId;
    internal byte[] _props;
    internal int _numInStreams;
    internal int _numOutStreams;

    internal void Reset(SevenZipContext context)
    {
        _methodId = 0;
        if (_props != null)
        {
            context.ByteArrayPool.Return(_props);
            _props = null;
        }
        _numInStreams = 0;
        _numOutStreams = 0;
    }
}
