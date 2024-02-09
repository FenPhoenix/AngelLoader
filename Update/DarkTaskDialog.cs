using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace Update;

public partial class DarkTaskDialog : DarkFormBase
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly bool _yesButtonVisible;
    private readonly bool _noButtonVisible;
    private readonly bool _cancelButtonVisible;

    [PublicAPI]
    public DarkTaskDialog(
        string message,
        string title,
        MessageBoxIcon icon = MessageBoxIcon.None,
        string? yesText = null,
        string? noText = null,
        string? cancelText = null,
        DialogResult defaultButton = DialogResult.Cancel)
    {
        // All numbers are just matching the original Win32 task dialog as closely as possible. Don't worry
        // about them.

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        #region Set fonts

        // Set these after InitializeComponent() in case that sets other fonts, but before anything else
        MessageLabel.Font = SystemFonts.MessageBoxFont;
        YesButton.Font = SystemFonts.MessageBoxFont;
        NoButton.Font = SystemFonts.MessageBoxFont;
        Cancel_Button.Font = SystemFonts.MessageBoxFont;

        #endregion

        // Have to use these bools, because if we check Visible it will always be false even though we might
        // just have set it to true, because we haven't shown ourselves yet so everything counts as not visible
        _yesButtonVisible = yesText != null;
        _noButtonVisible = noText != null;
        _cancelButtonVisible = cancelText != null;

        int imageMarginX = icon != MessageBoxIcon.None ? 49 : 7;

        #region Set control state

        base.Text = title;
        MessageLabel.Text = message;

        if (yesText != null) YesButton.Text = yesText;
        if (noText != null) NoButton.Text = noText;
        if (cancelText != null) Cancel_Button.Text = cancelText;

        YesButton.Visible = _yesButtonVisible;
        NoButton.Visible = _noButtonVisible;
        Cancel_Button.Visible = _cancelButtonVisible;

        if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

        MessageLabel.Location = MessageLabel.Location with { X = imageMarginX };

        #region Set default buttons

        CancelButton = _cancelButtonVisible ? Cancel_Button : _noButtonVisible ? NoButton : YesButton;

        NoButton.DialogResult = DialogResult.No;
        YesButton.DialogResult = DialogResult.Yes;
        Cancel_Button.DialogResult = DialogResult.Cancel;

        AcceptButton = defaultButton switch
        {
            DialogResult.Yes when (_yesButtonVisible) => YesButton,
            DialogResult.No when (_noButtonVisible) => NoButton,
            DialogResult.Cancel when (_cancelButtonVisible) => Cancel_Button,
            _ => Cancel_Button
        };

        #endregion

        #endregion

        #region Size form

        int bottomBarContentWidth = ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP);

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
                Y = (BottomFLP.Top / 2) - (MessageLabel.Height / 2)
            };
        }

        DialogResult = DialogResult.Cancel;

        SetTheme(Data.VisualTheme);
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

    private void SetTheme(VisualTheme theme)
    {
        if (theme == VisualTheme.Dark)
        {
            SetThemeBase(theme, x => x == BottomFLP);
            BottomFLP.BackColor = DarkColors.Fen_DarkBackground;
        }
    }
}
