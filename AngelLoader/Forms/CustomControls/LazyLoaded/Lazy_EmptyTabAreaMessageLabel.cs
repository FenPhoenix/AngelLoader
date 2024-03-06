using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_EmptyTabAreaMessageLabel : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    private WhichTabControl _which;

    private string _text = "";

    private DarkLabel Label = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            Label.DarkModeEnabled = value;
        }
    }

    public Lazy_EmptyTabAreaMessageLabel(MainForm owner) => _owner = owner;

    internal void SetWhich(WhichTabControl which) => _which = which;

    internal void SetText(string text)
    {
        if (_constructed)
        {
            Label.Text = text;
        }
        else
        {
            _text = text;
        }
    }

    private void Construct()
    {
        if (_constructed) return;

        var container =
            _which == WhichTabControl.Bottom
                ? _owner.LowerSplitContainer.Panel2
                : _owner.TopSplitContainer.Panel2;

        var collapseButton =
            _which == WhichTabControl.Bottom
                ? _owner.BottomFMTabsCollapseButton
                : _owner.TopFMTabsCollapseButton;

        Label = new DarkLabel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Size = new Size(
                container.Width - collapseButton.Width,
                container.Height),
            TextAlign = ContentAlignment.MiddleCenter,

            DarkModeEnabled = _darkModeEnabled
        };

        Label.PaintCustom += FMTabsEmptyMessageLabels_Paint;
        Label.MouseClick += _which == WhichTabControl.Bottom
            ? _owner.LowerFMTabsBar_MouseClick
            : _owner.TopFMTabsBar_MouseClick;

        container.Controls.Add(Label);

        _constructed = true;

        SetText(_text);
    }

    private void FMTabsEmptyMessageLabels_Paint(object sender, PaintEventArgs e)
    {
        DarkLabel label = Label;
        e.Graphics.DrawRectangle(
            _darkModeEnabled ? DarkColors.LighterBackgroundPen : SystemPens.ControlLight,
            new Rectangle(0, 0, label.ClientRectangle.Width - 1, label.ClientRectangle.Height - 1));
    }

    internal void Show(bool show)
    {
        if (show)
        {
            Trace.WriteLine(_which);
            Construct();
            Label.BringToFront();
            Label.Show();
        }
        else
        {
            if (_constructed) Label.Hide();
        }
    }
}
