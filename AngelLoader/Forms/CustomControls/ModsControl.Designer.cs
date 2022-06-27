#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls
{
    sealed partial class ModsControl
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
            this.components = new System.ComponentModel.Container();
            this.ModsHeaderLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ModsResetFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.ModsDisableNonImportantButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ModsEnableAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ModsShowUberCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ModsDisabledModsTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.ModsDisabledModsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ModsCheckList = new AngelLoader.Forms.CustomControls.DarkCheckList();
            this.ModsAutoScrollDummyPanel = new System.Windows.Forms.Panel();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ModsResetFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // ModsHeaderLabel
            // 
            this.ModsHeaderLabel.AutoSize = true;
            this.ModsHeaderLabel.Location = new System.Drawing.Point(7, 8);
            this.ModsHeaderLabel.Name = "ModsHeaderLabel";
            this.ModsHeaderLabel.Size = new System.Drawing.Size(174, 13);
            this.ModsHeaderLabel.TabIndex = 10;
            this.ModsHeaderLabel.Text = "Enable or disable mods for this FM: ";
            // 
            // ModsResetFLP
            // 
            this.ModsResetFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModsResetFLP.Controls.Add(this.ModsDisableNonImportantButton);
            this.ModsResetFLP.Controls.Add(this.ModsEnableAllButton);
            this.ModsResetFLP.Controls.Add(this.ModsShowUberCheckBox);
            this.ModsResetFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ModsResetFLP.Location = new System.Drawing.Point(7, 216);
            this.ModsResetFLP.Name = "ModsResetFLP";
            this.ModsResetFLP.Size = new System.Drawing.Size(513, 24);
            this.ModsResetFLP.TabIndex = 7;
            this.ModsResetFLP.WrapContents = false;
            // 
            // ModsDisableNonImportantButton
            // 
            this.ModsDisableNonImportantButton.Location = new System.Drawing.Point(438, 0);
            this.ModsDisableNonImportantButton.Margin = new System.Windows.Forms.Padding(0);
            this.ModsDisableNonImportantButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.ModsDisableNonImportantButton.Name = "ModsDisableNonImportantButton";
            this.ModsDisableNonImportantButton.Size = new System.Drawing.Size(75, 23);
            this.ModsDisableNonImportantButton.TabIndex = 2;
            this.ModsDisableNonImportantButton.Text = "Disable all";
            this.ModsDisableNonImportantButton.UseVisualStyleBackColor = true;
            this.ModsDisableNonImportantButton.Click += new System.EventHandler(this.ModsDisableNonImportantButton_Click);
            // 
            // ModsEnableAllButton
            // 
            this.ModsEnableAllButton.Location = new System.Drawing.Point(363, 0);
            this.ModsEnableAllButton.Margin = new System.Windows.Forms.Padding(0);
            this.ModsEnableAllButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.ModsEnableAllButton.Name = "ModsEnableAllButton";
            this.ModsEnableAllButton.Size = new System.Drawing.Size(75, 23);
            this.ModsEnableAllButton.TabIndex = 1;
            this.ModsEnableAllButton.Text = "Enable all";
            this.ModsEnableAllButton.UseVisualStyleBackColor = true;
            this.ModsEnableAllButton.Click += new System.EventHandler(this.ModsEnableAllButton_Click);
            // 
            // ModsShowUberCheckBox
            // 
            this.ModsShowUberCheckBox.AutoSize = true;
            this.ModsShowUberCheckBox.Location = new System.Drawing.Point(261, 3);
            this.ModsShowUberCheckBox.Name = "ModsShowUberCheckBox";
            this.ModsShowUberCheckBox.Size = new System.Drawing.Size(99, 17);
            this.ModsShowUberCheckBox.TabIndex = 0;
            this.ModsShowUberCheckBox.Text = "Show important";
            this.ModsShowUberCheckBox.UseVisualStyleBackColor = true;
            this.ModsShowUberCheckBox.CheckedChanged += new System.EventHandler(this.ModsShowUberCheckBox_CheckedChanged);
            // 
            // ModsDisabledModsTextBox
            // 
            this.ModsDisabledModsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModsDisabledModsTextBox.Location = new System.Drawing.Point(7, 256);
            this.ModsDisabledModsTextBox.Name = "ModsDisabledModsTextBox";
            this.ModsDisabledModsTextBox.Size = new System.Drawing.Size(512, 20);
            this.ModsDisabledModsTextBox.TabIndex = 9;
            this.ModsDisabledModsTextBox.TextChanged += new System.EventHandler(this.ModsDisabledModsTextBox_TextChanged);
            this.ModsDisabledModsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModsDisabledModsTextBox_KeyDown);
            this.ModsDisabledModsTextBox.Leave += new System.EventHandler(this.ModsDisabledModsTextBox_Leave);
            // 
            // ModsDisabledModsLabel
            // 
            this.ModsDisabledModsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ModsDisabledModsLabel.AutoSize = true;
            this.ModsDisabledModsLabel.Location = new System.Drawing.Point(7, 240);
            this.ModsDisabledModsLabel.Name = "ModsDisabledModsLabel";
            this.ModsDisabledModsLabel.Size = new System.Drawing.Size(79, 13);
            this.ModsDisabledModsLabel.TabIndex = 8;
            this.ModsDisabledModsLabel.Text = "Disabled mods:";
            // 
            // ModsCheckList
            // 
            this.ModsCheckList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModsCheckList.AutoScroll = true;
            this.ModsCheckList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ModsCheckList.Location = new System.Drawing.Point(7, 32);
            this.ModsCheckList.Name = "ModsCheckList";
            this.ModsCheckList.Size = new System.Drawing.Size(512, 184);
            this.ModsCheckList.TabIndex = 6;
            this.ModsCheckList.ItemCheckedChanged += new System.EventHandler<AngelLoader.Forms.CustomControls.DarkCheckList.DarkCheckListEventArgs>(this.ModsCheckList_ItemCheckedChanged);
            // 
            // ModsAutoScrollDummyPanel
            // 
            this.ModsAutoScrollDummyPanel.Location = new System.Drawing.Point(7, 8);
            this.ModsAutoScrollDummyPanel.Name = "ModsAutoScrollDummyPanel";
            this.ModsAutoScrollDummyPanel.Size = new System.Drawing.Size(280, 208);
            this.ModsAutoScrollDummyPanel.TabIndex = 11;
            // 
            // ModsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.ModsHeaderLabel);
            this.Controls.Add(this.ModsResetFLP);
            this.Controls.Add(this.ModsDisabledModsTextBox);
            this.Controls.Add(this.ModsDisabledModsLabel);
            this.Controls.Add(this.ModsCheckList);
            this.Controls.Add(this.ModsAutoScrollDummyPanel);
            this.Name = "ModsControl";
            this.Size = new System.Drawing.Size(527, 284);
            this.ModsResetFLP.ResumeLayout(false);
            this.ModsResetFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endif

        #endregion

        internal DarkLabel ModsHeaderLabel;
        internal System.Windows.Forms.FlowLayoutPanel ModsResetFLP;
        internal DarkButton ModsDisableNonImportantButton;
        internal DarkButton ModsEnableAllButton;
        internal DarkCheckBox ModsShowUberCheckBox;
        internal DarkTextBox ModsDisabledModsTextBox;
        internal DarkLabel ModsDisabledModsLabel;
        internal DarkCheckList ModsCheckList;
        internal System.Windows.Forms.Panel ModsAutoScrollDummyPanel;
        internal System.Windows.Forms.ToolTip MainToolTip;
    }
}
