using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AngelLoader.Forms.CustomControls;

public sealed class DGV_ProgressItem : DataGridView, IDarkable
{
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

            if (_darkModeEnabled)
            {
                BackgroundColor = DarkColors.Fen_DarkBackground;
                RowsDefaultCellStyle.ForeColor = DarkColors.Fen_DarkForeground;
                RowsDefaultCellStyle.BackColor = DarkColors.Fen_DarkBackground;
            }
            else
            {
                BackgroundColor = SystemColors.Window;
                RowsDefaultCellStyle.ForeColor = SystemColors.ControlText;
                RowsDefaultCellStyle.BackColor = SystemColors.Window;
            }
        }
    }

    public sealed class ProgressItemData
    {
        public int Handle;
        public string Text;
        public int Percent;

        public ProgressItemData(string text, int percent, int handle)
        {
            Text = text;
            Percent = percent;
            Handle = handle;
        }
    }

    public readonly List<ProgressItemData> ProgressItems = new();

    public DGV_ProgressItem()
    {
        DoubleBuffered = true;
        BackgroundColor = SystemColors.Window;
    }

    protected override void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);

        foreach (DataGridViewRow row in Rows)
        {
            row.Selected = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.Handled = true;
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_darkModeEnabled) return;

        if (BorderStyle == BorderStyle.FixedSingle)
        {
            e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        }

        if (VerticalScrollBar.Visible && HorizontalScrollBar.Visible)
        {
            int vertScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            int horzScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
            e.Graphics.FillRectangle(DarkColors.DarkBackgroundBrush,
                VerticalScrollBar.Left,
                HorizontalScrollBar.Top,
                vertScrollBarWidth,
                horzScrollBarHeight);
        }
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (e.Graphics == null) return;
        if (e.RowIndex <= -1) return;

        Brush bgBrush = _darkModeEnabled ? DarkColors.DarkBackgroundBrush : SystemBrushes.Window;
        e.Graphics.FillRectangle(bgBrush, e.CellBounds);

        if (ProgressItems.Count == 0 || e.RowIndex < ProgressItems.Count)
        {
            ProgressItemData item = ProgressItems[e.RowIndex];
            e.Graphics.FillRectangle(
                Brushes.Green,
                e.CellBounds.Left,
                e.CellBounds.Top,
                GetValueFromPercent_Int(item.Percent, e.CellBounds.Width),
                e.CellBounds.Height);
        }

        if (_darkModeEnabled)
        {
            e.CellStyle.ForeColor = DarkColors.Fen_DarkForeground;
        }

        e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

        Pen borderPen = _darkModeEnabled
            ? DarkColors.Fen_DGVCellBordersPen
            : SystemPens.ControlDark;
        e.Graphics.DrawRectangle(borderPen, e.CellBounds);

        e.Handled = true;
    }

    protected override void OnCellValueNeeded(DataGridViewCellValueEventArgs e)
    {
        base.OnCellValueNeeded(e);

        if (ProgressItems.Count == 0) return;

        if (e.RowIndex > ProgressItems.Count - 1)
        {
            e.Value = "";
            return;
        }

        ProgressItemData item = ProgressItems[e.RowIndex];

        e.Value = item.Text.IsEmpty() ? "" : item.Text + ", " + item.Percent.ToStrCur();
    }
}
