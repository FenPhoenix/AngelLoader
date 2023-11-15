namespace SharpCompress.Common;

public sealed class MultiVolumeExtractionException : ExtractionException
{
    public MultiVolumeExtractionException(string message)
        : base(message) { }

#if false
    public MultiVolumeExtractionException(string message, Exception inner)
        : base(message, inner) { }
#endif
}
