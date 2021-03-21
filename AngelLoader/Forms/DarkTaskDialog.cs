using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    /*
    Notes:
    -Icon is at x:10, y:10, and size is 32x32
    -Min width is 350, and max width is the greater of 350 and the bottom panel width (which is all the bottom
     panel controls and their paddings combined)
    -Text is x: Icon.Right + 10, or in other words 52 (Icon.Left (10) + Icon.Width (32) + 10 == 52),
             y: Icon.Top + 8 (or 18) (at least it looks like that visually, we can check later)
    -Allow up to 3 buttons (yes, no, cancel)
    -Allow an optional checkbox
    */
    public sealed partial class DarkTaskDialog : DarkForm
    {
        private readonly Dictionary<Control, (Color ForeColor, Color BackColor)> _controlColors = new();

        [PublicAPI]
        public bool IsVerificationChecked => VerificationCheckBox.Checked;

        [PublicAPI]
        public enum Button
        {
            Yes,
            No,
            Cancel
        }

        [PublicAPI]
        public DarkTaskDialog(
            string message,
            string title,
            MessageBoxIcon icon = MessageBoxIcon.None,
            string? yesText = null,
            string? noText = null,
            string? cancelText = null,
            string? checkBoxText = null,
            bool? checkBoxChecked = null,
            Button defaultButton = Button.Cancel)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            #region Set fonts

            // Set these after InitializeComponent() in case that sets other fonts, but before anything else
            MessageLabel.Font = SystemFonts.MessageBoxFont;
            YesButton.Font = SystemFonts.MessageBoxFont;
            NoButton.Font = SystemFonts.MessageBoxFont;
            Cancel_Button.Font = SystemFonts.MessageBoxFont;
            VerificationCheckBox.Font = SystemFonts.MessageBoxFont;

            #endregion

            // All numbers are just matching the original Win32 task dialog as closely as possible. Don't worry
            // about them.

            int imageMarginX = icon != MessageBoxIcon.None ? 49 : 7;

            MessageLabel.Location = new Point(imageMarginX, MessageLabel.Location.Y);

            Text = title;
            MessageLabel.Text = message;

            if (yesText != null) YesButton.Text = yesText;
            if (noText != null) NoButton.Text = noText;
            if (cancelText != null) Cancel_Button.Text = cancelText;
            if (checkBoxText != null) VerificationCheckBox.Text = checkBoxText;

            // Have to use these bools, because if we check Visible it will always be false even though we might
            // just have set it to true, because we haven't shown ourselves yet so everything counts as not visible
            bool yesButtonVisible = yesText != null;
            bool noButtonVisible = noText != null;
            bool cancelButtonVisible = cancelText != null;
            bool checkBoxVisible = checkBoxText != null;

            if (!yesButtonVisible && !noButtonVisible && !cancelButtonVisible)
            {
                throw new ArgumentException("At least one button must have text specified!");
            }

            YesButton.Visible = yesButtonVisible;
            NoButton.Visible = noButtonVisible;
            Cancel_Button.Visible = cancelButtonVisible;
            VerificationCheckBox.Visible = checkBoxVisible;
            VerificationCheckBox.Checked = checkBoxChecked == true;

            CancelButton = cancelButtonVisible ? Cancel_Button : noButtonVisible ? NoButton : YesButton;

            static void ThrowForDefaultButton(Button button) => throw new ArgumentException("Default button not visible: " + button);

            switch (defaultButton)
            {
                case Button.Yes:
                    if (!yesButtonVisible) ThrowForDefaultButton(Button.Yes);
                    AcceptButton = YesButton;
                    break;
                case Button.No:
                    if (!noButtonVisible) ThrowForDefaultButton(Button.No);
                    AcceptButton = NoButton;
                    break;
                case Button.Cancel:
                default:
                    if (!cancelButtonVisible) ThrowForDefaultButton(Button.Cancel);
                    AcceptButton = Cancel_Button;
                    break;
            }

            if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

            int bottomBarContentWidth = ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP);
            if (checkBoxVisible)
            {
                bottomBarContentWidth += VerificationCheckBox.Margin.Horizontal + VerificationCheckBox.Width + 22;
            }
            int width = Math.Max(bottomBarContentWidth, 350);
            MessageLabel.MaximumSize = new Size((width - imageMarginX) - 7, MessageLabel.MaximumSize.Height);
            int height = MessageLabel.Top + MessageLabel.Height + 15 + BottomFLP.Height;

            if (icon != MessageBoxIcon.None && MessageLabel.Bottom <= IconPictureBox.Bottom + 10)
            {
                height += 8;
            }

            ClientSize = new Size(width, height);

            // +2 fudge factor because the label height and the font height are not exactly the same (height is
            // 1px less than font height in this case, but I don't know how consistent that is)
            if (MessageLabel.Height <= MessageLabel.Font.Height + 2)
            {
                MessageLabel.Location = new Point(
                    MessageLabel.Location.X,
                    (BottomFLP.Top / 2) - (MessageLabel.Height / 2)
                );
            }

            DialogResult = DialogResult.Cancel;

            if (yesButtonVisible) YesButton.Click += YesButton_Click;
            if (noButtonVisible) NoButton.Click += NoButton_Click;
            if (cancelButtonVisible) Cancel_Button.Click += CancelButton_Click;
            SetTheme(Config.VisualTheme);
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SetTheme(VisualTheme theme)
        {
            if (theme == VisualTheme.Dark)
            {
                ControlUtils.ChangeFormThemeMode(theme, this, _controlColors, x => x == BottomFLP || x == VerificationCheckBox);
                BottomFLP.BackColor = DarkColors.Fen_DarkBackground;
                VerificationCheckBox.DarkModeBackColor = DarkColors.Fen_DarkBackground;
            }
            else
            {
                VerificationCheckBox.BackColor = SystemColors.Control;
            }
        }
    }
}
