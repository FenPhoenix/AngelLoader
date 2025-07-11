using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader.Forms;

public sealed partial class MessageBoxWithTextBoxForm : DarkFormBase
{
    private const int _bottomAreaHeight = 42;
    private const int _leftAreaWidth = 60;
    private const int _edgePadding = 21;

    [PublicAPI]
    public bool IsVerificationChecked => VerificationCheckBox.Checked;

    public MessageBoxWithTextBoxForm(
        string messageTop,
        string messageTextBox,
        string messageBottom,
        string title,
        MessageBoxIcon icon = MessageBoxIcon.None,
        string? okText = null,
        string? cancelText = null,
        bool okIsDangerous = false,
        string? checkBoxText = null,
        bool? checkBoxChecked = null)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        VerificationCheckBox.DarkModeBackColor = DarkColors.Fen_DarkBackground;

        #region Set fonts

        // Set these after InitializeComponent() in case that sets other fonts, but before anything else
        MessageTopLabel.Font = SystemFonts.MessageBoxFont;
        MessageBottomLabel.Font = SystemFonts.MessageBoxFont;
        OKButton.Font = SystemFonts.MessageBoxFont;
        Cancel_Button.Font = SystemFonts.MessageBoxFont;
        MainTextBox.Font = SystemFonts.DefaultFont;
        VerificationCheckBox.Font = SystemFonts.MessageBoxFont;

        #endregion

        #region Set passed-in values

        if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

        Text = title;
        MessageTopLabel.Text = messageTop;
        MainTextBox.Text = messageTextBox;
        MessageBottomLabel.Text = messageBottom;

        #endregion

        #region Autosize controls

        int innerControlWidth = MainFLP.Width - 10;
        MessageTopLabel.MaximumSize = MessageTopLabel.MaximumSize with { Width = innerControlWidth };
        MessageBottomLabel.MaximumSize = MessageBottomLabel.MaximumSize with { Width = innerControlWidth };

        MainTextBox.Size = MainTextBox.Size with
        {
            Height = TextRenderer.MeasureText(messageTextBox, MainTextBox.Font).Height + 32,
        };

        // Set these before setting button text
        if (okIsDangerous)
        {
            OKButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            OKButton.ImageAlign = ContentAlignment.MiddleCenter;
            OKButton.Image = Images.RedExclCircle;
        }

        // This is here instead of in the localize method because it's not really localizing, it's just setting
        // text that was passed to it. Which in our case actually is localized, but you know.
        if (okText != null) OKButton.Text = okText;
        if (cancelText != null)
        {
            Cancel_Button.Text = cancelText;
        }
        else
        {
            Cancel_Button.Hide();
        }
        if (checkBoxText != null) VerificationCheckBox.Text = checkBoxText;

        bool checkBoxVisible = checkBoxText != null;

        VerificationCheckBox.Visible = checkBoxVisible;
        VerificationCheckBox.Checked = checkBoxChecked == true;

        #endregion

        #region Autosize window

        // Run this after localization, so we have the right button widths

        #region Local functions

        static int GetControlFullHeight(Control control, bool onlyIfVisible = false) =>
            !onlyIfVisible
                ? control.Margin.Vertical +
                  control.Height
                : 0;

        #endregion

        int bottomBarContentWidth = ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP);
        if (checkBoxVisible)
        {
            bottomBarContentWidth += VerificationCheckBox.Margin.Horizontal + VerificationCheckBox.Width + 22;
        }

        // Set this last: all controls sizes are now set, so we can size the window
        ClientSize = new Size(
            width: _leftAreaWidth +
                   MathMax4(ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP),
                       MessageTopLabel.Width,
                       MessageBottomLabel.Width,
                       bottomBarContentWidth) +
                   _edgePadding,
            height: _bottomAreaHeight +
                    GetControlFullHeight(MessageTopLabel) +
                    GetControlFullHeight(MainTextBox) +
                    32 +
                    (messageBottom.IsEmpty() ? _edgePadding : GetControlFullHeight(MessageBottomLabel, onlyIfVisible: true)));

        if (ContentTLP.Height < IconPictureBox.Height + (IconPictureBox.Top * 2))
        {
            IconPictureBox.Margin = IconPictureBox.Margin with { Top = (ContentTLP.Height / 2) - (IconPictureBox.Height / 2), Bottom = 0 };
        }

        /*
        @Wine: Manual layout because Wine breaks with a FlowLayoutPanel, which insists on "top-to-bottom" meaning
        "top-to-bottom until a slight breeze hits me and then left-to-right".
        */
        #region Manual vertical flow layout

        MessageTopLabel.Location = new Point(0, 18);
        MessageTopLabel.Size = MessageTopLabel.Size with { Width = MainFLP.Width };

        const int rightMargin = 22;

        Control prevControl = MessageTopLabel;
        MainTextBox.Location = new Point(0, prevControl.Bottom + 16);

        prevControl = MainTextBox;

        MessageBottomLabel.Location = new Point(0, prevControl.Bottom + 14);
        MessageBottomLabel.Size = MessageBottomLabel.Size with { Width = MainFLP.Width - rightMargin };

        #endregion

        #endregion

        SetTheme(Config.VisualTheme);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // This doesn't take unless we put it all the way in Shown, annoyingly
        if (OKButton.Visible)
        {
            OKButton.Focus();
        }
        else if (Cancel_Button.Visible)
        {
            Cancel_Button.Focus();
        }
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
}
