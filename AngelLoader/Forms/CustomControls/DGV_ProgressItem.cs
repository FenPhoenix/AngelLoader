using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;

public sealed class DGV_ProgressItem : DataGridView, IDarkable
{
    private Bitmap? _progressBitmap_Light;
    private Bitmap? _progressBitmap_Dark;

    private Bitmap ProgressBitmap => _darkModeEnabled
        ? _progressBitmap_Dark ??= CreateProgressBitmap()
        : _progressBitmap_Light ??= CreateProgressBitmap();

    private Bitmap CreateProgressBitmap()
    {
        using Bitmap sectionBmp = new(_segmentLength, _barHeight, PixelFormat.Format32bppPArgb);

        Bitmap ret = new(_barLength + (_segmentLength * 2), _barHeight, PixelFormat.Format32bppPArgb);

        using Graphics sectionBmpG = Graphics.FromImage(sectionBmp);
        using Graphics progressBmpG = Graphics.FromImage(ret);

        sectionBmpG.SmoothingMode = SmoothingMode.HighQuality;
        progressBmpG.SmoothingMode = SmoothingMode.HighQuality;

        PointF[] points =
        {
            new(0, 0),
            new(13, 0),
            new(26, 13),
            new(13, 13),
            new(0, 0),
        };

        sectionBmpG.FillPolygon(GetItemProgressBarBrush(), points);

        for (int i = 0; i < _barLength + (_segmentLength * 2); i += _segmentLength)
        {
            progressBmpG.DrawImageUnscaled(sectionBmp, i, 0);
        }

        return ret;
    }

    private const int _barLength = 380;
    private const int _barHeight = 13;
    private const int _segmentLength = 26;

    private readonly Timer IndeterminateProgressAnimTimer;

