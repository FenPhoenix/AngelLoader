#define FenGen_GenSlimDesignerFromThis

namespace AngelLoader.Forms
{
    partial class AppearancePage
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
            this.ReadmeGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ReadmeFixedWidthFontCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ShowOrHideUIElementsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.HideExitButtonCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.HideFMListZoomButtonsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.HideUninstallButtonCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.VisualThemeGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.DarkThemeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.ClassicThemeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.LanguageGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.LanguageComboBox = new AngelLoader.Forms.CustomControls.ComboBoxWithBackingItems();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.PagePanel.SuspendLayout();
            this.ReadmeGroupBox.SuspendLayout();
            this.ShowOrHideUIElementsGroupBox.SuspendLayout();
            this.VisualThemeGroupBox.SuspendLayout();
            this.LanguageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.ReadmeGroupBox);
            this.PagePanel.Controls.Add(this.ShowOrHideUIElementsGroupBox);
            this.PagePanel.Controls.Add(this.VisualThemeGroupBox);
            this.PagePanel.Controls.Add(this.LanguageGroupBox);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(496, 362);
            this.PagePanel.TabIndex = 8;
            // 
            // ReadmeGroupBox
            // 
            this.ReadmeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadmeGroupBox.Controls.Add(this.ReadmeFixedWidthFontCheckBox);
            this.ReadmeGroupBox.Location = new System.Drawing.Point(8, 295);
            this.ReadmeGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.ReadmeGroupBox.Name = "ReadmeGroupBox";
            this.ReadmeGroupBox.Size = new System.Drawing.Size(480, 56);
            this.ReadmeGroupBox.TabIndex = 15;
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
            this.ReadmeFixedWidthFontCheckBox.UseVisualStyleBackColor = true;
            // 
            // ShowOrHideUIElementsGroupBox
            // 
            this.ShowOrHideUIElementsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.HideExitButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.HideFMListZoomButtonsCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.HideUninstallButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Location = new System.Drawing.Point(8, 176);
            this.ShowOrHideUIElementsGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.ShowOrHideUIElementsGroupBox.Name = "ShowOrHideUIElementsGroupBox";
            this.ShowOrHideUIElementsGroupBox.Size = new System.Drawing.Size(480, 107);
            this.ShowOrHideUIElementsGroupBox.TabIndex = 14;
            this.ShowOrHideUIElementsGroupBox.TabStop = false;
            this.ShowOrHideUIElementsGroupBox.Text = "Show or hide interface elements";
            // 
            // HideExitButtonCheckBox
            // 
            this.HideExitButtonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HideExitButtonCheckBox.Checked = true;
            this.HideExitButtonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HideExitButtonCheckBox.Location = new System.Drawing.Point(16, 72);
            this.HideExitButtonCheckBox.Name = "HideExitButtonCheckBox";
            this.HideExitButtonCheckBox.Size = new System.Drawing.Size(456, 32);
            this.HideExitButtonCheckBox.TabIndex = 3;
            this.HideExitButtonCheckBox.Text = "Hide exit button";
            this.HideExitButtonCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideFMListZoomButtonsCheckBox
            // 
            this.HideFMListZoomButtonsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HideFMListZoomButtonsCheckBox.Location = new System.Drawing.Point(16, 44);
            this.HideFMListZoomButtonsCheckBox.Name = "HideFMListZoomButtonsCheckBox";
            this.HideFMListZoomButtonsCheckBox.Size = new System.Drawing.Size(456, 32);
            this.HideFMListZoomButtonsCheckBox.TabIndex = 2;
            this.HideFMListZoomButtonsCheckBox.Text = "Hide FM list zoom buttons";
            this.HideFMListZoomButtonsCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideUninstallButtonCheckBox
            // 
            this.HideUninstallButtonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HideUninstallButtonCheckBox.Location = new System.Drawing.Point(16, 16);
            this.HideUninstallButtonCheckBox.Name = "HideUninstallButtonCheckBox";
            this.HideUninstallButtonCheckBox.Size = new System.Drawing.Size(456, 32);
            this.HideUninstallButtonCheckBox.TabIndex = 1;
            this.HideUninstallButtonCheckBox.Text = "Hide \"Install / Uninstall FM\" button (like FMSel)";
            this.HideUninstallButtonCheckBox.UseVisualStyleBackColor = true;
            // 
            // VisualThemeGroupBox
            // 
            this.VisualThemeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VisualThemeGroupBox.Controls.Add(this.DarkThemeRadioButton);
            this.VisualThemeGroupBox.Controls.Add(this.ClassicThemeRadioButton);
            this.VisualThemeGroupBox.Location = new System.Drawing.Point(8, 80);
            this.VisualThemeGroupBox.MinimumSize = new System.Drawing.Size(478, 0);
            this.VisualThemeGroupBox.Name = "VisualThemeGroupBox";
            this.VisualThemeGroupBox.Size = new System.Drawing.Size(480, 80);
            this.VisualThemeGroupBox.TabIndex = 11;
            this.VisualThemeGroupBox.TabStop = false;
            this.VisualThemeGroupBox.Text = "Theme";
            // 
            // DarkThemeRadioButton
            // 
            this.DarkThemeRadioButton.AutoSize = true;
            this.DarkThemeRadioButton.Location = new System.Drawing.Point(16, 48);
            this.DarkThemeRadioButton.Name = "DarkThemeRadioButton";
            this.DarkThemeRadioButton.Size = new System.Drawing.Size(48, 17);
            this.DarkThemeRadioButton.TabIndex = 0;
            this.DarkThemeRadioButton.Text = "Dark";
            this.DarkThemeRadioButton.UseVisualStyleBackColor = true;
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
            this.ClassicThemeRadioButton.UseVisualStyleBackColor = true;
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
            this.LanguageGroupBox.TabIndex = 10;
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
            this.DummyAutoScrollPanel.TabIndex = 8;
            // 
            // AppearancePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "AppearancePage";
            this.Size = new System.Drawing.Size(496, 362);
            this.PagePanel.ResumeLayout(false);
            this.ReadmeGroupBox.ResumeLayout(false);
            this.ShowOrHideUIElementsGroupBox.ResumeLayout(false);
            this.VisualThemeGroupBox.ResumeLayout(false);
            this.VisualThemeGroupBox.PerformLayout();
            this.LanguageGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
#endif

        #endregion

        internal AngelLoader.Forms.CustomControls.DarkPanel PagePanel;
        internal System.Windows.Forms.Control DummyAutoScrollPanel;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox LanguageGroupBox;
        internal AngelLoader.Forms.CustomControls.ComboBoxWithBackingItems LanguageComboBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox VisualThemeGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton DarkThemeRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton ClassicThemeRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox ReadmeGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox ReadmeFixedWidthFontCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox ShowOrHideUIElementsGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox HideExitButtonCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox HideFMListZoomButtonsCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox HideUninstallButtonCheckBox;
    }
}
