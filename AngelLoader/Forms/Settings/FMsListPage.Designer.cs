#define FenGen_GenSlimDesignerFromThis

namespace AngelLoader.Forms
{
    partial class FMsListPage
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
            this.PagePanel = new AngelLoader.Forms.CustomControls.DarkPanel();
            this.DateFormatGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.DateFormatRBPanel = new System.Windows.Forms.Panel();
            this.DateCurrentCultureShortRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DateCurrentCultureLongRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DateCustomRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.DateCustomFormatPanel = new System.Windows.Forms.Panel();
            this.DateSeparator3TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.DateSeparator2TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.Date1ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.DateSeparator1TextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.Date4ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.Date2ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.Date3ComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.PreviewDateFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.PreviewDateLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.RatingDisplayStyleGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.RatingNDLDisplayStyleRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RatingFMSelDisplayStyleRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RatingUseStarsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.RatingExamplePictureBox = new System.Windows.Forms.PictureBox();
            this.SortingGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.EnableIgnoreArticlesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ArticlesTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.MoveArticlesToEndCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.GameOrganizationGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.OrganizeGamesByTabRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.OrganizeGamesInOneListRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.UseShortGameTabNamesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.RecentFMsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.RecentFMsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.RecentFMsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.PagePanel.SuspendLayout();
            this.DateFormatGroupBox.SuspendLayout();
            this.DateFormatRBPanel.SuspendLayout();
            this.DateCustomFormatPanel.SuspendLayout();
            this.PreviewDateFlowLayoutPanel.SuspendLayout();
            this.RatingDisplayStyleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RatingExamplePictureBox)).BeginInit();
            this.SortingGroupBox.SuspendLayout();
            this.GameOrganizationGroupBox.SuspendLayout();
            this.RecentFMsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecentFMsNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.DateFormatGroupBox);
            this.PagePanel.Controls.Add(this.RatingDisplayStyleGroupBox);
            this.PagePanel.Controls.Add(this.SortingGroupBox);
            this.PagePanel.Controls.Add(this.GameOrganizationGroupBox);
            this.PagePanel.Controls.Add(this.RecentFMsGroupBox);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(496, 649);
            this.PagePanel.TabIndex = 8;
            // 
            // DateFormatGroupBox
            // 
            this.DateFormatGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DateFormatGroupBox.Controls.Add(this.DateFormatRBPanel);
            this.DateFormatGroupBox.Controls.Add(this.DateCustomFormatPanel);
            this.DateFormatGroupBox.Controls.Add(this.PreviewDateFlowLayoutPanel);
            this.DateFormatGroupBox.Location = new System.Drawing.Point(8, 392);
            this.DateFormatGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.DateFormatGroupBox.Name = "DateFormatGroupBox";
            this.DateFormatGroupBox.Size = new System.Drawing.Size(480, 160);
            this.DateFormatGroupBox.TabIndex = 35;
            this.DateFormatGroupBox.TabStop = false;
            this.DateFormatGroupBox.Text = "Date format";
            // 
            // DateFormatRBPanel
            // 
            this.DateFormatRBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DateFormatRBPanel.Controls.Add(this.DateCurrentCultureShortRadioButton);
            this.DateFormatRBPanel.Controls.Add(this.DateCurrentCultureLongRadioButton);
            this.DateFormatRBPanel.Controls.Add(this.DateCustomRadioButton);
            this.DateFormatRBPanel.Location = new System.Drawing.Point(16, 48);
            this.DateFormatRBPanel.Name = "DateFormatRBPanel";
            this.DateFormatRBPanel.Size = new System.Drawing.Size(448, 72);
            this.DateFormatRBPanel.TabIndex = 31;
            // 
            // DateCurrentCultureShortRadioButton
            // 
            this.DateCurrentCultureShortRadioButton.AutoSize = true;
            this.DateCurrentCultureShortRadioButton.Location = new System.Drawing.Point(0, 3);
            this.DateCurrentCultureShortRadioButton.Name = "DateCurrentCultureShortRadioButton";
            this.DateCurrentCultureShortRadioButton.Size = new System.Drawing.Size(119, 17);
            this.DateCurrentCultureShortRadioButton.TabIndex = 22;
            this.DateCurrentCultureShortRadioButton.Text = "System locale, short";
            this.DateCurrentCultureShortRadioButton.UseVisualStyleBackColor = true;
            // 
            // DateCurrentCultureLongRadioButton
            // 
            this.DateCurrentCultureLongRadioButton.AutoSize = true;
            this.DateCurrentCultureLongRadioButton.Location = new System.Drawing.Point(0, 27);
            this.DateCurrentCultureLongRadioButton.Name = "DateCurrentCultureLongRadioButton";
            this.DateCurrentCultureLongRadioButton.Size = new System.Drawing.Size(116, 17);
            this.DateCurrentCultureLongRadioButton.TabIndex = 23;
            this.DateCurrentCultureLongRadioButton.Text = "System locale, long";
            this.DateCurrentCultureLongRadioButton.UseVisualStyleBackColor = true;
            // 
            // DateCustomRadioButton
            // 
            this.DateCustomRadioButton.AutoSize = true;
            this.DateCustomRadioButton.Location = new System.Drawing.Point(0, 51);
            this.DateCustomRadioButton.Name = "DateCustomRadioButton";
            this.DateCustomRadioButton.Size = new System.Drawing.Size(63, 17);
            this.DateCustomRadioButton.TabIndex = 24;
            this.DateCustomRadioButton.Text = "Custom:";
            this.DateCustomRadioButton.UseVisualStyleBackColor = true;
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
            this.DateCustomFormatPanel.Location = new System.Drawing.Point(16, 124);
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
            this.Date4ComboBox.TabIndex = 6;
            // 
            // Date2ComboBox
            // 
            this.Date2ComboBox.FormattingEnabled = true;
            this.Date2ComboBox.Location = new System.Drawing.Point(120, 0);
            this.Date2ComboBox.Name = "Date2ComboBox";
            this.Date2ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date2ComboBox.TabIndex = 2;
            // 
            // Date3ComboBox
            // 
            this.Date3ComboBox.FormattingEnabled = true;
            this.Date3ComboBox.Location = new System.Drawing.Point(240, 0);
            this.Date3ComboBox.Name = "Date3ComboBox";
            this.Date3ComboBox.Size = new System.Drawing.Size(88, 21);
            this.Date3ComboBox.TabIndex = 4;
            // 
            // PreviewDateFlowLayoutPanel
            // 
            this.PreviewDateFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewDateFlowLayoutPanel.Controls.Add(this.PreviewDateLabel);
            this.PreviewDateFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.PreviewDateFlowLayoutPanel.Location = new System.Drawing.Point(16, 24);
            this.PreviewDateFlowLayoutPanel.Name = "PreviewDateFlowLayoutPanel";
            this.PreviewDateFlowLayoutPanel.Size = new System.Drawing.Size(456, 16);
            this.PreviewDateFlowLayoutPanel.TabIndex = 26;
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
            // RatingDisplayStyleGroupBox
            // 
            this.RatingDisplayStyleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RatingDisplayStyleGroupBox.Controls.Add(this.RatingNDLDisplayStyleRadioButton);
            this.RatingDisplayStyleGroupBox.Controls.Add(this.RatingFMSelDisplayStyleRadioButton);
            this.RatingDisplayStyleGroupBox.Controls.Add(this.RatingUseStarsCheckBox);
            this.RatingDisplayStyleGroupBox.Controls.Add(this.RatingExamplePictureBox);
            this.RatingDisplayStyleGroupBox.Location = new System.Drawing.Point(8, 256);
            this.RatingDisplayStyleGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.RatingDisplayStyleGroupBox.Name = "RatingDisplayStyleGroupBox";
            this.RatingDisplayStyleGroupBox.Size = new System.Drawing.Size(480, 128);
            this.RatingDisplayStyleGroupBox.TabIndex = 34;
            this.RatingDisplayStyleGroupBox.TabStop = false;
            this.RatingDisplayStyleGroupBox.Text = "Rating display style";
            // 
            // RatingNDLDisplayStyleRadioButton
            // 
            this.RatingNDLDisplayStyleRadioButton.AutoSize = true;
            this.RatingNDLDisplayStyleRadioButton.Location = new System.Drawing.Point(16, 24);
            this.RatingNDLDisplayStyleRadioButton.Name = "RatingNDLDisplayStyleRadioButton";
            this.RatingNDLDisplayStyleRadioButton.Size = new System.Drawing.Size(219, 17);
            this.RatingNDLDisplayStyleRadioButton.TabIndex = 7;
            this.RatingNDLDisplayStyleRadioButton.Text = "NewDarkLoader (0-10 in increments of 1)";
            this.RatingNDLDisplayStyleRadioButton.UseVisualStyleBackColor = true;
            // 
            // RatingFMSelDisplayStyleRadioButton
            // 
            this.RatingFMSelDisplayStyleRadioButton.AutoSize = true;
            this.RatingFMSelDisplayStyleRadioButton.Checked = true;
            this.RatingFMSelDisplayStyleRadioButton.Location = new System.Drawing.Point(16, 48);
            this.RatingFMSelDisplayStyleRadioButton.Name = "RatingFMSelDisplayStyleRadioButton";
            this.RatingFMSelDisplayStyleRadioButton.Size = new System.Drawing.Size(174, 17);
            this.RatingFMSelDisplayStyleRadioButton.TabIndex = 9;
            this.RatingFMSelDisplayStyleRadioButton.TabStop = true;
            this.RatingFMSelDisplayStyleRadioButton.Text = "FMSel (0-5 in increments of 0.5)";
            this.RatingFMSelDisplayStyleRadioButton.UseVisualStyleBackColor = true;
            // 
            // RatingUseStarsCheckBox
            // 
            this.RatingUseStarsCheckBox.AutoSize = true;
            this.RatingUseStarsCheckBox.Checked = true;
            this.RatingUseStarsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RatingUseStarsCheckBox.Location = new System.Drawing.Point(32, 72);
            this.RatingUseStarsCheckBox.Name = "RatingUseStarsCheckBox";
            this.RatingUseStarsCheckBox.Size = new System.Drawing.Size(70, 17);
            this.RatingUseStarsCheckBox.TabIndex = 10;
            this.RatingUseStarsCheckBox.Text = "Use stars";
            this.RatingUseStarsCheckBox.UseVisualStyleBackColor = true;
            // 
            // RatingExamplePictureBox
            // 
            this.RatingExamplePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RatingExamplePictureBox.Location = new System.Drawing.Point(392, 91);
            this.RatingExamplePictureBox.Name = "RatingExamplePictureBox";
            this.RatingExamplePictureBox.Size = new System.Drawing.Size(79, 23);
            this.RatingExamplePictureBox.TabIndex = 8;
            this.RatingExamplePictureBox.TabStop = false;
            // 
            // SortingGroupBox
            // 
            this.SortingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SortingGroupBox.Controls.Add(this.EnableIgnoreArticlesCheckBox);
            this.SortingGroupBox.Controls.Add(this.ArticlesTextBox);
            this.SortingGroupBox.Controls.Add(this.MoveArticlesToEndCheckBox);
            this.SortingGroupBox.Location = new System.Drawing.Point(8, 136);
            this.SortingGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.SortingGroupBox.Name = "SortingGroupBox";
            this.SortingGroupBox.Size = new System.Drawing.Size(480, 112);
            this.SortingGroupBox.TabIndex = 33;
            this.SortingGroupBox.TabStop = false;
            this.SortingGroupBox.Text = "Sorting";
            // 
            // EnableIgnoreArticlesCheckBox
            // 
            this.EnableIgnoreArticlesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EnableIgnoreArticlesCheckBox.Checked = true;
            this.EnableIgnoreArticlesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnableIgnoreArticlesCheckBox.Location = new System.Drawing.Point(16, 16);
            this.EnableIgnoreArticlesCheckBox.Name = "EnableIgnoreArticlesCheckBox";
            this.EnableIgnoreArticlesCheckBox.Size = new System.Drawing.Size(456, 32);
            this.EnableIgnoreArticlesCheckBox.TabIndex = 3;
            this.EnableIgnoreArticlesCheckBox.Text = "Ignore the following leading articles when sorting by title:";
            this.EnableIgnoreArticlesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ArticlesTextBox
            // 
            this.ArticlesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArticlesTextBox.Location = new System.Drawing.Point(16, 48);
            this.ArticlesTextBox.Name = "ArticlesTextBox";
            this.ArticlesTextBox.Size = new System.Drawing.Size(459, 20);
            this.ArticlesTextBox.TabIndex = 4;
            // 
            // MoveArticlesToEndCheckBox
            // 
            this.MoveArticlesToEndCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveArticlesToEndCheckBox.Checked = true;
            this.MoveArticlesToEndCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MoveArticlesToEndCheckBox.Location = new System.Drawing.Point(16, 72);
            this.MoveArticlesToEndCheckBox.Name = "MoveArticlesToEndCheckBox";
            this.MoveArticlesToEndCheckBox.Size = new System.Drawing.Size(456, 32);
            this.MoveArticlesToEndCheckBox.TabIndex = 5;
            this.MoveArticlesToEndCheckBox.Text = "Move articles to the end of names when displaying them";
            this.MoveArticlesToEndCheckBox.UseVisualStyleBackColor = true;
            // 
            // GameOrganizationGroupBox
            // 
            this.GameOrganizationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameOrganizationGroupBox.Controls.Add(this.OrganizeGamesByTabRadioButton);
            this.GameOrganizationGroupBox.Controls.Add(this.OrganizeGamesInOneListRadioButton);
            this.GameOrganizationGroupBox.Controls.Add(this.UseShortGameTabNamesCheckBox);
            this.GameOrganizationGroupBox.Location = new System.Drawing.Point(8, 8);
            this.GameOrganizationGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.GameOrganizationGroupBox.Name = "GameOrganizationGroupBox";
            this.GameOrganizationGroupBox.Size = new System.Drawing.Size(480, 120);
            this.GameOrganizationGroupBox.TabIndex = 17;
            this.GameOrganizationGroupBox.TabStop = false;
            this.GameOrganizationGroupBox.Text = "Game organization";
            // 
            // OrganizeGamesByTabRadioButton
            // 
            this.OrganizeGamesByTabRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrganizeGamesByTabRadioButton.Location = new System.Drawing.Point(16, 16);
            this.OrganizeGamesByTabRadioButton.Name = "OrganizeGamesByTabRadioButton";
            this.OrganizeGamesByTabRadioButton.Size = new System.Drawing.Size(456, 32);
            this.OrganizeGamesByTabRadioButton.TabIndex = 0;
            this.OrganizeGamesByTabRadioButton.Text = "Each game in its own tab";
            this.OrganizeGamesByTabRadioButton.UseVisualStyleBackColor = true;
            // 
            // OrganizeGamesInOneListRadioButton
            // 
            this.OrganizeGamesInOneListRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrganizeGamesInOneListRadioButton.Checked = true;
            this.OrganizeGamesInOneListRadioButton.Location = new System.Drawing.Point(16, 80);
            this.OrganizeGamesInOneListRadioButton.Name = "OrganizeGamesInOneListRadioButton";
            this.OrganizeGamesInOneListRadioButton.Size = new System.Drawing.Size(456, 32);
            this.OrganizeGamesInOneListRadioButton.TabIndex = 2;
            this.OrganizeGamesInOneListRadioButton.TabStop = true;
            this.OrganizeGamesInOneListRadioButton.Text = "Everything in one list, and games are filters";
            this.OrganizeGamesInOneListRadioButton.UseVisualStyleBackColor = true;
            // 
            // UseShortGameTabNamesCheckBox
            // 
            this.UseShortGameTabNamesCheckBox.AutoSize = true;
            this.UseShortGameTabNamesCheckBox.Location = new System.Drawing.Point(40, 56);
            this.UseShortGameTabNamesCheckBox.Name = "UseShortGameTabNamesCheckBox";
            this.UseShortGameTabNamesCheckBox.Size = new System.Drawing.Size(172, 17);
            this.UseShortGameTabNamesCheckBox.TabIndex = 1;
            this.UseShortGameTabNamesCheckBox.Text = "Use short names on game tabs";
            this.UseShortGameTabNamesCheckBox.UseVisualStyleBackColor = true;
            // 
            // RecentFMsGroupBox
            // 
            this.RecentFMsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RecentFMsGroupBox.Controls.Add(this.RecentFMsNumericUpDown);
            this.RecentFMsGroupBox.Controls.Add(this.RecentFMsLabel);
            this.RecentFMsGroupBox.Location = new System.Drawing.Point(8, 560);
            this.RecentFMsGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.RecentFMsGroupBox.Name = "RecentFMsGroupBox";
            this.RecentFMsGroupBox.Size = new System.Drawing.Size(480, 80);
            this.RecentFMsGroupBox.TabIndex = 16;
            this.RecentFMsGroupBox.TabStop = false;
            this.RecentFMsGroupBox.Text = "Recent FMs";
            // 
            // RecentFMsNumericUpDown
            // 
            this.RecentFMsNumericUpDown.Location = new System.Drawing.Point(16, 48);
            this.RecentFMsNumericUpDown.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.RecentFMsNumericUpDown.Name = "RecentFMsNumericUpDown";
            this.RecentFMsNumericUpDown.Size = new System.Drawing.Size(56, 20);
            this.RecentFMsNumericUpDown.TabIndex = 28;
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
            this.RecentFMsLabel.Location = new System.Drawing.Point(16, 16);
            this.RecentFMsLabel.Name = "RecentFMsLabel";
            this.RecentFMsLabel.Size = new System.Drawing.Size(456, 32);
            this.RecentFMsLabel.TabIndex = 27;
            this.RecentFMsLabel.Text = "Maximum number of days to consider an FM \"recent\":";
            this.RecentFMsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 248);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(480, 8);
            this.DummyAutoScrollPanel.TabIndex = 8;
            // 
            // FMsListPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "FMsListPage";
            this.Size = new System.Drawing.Size(496, 649);
            this.PagePanel.ResumeLayout(false);
            this.DateFormatGroupBox.ResumeLayout(false);
            this.DateFormatRBPanel.ResumeLayout(false);
            this.DateFormatRBPanel.PerformLayout();
            this.DateCustomFormatPanel.ResumeLayout(false);
            this.DateCustomFormatPanel.PerformLayout();
            this.PreviewDateFlowLayoutPanel.ResumeLayout(false);
            this.PreviewDateFlowLayoutPanel.PerformLayout();
            this.RatingDisplayStyleGroupBox.ResumeLayout(false);
            this.RatingDisplayStyleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RatingExamplePictureBox)).EndInit();
            this.SortingGroupBox.ResumeLayout(false);
            this.SortingGroupBox.PerformLayout();
            this.GameOrganizationGroupBox.ResumeLayout(false);
            this.GameOrganizationGroupBox.PerformLayout();
            this.RecentFMsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RecentFMsNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }
#endif

        #endregion

        internal AngelLoader.Forms.CustomControls.DarkPanel PagePanel;
        internal System.Windows.Forms.Control DummyAutoScrollPanel;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox RecentFMsGroupBox;
        internal System.Windows.Forms.FlowLayoutPanel PreviewDateFlowLayoutPanel;
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
        internal AngelLoader.Forms.CustomControls.DarkCheckBox MoveArticlesToEndCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox EnableIgnoreArticlesCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkTextBox ArticlesTextBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox UseShortGameTabNamesCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton OrganizeGamesByTabRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton OrganizeGamesInOneListRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkNumericUpDown RecentFMsNumericUpDown;
        internal AngelLoader.Forms.CustomControls.DarkLabel RecentFMsLabel;
        internal System.Windows.Forms.Panel DateFormatRBPanel;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox GameOrganizationGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox SortingGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox DateFormatGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox RatingDisplayStyleGroupBox;
    }
}
