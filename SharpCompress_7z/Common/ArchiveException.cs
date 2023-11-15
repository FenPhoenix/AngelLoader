using System;

namespace SharpCompress_7z.Common;

public class ArchiveException : Exception
{
    public ArchiveException(string message)
        : base(message) { }
}
