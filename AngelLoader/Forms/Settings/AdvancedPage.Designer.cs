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
            this.NvmeSsdThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SataSsdThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutoThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.HddThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.NvmeSsdThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SataSsdThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.AutoModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutoThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.HddThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.NvmeSsdModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.HddModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.SataSsdModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.AutoThreadsValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutoThreadingModeValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.HddThreadsValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SataSsdThreadsValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.NvmeSsdThreadsValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.HddThreadingModeValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SataSsdThreadingModeValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.NvmeSsdThreadingModeValueLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
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
            this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadingModeValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadingModeLabel);
            this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadingModeValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadingModeLabel);
            this.IOThreadingGroupBox.Controls.Add(this.AutoThreadingModeValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.AutoThreadingModeLabel);
            this.IOThreadingGroupBox.Controls.Add(this.HddThreadingModeValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.HddThreadingModeLabel);
            this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadsValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdThreadsLabel);
            this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadsValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.SataSsdThreadsLabel);
            this.IOThreadingGroupBox.Controls.Add(this.CustomModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.AutoModeLabel);
            this.IOThreadingGroupBox.Controls.Add(this.AutoThreadsValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.AutoThreadsLabel);
            this.IOThreadingGroupBox.Controls.Add(this.HddThreadsValueLabel);
            this.IOThreadingGroupBox.Controls.Add(this.HddThreadsLabel);
            this.IOThreadingGroupBox.Controls.Add(this.AutoModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.NvmeSsdModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.HddModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.SataSsdModeRadioButton);
            this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
            this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.IOThreadingGroupBox.Name = "IOThreadingGroupBox";
            this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 496);
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
            this.CustomModePanel.Location = new System.Drawing.Point(8, 384);
            this.CustomModePanel.Name = "CustomModePanel";
            this.CustomModePanel.Size = new System.Drawing.Size(408, 104);
            this.CustomModePanel.TabIndex = 14;
            // 
            // CustomThreadingModeAggressiveRadioButton
            // 
            this.CustomThreadingModeAggressiveRadioButton.AutoSize = true;
            this.CustomThreadingModeAggressiveRadioButton.Location = new System.Drawing.Point(24, 80);
            this.CustomThreadingModeAggressiveRadioButton.Name = "CustomThreadingModeAggressiveRadioButton";
            this.CustomThreadingModeAggressiveRadioButton.Size = new System.Drawing.Size(77, 17);
            this.CustomThreadingModeAggressiveRadioButton.TabIndex = 4;
            this.CustomThreadingModeAggressiveRadioButton.TabStop = true;
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
            // NvmeSsdThreadingModeLabel
            // 
            this.NvmeSsdThreadingModeLabel.AutoSize = true;
            this.NvmeSsdThreadingModeLabel.Location = new System.Drawing.Point(32, 320);
            this.NvmeSsdThreadingModeLabel.Name = "NvmeSsdThreadingModeLabel";
            this.NvmeSsdThreadingModeLabel.Size = new System.Drawing.Size(87, 13);
            this.NvmeSsdThreadingModeLabel.TabIndex = 12;
            this.NvmeSsdThreadingModeLabel.Text = "Threading mode:";
            // 
            // SataSsdThreadingModeLabel
            // 
            this.SataSsdThreadingModeLabel.AutoSize = true;
            this.SataSsdThreadingModeLabel.Location = new System.Drawing.Point(32, 240);
            this.SataSsdThreadingModeLabel.Name = "SataSsdThreadingModeLabel";
            this.SataSsdThreadingModeLabel.Size = new System.Drawing.Size(87, 13);
            this.SataSsdThreadingModeLabel.TabIndex = 9;
            this.SataSsdThreadingModeLabel.Text = "Threading mode:";
            // 
            // AutoThreadingModeLabel
            // 
            this.AutoThreadingModeLabel.AutoSize = true;
            this.AutoThreadingModeLabel.Location = new System.Drawing.Point(32, 80);
            this.AutoThreadingModeLabel.Name = "AutoThreadingModeLabel";
            this.AutoThreadingModeLabel.Size = new System.Drawing.Size(87, 13);
            this.AutoThreadingModeLabel.TabIndex = 3;
            this.AutoThreadingModeLabel.Text = "Threading mode:";
            // 
            // HddThreadingModeLabel
            // 
            this.HddThreadingModeLabel.AutoSize = true;
            this.HddThreadingModeLabel.Location = new System.Drawing.Point(32, 160);
            this.HddThreadingModeLabel.Name = "HddThreadingModeLabel";
            this.HddThreadingModeLabel.Size = new System.Drawing.Size(87, 13);
            this.HddThreadingModeLabel.TabIndex = 6;
            this.HddThreadingModeLabel.Text = "Threading mode:";
            // 
            // NvmeSsdThreadsLabel
            // 
            this.NvmeSsdThreadsLabel.AutoSize = true;
            this.NvmeSsdThreadsLabel.Location = new System.Drawing.Point(32, 304);
            this.NvmeSsdThreadsLabel.Name = "NvmeSsdThreadsLabel";
            this.NvmeSsdThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.NvmeSsdThreadsLabel.TabIndex = 11;
            this.NvmeSsdThreadsLabel.Text = "Threads:";
            // 
            // SataSsdThreadsLabel
            // 
            this.SataSsdThreadsLabel.AutoSize = true;
            this.SataSsdThreadsLabel.Location = new System.Drawing.Point(32, 224);
            this.SataSsdThreadsLabel.Name = "SataSsdThreadsLabel";
            this.SataSsdThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.SataSsdThreadsLabel.TabIndex = 8;
            this.SataSsdThreadsLabel.Text = "Threads:";
            // 
            // CustomModeRadioButton
            // 
            this.CustomModeRadioButton.AutoSize = true;
            this.CustomModeRadioButton.Location = new System.Drawing.Point(16, 360);
            this.CustomModeRadioButton.Name = "CustomModeRadioButton";
            this.CustomModeRadioButton.Size = new System.Drawing.Size(60, 17);
            this.CustomModeRadioButton.TabIndex = 13;
            this.CustomModeRadioButton.TabStop = true;
            this.CustomModeRadioButton.Text = "Custom";
            // 
            // AutoModeLabel
            // 
            this.AutoModeLabel.AutoSize = true;
            this.AutoModeLabel.Location = new System.Drawing.Point(32, 48);
            this.AutoModeLabel.Name = "AutoModeLabel";
            this.AutoModeLabel.Size = new System.Drawing.Size(40, 13);
            this.AutoModeLabel.TabIndex = 1;
            this.AutoModeLabel.Text = "[Mode]";
            // 
            // AutoThreadsLabel
            // 
            this.AutoThreadsLabel.AutoSize = true;
            this.AutoThreadsLabel.Location = new System.Drawing.Point(32, 64);
            this.AutoThreadsLabel.Name = "AutoThreadsLabel";
            this.AutoThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.AutoThreadsLabel.TabIndex = 2;
            this.AutoThreadsLabel.Text = "Threads:";
            // 
            // HddThreadsLabel
            // 
            this.HddThreadsLabel.AutoSize = true;
            this.HddThreadsLabel.Location = new System.Drawing.Point(32, 144);
            this.HddThreadsLabel.Name = "HddThreadsLabel";
            this.HddThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.HddThreadsLabel.TabIndex = 5;
            this.HddThreadsLabel.Text = "Threads:";
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
            // NvmeSsdModeRadioButton
            // 
            this.NvmeSsdModeRadioButton.AutoSize = true;
            this.NvmeSsdModeRadioButton.Location = new System.Drawing.Point(16, 280);
            this.NvmeSsdModeRadioButton.Name = "NvmeSsdModeRadioButton";
            this.NvmeSsdModeRadioButton.Size = new System.Drawing.Size(80, 17);
            this.NvmeSsdModeRadioButton.TabIndex = 10;
            this.NvmeSsdModeRadioButton.TabStop = true;
            this.NvmeSsdModeRadioButton.Text = "NVMe SSD";
            // 
            // HddModeRadioButton
            // 
            this.HddModeRadioButton.AutoSize = true;
            this.HddModeRadioButton.Location = new System.Drawing.Point(16, 120);
            this.HddModeRadioButton.Name = "HddModeRadioButton";
            this.HddModeRadioButton.Size = new System.Drawing.Size(49, 17);
            this.HddModeRadioButton.TabIndex = 4;
            this.HddModeRadioButton.TabStop = true;
            this.HddModeRadioButton.Text = "HDD";
            // 
            // SataSsdModeRadioButton
            // 
            this.SataSsdModeRadioButton.AutoSize = true;
            this.SataSsdModeRadioButton.Location = new System.Drawing.Point(16, 200);
            this.SataSsdModeRadioButton.Name = "SataSsdModeRadioButton";
            this.SataSsdModeRadioButton.Size = new System.Drawing.Size(78, 17);
            this.SataSsdModeRadioButton.TabIndex = 7;
            this.SataSsdModeRadioButton.TabStop = true;
            this.SataSsdModeRadioButton.Text = "SATA SSD";
            // 
            // AutoThreadsValueLabel
            // 
            this.AutoThreadsValueLabel.AutoSize = true;
            this.AutoThreadsValueLabel.Location = new System.Drawing.Point(120, 64);
            this.AutoThreadsValueLabel.Name = "AutoThreadsValueLabel";
            this.AutoThreadsValueLabel.Size = new System.Drawing.Size(48, 13);
            this.AutoThreadsValueLabel.TabIndex = 2;
            this.AutoThreadsValueLabel.Text = "[threads]";
            // 
            // AutoThreadingModeValueLabel
            // 
            this.AutoThreadingModeValueLabel.AutoSize = true;
            this.AutoThreadingModeValueLabel.Location = new System.Drawing.Point(120, 80);
            this.AutoThreadingModeValueLabel.Name = "AutoThreadingModeValueLabel";
            this.AutoThreadingModeValueLabel.Size = new System.Drawing.Size(86, 13);
            this.AutoThreadingModeValueLabel.TabIndex = 3;
            this.AutoThreadingModeValueLabel.Text = "[threading mode]";
            // 
            // HddThreadsValueLabel
            // 
            this.HddThreadsValueLabel.AutoSize = true;
            this.HddThreadsValueLabel.Location = new System.Drawing.Point(120, 144);
            this.HddThreadsValueLabel.Name = "HddThreadsValueLabel";
            this.HddThreadsValueLabel.Size = new System.Drawing.Size(13, 13);
            this.HddThreadsValueLabel.TabIndex = 5;
            this.HddThreadsValueLabel.Text = "1";
            // 
            // SataSsdThreadsValueLabel
            // 
            this.SataSsdThreadsValueLabel.AutoSize = true;
            this.SataSsdThreadsValueLabel.Location = new System.Drawing.Point(120, 224);
            this.SataSsdThreadsValueLabel.Name = "SataSsdThreadsValueLabel";
            this.SataSsdThreadsValueLabel.Size = new System.Drawing.Size(19, 13);
            this.SataSsdThreadsValueLabel.TabIndex = 8;
            this.SataSsdThreadsValueLabel.Text = "12";
            // 
            // NvmeSsdThreadsValueLabel
            // 
            this.NvmeSsdThreadsValueLabel.AutoSize = true;
            this.NvmeSsdThreadsValueLabel.Location = new System.Drawing.Point(120, 304);
            this.NvmeSsdThreadsValueLabel.Name = "NvmeSsdThreadsValueLabel";
            this.NvmeSsdThreadsValueLabel.Size = new System.Drawing.Size(19, 13);
            this.NvmeSsdThreadsValueLabel.TabIndex = 11;
            this.NvmeSsdThreadsValueLabel.Text = "12";
            // 
            // HddThreadingModeValueLabel
            // 
            this.HddThreadingModeValueLabel.AutoSize = true;
            this.HddThreadingModeValueLabel.Location = new System.Drawing.Point(120, 160);
            this.HddThreadingModeValueLabel.Name = "HddThreadingModeValueLabel";
            this.HddThreadingModeValueLabel.Size = new System.Drawing.Size(40, 13);
            this.HddThreadingModeValueLabel.TabIndex = 6;
            this.HddThreadingModeValueLabel.Text = "Normal";
            // 
            // SataSsdThreadingModeValueLabel
            // 
            this.SataSsdThreadingModeValueLabel.AutoSize = true;
            this.SataSsdThreadingModeValueLabel.Location = new System.Drawing.Point(120, 240);
            this.SataSsdThreadingModeValueLabel.Name = "SataSsdThreadingModeValueLabel";
            this.SataSsdThreadingModeValueLabel.Size = new System.Drawing.Size(40, 13);
            this.SataSsdThreadingModeValueLabel.TabIndex = 9;
            this.SataSsdThreadingModeValueLabel.Text = "Normal";
            // 
            // NvmeSsdThreadingModeValueLabel
            // 
            this.NvmeSsdThreadingModeValueLabel.AutoSize = true;
            this.NvmeSsdThreadingModeValueLabel.Location = new System.Drawing.Point(120, 320);
            this.NvmeSsdThreadingModeValueLabel.Name = "NvmeSsdThreadingModeValueLabel";
            this.NvmeSsdThreadingModeValueLabel.Size = new System.Drawing.Size(59, 13);
            this.NvmeSsdThreadingModeValueLabel.TabIndex = 12;
            this.NvmeSsdThreadingModeValueLabel.Text = "Aggressive";
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
    internal CustomControls.DarkLabel HddThreadingModeLabel;
    internal CustomControls.DarkLabel HddThreadsLabel;
    internal CustomControls.DarkRadioButton NvmeSsdModeRadioButton;
    internal CustomControls.DarkRadioButton SataSsdModeRadioButton;
    internal CustomControls.DarkRadioButton HddModeRadioButton;
    internal CustomControls.DarkLabel NvmeSsdThreadingModeLabel;
    internal CustomControls.DarkLabel SataSsdThreadingModeLabel;
    internal CustomControls.DarkLabel NvmeSsdThreadsLabel;
    internal CustomControls.DarkLabel SataSsdThreadsLabel;
    internal CustomControls.DarkLabel AutoThreadingModeLabel;
    internal CustomControls.DarkLabel AutoModeLabel;
    internal CustomControls.DarkLabel AutoThreadsLabel;
    internal CustomControls.DarkLabel CustomThreadsLabel;
    internal CustomControls.DarkLabel CustomThreadingModeLabel;
    internal System.Windows.Forms.Panel CustomModePanel;
    internal CustomControls.DarkRadioButton CustomThreadingModeAggressiveRadioButton;
    internal CustomControls.DarkRadioButton CustomThreadingModeNormalRadioButton;
    internal CustomControls.DarkLabel NvmeSsdThreadingModeValueLabel;
    internal CustomControls.DarkLabel SataSsdThreadingModeValueLabel;
    internal CustomControls.DarkLabel AutoThreadingModeValueLabel;
    internal CustomControls.DarkLabel HddThreadingModeValueLabel;
    internal CustomControls.DarkLabel NvmeSsdThreadsValueLabel;
    internal CustomControls.DarkLabel SataSsdThreadsValueLabel;
    internal CustomControls.DarkLabel AutoThreadsValueLabel;
    internal CustomControls.DarkLabel HddThreadsValueLabel;
}
