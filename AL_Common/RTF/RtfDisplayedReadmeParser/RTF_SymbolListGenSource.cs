/*
TODO(gperf stuff): Tidy this up and get rid of hardcoded directories and whatnot
*/

//#define SYMBOL_PERFECT_HASH_GEN

#if SYMBOL_PERFECT_HASH_GEN

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AL_Common.RTF;

public static class RTF_SymbolListGenSource
{
    // This is the original "canonical" list, generate the perfect hash from this
    private static readonly Symbol[] _symbolList =
    {
        #region Code pages / charsets / fonts

        new Symbol("ansi", 0, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),

        new Symbol("pc", 437, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),

        // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
        // assume 10000 ("Mac Roman")
        new Symbol("mac", 10000, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),

        new Symbol("pca", 850, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        new Symbol("ansicpg", 0, false, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),

        new Symbol("deff", 0, false, KeywordType.Special, (ushort)SpecialType.DefaultFont),

        new Symbol("fonttbl", 0, false, KeywordType.Special, (ushort)SpecialType.FontTable),
        //new Symbol("f", 0, false, KeywordType.Property, (ushort)Property.FontNum),
        new Symbol("fcharset", ushort.MaxValue, false, KeywordType.FCharset, 0),
        new Symbol("cpg", ushort.MaxValue, false, KeywordType.CPG, 0),

        #endregion

        new Symbol("lang", 0, false, KeywordType.Property, (ushort)Property.Lang),

        #region Encoded characters

        new Symbol("uc", 1, false, KeywordType.Property, (ushort)Property.UnicodeCharSkipCount),
        new Symbol("u", 0, false, KeywordType.Special, (ushort)SpecialType.UnicodeChar),

        #endregion

        // \v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again
        new Symbol("v", 1, false, KeywordType.Property, (ushort)Property.Hidden),

        #region Newlines

        new Symbol("par", 0, false, KeywordType.Character, '\n'),
        new Symbol("line", 0, false, KeywordType.Character, '\n'),
        new Symbol("sect", 0, false, KeywordType.Character, '\n'),

        #endregion

        #region Control words that map to a single character

        new Symbol("tab", 0, false, KeywordType.Character, '\t'),
        new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
        new Symbol("emspace", 0, false, KeywordType.Character, '\x2003'),
        new Symbol("enspace", 0, false, KeywordType.Character, '\x2002'),
        new Symbol("qmspace", 0, false, KeywordType.Character, '\x2005'),
        new Symbol("emdash", 0, false, KeywordType.Character, '\x2014'),
        new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
        new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
        new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
        new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
        new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),

        new Symbol("zwbo", 0, false, KeywordType.Character, '\x200B'),
        new Symbol("zwnbo", 0, false, KeywordType.Character, '\xFEFF'),
        new Symbol("zwj", 0, false, KeywordType.Character, '\x200D'),
        new Symbol("zwnj", 0, false, KeywordType.Character, '\x200C'),

        #endregion

        new Symbol("bin", 0, false, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),

        new Symbol("fldinst", 0, false, KeywordType.Destination, (ushort)DestinationType.FieldInstruction),

        new Symbol("cs", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
        new Symbol("ds", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
        new Symbol("ts", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),

        #region Custom skip-destinations

        // TODO(listtext/pntext): Temporarily disabled with a hack, but decide what we want to do here
        new Symbol("listtext", 0, false, KeywordType.Destination, 255),
        new Symbol("pntext", 0, false, KeywordType.Destination, 255),

        #endregion

        #region Required skip-destinations

        new Symbol("author", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("buptim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("colortbl", 0, false, KeywordType.Special, (ushort)SpecialType.ColorTable),
        new Symbol("comment", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("creatim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("doccomm", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("footer", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("footerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("footerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("footerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("footnote", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("ftncn", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("ftnsep", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("ftnsepc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("header", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("headerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("headerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("headerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("info", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("keywords", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("operator", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("printim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("private", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("revtim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("rxe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("stylesheet", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("subject", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("tc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("title", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("txe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        new Symbol("xe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),

        #region Groups containing skippable hex data ("#SDATA")

        new Symbol("pict", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("themedata", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("colorschememapping", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("passwordhash", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("datastore", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("datafield", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("objdata", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        new Symbol("blipuid", 32, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        new Symbol("panose", 20, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),

        #endregion

        #region Quick table hacks

        new Symbol("row", 0, false, KeywordType.Special, (ushort)SpecialType.CellRowEnd),
        new Symbol("cell", 0, false, KeywordType.Character, '\t'),
        new Symbol("nestrow", 0, false, KeywordType.Special, (ushort)SpecialType.CellRowEnd),
        new Symbol("nestcell", 0, false, KeywordType.Character, '\t'),

        #endregion

        #endregion
    };

    /*
    Generate with gperf 3.1. It's GNU so they're way into their source code with no binaries ever, but binaries
    can be found on Chocolatey at least. Slightly inconvenient but oh well.

    Make sure the above array has only ONE LINE per entry! The generator code in here just does a cheap line-
    by-line search through the array code, no parsing or anything. So it won't work if an entry is broken up
    over multiple lines.
    
    Instructions for semi-automatic perfect hash function regeneration (for updates requiring such):

    1. Call ConvertSymbolListToGPerfFormat().
    3. Copy the contents of the symbols array above (just the body, not the header or closing brace) to a file.
       Call it symbolsCodeFile.
    4. Copy the contents of the gperf-generated table from gperfOutputFile.txt (again, just the body) to another file.
       Call it inputFile.
    5. Call ConvertGPerfOutputToCSharp().
    6. Copy the C# symbols array-body code out of outputFile.txt and paste it into the symbols array in the
       main file, overwriting the previous symbols array body.
    7. Port over the rest of the relevant code in the gperf output file (if necessary - some of it may not
       have changed).
    8. Done!
    */

    private const string genDir = @"C:\_al_rtf_table_gen";
    private const string gperfExePath = @"C:\gperf\tools\gperf.exe";

    public static void ConvertSymbolListToGPerfFormat()
    {
        Directory.CreateDirectory(genDir);

        string gperfFormatFile = Path.Combine(genDir, "gperfFormatFile.txt");

        List<string> outLines = new()
        {
            "struct Symbol { char *name; int dummy; };",
            "%%",
        };

        for (int i = 0; i < _symbolList.Length; i++)
        {
            Symbol symbol = _symbolList[i];
            outLines.Add(symbol.Keyword + ", 0");
        }
        File.WriteAllLines(gperfFormatFile, outLines);

        // gperf --output-file=[gperf output file] -t [gperfFormatFile]
        using (Process.Start(
                   gperfExePath,
                   "--output-file=" + Path.Combine(genDir, "gperfOutputFile.txt") + " " +
                   // -r = random, which increases the size of the table, leading to faster misses from more null
                   // hits
                   "-r " +
                   "-t " +
                   gperfFormatFile))
        {
        }
    }

    public static void ConvertGPerfOutputToCSharp()
    {
        string inputFile = Path.Combine(genDir, "inputFile.txt");
        string outputFile = Path.Combine(genDir, "outputFile.txt");
        string symbolsCodeFile = Path.Combine(genDir, "symbolsCodeFile.txt");

        static int FindIndexOfValueInSymbolList(string value)
        {
            for (int i = 0; i < _symbolList.Length; i++)
            {
                if (_symbolList[i].Keyword == value)
                {
                    return i;
                }
            }
            return -1;
        }

        string[] codeLines = File.ReadAllLines(symbolsCodeFile);
        string[] symbolLines = new string[_symbolList.Length];
        for (int i = 0, j = 0; i < codeLines.Length; i++)
        {
            string codeLine = codeLines[i].Trim();
            if (codeLine.StartsWithO("new " + nameof(Symbol)))
            {
                symbolLines[j] = codeLine;
                j++;
            }
        }

        string[] lines = File.ReadAllLines(inputFile);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWithO("#line"))
            {
                string lineNumRaw = line.Substring(5, line.IndexOf('\"') - 5).Trim();
                if (Int_TryParseInv(lineNumRaw, out int result))
                {
                    // -3 because the gperf in format is like
                    // (line 1) struct declaration
                    // (line 2) %%
                    // (line 3) 1st entry (but 3rd line)
                    lines[i] = "// Entry " + (result - 3);
                }
            }
            else
            {
                lines[i] = lines[i].Replace("{\"\"}", "null");
                string lineT = lines[i].Trim();
                Match m = Regex.Match(lineT, @"{\""(?<Value>[^\""]+)");
                if (m.Success)
                {
                    string value = m.Groups["Value"].Value;
                    value = value.Replace(@"\\", @"\");
                    int symbolIndex = FindIndexOfValueInSymbolList(value);
                    if (symbolIndex > -1)
                    {
                        lines[i] = symbolLines[symbolIndex];
                    }
                }
            }
        }

        File.WriteAllLines(outputFile, lines);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Int_TryParseInv(string s, out int result)
    {
        return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool StartsWithO(this string str, string value) => str.StartsWith(value, StringComparison.Ordinal);
}

#endif