    private uint _indeterminateProgressBarsRefCount;
    public uint IndeterminateProgressBarsRefCount
    {
        get => _indeterminateProgressBarsRefCount;
        set
        {
            if (_indeterminateProgressBarsRefCount == 0 && value > 0)
            {
                IndeterminateProgressAnimTimer.Enabled = true;
            }
            else if (_indeterminateProgressBarsRefCount > 0 && value == 0)
            {
                IndeterminateProgressAnimTimer.Enabled = false;
            }

            _indeterminateProgressBarsRefCount = value;
        }
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

            if (_darkModeEnabled)
            {
                BackgroundColor = DarkColors.DarkBackground;
                RowsDefaultCellStyle.ForeColor = DarkColors.Fen_DarkForeground;
                RowsDefaultCellStyle.BackColor = DarkColors.DarkBackground;
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
        public string Line1;
        public string Line2;
        public int Percent;
        public ProgressType ProgressType;

        internal int IndeterminateAnimPosition;

        public ProgressItemData(string line1, string line2, int percent, ProgressType progressType)
        {
            Line1 = line1;
            Line2 = line2;
            Percent = percent;
            ProgressType = progressType;
        }
    }

    public readonly List<ProgressItemData> ProgressItems = new();

    public DGV_ProgressItem()
    {
        DoubleBuffered = true;
        RowTemplate.Height = (DefaultCellStyle.Font.Height + 4) * 3;

        IndeterminateProgressAnimTimer = new Timer();
        IndeterminateProgressAnimTimer.Interval = 16;
        IndeterminateProgressAnimTimer.Tick += IndeterminateProgressAnimTimer_Tick;
    }

    private void UpdateIndeterminateProgress()
    {
        if (_indeterminateProgressBarsRefCount == 0) return;

        try
        {
            int firstDisplayedIndex = FirstDisplayedScrollingRowIndex;
            int displayedRowCount = DisplayedRowCount(includePartialRow: true);
            int lastDisplayedIndex = firstDisplayedIndex + displayedRowCount;

            int rowCount = RowCount;
            for (int i = firstDisplayedIndex; i < lastDisplayedIndex; i++)
            {
                if (i > ProgressItems.Count - 1 || i > rowCount - 1) continue;
                if (ProgressItems[i].ProgressType == ProgressType.Indeterminate)
                {
                    InvalidateRow(i);
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    private void IndeterminateProgressAnimTimer_Tick(object sender, EventArgs e)
    {
        // System.Windows.Forms.Timer-and-Invoke() rather than System.Timers.Timer-and-sync-object, because the
        // latter causes the animation to freeze up in some cases. Don't know why exactly but whatever...
        Invoke(UpdateIndeterminateProgress);
    }

    protected override void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);

        foreach (DataGridViewRow row in Rows)
        {
            row.Selected = false;
        }
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

    private Brush GetItemBackgroundBrush() => _darkModeEnabled ? DarkColors.DarkBackgroundBrush : SystemBrushes.Window;

    private Brush? _lightModeProgressGreenBrush;
    private Brush GetItemProgressBarBrush()
    {
        if (_darkModeEnabled)
        {
            return DarkColors.BlueHighlightBrush;
        }
        else
        {
            if (_lightModeProgressGreenBrush == null)
            {
                try
                {
                    /*
                    Of course, you can get use GetThemeColor() to get the progress bar color, and of course, it's
                    slightly off. Just like that toolbar gray. What's the point of exposing the theme colors if
                    they're always bloody wrong? Who knows. So do a completely stupid thing here to get the color
                    that the control displays in the real actual world, rather than the pretend fantasy world
                    that the officially returned colors are living in.
                    */
                    using ProgressBar pb = new();
                    pb.Size = new Size(32, 32);
                    pb.Value = 100;
                    using Bitmap bmp = new(32, 32, PixelFormat.Format32bppPArgb);
                    pb.DrawToBitmap(bmp, pb.Bounds);
                    _lightModeProgressGreenBrush = new SolidBrush(bmp.GetPixel(16, 16));
                }
                catch
                {
                    // If we fail, just use the manually grabbed progress bar color on Windows 11, which is what
                    // we're expecting to get from the above.
                    _lightModeProgressGreenBrush = new SolidBrush(Color.FromArgb(6, 176, 37));
                }
            }

            return _lightModeProgressGreenBrush;
        }
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (e.Graphics == null) return;
        if (e.RowIndex <= -1) return;

        Brush bgBrush = GetItemBackgroundBrush();
        Pen borderPen;
        if (_darkModeEnabled)
        {
            borderPen = DarkColors.Fen_DGVCellBordersPen;
            e.CellStyle.ForeColor = DarkColors.Fen_DarkForeground;
        }
        else
        {
            borderPen = SystemPens.ControlDark;
        }

        e.Graphics.FillRectangle(bgBrush, e.CellBounds);

        if (ProgressItems.Count == 0 || e.RowIndex < ProgressItems.Count)
        {
            int fontHeight = DefaultCellStyle.Font.Height + 20;

            ProgressItemData item = ProgressItems[e.RowIndex];
            if (item.ProgressType == ProgressType.Indeterminate)
            {
                DrawProgressBar(item, fontHeight, ProgressType.Indeterminate);
            }
            else if (item.Percent > 0)
            {
                DrawProgressBar(item, fontHeight, ProgressType.Determinate);
            }

            // Draw the second line manually because linebreaks are ignored by the standard text cell
            TextRenderer.DrawText(
                e.Graphics,
                item.Line2,
                e.CellStyle.Font,
                new Point(e.CellBounds.Left + 2, e.CellBounds.Top + e.CellStyle.Font.Height + 4),
                e.CellStyle.ForeColor);
        }

        e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

        e.Graphics.DrawRectangle(borderPen, e.CellBounds with { Width = e.CellBounds.Width - 1 });

        e.Handled = true;

        return;

        void DrawProgressBar(ProgressItemData item, int fontHeight, ProgressType progressType)
        {
            if (progressType == ProgressType.Determinate)
            {
                e.Graphics.FillRectangle(
                    GetItemProgressBarBrush(),
                    e.CellBounds.Left + 4,
                    e.CellBounds.Top + fontHeight,
                    GetValueFromPercent_Int(item.Percent, e.CellBounds.Width - 8),
                    e.CellBounds.Height - (fontHeight + 5));
            }
            else
            {
                if (item.IndeterminateAnimPosition >= 0)
                {
                    item.IndeterminateAnimPosition = -_segmentLength;
                }

                e.Graphics.DrawImageUnscaled(
                    ProgressBitmap,
                    e.CellBounds.Left + item.IndeterminateAnimPosition,
                    e.CellBounds.Top + fontHeight);

                e.Graphics.FillRectangle(
                    bgBrush,
                    e.CellBounds.Left,
                    e.CellBounds.Top,
                    4,
                    e.CellBounds.Height);

                e.Graphics.FillRectangle(
                    bgBrush,
                    e.CellBounds.Right - 4,
                    e.CellBounds.Top,
                    4,
                    e.CellBounds.Height);

                item.IndeterminateAnimPosition++;
            }
        }
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

        e.Value = item.Line1;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IndeterminateProgressAnimTimer.Dispose();
            _progressBitmap_Light?.Dispose();
            _progressBitmap_Dark?.Dispose();
        }
        base.Dispose(disposing);
    }
}
