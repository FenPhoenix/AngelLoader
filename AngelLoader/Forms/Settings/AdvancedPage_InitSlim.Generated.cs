namespace AngelLoader.Forms;

sealed partial class AdvancedPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.IOThreadingGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.CustomModePanel = new System.Windows.Forms.Panel();
        this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.PagePanel.SuspendLayout();
        this.IOThreadingGroupBox.SuspendLayout();
        this.CustomModePanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).BeginInit();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
        this.PagePanel.Controls.Add(this.IOThreadingGroupBox);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.TabIndex = 0;
        // 
        // IOThreadingGroupBox
        // 
        this.IOThreadingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.IOThreadingGroupBox.Controls.Add(this.CustomModePanel);
        this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 336);
        this.IOThreadingGroupBox.TabIndex = 0;
        this.IOThreadingGroupBox.TabStop = false;
        this.IOThreadingGroupBox.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.IOThreadingGroupBox_PaintCustom);
        // 
        // CustomModePanel
        // 
        this.CustomModePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CustomModePanel.Controls.Add(this.CustomThreadsNumericUpDown);
        this.CustomModePanel.Controls.Add(this.CustomThreadsLabel);
        this.CustomModePanel.Controls.Add(this.AutoModeRadioButton);
        this.CustomModePanel.Controls.Add(this.CustomModeRadioButton);
        this.CustomModePanel.Location = new System.Drawing.Point(8, 16);
        this.CustomModePanel.Size = new System.Drawing.Size(408, 104);
        this.CustomModePanel.TabIndex = 0;
        // 
        // CustomThreadsNumericUpDown
        // 
        this.CustomThreadsNumericUpDown.Location = new System.Drawing.Point(24, 72);
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
        this.CustomThreadsNumericUpDown.TabIndex = 3;
        this.CustomThreadsNumericUpDown.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // CustomThreadsLabel
        // 
        this.CustomThreadsLabel.AutoSize = true;
        this.CustomThreadsLabel.Location = new System.Drawing.Point(24, 56);
        this.CustomThreadsLabel.UseMnemonic = false;
        // 
        // AutoModeRadioButton
        // 
        this.AutoModeRadioButton.AutoSize = true;
        this.AutoModeRadioButton.Checked = true;
        this.AutoModeRadioButton.Location = new System.Drawing.Point(8, 8);
        this.AutoModeRadioButton.TabIndex = 0;
        this.AutoModeRadioButton.TabStop = true;
        this.AutoModeRadioButton.UseMnemonic = false;
        this.AutoModeRadioButton.UseVisualStyleBackColor = true;
        // 
        // CustomModeRadioButton
        // 
        this.CustomModeRadioButton.AutoSize = true;
        this.CustomModeRadioButton.Location = new System.Drawing.Point(8, 32);
        this.CustomModeRadioButton.TabIndex = 1;
        this.CustomModeRadioButton.TabStop = true;
        this.CustomModeRadioButton.UseMnemonic = false;
        this.CustomModeRadioButton.UseVisualStyleBackColor = true;
        // 
        // AdvancedPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.IOThreadingGroupBox.ResumeLayout(false);
        this.CustomModePanel.ResumeLayout(false);
        this.CustomModePanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
