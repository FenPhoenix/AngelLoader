using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_FMsListZoomButtons : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    internal readonly ToolStripButtonCustom[] Buttons = new ToolStripButtonCustom[3];

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            RegenerateButtonImages();
        }
    }

    internal Lazy_FMsListZoomButtons(MainForm owner) => _owner = owner;

    private void RegenerateButtonImages()
    {
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].Image?.Dispose();
            Buttons[i].Image = Images.GetZoomImage(Buttons[i].ContentRectangle, (Zoom)i);
        }
    }

    private void Construct()
    {
        if (_constructed) return;

        int insertIndex = _owner.RefreshAreaToolStrip.Items.Count > 0 &&
                          _owner.RefreshAreaToolStrip.Items[0]
                              .EqualsIfNotNull(_owner.Lazy_UpdateNotification.Button)
            ? 1
            : 0;

        // Insert them in reverse order so we always insert at 0
        for (int i = 2; i >= 0; i--)
        {
            var button = new ToolStripButtonCustom
            {
                AutoSize = false,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Margin = new Padding(0),
                Size = new Size(25, 25)
            };
            Buttons[i] = button;
            _owner.RefreshAreaToolStrip.Items.Insert(insertIndex, button);
            button.Click += _owner.FMsListZoomButtons_Click;
        }

        RegenerateButtonImages();

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        Buttons[0].ToolTipText = LText.Global.ZoomIn;
        Buttons[1].ToolTipText = LText.Global.ZoomOut;
        Buttons[2].ToolTipText = LText.Global.ResetZoom;
    }

    internal void SetVisible(bool enabled)
    {
        if (!enabled && !_constructed) return;

        Construct();

        for (int i = 0; i < 3; i++)
        {
            Buttons[i].Visible = enabled;
        }
    }
}
