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
        this.CustomThreadingModeAggressiveRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.CustomThreadingModeNormalRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.CustomThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.NvmeSsdThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SataSsdThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.HddThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.NvmeSsdThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.SataSsdThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.AutoModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.HddThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.NvmeSsdModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.HddModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.SataSsdModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
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
        this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadingModeLabel);
        this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadingModeLabel);
        this.IOThreadingGroupBox.Controls.Add(this.AutoThreadingModeLabel);
        this.IOThreadingGroupBox.Controls.Add(this.HddThreadingModeLabel);
        this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadsLabel);
        this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadsLabel);
        this.IOThreadingGroupBox.Controls.Add(this.CustomModeRadioButton);
        this.IOThreadingGroupBox.Controls.Add(this.AutoModeLabel);
        this.IOThreadingGroupBox.Controls.Add(this.AutoThreadsLabel);
        this.IOThreadingGroupBox.Controls.Add(this.HddThreadsLabel);
        this.IOThreadingGroupBox.Controls.Add(this.AutoModeRadioButton);
        this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdModeRadioButton);
        this.IOThreadingGroupBox.Controls.Add(this.HddModeRadioButton);
        this.IOThreadingGroupBox.Controls.Add(this.SataSsdModeRadioButton);
        this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 496);
        this.IOThreadingGroupBox.TabIndex = 0;
        this.IOThreadingGroupBox.TabStop = false;
        // 
        // CustomModePanel
        // 
        this.CustomModePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CustomModePanel.Controls.Add(this.CustomThreadingModeAggressiveRadioButton);
        this.CustomModePanel.Controls.Add(this.CustomThreadingModeLabel);
        this.CustomModePanel.Controls.Add(this.CustomThreadingModeNormalRadioButton);
        this.CustomModePanel.Controls.Add(this.CustomThreadsNumericUpDown);
        this.CustomModePanel.Controls.Add(this.CustomThreadsLabel);
        this.CustomModePanel.Location = new System.Drawing.Point(8, 384);
        this.CustomModePanel.Size = new System.Drawing.Size(408, 104);
        this.CustomModePanel.TabIndex = 1;
        // 
        // CustomThreadingModeAggressiveRadioButton
        // 
        this.CustomThreadingModeAggressiveRadioButton.AutoSize = true;
        this.CustomThreadingModeAggressiveRadioButton.Location = new System.Drawing.Point(24, 80);
        this.CustomThreadingModeAggressiveRadioButton.TabIndex = 0;
        this.CustomThreadingModeAggressiveRadioButton.TabStop = true;
        // 
        // CustomThreadingModeNormalRadioButton
        // 
        this.CustomThreadingModeNormalRadioButton.AutoSize = true;
        this.CustomThreadingModeNormalRadioButton.Location = new System.Drawing.Point(24, 64);
        this.CustomThreadingModeNormalRadioButton.TabIndex = 0;
        this.CustomThreadingModeNormalRadioButton.TabStop = true;
        // 
        // CustomThreadingModeLabel
        // 
        this.CustomThreadingModeLabel.AutoSize = true;
        this.CustomThreadingModeLabel.Location = new System.Drawing.Point(24, 48);
        // 
        // NvmeSsdThreadingModeLabel
        // 
        this.NvmeSsdThreadingModeLabel.AutoSize = true;
        this.NvmeSsdThreadingModeLabel.Location = new System.Drawing.Point(32, 320);
        // 
        // SataSsdThreadingModeLabel
        // 
        this.SataSsdThreadingModeLabel.AutoSize = true;
        this.SataSsdThreadingModeLabel.Location = new System.Drawing.Point(32, 240);
        // 
        // AutoThreadingModeLabel
        // 
        this.AutoThreadingModeLabel.AutoSize = true;
        this.AutoThreadingModeLabel.Location = new System.Drawing.Point(24, 80);
        // 
        // HddThreadingModeLabel
        // 
        this.HddThreadingModeLabel.AutoSize = true;
        this.HddThreadingModeLabel.Location = new System.Drawing.Point(32, 160);
        // 
        // CustomThreadsLabel
        // 
        this.CustomThreadsLabel.AutoSize = true;
        this.CustomThreadsLabel.Location = new System.Drawing.Point(24, 0);
        // 
        // NvmeSsdThreadsLabel
        // 
        this.NvmeSsdThreadsLabel.AutoSize = true;
        this.NvmeSsdThreadsLabel.Location = new System.Drawing.Point(32, 304);
        // 
        // CustomThreadsNumericUpDown
        // 
        this.CustomThreadsNumericUpDown.Location = new System.Drawing.Point(24, 16);
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
        this.CustomThreadsNumericUpDown.TabIndex = 2;
        this.CustomThreadsNumericUpDown.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // SataSsdThreadsLabel
        // 
        this.SataSsdThreadsLabel.AutoSize = true;
        this.SataSsdThreadsLabel.Location = new System.Drawing.Point(32, 224);
        // 
        // CustomModeRadioButton
        // 
        this.CustomModeRadioButton.AutoSize = true;
        this.CustomModeRadioButton.Location = new System.Drawing.Point(16, 360);
        this.CustomModeRadioButton.TabIndex = 1;
        this.CustomModeRadioButton.TabStop = true;
        // 
        // AutoModeLabel
        // 
        this.AutoModeLabel.AutoSize = true;
        this.AutoModeLabel.Location = new System.Drawing.Point(24, 48);
        // 
        // AutoThreadsLabel
        // 
        this.AutoThreadsLabel.AutoSize = true;
        this.AutoThreadsLabel.Location = new System.Drawing.Point(24, 64);
        // 
        // HddThreadsLabel
        // 
        this.HddThreadsLabel.AutoSize = true;
        this.HddThreadsLabel.Location = new System.Drawing.Point(32, 144);
        // 
        // AutoModeRadioButton
        // 
        this.AutoModeRadioButton.AutoSize = true;
        this.AutoModeRadioButton.Checked = true;
        this.AutoModeRadioButton.Location = new System.Drawing.Point(16, 24);
        this.AutoModeRadioButton.TabIndex = 0;
        this.AutoModeRadioButton.TabStop = true;
        // 
        // NvmeSsdModeRadioButton
        // 
        this.NvmeSsdModeRadioButton.AutoSize = true;
        this.NvmeSsdModeRadioButton.Location = new System.Drawing.Point(16, 280);
        this.NvmeSsdModeRadioButton.TabIndex = 0;
        this.NvmeSsdModeRadioButton.TabStop = true;
        // 
        // HddModeRadioButton
        // 
        this.HddModeRadioButton.AutoSize = true;
        this.HddModeRadioButton.Location = new System.Drawing.Point(16, 120);
        this.HddModeRadioButton.TabIndex = 0;
        this.HddModeRadioButton.TabStop = true;
        // 
        // SataSsdModeRadioButton
        // 
        this.SataSsdModeRadioButton.AutoSize = true;
        this.SataSsdModeRadioButton.Location = new System.Drawing.Point(16, 200);
        this.SataSsdModeRadioButton.TabIndex = 0;
        this.SataSsdModeRadioButton.TabStop = true;
        // 
        // AdvancedPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.IOThreadingGroupBox.ResumeLayout(false);
        this.IOThreadingGroupBox.PerformLayout();
        this.CustomModePanel.ResumeLayout(false);
        this.CustomModePanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
