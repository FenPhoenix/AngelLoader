using SharpCompress.Common;

namespace SharpCompress.Readers;

public sealed class ReaderOptions : OptionsBase
{
    /// <summary>
    /// Look for RarArchive (Check for self-extracting archives or cases where RarArchive isn't at the start of the file)
    /// </summary>
    public bool LookForHeader { get; set; }

    public bool DisableCheckIncomplete { get; set; }
}
