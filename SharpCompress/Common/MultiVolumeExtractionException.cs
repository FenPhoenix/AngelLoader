using System;

namespace SharpCompress_7z.Common;

public sealed class MultiVolumeExtractionException : ExtractionException
{
    public MultiVolumeExtractionException(string message)
        : base(message) { }

    public MultiVolumeExtractionException(string message, Exception inner)
        : base(message, inner) { }
}
