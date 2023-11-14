using System;

namespace SharpCompress_7z.Common;

public sealed class CryptographicException : Exception
{
    public CryptographicException(string message)
        : base(message) { }
}
