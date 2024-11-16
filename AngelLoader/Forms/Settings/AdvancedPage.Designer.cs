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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.darkLabel1 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.darkLabel2 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.darkComboBox1 = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.darkComboBox2 = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.CustomModePanel = new System.Windows.Forms.Panel();
            this.CustomThreadingModeAggressiveRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.CustomThreadingModeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomThreadingModeNormalRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.CustomThreadsNumericUpDown = new AngelLoader.Forms.CustomControls.DarkNumericUpDown();
            this.CustomThreadsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CustomModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.AutoModeRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkLabel3 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.darkLabel4 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.darkLabel5 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.darkRadioButton1 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton2 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton3 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton4 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.darkRadioButton5 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton6 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton7 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkRadioButton8 = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.darkLabel6 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.darkLabel7 = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.PagePanel.SuspendLayout();
            this.IOThreadingGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.CustomModePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
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
            this.IOThreadingGroupBox.Controls.Add(this.darkLabel7);
            this.IOThreadingGroupBox.Controls.Add(this.panel2);
            this.IOThreadingGroupBox.Controls.Add(this.panel1);
            this.IOThreadingGroupBox.Controls.Add(this.tableLayoutPanel1);
            this.IOThreadingGroupBox.Controls.Add(this.CustomModePanel);
            this.IOThreadingGroupBox.Controls.Add(this.CustomModeRadioButton);
            this.IOThreadingGroupBox.Controls.Add(this.AutoModeRadioButton);
            this.IOThreadingGroupBox.Location = new System.Drawing.Point(8, 8);
            this.IOThreadingGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.IOThreadingGroupBox.Name = "IOThreadingGroupBox";
            this.IOThreadingGroupBox.Size = new System.Drawing.Size(424, 568);
            this.IOThreadingGroupBox.TabIndex = 0;
            this.IOThreadingGroupBox.TabStop = false;
            this.IOThreadingGroupBox.Text = "I/O threading";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.darkLabel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.darkLabel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.darkComboBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.darkComboBox2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.darkLabel3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.darkLabel4, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 512);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(408, 45);
            this.tableLayoutPanel1.TabIndex = 3;
            this.tableLayoutPanel1.Visible = false;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.Location = new System.Drawing.Point(3, 5);
            this.darkLabel1.Margin = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(17, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "C:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Location = new System.Drawing.Point(3, 27);
            this.darkLabel2.Margin = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(18, 13);
            this.darkLabel2.TabIndex = 0;
            this.darkLabel2.Text = "D:";
            // 
            // darkComboBox1
            // 
            this.darkComboBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkComboBox1.FormattingEnabled = true;
            this.darkComboBox1.Items.AddRange(new object[] {
            "Autodetect",
            "NVMe",
            "SATA",
            "HDD",
            "Very long string that is guaranteed to make us very very long and extremely lengt" +
                "hy"});
            this.darkComboBox1.Location = new System.Drawing.Point(22, 1);
            this.darkComboBox1.Margin = new System.Windows.Forms.Padding(0);
            this.darkComboBox1.Name = "darkComboBox1";
            this.darkComboBox1.Size = new System.Drawing.Size(120, 21);
            this.darkComboBox1.TabIndex = 1;
            // 
            // darkComboBox2
            // 
            this.darkComboBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkComboBox2.FormattingEnabled = true;
            this.darkComboBox2.Items.AddRange(new object[] {
            "Autodetect",
            "NVMe",
            "SATA",
            "HDD"});
            this.darkComboBox2.Location = new System.Drawing.Point(22, 23);
            this.darkComboBox2.Margin = new System.Windows.Forms.Padding(0);
            this.darkComboBox2.Name = "darkComboBox2";
            this.darkComboBox2.Size = new System.Drawing.Size(120, 21);
            this.darkComboBox2.TabIndex = 1;
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
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Location = new System.Drawing.Point(145, 5);
            this.darkLabel3.Margin = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(59, 13);
            this.darkLabel3.TabIndex = 0;
            this.darkLabel3.Text = "Autodetect";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.Location = new System.Drawing.Point(145, 27);
            this.darkLabel4.Margin = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(59, 13);
            this.darkLabel4.TabIndex = 0;
            this.darkLabel4.Text = "Autodetect";
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
            // panel1
            // 
            this.panel1.Controls.Add(this.darkRadioButton4);
            this.panel1.Controls.Add(this.darkRadioButton3);
            this.panel1.Controls.Add(this.darkRadioButton2);
            this.panel1.Controls.Add(this.darkRadioButton1);
            this.panel1.Controls.Add(this.darkLabel5);
            this.panel1.Location = new System.Drawing.Point(8, 272);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 88);
            this.panel1.TabIndex = 5;
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
            // panel2
            // 
            this.panel2.Controls.Add(this.darkRadioButton5);
            this.panel2.Controls.Add(this.darkRadioButton6);
            this.panel2.Controls.Add(this.darkRadioButton7);
            this.panel2.Controls.Add(this.darkRadioButton8);
            this.panel2.Controls.Add(this.darkLabel6);
            this.panel2.Location = new System.Drawing.Point(8, 360);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 88);
            this.panel2.TabIndex = 5;
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
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.Location = new System.Drawing.Point(8, 248);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(42, 13);
            this.darkLabel7.TabIndex = 6;
            this.darkLabel7.Text = "Manual";
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
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.CustomModePanel.ResumeLayout(false);
            this.CustomModePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomThreadsNumericUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private CustomControls.DarkLabel darkLabel1;
    private CustomControls.DarkLabel darkLabel2;
    private CustomControls.DarkComboBox darkComboBox1;
    private CustomControls.DarkComboBox darkComboBox2;
    private CustomControls.DarkLabel darkLabel3;
    private CustomControls.DarkLabel darkLabel4;
    private System.Windows.Forms.Panel panel1;
    private CustomControls.DarkLabel darkLabel5;
    private CustomControls.DarkRadioButton darkRadioButton4;
    private CustomControls.DarkRadioButton darkRadioButton3;
    private CustomControls.DarkRadioButton darkRadioButton2;
    private CustomControls.DarkRadioButton darkRadioButton1;
    private System.Windows.Forms.Panel panel2;
    private CustomControls.DarkRadioButton darkRadioButton5;
    private CustomControls.DarkRadioButton darkRadioButton6;
    private CustomControls.DarkRadioButton darkRadioButton7;
    private CustomControls.DarkRadioButton darkRadioButton8;
    private CustomControls.DarkLabel darkLabel6;
    private CustomControls.DarkLabel darkLabel7;
}
