namespace AngelLoader.Forms;

sealed partial class IOThreadingPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.IOThreadingLevelGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IOThreadCountBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IOThreadsResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.PagePanel.SuspendLayout();
        this.IOThreadCountBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).BeginInit();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
        this.PagePanel.Controls.Add(this.IOThreadingLevelGroupBox);
        this.PagePanel.Controls.Add(this.IOThreadCountBox);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.TabIndex = 0;
        // 
        // IOThreadingLevelGroupBox
        // 
        this.IOThreadingLevelGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.IOThreadingLevelGroupBox.Location = new System.Drawing.Point(8, 148);
        this.IOThreadingLevelGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadingLevelGroupBox.Size = new System.Drawing.Size(424, 104);
        this.IOThreadingLevelGroupBox.TabIndex = 0;
        this.IOThreadingLevelGroupBox.TabStop = false;
        this.IOThreadingLevelGroupBox.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.IOThreadingLevelGroupBox_PaintCustom);
        // 
        // IOThreadCountBox
        // 
        this.IOThreadCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.IOThreadCountBox.Controls.Add(this.IOThreadsResetButton);
        this.IOThreadCountBox.Controls.Add(this.CustomThreadsNumericUpDown);
        this.IOThreadCountBox.Controls.Add(this.CustomThreadsLabel);
        this.IOThreadCountBox.Controls.Add(this.AutoModeRadioButton);
        this.IOThreadCountBox.Controls.Add(this.CustomModeRadioButton);
        this.IOThreadCountBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadCountBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadCountBox.Size = new System.Drawing.Size(424, 128);
        this.IOThreadCountBox.TabIndex = 0;
        this.IOThreadCountBox.TabStop = false;
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
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.IOThreadCountBox.ResumeLayout(false);
        this.IOThreadCountBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
