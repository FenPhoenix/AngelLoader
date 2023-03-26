using System;

namespace SharpCompress.Common;

public sealed class CryptographicException : Exception
{
    public CryptographicException(string message)
        : base(message) { }
}
