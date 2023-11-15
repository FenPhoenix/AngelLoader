using System;

namespace SharpCompress.Readers;

public interface IReader : IDisposable
{
    bool Cancelled { get; }
}
