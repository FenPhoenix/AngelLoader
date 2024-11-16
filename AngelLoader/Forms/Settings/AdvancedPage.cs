using System.Collections.Generic;
using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;
public sealed partial class AdvancedPage : UserControl, Interfaces.ISettingsPage
{
    /*
    @MT_TASK: Do we want like a "reset to max" button by the custom threads box?

    @MT_TASK: How do we want to do the Custom section now?
    The one global option is not offering the granularity that the Auto option uses. Maybe we should just allow
    setting separate custom values for all the "scenarios": Install/uninstall; audio convert; scan.
    This is a little janky but it matches the Auto logic 1-to-1 and it IS accurate, even if kinda baroque.
    But note that if we got fancy with Auto and removed individual paths that aren't being accessed for that
    particular run of the scenario, then we wouldn't be matching Auto 1-to-1 anymore.
    */
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

    private void IOThreadingGroupBox_PaintCustom(object sender, PaintEventArgs e)
    {
        for (int i = 0; i < HorizDivYPositions.Count; i++)
        {
            int y = HorizDivYPositions[i];
            Images.DrawHorizDiv(e.Graphics, 8, y, IOThreadingGroupBox.Width - 8);
        }
    }
}
