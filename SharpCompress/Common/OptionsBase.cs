namespace SharpCompress.Common;

public sealed class OptionsBase
{
    /// <summary>
    /// SharpCompress will keep the supplied streams open.  Default is true.
    /// </summary>
    public bool LeaveStreamOpen { get; set; } = true;
}
