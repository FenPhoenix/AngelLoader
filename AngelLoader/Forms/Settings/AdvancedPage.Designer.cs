#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class AdvancedPage
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
            this.IOThreadingGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.CustomModePanel = new System.Windows.Forms.Panel();
            this.CustomThreadingModeAggressiveRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.CustomThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomThreadingModeNormalRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
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
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.TabIndex = 0;
            // 
            // IOThreadingGroupBox
            // 
            this.IOThreadingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IOThreadingGroupBox.Controls.Add(this.CustomModePanel);
            this.IOThreadingGroupBox.Controls.Add(this.CustomModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.AutoModeRadioButton);
            this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
            this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.IOThreadingGroupBox.Name = "IOThreadingGroupBox";
            this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 192);
            this.IOThreadingGroupBox.TabIndex = 0;
            this.IOThreadingGroupBox.TabStop = false;
            this.IOThreadingGroupBox.Text = "I/O threading";
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
            this.CustomModePanel.Location = new System.Drawing.Point(8, 72);
            this.CustomModePanel.Name = "CustomModePanel";
            this.CustomModePanel.Size = new System.Drawing.Size(408, 104);
            this.CustomModePanel.TabIndex = 2;
            // 
            // CustomThreadingModeAggressiveRadioButton
            // 
            this.CustomThreadingModeAggressiveRadioButton.AutoSize = true;
            this.CustomThreadingModeAggressiveRadioButton.Location = new System.Drawing.Point(24, 80);
            this.CustomThreadingModeAggressiveRadioButton.Name = "CustomThreadingModeAggressiveRadioButton";
            this.CustomThreadingModeAggressiveRadioButton.Size = new System.Drawing.Size(77, 17);
            this.CustomThreadingModeAggressiveRadioButton.TabIndex = 4;
            this.CustomThreadingModeAggressiveRadioButton.Text = "Aggressive";
            // 
            // CustomThreadingModeLabel
            // 
            this.CustomThreadingModeLabel.AutoSize = true;
            this.CustomThreadingModeLabel.Location = new System.Drawing.Point(24, 48);
            this.CustomThreadingModeLabel.Name = "CustomThreadingModeLabel";
            this.CustomThreadingModeLabel.Size = new System.Drawing.Size(87, 13);
            this.CustomThreadingModeLabel.TabIndex = 2;
            this.CustomThreadingModeLabel.Text = "Threading mode:";
            // 
            // CustomThreadingModeNormalRadioButton
            // 
            this.CustomThreadingModeNormalRadioButton.AutoSize = true;
            this.CustomThreadingModeNormalRadioButton.Checked = true;
            this.CustomThreadingModeNormalRadioButton.Location = new System.Drawing.Point(24, 64);
            this.CustomThreadingModeNormalRadioButton.Name = "CustomThreadingModeNormalRadioButton";
            this.CustomThreadingModeNormalRadioButton.Size = new System.Drawing.Size(58, 17);
            this.CustomThreadingModeNormalRadioButton.TabIndex = 3;
            this.CustomThreadingModeNormalRadioButton.TabStop = true;
            this.CustomThreadingModeNormalRadioButton.Text = "Normal";
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
            this.CustomThreadsNumericUpDown.Name = "CustomThreadsNumericUpDown";
            this.CustomThreadsNumericUpDown.Size = new System.Drawing.Size(104, 20);
            this.CustomThreadsNumericUpDown.TabIndex = 1;
            this.CustomThreadsNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CustomThreadsLabel
            // 
            this.CustomThreadsLabel.AutoSize = true;
            this.CustomThreadsLabel.Location = new System.Drawing.Point(24, 0);
            this.CustomThreadsLabel.Name = "CustomThreadsLabel";
            this.CustomThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.CustomThreadsLabel.TabIndex = 0;
            this.CustomThreadsLabel.Text = "Threads:";
            // 
            // CustomModeRadioButton
            // 
            this.CustomModeRadioButton.AutoSize = true;
            this.CustomModeRadioButton.Location = new System.Drawing.Point(16, 48);
            this.CustomModeRadioButton.Name = "CustomModeRadioButton";
            this.CustomModeRadioButton.Size = new System.Drawing.Size(60, 17);
            this.CustomModeRadioButton.TabIndex = 1;
            this.CustomModeRadioButton.TabStop = true;
            this.CustomModeRadioButton.Text = "Custom";
            // 
            // AutoModeRadioButton
            // 
            this.AutoModeRadioButton.AutoSize = true;
            this.AutoModeRadioButton.Checked = true;
            this.AutoModeRadioButton.Location = new System.Drawing.Point(16, 24);
            this.AutoModeRadioButton.Name = "AutoModeRadioButton";
            this.AutoModeRadioButton.Size = new System.Drawing.Size(47, 17);
            this.AutoModeRadioButton.TabIndex = 0;
            this.AutoModeRadioButton.TabStop = true;
            this.AutoModeRadioButton.Text = "Auto";
            // 
            // AdvancedPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "AdvancedPage";
            this.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.ResumeLayout(false);
            this.IOThreadingGroupBox.ResumeLayout(false);
            this.IOThreadingGroupBox.PerformLayout();
            this.CustomModePanel.ResumeLayout(false);
            this.CustomModePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal CustomControls.DarkGroupBox IOThreadingGroupBox;
    internal CustomControls.DarkRadioButton AutoModeRadioButton;
    internal CustomControls.DarkRadioButton CustomModeRadioButton;
    internal CustomControls.DarkNumericUpDown CustomThreadsNumericUpDown;
    internal CustomControls.DarkLabel CustomThreadsLabel;
    internal CustomControls.DarkLabel CustomThreadingModeLabel;
    internal System.Windows.Forms.Panel CustomModePanel;
    internal CustomControls.DarkRadioButton CustomThreadingModeAggressiveRadioButton;
    internal CustomControls.DarkRadioButton CustomThreadingModeNormalRadioButton;
}
