using System;
using System.Collections.Generic;
using System.ComponentModel;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class ScreenshotsTabPage : Lazy_TabsBase
{
    private Lazy_ScreenshotsPage _page = null!;

    private readonly List<string> ScreenshotFileNames = new();
    private string StoredScreenshotFileName = "";

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

    // @ScreenshotDisplay: Implement not loading images when hidden, and loading on show

    public override void UpdatePage()
    {
        if (!_constructed) return;

        if (!_owner.StartupState && !Visible) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        Core.PopulateScreenshotFileNames(fm, ScreenshotFileNames);

        if (ScreenshotFileNames.Count == 0)
        {
            StoredScreenshotFileName = "";
            _page.ScreenshotsPictureBox.Image = null;
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.ScreenshotsPrevButton.Enabled = false;
            _page.ScreenshotsNextButton.Enabled = false;
        }
        // @ScreenshotDisplay: Should we save the selected screenshot in the FM object?
        // @TDM_CASE (when the FM is TDM)
        else if (!StoredScreenshotFileName.EqualsI(ScreenshotFileNames[0]))
        {
            // @ScreenshotDisplay: We can't just use the 0th element, we need to use the actual selected one
            StoredScreenshotFileName = ScreenshotFileNames[0];
            _page.ScreenshotsPictureBox.Load(StoredScreenshotFileName);
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.ScreenshotsPrevButton.Enabled = ScreenshotFileNames.Count > 1;
            _page.ScreenshotsNextButton.Enabled = ScreenshotFileNames.Count > 1;
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!_owner.StartupState && Visible)
        {
            UpdatePage();
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
