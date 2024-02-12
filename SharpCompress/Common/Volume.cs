using System;
using System.IO;
using SharpCompress.IO;

namespace SharpCompress.Common;

public abstract class Volume : IDisposable
{
    private readonly Stream _actualStream;

    internal Volume(Stream stream, OptionsBase OptionsBase, int index = 0)
    {
        Index = index;
        OptionsBase = OptionsBase;
        if (OptionsBase.LeaveStreamOpen)
        {
            stream = NonDisposingStream.Create(stream);
        }
        _actualStream = stream;
    }

    internal Stream Stream => _actualStream;

    protected OptionsBase OptionsBase { get; }

    public int Index { get; }

    public string FileName => (_actualStream as FileStream)?.Name!;

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _actualStream.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
