using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static AL_Common.Common;

namespace AL_Common;

/* @PERF_TODO(RTF parser / UnGetChar):
We might be able to get rid of our un-get buffer if we're clever...
The only place "UnGet" is called in the original example in the spec file is when it un-gets the non-space
char after a keyword if there isn't a space.

We use it in:
-ParseKeyword() (the above-mentioned optional-space-after-keyword logic)
-Hex parser (a bunch of times, mostly to do with "last char in stream corner case")
-Field instruction parser ("rewind and skip group")

If we can get rid of the un-get buffer, we can avoid the hot-loop branch that checks if there's anything in
said buffer every time we get a char, and associated un-getting logic (which is minimal, but still).

Counts of GetNextChar()/GetNextCharFast() sources:
Stream:       172,766,662
UnGet buffer: 996,390

In ParseKeyword():
Char | Number of times un-get-ed
'\'    862809
'{'    21558
'?'    402
'13'   37765
';'    9942
'}'    9732
'10'   533
'.'    2

* numbers are ascii codes, so 13 and 10 are CR and LF

In theory, the un-get-ed char here can be:

If the keyword had no param, then it can be anything not alphanumeric and not a space.
Because if it was alpha, it would have been part of the keyword, and if it was numeric, it would have been
a param.

If the keyword DID have a param, then it can be anything not numeric and not a space. It CAN be alpha,
because we've just come off the numeric param, and so alpha would be distinguishable from the last thing
we were parsing so it would pass (not tested but I'm pretty sure that's the case).

Obviously rtf is not supposed to have non-ASCII chars in it, so if any are present then it's undefined,
but I guess they would all be un-get-ed too.

Note the '?' is the usual Unicode fallback char at the end of the \u2341? keyword for example. But that char
can be anything, even a keyword and all that other crap as we know.
*/

/*
@vNext(RTF Cyrillic (some Zontik FMs) issue):
This may also occur in other FMs and/or with other codepages (have to check), but basically Windows RichEdit is
"interpreting the spec differently" from LibreOffice (and possibly other apps, I don't have MS Office / Word)
and is displaying what should be Cyrillic text in garbled Windows-1252. The reason is the \langN (see note)
control word, which the spec is very vague about. It merely says: "Applies a language to a text run". It doesn't
say what that means in any technical sense or what readers are supposed to do with it, but Windows RichEdit
appears to simply ignore it. Other apps may interpret it as "use an appropriate codepage for this language",
which is how LibreOffice - at least - is displaying the Cyrillic in these Zontik readmes correctly, as \lang1049
is in effect for that text (1049 is language "Russian (Russia) 0x419 1049" per the language table in the spec).
We can pre-parse the rtf before load (like we do for the color table, but always) and detect a \langN / current-
font-codepage mismatch, and fix up the font table and then set the appropriate-codepage font for this scope
alongside the \langN control word.

Remember to have the parser return the extra length of the required inserts so we can set it once and not have a
zillion list copy allocations.

We could also do this for the plaintext converter, and we wouldn't take any extra allocations then, we'd just
override the current font codepage with the current \langN codepage, simple enough.

*Note: There are also \langnpN, \langfeN, and \langfenpN which are similar and should be accounted for I guess.

UPDATE: There's also \alangN, about which the spec says literally nothing other than that it exists and is a
"language id". It doesn't say what the difference is between \alangN and \langN, nor for that matter what the
difference is between \langN and \langfeN. The "np" versions do have an explanation:

"langnpN: Applies a language to a text run. N is the language ID. The \plain control word resets the
language property to the language defined by \deflangN in the document properties. It is
identical to \langN, but needed when \noproof is written together with \lang1024 to preserve
the language of the text that is not being checked for spelling or grammar. Usually follows
\langN."

UPDATE 2:

Actually it does explain them all in a roundabout way, but the explanations are in the \deflang section:

"\deflangN: Defines default language to be used when the \plain control word is encountered. See the
standard language table for a list of possible values for N."

"\deflangfeN: Default language ID for East Asian text in Word."

"\adeflangN: Default language ID for South Asian/Middle Eastern text in Word. The default languages are
determined by the current primary editing language and the enabled editing languages (can
be changed via Microsoft Office Language Settings applet)."

So, yeah, like... wtf

Also:

"\noproof: Do not check spelling or grammar for text in the group. Serves the function of \lang1024
(undefined language). Usually \lang1024 is emitted with it for backward compatibility with old readers."

So... I guess maybe our logic should be that non-1024 "np" versions supersede 1024 regular versions.
But should they supersede regular versions always?

We should also remember to ignore language 1024 when doing the font codepage match check.
*/

