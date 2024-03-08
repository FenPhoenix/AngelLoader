using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_StatsPage : UserControl
{
    internal readonly DarkCheckBox[] _checkBoxes = new DarkCheckBox[Misc.CustomResourcesCount - 1];

    public Lazy_StatsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        StatsCheckBoxesPanel.SuspendLayout();

        StatsCheckBoxesPanel.TabIndex = 16;

        for (int i = 0, tabIndex = 2, y = 0;
             i < Misc.CustomResourcesCount - 1;
             i++, tabIndex++, y += 16)
        {
            var cb = new DarkCheckBox
            {
                AutoCheck = false,
                AutoSize = true,
                TabIndex = tabIndex,
                Location = new Point(0, y)
            };
            _checkBoxes[i] = cb;
            StatsCheckBoxesPanel.Controls.Add(cb);
        }

        StatsCheckBoxesPanel.ResumeLayout(false);
        StatsCheckBoxesPanel.PerformLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Images.DrawHorizDiv(e.Graphics, 6, 24, ClientSize.Width - 8);
    }
}
