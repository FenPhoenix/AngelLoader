using JetBrains.Annotations;

namespace FMInfoGen;

internal static class Misc
{
    internal static ConfigData Config { get; } = new ConfigData();

    // Class instead of enum so we don't have to keep casting its fields
    [PublicAPI]
    internal static class ByteSize
    {
        internal const int KB = 1024;
        internal const int MB = KB * 1024;
        internal const int GB = MB * 1024;
    }
}
