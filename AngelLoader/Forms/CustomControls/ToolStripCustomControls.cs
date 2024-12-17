using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

internal sealed class ToolStripCustom : ToolStrip, IDarkable
{
    private sealed class DarkModeToolStripColorTable : ProfessionalColorTable
    {
        public override Color ButtonSelectedHighlight => DarkColors.Fen_DGVColumnHeaderPressed;
        public override Color ButtonSelectedGradientBegin => DarkColors.Fen_DGVColumnHeaderPressed;
        public override Color ButtonSelectedGradientEnd => DarkColors.Fen_DGVColumnHeaderPressed;
        public override Color ButtonSelectedGradientMiddle => DarkColors.Fen_DGVColumnHeaderPressed;

        public override Color ButtonPressedHighlight => DarkColors.Fen_DGVColumnHeaderHighlight;
        public override Color ButtonPressedGradientBegin => DarkColors.Fen_DGVColumnHeaderHighlight;
        public override Color ButtonPressedGradientEnd => DarkColors.Fen_DGVColumnHeaderHighlight;
        public override Color ButtonPressedGradientMiddle => DarkColors.Fen_DGVColumnHeaderHighlight;

        public override Color ButtonCheckedHighlight => DarkColors.BlueSelection;
        public override Color ButtonCheckedGradientBegin => DarkColors.BlueSelection;
        public override Color ButtonCheckedGradientEnd => DarkColors.BlueSelection;
        public override Color ButtonCheckedGradientMiddle => DarkColors.BlueSelection;

        public override Color ButtonSelectedBorder => DarkColors.BlueHighlight;
        public override Color ButtonSelectedHighlightBorder => DarkColors.BlueHighlight;
        public override Color ButtonPressedBorder => DarkColors.BlueHighlight;
        public override Color ButtonPressedHighlightBorder => DarkColors.BlueHighlight;
        public override Color ButtonCheckedHighlightBorder => DarkColors.BlueHighlight;
    }

    // We CAN cache one instance of the color table at least
    private static DarkModeToolStripColorTable? _colorTable;
    private static DarkModeToolStripColorTable ColorTable => _colorTable ??= new DarkModeToolStripColorTable();

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
                BackColor = DarkColors.Fen_ControlBackground;
                // We can't cache the renderer because for some reason it only takes the first time if we do
                Renderer = new ToolStripProfessionalRenderer(ColorTable) { RoundedEdges = false };
            }
            else
            {
                BackColor = SystemColors.Control;
                RenderMode = ToolStripRenderMode.ManagerRenderMode;
            }

            Invalidate();
        }
    }

    private void TrySetToolTipMaxDelay()
    {
        // Perf - don't do reflection if it will be a no-op
        if (WinVersion.SupportsPersistentToolTips) return;

        try
        {
            PropertyInfo? toolTipProperty = typeof(ToolStrip).GetProperty("ToolTip",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (toolTipProperty != null)
            {
                ToolTip? toolTip = (ToolTip?)toolTipProperty.GetValue(this);
                toolTip?.SetMaxDelay();
            }
        }
        catch
        {
            // ignore
        }
    }

    public ToolStripCustom() => TrySetToolTipMaxDelay();

    public ToolStripCustom(params ToolStripItem[] items) : base(items) => TrySetToolTipMaxDelay();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // This whole checking for item count and last visible item and all is to fix the bug where sometimes
        // the last item will be mostly painted over. Finally figured it out.

        int firstItemX = 0;
        int firstItemMarginLeft = 0;

        if (Items.Count > 0)
        {
            firstItemX = Items[0].Bounds.X;
            firstItemMarginLeft = Items[0].Margin.Left;
        }

        // Hack in order to be able to have ManagerRenderMode, but also get rid of any garbage around the
        // edges that may be drawn. In particular, there's an ugly visual-styled vertical line at the right
        // side if you don't do this.
        // Take margin into account to allow drawing past the left side of the first item or the right of the
        // last
        var rectLeft = new Rectangle(0, 0, firstItemX - firstItemMarginLeft, Height);

        int lastItemIndex = -1;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Visible) lastItemIndex = i;
        }

        int lastItemX = 0;
        int lastItemWidth = 0;
        int lastItemMarginRight = 0;

        if (lastItemIndex > -1)
        {
            ToolStripItem last = Items[lastItemIndex];
            lastItemX = last.Bounds.X;
            lastItemWidth = last.Bounds.Width;
            lastItemMarginRight = last.Margin.Right;
        }

        int rect2Start = lastItemX + lastItemWidth + lastItemMarginRight;
        var rectRight = new Rectangle(rect2Start, 0, Width - rect2Start, Height);
        var rectBottom = new Rectangle(0, Height - 1, Width, 1);

        Brush brush = _darkModeEnabled ? DarkColors.Fen_ControlBackgroundBrush : SystemBrushes.Control;
        Pen pen = _darkModeEnabled ? DarkColors.Fen_ControlBackgroundPen : SystemPens.Control;

        e.Graphics.FillRectangle(brush, rectLeft);
        e.Graphics.FillRectangle(brush, rectRight);
        e.Graphics.FillRectangle(brush, rectBottom);
        e.Graphics.DrawLine(pen, Width - 2, Height - 2, Width - 1, Height - 2);
    }
}

