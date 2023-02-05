﻿using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;
public sealed partial class ThiefBuddyPage : UserControl, Interfaces.ISettingsPage
{
    public ThiefBuddyPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        TBHelpPictureBox.Image = Images.HelpSmall;
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;
}
