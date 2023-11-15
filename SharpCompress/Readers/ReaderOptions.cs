using SharpCompress.Common;

namespace SharpCompress.Readers;

public sealed class ReaderOptions
{
    /// <summary>
    /// Look for RarArchive (Check for self-extracting archives or cases where RarArchive isn't at the start of the file)
    /// </summary>
    public bool LookForHeader { get; set; }

    public string? Password { get; set; }

    public bool DisableCheckIncomplete { get; set; }

    /// <summary>
    /// SharpCompress will keep the supplied streams open.  Default is true.
    /// </summary>
    public bool LeaveStreamOpen { get; set; } = true;

    public ArchiveEncoding ArchiveEncoding { get; } = new ArchiveEncoding();
}
