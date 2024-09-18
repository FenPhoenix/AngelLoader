namespace AngelLoader.Forms;

sealed partial class AdvancedPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.IOThreadsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.AggressiveIOThreadingHelpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.AggressiveIOThreadingPictureBox = new System.Windows.Forms.PictureBox();
        this.AggressiveIOThreadingCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.IOThreadsManualNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.IOThreadsManualRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.IOThreadsAutoRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.PagePanel.SuspendLayout();
        this.IOThreadsGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AggressiveIOThreadingPictureBox)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.IOThreadsManualNumericUpDown)).BeginInit();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
        this.PagePanel.Controls.Add(this.IOThreadsGroupBox);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.TabIndex = 0;
        // 
        // IOThreadsGroupBox
        // 
        this.IOThreadsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.IOThreadsGroupBox.Controls.Add(this.AggressiveIOThreadingHelpLabel);
        this.IOThreadsGroupBox.Controls.Add(this.AggressiveIOThreadingPictureBox);
        this.IOThreadsGroupBox.Controls.Add(this.AggressiveIOThreadingCheckBox);
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualNumericUpDown);
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualRadioButton);
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsAutoRadioButton);
        this.IOThreadsGroupBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadsGroupBox.Size = new System.Drawing.Size(424, 184);
        this.IOThreadsGroupBox.TabIndex = 0;
        this.IOThreadsGroupBox.TabStop = false;
        // 
        // AggressiveIOThreadingHelpLabel
        // 
        this.AggressiveIOThreadingHelpLabel.AutoSize = true;
        this.AggressiveIOThreadingHelpLabel.Location = new System.Drawing.Point(40, 136);
        this.AggressiveIOThreadingHelpLabel.MaximumSize = new System.Drawing.Size(380, 0);
        // 
        // AggressiveIOThreadingPictureBox
        // 
        this.AggressiveIOThreadingPictureBox.Location = new System.Drawing.Point(16, 136);
        this.AggressiveIOThreadingPictureBox.Size = new System.Drawing.Size(16, 16);
        this.AggressiveIOThreadingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        // 
        // AggressiveIOThreadingCheckBox
        // 
        this.AggressiveIOThreadingCheckBox.AutoSize = true;
        this.AggressiveIOThreadingCheckBox.Location = new System.Drawing.Point(16, 104);
        this.AggressiveIOThreadingCheckBox.TabIndex = 3;
        // 
        // IOThreadsManualNumericUpDown
        // 
        this.IOThreadsManualNumericUpDown.Location = new System.Drawing.Point(32, 72);
        this.IOThreadsManualNumericUpDown.Maximum = new decimal(new int[] {
        2147483647,
        0,
        0,
        0});
        this.IOThreadsManualNumericUpDown.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.IOThreadsManualNumericUpDown.Size = new System.Drawing.Size(88, 20);
        this.IOThreadsManualNumericUpDown.TabIndex = 2;
        this.IOThreadsManualNumericUpDown.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // IOThreadsManualRadioButton
        // 
        this.IOThreadsManualRadioButton.AutoSize = true;
        this.IOThreadsManualRadioButton.Location = new System.Drawing.Point(16, 48);
        this.IOThreadsManualRadioButton.TabIndex = 1;
        this.IOThreadsManualRadioButton.TabStop = true;
        // 
        // IOThreadsAutoRadioButton
        // 
        this.IOThreadsAutoRadioButton.AutoSize = true;
        this.IOThreadsAutoRadioButton.Checked = true;
        this.IOThreadsAutoRadioButton.Location = new System.Drawing.Point(16, 24);
        this.IOThreadsAutoRadioButton.TabIndex = 0;
        this.IOThreadsAutoRadioButton.TabStop = true;
        // 
        // AdvancedPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.IOThreadsGroupBox.ResumeLayout(false);
        this.IOThreadsGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AggressiveIOThreadingPictureBox)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.IOThreadsManualNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
