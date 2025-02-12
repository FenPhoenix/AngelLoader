using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class InstallUninstallFMLLButton : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    private bool _enabled;

    private bool _sayInstall;

    internal DarkButton Button = null!;

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

    internal InstallUninstallFMLLButton(MainForm owner) => _owner = owner;

    internal void Localize()
    {
        if (!_constructed) return;

        // Special-case this button to always be the width of the longer of the two localized strings for
        // "Install" and "Uninstall" so it doesn't resize when its text changes. (visual nicety)
        try
        {
            if (Button.Visible)
            {
                Button.SuspendDrawing();
            }

            (string Text, int Length)[] stringsAndLengths =
            {
                (LText.Global.UninstallFMs, TextRenderer.MeasureText(LText.Global.UninstallFMs, Button.Font).Width),
                (LText.Global.UninstallFM, TextRenderer.MeasureText(LText.Global.UninstallFM, Button.Font).Width),
                (LText.Global.InstallFMs, TextRenderer.MeasureText(LText.Global.InstallFMs, Button.Font).Width),
                (LText.Global.InstallFM, TextRenderer.MeasureText(LText.Global.InstallFM, Button.Font).Width),
                (LText.Global.InstallFM, TextRenderer.MeasureText(LText.Global.DeselectFM_DarkMod, Button.Font).Width),
                (LText.Global.InstallFM, TextRenderer.MeasureText(LText.Global.SelectFM_DarkMod, Button.Font).Width),
            };

            stringsAndLengths = stringsAndLengths.OrderByDescending(static x => x.Length).ToArray();

            string longestString = stringsAndLengths[0].Text;

            // Special case autosize text-set: can't be GrowAndShrink
            Button.SetTextAutoSize(longestString);

            SetSayInstall(_sayInstall);
        }
        finally
        {
            if (Button.Visible)
            {
                Button.ResumeDrawing();
            }
        }
    }

    internal void SetSayInstall(bool value)
    {
        if (_constructed)
        {
            bool multiSelected = _owner.FMsDGV.MultipleFMsSelected();
            Game game = _owner.GetMainSelectedFMOrNull()?.Game ?? Game.Null;
            Button.Text = ControlUtils.GetInstallStateText(game, value, multiSelected);
            Button.Invalidate();
        }
        _sayInstall = value;
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

    private void Construct()
    {
        if (_constructed) return;

        var container = _owner.BottomLeftFLP;

        Button = new DarkButton
        {
            Tag = LoadType.Lazy,

            AutoSize = true,
            Margin = new Padding(6, 3, 0, 3),
            Padding = new Padding(30, 0, 6, 0),
            MinimumSize = new Size(0, 36),
            TabIndex = 58,
            Enabled = _enabled,

            DarkModeEnabled = _darkModeEnabled,
        };

        Button.Click += _owner.Async_EventHandler_Main;
        Button.PaintCustom += InstallUninstallButton_Paint;

        container.Controls.Add(Button);
        container.Controls.SetChildIndex(Button, 2);

        _constructed = true;
    }

    internal void SetVisible(bool enabled)
    {
        if (enabled)
        {
            Construct();
            Button.Show();
            // We have to always localize here because that sets our max fixed width
            Localize();
        }
        else if (_constructed)
        {
            Button.Hide();
        }
    }

    private void InstallUninstallButton_Paint(object? sender, PaintEventArgs e)
    {
        bool enabled = Button.Enabled;

        Images.PaintBitmapButton(
            button: Button,
            e: e,
            img: _sayInstall
                ? enabled ? Images.Install_24 : Images.GetDisabledImage(Images.Install_24)
                : enabled ? Images.Uninstall_24 : Images.GetDisabledImage(Images.Uninstall_24),
            x: 10);
    }
}
