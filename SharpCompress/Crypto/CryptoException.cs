using System;

namespace SharpCompress.Crypto;

public class CryptoException : Exception
{
#if false
    public CryptoException() { }

    public CryptoException(string message, Exception exception)
        : base(message, exception) { }

#endif
    public CryptoException(string message)
        : base(message) { }
}
