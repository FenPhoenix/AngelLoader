using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AL_Common;
using static AL_Common.Common;

namespace AngelLoader.Forms.CustomControls;

internal sealed partial class RichTextBoxCustom
{
    private sealed class PreProcessedRTF
    {
        private readonly string _fileName;
        internal readonly byte[] Bytes;
        private readonly bool _darkMode;

        /*
        It's possible for us to preload a readme but then end up on a different FM. It could happen if we
        filter out the selected FM that was specified in the config, or if we load in new FMs and we reset
        our selection, etc. So make sure the readme we want to display is in fact the one we preloaded.
        Otherwise, we're just going to cancel the preload and load the new readme normally.
        */
        internal bool Identical(string fileName, bool darkMode) =>
            // Ultra paranoid checks
            !fileName.IsWhiteSpace() &&
            !_fileName.IsWhiteSpace() &&
            _fileName.PathEqualsI(fileName) &&
            _darkMode == darkMode;

        internal PreProcessedRTF(string fileName, byte[] bytes, bool darkMode)
        {
            _fileName = fileName;
            Bytes = bytes;
            _darkMode = darkMode;
        }
    }

    private static PreProcessedRTF? _preProcessedRTF;

    /// <summary>
    /// Perform pre-processing that needs to be done regardless of visual theme.
    /// </summary>
    /// <param name="bytes"></param>
    private static byte[] GlobalPreProcessRTF(byte[] bytes)
    {
        /*
        It's six of one half a dozen of the other - each method causes rare cases of images
        not showing, but for different files.
        And trying to get too clever and specific about it (if shppict says pngblip, and
        nonshppict says wmetafile, then DON'T patch shppict, otherwise do, etc.) is making
        me uncomfortable. I don't even know what Win7 or Win11 will do with that kind of
        overly-specific meddling. Microsoft have changed their RichEdit control before, and
        they might again, in which case I'm screwed either way.
        */
        ReplaceByteSequence(bytes, _shppict, _shppictBlanked);
        ReplaceByteSequence(bytes, _nonshppict, _nonshppictBlanked);

        return ReplaceLangsWithAnsiCpg(bytes);
    }

    private static readonly ListFast<byte> _codePageBytes = new(RTFParserBase.MaxLangNumDigits);

