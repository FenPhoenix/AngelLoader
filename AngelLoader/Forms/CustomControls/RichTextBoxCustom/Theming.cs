//#define BYTE_IDENTICALITY_TEST

using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;

internal sealed partial class RichTextBoxCustom
{
#if BYTE_IDENTICALITY_TEST

    internal static async System.Threading.Tasks.Task DoByteIdenticalityTest()
    {
        foreach (FanMission fm in Global.FMsViewList)
        {
            FMCache.CacheData cacheData = await FMCache.GetCacheableData(fm, false);
            foreach (string readme in cacheData.Readmes)
            {
                string oldSel = fm.SelectedReadme;

                try
                {
                    fm.SelectedReadme = readme;
                    (string readmePath, ReadmeType readmeType) = Core.GetReadmeFileAndType(fm);
                    if (readmeType == ReadmeType.RichText)
                    {
                        Core.View.LoadReadmeContent(readmePath, readmeType, null);
                    }
                }
                finally
                {
                    fm.SelectedReadme = oldSel;
                }
            }
        }
    }

#endif

    private bool _changedThemeWhileDisabled;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            SetReadmeTypeAndColorState(_currentReadmeType);
            Lazy_RTFBoxMenu.DarkModeEnabled = value;
            // Perf: Don't load readme twice on startup, and don't load it again if we're on HTML or no FM
            // selected or whatever
            if (Visible) RefreshDarkModeState(preProcessedRtf: null, skipSuspend: false);

            if (Visible && !Enabled)
            {
                _changedThemeWhileDisabled = true;
                Native.EnableWindow(new HandleRef(this, Handle), _darkModeEnabled);
            }
        }
    }

    #region Methods

    private void SetReadmeTypeAndColorState(ReadmeType readmeType)
    {
        _currentReadmeType = readmeType;

        (BackColor, ForeColor) = (readmeType is ReadmeType.PlainText or ReadmeType.Wri) && _darkModeEnabled
            ? (DarkColors.Fen_DarkBackground, DarkColors.Fen_DarkForeground)
            : (SystemColors.Window, SystemColors.WindowText);
    }

    private void RefreshDarkModeState(
#if BYTE_IDENTICALITY_TEST
        string path = "",
#endif
        PreProcessedRTF? preProcessedRtf = null,
        bool skipSuspend = false)
    {
        // Save/restore scroll position even for plaintext, because merely setting the fore/back colors makes
        // our scroll position bump itself slightly. Weird.

        bool plainText = _currentReadmeType == ReadmeType.PlainText;

        bool toggleReadOnly = _currentReadmeType is ReadmeType.RichText or ReadmeType.GLML;

        Native.SCROLLINFO si = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
        try
        {
            if (!skipSuspend)
            {
                if (!plainText) SaveZoom();
                this.SuspendDrawing();
                if (toggleReadOnly) ReadOnly = false;
            }

            if (_currentReadmeType == ReadmeType.RichText)
            {
                if (preProcessedRtf != null)
                {
                    using MemoryStream ms = new(preProcessedRtf.ProcessedBytes);
                    // @NET5: On modern .NET, RichTextBox now throws if the rtf is broken.
                    LoadFile(ms, RichTextBoxStreamType.RichText);
                }
                else
                {
                    byte[] bytes = RtfProcessing.GetProcessedRTFBytes(_currentReadmeBytes, _darkModeEnabled);
                    using MemoryStream ms = new(bytes);
                    LoadFile(ms, RichTextBoxStreamType.RichText);

#if BYTE_IDENTICALITY_TEST
                    if (!string.IsNullOrEmpty(path))
                    {
                        string theme = _darkModeEnabled ? "dark" : "light";
                        // Say "new" if this is the current version; "old" if it's the previous version
                        string dir = @"C:\rtf_byte_identicality_new_" + theme;
                        Directory.CreateDirectory(dir);

                        File.WriteAllBytes(Path.Combine(dir, path.Replace(":", "_").Replace("\\", "_").Replace("/", "_")), bytes);
                    }
#endif
                }
            }
            else if (_currentReadmeType == ReadmeType.GLML)
            {
                Rtf = GLMLConversion.GLMLToRTF(_currentReadmeBytes, _darkModeEnabled);
            }
        }
        // Let exceptions propagate back, so we'll get the "unable to load readme" message.
        finally
        {
            RTFPreprocessing.SwitchOffPreloadState();

            if (!skipSuspend)
            {
                if (!plainText)
                {
                    if (toggleReadOnly) ReadOnly = true;
                    RestoreZoom();
                }

                ControlUtils.RepositionScroll(Handle, si, Native.SB_VERT);
                this.ResumeDrawing();
            }
        }
    }

    #endregion
}
