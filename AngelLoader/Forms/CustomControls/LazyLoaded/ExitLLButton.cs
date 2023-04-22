using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class ExitLLButton : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

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

    internal ExitLLButton(MainForm owner) => _owner = owner;

    internal void Localize()
    {
        if (!_constructed) return;
        Button.Text = LText.Global.Exit;
    }

    private void Construct()
    {
        if (_constructed) return;

        var container = _owner.BottomRightFLP;

        Button = new DarkButton
        {
            Tag = LoadType.Lazy,

            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(36, 36),
            TabIndex = 63,
            UseVisualStyleBackColor = true,

            DarkModeEnabled = _darkModeEnabled
        };

        Button.Click += _owner.Exit_Click;

        container.Controls.Add(Button);
        container.Controls.SetChildIndex(Button, 0);

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
}
