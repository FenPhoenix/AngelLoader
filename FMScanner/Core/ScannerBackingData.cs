using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AL_Common;
using static AL_Common.Common;

namespace FMScanner;

public sealed partial class Scanner
{
    private readonly byte[] _rtfHeaderBuffer = new byte[RTFHeaderBytes.Length];

    private readonly byte[] _misChunkHeaderBuffer = new byte[12];

    private ListFast<char>? _utf32CharBuffer;
    private ListFast<char> Utf32CharBuffer => _utf32CharBuffer ??= new ListFast<char>(2);

    private readonly BinaryBuffer _binaryReadBuffer = new();

    internal const char LeftDoubleQuote = '\u201C';
    internal const char RightDoubleQuote = '\u201D';

    internal readonly struct AsciiCharWithNonAsciiEquivalent(char original, char ascii)
    {
        internal readonly char Original = original;
        internal readonly char Ascii = ascii;
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private static class FMDirs
    {
        // PERF: const string concatenation is free (const concats are done at compile time), so do it to lessen
        // the chance of error.

        // We only need BooksS
        internal const string Fam = "fam";
        // We only need IntrfaceS
        internal const string Mesh = "mesh";
        internal const string Motions = "motions";
        internal const string Movies = "movies";
        internal const string Cutscenes = "cutscenes"; // SS2 only
        internal const string Obj = "obj";
        internal const string Scripts = "scripts";
        internal const string Snd = "snd";
        internal const string Snd2 = "snd2"; // SS2 only
        // We only need StringsS
        internal const string Subtitles = "subtitles";

        internal const string BooksS = "books/";
        internal const string FamS = Fam + "/";
        internal const string IntrfaceS = "intrface/";
        internal const int IntrfaceSLen = 9; // workaround for .NET 4.7.2 not inlining const string lengths
        internal const string MeshS = Mesh + "/";
        internal const string MotionsS = Motions + "/";
        internal const string MoviesS = Movies + "/";
        internal const string CutscenesS = Cutscenes + "/"; // SS2 only
        internal const string ObjS = Obj + "/";
        internal const string ScriptsS = Scripts + "/";
        internal const string SndS = Snd + "/";
        internal const string Snd2S = Snd2 + "/"; // SS2 only
        internal const string StringsS = "strings/";
        internal const string SubtitlesS = Subtitles + "/";

        internal const string T3FMExtras1S = "Fan Mission Extras/";
        internal const string T3FMExtras2S = "FanMissionExtras/";

        internal const string T3DetectS = "Content/T3/Maps/";
        internal const int T3DetectSLen = 16; // workaround for .NET 4.7.2 not inlining const string lengths
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    private static class FMFiles
    {
        internal const string SS2Fingerprint1 = "/usemsg.str";
        internal const string SS2Fingerprint2 = "/savename.str";
        internal const string SS2Fingerprint3 = "/objlooks.str";
        internal const string SS2Fingerprint4 = "/OBJSHORT.str";

        internal const string IntrfaceEnglishNewGameStr = "intrface/english/newgame.str";
        internal const string IntrfaceNewGameStr = "intrface/newgame.str";
        internal const string SNewGameStr = "/newgame.str";

        internal const string StringsMissFlag = "strings/missflag.str";
        internal const string StringsEnglishMissFlag = "strings/english/missflag.str";
        internal const string SMissFlag = "/missflag.str";

        // Telliamed's fminfo.xml file, used in a grand total of three missions
        internal const string FMInfoXml = "fminfo.xml";

        // fm.ini, a NewDark (or just FMSel?) file
        internal const string FMIni = "fm.ini";

        // System Shock 2 file
        internal const string ModIni = "mod.ini";

        // For Thief 3 missions, all of them have this file, and then any other .gmp files are the actual missions
        internal const string EntryGmp = "Entry.gmp";

        internal const string TDM_DarkModTxt = "darkmod.txt";
        internal const string TDM_ReadmeTxt = "readme.txt";
        internal const string TDM_MapSequence = "tdm_mapsequence.txt";
    }

    #region Game detection

    private const int _gameTypeBufferSize = 81_920;

    private byte[]? _gameTypeBuffer_ChunkPlusRopeyArrow;
    private byte[] GameTypeBuffer_ChunkPlusRopeyArrow => _gameTypeBuffer_ChunkPlusRopeyArrow ??= new byte[_gameTypeBufferSize + _ctx.RopeyArrow.Length];

    private byte[]? _gameTypeBuffer_ChunkPlusMAPPARAM;
    private byte[] GameTypeBuffer_ChunkPlusMAPPARAM => _gameTypeBuffer_ChunkPlusMAPPARAM ??= new byte[_gameTypeBufferSize + MAPPARAM.Length];

    internal const int _ss2MapParamNewDarkLoc = 696;
    internal const int _oldDarkT2Loc = 772;
    internal const int _ss2MapParamOldDarkLoc = 916;
    // Neither of these clash with SS2's SKYOBJVAR locations (3168, 7292).
    internal const int _newDarkLoc1 = 7217;
    internal const int _newDarkLoc2 = 3093;

    internal const int _ss2NewDarkOffset = 705; // 696+9 = 705
    internal const int _t2OldDarkOffset = 76;   // (772+9)-705 = 76
    internal const int _ss2OldDarkOffset = 144; // ((916+9)-76)-705 = 144
    internal const int _newDarkOffset1 = 2177;  // (((3093+9)-144)-76)-705 = 2177
    internal const int _newDarkOffset2 = 4124;  // ((((7217+9)-2177)-144)-76)-705 = 4124

    private readonly byte[][] _zipOffsetBuffers =
    {
        new byte[_ss2NewDarkOffset],
        new byte[_t2OldDarkOffset],
        new byte[_ss2OldDarkOffset],
        new byte[_newDarkOffset1],
        new byte[_newDarkOffset2],
    };

    // MAPPARAM is 8 bytes, so for that we just check the first 8 bytes and ignore the last, rather than
    // complicating things any further than they already are.
    private const int _gameDetectStringBufferLength = 9;
    private readonly byte[] _gameDetectStringBuffer = new byte[_gameDetectStringBufferLength];

    // ReSharper restore IdentifierTypo

    #endregion

    /// <summary>
    /// Specialized (therefore fast) sort for titles.str lines only. Anything else is likely to throw an
    /// IndexOutOfRangeException.
    /// </summary>
    internal sealed class TitlesStrNaturalNumericSort : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x.IsEmpty()) return -1;
            if (y.IsEmpty()) return 1;

            // int32 max digits minus 1 (to avoid having to check for overflow)
            const int maxDigits = 9;

            int xIndex1 = x.IndexOf('_');
            int xIndex2 = x.IndexOf(':', xIndex1);

            int xNum = 0;
            int xEnd = Math.Min(xIndex2, xIndex1 + maxDigits);
            for (int i = xIndex1 + 1; i < xEnd; i++)
            {
                char c = x[i];
                if (c.IsAsciiNumeric())
                {
                    xNum *= 10;
                    xNum += c - '0';
                }
                else
                {
                    return 0;
                }
            }

            int yIndex1 = y.IndexOf('_');
            int yIndex2 = y.IndexOf(':', yIndex1);

            int yNum = 0;
            int yEnd = Math.Min(yIndex2, yIndex1 + maxDigits);
            for (int i = yIndex1 + 1; i < yEnd; i++)
            {
                char c = y[i];
                if (c.IsAsciiNumeric())
                {
                    yNum *= 10;
                    yNum += c - '0';
                }
                else
                {
                    return 0;
                }
            }

            return xNum - yNum;
        }
    }

    internal sealed class FMScanOriginalIndexComparer : IComparer<ScannedFMDataAndError>
    {
        public int Compare(ScannedFMDataAndError x, ScannedFMDataAndError y)
        {
            return x.OriginalIndex.CompareTo(y.OriginalIndex);
        }
    }
}