/// <summary>
/// Tool strip menu item but with automatic ampersand escaping, because it's hell to make sure they all get escaped otherwise.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
public class ToolStripMenuItemCustom : ToolStripMenuItem
{
    /// <summary>
    /// If this component represents a game in some way, you can set its <see cref="GameSupport.GameIndex"/> here.
    /// </summary>
    [PublicAPI]
    public GameSupport.GameIndex GameIndex = GameSupport.GameIndex.Thief1;

    public ToolStripMenuItemCustom() { }

    /// <summary>Set text with escaped ampersands.</summary>
    /// <param name="text">The text to display on the menu item, with escaped ampersands.</param>
    // Call it with bare text because the constructor will set Text, which will jump back up to setting it
    // here (which will escape ampersands), then jump back down to setting it in the base class. OOP IS THE
    // LITERAL DEFINITION OF SPAGHETTI CODE.
    // But we do it anyway to try and reduce our own errors of forgetting to call the ampersand escaper every
    // time we set text one way or the other...
    public ToolStripMenuItemCustom(string text) : base(text) { }

    /// <summary>
    /// Sets the text and escapes ampersands.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public override string? Text
    {
        get => base.Text;
        set => base.Text = value?.EscapeAmpersands() ?? "";
    }
}

/// <summary>
/// Because the text will be displayed as "One &amp; Two" but will still be stored as "One &amp;&amp; Two"
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
public sealed class ToolStripMenuItemWithBackingText : ToolStripMenuItemCustom
{
    public string BackingText { get; }

    public ToolStripMenuItemWithBackingText(string text)
    {
        BackingText = text;
        Text = text;
    }
}

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
public sealed class ToolStripMenuItemWithBackingField<T>(T field) : ToolStripMenuItemCustom
{
    public readonly T Field = field;
}

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
public sealed class ToolStripButtonCustom : ToolStripButton, IDarkable
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
            BackColor = _darkModeEnabled ? DarkColors.Fen_ControlBackground : SystemColors.Control;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            // Use the mouseover BackColor when it's checked, for a more visible checked experience
            if (Checked) e.Graphics.FillRectangle(Brushes.LightSkyBlue, 0, 0, Width, Height);
        }
        base.OnPaint(e);
    }
}

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
public sealed class ToolStripArrowButton : ToolStripButton, IDarkable
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
            BackColor = _darkModeEnabled ? DarkColors.Fen_ControlBackground : SystemColors.Control;
        }
    }

    private Direction _arrowDirection;

    [Browsable(true)]
    [PublicAPI]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Direction ArrowDirection
    {
        get => _arrowDirection;
        set
        {
            _arrowDirection = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Images.PaintArrow7x4(
            g: e.Graphics,
            direction: _arrowDirection,
            area: ContentRectangle,
            controlEnabled: Enabled
        );
    }
}
