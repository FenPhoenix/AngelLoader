/*
@ScreenshotDisplay: Options for watching screenshot folders and reloading from disk automatically

-Have one FileSystemWatcher and change its watch path every FM load. This will add time (up to 20-30ms iirc?)
 to FM selection. It will also need to be turned off any time the FM installed directory is modified in some way
 (install/uninstall/audio convert/etc.) to prevent a tsunami of events (perf).

-Have one FileSystemWatcher per game, and watch the FM install base dir (and global screenshots dir for TDM).
 Again, it would need to be disabled during FM dir modifications (to ANY fm subdir!). This would prevent having
 to set the watcher path for every FM selection change. The event handlers give us the affected filename, and we
 can filter to just image types for perf.

We could also have the watchers be wrapped in a lazy-loaded class that only loads them up when the screenshots
tab shows for the first time per game.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class ScreenshotsTabPage : Lazy_TabsBase
{
    private Lazy_ScreenshotsPage _page = null!;

    /*
    Images loaded from files keep the file stream alive for their entire lifetime, insanely. This means the file
    is "in use" and will cause delete attempts (like FM uninstallation) to fail. So we need to use this workaround
    of loading the file into a memory stream first, so it's only the memory stream being kept alive. This does
    mean we carry around the full file bytes in memory as well as the displayed image, but since we're only
    displaying one at a time and they'll probably be a few megs at most, it's not a big deal.
    */
    private sealed class MemoryImage : IDisposable
    {
        private readonly MemoryStream _memoryStream;
        public readonly Image Img;
        public string Path { get; private set; }

        public MemoryImage(string path)
        {
            Path = path;
            byte[] bytes = File.ReadAllBytes(path);
            _memoryStream = new MemoryStream(bytes);
            Img = Image.FromStream(_memoryStream);
        }

        public void Dispose()
        {
            Path = "";
            Img.Dispose();
            _memoryStream.Dispose();
        }
    }

    private readonly List<string> ScreenshotFileNames = new();
    private string CurrentScreenshotFileName = "";
    private MemoryImage? _currentScreenshotStream;
    private readonly Timer CopiedMessageFadeoutTimer = new();

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

            _page.PrevButton.Click += ScreenshotsPrevButton_Click;
            _page.NextButton.Click += ScreenshotsNextButton_Click;
            _page.OpenScreenshotsFolderButton.Click += OpenScreenshotsFolderButton_Click;
            _page.GammaTrackBar.Scroll += GammaTrackBar_Scroll;
            _page.GammaTrackBar.MouseDown += GammaTrackBar_MouseDown;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        UpdatePageInternal(forceUpdate: false);
    }

    public override void Localize()
    {
        if (!_constructed) return;
        _page.GammaLabel.Text = LText.ScreenshotsTab.Gamma;
        _owner.MainToolTip.SetToolTip(_page.GammaTrackBar, LText.ScreenshotsTab.ResetGammaToolTip);
        _owner.MainToolTip.SetToolTip(_page.ScreenshotsPictureBox, LText.ScreenshotsTab.CopyScreenshotToolTip);
    }

    public void RefreshScreenshots()
    {
        if (!_constructed) return;
        UpdatePageInternal(forceUpdate: true);
    }

    #endregion

    #region Page

    private void UpdatePageInternal(bool forceUpdate)
    {
        FanMission? fm = _owner.GetMainSelectedFMOrNull();
        if (fm != null && fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
        {
            Screenshots.GetScreenshotWatcher(gameIndex).Construct();
        }

        Screenshots.PopulateScreenshotFileNames(fm, ScreenshotFileNames);

        // @ScreenshotDisplay: Should we hide everything and just put a label "No screenshots"?
        if (ScreenshotFileNames.Count == 0)
        {
            CurrentScreenshotFileName = "";
            ClearCurrentScreenshot();
            _page.GammaLabel.Enabled = false;
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.GammaTrackBar.Enabled = false;
            _page.OpenScreenshotsFolderButton.Enabled = false;
            _page.PrevButton.Enabled = false;
            _page.NextButton.Enabled = false;
            SetNumberLabelText("");
        }
        // @ScreenshotDisplay: Should we save the selected screenshot in the FM object?
        else
        {
            CurrentScreenshotFileName = ScreenshotFileNames[0];
            DisplayCurrentScreenshot(forceUpdate);
            _page.GammaLabel.Enabled = true;
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.GammaTrackBar.Enabled = true;
            _page.OpenScreenshotsFolderButton.Enabled = true;
            _page.PrevButton.Enabled = ScreenshotFileNames.Count > 1;
            _page.NextButton.Enabled = ScreenshotFileNames.Count > 1;
        }
    }

    // Manual right-align to avoid needing a FlowLayoutPanel
    private void SetNumberLabelText(string text)
    {
        _page.NumberLabel.Text = text;
        SetNumberLabelPosition();
    }

    private void SetNumberLabelPosition()
    {
        _page.NumberLabel.Location = _page.NumberLabel.Location with
        {
            X = (_page.ClientSize.Width - 8) - _page.NumberLabel.Width
        };
    }

    private void ClearCurrentScreenshot()
    {
        _page.ScreenshotsPictureBox.SetImage(null);
        _currentScreenshotStream?.Dispose();
    }

    /*
    The standard behavior for lazy loaded tabs is that they don't update until loaded, after which they always
    update. However, loading an image could take a significant amount of time, and we don't want users to pay the
    performance cost if they're not actually able to see the image. So we only update when visible (or when we
    become visible).
    */
    private void DisplayCurrentScreenshot(bool forceUpdate = false)
    {
        if (!_constructed) return;
        // @ScreenshotDisplay: I think the unconditional-if-force-update check is not quite correct
        // That would probably make it update even if the tab is hidden? Maybe that's fine?
        if (!forceUpdate && !_owner.StartupState && !Visible) return;

        if (forceUpdate ||
            (!CurrentScreenshotFileName.IsEmpty() &&
            // @TDM_CASE when FM is TDM
            _currentScreenshotStream?.Path.EqualsI(CurrentScreenshotFileName) != true))
        {
            try
            {
                _currentScreenshotStream?.Dispose();
                _currentScreenshotStream = new MemoryImage(CurrentScreenshotFileName);
                _page.ScreenshotsPictureBox.SetImage(_currentScreenshotStream.Img, GetGamma());
            }
            catch
            {
                ClearCurrentScreenshot();
                _page.ScreenshotsPictureBox.SetErrorImage();
            }
            finally
            {
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
        CopyImageToClipboard();
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
        float ret = (tb.Maximum - tb.Value) * (1.0f / (tb.Maximum / 2.0f));
        ret = (float)Math.Round(ret, 2, MidpointRounding.AwayFromZero);
        return ret;
    }

    private void GammaTrackBar_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button is MouseButtons.Middle or MouseButtons.Right)
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
