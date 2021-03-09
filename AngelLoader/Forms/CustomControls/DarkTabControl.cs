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
            SetStyle(
                // Double-buffering prevents flickering when mouse is moved over in dark mode
                ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint
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

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_darkModeEnabled)
            {
                var g = e.Graphics;

                if (Parent != null)
                {
                    // Fill background behind the control (shows up behind tabs)
                    using (var b = new SolidBrush(Parent.BackColor))
                    {
                        g.FillRectangle(b, ClientRectangle);
                    }
                }

                if (TabPages.Count > 0)
                {
                    var firstTabRect = GetTabRect(0);
                    var pageRect = new Rectangle(
                        ClientRectangle.X,
                        ClientRectangle.Y + firstTabRect.Y + firstTabRect.Height,
                        (ClientRectangle.Width - firstTabRect.X) - 1,
                        (ClientRectangle.Height - (firstTabRect.Y + firstTabRect.Height + 1)) - 1);

                    // Fill tab page background (shows up as a small border around the tab page)
                    // (our actual area is slightly larger than gets filled by simply setting BackColor)
                    g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, pageRect);

                    // Draw tab page border
                    g.DrawRectangle(DarkColors.LighterBackgroundPen, pageRect);

                    // Paint tabs
                    for (int i = 0; i < TabPages.Count; i++)
                    {
                        TabPage tabPage = TabPages[i];
                        Rectangle tabRect = GetTabRect(i);

                        bool focused = SelectedTab == tabPage;

                        if (focused)
                        {
                            tabRect = new Rectangle(
                                tabRect.X,
                                tabRect.Y - 2,
                                tabRect.Width,
                                tabRect.Height + 2
                            );
                        }

                        var backColorBrush = focused
                            ? DarkColors.LightBackgroundBrush
                            : tabRect.Contains(PointToClient(Cursor.Position))
                            ? DarkColors.Fen_HotTabBackgroundBrush
                            : DarkColors.Fen_DeselectedTabBackgroundBrush;

                        // Draw tab background
                        g.FillRectangle(backColorBrush, tabRect);

                        // Draw tab border
                        g.DrawRectangle(DarkColors.LighterBackgroundPen, tabRect);

                        bool thisTabHasImage = ImageList?.Images?.Empty == false &&
                                               tabPage.ImageIndex > -1;

                        #region Image

                        // Don't try to be clever and complicated and check for missing indexes etc.
                        // That would be a bug as far as I'm concerned, so just let it crash in that case.

                        if (thisTabHasImage)
                        {
                            int textWidth = TextRenderer.MeasureText(
                                g,
                                tabPage.Text,
                                Font
                            ).Width;

                            Image image = ImageList!.Images![tabPage.ImageIndex]!;

                            int leftMargin = tabRect.Width - textWidth;

                            Point imgPoint = new Point(
                                tabRect.Left + 1 + ((leftMargin / 2) - (image.Width / 2)),
                                4
                            );
                            g.DrawImage(image, imgPoint.X, imgPoint.Y);
                        }

                        #endregion

                        TextFormatFlags textHorzAlign = thisTabHasImage
                            ? TextFormatFlags.Right
                            : TextFormatFlags.HorizontalCenter;

                        // No TextAlign property, so leave constant
                        TextFormatFlags textFormat =
                            textHorzAlign |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis |
                            TextFormatFlags.NoPrefix |
                            TextFormatFlags.NoClipping;

                        var textRect =
                            thisTabHasImage
                                ? new Rectangle(
                                    tabRect.X - 2,
                                    tabRect.Y + 1,
                                    tabRect.Width,
                                    tabRect.Height - 1
                                )
                                : tabRect;

                        Color textColor = SelectedTab == tabPage
                            //? Color.FromArgb(220,220,220)
                            ? DarkColors.LightText
                            : DarkColors.LightText;

                        TextRenderer.DrawText(g, tabPage.Text, Font, textRect, textColor, textFormat);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
