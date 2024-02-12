using System;
using System.IO;
using SharpCompress.IO;

namespace SharpCompress.Common;

public abstract class Volume : IDisposable
{
    private readonly Stream _actualStream;

    internal Volume(Stream stream, int index = 0)
    {
        _actualStream = NonDisposingStream.Create(stream);
    }

    internal Stream Stream => _actualStream;

#if false
    protected OptionsBase OptionsBase { get; }

    public string FileName => (_actualStream as FileStream)?.Name!;
#endif

    private void Dispose(bool disposing)
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
