//#define PATCH_ALL_LANGS

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AL_Common.RTF;

public static class RtfCommon
{
    public const int KeywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    public const int ParamMaxLen = 10;

    public const int UndefinedLanguage = 1024;

    /// <summary>
    /// Since font numbers can be negative, let's just use a slightly less likely value than the already unlikely
    /// enough -1...
    /// </summary>
    public const int NoFontNumber = int.MinValue;

    public const int MaxLangNumDigits = 5;
    public const int MaxLangNumIndex = 21514;
    public const ushort NoLang = ushort.MaxValue;

    public const ushort NoCodePage = ushort.MaxValue;

    public const int KeywordParseMaxRequiredBytes =
        KeywordMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1 + // Minus sign
        ParamMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1; // Space at end

    /*
    FMs can have 100+ of these...
    Highest measured was 131
    Fonts can specify themselves as whatever number they want, so we can't just count by index
    eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45

    The size of a Dictionary<int, FontEntry> with 150 entries when FontEntry is 4 bytes would be approximately
    3 KB (from looking at its internal memory use per entry, which is larger than just the KeyValuePair<>).
    That's totally fine to keep around permanently.
    */
    public const int FontTableDefaultCapacity = 150;

    // "\bin"
    public const int BinLength = 4;
    public const uint BinUInt = 0x6E69625Cu;

