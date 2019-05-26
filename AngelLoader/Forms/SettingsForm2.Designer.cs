namespace AngelLoader.Forms
{
    partial class SettingsForm2
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.PagesListBox = new System.Windows.Forms.ListBox();
            this.PagePanel = new System.Windows.Forms.Panel();
            this.BottomFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.MainSplitContainer = new AngelLoader.CustomControls.SplitContainerCustom();
            this.BottomFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagesListBox
            // 
            this.PagesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagesListBox.FormattingEnabled = true;
            this.PagesListBox.IntegralHeight = false;
            this.PagesListBox.Items.AddRange(new object[] {
            "Paths",
            "FM Display",
            "Other"});
            this.PagesListBox.Location = new System.Drawing.Point(0, 0);
            this.PagesListBox.Name = "PagesListBox";
            this.PagesListBox.Size = new System.Drawing.Size(155, 613);
            this.PagesListBox.TabIndex = 1;
            this.PagesListBox.SelectedIndexChanged += new System.EventHandler(this.PagesListBox_SelectedIndexChanged);
            this.PagesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PagesListBox_MouseDown);
            // 
            // PagePanel
            // 
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(535, 613);
            this.PagePanel.TabIndex = 2;
            // 
            // BottomFlowLayoutPanel
            // 
            this.BottomFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.BottomFlowLayoutPanel.Controls.Add(this.OKButton);
            this.BottomFlowLayoutPanel.Controls.Add(this.ErrorLabel);
            this.BottomFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFlowLayoutPanel.Location = new System.Drawing.Point(0, 628);
            this.BottomFlowLayoutPanel.Name = "BottomFlowLayoutPanel";
            this.BottomFlowLayoutPanel.Size = new System.Drawing.Size(694, 40);
            this.BottomFlowLayoutPanel.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(610, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(529, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = true;
            this.errorProvider1.SetError(this.ErrorLabel, "temp");
            this.ErrorLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.errorProvider1.SetIconAlignment(this.ErrorLabel, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.ErrorLabel.Location = new System.Drawing.Point(462, 12);
            this.ErrorLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(61, 13);
            this.ErrorLabel.TabIndex = 4;
            this.ErrorLabel.Text = "[ErrorLabel]";
            this.ErrorLabel.Visible = false;
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainSplitContainer.BackColor = System.Drawing.SystemColors.ControlDark;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.MouseOverCrossSection = false;
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel1.Controls.Add(this.PagesListBox);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel2.Controls.Add(this.PagePanel);
            this.MainSplitContainer.Size = new System.Drawing.Size(694, 613);
            this.MainSplitContainer.SplitterDistance = 155;
            this.MainSplitContainer.TabIndex = 5;
            // 
            // SettingsForm2
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(694, 656);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.BottomFlowLayoutPanel);
            this.DoubleBuffered = true;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.BottomFlowLayoutPanel.ResumeLayout(false);
            this.BottomFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox PagesListBox;
        private System.Windows.Forms.Panel PagePanel;
        private System.Windows.Forms.FlowLayoutPanel BottomFlowLayoutPanel;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ToolTip MainToolTip;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label ErrorLabel;
        private AngelLoader.CustomControls.SplitContainerCustom MainSplitContainer;
    }
}