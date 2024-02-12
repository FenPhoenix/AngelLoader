using System;
using SharpCompress.Common;

namespace SharpCompress.Readers;

public interface IReader : IDisposable
{
    IEntry Entry { get; }

    bool Cancelled { get; }
}