    // Perf: On modern .NET, the "ReadOnlySpan<> x =>" pattern removes bounds checking (assuming you index with a
    // numeric type that's <= the length of the span), and generates only a tiny amount of asm. But on Framework,
    // the JIT doesn't recognize the pattern, and performance is catastrophic. So we have to use an old-fashioned
    // bounds-checked array.
    public static readonly bool[] IsNonPlainText =
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false,
        true, // '\\' (92)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        true, // '{' (123)
        false,
        true, // '}' (125)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
    ];

    public static readonly bool[] IsIgnoreChar =
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false,
    ];

    public static readonly ushort[] LangToCodePage = RunFunc(static () =>
    {
        ushort[] langToCodePage = InitializedArray(MaxLangNumIndex + 1, NoCodePage);

        /*
        Generated from the list here: https://github.com/lovasoa/lcid-to-codepage/blob/main/windows_locales_extended.csv
        And then trimmed down to only the ones listed in the RTF spec.

        Note: 1024 is implicitly rejected by simply not being in the list, so we're all good there.

        2023-03-31: Only handle 1049 for now (and leave in 1033 for the plaintext converter).
        */

#if PATCH_ALL_LANGS
        langToCodePage[1025] = 1256;  // Arabic / Saudi Arabia
        langToCodePage[1026] = 1251;  // Bulgarian / Bulgaria
        langToCodePage[1027] = 1252;  // Catalan / Spain
        langToCodePage[1028] = 950;   // Chinese (Traditional) / Taiwan
        langToCodePage[1029] = 1250;  // Czech / Czechia
        langToCodePage[1030] = 1252;  // Danish / Denmark
        langToCodePage[1031] = 1252;  // German / Germany
        langToCodePage[1032] = 1253;  // Greek / Greece
#endif
        langToCodePage[1033] = 1252;  // English / United States
#if PATCH_ALL_LANGS
        langToCodePage[1034] = 1252;  // Spanish / Spain
        langToCodePage[1035] = 1252;  // Finnish / Finland
        langToCodePage[1036] = 1252;  // French / France
        langToCodePage[1037] = 1255;  // Hebrew / Israel
        langToCodePage[1038] = 1250;  // Hungarian / Hungary
        langToCodePage[1039] = 1252;  // Icelandic / Iceland
        langToCodePage[1040] = 1252;  // Italian / Italy
        langToCodePage[1041] = 932;   // Japanese / Japan
        langToCodePage[1042] = 949;   // Korean / Korea
        langToCodePage[1043] = 1252;  // Dutch / Netherlands
        langToCodePage[1044] = 1252;  // Norwegian Bokmål / Norway
        langToCodePage[1045] = 1250;  // Polish / Poland
        langToCodePage[1046] = 1252;  // Portuguese / Brazil
        langToCodePage[1047] = 1252;  // Romansh / Switzerland
        langToCodePage[1048] = 1250;  // Romanian / Romania
#endif
        langToCodePage[1049] = 1251;  // Russian / Russia
#if PATCH_ALL_LANGS
        langToCodePage[1050] = 1250;  // Croatian / Croatia
        langToCodePage[1051] = 1250;  // Slovak / Slovakia
        langToCodePage[1052] = 1250;  // Albanian / Albania
        langToCodePage[1053] = 1252;  // Swedish / Sweden
        langToCodePage[1054] = 874;   // Thai / Thailand
        langToCodePage[1055] = 1254;  // Turkish / Türkiye
        langToCodePage[1056] = 1256;  // Urdu / Pakistan
        langToCodePage[1057] = 1252;  // Indonesian / Indonesia
        langToCodePage[1058] = 1251;  // Ukrainian / Ukraine
        langToCodePage[1059] = 1251;  // Belarusian / Belarus
        langToCodePage[1060] = 1250;  // Slovenian / Slovenia
        langToCodePage[1061] = 1257;  // Estonian / Estonia
        langToCodePage[1062] = 1257;  // Latvian / Latvia
        langToCodePage[1063] = 1257;  // Lithuanian / Lithuania
        langToCodePage[1064] = 1251;  // Tajik (Cyrillic) / Tajikistan
        langToCodePage[1065] = 1256;  // Persian / Iran
        langToCodePage[1066] = 1258;  // Vietnamese / Vietnam
        langToCodePage[1068] = 1254;  // Azerbaijani (Latin) / Azerbaijan
        langToCodePage[1069] = 1252;  // Basque / Spain
        langToCodePage[1070] = 1252;  // Upper Sorbian / Germany
        langToCodePage[1071] = 1251;  // Macedonian / North Macedonia
        langToCodePage[1074] = 1252;  // Setswana / South Africa
        langToCodePage[1076] = 1252;  // isiXhosa / South Africa
        langToCodePage[1077] = 1252;  // isiZulu / South Africa
        langToCodePage[1078] = 1252;  // Afrikaans / South Africa
        langToCodePage[1080] = 1252;  // Faroese / Faroe Islands
        langToCodePage[1083] = 1252;  // Northern Sami / Norway
        langToCodePage[1086] = 1252;  // Malay / Malaysia
        langToCodePage[1088] = 1251;  // Kyrgyz / Kyrgyzstan
        langToCodePage[1089] = 1252;  // Kiswahili / Kenya
        langToCodePage[1090] = 1250;  // Turkmen / Turkmenistan
        langToCodePage[1091] = 1254;  // Uzbek (Latin) / Uzbekistan
        langToCodePage[1092] = 1251;  // Tatar / Russia
        langToCodePage[1104] = 1251;  // Mongolian / Mongolia
        langToCodePage[1106] = 1252;  // Welsh / United Kingdom
        langToCodePage[1110] = 1252;  // Galician / Spain
        langToCodePage[1119] = 1256;  // Central Atlas Tamazight (Arabic) / Morocco
        langToCodePage[1122] = 1252;  // Western Frisian / Netherlands
        langToCodePage[1124] = 1252;  // Filipino / Philippines
        langToCodePage[1126] = 1252;  // Edo / Nigeria
        langToCodePage[1127] = 1252;  // Fulah (Latin) / Nigeria
        langToCodePage[1128] = 1252;  // Hausa (Latin) / Nigeria
        langToCodePage[1129] = 1252;  // Ibibio / Nigeria
        langToCodePage[1130] = 1252;  // Yoruba / Nigeria
        langToCodePage[1131] = 1252;  // Quechua / Bolivia
        langToCodePage[1132] = 1252;  // Sesotho sa Leboa / South Africa
        langToCodePage[1133] = 1251;  // Bashkir / Russia
        langToCodePage[1134] = 1252;  // Luxembourgish / Luxembourg
        langToCodePage[1135] = 1252;  // Kalaallisut / Greenland
        langToCodePage[1136] = 1252;  // Igbo / Nigeria
        langToCodePage[1137] = 1252;  // Kanuri / Nigeria
        langToCodePage[1140] = 1252;  // Guarani / Paraguay
        langToCodePage[1141] = 1252;  // Hawaiian / United States
        langToCodePage[1142] = 1252;  // Latin / World
        langToCodePage[1145] = 1252;  // Papiamento / Caribbean
        langToCodePage[1146] = 1252;  // Mapuche / Chile
        langToCodePage[1148] = 1252;  // Mohawk / Canada
        langToCodePage[1150] = 1252;  // Breton / France
        langToCodePage[1152] = 1256;  // Uyghur / China
        langToCodePage[1154] = 1252;  // Occitan / France
        langToCodePage[1155] = 1252;  // Corsican / France
        langToCodePage[1156] = 1252;  // Alsatian / France
        langToCodePage[1157] = 1251;  // Sakha / Russia
        langToCodePage[1158] = 1252;  // K'iche' / Guatemala
        langToCodePage[1159] = 1252;  // Kinyarwanda / Rwanda
        langToCodePage[1160] = 1252;  // Wolof / Senegal
        langToCodePage[1164] = 1256;  // Dari / Afghanistan
        langToCodePage[2049] = 1256;  // Arabic / Iraq
        langToCodePage[2052] = 936;   // Chinese (Simplified) / China
        langToCodePage[2055] = 1252;  // German / Switzerland
        langToCodePage[2057] = 1252;  // English / United Kingdom
        langToCodePage[2058] = 1252;  // Spanish / Mexico
        langToCodePage[2060] = 1252;  // French / Belgium
        langToCodePage[2064] = 1252;  // Italian / Switzerland
        langToCodePage[2067] = 1252;  // Dutch / Belgium
        langToCodePage[2068] = 1252;  // Norwegian Nynorsk / Norway
        langToCodePage[2070] = 1252;  // Portuguese / Portugal
        langToCodePage[2072] = 1250;  // Romanian / Moldova
        langToCodePage[2073] = 1251;  // Russian / Moldova
        langToCodePage[2077] = 1252;  // Swedish / Finland
        langToCodePage[2080] = 1256;  // Urdu / India
        langToCodePage[2092] = 1251;  // Azerbaijani (Cyrillic) / Azerbaijan
        langToCodePage[2094] = 1252;  // Lower Sorbian / Germany
        langToCodePage[2107] = 1252;  // Sami (Northern) / Sweden
        langToCodePage[2108] = 1252;  // Irish / Ireland
        langToCodePage[2110] = 1252;  // Malay / Brunei
        langToCodePage[2115] = 1251;  // Uzbek (Cyrillic) / Uzbekistan
        langToCodePage[2118] = 1256;  // Punjabi / Pakistan
        langToCodePage[2137] = 1256;  // Sindhi / Pakistan
        langToCodePage[2141] = 1252;  // Inuktitut (Latin) / Canada
        langToCodePage[2143] = 1252;  // Central Atlas Tamazight (Latin) / Algeria
        langToCodePage[2155] = 1252;  // Quechua / Ecuador
        langToCodePage[3073] = 1256;  // Arabic / Egypt
        langToCodePage[3076] = 950;   // Chinese (Traditional) / Hong Kong SAR
        langToCodePage[3079] = 1252;  // German / Austria
        langToCodePage[3081] = 1252;  // English / Australia
        langToCodePage[3082] = 1252;  // Spanish / Spain
        langToCodePage[3084] = 1252;  // French / Canada
        langToCodePage[3131] = 1252;  // Sami (Northern) / Finland
        langToCodePage[3179] = 1252;  // Quechua / Peru
        langToCodePage[4097] = 1256;  // Arabic / Libya
        langToCodePage[4100] = 936;   // Chinese (Simplified) / Singapore
        langToCodePage[4103] = 1252;  // German / Luxembourg
        langToCodePage[4105] = 1252;  // English / Canada
        langToCodePage[4106] = 1252;  // Spanish / Guatemala
        langToCodePage[4108] = 1252;  // French / Switzerland
        langToCodePage[4122] = 1250;  // Croatian / Bosnia & Herzegovina
        langToCodePage[4155] = 1252;  // Sami (Lule) / Norway
        langToCodePage[5121] = 1256;  // Arabic / Algeria
        langToCodePage[5124] = 950;   // Chinese (Traditional) / Macao SAR
        langToCodePage[5127] = 1252;  // German / Liechtenstein
        langToCodePage[5129] = 1252;  // English / New Zealand
        langToCodePage[5130] = 1252;  // Spanish / Costa Rica
        langToCodePage[5132] = 1252;  // French / Luxembourg
        langToCodePage[5146] = 1250;  // Bosnian (Latin) / Bosnia & Herzegovina
        langToCodePage[5179] = 1252;  // Sami (Lule) / Sweden
        langToCodePage[6145] = 1256;  // Arabic / Morocco
        langToCodePage[6153] = 1252;  // English / Ireland
        langToCodePage[6154] = 1252;  // Spanish / Panama
        langToCodePage[6156] = 1252;  // French / Monaco
        langToCodePage[6170] = 1250;  // Serbian (Latin) / Bosnia & Herzegovina
        langToCodePage[6203] = 1252;  // Sami (Southern) / Norway
        langToCodePage[7169] = 1256;  // Arabic / Tunisia
        langToCodePage[7177] = 1252;  // English / South Africa
        langToCodePage[7178] = 1252;  // Spanish / Dominican Republic
        langToCodePage[7180] = 1252;  // French / Caribbean
        langToCodePage[7194] = 1251;  // Serbian (Cyrillic) / Bosnia and Herzegovina
        langToCodePage[7227] = 1252;  // Sami (Southern) / Sweden
        langToCodePage[8193] = 1256;  // Arabic / Oman
        langToCodePage[8201] = 1252;  // English / Jamaica
        langToCodePage[8202] = 1252;  // Spanish / Venezuela
        langToCodePage[8204] = 1252;  // French / Réunion
        langToCodePage[8218] = 1251;  // Bosnian (Cyrillic) / Bosnia and Herzegovina
        langToCodePage[8251] = 1252;  // Sami (Skolt) / Finland
        langToCodePage[9217] = 1256;  // Arabic / Yemen
        langToCodePage[9225] = 1252;  // English / Caribbean
        langToCodePage[9226] = 1252;  // Spanish / Colombia
        langToCodePage[9228] = 1252;  // French / Congo (DRC)
        langToCodePage[9275] = 1252;  // Sami (Inari) / Finland
        langToCodePage[10241] = 1256; // Arabic / Syria
        langToCodePage[10249] = 1252; // English / Belize
        langToCodePage[10250] = 1252; // Spanish / Peru
        langToCodePage[10252] = 1252; // French / Senegal
        langToCodePage[11265] = 1256; // Arabic / Jordan
        langToCodePage[11273] = 1252; // English / Trinidad & Tobago
        langToCodePage[11274] = 1252; // Spanish / Argentina
        langToCodePage[11276] = 1252; // French / Cameroon
        langToCodePage[12289] = 1256; // Arabic / Lebanon
        langToCodePage[12297] = 1252; // English / Zimbabwe
        langToCodePage[12298] = 1252; // Spanish / Ecuador
        langToCodePage[12300] = 1252; // French / Côte d’Ivoire
        langToCodePage[13313] = 1256; // Arabic / Kuwait
        langToCodePage[13321] = 1252; // English / Philippines
        langToCodePage[13322] = 1252; // Spanish / Chile
        langToCodePage[13324] = 1252; // French / Mali
        langToCodePage[14337] = 1256; // Arabic / United Arab Emirates
        langToCodePage[14345] = 1252; // English / Indonesia
        langToCodePage[14346] = 1252; // Spanish / Uruguay
        langToCodePage[14348] = 1252; // French / Morocco
        langToCodePage[15361] = 1256; // Arabic / Bahrain
        langToCodePage[15369] = 1252; // English / Hong Kong SAR
        langToCodePage[15370] = 1252; // Spanish / Paraguay
        langToCodePage[15372] = 1252; // French / Haiti
        langToCodePage[16385] = 1256; // Arabic / Qatar
        langToCodePage[16393] = 1252; // English / India
        langToCodePage[16394] = 1252; // Spanish / Bolivia
        langToCodePage[17417] = 1252; // English / Malaysia
        langToCodePage[17418] = 1252; // Spanish / El Salvador
        langToCodePage[18441] = 1252; // English / Singapore
        langToCodePage[18442] = 1252; // Spanish / Honduras
        langToCodePage[19466] = 1252; // Spanish / Nicaragua
        langToCodePage[20490] = 1252; // Spanish / Puerto Rico
        langToCodePage[21514] = 1252; // Spanish / United States
#endif

        return langToCodePage;
    });

    #region Charset to code page

    public const int CharSetToCodePageLength = 256;

    public static readonly ushort[] CharSetToCodePage = RunFunc(static () =>
    {
        ushort[] charSetToCodePage = InitializedArray(CharSetToCodePageLength, NoCodePage);

        charSetToCodePage[0] = 1252;   // "ANSI" (1252) (Yes, this is specified as _explicitly_ 1252, so this
                                       // isn't a straggling 1252-default)

        charSetToCodePage[1] = 0;      // Default

        charSetToCodePage[2] = 42;     // Symbol
        charSetToCodePage[77] = 10000; // Mac Roman
        charSetToCodePage[78] = 10001; // Mac Shift Jis
        charSetToCodePage[79] = 10003; // Mac Hangul
        charSetToCodePage[80] = 10008; // Mac GB2312
        charSetToCodePage[81] = 10002; // Mac Big5
        //charSetToCodePage[82] = ?    // Mac Johab (old)
        charSetToCodePage[83] = 10005; // Mac Hebrew
        charSetToCodePage[84] = 10004; // Mac Arabic
        charSetToCodePage[85] = 10006; // Mac Greek
        charSetToCodePage[86] = 10081; // Mac Turkish
        charSetToCodePage[87] = 10021; // Mac Thai
        charSetToCodePage[88] = 10029; // Mac East Europe
        charSetToCodePage[89] = 10007; // Mac Russian
        charSetToCodePage[128] = 932;  // Shift JIS (Windows-31J) (932)
        charSetToCodePage[129] = 949;  // Hangul
        charSetToCodePage[130] = 1361; // Johab
        charSetToCodePage[134] = 936;  // GB2312
        charSetToCodePage[136] = 950;  // Big5
        charSetToCodePage[161] = 1253; // Greek
        charSetToCodePage[162] = 1254; // Turkish
        charSetToCodePage[163] = 1258; // Vietnamese
        charSetToCodePage[177] = 1255; // Hebrew
        charSetToCodePage[178] = 1256; // Arabic
        //charSetToCodePage[179] = ?   // Arabic Traditional (old)
        //charSetToCodePage[180] = ?   // Arabic user (old)
        //charSetToCodePage[181] = ?   // Hebrew user (old)
        charSetToCodePage[186] = 1257; // Baltic
        charSetToCodePage[204] = 1251; // Russian
        charSetToCodePage[222] = 874;  // Thai
        charSetToCodePage[238] = 1250; // Eastern European
        charSetToCodePage[254] = 437;  // PC 437
        charSetToCodePage[255] = 850;  // OEM

        return charSetToCodePage;
    });

    #endregion

    #region SIMD

    public static readonly Vector<byte> ZeroVector = new((byte)'\0');
    public static readonly Vector<byte> LF_Vector = new((byte)'\n');
    public static readonly Vector<byte> CR_Vector = new((byte)'\r');
    public static readonly Vector<byte> BackslashVector = new((byte)'\\');
    public static readonly Vector<byte> OpenBraceVector = new((byte)'{');
    public static readonly Vector<byte> ClosingBraceVector = new((byte)'}');
    public static readonly Vector<byte> n_Vector = new((byte)'n');
    public static readonly Vector<byte> SemicolonVector = new((byte)';');

    public const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                   0x06ul << 8 |
                                                   0x05ul << 16 |
                                                   0x04ul << 24 |
                                                   0x03ul << 32 |
                                                   0x02ul << 40 |
                                                   0x01ul << 48) + 1;

    // Vector length is unknowable at compile time, so make sure this program still runs on AVX2048 in 200 years
    public static readonly bool VectorLengthFitsInAByte = Vector<byte>.Count <= 256;
    public static readonly Vector<byte> IndexVec = RunFunc(static () =>
    {
        if (VectorLengthFitsInAByte)
        {
            byte[] bytes = new byte[Vector<byte>.Count];
            for (byte i = 0; i < Vector<byte>.Count; i++)
            {
                bytes[i] = i;
            }
            return new Vector<byte>(bytes);
        }
        else
        {
            return Vector<byte>.Zero;
        }
    });


    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    public static int SIMD_SkipDest(
    ref byte bufferRef,
    int startIndex,
    int spanLength)
    {
        if (!Vector.IsHardwareAccelerated || !VectorLengthFitsInAByte)
        {
            return -1;
        }

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte searchSpace = ref Unsafe.AddByteOffset(ref bufferRef, (nint)startIndex);
            Vector<byte> equalsBraces;
            Vector<byte> equalsBackslash;
            Vector<byte> equals;
            Vector<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (nint)(spanLength - Vector<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);
                equalsBraces = Vector.Equals(OpenBraceVector, current) | Vector.Equals(ClosingBraceVector, current);
                equalsBackslash = Vector.Equals(BackslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector<byte>.Zero)
                {
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector<byte>.Zero;
                    if (!bracesFound || (backslashIndex = LocateFirstFoundByte(equalsBackslash)) < (bracesIndex = LocateFirstFoundByte(equalsBraces)))
                    {
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector<byte>.Count + (BinLength - 1)) <= spanLength)
                        {
                            Vector<byte> lastBlock = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref currentSearchSpace, BinLength - 1));
                            Vector<byte> lastEquals = Vector.Equals(n_Vector, lastBlock);

                            Vector<byte> containsBin = Vector.BitwiseAnd(equalsBackslash, lastEquals);

                            if (containsBin == Vector<byte>.Zero)
                            {
                                if (!bracesFound)
                                {
                                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
                                    continue;
                                }
                                else
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                                }
                            }
                            else
                            {
                                Vector<byte> mask = Vector.BitwiseAnd(equalsBackslash, lastEquals);
                                while (mask != Vector<byte>.Zero)
                                {
                                    int vectorIndex = LocateFirstFoundByte(mask);
                                    int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, vectorIndex);
                                    if (index >= spanLength - sizeof(uint) ||
                                        Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)index)) == BinUInt)
                                    {
                                        if (backslashIndex == -1) backslashIndex = LocateFirstFoundByte(equalsBackslash);
                                        return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                    }

                                    mask = ClearMaskElementAtIndex(mask, vectorIndex);
                                }
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = LocateFirstFoundByte(equalsBackslash);
                            int currentVectorIndex = backslashIndex;
                            Vector<byte> mask = ClearMaskElementAtIndex(equalsBackslash, currentVectorIndex);
                            while (currentVectorIndex < Vector<byte>.Count)
                            {
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)spanIndex)) == BinUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ClearMaskElementAtIndex(mask, currentVectorIndex);
                                currentVectorIndex = LocateFirstFoundByte_VectorCountOnFail(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector<byte>.Count != 0)
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref oneVectorAwayFromEnd);
                equalsBraces = Vector.Equals(OpenBraceVector, current) | Vector.Equals(ClosingBraceVector, current);
                equalsBackslash = Vector.Equals(BackslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }

        return -1;
    }

    #region SIMD Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<byte> ClearMaskElementAtIndex(Vector<byte> mask, int index)
    {
        return Vector.BitwiseAnd(mask, Vector.LessThan(new Vector<byte>((byte)index), IndexVec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector<byte> equals)
    {
        int index = LocateFirstFoundByte(equals);
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte(Vector<byte> match)
    {
        Vector<ulong> vector64 = Vector.AsVectorUInt64(match);
        ulong candidate = 0;
        int i = 0;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            candidate = vector64[i];
            if (candidate != 0)
            {
                break;
            }
        }

        // Single LEA instruction with jitted const (using function result)
        return i * 8 + LocateFirstFoundByte(candidate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte_VectorCountOnFail(Vector<byte> match)
    {
        Vector<ulong> vector64 = Vector.AsVectorUInt64(match);
        int i = 0;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            ulong candidate = vector64[i];
            if (candidate != 0)
            {
                // Single LEA instruction with jitted const (using function result)
                return i * 8 + LocateFirstFoundByte(candidate);
            }
        }

        return Vector<byte>.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte(ulong match)
    {
        // Flag least significant power of two bit
        ulong powerOfTwoFlag = match ^ (match - 1);
        // Shift all powers of two into the high byte and extract
        return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
    }

    #endregion

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RTF_Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
        /*
        On .NET, Array.IndexOf() uses crazy fast SIMD. On Framework, it normally doesn't.
        However, on Framework 64-bit only, we can make it use SIMD by using span.IndexOf(), if we reference the
        appropriate package (directly or indirectly), System.Memory or whatever it is.
        If we're 32-bit, though, SIMD is not supported, so we just stick to the regular Array.IndexOf(), which
        while substantially slower than the SIMD version, is still reasonably fast.

        But instead of checking for 64-bit vs. 32-bit, we can just check directly if SIMD is supported.
        */
        if (Vector.IsHardwareAccelerated)
        {
            int index = array.AsSpan(startIndex, count).IndexOf(value);
            if (index > -1) index += startIndex;
            return index;
        }
        else
        {
            return Array.IndexOf(array, value, startIndex, count);
        }
    }

    // Total hack so we don't have to return and check a value eight trillion times (perf)
    public sealed class UnmatchedBraceException : Exception;
}
