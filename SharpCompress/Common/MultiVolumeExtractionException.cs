namespace SharpCompress.Common;

public sealed class MultiVolumeExtractionException : ExtractionException
{
    public MultiVolumeExtractionException(string message)
        : base(message) { }

#if false
    public MultiVolumeExtractionException(string message, System.Exception inner)
        : base(message, inner) { }
#endif
}
