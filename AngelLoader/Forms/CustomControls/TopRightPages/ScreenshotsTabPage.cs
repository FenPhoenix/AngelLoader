﻿// @ScreenshotDisplay: Make screenshot scale with resize

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
    private string CurrentScreenshotFileName = "";

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

    // @ScreenshotDisplay: Implement not loading images when hidden, and loading on show

    public override void UpdatePage()
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        Core.PopulateScreenshotFileNames(fm, ScreenshotFileNames);

        if (ScreenshotFileNames.Count == 0)
        {
            CurrentScreenshotFileName = "";
            _page.ScreenshotsPictureBox.Image = null;
            _page.ScreenshotsPictureBox.ImageLocation = "";
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.ScreenshotsPrevButton.Enabled = false;
            _page.ScreenshotsNextButton.Enabled = false;
            _page.NumberLabel.Text = "";
        }
        // @ScreenshotDisplay: Should we save the selected screenshot in the FM object?
        else
        {
            CurrentScreenshotFileName = ScreenshotFileNames[0];
            DisplayCurrentScreenshot();
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.ScreenshotsPrevButton.Enabled = ScreenshotFileNames.Count > 1;
            _page.ScreenshotsNextButton.Enabled = ScreenshotFileNames.Count > 1;
        }
    }

    private void DisplayCurrentScreenshot()
    {
        if (!_constructed) return;
        if (!_owner.StartupState && !Visible) return;

        if (!CurrentScreenshotFileName.IsEmpty() &&
            // @TDM_CASE when FM is TDM
            (_page.ScreenshotsPictureBox.ImageLocation?.EqualsI(CurrentScreenshotFileName) != true))
        {
            _page.ScreenshotsPictureBox.Load(CurrentScreenshotFileName);
            _page.NumberLabel.Text = (ScreenshotFileNames.IndexOf(CurrentScreenshotFileName) + 1).ToStrInv() + " / " +
                                     ScreenshotFileNames.Count.ToStrInv();
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) DisplayCurrentScreenshot();
    }

    #region Page

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

    #endregion

    #endregion
}
