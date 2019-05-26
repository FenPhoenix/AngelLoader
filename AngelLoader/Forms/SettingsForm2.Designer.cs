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
            this.SectionPanel = new System.Windows.Forms.Panel();
            this.BottomFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.BottomFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // PagesListBox
            // 
            this.PagesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PagesListBox.FormattingEnabled = true;
            this.PagesListBox.IntegralHeight = false;
            this.PagesListBox.Items.AddRange(new object[] {
            "Paths",
            "FM Display",
            "Other"});
            this.PagesListBox.Location = new System.Drawing.Point(0, 0);
            this.PagesListBox.Name = "PagesListBox";
            this.PagesListBox.Size = new System.Drawing.Size(232, 544);
            this.PagesListBox.TabIndex = 1;
            this.PagesListBox.SelectedIndexChanged += new System.EventHandler(this.PagesListBox_SelectedIndexChanged);
            this.PagesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PagesListBox_MouseDown);
            // 
            // SectionPanel
            // 
            this.SectionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SectionPanel.Location = new System.Drawing.Point(232, 0);
            this.SectionPanel.Name = "SectionPanel";
            this.SectionPanel.Size = new System.Drawing.Size(461, 544);
            this.SectionPanel.TabIndex = 2;
            // 
            // BottomFlowLayoutPanel
            // 
            this.BottomFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.BottomFlowLayoutPanel.Controls.Add(this.OKButton);
            this.BottomFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFlowLayoutPanel.Location = new System.Drawing.Point(0, 547);
            this.BottomFlowLayoutPanel.Name = "BottomFlowLayoutPanel";
            this.BottomFlowLayoutPanel.Size = new System.Drawing.Size(693, 40);
            this.BottomFlowLayoutPanel.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(609, 8);
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
            this.OKButton.Location = new System.Drawing.Point(528, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // SettingsForm2
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(693, 587);
            this.Controls.Add(this.BottomFlowLayoutPanel);
            this.Controls.Add(this.SectionPanel);
            this.Controls.Add(this.PagesListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm2";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.BottomFlowLayoutPanel.ResumeLayout(false);
            this.BottomFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox PagesListBox;
        private System.Windows.Forms.Panel SectionPanel;
        private System.Windows.Forms.FlowLayoutPanel BottomFlowLayoutPanel;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ToolTip MainToolTip;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}