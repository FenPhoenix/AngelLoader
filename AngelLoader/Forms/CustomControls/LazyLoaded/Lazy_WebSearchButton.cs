using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_WebSearchButton : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    private bool _enabled;

    private DarkButton Button = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            Button.DarkModeEnabled = value;
        }
    }

    internal Lazy_WebSearchButton(MainForm owner) => _owner = owner;

    internal void Localize()
    {
        if (!_constructed) return;
        Button.Text = LText.MainButtons.WebSearch;
    }

    private void Construct()
    {
        if (_constructed) return;

        var container = _owner.BottomLeftFLP;

        Button = new DarkButton
        {
            Tag = LoadType.Lazy,

            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(15, 3, 3, 3),
            MinimumSize = new Size(116, 36),
            Padding = new Padding(33, 0, 6, 0),
            TabIndex = 60,
            Enabled = _enabled,

            DarkModeEnabled = _darkModeEnabled
        };

        Button.PaintCustom += WebSearchButton_Paint;
        Button.Click += _owner.WebSearchButton_Click;

        container.Controls.Add(Button);
        container.Controls.SetChildIndex(Button, 3);

        _owner._bottomLeftAreaSeparatedItems[0] = Button;

        _constructed = true;

        Localize();
    }

    internal void SetVisible(bool visible)
    {
        if (visible)
        {
            Construct();
            Button.Show();
        }
        else
        {
            if (_constructed) Button.Hide();
        }
    }

    internal void SetEnabled(bool value)
    {
        if (_constructed)
        {
            Button.Enabled = value;
        }
        else
        {
            _enabled = value;
        }
    }

    private void WebSearchButton_Paint(object sender, PaintEventArgs e) => Images.PaintWebSearchButton(Button, e);
}
