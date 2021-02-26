using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    public sealed partial class FMDisplayPage
    {
        private void InitComponentManual()
        {
            RatingDisplayStyleGroupBox = new DarkGroupBox();
            RatingUseStarsCheckBox = new DarkCheckBox();
            RatingExamplePictureBox = new PictureBox();
            RatingFMSelDisplayStyleRadioButton = new DarkRadioButton();
            RatingNDLDisplayStyleRadioButton = new DarkRadioButton();
            DateFormatGroupBox = new DarkGroupBox();
            PreviewDateFlowLayoutPanel = new FlowLayoutPanel();
            PreviewDateLabel = new DarkLabel();
            DateCustomFormatPanel = new Panel();
            DateSeparator3TextBox = new DarkTextBox();
            DateSeparator2TextBox = new DarkTextBox();
            Date1ComboBox = new DarkComboBox();
            DateSeparator1TextBox = new DarkTextBox();
            Date4ComboBox = new DarkComboBox();
            Date2ComboBox = new DarkComboBox();
            Date3ComboBox = new DarkComboBox();
            DateCustomRadioButton = new DarkRadioButton();
            DateCurrentCultureLongRadioButton = new DarkRadioButton();
            DateCurrentCultureShortRadioButton = new DarkRadioButton();
            SortingGroupBox = new DarkGroupBox();
            MoveArticlesToEndCheckBox = new DarkCheckBox();
            EnableIgnoreArticlesCheckBox = new DarkCheckBox();
            ArticlesTextBox = new DarkTextBox();
            GameOrganizationGroupBox = new DarkGroupBox();
            UseShortGameTabNamesCheckBox = new DarkCheckBox();
            OrganizeGamesByTabRadioButton = new DarkRadioButton();
            OrganizeGamesInOneListRadioButton = new DarkRadioButton();
            PagePanel = new Panel();
            DummyAutoScrollPanel = new Control();
            RecentFMsGroupBox = new DarkGroupBox();
            RecentFMsLabel = new DarkLabel();
            RecentFMsNumericUpDown = new DarkNumericUpDown();
            RatingDisplayStyleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RatingExamplePictureBox).BeginInit();
            DateFormatGroupBox.SuspendLayout();
            PreviewDateFlowLayoutPanel.SuspendLayout();
            DateCustomFormatPanel.SuspendLayout();
            SortingGroupBox.SuspendLayout();
            GameOrganizationGroupBox.SuspendLayout();
            PagePanel.SuspendLayout();
            RecentFMsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RecentFMsNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // RatingDisplayStyleGroupBox
            // 
            RatingDisplayStyleGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            RatingDisplayStyleGroupBox.Controls.Add(RatingUseStarsCheckBox);
            RatingDisplayStyleGroupBox.Controls.Add(RatingExamplePictureBox);
            RatingDisplayStyleGroupBox.Controls.Add(RatingFMSelDisplayStyleRadioButton);
            RatingDisplayStyleGroupBox.Controls.Add(RatingNDLDisplayStyleRadioButton);
            RatingDisplayStyleGroupBox.Location = new Point(8, 256);
            RatingDisplayStyleGroupBox.MinimumSize = new Size(478, 0);
            RatingDisplayStyleGroupBox.Size = new Size(480, 124);
            RatingDisplayStyleGroupBox.TabIndex = 6;
            RatingDisplayStyleGroupBox.TabStop = false;
            // 
            // RatingUseStarsCheckBox
            // 
            RatingUseStarsCheckBox.AutoSize = true;
            RatingUseStarsCheckBox.Checked = true;
            RatingUseStarsCheckBox.Location = new Point(32, 72);
            RatingUseStarsCheckBox.TabIndex = 2;
            RatingUseStarsCheckBox.UseVisualStyleBackColor = true;
            // 
            // RatingExamplePictureBox
            // 
            RatingExamplePictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RatingExamplePictureBox.Location = new Point(395, 92);
            RatingExamplePictureBox.Size = new Size(79, 23);
            RatingExamplePictureBox.TabIndex = 1;
            RatingExamplePictureBox.TabStop = false;
            // 
            // RatingFMSelDisplayStyleRadioButton
            // 
            RatingFMSelDisplayStyleRadioButton.AutoSize = true;
            RatingFMSelDisplayStyleRadioButton.Checked = true;
            RatingFMSelDisplayStyleRadioButton.Location = new Point(16, 48);
            RatingFMSelDisplayStyleRadioButton.TabIndex = 1;
            RatingFMSelDisplayStyleRadioButton.TabStop = true;
            RatingFMSelDisplayStyleRadioButton.UseVisualStyleBackColor = true;
            // 
            // RatingNDLDisplayStyleRadioButton
            // 
            RatingNDLDisplayStyleRadioButton.AutoSize = true;
            RatingNDLDisplayStyleRadioButton.Location = new Point(16, 24);
            RatingNDLDisplayStyleRadioButton.TabIndex = 0;
            RatingNDLDisplayStyleRadioButton.UseVisualStyleBackColor = true;
            // 
            // DateFormatGroupBox
            // 
            DateFormatGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            DateFormatGroupBox.Controls.Add(PreviewDateFlowLayoutPanel);
            DateFormatGroupBox.Controls.Add(DateCustomFormatPanel);
            DateFormatGroupBox.Controls.Add(DateCustomRadioButton);
            DateFormatGroupBox.Controls.Add(DateCurrentCultureLongRadioButton);
            DateFormatGroupBox.Controls.Add(DateCurrentCultureShortRadioButton);
            DateFormatGroupBox.Location = new Point(8, 388);
            DateFormatGroupBox.MinimumSize = new Size(478, 0);
            DateFormatGroupBox.Size = new Size(480, 152);
            DateFormatGroupBox.TabIndex = 7;
            DateFormatGroupBox.TabStop = false;
            // 
            // PreviewDateFlowLayoutPanel
            // 
            PreviewDateFlowLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PreviewDateFlowLayoutPanel.Controls.Add(PreviewDateLabel);
            PreviewDateFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            PreviewDateFlowLayoutPanel.Location = new Point(8, 16);
            PreviewDateFlowLayoutPanel.Size = new Size(464, 16);
            PreviewDateFlowLayoutPanel.TabIndex = 21;
            // 
            // PreviewDateLabel
            // 
            PreviewDateLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PreviewDateLabel.AutoSize = true;
            PreviewDateLabel.Location = new Point(386, 0);
            PreviewDateLabel.TabIndex = 0;
            // 
            // DateCustomFormatPanel
            // 
            DateCustomFormatPanel.Controls.Add(DateSeparator3TextBox);
            DateCustomFormatPanel.Controls.Add(DateSeparator2TextBox);
            DateCustomFormatPanel.Controls.Add(Date1ComboBox);
            DateCustomFormatPanel.Controls.Add(DateSeparator1TextBox);
            DateCustomFormatPanel.Controls.Add(Date4ComboBox);
            DateCustomFormatPanel.Controls.Add(Date2ComboBox);
            DateCustomFormatPanel.Controls.Add(Date3ComboBox);
            DateCustomFormatPanel.Location = new Point(16, 112);
            DateCustomFormatPanel.Size = new Size(448, 24);
            DateCustomFormatPanel.TabIndex = 19;
            // 
            // DateSeparator3TextBox
            // 
            DateSeparator3TextBox.Location = new Point(332, 0);
            DateSeparator3TextBox.Size = new Size(24, 20);
            DateSeparator3TextBox.TabIndex = 5;
            // 
            // DateSeparator2TextBox
            // 
            DateSeparator2TextBox.Location = new Point(212, 0);
            DateSeparator2TextBox.Size = new Size(24, 20);
            DateSeparator2TextBox.TabIndex = 3;
            // 
            // Date1ComboBox
            // 
            Date1ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Date1ComboBox.FormattingEnabled = true;
            Date1ComboBox.Location = new Point(0, 0);
            Date1ComboBox.Size = new Size(88, 21);
            Date1ComboBox.TabIndex = 0;
            // 
            // DateSeparator1TextBox
            // 
            DateSeparator1TextBox.Location = new Point(92, 0);
            DateSeparator1TextBox.Size = new Size(24, 20);
            DateSeparator1TextBox.TabIndex = 1;
            // 
            // Date4ComboBox
            // 
            Date4ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Date4ComboBox.FormattingEnabled = true;
            Date4ComboBox.Location = new Point(360, 0);
            Date4ComboBox.Size = new Size(88, 21);
            Date4ComboBox.TabIndex = 6;
            // 
            // Date2ComboBox
            // 
            Date2ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Date2ComboBox.FormattingEnabled = true;
            Date2ComboBox.Location = new Point(120, 0);
            Date2ComboBox.Size = new Size(88, 21);
            Date2ComboBox.TabIndex = 2;
            // 
            // Date3ComboBox
            // 
            Date3ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Date3ComboBox.FormattingEnabled = true;
            Date3ComboBox.Location = new Point(240, 0);
            Date3ComboBox.Size = new Size(88, 21);
            Date3ComboBox.TabIndex = 4;
            // 
            // DateCustomRadioButton
            // 
            DateCustomRadioButton.AutoSize = true;
            DateCustomRadioButton.Location = new Point(16, 88);
            DateCustomRadioButton.TabIndex = 2;
            DateCustomRadioButton.UseVisualStyleBackColor = true;
            // 
            // DateCurrentCultureLongRadioButton
            // 
            DateCurrentCultureLongRadioButton.AutoSize = true;
            DateCurrentCultureLongRadioButton.Location = new Point(16, 64);
            DateCurrentCultureLongRadioButton.TabIndex = 1;
            DateCurrentCultureLongRadioButton.UseVisualStyleBackColor = true;
            // 
            // DateCurrentCultureShortRadioButton
            // 
            DateCurrentCultureShortRadioButton.AutoSize = true;
            DateCurrentCultureShortRadioButton.Location = new Point(16, 40);
            DateCurrentCultureShortRadioButton.TabIndex = 0;
            DateCurrentCultureShortRadioButton.UseVisualStyleBackColor = true;
            // 
            // SortingGroupBox
            // 
            SortingGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SortingGroupBox.Controls.Add(MoveArticlesToEndCheckBox);
            SortingGroupBox.Controls.Add(EnableIgnoreArticlesCheckBox);
            SortingGroupBox.Controls.Add(ArticlesTextBox);
            SortingGroupBox.Location = new Point(8, 136);
            SortingGroupBox.MinimumSize = new Size(478, 0);
            SortingGroupBox.Size = new Size(480, 112);
            SortingGroupBox.TabIndex = 5;
            SortingGroupBox.TabStop = false;
            // 
            // MoveArticlesToEndCheckBox
            // 
            MoveArticlesToEndCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MoveArticlesToEndCheckBox.Checked = true;
            MoveArticlesToEndCheckBox.Location = new Point(16, 72);
            MoveArticlesToEndCheckBox.Size = new Size(456, 32);
            MoveArticlesToEndCheckBox.TabIndex = 2;
            MoveArticlesToEndCheckBox.UseVisualStyleBackColor = true;
            // 
            // EnableIgnoreArticlesCheckBox
            // 
            EnableIgnoreArticlesCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            EnableIgnoreArticlesCheckBox.Checked = true;
            EnableIgnoreArticlesCheckBox.Location = new Point(16, 16);
            EnableIgnoreArticlesCheckBox.Size = new Size(456, 32);
            EnableIgnoreArticlesCheckBox.TabIndex = 0;
            EnableIgnoreArticlesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ArticlesTextBox
            // 
            ArticlesTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ArticlesTextBox.Location = new Point(16, 48);
            ArticlesTextBox.Size = new Size(451, 20);
            ArticlesTextBox.TabIndex = 1;
            // 
            // GameOrganizationGroupBox
            // 
            GameOrganizationGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GameOrganizationGroupBox.Controls.Add(UseShortGameTabNamesCheckBox);
            GameOrganizationGroupBox.Controls.Add(OrganizeGamesByTabRadioButton);
            GameOrganizationGroupBox.Controls.Add(OrganizeGamesInOneListRadioButton);
            GameOrganizationGroupBox.Location = new Point(8, 8);
            GameOrganizationGroupBox.MinimumSize = new Size(478, 0);
            GameOrganizationGroupBox.Size = new Size(480, 120);
            GameOrganizationGroupBox.TabIndex = 4;
            GameOrganizationGroupBox.TabStop = false;
            // 
            // UseShortGameTabNamesCheckBox
            // 
            UseShortGameTabNamesCheckBox.AutoSize = true;
            UseShortGameTabNamesCheckBox.Location = new Point(40, 56);
            UseShortGameTabNamesCheckBox.TabIndex = 1;
            UseShortGameTabNamesCheckBox.UseVisualStyleBackColor = true;
            // 
            // OrganizeGamesByTabRadioButton
            // 
            OrganizeGamesByTabRadioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OrganizeGamesByTabRadioButton.Location = new Point(16, 16);
            OrganizeGamesByTabRadioButton.Size = new Size(456, 32);
            OrganizeGamesByTabRadioButton.TabIndex = 0;
            OrganizeGamesByTabRadioButton.UseVisualStyleBackColor = true;
            // 
            // OrganizeGamesInOneListRadioButton
            // 
            OrganizeGamesInOneListRadioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OrganizeGamesInOneListRadioButton.Checked = true;
            OrganizeGamesInOneListRadioButton.Location = new Point(16, 80);
            OrganizeGamesInOneListRadioButton.Size = new Size(456, 32);
            OrganizeGamesInOneListRadioButton.TabIndex = 2;
            OrganizeGamesInOneListRadioButton.TabStop = true;
            OrganizeGamesInOneListRadioButton.UseVisualStyleBackColor = true;
            // 
            // PagePanel
            // 
            PagePanel.AutoScroll = true;
            PagePanel.Controls.Add(RecentFMsGroupBox);
            PagePanel.Controls.Add(GameOrganizationGroupBox);
            PagePanel.Controls.Add(RatingDisplayStyleGroupBox);
            PagePanel.Controls.Add(SortingGroupBox);
            PagePanel.Controls.Add(DateFormatGroupBox);
            PagePanel.Controls.Add(DummyAutoScrollPanel);
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new Point(0, 0);
            PagePanel.Size = new Size(496, 640);
            PagePanel.TabIndex = 8;
            // 
            // DummyAutoScrollPanel
            // 
            DummyAutoScrollPanel.Location = new Point(8, 128);
            DummyAutoScrollPanel.Size = new Size(480, 8);
            DummyAutoScrollPanel.TabIndex = 8;
            // 
            // RecentFMsGroupBox
            // 
            RecentFMsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            RecentFMsGroupBox.Controls.Add(RecentFMsNumericUpDown);
            RecentFMsGroupBox.Controls.Add(RecentFMsLabel);
            RecentFMsGroupBox.Location = new Point(8, 552);
            RecentFMsGroupBox.MinimumSize = new Size(478, 0);
            RecentFMsGroupBox.Size = new Size(480, 80);
            RecentFMsGroupBox.TabIndex = 9;
            RecentFMsGroupBox.TabStop = false;
            // 
            // RecentFMsLabel
            // 
            RecentFMsLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            RecentFMsLabel.Location = new Point(16, 16);
            RecentFMsLabel.Size = new Size(456, 32);
            RecentFMsLabel.TabIndex = 0;
            RecentFMsLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // RecentFMsNumericUpDown
            // 
            RecentFMsNumericUpDown.Location = new Point(16, 48);
            RecentFMsNumericUpDown.Size = new Size(56, 20);
            RecentFMsNumericUpDown.TabIndex = 1;
            // 
            // FMDisplayPage
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PagePanel);
            Size = new Size(496, 640);
            RatingDisplayStyleGroupBox.ResumeLayout(false);
            RatingDisplayStyleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RatingExamplePictureBox).EndInit();
            DateFormatGroupBox.ResumeLayout(false);
            DateFormatGroupBox.PerformLayout();
            PreviewDateFlowLayoutPanel.ResumeLayout(false);
            PreviewDateFlowLayoutPanel.PerformLayout();
            DateCustomFormatPanel.ResumeLayout(false);
            DateCustomFormatPanel.PerformLayout();
            SortingGroupBox.ResumeLayout(false);
            SortingGroupBox.PerformLayout();
            GameOrganizationGroupBox.ResumeLayout(false);
            GameOrganizationGroupBox.PerformLayout();
            PagePanel.ResumeLayout(false);
            RecentFMsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)RecentFMsNumericUpDown).EndInit();
            ResumeLayout(false);
        }
    }
}
