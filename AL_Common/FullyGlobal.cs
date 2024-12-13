using System;
using System.Text;

namespace AL_Common;

/// <summary>
/// For absolutely hardcore everything-needs-access stuff
/// </summary>
public static class FullyGlobal
{
    /// <summary>
    /// Shorthand for <see cref="Environment.NewLine"/>
    /// </summary>
    public static readonly string NL = Environment.NewLine;

    public const int FileStreamBufferSize = 4096;

    // Immediately static init for thread safety
    public static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);
}
