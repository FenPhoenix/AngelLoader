namespace AngelLoader.Forms;

partial class AdvancedPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.IOThreadsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IOThreadsManualNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
        this.IOThreadsManualRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.IOThreadsAutomaticRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.PagePanel.SuspendLayout();
        this.IOThreadsGroupBox.SuspendLayout();
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
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualNumericUpDown);
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualRadioButton);
        this.IOThreadsGroupBox.Controls.Add(this.IOThreadsAutomaticRadioButton);
        this.IOThreadsGroupBox.Location = new System.Drawing.Point(8, 8);
        this.IOThreadsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.IOThreadsGroupBox.Size = new System.Drawing.Size(424, 112);
        this.IOThreadsGroupBox.TabIndex = 0;
        this.IOThreadsGroupBox.TabStop = false;
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
        // IOThreadsAutomaticRadioButton
        // 
        this.IOThreadsAutomaticRadioButton.AutoSize = true;
        this.IOThreadsAutomaticRadioButton.Checked = true;
        this.IOThreadsAutomaticRadioButton.Location = new System.Drawing.Point(16, 24);
        this.IOThreadsAutomaticRadioButton.TabIndex = 0;
        this.IOThreadsAutomaticRadioButton.TabStop = true;
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
        ((System.ComponentModel.ISupportInitialize)(this.IOThreadsManualNumericUpDown)).EndInit();
        this.ResumeLayout(false);
    }
}
