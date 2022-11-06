using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms
{
    public class DarkFormBase : Form
    {
        private bool _loading = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new Icon? Icon
        {
            get => base.Icon;
            set => base.Icon = value;
        }

        [Browsable(true)]
        [PublicAPI]
        [DefaultValue(false)]
        public new bool ShowInTaskbar
        {
            get => base.ShowInTaskbar;
            set => base.ShowInTaskbar = value;
        }

        public DarkFormBase()
        {
            base.Icon = AL_Icon.AngelLoader;
            base.ShowInTaskbar = false;

            Win32ThemeHooks.InstallHooks();
        }

        #region Theming

        private sealed class ControlOriginalColors
        {
            internal readonly Color ForeColor;
            internal readonly Color BackColor;

            internal ControlOriginalColors(Color foreColor, Color backColor)
            {
                ForeColor = foreColor;
                BackColor = backColor;
            }
        }

        private readonly List<KeyValuePair<Control, ControlOriginalColors?>> _controlColors = new();

        private static void FillControlColorList(
            Control control,
            List<KeyValuePair<Control, ControlOriginalColors?>>? controlColors,
            bool createControlHandles
#if !ReleasePublic && !NoAsserts
            , int stackCounter = 0
#endif
        )
        {
#if !ReleasePublic && !NoAsserts
            const int maxStackCount = 100;
#endif

            if (controlColors != null && control.Tag is not LoadType.Lazy)
            {
                ControlOriginalColors? origColors = control is IDarkable
                    ? null
                    : new ControlOriginalColors(control.ForeColor, control.BackColor);
                controlColors.Add(new KeyValuePair<Control, ControlOriginalColors?>(control, origColors));
            }

            if (createControlHandles && !control.IsHandleCreated)
            {
                IntPtr dummy = control.Handle;
            }

#if !ReleasePublic && !NoAsserts
            stackCounter++;

            AssertR(
                stackCounter <= maxStackCount,
                nameof(FillControlColorList) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");
#endif

            // Our custom tab control is a special case in that we have the ability to show/hide tabs, which is
            // implemented by actually adding and removing the tab pages from the control and keeping them in a
            // backing list (that's the only way to do it). So we can run into problems where if a tab page is
            // not part of the control (because it's hidden), it will not be hit by this method and therefore
            // will never be themed correctly. So handle custom tab controls separately and go through their
            // backing lists rather than their Controls collection.
            if (control is DarkTabControl dtc)
            {
                var backingPages = dtc.BackingTabPages;
                for (int i = 0; i < backingPages.Length; i++)
                {
                    FillControlColorList(backingPages[i], controlColors, createControlHandles
#if !ReleasePublic && !NoAsserts
                    , stackCounter
#endif
                    );
                }
            }
            else
            {
                for (int i = 0; i < control.Controls.Count; i++)
                {
                    FillControlColorList(control.Controls[i], controlColors, createControlHandles
#if !ReleasePublic && !NoAsserts
                    , stackCounter
#endif
                    );
                }
            }
        }

        internal static void CreateAllControlsHandles(Control control) => FillControlColorList(control, null, createControlHandles: true);

        internal void SetThemeBase(
            VisualTheme theme,
            Func<Component, bool>? excludePredicate = null,
            bool createControlHandles = false,
            int capacity = -1)
        {
            bool darkMode = theme == VisualTheme.Dark;

            Images.DarkModeEnabled = darkMode;

            // @DarkModeNote(FillControlColorList): Controls might change their colors after construct
            // Remember to handle this if new controls are added that this applies to.
            if (_controlColors.Count == 0)
            {
                if (capacity >= 0) _controlColors.Capacity = capacity;
                FillControlColorList(this, (List<KeyValuePair<Control, ControlOriginalColors?>>?)_controlColors, createControlHandles);
            }

            foreach (var item in _controlColors)
            {
                Control control = item.Key;

                // Separate if because a control could be IDarkable AND be a ToolStrip
                if (control is ToolStrip ts)
                {
                    foreach (ToolStripItem tsItem in ts.Items)
                    {
                        if (tsItem is IDarkable darkableTSItem && (excludePredicate == null || !excludePredicate(tsItem)))
                        {
                            darkableTSItem.DarkModeEnabled = darkMode;
                        }
                    }
                }

                // We might want to exclude a ToolStrip but not its subcomponents, so we put this check after the
                // ToolStrip component check
                if (excludePredicate?.Invoke(control) == true) continue;

                if (control is IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = darkMode;
                }
                else
                {
                    (control.ForeColor, control.BackColor) =
                        darkMode
                            ? (DarkColors.LightText, DarkColors.Fen_ControlBackground)
                            : (item.Value!.ForeColor, item.Value!.BackColor);
                }
            }
        }

        #endregion

        #region Event handling

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _loading = false;

            if (!Config.DarkMode) return;

            Refresh();
            // Explicitly refresh non-client area - otherwise on Win7 the non-client area doesn't refresh and we
            // end up with blacked-out title bar and borders etc.
            Native.SendMessage(Handle, Native.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void WndProc(ref Message m)
        {
            // Cover up the flash of bright/half-drawn controls on startup when in dark mode
            if (_loading &&
                Config.DarkMode &&
                IsHandleCreated &&
                (m.Msg
                    is Native.WM_PAINT
                    or Native.WM_SIZE
                    or Native.WM_MOVE
                    or Native.WM_WINDOWPOSCHANGED
                    or Native.WM_ERASEBKGND
                ))
            {
                using (var gc = new Native.GraphicsContext(Handle))
                {
                    gc.G.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, new Rectangle(0, 0, Width, Height));
                }

                if (m.Msg != Native.WM_PAINT)
                {
                    base.WndProc(ref m);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion
    }
}
