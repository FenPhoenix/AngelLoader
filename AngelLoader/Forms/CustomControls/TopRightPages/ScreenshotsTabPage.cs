using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

public sealed class ScreenshotsTabPage : Lazy_TabsBase
{
    private Lazy_ScreenshotsPage _page = null!;

    #region Theme

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool DarkModeEnabled
    {
        get => base.DarkModeEnabled;
        set
        {
            if (DarkModeEnabled == value) return;
            base.DarkModeEnabled = value;
        }
    }

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_ScreenshotsPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.ScreenshotsPrevButton.Click += ScreenshotsPrevButton_Click;
            _page.ScreenshotsNextButton.Click += ScreenshotsNextButton_Click;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void Localize()
    {
        if (!_constructed) return;

        // @ScreenshotDisplay: Localize these
        _page.ScreenshotsPrevButton.Text = "Prev";
        _page.ScreenshotsNextButton.Text = "Next";
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.ScreenshotsPrevButton.Enabled = true;
            _page.ScreenshotsNextButton.Enabled = true;

            LoadScreenshots(fm);
        }
        else
        {
            _page.ScreenshotsPictureBox.Image = null;
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.ScreenshotsPrevButton.Enabled = false;
            _page.ScreenshotsNextButton.Enabled = false;
        }
    }

    // @ScreenshotDisplay: Move business logic out?
    private void LoadScreenshots(FanMission fm)
    {
        if (!GameIsKnownAndSupported(fm.Game))
        {
            _page.ScreenshotsPictureBox.Image = null;
        }
        else if (fm.Game == Game.TDM)
        {
            // @ScreenshotDisplay: Implement later
            _page.ScreenshotsPictureBox.Image = null;
        }
        else
        {
            // @ScreenshotDisplay: Performance... we need a custom FileInfo getter without the 8.3 stuff
            // And a custom comparer to avoid OrderBy()
            if (fm.Installed && FMIsReallyInstalled(fm, out string fmInstalledPath))
            {
                FileInfo[] files;
                try
                {
                    string ssPath = Path.Combine(fmInstalledPath, "screenshots");
                    files = new DirectoryInfo(ssPath).GetFiles("*");
                }
                catch
                {
                    _page.ScreenshotsPictureBox.Image = null;
                    return;
                }

                if (files.Length == 0)
                {
                    _page.ScreenshotsPictureBox.Image = null;
                    return;
                }

                files = files.OrderBy(static x => x.LastWriteTime).ToArray();
                _page.ScreenshotsPictureBox.Load(files[0].FullName);
            }
            else
            {
                _page.ScreenshotsPictureBox.Image = null;
            }
        }
    }

    #region Page

    private void ScreenshotsPrevButton_Click(object sender, EventArgs e)
    {
        // @ScreenshotDisplay: Implement
    }

    private void ScreenshotsNextButton_Click(object sender, EventArgs e)
    {
        // @ScreenshotDisplay: Implement
    }

    #endregion

    #endregion
}
