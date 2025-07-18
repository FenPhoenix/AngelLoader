﻿using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class ThiefBuddyPage : UserControlCustom, Interfaces.ISettingsPage
{
    public ThiefBuddyPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        TBHelpPictureBox.Image = Images.HelpSmall;

        ThiefBuddyOptionsGroupBox.PaintCustom += ThiefBuddyOptionsGroupBox_PaintCustom;
    }

    private void ThiefBuddyOptionsGroupBox_PaintCustom(object sender, PaintEventArgs e)
    {
        Images.DrawHorizDiv(e.Graphics, 16, (RunTBPanel.Top + RunThiefBuddyWhenPlayingFMsLabel.Top) - 20, ThiefBuddyOptionsGroupBox.Width - 17);
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;
}
