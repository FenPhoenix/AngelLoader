using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AL_Common.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;

internal sealed partial class RichTextBoxCustom
{
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
            // Perf: Don't load readme twice on startup, and don't load it again if we're on HTML or no FM
            // selected or whatever
            if (Visible) RefreshDarkModeState(preProcessedRtf: null, skipSuspend: false);
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

    private void RefreshDarkModeState(PreProcessedRTF? preProcessedRtf = null, bool skipSuspend = false)
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
                byte[] bytes = preProcessedRtf != null
                    ? preProcessedRtf.Bytes
                    : RtfProcessing.GetProcessedRTFBytes(_currentReadmeBytes, _darkModeEnabled);
                using var ms = new MemoryStream(bytes);
                LoadFile(ms, RichTextBoxStreamType.RichText);
            }
            else if (_currentReadmeType == ReadmeType.GLML)
            {
                Rtf = GLMLConversion.GLMLToRTF(_currentReadmeBytes, _darkModeEnabled);
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.Un + "set dark mode to " + _darkModeEnabled, ex);
        }
        finally
        {
            SwitchOffPreloadState();

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
