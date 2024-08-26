using System.Collections.Generic;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public partial class ProgressBox_MultiItem : UserControl
{
    private readonly List<ProgressItem> _items = new();

    public ProgressBox_MultiItem()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
}
