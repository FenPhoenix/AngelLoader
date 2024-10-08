using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;
public sealed partial class AdvancedPage : UserControl, Interfaces.ISettingsPage
{
    // @MT_TASK: In addition to "Auto", do we want like a "Max" option, or even just a "Max" button by the threads field?
    // @MT_TASK: Should we display on the UI the number of threads Auto has detected?
    // @MT_TASK: Make the threading levels be displayed better - key-values tab aligned, drive type images, etc?
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
}
