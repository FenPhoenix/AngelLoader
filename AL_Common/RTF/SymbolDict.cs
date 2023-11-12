using System.Runtime.CompilerServices;

namespace AL_Common;

public static partial class RTFParserCommon
{
    public sealed partial class SymbolDict
    {
        // For "emspace", "enspace", "qmspace", "~"
        // Just convert these to regular spaces because we're just trying to scan for strings in readmes
        // without weird crap tripping us up
        // emspace  '\x2003'
        // enspace  '\x2002'
        // qmspace  '\x2005'
        // ~        '\xa0'

        // For "emdash", "endash", "lquote", "rquote", "ldblquote", "rdblquote"
        // NOTE: Maybe just convert these all to ASCII equivalents as well?

        // For "cs", "ds", "ts"
        // Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.

        // For "listtext", "pntext"
        // Ignore list item bullets and numeric prefixes etc. We don't need them.

        // For "v"
        // \v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again

        // For "ansi"
        // The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
        // "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.

        // For "mac"
        // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
        // assume 10000 ("Mac Roman")

        // For "fldinst"
        // We need to do stuff with this (SYMBOL instruction)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Symbol? LookUpControlSymbol(char ch)
        {
            byte val = asso_values[ch];
            int key = 1 + val + val;

            if (key <= MAX_HASH_VALUE)
            {
                Symbol? symbol = _symbolTable[key];
                if (symbol == null)
                {
                    return null;
                }

                string seq2 = symbol.Keyword;
                if (seq2.Length != 1)
                {
                    return null;
                }

                if (ch != seq2[0])
                {
                    return null;
                }

                return symbol;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Symbol? LookUpControlWord(char[] keyword, int len)
        {
            // Min word length is 1, and we're guaranteed to always be at least 1, so no need to check for >= min
            if (len <= MAX_WORD_LENGTH)
            {
                int key = Hash(keyword,len);

                if (key <= MAX_HASH_VALUE)
                {
                    Symbol? symbol = _symbolTable[key];
                    if (symbol == null)
                    {
                        return null;
                    }

                    string seq2 = symbol.Keyword;
                    if (len != seq2.Length)
                    {
                        return null;
                    }

                    for (int ci = 0; ci < len; ci++)
                    {
                        if (keyword[ci] != seq2[ci])
                        {
                            return null;
                        }
                    }

                    return symbol;
                }
            }

            return null;
        }
    }
}
