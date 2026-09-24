using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader.Forms;

public sealed partial class MessageBoxCustomForm : DarkFormBase
{
    private readonly bool _multiChoice;
    private const int _bottomAreaHeight = 42;
    private const int _leftAreaWidth = 60;
    private const int _edgePadding = 21;

    public readonly List<string> SelectedItems = new();

    public MessageBoxCustomForm(
        string messageTop,
        string messageBottom,
        string title,
        MessageBoxIcon icon,
        string okText,
        string cancelText,
        bool okIsDangerous,
        IReadOnlyList<string>? choiceStrings = null,
        bool multiSelectionAllowed = true)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        _multiChoice = choiceStrings?.Count > 0;

        #region Set fonts

        // Set these after InitializeComponent() in case that sets other fonts, but before anything else
        MessageTopLabel.Font = SystemFonts.MessageBoxFont;
        MessageBottomLabel.Font = SystemFonts.MessageBoxFont;
        SelectAllButton.Font = SystemFonts.MessageBoxFont;
        OKButton.Font = SystemFonts.MessageBoxFont;
        Cancel_Button.Font = SystemFonts.MessageBoxFont;
        ChoiceListBox.Font = SystemFonts.DefaultFont;

        #endregion

        #region Set passed-in values

        if (icon != MessageBoxIcon.None) ControlUtils.SetMessageBoxIcon(IconPictureBox, icon);

        Text = title;
        MessageTopLabel.Text = messageTop;
        MessageBottomLabel.Text = messageBottom;

        if (_multiChoice)
        {
            ChoiceListBox.MultiSelect = multiSelectionAllowed;

            using (new UpdateRegion(ChoiceListBox))
            {
                // Set this first: the list is now populated
                for (int i = 0; i < choiceStrings!.Count; i++)
                {
                    ChoiceListBox.Items.Add(choiceStrings[i]);
                }
            }

            if (!multiSelectionAllowed)
            {
                SelectAllButton.Hide();
            }
        }
        else
        {
            ChoiceListBox.Hide();
            SelectButtonsFLP.Hide();
            MessageBottomLabel.Hide();
        }

        #endregion

        #region Autosize controls

        int innerControlWidth = MainFLP.Width - 10;
        MessageTopLabel.MaximumSize = MessageTopLabel.MaximumSize with { Width = innerControlWidth };
        MessageBottomLabel.MaximumSize = MessageBottomLabel.MaximumSize with { Width = innerControlWidth };

        if (_multiChoice)
        {
            // Set this second: the list is now sized based on its content
            ChoiceListBox.Size = new Size(innerControlWidth,
                (ChoiceListBox.ItemHeight * ChoiceListBox.Items.Count.Clamp(5, 20)) +
                (SystemInformation.BorderSize.Height * 2));

            // Set this before window autosizing
            SelectButtonsFLP.Width = innerControlWidth + 1;
        }

        // Set these before setting button text
        if (okIsDangerous)
        {
            OKButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            OKButton.ImageAlign = ContentAlignment.MiddleCenter;
            OKButton.Image = Images.RedExclCircle;
        }

        // This is here instead of in the localize method because it's not really localizing, it's just setting
        // text that was passed to it. Which in our case actually is localized, but you know.
        OKButton.Text = okText;
        Cancel_Button.Text = cancelText;

        #endregion

        Localize();

        SelectButtonsFLP.Height = SelectAllButton.Height;

        #region Autosize window

        // Run this after localization, so we have the right button widths

        #region Local functions

        int GetControlFullHeight(Control control, bool onlyIfVisible = false) =>
            !onlyIfVisible || _multiChoice
                ? control.Margin.Vertical +
                  control.Height
                : 0;

        #endregion

        // Set this last: all controls sizes are now set, so we can size the window
        ClientSize = new Size(
            width: _leftAreaWidth +
                   MathMax4(ControlUtils.GetFlowLayoutPanelControlsWidthAll(BottomFLP),
                       MessageTopLabel.Width,
                       _multiChoice ? MessageBottomLabel.Width : 0,
                       _multiChoice ? SelectButtonsFLP.Width : 0) +
                   _edgePadding,
            height: _bottomAreaHeight +
                    GetControlFullHeight(MessageTopLabel) +
                    GetControlFullHeight(ChoiceListBox, onlyIfVisible: true) +
                    GetControlFullHeight(SelectButtonsFLP, onlyIfVisible: true) +
                    (_multiChoice && messageBottom.IsEmpty() ? _edgePadding : GetControlFullHeight(MessageBottomLabel, onlyIfVisible: true)));

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
        if (_multiChoice)
        {
            ChoiceListBox.Location = new Point(0, prevControl.Bottom + 24);
            ChoiceListBox.Size = ChoiceListBox.Size with { Width = MainFLP.Width - rightMargin };

            prevControl = ChoiceListBox;

            SelectButtonsFLP.Location = new Point(0, prevControl.Bottom);
            SelectButtonsFLP.Size = SelectButtonsFLP.Size with { Width = MainFLP.Width - (rightMargin - 1) };

            prevControl = SelectButtonsFLP;
        }

        MessageBottomLabel.Location = new Point(0, prevControl.Bottom + 3);
        MessageBottomLabel.Size = MessageBottomLabel.Size with { Width = MainFLP.Width - rightMargin };

        #endregion

        #endregion

        if (_multiChoice)
        {
            if (ChoiceListBox.Items.Count > 0)
            {
                ChoiceListBox.Items[0].Selected = true;
            }
            else
            {
                OKButton.Enabled = false;
            }
        }

        if (Config.DarkMode) SetTheme(Config.VisualTheme);
    }

    public override void RespondToSystemThemeChange() => SetTheme(Config.VisualTheme);

    private void SetTheme(VisualTheme theme)
    {
        SetThemeBase(theme, x => x == BottomFLP);
        BottomFLP.BackColor = DarkColors.Fen_DarkBackground;
    }

    private void Localize()
    {
        if (_multiChoice)
        {
            SelectAllButton.Text = LText.Global.SelectAll;
        }
    }

    private void SelectAllButton_Click(object sender, EventArgs e)
    {
        if (ChoiceListBox.Items.Count > 0)
        {
            for (int i = 0; i < ChoiceListBox.Items.Count; i++)
            {
                ChoiceListBox.Items[i].Selected = true;
            }
        }
    }

    private void ChoiceListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_multiChoice) OKButton.Enabled = ChoiceListBox.SelectedIndex > -1;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK && _multiChoice && ChoiceListBox.SelectedIndex > -1)
        {
            SelectedItems.AddRange(ChoiceListBox.SelectedItemsAsStrings);
        }
        base.OnFormClosing(e);
    }
}
