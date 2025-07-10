namespace AngelLoader.Forms;

sealed partial class IOThreadingPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new AngelLoader.Forms.CustomControls.PanelCustom();
        this.ActualPagePanel = new AngelLoader.Forms.CustomControls.PanelCustom();
        this.LayoutFLP = new AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom();
        this.HelpPanel = new AngelLoader.Forms.CustomControls.PanelCustom();
        this.HelpPictureBox = new System.Windows.Forms.PictureBox();
        this.HelpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.IOThreadingLevelGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IOThreadCountGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IOThreadsResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.PagePanel.SuspendLayout();
        this.ActualPagePanel.SuspendLayout();
        this.LayoutFLP.SuspendLayout();
        this.HelpPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).BeginInit();
        this.IOThreadCountGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).BeginInit();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
        this.PagePanel.Controls.Add(this.ActualPagePanel);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 591);
        this.PagePanel.TabIndex = 0;
        // 
        // ActualPagePanel
        // 
        this.ActualPagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ActualPagePanel.Controls.Add(this.LayoutFLP);
        this.ActualPagePanel.Controls.Add(this.IOThreadCountGroupBox);
        this.ActualPagePanel.MinimumSize = new System.Drawing.Size(440, 0);
        this.ActualPagePanel.Size = new System.Drawing.Size(440, 568);
        this.ActualPagePanel.TabIndex = 5;
        // 
        // LayoutFLP
        // 
        this.LayoutFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LayoutFLP.Controls.Add(this.HelpPanel);
        this.LayoutFLP.Controls.Add(this.IOThreadingLevelGroupBox);
        this.LayoutFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.LayoutFLP.Location = new System.Drawing.Point(0, 144);
        this.LayoutFLP.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.LayoutFLP.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
        this.LayoutFLP.Size = new System.Drawing.Size(440, 403);
        this.LayoutFLP.TabIndex = 4;
        this.LayoutFLP.WrapContents = false;
        this.LayoutFLP.Layout += new System.Windows.Forms.LayoutEventHandler(this.LayoutFLP_Layout);
        // 
        // HelpPanel
        // 
        this.HelpPanel.Controls.Add(this.HelpPictureBox);
        this.HelpPanel.Controls.Add(this.HelpLabel);
        this.HelpPanel.Margin = new System.Windows.Forms.Padding(0);
        this.HelpPanel.MinimumSize = new System.Drawing.Size(424, 0);
        this.HelpPanel.Size = new System.Drawing.Size(430, 146);
        this.HelpPanel.TabIndex = 5;
        // 
        // HelpPictureBox
        // 
        this.HelpPictureBox.Location = new System.Drawing.Point(8, 12);
        this.HelpPictureBox.Size = new System.Drawing.Size(16, 16);
        this.HelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        // 
        // HelpLabel
        // 
        this.HelpLabel.AutoSize = true;
        this.HelpLabel.Location = new System.Drawing.Point(28, 14);
        this.HelpLabel.MaximumSize = new System.Drawing.Size(380, 0);
        this.HelpLabel.TextChanged += new System.EventHandler(this.HelpLabel_TextChanged);
        // 
        // IOThreadingLevelGroupBox
        // 
        this.IOThreadingLevelGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadingLevelGroupBox.Size = new System.Drawing.Size(424, 104);
        this.IOThreadingLevelGroupBox.TabIndex = 0;
        this.IOThreadingLevelGroupBox.TabStop = false;
        this.IOThreadingLevelGroupBox.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.IOThreadingLevelGroupBox_PaintCustom);
        // 
        // IOThreadCountGroupBox
        // 
        this.IOThreadCountGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.IOThreadCountGroupBox.Controls.Add(this.IOThreadsResetButton);
        this.IOThreadCountGroupBox.Controls.Add(this.CustomThreadsNumericUpDown);
        this.IOThreadCountGroupBox.Controls.Add(this.CustomThreadsLabel);
        this.IOThreadCountGroupBox.Controls.Add(this.AutoModeRadioButton);
        this.IOThreadCountGroupBox.Controls.Add(this.CustomModeRadioButton);
        this.IOThreadCountGroupBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadCountGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadCountGroupBox.Size = new System.Drawing.Size(424, 128);
        this.IOThreadCountGroupBox.TabIndex = 0;
        this.IOThreadCountGroupBox.TabStop = false;
        // 
        // IOThreadsResetButton
        // 
        this.IOThreadsResetButton.Location = new System.Drawing.Point(136, 88);
        this.IOThreadsResetButton.Size = new System.Drawing.Size(22, 22);
        this.IOThreadsResetButton.TabIndex = 9;
        this.IOThreadsResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.IOThreadsResetButton_PaintCustom);
        // 
        // CustomThreadsNumericUpDown
        // 
        this.CustomThreadsNumericUpDown.Location = new System.Drawing.Point(30, 89);
        this.CustomThreadsNumericUpDown.Maximum = new decimal(new int[] {
        2147483647,
        0,
        0,
        0});
        this.CustomThreadsNumericUpDown.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.CustomThreadsNumericUpDown.Size = new System.Drawing.Size(104, 20);
        this.CustomThreadsNumericUpDown.TabIndex = 8;
        this.CustomThreadsNumericUpDown.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // CustomThreadsLabel
        // 
        this.CustomThreadsLabel.AutoSize = true;
        this.CustomThreadsLabel.Location = new System.Drawing.Point(30, 73);
        // 
        // AutoModeRadioButton
        // 
        this.AutoModeRadioButton.AutoSize = true;
        this.AutoModeRadioButton.Checked = true;
        this.AutoModeRadioButton.Location = new System.Drawing.Point(14, 25);
        this.AutoModeRadioButton.TabIndex = 5;
        this.AutoModeRadioButton.TabStop = true;
        // 
        // CustomModeRadioButton
        // 
        this.CustomModeRadioButton.AutoSize = true;
        this.CustomModeRadioButton.Location = new System.Drawing.Point(14, 49);
        this.CustomModeRadioButton.TabIndex = 6;
        this.CustomModeRadioButton.TabStop = true;
        // 
        // IOThreadingPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 591);
        this.PagePanel.ResumeLayout(false);
        this.ActualPagePanel.ResumeLayout(false);
        this.LayoutFLP.ResumeLayout(false);
        this.HelpPanel.ResumeLayout(false);
        this.HelpPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).EndInit();
        this.IOThreadCountGroupBox.ResumeLayout(false);
        this.IOThreadCountGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
