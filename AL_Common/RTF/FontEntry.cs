using System.Runtime.InteropServices;

namespace AL_Common.RTF;

// Sequential makes it 4 bytes on all targets. If Auto, then it's 8 bytes on .NET Framework x64.
[StructLayout(LayoutKind.Sequential)]
internal readonly struct FontEntry
{
    internal readonly ushort CodePage;
    internal readonly bool IsSet;
    internal readonly SymbolFont SymbolFont;

    internal FontEntry(ushort codePage, SymbolFont symbolFont)
    {
        CodePage = codePage;
        SymbolFont = symbolFont;
        IsSet = true;
    }
}
