namespace SharpCompress.Common;

public sealed class IncompleteArchiveException : ArchiveException
{
    public IncompleteArchiveException(string message)
        : base(message) { }
}
