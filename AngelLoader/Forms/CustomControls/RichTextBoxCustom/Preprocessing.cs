using System.Diagnostics.CodeAnalysis;
using AL_Common;
using static AL_Common.Common;

namespace AngelLoader.Forms.CustomControls;

internal sealed class PreProcessedRTF
{
    private readonly string _fileName;
    internal readonly byte[] OriginalBytes;
    internal readonly byte[] ProcessedBytes;
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

    internal PreProcessedRTF(string fileName, byte[] originalBytes, byte[] processedBytes, bool darkMode)
    {
        _fileName = fileName;
        OriginalBytes = originalBytes;
        ProcessedBytes = processedBytes;
        _darkMode = darkMode;
    }
}

internal static class RTFPreprocessing
{
    private static PreProcessedRTF? _preProcessedRTF;

    internal static PreProcessedRTF? GetPreProcessedRtf() => _preProcessedRTF;

    /// <summary>
    /// Perform pre-processing that needs to be done regardless of visual theme.
    /// </summary>
    /// <param name="bytes"></param>
    internal static void GlobalPreProcessRTF(byte[] bytes)
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
        ReplaceByteSequence(bytes, RichTextBoxCustom._shppict, RichTextBoxCustom._shppictBlanked);
        ReplaceByteSequence(bytes, RichTextBoxCustom._nonshppict, RichTextBoxCustom._nonshppictBlanked);
    }

    [MemberNotNullWhen(true, nameof(_preProcessedRTF))]
    internal static bool InPreloadedState(string readmeFile, bool darkMode)
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

    internal static void SwitchOffPreloadState() => _preProcessedRTF = null;

    public static void PreloadRichFormat(string readmeFile, byte[] preloadedBytesRaw, bool darkMode)
    {
        try
        {
            GlobalPreProcessRTF(preloadedBytesRaw);

            _preProcessedRTF = new PreProcessedRTF(
                fileName: readmeFile,
                originalBytes: preloadedBytesRaw,
                processedBytes: RtfProcessing.GetProcessedRTFBytes(preloadedBytesRaw, darkMode),
                darkMode: darkMode
            );
        }
        catch
        {
            SwitchOffPreloadState();
        }
    }
}
