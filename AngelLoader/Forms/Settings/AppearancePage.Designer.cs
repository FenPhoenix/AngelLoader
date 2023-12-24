#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class AppearancePage
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.PagePanel = new System.Windows.Forms.Panel();
            this.PlayWithoutFMGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.PlayWithoutFM_MultipleButtonsRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.PlayWithoutFM_SingleButtonRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.FMsListGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.DateFormatRBPanel = new System.Windows.Forms.Panel();
            this.DateCurrentCultureShortRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DateCurrentCultureLongRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DateCustomRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RatingDisplayStyleRBPanel = new System.Windows.Forms.Panel();
            this.RatingNDLDisplayStyleRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RatingFMSelDisplayStyleRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.GameOrganizationRBPanel = new System.Windows.Forms.Panel();
            this.OrganizeGamesByTabRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.OrganizeGamesInOneListRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.UseShortGameTabNamesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.RecentFMsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.RecentFMsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.PreviewDateFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.PreviewDateLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.DateCustomFormatPanel = new System.Windows.Forms.Panel();
            this.DateSeparator3TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.DateSeparator2TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.Date1ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.DateSeparator1TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.Date4ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.Date2ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.Date3ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.RatingUseStarsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.RatingExamplePictureBox = new System.Windows.Forms.PictureBox();
            this.RecentFMsHeaderLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.DateFormatLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.RatingDisplayStyleLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.GameOrganizationLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SortingLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MoveArticlesToEndCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.EnableIgnoreArticlesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ArticlesTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.ReadmeGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ReadmeFixedWidthFontCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ShowOrHideUIElementsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ShowWebSearchButtonCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ShowExitButtonCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ShowFMListZoomButtonsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ShowUninstallButtonCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.VisualThemeGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.FollowSystemThemeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DarkThemeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.ClassicThemeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.LanguageGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.LanguageComboBox = new AngelLoader.Forms.CustomControls.DarkComboBoxWithBackingItems();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.PagePanel.SuspendLayout();
            this.PlayWithoutFMGroupBox.SuspendLayout();
            this.FMsListGroupBox.SuspendLayout();
            this.DateFormatRBPanel.SuspendLayout();
            this.RatingDisplayStyleRBPanel.SuspendLayout();
            this.GameOrganizationRBPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecentFMsNumericUpDown)).BeginInit();
            this.PreviewDateFLP.SuspendLayout();
            this.DateCustomFormatPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RatingExamplePictureBox)).BeginInit();
            this.ReadmeGroupBox.SuspendLayout();
            this.ShowOrHideUIElementsGroupBox.SuspendLayout();
            this.VisualThemeGroupBox.SuspendLayout();
            this.LanguageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.PlayWithoutFMGroupBox);
            this.PagePanel.Controls.Add(this.FMsListGroupBox);
            this.PagePanel.Controls.Add(this.ReadmeGroupBox);
            this.PagePanel.Controls.Add(this.ShowOrHideUIElementsGroupBox);
            this.PagePanel.Controls.Add(this.VisualThemeGroupBox);
            this.PagePanel.Controls.Add(this.LanguageGroupBox);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(496, 1219);
            this.PagePanel.TabIndex = 0;
            // 
            // PlayWithoutFMGroupBox
            // 
            this.PlayWithoutFMGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayWithoutFMGroupBox.Controls.Add(this.PlayWithoutFM_MultipleButtonsRadioButton);
            this.PlayWithoutFMGroupBox.Controls.Add(this.PlayWithoutFM_SingleButtonRadioButton);
            this.PlayWithoutFMGroupBox.Location = new System.Drawing.Point(8, 1120);
            this.PlayWithoutFMGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.PlayWithoutFMGroupBox.Name = "PlayWithoutFMGroupBox";
            this.PlayWithoutFMGroupBox.Size = new System.Drawing.Size(480, 80);
            this.PlayWithoutFMGroupBox.TabIndex = 5;
            this.PlayWithoutFMGroupBox.TabStop = false;
            this.PlayWithoutFMGroupBox.Text = "Play without FM";
            // 
            // PlayWithoutFM_MultipleButtonsRadioButton
            // 
            this.PlayWithoutFM_MultipleButtonsRadioButton.AutoSize = true;
            this.PlayWithoutFM_MultipleButtonsRadioButton.Location = new System.Drawing.Point(16, 48);
            this.PlayWithoutFM_MultipleButtonsRadioButton.Name = "PlayWithoutFM_MultipleButtonsRadioButton";
            this.PlayWithoutFM_MultipleButtonsRadioButton.Size = new System.Drawing.Size(99, 17);
            this.PlayWithoutFM_MultipleButtonsRadioButton.TabIndex = 0;
            this.PlayWithoutFM_MultipleButtonsRadioButton.TabStop = true;
            this.PlayWithoutFM_MultipleButtonsRadioButton.Text = "Multiple buttons";
            // 
            // PlayWithoutFM_SingleButtonRadioButton
            // 
            this.PlayWithoutFM_SingleButtonRadioButton.AutoSize = true;
            this.PlayWithoutFM_SingleButtonRadioButton.Location = new System.Drawing.Point(16, 24);
            this.PlayWithoutFM_SingleButtonRadioButton.Name = "PlayWithoutFM_SingleButtonRadioButton";
            this.PlayWithoutFM_SingleButtonRadioButton.Size = new System.Drawing.Size(138, 17);
            this.PlayWithoutFM_SingleButtonRadioButton.TabIndex = 0;
            this.PlayWithoutFM_SingleButtonRadioButton.TabStop = true;
            this.PlayWithoutFM_SingleButtonRadioButton.Text = "Single button with menu";
            // 
            // FMsListGroupBox
            // 
            this.FMsListGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMsListGroupBox.Controls.Add(this.DateFormatRBPanel);
            this.FMsListGroupBox.Controls.Add(this.RatingDisplayStyleRBPanel);
            this.FMsListGroupBox.Controls.Add(this.GameOrganizationRBPanel);
            this.FMsListGroupBox.Controls.Add(this.RecentFMsNumericUpDown);
            this.FMsListGroupBox.Controls.Add(this.RecentFMsLabel);
            this.FMsListGroupBox.Controls.Add(this.PreviewDateFLP);
            this.FMsListGroupBox.Controls.Add(this.DateCustomFormatPanel);
            this.FMsListGroupBox.Controls.Add(this.RatingUseStarsCheckBox);
            this.FMsListGroupBox.Controls.Add(this.RatingExamplePictureBox);
            this.FMsListGroupBox.Controls.Add(this.RecentFMsHeaderLabel);
            this.FMsListGroupBox.Controls.Add(this.DateFormatLabel);
            this.FMsListGroupBox.Controls.Add(this.RatingDisplayStyleLabel);
            this.FMsListGroupBox.Controls.Add(this.GameOrganizationLabel);
            this.FMsListGroupBox.Controls.Add(this.SortingLabel);
            this.FMsListGroupBox.Controls.Add(this.MoveArticlesToEndCheckBox);
            this.FMsListGroupBox.Controls.Add(this.EnableIgnoreArticlesCheckBox);
            this.FMsListGroupBox.Controls.Add(this.ArticlesTextBox);
            this.FMsListGroupBox.Location = new System.Drawing.Point(8, 196);
            this.FMsListGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.FMsListGroupBox.Name = "FMsListGroupBox";
            this.FMsListGroupBox.Size = new System.Drawing.Size(480, 688);
            this.FMsListGroupBox.TabIndex = 2;
            this.FMsListGroupBox.TabStop = false;
            this.FMsListGroupBox.Text = "FMs list";
            // 
            // DateFormatRBPanel
            // 
            this.DateFormatRBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DateFormatRBPanel.Controls.Add(this.DateCurrentCultureShortRadioButton);
            this.DateFormatRBPanel.Controls.Add(this.DateCurrentCultureLongRadioButton);
            this.DateFormatRBPanel.Controls.Add(this.DateCustomRadioButton);
            this.DateFormatRBPanel.Location = new System.Drawing.Point(16, 480);
            this.DateFormatRBPanel.Name = "DateFormatRBPanel";
            this.DateFormatRBPanel.Size = new System.Drawing.Size(448, 72);
            this.DateFormatRBPanel.TabIndex = 14;
            // 
            // DateCurrentCultureShortRadioButton
            // 
            this.DateCurrentCultureShortRadioButton.AutoSize = true;
            this.DateCurrentCultureShortRadioButton.Location = new System.Drawing.Point(0, 3);
            this.DateCurrentCultureShortRadioButton.Name = "DateCurrentCultureShortRadioButton";
            this.DateCurrentCultureShortRadioButton.Size = new System.Drawing.Size(119, 17);
            this.DateCurrentCultureShortRadioButton.TabIndex = 0;
            this.DateCurrentCultureShortRadioButton.Text = "System locale, short";
            // 
            // DateCurrentCultureLongRadioButton
            // 
            this.DateCurrentCultureLongRadioButton.AutoSize = true;
            this.DateCurrentCultureLongRadioButton.Location = new System.Drawing.Point(0, 27);
            this.DateCurrentCultureLongRadioButton.Name = "DateCurrentCultureLongRadioButton";
            this.DateCurrentCultureLongRadioButton.Size = new System.Drawing.Size(116, 17);
            this.DateCurrentCultureLongRadioButton.TabIndex = 1;
            this.DateCurrentCultureLongRadioButton.Text = "System locale, long";
            // 
            // DateCustomRadioButton
            // 
            this.DateCustomRadioButton.AutoSize = true;
            this.DateCustomRadioButton.Location = new System.Drawing.Point(0, 51);
            this.DateCustomRadioButton.Name = "DateCustomRadioButton";
            this.DateCustomRadioButton.Size = new System.Drawing.Size(63, 17);
            this.DateCustomRadioButton.TabIndex = 2;
            this.DateCustomRadioButton.Text = "Custom:";
            // 
            // RatingDisplayStyleRBPanel
            // 
            this.RatingDisplayStyleRBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RatingDisplayStyleRBPanel.Controls.Add(this.RatingNDLDisplayStyleRadioButton);
            this.RatingDisplayStyleRBPanel.Controls.Add(this.RatingFMSelDisplayStyleRadioButton);
            this.RatingDisplayStyleRBPanel.Location = new System.Drawing.Point(16, 312);
            this.RatingDisplayStyleRBPanel.Name = "RatingDisplayStyleRBPanel";
            this.RatingDisplayStyleRBPanel.Size = new System.Drawing.Size(456, 48);
            this.RatingDisplayStyleRBPanel.TabIndex = 9;
            // 
            // RatingNDLDisplayStyleRadioButton
            // 
            this.RatingNDLDisplayStyleRadioButton.AutoSize = true;
            this.RatingNDLDisplayStyleRadioButton.Location = new System.Drawing.Point(0, 0);
            this.RatingNDLDisplayStyleRadioButton.Name = "RatingNDLDisplayStyleRadioButton";
            this.RatingNDLDisplayStyleRadioButton.Size = new System.Drawing.Size(219, 17);
            this.RatingNDLDisplayStyleRadioButton.TabIndex = 0;
            this.RatingNDLDisplayStyleRadioButton.Text = "NewDarkLoader (0-10 in increments of 1)";
            // 
            // RatingFMSelDisplayStyleRadioButton
            // 
            this.RatingFMSelDisplayStyleRadioButton.AutoSize = true;
            this.RatingFMSelDisplayStyleRadioButton.Checked = true;
            this.RatingFMSelDisplayStyleRadioButton.Location = new System.Drawing.Point(0, 24);
            this.RatingFMSelDisplayStyleRadioButton.Name = "RatingFMSelDisplayStyleRadioButton";
            this.RatingFMSelDisplayStyleRadioButton.Size = new System.Drawing.Size(174, 17);
            this.RatingFMSelDisplayStyleRadioButton.TabIndex = 1;
            this.RatingFMSelDisplayStyleRadioButton.TabStop = true;
            this.RatingFMSelDisplayStyleRadioButton.Text = "FMSel (0-5 in increments of 0.5)";
            // 
            // GameOrganizationRBPanel
            // 
            this.GameOrganizationRBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameOrganizationRBPanel.Controls.Add(this.OrganizeGamesByTabRadioButton);
            this.GameOrganizationRBPanel.Controls.Add(this.OrganizeGamesInOneListRadioButton);
            this.GameOrganizationRBPanel.Controls.Add(this.UseShortGameTabNamesCheckBox);
            this.GameOrganizationRBPanel.Location = new System.Drawing.Point(16, 40);
            this.GameOrganizationRBPanel.Name = "GameOrganizationRBPanel";
            this.GameOrganizationRBPanel.Size = new System.Drawing.Size(456, 96);
            this.GameOrganizationRBPanel.TabIndex = 1;
            // 
            // OrganizeGamesByTabRadioButton
            // 
            this.OrganizeGamesByTabRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrganizeGamesByTabRadioButton.Location = new System.Drawing.Point(0, 0);
            this.OrganizeGamesByTabRadioButton.Name = "OrganizeGamesByTabRadioButton";
            this.OrganizeGamesByTabRadioButton.Size = new System.Drawing.Size(456, 32);
            this.OrganizeGamesByTabRadioButton.TabIndex = 0;
            this.OrganizeGamesByTabRadioButton.Text = "Each game in its own tab";
            // 
            // OrganizeGamesInOneListRadioButton
            // 
            this.OrganizeGamesInOneListRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrganizeGamesInOneListRadioButton.Checked = true;
            this.OrganizeGamesInOneListRadioButton.Location = new System.Drawing.Point(0, 64);
            this.OrganizeGamesInOneListRadioButton.Name = "OrganizeGamesInOneListRadioButton";
            this.OrganizeGamesInOneListRadioButton.Size = new System.Drawing.Size(456, 32);
            this.OrganizeGamesInOneListRadioButton.TabIndex = 2;
            this.OrganizeGamesInOneListRadioButton.TabStop = true;
            this.OrganizeGamesInOneListRadioButton.Text = "Everything in one list, and games are filters";
            // 
            // UseShortGameTabNamesCheckBox
            // 
            this.UseShortGameTabNamesCheckBox.AutoSize = true;
            this.UseShortGameTabNamesCheckBox.Location = new System.Drawing.Point(24, 40);
            this.UseShortGameTabNamesCheckBox.Name = "UseShortGameTabNamesCheckBox";
            this.UseShortGameTabNamesCheckBox.Size = new System.Drawing.Size(172, 17);
            this.UseShortGameTabNamesCheckBox.TabIndex = 1;
            this.UseShortGameTabNamesCheckBox.Text = "Use short names on game tabs";
            // 
            // RecentFMsNumericUpDown
            // 
            this.RecentFMsNumericUpDown.Location = new System.Drawing.Point(16, 656);
            this.RecentFMsNumericUpDown.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.RecentFMsNumericUpDown.Name = "RecentFMsNumericUpDown";
            this.RecentFMsNumericUpDown.Size = new System.Drawing.Size(56, 20);
            this.RecentFMsNumericUpDown.TabIndex = 55;
            this.RecentFMsNumericUpDown.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // RecentFMsLabel
            // 
            this.RecentFMsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RecentFMsLabel.Location = new System.Drawing.Point(16, 624);
            this.RecentFMsLabel.Name = "RecentFMsLabel";
            this.RecentFMsLabel.Size = new System.Drawing.Size(456, 32);
            this.RecentFMsLabel.TabIndex = 14;
            this.RecentFMsLabel.Text = "Maximum number of days to consider an FM \"recent\":";
            this.RecentFMsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PreviewDateFLP
            // 
            this.PreviewDateFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewDateFLP.Controls.Add(this.PreviewDateLabel);
            this.PreviewDateFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.PreviewDateFLP.Location = new System.Drawing.Point(16, 456);
            this.PreviewDateFLP.Name = "PreviewDateFLP";
            this.PreviewDateFLP.Size = new System.Drawing.Size(456, 16);
            this.PreviewDateFLP.TabIndex = 11;
            // 
            // PreviewDateLabel
            // 
            this.PreviewDateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewDateLabel.AutoSize = true;
            this.PreviewDateLabel.Location = new System.Drawing.Point(378, 0);
            this.PreviewDateLabel.Name = "PreviewDateLabel";
            this.PreviewDateLabel.Size = new System.Drawing.Size(75, 13);
            this.PreviewDateLabel.TabIndex = 0;
            this.PreviewDateLabel.Text = "[Preview date]";
            // 
            // DateCustomFormatPanel
            // 
            this.DateCustomFormatPanel.Controls.Add(this.DateSeparator3TextBox);
            this.DateCustomFormatPanel.Controls.Add(this.DateSeparator2TextBox);
            this.DateCustomFormatPanel.Controls.Add(this.Date1ComboBox);
            this.DateCustomFormatPanel.Controls.Add(this.DateSeparator1TextBox);
            this.DateCustomFormatPanel.Controls.Add(this.Date4ComboBox);
            this.DateCustomFormatPanel.Controls.Add(this.Date2ComboBox);
            this.DateCustomFormatPanel.Controls.Add(this.Date3ComboBox);
            this.DateCustomFormatPanel.Location = new System.Drawing.Point(16, 556);
            this.DateCustomFormatPanel.Name = "DateCustomFormatPanel";
            this.DateCustomFormatPanel.Size = new System.Drawing.Size(448, 24);
            this.DateCustomFormatPanel.TabIndex = 25;
            // 
            // DateSeparator3TextBox
            // 
            this.DateSeparator3TextBox.Location = new System.Drawing.Point(332, 0);
            this.DateSeparator3TextBox.Name = "DateSeparator3TextBox";
            this.DateSeparator3TextBox.Size = new System.Drawing.Size(24, 20);
            this.DateSeparator3TextBox.TabIndex = 5;
            // 
            // DateSeparator2TextBox
            // 
            this.DateSeparator2TextBox.Location = new System.Drawing.Point(212, 0);
            this.DateSeparator2TextBox.Name = "DateSeparator2TextBox";
            this.DateSeparator2TextBox.Size = new System.Drawing.Size(24, 20);
            this.DateSeparator2TextBox.TabIndex = 3;
            // 
            // Date1ComboBox
            // 
            this.Date1ComboBox.FormattingEnabled = true;
            this.Date1ComboBox.Location = new System.Drawing.Point(0, 0);
            this.Date1ComboBox.Name = "Date1ComboBox";
            this.Date1ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date1ComboBox.SuppressScrollWheelValueChange = true;
            this.Date1ComboBox.TabIndex = 0;
            // 
            // DateSeparator1TextBox
            // 
            this.DateSeparator1TextBox.Location = new System.Drawing.Point(92, 0);
            this.DateSeparator1TextBox.Name = "DateSeparator1TextBox";
            this.DateSeparator1TextBox.Size = new System.Drawing.Size(24, 20);
            this.DateSeparator1TextBox.TabIndex = 1;
            // 
            // Date4ComboBox
            // 
            this.Date4ComboBox.FormattingEnabled = true;
            this.Date4ComboBox.Location = new System.Drawing.Point(360, 0);
            this.Date4ComboBox.Name = "Date4ComboBox";
            this.Date4ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date4ComboBox.SuppressScrollWheelValueChange = true;
            this.Date4ComboBox.TabIndex = 6;
            // 
            // Date2ComboBox
            // 
            this.Date2ComboBox.FormattingEnabled = true;
            this.Date2ComboBox.Location = new System.Drawing.Point(120, 0);
            this.Date2ComboBox.Name = "Date2ComboBox";
            this.Date2ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date2ComboBox.SuppressScrollWheelValueChange = true;
            this.Date2ComboBox.TabIndex = 2;
            // 
            // Date3ComboBox
            // 
            this.Date3ComboBox.FormattingEnabled = true;
            this.Date3ComboBox.Location = new System.Drawing.Point(240, 0);
            this.Date3ComboBox.Name = "Date3ComboBox";
            this.Date3ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date3ComboBox.SuppressScrollWheelValueChange = true;
            this.Date3ComboBox.TabIndex = 4;
            // 
            // RatingUseStarsCheckBox
            // 
            this.RatingUseStarsCheckBox.AutoSize = true;
            this.RatingUseStarsCheckBox.Checked = true;
            this.RatingUseStarsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RatingUseStarsCheckBox.Location = new System.Drawing.Point(32, 360);
            this.RatingUseStarsCheckBox.Name = "RatingUseStarsCheckBox";
            this.RatingUseStarsCheckBox.Size = new System.Drawing.Size(70, 17);
            this.RatingUseStarsCheckBox.TabIndex = 9;
            this.RatingUseStarsCheckBox.Text = "Use stars";
            // 
            // RatingExamplePictureBox
            // 
            this.RatingExamplePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RatingExamplePictureBox.Location = new System.Drawing.Point(395, 380);
            this.RatingExamplePictureBox.Name = "RatingExamplePictureBox";
            this.RatingExamplePictureBox.Size = new System.Drawing.Size(79, 23);
            this.RatingExamplePictureBox.TabIndex = 8;
            this.RatingExamplePictureBox.TabStop = false;
            // 
            // RecentFMsHeaderLabel
            // 
            this.RecentFMsHeaderLabel.AutoSize = true;
            this.RecentFMsHeaderLabel.Location = new System.Drawing.Point(8, 608);
            this.RecentFMsHeaderLabel.Name = "RecentFMsHeaderLabel";
            this.RecentFMsHeaderLabel.Size = new System.Drawing.Size(68, 13);
            this.RecentFMsHeaderLabel.TabIndex = 13;
            this.RecentFMsHeaderLabel.Text = "Recent FMs:";
            // 
            // DateFormatLabel
            // 
            this.DateFormatLabel.AutoSize = true;
            this.DateFormatLabel.Location = new System.Drawing.Point(8, 432);
            this.DateFormatLabel.Name = "DateFormatLabel";
            this.DateFormatLabel.Size = new System.Drawing.Size(65, 13);
            this.DateFormatLabel.TabIndex = 10;
            this.DateFormatLabel.Text = "Date format:";
            // 
            // RatingDisplayStyleLabel
            // 
            this.RatingDisplayStyleLabel.AutoSize = true;
            this.RatingDisplayStyleLabel.Location = new System.Drawing.Point(8, 288);
            this.RatingDisplayStyleLabel.Name = "RatingDisplayStyleLabel";
            this.RatingDisplayStyleLabel.Size = new System.Drawing.Size(100, 13);
            this.RatingDisplayStyleLabel.TabIndex = 7;
            this.RatingDisplayStyleLabel.Text = "Rating display style:";
            // 
            // GameOrganizationLabel
            // 
            this.GameOrganizationLabel.AutoSize = true;
            this.GameOrganizationLabel.Location = new System.Drawing.Point(8, 24);
            this.GameOrganizationLabel.Name = "GameOrganizationLabel";
            this.GameOrganizationLabel.Size = new System.Drawing.Size(98, 13);
            this.GameOrganizationLabel.TabIndex = 0;
            this.GameOrganizationLabel.Text = "Game organization:";
            // 
            // SortingLabel
            // 
            this.SortingLabel.AutoSize = true;
            this.SortingLabel.Location = new System.Drawing.Point(8, 160);
            this.SortingLabel.Name = "SortingLabel";
            this.SortingLabel.Size = new System.Drawing.Size(43, 13);
            this.SortingLabel.TabIndex = 2;
            this.SortingLabel.Text = "Sorting:";
            // 
            // MoveArticlesToEndCheckBox
            // 
            this.MoveArticlesToEndCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveArticlesToEndCheckBox.Checked = true;
            this.MoveArticlesToEndCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MoveArticlesToEndCheckBox.Location = new System.Drawing.Point(16, 232);
            this.MoveArticlesToEndCheckBox.Name = "MoveArticlesToEndCheckBox";
            this.MoveArticlesToEndCheckBox.Size = new System.Drawing.Size(456, 32);
            this.MoveArticlesToEndCheckBox.TabIndex = 5;
            this.MoveArticlesToEndCheckBox.Text = "Move articles to the end of names when displaying them";
            // 
            // EnableIgnoreArticlesCheckBox
            // 
            this.EnableIgnoreArticlesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EnableIgnoreArticlesCheckBox.Checked = true;
            this.EnableIgnoreArticlesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnableIgnoreArticlesCheckBox.Location = new System.Drawing.Point(16, 176);
            this.EnableIgnoreArticlesCheckBox.Name = "EnableIgnoreArticlesCheckBox";
            this.EnableIgnoreArticlesCheckBox.Size = new System.Drawing.Size(456, 32);
            this.EnableIgnoreArticlesCheckBox.TabIndex = 3;
            this.EnableIgnoreArticlesCheckBox.Text = "Ignore the following leading articles when sorting by title:";
            // 
            // ArticlesTextBox
            // 
            this.ArticlesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArticlesTextBox.Location = new System.Drawing.Point(16, 208);
            this.ArticlesTextBox.Name = "ArticlesTextBox";
            this.ArticlesTextBox.Size = new System.Drawing.Size(451, 20);
            this.ArticlesTextBox.TabIndex = 4;
            // 
            // ReadmeGroupBox
            // 
            this.ReadmeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadmeGroupBox.Controls.Add(this.ReadmeFixedWidthFontCheckBox);
            this.ReadmeGroupBox.Location = new System.Drawing.Point(8, 1052);
            this.ReadmeGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.ReadmeGroupBox.Name = "ReadmeGroupBox";
            this.ReadmeGroupBox.Size = new System.Drawing.Size(480, 56);
            this.ReadmeGroupBox.TabIndex = 4;
            this.ReadmeGroupBox.TabStop = false;
            this.ReadmeGroupBox.Text = "Readme box";
            // 
            // ReadmeFixedWidthFontCheckBox
            // 
            this.ReadmeFixedWidthFontCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadmeFixedWidthFontCheckBox.Checked = true;
            this.ReadmeFixedWidthFontCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ReadmeFixedWidthFontCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ReadmeFixedWidthFontCheckBox.Name = "ReadmeFixedWidthFontCheckBox";
            this.ReadmeFixedWidthFontCheckBox.Size = new System.Drawing.Size(456, 32);
            this.ReadmeFixedWidthFontCheckBox.TabIndex = 0;
            this.ReadmeFixedWidthFontCheckBox.Text = "Use a fixed-width font when displaying plain text";
            // 
            // ShowOrHideUIElementsGroupBox
            // 
            this.ShowOrHideUIElementsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.ShowWebSearchButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.ShowExitButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.ShowFMListZoomButtonsCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.ShowUninstallButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Location = new System.Drawing.Point(8, 896);
            this.ShowOrHideUIElementsGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.ShowOrHideUIElementsGroupBox.Name = "ShowOrHideUIElementsGroupBox";
            this.ShowOrHideUIElementsGroupBox.Size = new System.Drawing.Size(480, 144);
            this.ShowOrHideUIElementsGroupBox.TabIndex = 3;
            this.ShowOrHideUIElementsGroupBox.TabStop = false;
            this.ShowOrHideUIElementsGroupBox.Text = "Show or hide interface elements";
            // 
            // ShowWebSearchButtonCheckBox
            // 
            this.ShowWebSearchButtonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowWebSearchButtonCheckBox.Checked = true;
            this.ShowWebSearchButtonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowWebSearchButtonCheckBox.Location = new System.Drawing.Point(16, 72);
            this.ShowWebSearchButtonCheckBox.Name = "ShowWebSearchButtonCheckBox";
            this.ShowWebSearchButtonCheckBox.Size = new System.Drawing.Size(456, 32);
            this.ShowWebSearchButtonCheckBox.TabIndex = 2;
            this.ShowWebSearchButtonCheckBox.Text = "Show web search button";
            // 
            // ShowExitButtonCheckBox
            // 
            this.ShowExitButtonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowExitButtonCheckBox.Location = new System.Drawing.Point(16, 100);
            this.ShowExitButtonCheckBox.Name = "ShowExitButtonCheckBox";
            this.ShowExitButtonCheckBox.Size = new System.Drawing.Size(456, 32);
            this.ShowExitButtonCheckBox.TabIndex = 3;
            this.ShowExitButtonCheckBox.Text = "Show exit button";
            // 
            // ShowFMListZoomButtonsCheckBox
            // 
            this.ShowFMListZoomButtonsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowFMListZoomButtonsCheckBox.Checked = true;
            this.ShowFMListZoomButtonsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowFMListZoomButtonsCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ShowFMListZoomButtonsCheckBox.Name = "ShowFMListZoomButtonsCheckBox";
            this.ShowFMListZoomButtonsCheckBox.Size = new System.Drawing.Size(456, 32);
            this.ShowFMListZoomButtonsCheckBox.TabIndex = 0;
            this.ShowFMListZoomButtonsCheckBox.Text = "Show FM list zoom buttons";
            // 
            // ShowUninstallButtonCheckBox
            // 
            this.ShowUninstallButtonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowUninstallButtonCheckBox.Checked = true;
            this.ShowUninstallButtonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowUninstallButtonCheckBox.Location = new System.Drawing.Point(16, 44);
            this.ShowUninstallButtonCheckBox.Name = "ShowUninstallButtonCheckBox";
            this.ShowUninstallButtonCheckBox.Size = new System.Drawing.Size(456, 32);
            this.ShowUninstallButtonCheckBox.TabIndex = 1;
            this.ShowUninstallButtonCheckBox.Text = "Show \"Install FM / Uninstall FM\" button";
            // 
            // VisualThemeGroupBox
            // 
            this.VisualThemeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VisualThemeGroupBox.Controls.Add(this.FollowSystemThemeRadioButton);
            this.VisualThemeGroupBox.Controls.Add(this.DarkThemeRadioButton);
            this.VisualThemeGroupBox.Controls.Add(this.ClassicThemeRadioButton);
            this.VisualThemeGroupBox.Location = new System.Drawing.Point(8, 80);
            this.VisualThemeGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.VisualThemeGroupBox.Name = "VisualThemeGroupBox";
            this.VisualThemeGroupBox.Size = new System.Drawing.Size(480, 104);
            this.VisualThemeGroupBox.TabIndex = 1;
            this.VisualThemeGroupBox.TabStop = false;
            this.VisualThemeGroupBox.Text = "Theme";
            // 
            // FollowSystemThemeRadioButton
            // 
            this.FollowSystemThemeRadioButton.AutoSize = true;
            this.FollowSystemThemeRadioButton.Location = new System.Drawing.Point(16, 72);
            this.FollowSystemThemeRadioButton.Name = "FollowSystemThemeRadioButton";
            this.FollowSystemThemeRadioButton.Size = new System.Drawing.Size(122, 17);
            this.FollowSystemThemeRadioButton.TabIndex = 1;
            this.FollowSystemThemeRadioButton.Text = "Follow system theme";
            // 
            // DarkThemeRadioButton
            // 
            this.DarkThemeRadioButton.AutoSize = true;
            this.DarkThemeRadioButton.Location = new System.Drawing.Point(16, 48);
            this.DarkThemeRadioButton.Name = "DarkThemeRadioButton";
            this.DarkThemeRadioButton.Size = new System.Drawing.Size(48, 17);
            this.DarkThemeRadioButton.TabIndex = 1;
            this.DarkThemeRadioButton.Text = "Dark";
            // 
            // ClassicThemeRadioButton
            // 
            this.ClassicThemeRadioButton.AutoSize = true;
            this.ClassicThemeRadioButton.Checked = true;
            this.ClassicThemeRadioButton.Location = new System.Drawing.Point(16, 24);
            this.ClassicThemeRadioButton.Name = "ClassicThemeRadioButton";
            this.ClassicThemeRadioButton.Size = new System.Drawing.Size(58, 17);
            this.ClassicThemeRadioButton.TabIndex = 0;
            this.ClassicThemeRadioButton.TabStop = true;
            this.ClassicThemeRadioButton.Text = "Classic";
            // 
            // LanguageGroupBox
            // 
            this.LanguageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LanguageGroupBox.Controls.Add(this.LanguageComboBox);
            this.LanguageGroupBox.Location = new System.Drawing.Point(8, 8);
            this.LanguageGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.LanguageGroupBox.Name = "LanguageGroupBox";
            this.LanguageGroupBox.Size = new System.Drawing.Size(480, 60);
            this.LanguageGroupBox.TabIndex = 0;
            this.LanguageGroupBox.TabStop = false;
            this.LanguageGroupBox.Text = "Language";
            // 
            // LanguageComboBox
            // 
            this.LanguageComboBox.FormattingEnabled = true;
            this.LanguageComboBox.Location = new System.Drawing.Point(16, 24);
            this.LanguageComboBox.Name = "LanguageComboBox";
            this.LanguageComboBox.Size = new System.Drawing.Size(184, 21);
            this.LanguageComboBox.TabIndex = 0;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 288);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(480, 8);
            this.DummyAutoScrollPanel.TabIndex = 0;
            // 
            // AppearancePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "AppearancePage";
            this.Size = new System.Drawing.Size(496, 1219);
            this.PagePanel.ResumeLayout(false);
            this.PlayWithoutFMGroupBox.ResumeLayout(false);
            this.PlayWithoutFMGroupBox.PerformLayout();
            this.FMsListGroupBox.ResumeLayout(false);
            this.FMsListGroupBox.PerformLayout();
            this.DateFormatRBPanel.ResumeLayout(false);
            this.DateFormatRBPanel.PerformLayout();
            this.RatingDisplayStyleRBPanel.ResumeLayout(false);
            this.RatingDisplayStyleRBPanel.PerformLayout();
            this.GameOrganizationRBPanel.ResumeLayout(false);
            this.GameOrganizationRBPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecentFMsNumericUpDown)).EndInit();
            this.PreviewDateFLP.ResumeLayout(false);
            this.PreviewDateFLP.PerformLayout();
            this.DateCustomFormatPanel.ResumeLayout(false);
            this.DateCustomFormatPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RatingExamplePictureBox)).EndInit();
            this.ReadmeGroupBox.ResumeLayout(false);
            this.ShowOrHideUIElementsGroupBox.ResumeLayout(false);
            this.VisualThemeGroupBox.ResumeLayout(false);
            this.VisualThemeGroupBox.PerformLayout();
            this.LanguageGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal System.Windows.Forms.Control DummyAutoScrollPanel;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox LanguageGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkComboBoxWithBackingItems LanguageComboBox;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox VisualThemeGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton DarkThemeRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton ClassicThemeRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox ReadmeGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ReadmeFixedWidthFontCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox FMsListGroupBox;
    internal System.Windows.Forms.FlowLayoutPanel PreviewDateFLP;
    internal AngelLoader.Forms.CustomControls.DarkLabel PreviewDateLabel;
    internal System.Windows.Forms.Panel DateCustomFormatPanel;
    internal AngelLoader.Forms.CustomControls.DarkTextBox DateSeparator3TextBox;
    internal AngelLoader.Forms.CustomControls.DarkTextBox DateSeparator2TextBox;
    internal AngelLoader.Forms.CustomControls.DarkComboBox Date1ComboBox;
    internal AngelLoader.Forms.CustomControls.DarkTextBox DateSeparator1TextBox;
    internal AngelLoader.Forms.CustomControls.DarkComboBox Date4ComboBox;
    internal AngelLoader.Forms.CustomControls.DarkComboBox Date2ComboBox;
    internal AngelLoader.Forms.CustomControls.DarkComboBox Date3ComboBox;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton DateCustomRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton DateCurrentCultureLongRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton DateCurrentCultureShortRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox RatingUseStarsCheckBox;
    internal System.Windows.Forms.PictureBox RatingExamplePictureBox;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton RatingFMSelDisplayStyleRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton RatingNDLDisplayStyleRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkLabel DateFormatLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel RatingDisplayStyleLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel SortingLabel;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox MoveArticlesToEndCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox EnableIgnoreArticlesCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkTextBox ArticlesTextBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox UseShortGameTabNamesCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton OrganizeGamesByTabRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton OrganizeGamesInOneListRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkNumericUpDown RecentFMsNumericUpDown;
    internal AngelLoader.Forms.CustomControls.DarkLabel RecentFMsLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel RecentFMsHeaderLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel GameOrganizationLabel;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox ShowOrHideUIElementsGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ShowExitButtonCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ShowFMListZoomButtonsCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ShowUninstallButtonCheckBox;
    internal System.Windows.Forms.Panel RatingDisplayStyleRBPanel;
    internal System.Windows.Forms.Panel GameOrganizationRBPanel;
    internal System.Windows.Forms.Panel DateFormatRBPanel;
    internal CustomControls.DarkGroupBox PlayWithoutFMGroupBox;
    internal CustomControls.DarkRadioButton PlayWithoutFM_MultipleButtonsRadioButton;
    internal CustomControls.DarkRadioButton PlayWithoutFM_SingleButtonRadioButton;
    internal CustomControls.DarkCheckBox ShowWebSearchButtonCheckBox;
    internal CustomControls.DarkRadioButton FollowSystemThemeRadioButton;
}
