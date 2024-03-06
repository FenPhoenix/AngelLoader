using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkTrackBar : TrackBar, IDarkable
{
    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            BackColor = _darkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor;
        }
    }

    #region Double-click

    // TrackBars don't fire double-click events, so we roll our own

    private MouseButtons _lastClickButton = MouseButtons.None;
    private Point _lastMouseDownPoint = Point.Empty;
    private readonly System.Timers.Timer _doubleClickTimer = new() { AutoReset = false };

    public event MouseEventHandler? DoubleClickEndingOnMouseDown;

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (!_doubleClickTimer.Enabled)
        {
            _lastClickButton = e.Button;
            _lastMouseDownPoint = this.ClientCursorPos();
            _doubleClickTimer.Interval = SystemInformation.DoubleClickTime;
            _doubleClickTimer.Reset();
        }
        else
        {
            FireDoubleClickEvent(e, endingInDownVersion: false);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_doubleClickTimer.Enabled)
        {
            FireDoubleClickEvent(e, endingInDownVersion: true);
        }
    }

    private void FireDoubleClickEvent(MouseEventArgs e, bool endingInDownVersion)
    {
        Size doubleClickSize = SystemInformation.DoubleClickSize;
        Point cursorPos = this.ClientCursorPos();
        Rectangle rect = new(
            _lastMouseDownPoint.X - (doubleClickSize.Width / 2),
            _lastMouseDownPoint.Y - (doubleClickSize.Height / 2),
            doubleClickSize.Width,
            doubleClickSize.Height
        );
        if (rect.Contains(cursorPos) && e.Button == _lastClickButton)
        {
            if (endingInDownVersion)
            {
                DoubleClickEndingOnMouseDown?.Invoke(this, e);
            }
            else
            {
                OnMouseDoubleClick(e);
            }
        }
        _lastClickButton = MouseButtons.None;
        _lastMouseDownPoint = Point.Empty;
        _doubleClickTimer.Stop();
    }

    #endregion

    protected override void WndProc(ref Message m)
    {
        // Prevents white flicker when the main window redraws in certain cases (restore from minimize, game
        // window closing, etc.)
        if (m.Msg == Native.WM_ERASEBKGND) return;
        base.WndProc(ref m);
    }
}
