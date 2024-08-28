#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class AdvancedPage
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
            this.PagePanel = new System.Windows.Forms.Panel();
            this.IOThreadsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.IOThreadsManualNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.IOThreadsManualRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.IOThreadsAutoRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
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
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.TabIndex = 0;
            // 
            // IOThreadsGroupBox
            // 
            this.IOThreadsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualNumericUpDown);
            this.IOThreadsGroupBox.Controls.Add(this.IOThreadsManualRadioButton);
            this.IOThreadsGroupBox.Controls.Add(this.IOThreadsAutoRadioButton);
            this.IOThreadsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.IOThreadsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.IOThreadsGroupBox.Name = "IOThreadsGroupBox";
            this.IOThreadsGroupBox.Size = new System.Drawing.Size(424, 112);
            this.IOThreadsGroupBox.TabIndex = 0;
            this.IOThreadsGroupBox.TabStop = false;
            this.IOThreadsGroupBox.Text = "Threads for disk access";
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
            this.IOThreadsManualNumericUpDown.Name = "IOThreadsManualNumericUpDown";
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
            this.IOThreadsManualRadioButton.Name = "IOThreadsManualRadioButton";
            this.IOThreadsManualRadioButton.Size = new System.Drawing.Size(60, 17);
            this.IOThreadsManualRadioButton.TabIndex = 1;
            this.IOThreadsManualRadioButton.TabStop = true;
            this.IOThreadsManualRadioButton.Text = "Manual";
            this.IOThreadsManualRadioButton.UseMnemonic = false;
            this.IOThreadsManualRadioButton.UseVisualStyleBackColor = true;
            // 
            // IOThreadsAutoRadioButton
            // 
            this.IOThreadsAutoRadioButton.AutoSize = true;
            this.IOThreadsAutoRadioButton.Checked = true;
            this.IOThreadsAutoRadioButton.Location = new System.Drawing.Point(16, 24);
            this.IOThreadsAutoRadioButton.Name = "IOThreadsAutoRadioButton";
            this.IOThreadsAutoRadioButton.Size = new System.Drawing.Size(47, 17);
            this.IOThreadsAutoRadioButton.TabIndex = 0;
            this.IOThreadsAutoRadioButton.TabStop = true;
            this.IOThreadsAutoRadioButton.Text = "Auto";
            this.IOThreadsAutoRadioButton.UseMnemonic = false;
            this.IOThreadsAutoRadioButton.UseVisualStyleBackColor = true;
            // 
            // AdvancedPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "AdvancedPage";
            this.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.ResumeLayout(false);
            this.IOThreadsGroupBox.ResumeLayout(false);
            this.IOThreadsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IOThreadsManualNumericUpDown)).EndInit();
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal CustomControls.DarkGroupBox IOThreadsGroupBox;
    internal CustomControls.DarkRadioButton IOThreadsAutoRadioButton;
    internal CustomControls.DarkRadioButton IOThreadsManualRadioButton;
    internal CustomControls.DarkNumericUpDown IOThreadsManualNumericUpDown;
}
