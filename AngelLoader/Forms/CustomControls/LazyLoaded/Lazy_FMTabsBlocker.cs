using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_FMTabsBlocker : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    private WhichTabControl _which;

    private string _text = "";

    private DrawnPanel Panel = null!;
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

            Panel.DarkModeEnabled = value;
            Label.DarkModeEnabled = value;
        }
    }

    internal Lazy_FMTabsBlocker(MainForm owner) => _owner = owner;

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

        FMTabControlGroup group = _owner.GetFMTabControlGroup(_which);

        var container = group.Splitter.Panel2;

        Panel = new DrawnPanel
        {
            Location = Point.Empty,
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            Size = new Size(
                container.Width - group.CollapseButton.Width,
                container.Height),
            DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground,

            DarkModeEnabled = _darkModeEnabled,
        };

        Label = new DarkLabel
        {
            AutoSize = false,
            DarkModeBackColor = DarkColors.Fen_ControlBackground,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,

            DarkModeEnabled = _darkModeEnabled,
        };

        container.Controls.Add(Panel);
        Panel.BringToFront();
        Panel.Controls.Add(Label);

        _constructed = true;

        SetText(_text);
    }

    internal bool Visible
    {
        get => _constructed && Panel.Visible;
        set
        {
            if (value)
            {
                Construct();
                Panel.Show();
            }
            else
            {
                if (_constructed) Panel.Hide();
            }
        }
    }

    internal void SuspendDrawing()
    {
        if (_constructed) Panel.SuspendDrawing();
    }

    internal void ResumeDrawing()
    {
        if (_constructed) Panel.ResumeDrawing();
    }
}
