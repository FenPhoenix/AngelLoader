using System.Runtime.CompilerServices;

namespace AL_Common;

/*
This is so we can do FAST allocation-free deserialization of the comma-separated languages string.
We simply ensure internal language names are always stored and written out in ascii lowercase, so this
simple hash lookup can work without any string casing issues.

Unfortunately it's not so easy to regenerate it, you need to use gperf in a similar process as described
in the RTF symbol list generator file. Meh.
*/

public static partial class LanguageSupport
{
    /* ANSI-C code produced by gperf version 3.1 */
    /* Command-line: gperf --output-file='C:\\gperf_out.txt' -t 'C:\\gperf_in.txt'  */
    /* Computed positions: -k'1' */

    //private const int TOTAL_KEYWORDS = 11;
    private const int MIN_WORD_LENGTH = 5;
    private const int MAX_WORD_LENGTH = 9;
    //private const int MIN_HASH_VALUE = 5;
    private const int MAX_HASH_VALUE = 22;
    /* maximum key range = 18, duplicates = 0 */

    private static readonly byte[] asso_values =
    {
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 5,
        0, 15, 10, 5, 0, 10, 0, 23, 23, 23,
        23, 23, 0, 23, 5, 0, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
        23, 23, 23, 23, 23, 23
    };

    private sealed class LangNameAndEnumField
    {
        internal readonly string LangName;
        internal readonly Language EnumField;
        internal LangNameAndEnumField(string langName, Language enumField)
        {
            LangName = langName;
            EnumField = enumField;
        }
    }

    private static readonly LangNameAndEnumField?[] _perfectHash_LangList =
    {
        null, null, null, null, null,
        new ("dutch", Language.Dutch),
        new ("polish", Language.Polish),
        new ("spanish", Language.Spanish),
        new ("japanese", Language.Japanese),
        new ("hungarian", Language.Hungarian),
        new ("czech", Language.Czech),
        new ("german", Language.German),
        new ("russian", Language.Russian),
        null, null, null,
        new ("french", Language.French),
        new ("italian", Language.Italian),
        null, null, null, null,
        new ("english", Language.English)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash(string str, int start, int len)
    {
        uint hval = (uint)len;
        return hval + asso_values[str[start]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SeqEqual(string seq1, int seq1Start, int seq1Length, string seq2)
    {
        if (seq1Length != seq2.Length) return false;

        for (int seq1i = seq1Start, seq2i = 0; seq1i < seq1Start + seq1Length; seq1i++, seq2i++)
        {
            if (seq1[seq1i] != seq2[seq2i]) return false;
        }
        return true;
    }

    public static bool Langs_TryGetValue(string str, int start, int end, out Language result)
    {
        int len = end - start;
        if (len is <= MAX_WORD_LENGTH and >= MIN_WORD_LENGTH)
        {
            uint key = Hash(str, start, len);

            if (key <= MAX_HASH_VALUE)
            {
                LangNameAndEnumField? language = _perfectHash_LangList[key];
                if (language == null)
                {
                    result = Language.Default;
                    return false;
                }

                if (SeqEqual(str, start, len, language.LangName))
                {
                    result = language.EnumField;
                    return true;
                }
            }
        }

        result = Language.Default;
        return false;
    }
}
