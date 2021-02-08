using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTabControl : TabControl, IDarkable
    {
        private Font _originalFont;

        public DarkTabControl() { }

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        private void SetUpTheme()
        {
            SetStyle(ControlStyles.UserPaint
                     | ControlStyles.AllPaintingInWmPaint,
                _darkModeEnabled);

            if (_darkModeEnabled)
            {
                _originalFont = (Font)Font.Clone();
            }
            else
            {
                if (_originalFont != null) Font = (Font)_originalFont.Clone();
            }

            Refresh();
        }

        // TODO: @DarkMode(DarkTabControl): Implement hot-tracked coloring
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_darkModeEnabled)
            {
                var g = e.Graphics;

                // Draw background
                using (var b = new SolidBrush(Config.Colors.Fen_DarkBackground))
                {
                    g.FillRectangle(b, ClientRectangle);
                }

                if (TabPages.Count > 0)
                {
                    var firstTabRect = GetTabRect(0);
                    var pageRect = new Rectangle(ClientRectangle.X,
                        ClientRectangle.Y + firstTabRect.Y + firstTabRect.Height + 1,
                        (ClientRectangle.Width - firstTabRect.X) - 1,
                        (ClientRectangle.Height - (firstTabRect.Y + firstTabRect.Height + 1)) - 2);

                    // Fill background (our actual area is slightly larger than gets filled by simply setting BackColor)
                    using (var b = new SolidBrush(Color.FromArgb(44, 44, 44)))
                    {
                        g.FillRectangle(b, pageRect);
                    }

                    // Draw page border
                    using (var p = new Pen(Config.Colors.LighterBackground))
                    {
                        g.DrawRectangle(p, pageRect);
                    }

                    // Paint tabs
                    for (int i = 0; i < TabPages.Count; i++)
                    {
                        TabPage tabPage = TabPages[i];
                        Rectangle tabRect = GetTabRect(i);

                        bool focused = SelectedTab == tabPage;

                        Color backColor = focused ? Config.Colors.GreySelection : Config.Colors.GreyBackground;

                        // Draw tab background
                        using (var b = new SolidBrush(backColor))
                        {
                            g.FillRectangle(b, Rectangle.Inflate(tabRect, 0, 1));
                        }

                        // Draw tab border
                        using (var p = new Pen(Config.Colors.LighterBackground))
                        {
                            g.DrawRectangle(p, Rectangle.Inflate(tabRect, 0, 1));
                        }

                        const TextFormatFlags textFormat =
                            TextFormatFlags.HorizontalCenter |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis |
                            TextFormatFlags.NoPrefix |
                            TextFormatFlags.NoClipping;

                        // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
                        TextRenderer.DrawText(g, tabPage.Text, Font, tabRect, Config.Colors.Fen_DarkForeground, textFormat);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
