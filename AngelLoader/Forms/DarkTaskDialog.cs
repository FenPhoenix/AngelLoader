using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class DarkTaskDialog : DarkForm
    {
        #region Private fields

        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        #endregion

        #region Public fields

        [PublicAPI]
        public bool IsVerificationChecked => VerificationCheckBox.Checked;

        [PublicAPI]
        public enum Button
        {
            Yes,
            No,
            Cancel
        }

        #endregion

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
            // All numbers are just matching the original Win32 task dialog as closely as possible. Don't worry
            // about them.

#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            VerificationCheckBox.DarkModeBackColor = DarkColors.Fen_DarkBackground;

            #region Set fonts

            // Set these after InitializeComponent() in case that sets other fonts, but before anything else
            MessageLabel.Font = SystemFonts.MessageBoxFont;
            YesButton.Font = SystemFonts.MessageBoxFont;
            NoButton.Font = SystemFonts.MessageBoxFont;
            Cancel_Button.Font = SystemFonts.MessageBoxFont;
            VerificationCheckBox.Font = SystemFonts.MessageBoxFont;

            #endregion

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

            int imageMarginX = icon != MessageBoxIcon.None ? 49 : 7;

            #region Set control state

            Text = title;
            MessageLabel.Text = message;

            if (yesText != null) YesButton.Text = yesText;
            if (noText != null) NoButton.Text = noText;
            if (cancelText != null) Cancel_Button.Text = cancelText;
            if (checkBoxText != null) VerificationCheckBox.Text = checkBoxText;

            YesButton.Visible = yesButtonVisible;
            NoButton.Visible = noButtonVisible;
            Cancel_Button.Visible = cancelButtonVisible;
            VerificationCheckBox.Visible = checkBoxVisible;
            VerificationCheckBox.Checked = checkBoxChecked == true;

            if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

            MessageLabel.Location = new Point(imageMarginX, MessageLabel.Location.Y);

            #region Set default buttons

            CancelButton = cancelButtonVisible ? Cancel_Button : noButtonVisible ? NoButton : YesButton;

            static void ThrowForDefaultButton(Button button) => throw new ArgumentException("Default button not visible: " + button);

            NoButton.DialogResult = DialogResult.No;
            YesButton.DialogResult = DialogResult.Yes;
            Cancel_Button.DialogResult = DialogResult.Cancel;

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

            #endregion

            #endregion

            #region Size form

            int bottomBarContentWidth = ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP);
            if (checkBoxVisible)
            {
                bottomBarContentWidth += VerificationCheckBox.Margin.Horizontal + VerificationCheckBox.Width + 22;
            }
            int width = Math.Max(bottomBarContentWidth, 350);
            MessageLabel.MaximumSize = new Size((width - imageMarginX) - 7, MessageLabel.MaximumSize.Height);
            int height = MessageLabel.Top + MessageLabel.Height + 15 + BottomFLP.Height;

            // Add some padding between the bottom of the icon and the bottom panel
            if (icon != MessageBoxIcon.None && MessageLabel.Bottom <= IconPictureBox.Bottom + 10)
            {
                height += 8;
            }

            ClientSize = new Size(width, height);

            #endregion

            // If only one line of text, center the label vertically for a better look
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

            SetTheme(Config.VisualTheme);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // This doesn't take unless we put it all the way in Shown, annoyingly
            if (AcceptButton == YesButton)
            {
                YesButton.Focus();
            }
            else if (AcceptButton == NoButton)
            {
                NoButton.Focus();
            }
            else if (AcceptButton == Cancel_Button)
            {
                Cancel_Button.Focus();
            }
        }

        private void SetTheme(VisualTheme theme)
        {
            if (theme == VisualTheme.Dark)
            {
                ControlUtils.ChangeFormThemeMode(theme, this, _controlColors, x => x == BottomFLP);
                BottomFLP.BackColor = DarkColors.Fen_DarkBackground;
            }
            else
            {
                VerificationCheckBox.BackColor = SystemColors.Control;
            }
        }
    }
}
