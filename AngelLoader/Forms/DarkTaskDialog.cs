using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    public partial class DarkTaskDialog : Form
    {
        public bool IsVerificationChecked;

        public enum Button
        {
            Yes,
            No,
            Cancel
        }

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
            InitializeComponent();

            YesButton.Visible = yesText != null;
            NoButton.Visible = noText != null;
            Cancel_Button.Visible = cancelText != null;
            VerificationCheckBox.Visible = checkBoxText != null;
            VerificationCheckBox.Checked = checkBoxChecked == true;

            CancelButton = Cancel_Button.Visible ? Cancel_Button : NoButton.Visible ? NoButton : YesButton;

            static void ThrowForDefaultButton(Button button) => throw new ArgumentException("Default button not visible: " + button);

            switch (defaultButton)
            {
                case Button.Yes:
                    if (!YesButton.Visible) ThrowForDefaultButton(Button.Yes);
                    AcceptButton = YesButton;
                    break;
                case Button.No:
                    if (!NoButton.Visible) ThrowForDefaultButton(Button.No);
                    AcceptButton = NoButton;
                    break;
                case Button.Cancel:
                default:
                    if (!Cancel_Button.Visible) ThrowForDefaultButton(Button.Cancel);
                    AcceptButton = Cancel_Button;
                    break;
            }

            if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);
        }
    }
}
