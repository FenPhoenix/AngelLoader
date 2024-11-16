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
            this.panel2 = new System.Windows.Forms.Panel();
            this.darkRadioButton5 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton6 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton7 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton8 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkLabel6 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.darkRadioButton4 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton3 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton2 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton1 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkLabel5 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomModePanel = new System.Windows.Forms.Panel();
            this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.PagePanel.SuspendLayout();
            this.IOThreadingGroupBox.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
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
            this.IOThreadingGroupBox.Controls.Add(this.panel2);
            this.IOThreadingGroupBox.Controls.Add(this.panel1);
            this.IOThreadingGroupBox.Controls.Add(this.CustomModePanel);
            this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
            this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.IOThreadingGroupBox.Name = "IOThreadingGroupBox";
            this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 336);
            this.IOThreadingGroupBox.TabIndex = 0;
            this.IOThreadingGroupBox.TabStop = false;
            this.IOThreadingGroupBox.Text = "I/O threading";
            this.IOThreadingGroupBox.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.IOThreadingGroupBox_PaintCustom);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.darkRadioButton5);
            this.panel2.Controls.Add(this.darkRadioButton6);
            this.panel2.Controls.Add(this.darkRadioButton7);
            this.panel2.Controls.Add(this.darkRadioButton8);
            this.panel2.Controls.Add(this.darkLabel6);
            this.panel2.Location = new System.Drawing.Point(8, 240);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(408, 88);
            this.panel2.TabIndex = 5;
            this.panel2.Visible = false;
            // 
            // darkRadioButton5
            // 
            this.darkRadioButton5.AutoSize = true;
            this.darkRadioButton5.Location = new System.Drawing.Point(8, 64);
            this.darkRadioButton5.Name = "darkRadioButton5";
            this.darkRadioButton5.Size = new System.Drawing.Size(49, 17);
            this.darkRadioButton5.TabIndex = 5;
            this.darkRadioButton5.TabStop = true;
            this.darkRadioButton5.Text = "HDD";
            // 
            // darkRadioButton6
            // 
            this.darkRadioButton6.AutoSize = true;
            this.darkRadioButton6.Location = new System.Drawing.Point(8, 48);
            this.darkRadioButton6.Name = "darkRadioButton6";
            this.darkRadioButton6.Size = new System.Drawing.Size(53, 17);
            this.darkRadioButton6.TabIndex = 5;
            this.darkRadioButton6.TabStop = true;
            this.darkRadioButton6.Text = "SATA";
            // 
            // darkRadioButton7
            // 
            this.darkRadioButton7.AutoSize = true;
            this.darkRadioButton7.Location = new System.Drawing.Point(8, 32);
            this.darkRadioButton7.Name = "darkRadioButton7";
            this.darkRadioButton7.Size = new System.Drawing.Size(55, 17);
            this.darkRadioButton7.TabIndex = 5;
            this.darkRadioButton7.TabStop = true;
            this.darkRadioButton7.Text = "NVMe";
            // 
            // darkRadioButton8
            // 
            this.darkRadioButton8.AutoSize = true;
            this.darkRadioButton8.Location = new System.Drawing.Point(8, 16);
            this.darkRadioButton8.Name = "darkRadioButton8";
            this.darkRadioButton8.Size = new System.Drawing.Size(122, 17);
            this.darkRadioButton8.TabIndex = 5;
            this.darkRadioButton8.TabStop = true;
            this.darkRadioButton8.Text = "Autodetected (HDD)";
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.Location = new System.Drawing.Point(0, 0);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(16, 13);
            this.darkLabel6.TabIndex = 4;
            this.darkLabel6.Text = "F:";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.darkRadioButton4);
            this.panel1.Controls.Add(this.darkRadioButton3);
            this.panel1.Controls.Add(this.darkRadioButton2);
            this.panel1.Controls.Add(this.darkRadioButton1);
            this.panel1.Controls.Add(this.darkLabel5);
            this.panel1.Location = new System.Drawing.Point(8, 152);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(408, 88);
            this.panel1.TabIndex = 5;
            this.panel1.Visible = false;
            // 
            // darkRadioButton4
            // 
            this.darkRadioButton4.AutoSize = true;
            this.darkRadioButton4.Location = new System.Drawing.Point(8, 64);
            this.darkRadioButton4.Name = "darkRadioButton4";
            this.darkRadioButton4.Size = new System.Drawing.Size(49, 17);
            this.darkRadioButton4.TabIndex = 5;
            this.darkRadioButton4.TabStop = true;
            this.darkRadioButton4.Text = "HDD";
            // 
            // darkRadioButton3
            // 
            this.darkRadioButton3.AutoSize = true;
            this.darkRadioButton3.Location = new System.Drawing.Point(8, 48);
            this.darkRadioButton3.Name = "darkRadioButton3";
            this.darkRadioButton3.Size = new System.Drawing.Size(53, 17);
            this.darkRadioButton3.TabIndex = 5;
            this.darkRadioButton3.TabStop = true;
            this.darkRadioButton3.Text = "SATA";
            // 
            // darkRadioButton2
            // 
            this.darkRadioButton2.AutoSize = true;
            this.darkRadioButton2.Location = new System.Drawing.Point(8, 32);
            this.darkRadioButton2.Name = "darkRadioButton2";
            this.darkRadioButton2.Size = new System.Drawing.Size(55, 17);
            this.darkRadioButton2.TabIndex = 5;
            this.darkRadioButton2.TabStop = true;
            this.darkRadioButton2.Text = "NVMe";
            // 
            // darkRadioButton1
            // 
            this.darkRadioButton1.AutoSize = true;
            this.darkRadioButton1.Location = new System.Drawing.Point(8, 16);
            this.darkRadioButton1.Name = "darkRadioButton1";
            this.darkRadioButton1.Size = new System.Drawing.Size(128, 17);
            this.darkRadioButton1.TabIndex = 5;
            this.darkRadioButton1.TabStop = true;
            this.darkRadioButton1.Text = "Autodetected (NVMe)";
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.Location = new System.Drawing.Point(0, 0);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(17, 13);
            this.darkLabel5.TabIndex = 4;
            this.darkLabel5.Text = "C:";
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
            this.CustomModePanel.Name = "CustomModePanel";
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
            this.CustomThreadsNumericUpDown.Name = "CustomThreadsNumericUpDown";
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
            this.CustomThreadsLabel.Name = "CustomThreadsLabel";
            this.CustomThreadsLabel.Size = new System.Drawing.Size(49, 13);
            this.CustomThreadsLabel.TabIndex = 2;
            this.CustomThreadsLabel.Text = "Threads:";
            // 
            // AutoModeRadioButton
            // 
            this.AutoModeRadioButton.AutoSize = true;
            this.AutoModeRadioButton.Checked = true;
            this.AutoModeRadioButton.Location = new System.Drawing.Point(8, 8);
            this.AutoModeRadioButton.Name = "AutoModeRadioButton";
            this.AutoModeRadioButton.Size = new System.Drawing.Size(47, 17);
            this.AutoModeRadioButton.TabIndex = 0;
            this.AutoModeRadioButton.TabStop = true;
            this.AutoModeRadioButton.Text = "Auto";
            // 
            // CustomModeRadioButton
            // 
            this.CustomModeRadioButton.AutoSize = true;
            this.CustomModeRadioButton.Location = new System.Drawing.Point(8, 32);
            this.CustomModeRadioButton.Name = "CustomModeRadioButton";
            this.CustomModeRadioButton.Size = new System.Drawing.Size(60, 17);
            this.CustomModeRadioButton.TabIndex = 1;
            this.CustomModeRadioButton.TabStop = true;
            this.CustomModeRadioButton.Text = "Custom";
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
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
    internal System.Windows.Forms.Panel CustomModePanel;
    internal System.Windows.Forms.Panel panel1;
    internal CustomControls.DarkLabel darkLabel5;
    internal CustomControls.DarkRadioButton darkRadioButton4;
    internal CustomControls.DarkRadioButton darkRadioButton3;
    internal CustomControls.DarkRadioButton darkRadioButton2;
    internal CustomControls.DarkRadioButton darkRadioButton1;
    internal System.Windows.Forms.Panel panel2;
    internal CustomControls.DarkRadioButton darkRadioButton5;
    internal CustomControls.DarkRadioButton darkRadioButton6;
    internal CustomControls.DarkRadioButton darkRadioButton7;
    internal CustomControls.DarkRadioButton darkRadioButton8;
    internal CustomControls.DarkLabel darkLabel6;
}
