using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkTabControl : TabControl, IDarkable
    {
        private Font? _originalFont;

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
                _originalFont ??= (Font)Font.Clone();
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
                g.FillRectangle(DarkColors.Fen_DarkBackgroundBrush, ClientRectangle);

                if (TabPages.Count > 0)
                {
                    var firstTabRect = GetTabRect(0);
                    var pageRect = new Rectangle(
                        ClientRectangle.X,
                        ClientRectangle.Y + firstTabRect.Y + firstTabRect.Height,
                        (ClientRectangle.Width - firstTabRect.X) - 1,
                        (ClientRectangle.Height - (firstTabRect.Y + firstTabRect.Height + 1)) - 1);

                    // Fill background (our actual area is slightly larger than gets filled by simply setting BackColor)
                    g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, pageRect);

                    // Draw page border
                    g.DrawRectangle(DarkColors.LighterBackgroundPen, pageRect);

                    // Paint tabs
                    for (int i = 0; i < TabPages.Count; i++)
                    {
                        TabPage tabPage = TabPages[i];
                        Rectangle tabRect = GetTabRect(i);

                        bool focused = SelectedTab == tabPage;

                        var backColorBrush = focused ? DarkColors.LightBackgroundBrush : DarkColors.Fen_DeselectedTabBackgroundBrush;
                        //var backColorBrush = focused ? DarkColors.Fen_ControlBackgroundBrush : DarkColors.Fen_DarkBackgroundBrush;

                        // Draw tab background
                        g.FillRectangle(backColorBrush, tabRect);

                        // Draw tab border
                        g.DrawRectangle(DarkColors.LighterBackgroundPen, tabRect);

                        // No TextAlign property, so leave constant
                        const TextFormatFlags textFormat =
                            TextFormatFlags.HorizontalCenter |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis |
                            TextFormatFlags.NoPrefix |
                            TextFormatFlags.NoClipping;

                        // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
                        Color textColor = SelectedTab == tabPage
                            //? Color.FromArgb(220,220,220)
                            ? DarkColors.LightText
                            : DarkColors.LightText;
                        TextRenderer.DrawText(g, tabPage.Text, Font, tabRect, textColor, textFormat);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
