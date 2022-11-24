using System.Diagnostics.CodeAnalysis;
using static AL_Common.Common;
namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        private sealed class PreProcessedRTF
        {
            private readonly string FileName;
            internal readonly byte[] Bytes;
            private readonly bool DarkMode;

            /*
            It's possible for us to preload a readme but then end up on a different FM. It could happen if we
            filter out the selected FM that was specified in the config, or if we load in new FMs and we reset
            our selection, etc. So make sure the readme we want to display is in fact the one we preloaded.
            Otherwise, we're just going to cancel the preload and load the new readme normally.
            */
            internal bool Identical(string fileName, bool darkMode) =>
                // Ultra paranoid checks
                !fileName.IsWhiteSpace() &&
                !FileName.IsWhiteSpace() &&
                FileName.PathEqualsI(fileName) &&
                DarkMode == darkMode;

            internal PreProcessedRTF(string fileName, byte[] bytes, bool darkMode)
            {
                FileName = fileName;
                Bytes = bytes;
                DarkMode = darkMode;
            }
        }

        private static PreProcessedRTF? _preProcessedRTF;

        /// <summary>
        /// Perform pre-processing that needs to be done regardless of visual theme.
        /// </summary>
        /// <param name="bytes"></param>
        private static void GlobalPreProcessRTF(byte[] bytes)
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
        }

        [MemberNotNullWhen(true, nameof(_preProcessedRTF))]
        private static bool InPreloadedState(string readmeFile, bool darkMode)
        {
            if (_preProcessedRTF != null && _preProcessedRTF.Identical(readmeFile, darkMode))
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
                GlobalPreProcessRTF(_currentReadmeBytes);

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
}