    /*
    @RTF(\langN processing):
    We could do a proper parse for this and then we would be able to reject \langNs that point to the same code
    page as the current font, and maybe avoid some inserts (save a bit of memory?). But then on the other hand,
    we'd have to do a full-file parse always, instead of exiting after finding the color table. And that would
    certainly be slower than just blazing through with a byte search like we do here. So, meh.
    */
    private static byte[] ReplaceLangsWithAnsiCpg(byte[] bytes)
    {
        static int GetDigitsUpTo5(int number)
        {
            return
                number <= 9 ? 1 :
                number <= 99 ? 2 :
                number <= 999 ? 3 :
                number <= 9999 ? 4 :
                5;
        }

        static ListFast<byte> CodePageToBytes(int codePage, int digits)
        {
            // Use global 3-byte list and do allocation-less clears and inserts, otherwise we would allocate
            // a new byte array EVERY time through here (which is a lot)
            _codePageBytes.ClearFast();

            for (int i = 0; i < digits; i++)
            {
                _codePageBytes.InsertAtZeroFast((byte)((codePage % 10) + '0'));
                codePage /= 10;
            }

            return _codePageBytes;
        }

        var langIndexes = new List<(int Index, int CodePage, int CodePageDigitCount)>();

        int startFrom = 0;
        while (startFrom > -1 && startFrom < bytes.Length - 1)
        {
            (int start, int end) = FindIndexOfLangWithNum(bytes, startFrom);
            if (start > -1 && end > -1 &&
                end - (start + 5) <= RTFParserBase.MaxLangNumDigits)
            {
                int num = 0;
                for (int i = start + 5; i < end; i++)
                {
                    byte b = bytes[i];
                    if (b.IsAsciiNumeric())
                    {
                        num *= 10;
                        num += b - '0';
                    }
                }

                if (num <= RTFParserBase.MaxLangNumIndex)
                {
                    int codePage = RTFParserBase.LangToCodePage[num];
                    if (codePage > -1)
                    {
                        langIndexes.Add((end, codePage, GetDigitsUpTo5(codePage)));
                    }
                }
            }
            startFrom = end;
        }

        if (langIndexes.Count == 0) return bytes;

        int extraLength = 0;

        int ansiCpgLength = _ansicpg.Length;

        for (int i = 0; i < langIndexes.Count; i++)
        {
            (_, _, int codePageDigitCount) = langIndexes[i];
            extraLength += ansiCpgLength + codePageDigitCount;
        }

        /*
        @RTF(\langN)/@MEM: Temporary memory hog just to get it working
        We should combine this in with GetDarkModeRTFBytes(), and then call it always, but have a bool saying
        whether it's dark mode so we can skip the color table generation for light mode. Because the one-time
        new byte array with the final size happens in there, so if we combine this in, we don't have to create
        another byte array here.

        Granted, we already exit early and return the same array we were passed in if we find no work to do,
        so we're already in pretty decent shape. Our worst case is Beginning of Era Karath-Din's 4.5MB readme,
        which is not that bad.
        */
        byte[] newBytes = new byte[bytes.Length + extraLength];

        int lastIndex = 0;
        int newBytePointer = 0;
        for (int i = 0; i < langIndexes.Count; i++)
        {
            (int index, int codePage, int codePageDigitCount) = langIndexes[i];

            ListFast<byte> cpgBytes = CodePageToBytes(codePage, codePageDigitCount);

            ReadOnlySpan<byte> byteSpan = bytes.AsSpan(lastIndex, index - lastIndex);
            byteSpan.CopyTo(newBytes.AsSpan().Slice(newBytePointer));
            lastIndex = index;

            newBytePointer += byteSpan.Length;

            ReadOnlySpan<byte> ansiCpgSpan = _ansicpg.AsSpan();
            ansiCpgSpan.CopyTo(newBytes.AsSpan().Slice(newBytePointer));

            newBytePointer += ansiCpgLength;

            ReadOnlySpan<byte> codePageSpan = cpgBytes.ItemsArray.AsSpan().Slice(0, cpgBytes.ItemsArray.Length);
            codePageSpan.CopyTo(newBytes.AsSpan().Slice(newBytePointer));

            newBytePointer += codePageDigitCount;
        }

        ReadOnlySpan<byte> segmentLast = bytes.AsSpan(lastIndex);
        segmentLast.CopyTo(newBytes.AsSpan().Slice(newBytePointer));

        return newBytes;
    }

    private static (int Start, int End) FindIndexOfLangWithNum(byte[] input, int start = 0)
    {
        byte firstByte = _lang[0];
        int index = Array.IndexOf(input, firstByte, start);

        while (index > -1)
        {
            for (int i = 0; i < _lang.Length; i++)
            {
                if (index + i >= input.Length) return (-1, -1);
                if (_lang[i] != input[index + i])
                {
                    if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return (-1, -1);
                    break;
                }

                if (i == _lang.Length - 1)
                {
                    int firstDigitIndex = index + i + 1;

                    int numIndex = firstDigitIndex;
                    while (numIndex < input.Length - 1 && input[numIndex].IsAsciiNumeric())
                    {
                        numIndex++;
                    }
                    if (numIndex > firstDigitIndex)
                    {
                        return (index, numIndex);
                    }
                    else
                    {
                        index = numIndex;
                    }
                }
            }
        }

        return (-1, -1);
    }

    [MemberNotNullWhen(true, nameof(_preProcessedRTF))]
    private static bool InPreloadedState(string readmeFile, bool darkMode)
    {
        if (_preProcessedRTF?.Identical(readmeFile, darkMode) == true)
        {
            return true;
        }
        else
        {
            SwitchOffPreloadState();
            return false;
        }
    }

    private static void SwitchOffPreloadState() => _preProcessedRTF = null;

    public static void PreloadRichFormat(string readmeFile, byte[] preloadedBytesRaw, bool darkMode)
    {
        _currentReadmeBytes = preloadedBytesRaw;

        try
        {
            _currentReadmeBytes = GlobalPreProcessRTF(_currentReadmeBytes);

            _preProcessedRTF = new PreProcessedRTF(
                readmeFile,
                darkMode ? RtfTheming.GetDarkModeRTFBytes(_currentReadmeBytes) : _currentReadmeBytes,
                darkMode
            );
        }
        catch
        {
            SwitchOffPreloadState();
        }
    }
}
