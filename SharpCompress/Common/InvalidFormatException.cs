using System;

namespace SharpCompress_7z.Common;

public sealed class InvalidFormatException : ExtractionException
{
    public InvalidFormatException(string message)
        : base(message) { }

    public InvalidFormatException(string message, Exception inner)
        : base(message, inner) { }
}
