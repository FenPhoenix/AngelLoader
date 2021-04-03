using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkTextBox : TextBox, IDarkable
    {
        private bool _origValuesStored;
        private Color? _origForeColor;
        private Color? _origBackColor;
        private Padding? _origPadding;
        private BorderStyle? _origBorderStyle;

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

                var sbi_v = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
                int result_v = Native.GetScrollBarInfo(Handle, Native.OBJID_VSCROLL, ref sbi_v);

                bool vertScrollBarNeedsRepositioning =
                    result_v != 0 &&
                    (sbi_v.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE &&
                    (sbi_v.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) != Native.STATE_SYSTEM_UNAVAILABLE;

                var sbi_h = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
                int result_h = Native.GetScrollBarInfo(Handle, Native.OBJID_HSCROLL, ref sbi_h);

                bool horzScrollBarNeedsRepositioning =
                    result_h != 0 &&
                    (sbi_h.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE &&
                    (sbi_h.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) != Native.STATE_SYSTEM_UNAVAILABLE;

                Native.SCROLLINFO? si_v = null, si_h = null;
                if (vertScrollBarNeedsRepositioning) si_v = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
                if (horzScrollBarNeedsRepositioning) si_h = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_HORZ);

                if (_darkModeEnabled)
                {
                    if (!_origValuesStored)
                    {
                        _origForeColor ??= ForeColor;
                        _origBackColor ??= BackColor;
                        _origPadding ??= Padding;
                        _origBorderStyle ??= BorderStyle;
                        _origValuesStored = true;
                    }

                    BackColor = DarkColors.LightBackground;
                    ForeColor = DarkColors.LightText;
                    Padding = new Padding(2, 2, 2, 2);
                    BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    if (_origValuesStored)
                    {
                        BackColor = (Color)_origBackColor!;
                        ForeColor = (Color)_origForeColor!;
                        Padding = (Padding)_origPadding!;
                        BorderStyle = (BorderStyle)_origBorderStyle!;
                    }
                }

                if (vertScrollBarNeedsRepositioning && si_v != null)
                {
                    ControlUtils.RepositionScroll(Handle, (Native.SCROLLINFO)si_v, Native.SB_VERT);
                }
                if (horzScrollBarNeedsRepositioning && si_h != null)
                {
                    ControlUtils.RepositionScroll(Handle, (Native.SCROLLINFO)si_h, Native.SB_HORZ);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (!_darkModeEnabled)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case Native.WM_PAINT:
                    NativeHooks.SysColorOverride = NativeHooks.Override.Full;
                    base.WndProc(ref m);
                    NativeHooks.SysColorOverride = NativeHooks.Override.None;
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