public abstract partial class RTFParserBase
{
    #region Constants

    private const int _maxScopes = 100;
    private const int _keywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    private const int _paramMaxLen = 10;

    #endregion

    protected RTFParserBase()
    {
#if !NETFRAMEWORK
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
    }

    #region Classes

    protected sealed class ScopeStack
    {
        private readonly Scope[] _scopesArray;
        public int Count;

        public ScopeStack()
        {
            _scopesArray = new Scope[_maxScopes];
            for (int i = 0; i < _maxScopes; i++)
            {
                _scopesArray[i] = new Scope();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Scope currentScope) => currentScope.DeepCopyTo(_scopesArray[Count++]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop(Scope currentScope) => _scopesArray[--Count].DeepCopyTo(currentScope);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFast() => Count = 0;
    }

    protected sealed class Scope
    {
        public RtfDestinationState RtfDestinationState;
        public RtfInternalState RtfInternalState;
        public bool InFontTable;
        public SymbolFont SymbolFont;

        public readonly int[] Properties = new int[_propertiesLen];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeepCopyTo(Scope dest)
        {
            dest.RtfDestinationState = RtfDestinationState;
            dest.RtfInternalState = RtfInternalState;
            dest.InFontTable = InFontTable;
            dest.SymbolFont = SymbolFont;

            Array.Copy(Properties, 0, dest.Properties, 0, _propertiesLen);
        }

        public void Reset()
        {
            RtfDestinationState = 0;
            RtfInternalState = 0;
            InFontTable = false;
            SymbolFont = SymbolFont.None;

            Properties[(int)Property.Hidden] = 0;
            Properties[(int)Property.UnicodeCharSkipCount] = 1;
            Properties[(int)Property.FontNum] = -1;
            Properties[(int)Property.Lang] = -1;
        }
    }

    protected sealed class Symbol
    {
        public readonly string Keyword;
        public readonly int DefaultParam;
        public readonly bool UseDefaultParam;
        public readonly KeywordType KeywordType;
        /// <summary>
        /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
        /// </summary>
        public readonly int Index;

        public Symbol(string keyword, int defaultParam, bool useDefaultParam, KeywordType keywordType, int index)
        {
            Keyword = keyword;
            DefaultParam = defaultParam;
            UseDefaultParam = useDefaultParam;
            KeywordType = keywordType;
            Index = index;
        }
    }

    protected sealed class SymbolDict
    {
        /* ANSI-C code produced by gperf version 3.1 */
        /* Command-line: gperf --output-file='C:\\gperf_out.txt' -t 'C:\\gperf_in.txt'  */
        /* Computed positions: -k'1-3,$' */

        // Then ported to C# semi-manually. Woe.

        // Two ways we gain perf with this generated perfect hash thing over a standard Dictionary:
        // First, it's just faster to begin with, and second, it lets us finally ditch the StringBuilders and
        // ToString()s and just pass in simple char arrays. We are now unmeasurable. Hallelujah!

        //const int TOTAL_KEYWORDS = 76;
        private const int MIN_WORD_LENGTH = 1;
        private const int MAX_WORD_LENGTH = 10;
        //const int MIN_HASH_VALUE = 1;
        private const int MAX_HASH_VALUE = 196;
        /* maximum key range = 196, duplicates = 0 */

        private readonly byte[] asso_values =
        {
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 40,
            197, 197, 35, 197, 197, 197, 197, 197, 197, 5,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 30, 197, 197, 197, 197, 10, 10, 0,
            70, 0, 10, 65, 75, 0, 197, 0, 0, 45,
            25, 0, 45, 40, 35, 5, 20, 75, 0, 0,
            5, 0, 197, 20, 197, 15, 5, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197, 197, 197, 197, 197,
            197, 197, 197, 197, 197, 197
        };

        // For "emspace", "enspace", "qmspace", "~"
        // Just convert these to regular spaces because we're just trying to scan for strings in readmes
        // without weird crap tripping us up

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

        // NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
        // Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
        // See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
        // Symbol list which must be used as the source to generate this one).
        private readonly Symbol?[] _symbolTable =
        {
            null,
// Entry 15
            new Symbol("v", 1, false, KeywordType.Property, (int)Property.Hidden),
            null, null,
// Entry 72
            new Symbol("cell", 0, false, KeywordType.Character, ' '),
            null, null,
// Entry 70
            new Symbol("xe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 41
            new Symbol("colortbl", 0, false, KeywordType.Special, (int)SpecialType.ColorTable),
            null, null,
// Entry 24
            new Symbol("~", 0, false, KeywordType.Character, ' '),
// Entry 34
            new Symbol("cs", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
// Entry 58
            new Symbol("keywords", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 47
            new Symbol("footerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 49
            new Symbol("footnote", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null,
// Entry 8
            new Symbol("f", 0, false, KeywordType.Property, (int)Property.FontNum),
// Entry 67
            new Symbol("tc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 18
            new Symbol("softline", 0, false, KeywordType.Character, '\n'),
            null, null, null,
// Entry 46
            new Symbol("footerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 69
            new Symbol("txe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 17
            new Symbol("line", 0, false, KeywordType.Character, '\n'),
            null,
// Entry 74
            new Symbol("}", 0, false, KeywordType.Character, '}'),
// Entry 36
            new Symbol("ts", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
// Entry 37
            new Symbol("listtext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 22
            new Symbol("enspace", 0, false, KeywordType.Character, ' '),
// Entry 71
            new Symbol("row", 0, false, KeywordType.Character, '\n'),
// Entry 57
            new Symbol("info", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 73
            new Symbol("{", 0, false, KeywordType.Character, '{'),
// Entry 7
            new Symbol("fonttbl", 0, false, KeywordType.Special, (int)SpecialType.FontTable),
// Entry 64
            new Symbol("rxe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 0
            new Symbol("ansi", 1252, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 68
            new Symbol("title", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 1
            new Symbol("pc", 437, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null, null, null,
// Entry 45
            new Symbol("footer", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 48
            new Symbol("footerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 19
            new Symbol("tab", 0, false, KeywordType.Character, '\t'),
            null,
// Entry 65
            new Symbol("stylesheet", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 21
            new Symbol("emspace", 0, false, KeywordType.Character, ' '),
// Entry 2
            new Symbol("mac", 10000, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null, null,
// Entry 75
            new Symbol("\\", 0, false, KeywordType.Character, '\\'),
// Entry 52
            new Symbol("ftnsepc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 31
            new Symbol("bin", 0, false, KeywordType.Special, (int)SpecialType.Bin),
            null, null, null, null,
// Entry 3
            new Symbol("pca", 850, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 60
            new Symbol("pict", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 32
            new Symbol("*", 0, false, KeywordType.Special, (int)SpecialType.SkipDest),
// Entry 42
            new Symbol("comment", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null,
// Entry 12
            new Symbol("uc", 1, false, KeywordType.Property, (int)Property.UnicodeCharSkipCount),
            null, null, null,
// Entry 13
            new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar),
// Entry 35
            new Symbol("ds", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null, null,
// Entry 50
            new Symbol("ftncn", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 63
            new Symbol("revtim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 43
            new Symbol("creatim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 59
            new Symbol("operator", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 29
            new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
            null, null,
// Entry 55
            new Symbol("headerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 62
            new Symbol("private1", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 5
            new Symbol("deff", 0, false, KeywordType.Special, (int)SpecialType.DefaultFont),
            null, null,
// Entry 23
            new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
            null, null, null, null,
// Entry 54
            new Symbol("headerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 11
            new Symbol("lang", 0, false, KeywordType.Property, (int)Property.Lang),
            null,
// Entry 51
            new Symbol("ftnsep", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 33
            new Symbol("fldinst", 0, false, KeywordType.Destination, (int)DestinationType.FieldInstruction),
            null, null, null,
// Entry 20
            new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
// Entry 4
            new Symbol("ansicpg", 1252, false, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 9
            new Symbol("fcharset", -1, false, KeywordType.Special, (int)SpecialType.Charset),
            null, null,
// Entry 38
            new Symbol("pntext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 66
            new Symbol("subject", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 27
            new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
// Entry 44
            new Symbol("doccomm", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 30
            new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),
            null,
// Entry 53
            new Symbol("header", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 56
            new Symbol("headerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 16
            new Symbol("par", 0, false, KeywordType.Character, '\n'),
            null, null, null,
// Entry 61
            new Symbol("printim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null,
// Entry 39
            new Symbol("author", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null,
// Entry 14
            new Symbol("u", 0, false, KeywordType.Special, (int)SpecialType.UnicodeChar),
// Entry 6
            new Symbol("deflang", 0, false, KeywordType.Special, (int)SpecialType.DefaultLang),
            null, null, null,
// Entry 28
            new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null,
// Entry 26
            new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
            null,
// Entry 10
            new Symbol("cpg", -1, false, KeywordType.Special, (int)SpecialType.CodePage),
            null, null,
// Entry 40
            new Symbol("buptim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null,
// Entry 25
            new Symbol("emdash", 0, false, KeywordType.Character, '\x2014')
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(ListFast<char> str, [NotNullWhen(true)] out Symbol? result)
        {
            int len = str.Count;
            if (len is <= MAX_WORD_LENGTH and >= MIN_WORD_LENGTH)
            {
                int key = len;

                // Original C code does a stupid thing where it puts default at the top and falls through and junk,
                // but we can't do that in C#, so have something clearer/clunkier
                switch (len)
                {
                    case 1:
                        key += asso_values[str.ItemsArray[0]];
                        break;
                    case 2:
                        key += asso_values[str.ItemsArray[1]];
                        key += asso_values[str.ItemsArray[0]];
                        break;
                    default:
                        key += asso_values[str.ItemsArray[2]];
                        key += asso_values[str.ItemsArray[1]];
                        key += asso_values[str.ItemsArray[0]];
                        break;
                }
                key += asso_values[str.ItemsArray[len - 1]];

                if (key <= MAX_HASH_VALUE)
                {
                    Symbol? symbol = _symbolTable[key];
                    if (symbol == null)
                    {
                        result = null;
                        return false;
                    }

                    var seq2 = symbol.Keyword;
                    if (len != seq2.Length)
                    {
                        result = null;
                        return false;
                    }

                    for (int ci = 0; ci < len; ci++)
                    {
                        if (str.ItemsArray[ci] != seq2[ci])
                        {
                            result = null;
                            return false;
                        }
                    }

                    result = symbol;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }

    protected sealed class UnGetStack
    {
        private char[] _array;
        private int _capacity;

        private const int _resetCapacity = 100;

        public UnGetStack()
        {
            _array = new char[_resetCapacity];
            _capacity = _resetCapacity;
            Count = 0;
        }

        /// <summary>
        /// Do not set from outside. Properties are slow.
        /// </summary>
        public int Count;

        public void Clear()
        {
            if (_capacity > _resetCapacity)
            {
                _array = new char[_resetCapacity];
                _capacity = _resetCapacity;
            }
            Count = 0;
        }

        public char Pop() => _array[--Count];

        public void Push(char item)
        {
            if (Count == _capacity)
            {
                int capacity = _array.Length == 0 ? 4 : 2 * _array.Length;
                char[] destinationArray = new char[capacity];
                Array.Copy(_array, 0, destinationArray, 0, Count);
                _array = destinationArray;
                _capacity = capacity;
            }
            _array[Count++] = item;
        }
    }

    #endregion

    #region Enums

    protected enum SpecialType
    {
        HeaderCodePage,
        DefaultFont,
        FontTable,
        Charset,
        CodePage,
        UnicodeChar,
        HexEncodedChar,
        Bin,
        SkipDest,
        ColorTable,
        DefaultLang
    }

    protected enum KeywordType
    {
        Character,
        Property,
        Destination,
        Special
    }

    protected enum DestinationType
    {
        FieldInstruction,
        /// <summary>
        /// This for \csN, \dsN, and \tsN.
        /// <para/>
        /// These are weird hybrids that can either be written as destinations (eg. "\*\cs15") or not (eg. "\cs15").
        /// <para/>
        /// The spec explains:<br/>
        /// "\csN:<br/>
        /// Designates character style with a style handle N. Like \sN, \csN is not a destination control word.<br/>
        /// However, it is important to treat it like one inside the style sheet; that is, \csN must be prefixed<br/>
        /// with \* and must appear as the first item inside a group. Doing so ensures that readers that do not<br/>
        /// understand character styles will skip the character style information correctly. When used in body<br/>
        /// text to indicate that a character style was applied, do not include the \* prefix."
        /// <para/>
        /// We don't really have a way to handle this table-side, so just call it a destination and give it a<br/>
        /// special-cased type where it just skips the control word always, even if it was a destination and<br/>
        /// would normally have caused its entire group to be skipped.
        /// <para/>
        /// You might think we could just elide these from the table entirely and accomplish the same thing,<br/>
        /// but there's one readme (Thief Trinity) where \*\csN is written in the middle of a group, rather<br/>
        /// than the start of a group as destinations are supposed to be written, causing us to skip its group<br/>
        /// and miss some text.
        /// <para/>
        /// The correct way to handle this would be to track whether we're at the start of a group when we hit<br/>
        /// one of these, but that's a bunch of crufty garbage so whatever, let's just stick with this, as it<br/>
        /// seems to work fine...
        /// </summary>
        CanBeDestOrNotDest,
        Skip
    }

    private const int _propertiesLen = 4;
    protected enum Property
    {
        Hidden,
        UnicodeCharSkipCount,
        FontNum,
        Lang
    }

    protected enum SymbolFont
    {
        None,
        Symbol,
        Wingdings,
        Webdings,
        Unset
    }

    protected enum RtfDestinationState
    {
        Normal,
        Skip
    }

    protected enum RtfInternalState
    {
        Normal,
        Binary,
        HexEncodedChar
    }

    protected enum Error
    {
        /// <summary>
        /// No error.
        /// </summary>
        OK,
        /// <summary>
        /// Unmatched '}'.
        /// </summary>
        StackUnderflow,
        /// <summary>
        /// Too many subgroups (we cap it at 100).
        /// </summary>
        StackOverflow,
        /// <summary>
        /// RTF ended during an open group.
        /// </summary>
        UnmatchedBrace,
        /// <summary>
        /// Invalid hexadecimal character found while parsing \'hh character(s).
        /// </summary>
        InvalidHex,
        /// <summary>
        /// A \uN keyword's parameter was out of range.
        /// </summary>
        InvalidUnicode,
        /// <summary>
        /// A symbol table entry was malformed. Possibly one of its enum values was out of range.
        /// </summary>
        InvalidSymbolTableEntry,
        /// <summary>
        /// Unexpected end of file reached while reading RTF.
        /// </summary>
        EndOfFile,
        /// <summary>
        /// A keyword was found that exceeds the max keyword length.
        /// </summary>
        KeywordTooLong,
        /// <summary>
        /// A parameter was found that exceeds the max parameter length.
        /// </summary>
        ParameterTooLong
    }

    #endregion

    #region Resettables

    protected readonly ListFast<char> _keyword = new(_keywordMaxLen);

    protected int _binaryCharsLeftToSkip;
    protected int _unicodeCharsLeftToSkip;

    protected bool _skipDestinationIfUnknown;

    // Static - otherwise the color table parser instantiates this huge thing every RTF readme in dark mode!
    // Also it's readonly so it's thread-safe anyway.
    protected static readonly SymbolDict Symbols = new();

    // Highest measured was 10
    protected readonly ScopeStack _scopeStack = new();

    protected readonly Scope _currentScope = new();

    // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
    protected int _groupCount;

    #endregion

    protected void ResetBase()
    {
        #region Fixed-size fields

        // Specific capacity and won't grow; no need to deallocate
        _keyword.ClearFast();

        // Fixed-size value types
        _groupCount = 0;
        _binaryCharsLeftToSkip = 0;
        _unicodeCharsLeftToSkip = 0;
        _skipDestinationIfUnknown = false;

        // Types that contain only fixed-size value types
        _currentScope.Reset();

        #endregion

        _scopeStack.ClearFast();
    }

    #region Stream

    // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
    // length as a param and store it.
    /// <summary>
    /// Do not modify!
    /// </summary>
    protected long Length;

    /// <summary>
    /// Do not modify!
    /// </summary>
    protected long CurrentPos;

    protected const int _bufferLen = 81920;
    protected readonly byte[] _buffer = new byte[_bufferLen];
    // Start it ready to roll over to 0 so we don't need extra logic for the first get
    protected int _bufferPos = _bufferLen - 1;

    /*
    We use this as a "seek-back" buffer for when we want to move back in the stream. We put chars back
    "into the stream", but they actually go in here and then when we go to read, we read from this first
    and so on until it's empty, then go back to reading from the main stream again. In this way, we
    support a rudimentary form of peek-and-rewind without ever actually seeking backwards in the stream.
    This is required to support zip entry streams which are unseekable. If we required a seekable stream,
    we would have to copy the entire, potentially very large, zip entry stream to memory first and then
    read it, which is possibly unnecessarily memory-hungry.

    2020-08-15:
    We now have a buffered stream so in theory we could check if we're > 0 in the buffer and just actually
    rewind if we are, but our seek-back buffer is fast enough already so we're just keeping that for now.
    */
    private readonly UnGetStack _unGetBuffer = new();

    /// <summary>
    /// Puts a char back into the stream and decrements the read position. Actually doesn't really do that
    /// but uses an internal seek-back buffer to allow it work with forward-only streams. But don't worry
    /// about that. Just use it as normal.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UnGetChar(char c)
    {
        if (CurrentPos < 0) return;

        _unGetBuffer.Push(c);
        if (CurrentPos > 0) CurrentPos--;
    }

    /// <summary>
    /// Returns false if the end of the stream has been reached.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool GetNextChar(out char ch)
    {
        if (CurrentPos == Length)
        {
            ch = '\0';
            return false;
        }

        // For some reason leaving this as a full if makes us fast but changing it to a ternary makes us slow?!
#pragma warning disable IDE0045 // Convert to conditional expression
        if (_unGetBuffer.Count > 0)
        {
            ch = _unGetBuffer.Pop();
        }
        else
        {
            ch = (char)StreamReadByte();
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        CurrentPos++;

        return true;
    }

    /// <summary>
    /// For use in loops that already check the stream position against the end as a loop condition
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected char GetNextCharFast()
    {
        char ch;
        // Ditto above
#pragma warning disable IDE0045 // Convert to conditional expression
        if (_unGetBuffer.Count > 0)
        {
            ch = _unGetBuffer.Pop();
        }
        else
        {
            ch = (char)StreamReadByte();
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        CurrentPos++;

        return ch;
    }

    protected abstract byte StreamReadByte();

    protected void ResetStreamBase(long streamLength)
    {
        Length = streamLength;

        CurrentPos = 0;

        // Don't clear the buffer; we don't need to and it wastes time
        _bufferPos = _bufferLen - 1;

        _unGetBuffer.Clear();
    }

    #endregion

    #region Scope push/pop

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Error PushScope()
    {
        // Don't wait for out-of-memory; just put a sane cap on it.
        if (_scopeStack.Count >= _maxScopes) return Error.StackOverflow;

        _scopeStack.Push(_currentScope);

        _currentScope.RtfInternalState = RtfInternalState.Normal;

        _groupCount++;

        return Error.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Error PopScope()
    {
        if (_scopeStack.Count == 0) return Error.StackUnderflow;

        _scopeStack.Pop(_currentScope);
        _groupCount--;

        return Error.OK;
    }

    #endregion

    protected Error ParseKeyword()
    {
        bool hasParam = false;
        bool negateParam = false;
        int param = 0;

        if (!GetNextChar(out char ch)) return Error.EndOfFile;

        _keyword.ClearFast();

        if (!ch.IsAsciiAlpha())
        {
            /* From the spec:
             "A control symbol consists of a backslash followed by a single, non-alphabetical character.
             For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
             delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

             So just go straight to dispatching without looking for a param and without eating the space.
            */
            _keyword.AddFast(ch);
            return DispatchKeyword(0, false);
        }

        int i;
        bool eof = false;
        for (i = 0; i < _keywordMaxLen && ch.IsAsciiAlpha(); i++, eof = !GetNextChar(out ch))
        {
            if (eof) return Error.EndOfFile;
            _keyword.AddFast(ch);
        }
        if (i > _keywordMaxLen) return Error.KeywordTooLong;

        if (ch == '-')
        {
            negateParam = true;
            if (!GetNextChar(out ch)) return Error.EndOfFile;
        }

        if (ch.IsAsciiNumeric())
        {
            hasParam = true;

            // Parse param in real-time to avoid doing a second loop over
            for (i = 0; i < _paramMaxLen && ch.IsAsciiNumeric(); i++, eof = !GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                param += ch - '0';
                param *= 10;
            }
            // Undo the last multiply just one time to avoid checking if we should do it every time through
            // the loop
            param /= 10;
            if (i > _paramMaxLen) return Error.ParameterTooLong;

            if (negateParam) param = -param;
        }

        /* From the spec:
         "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI characters)
         that is not counted in the characters to skip."
         This implements the spec for regular control words and \uN alike. Nothing extra needed for removing
         the space from the skip-chars to count.
        */
        if (ch != ' ') UnGetChar(ch);

        return DispatchKeyword(param, hasParam);
    }

    protected abstract Error DispatchKeyword(int param, bool hasParam);
}
