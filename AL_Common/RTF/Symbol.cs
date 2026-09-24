namespace AL_Common.RTF;

internal sealed class Symbol
{
    internal readonly string Keyword;
    internal readonly int DefaultParam;
    internal readonly bool UseDefaultParam;
    internal readonly KeywordType KeywordType;
    /// <summary>
    /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
    /// </summary>
    internal readonly ushort Index;

    internal Symbol(string keyword, int defaultParam, bool useDefaultParam, KeywordType keywordType, ushort index)
    {
        Keyword = keyword;
        DefaultParam = defaultParam;
        UseDefaultParam = useDefaultParam;
        KeywordType = keywordType;
        Index = index;
    }
}
