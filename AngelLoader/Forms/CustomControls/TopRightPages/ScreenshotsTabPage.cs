//#define TESTING

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class ScreenshotsTabPage : Lazy_TabsBase
{
    private Lazy_ScreenshotsPage _page = null!;

    private readonly List<string> ScreenshotFileNames = new();
    private string CurrentScreenshotFileName = "";
    private MemoryImage? _currentScreenshotStream;
    private readonly Timer CopiedMessageFadeoutTimer = new();
    private bool _forceUpdateArmed;

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_ScreenshotsPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.GammaTrackBar.Value = Config.ScreenshotGammaPercent;

            CopiedMessageFadeoutTimer.Interval = 2500;
            CopiedMessageFadeoutTimer.Tick += CopiedMessageFadeoutTimer_Tick;

            _page.ScreenshotsPictureBox.MouseClick += ScreenshotsPictureBox_MouseClick;

            _page.OpenScreenshotsFolderButton.Click += OpenScreenshotsFolderButton_Click;
            _page.PrevButton.Click += ScreenshotsPrevButton_Click;
            _page.NextButton.Click += ScreenshotsNextButton_Click;
            _page.GammaTrackBar.Scroll += GammaTrackBar_Scroll;
            _page.GammaTrackBar.MouseDown += GammaTrackBar_MouseDown;
            _page.GammaTrackBar.DoubleClickEndingOnMouseDown += GammaTrackBar_MouseDoubleClick;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        UpdatePageInternal();
    }

    public override void Localize()
    {
        if (!_constructed) return;
        _owner.MainToolTip.SetToolTip(_page.ScreenshotsPictureBox, LText.ScreenshotsTab.CopyScreenshotToolTip);
        _page.GammaLabel.Text = LText.ScreenshotsTab.Gamma;
    }

    public void RefreshScreenshots()
    {
        if (!_constructed) return;

        _forceUpdateArmed = true;
        UpdatePageInternal();
    }

    #endregion

    #region Page

    private void UpdatePageInternal(int index = -1)
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();
        if (fm != null && fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
        {
            Screenshots.GetScreenshotWatcher(gameIndex).Construct();
        }

        if (_owner.StartupState && ScreenshotsPreprocessing.HasBeenActivated)
        {
            ScreenshotFileNames.ClearAndAdd_Small(ScreenshotsPreprocessing.ScreenshotFileNames);
        }
        else
        {
            Screenshots.PopulateScreenshotFileNames(fm, ScreenshotFileNames);
        }

        if (ScreenshotFileNames.Count == 0)
        {
            CurrentScreenshotFileName = "";
            ClearCurrentScreenshot();
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.GammaLabel.Enabled = false;
            _page.GammaTrackBar.Enabled = false;
            _page.OpenScreenshotsFolderButton.Enabled = false;
            _page.PrevButton.Enabled = false;
            _page.NextButton.Enabled = false;
            SetNumberLabelText("");
            _forceUpdateArmed = false;
        }
        else
        {
            SetCurrentScreenshotFileName(index);
            DisplayCurrentScreenshot();
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.GammaLabel.Enabled = true;
            _page.GammaTrackBar.Enabled = true;
            _page.OpenScreenshotsFolderButton.Enabled = true;
            _page.PrevButton.Enabled = ScreenshotFileNames.Count > 1;
            _page.NextButton.Enabled = ScreenshotFileNames.Count > 1;
        }
    }

    private void SetCurrentScreenshotFileName(int index = -1)
    {
        CurrentScreenshotFileName = ScreenshotFileNames[index > -1 ? index : ScreenshotFileNames.Count - 1];
    }

    // Manual right-align to avoid needing a FlowLayoutPanel
    private void SetNumberLabelText(string text)
    {
        if (!_constructed) return;
        _page.NumberLabel.Text = text;
        SetNumberLabelPosition();
    }

    private void SetNumberLabelPosition()
    {
        if (!_constructed) return;
        _page.NumberLabel.Location = _page.NumberLabel.Location with
        {
            X = (_page.ClientSize.Width - 8) - _page.NumberLabel.Width
        };
    }

    private void ClearCurrentScreenshot()
    {
        if (!_constructed) return;
        _page.ScreenshotsPictureBox.SetImage(null);
        _currentScreenshotStream?.Dispose();
    }

    /*
    The standard behavior for lazy loaded tabs is that they don't update until loaded, after which they always
    update. However, loading an image could take a significant amount of time, and we don't want users to pay the
    performance cost if they're not actually able to see the image. So we only update when visible (or when we
    become visible).
    */
    private void DisplayCurrentScreenshot()
    {
        if (!_constructed) return;
        if (!_owner.StartupState && !Visible) return;

#if TESTING
        System.Diagnostics.Trace.WriteLine("Startup state: " + _owner.StartupState);
#endif

        if (_forceUpdateArmed ||
            (!CurrentScreenshotFileName.IsEmpty() &&
             // @TDM_CASE when FM is TDM
             _currentScreenshotStream?.Path.EqualsI(CurrentScreenshotFileName) != true))
        {
            try
            {
                if (_owner.StartupState)
                {
                    FanMission? fm = _owner.GetMainSelectedFMOrNull();
                    try
                    {
                        MemoryImage? mi = ScreenshotsPreprocessing.GetMemoryImage(fm);
                        if (mi != null)
                        {
#if TESTING
                            System.Diagnostics.Trace.WriteLine("Preload succeeded");
#endif
                            _currentScreenshotStream = mi;
                        }
                        else
                        {
#if TESTING
                            System.Diagnostics.Trace.WriteLine("Preload failed");
#endif
                            Screenshots.PopulateScreenshotFileNames(fm, ScreenshotFileNames);
                            SetCurrentScreenshotFileName();
                            _currentScreenshotStream?.Dispose();
                            _currentScreenshotStream = new MemoryImage(CurrentScreenshotFileName);
                        }
                    }
                    catch
                    {
#if TESTING
                        System.Diagnostics.Trace.WriteLine("Preload failed");
#endif
                        Screenshots.PopulateScreenshotFileNames(fm, ScreenshotFileNames);
                        SetCurrentScreenshotFileName();
                        _currentScreenshotStream?.Dispose();
                        _currentScreenshotStream = new MemoryImage(CurrentScreenshotFileName);
                    }
                }
                else
                {
#if TESTING
                    System.Diagnostics.Trace.WriteLine("Didn't preload");
#endif
                    _currentScreenshotStream?.Dispose();
                    _currentScreenshotStream = new MemoryImage(CurrentScreenshotFileName);
                }
                _page.ScreenshotsPictureBox.SetImage(_currentScreenshotStream.Img, GetGamma());
            }
            catch
            {
                ClearCurrentScreenshot();
                _page.ScreenshotsPictureBox.SetErrorImage();
            }
            finally
            {
                ScreenshotsPreprocessing.Clear();
                _forceUpdateArmed = false;
                SetNumberLabelText(
                    (ScreenshotFileNames.IndexOf(CurrentScreenshotFileName) + 1).ToStrInv() + " / " +
                    ScreenshotFileNames.Count.ToStrInv()
                );
            }
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            SetNumberLabelPosition();
            DisplayCurrentScreenshot();
        }
    }

    private void ScreenshotsPictureBox_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            CopyImageToClipboard();
        }
    }

    private void OpenScreenshotsFolderButton_Click(object sender, EventArgs e)
    {
        Core.OpenFMScreenshotsFolder(_owner.FMsDGV.GetMainSelectedFM(), CurrentScreenshotFileName);
    }

    private void ScreenshotsPrevButton_Click(object sender, EventArgs e) => CycleScreenshot(step: -1);

    private void ScreenshotsNextButton_Click(object sender, EventArgs e) => CycleScreenshot(step: 1);

    private void CycleScreenshot(int step)
    {
        if (ScreenshotFileNames.Count <= 1) return;
        int index = ScreenshotFileNames.IndexOf(CurrentScreenshotFileName);

        if (index == -1) return;

        index =
            step == 1
                ? index == ScreenshotFileNames.Count - 1 ? 0 : index + 1
                : index == 0 ? ScreenshotFileNames.Count - 1 : index - 1;

        CurrentScreenshotFileName = ScreenshotFileNames[index];
        DisplayCurrentScreenshot();
    }

    private void GammaTrackBar_Scroll(object sender, EventArgs e)
    {
        Config.ScreenshotGammaPercent = _page.GammaTrackBar.Value;
        _page.ScreenshotsPictureBox.SetGamma(GetGamma());
    }

    private float GetGamma()
    {
        TrackBar tb = _page.GammaTrackBar;

        // Make sure 1.0 is really 1.0 exactly, not like eg. 1.001234
        if (tb.Value == tb.Maximum / 2)
        {
            return 1.0f;
        }

        int param = tb.Maximum - tb.Value;
        float ret = param * (1.0f / (tb.Maximum / 2.0f));

        // @ScreenshotDisplay(gamma math):
        // Trial-and-error flailing until we get something with a good range and that has 1.0 in the middle for
        // non-confusing UX. The last 4 values end up differing by a small enough amount that the image doesn't
        // change between them (not that I can see anyway). Whatever... I might come back to it later...
        ret = (ret * ret) / 1.18f;
        ret += 0.15f;

        return ret;
    }

    private void GammaTrackBar_MouseDown(object sender, MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Left)
        {
            ResetGammaSlider();
        }
    }

    private void GammaTrackBar_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ResetGammaSlider();
        }
    }

    private void ResetGammaSlider()
    {
        _page.GammaTrackBar.Value = 50;
        Config.ScreenshotGammaPercent = 50;
        _page.ScreenshotsPictureBox.SetGamma(1.0f);
    }

    private void SetCopiedMessageLabelText(string text, bool success)
    {
        if (!_constructed) return;
        if (CopiedMessageFadeoutTimer.Enabled) return;

        _page.CopiedMessageLabel.ForeColor = success ? Color.Green : Color.DarkRed;
        _page.CopiedMessageLabel.DarkModeForeColor = success ? DarkColors.SuccessGreenDark : DarkColors.Fen_CautionText;

        _page.CopiedMessageLabel.Text = text;
        _page.CopiedMessageLabel.CenterH(_page);
        _page.CopiedMessageLabel.Show();
        CopiedMessageFadeoutTimer.Start();
    }

    private void CopiedMessageFadeoutTimer_Tick(object sender, EventArgs e)
    {
        if (!_constructed) return;
        if (Disposing || _owner.AboutToClose) return;
        try
        {
            _page.CopiedMessageLabel.Hide();
            CopiedMessageFadeoutTimer.Stop();
        }
        catch
        {
            // Just in case it happens during dispose or whatever
        }
    }

    public void CopyImageToClipboard()
    {
        if (!_constructed) return;

        try
        {
            using Bitmap? bmp = _page.ScreenshotsPictureBox.GetSnapshot();
            if (bmp != null)
            {
                Clipboard.SetImage(bmp);
                SetCopiedMessageLabelText(LText.ScreenshotsTab.ImageCopied, success: true);
            }
        }
        catch
        {
            SetCopiedMessageLabelText(LText.ScreenshotsTab.ImageCopyFailed, success: false);
        }
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentScreenshotStream?.Dispose();
            CopiedMessageFadeoutTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
