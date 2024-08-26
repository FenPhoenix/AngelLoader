using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class ProgressBox_MultiItem : UserControl, IDarkable
{
    private readonly MainForm _owner;

    private ProgressItem[] _items = Array.Empty<ProgressItem>();

    public ProgressBox_MultiItem(MainForm owner)
    {
        _owner = owner;

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            Cancel_Button.DarkModeEnabled = _darkModeEnabled;

            (Color fore, Color back) =
                _darkModeEnabled
                    // Use a lighter background to make it easy to see we're supposed to be in front and modal
                    ? (fore: DarkColors.LightText, back: DarkColors.LightBackground)
                    : (fore: SystemColors.ControlText, back: SystemColors.Control);

            ForeColor = fore;
            BackColor = back;

            MainMessage1Label.ForeColor = fore;
            MainMessage1Label.BackColor = back;

            MainMessage2Label.ForeColor = fore;
            MainMessage2Label.BackColor = back;
        }
    }

    public void SetItemsCount(int count)
    {
        if (_items.Length == count)
        {
            foreach (ProgressItem item in _items)
            {
                item.SetText("");
                item.SetProgressPercent(0);
            }
        }
        else
        {
            foreach (ProgressItem item in _items)
            {
                Controls.Remove(item);
                item.Dispose();
            }
            _items = InitializedArray<ProgressItem>(count);
            foreach (ProgressItem item in _items)
            {
                Controls.Add(item);
            }
        }
    }

    public void SetItemText(int index, string text)
    {
        _items[index].SetText(text);
    }

    public void SetItemProgressPercent(int index, int percent)
    {
        _items[index].SetProgressPercent(percent);
    }

    private void SetCancelButtonText(string text)
    {
        Cancel_Button.Text = text;
        Cancel_Button.CenterH(this);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_darkModeEnabled && BorderStyle == BorderStyle.FixedSingle)
        {
            e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        }
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
