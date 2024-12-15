using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public partial class DarkTaskDialog : DarkFormBase
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly bool _yesButtonVisible;
    private readonly bool _noButtonVisible;
    private readonly bool _cancelButtonVisible;

    #region Public fields

    [PublicAPI]
    public bool IsVerificationChecked => VerificationCheckBox.Checked;

    #endregion

    [PublicAPI]
    public DarkTaskDialog(
        string message,
        string title,
        MessageBoxIcon icon = MessageBoxIcon.None,
        string? yesText = null,
        string? noText = null,
        string? cancelText = null,
        bool yesIsDangerous = false,
        bool noIsDangerous = false,
        bool cancelIsDangerous = false,
        string? checkBoxText = null,
        bool? checkBoxChecked = null,
        MBoxButton defaultButton = MBoxButton.Cancel,
        bool viewLogButtonVisible = false)
    {
        // All numbers are just matching the original Win32 task dialog as closely as possible. Don't worry
        // about them.

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
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
        _yesButtonVisible = yesText != null;
        _noButtonVisible = noText != null;
        _cancelButtonVisible = cancelText != null;
        bool checkBoxVisible = checkBoxText != null;

        if (!_yesButtonVisible && !_noButtonVisible && !_cancelButtonVisible)
        {
            ThrowHelper.ArgumentException("At least one button must have text specified!");
        }

        int imageMarginX = icon != MessageBoxIcon.None ? 49 : 7;

        #region Set control state

        base.Text = title;
        MessageLabel.Text = message;

        if (yesIsDangerous && _yesButtonVisible)
        {
            YesButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            YesButton.ImageAlign = ContentAlignment.MiddleCenter;
            YesButton.Image = Images.RedExclCircle;
        }

        if (noIsDangerous && _noButtonVisible)
        {
            NoButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            NoButton.ImageAlign = ContentAlignment.MiddleCenter;
            NoButton.Image = Images.RedExclCircle;
        }

        if (cancelIsDangerous && _cancelButtonVisible)
        {
            Cancel_Button.TextImageRelation = TextImageRelation.ImageBeforeText;
            Cancel_Button.ImageAlign = ContentAlignment.MiddleCenter;
            Cancel_Button.Image = Images.RedExclCircle;
        }

        if (yesText != null) YesButton.Text = yesText;
        if (noText != null) NoButton.Text = noText;
        if (cancelText != null) Cancel_Button.Text = cancelText;
        if (checkBoxText != null) VerificationCheckBox.Text = checkBoxText;

        YesButton.Visible = _yesButtonVisible;
        NoButton.Visible = _noButtonVisible;
        Cancel_Button.Visible = _cancelButtonVisible;
        VerificationCheckBox.Visible = checkBoxVisible;
        VerificationCheckBox.Checked = checkBoxChecked == true;
        ViewLogButton.Visible = viewLogButtonVisible;

        if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

        MessageLabel.Location = MessageLabel.Location with { X = imageMarginX };

        #region Set default buttons

        CancelButton = _cancelButtonVisible ? Cancel_Button : _noButtonVisible ? NoButton : YesButton;

        static void ThrowForDefaultButton(MBoxButton button) => ThrowHelper.ArgumentException("Default button not visible: " + button);

        NoButton.DialogResult = DialogResult.No;
        YesButton.DialogResult = DialogResult.Yes;
        Cancel_Button.DialogResult = DialogResult.Cancel;

        switch (defaultButton)
        {
            case MBoxButton.Yes:
                if (!_yesButtonVisible) ThrowForDefaultButton(MBoxButton.Yes);
                AcceptButton = YesButton;
                break;
            case MBoxButton.No:
                if (!_noButtonVisible) ThrowForDefaultButton(MBoxButton.No);
                AcceptButton = NoButton;
                break;
            case MBoxButton.Cancel:
            default:
                if (!_cancelButtonVisible) ThrowForDefaultButton(MBoxButton.Cancel);
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
        int width = Math.Max(bottomBarContentWidth, Math.Min(550, MessageLabel.Width + imageMarginX + 7));
        MessageLabel.MaximumSize = MessageLabel.MaximumSize with { Width = (width - imageMarginX) - 7 };
        int height = MessageLabel.Top + MessageLabel.Height + 15 + BottomFLP.Height;

        // Add some padding between the bottom of the icon and the bottom panel
        if (icon != MessageBoxIcon.None && MessageLabel.Bottom <= IconPictureBox.Bottom + 10)
        {
            height += 8;
        }

        ClientSize = new Size(width, height);

        #endregion

        // If only one line of text, center the label vertically for a better look
        if (MessageLabel.Height <= TextRenderer.MeasureText("j^", MessageLabel.Font).Height)
        {
            MessageLabel.Location = MessageLabel.Location with
            {
                Y = (BottomFLP.Top / 2) - (MessageLabel.Height / 2),
            };
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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            DialogResult =
                _cancelButtonVisible
                    ? DialogResult.Cancel
                    : _noButtonVisible
                        ? DialogResult.No
                        : DialogResult.Yes;
        }
        base.OnFormClosing(e);
    }

    public override void RespondToSystemThemeChange() => SetTheme(Config.VisualTheme);

    private void SetTheme(VisualTheme theme)
    {
        if (theme == VisualTheme.Dark)
        {
            SetThemeBase(theme, x => x == BottomFLP);
            BottomFLP.BackColor = DarkColors.Fen_DarkBackground;
        }
        else
        {
            VerificationCheckBox.BackColor = SystemColors.Control;
        }
    }

    private void ViewLogButton_Click(object sender, EventArgs e) => Core.OpenLogFile();
}
