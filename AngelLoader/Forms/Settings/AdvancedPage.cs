﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;
public sealed partial class AdvancedPage : UserControl, Interfaces.ISettingsPage
{
    // @MT_TASK(Advanced page): Put some kind of help message on here, like:
    //  "Increasing the thread level can result in slower I/O performance if the drive can't handle it"
    // @MT_TASK: Do we want to rename this tab to I/O threading?
    public AdvancedPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    internal readonly List<int> HorizDivYPositions = new();

    private void IOThreadingLevelGroupBox_PaintCustom(object sender, PaintEventArgs e)
    {
        for (int i = 0; i < HorizDivYPositions.Count; i++)
        {
            int y = HorizDivYPositions[i];
            Images.DrawHorizDiv(e.Graphics, 8, y, IOThreadingLevelGroupBox.Width - 8);
        }
    }

    private void IOThreadsResetButton_PaintCustom(object sender, PaintEventArgs e)
    {
        DarkButton button = (DarkButton)sender;
        Rectangle cr = button.ClientRectangle;
        Images.PaintBitmapButton(
            e: e,
            img: button.Enabled
                ? Images.Refresh
                : Images.Refresh_Disabled,
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }
}
