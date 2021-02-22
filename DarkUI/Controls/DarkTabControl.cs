using System.Drawing;
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
                    var pageRect = new Rectangle(
                        ClientRectangle.X,
                        ClientRectangle.Y + firstTabRect.Y + firstTabRect.Height,
                        (ClientRectangle.Width - firstTabRect.X) - 1,
                        (ClientRectangle.Height - (firstTabRect.Y + firstTabRect.Height + 1)) - 1);

                    // Fill background (our actual area is slightly larger than gets filled by simply setting BackColor)
                    using (var b = new SolidBrush(Config.Colors.Fen_ControlBackground))
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

                        Color backColor = focused ? Config.Colors.LightBackground : Config.Colors.Fen_DeselectedTabBackground;
                        //Color backColor = focused ? Config.Colors.Fen_ControlBackground : Config.Colors.Fen_DarkBackground;

                        // Draw tab background
                        using (var b = new SolidBrush(backColor))
                        {
                            g.FillRectangle(b, tabRect);
                        }

                        // Draw tab border
                        using (var p = new Pen(Config.Colors.LighterBackground))
                        {
                            g.DrawRectangle(p, tabRect);
                        }

                        const TextFormatFlags textFormat =
                            TextFormatFlags.HorizontalCenter |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis |
                            TextFormatFlags.NoPrefix |
                            TextFormatFlags.NoClipping;

                        // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
                        Color textColor = SelectedTab == tabPage
                            //? Color.FromArgb(220,220,220)
                            ? Config.Colors.Fen_DarkForeground
                            : Config.Colors.Fen_DarkForeground;
                        TextRenderer.DrawText(g, tabPage.Text, Font, tabRect, textColor, textFormat);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
