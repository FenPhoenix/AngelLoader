namespace SharpCompress.Common;

public sealed class MultipartStreamRequiredException : ExtractionException
{
    public MultipartStreamRequiredException(string message)
        : base(message) { }
}
