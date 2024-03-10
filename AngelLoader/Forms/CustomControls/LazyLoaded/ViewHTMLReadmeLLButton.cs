using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class ViewHTMLReadmeLLButton : IDarkable
{
    private bool _constructed;
    private DarkButton Button = null!;

    private readonly MainForm _owner;

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

    internal ViewHTMLReadmeLLButton(MainForm owner) => _owner = owner;

    internal void Localize()
    {
        if (!_constructed) return;

        Button.Text = LText.ReadmeArea.ViewHTMLReadme;
        Button.CenterHV(_owner.ReadmeContainer);
    }

    internal bool Visible => _constructed && Button.Visible;

    internal void Hide()
    {
        if (_constructed) Button.Hide();
    }

    internal void Show()
    {
        if (!_constructed)
        {
            Button = new DarkButton
            {
                Tag = LoadType.Lazy,

                Anchor = AnchorStyles.None,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                // Button gets centered on localize so no location is specified here
                Padding = new Padding(6, 0, 6, 0),
                MinimumSize = new Size(0, 23),
                TabIndex = 49,
                Visible = false,

                DarkModeEnabled = _darkModeEnabled
            };

            Control container = _owner.ReadmeContainer;
            container.Controls.Add(Button);
            Button.Click += _owner.ViewHTMLReadmeButton_Click;
            Button.MouseLeave += _owner.ReadmeArea_MouseLeave;

            _constructed = true;

            Localize();
        }

        Button.Show();
    }
}
