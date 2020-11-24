namespace AngelLoader.Forms
{
    sealed partial class ExportFMIniForm
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
            this.NiceNameLabel = new System.Windows.Forms.Label();
            this.InfoFileLabel = new System.Windows.Forms.Label();
            this.ReleaseDateLabel = new System.Windows.Forms.Label();
            this.TagsLabel = new System.Windows.Forms.Label();
            this.DescrLabel = new System.Windows.Forms.Label();
            this.NiceNameTextBox = new System.Windows.Forms.TextBox();
            this.InfoFileTextBox = new System.Windows.Forms.TextBox();
            this.ReleaseDateTextBox = new System.Windows.Forms.TextBox();
            this.TagsTextBox = new System.Windows.Forms.TextBox();
            this.DescrTextBox = new System.Windows.Forms.TextBox();
            this.BottomButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.BottomButtonsFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // NiceNameLabel
            // 
            this.NiceNameLabel.AutoSize = true;
            this.NiceNameLabel.Location = new System.Drawing.Point(16, 16);
            this.NiceNameLabel.Name = "NiceNameLabel";
            this.NiceNameLabel.Size = new System.Drawing.Size(63, 13);
            this.NiceNameLabel.TabIndex = 1;
            this.NiceNameLabel.Text = "NiceName=";
            // 
            // InfoFileLabel
            // 
            this.InfoFileLabel.AutoSize = true;
            this.InfoFileLabel.Location = new System.Drawing.Point(16, 40);
            this.InfoFileLabel.Name = "InfoFileLabel";
            this.InfoFileLabel.Size = new System.Drawing.Size(47, 13);
            this.InfoFileLabel.TabIndex = 3;
            this.InfoFileLabel.Text = "InfoFile=";
            // 
            // ReleaseDateLabel
            // 
            this.ReleaseDateLabel.AutoSize = true;
            this.ReleaseDateLabel.Location = new System.Drawing.Point(16, 64);
            this.ReleaseDateLabel.Name = "ReleaseDateLabel";
            this.ReleaseDateLabel.Size = new System.Drawing.Size(75, 13);
            this.ReleaseDateLabel.TabIndex = 5;
            this.ReleaseDateLabel.Text = "ReleaseDate=";
            // 
            // TagsLabel
            // 
            this.TagsLabel.AutoSize = true;
            this.TagsLabel.Location = new System.Drawing.Point(16, 88);
            this.TagsLabel.Name = "TagsLabel";
            this.TagsLabel.Size = new System.Drawing.Size(37, 13);
            this.TagsLabel.TabIndex = 7;
            this.TagsLabel.Text = "Tags=";
            // 
            // DescrLabel
            // 
            this.DescrLabel.AutoSize = true;
            this.DescrLabel.Location = new System.Drawing.Point(16, 112);
            this.DescrLabel.Name = "DescrLabel";
            this.DescrLabel.Size = new System.Drawing.Size(41, 13);
            this.DescrLabel.TabIndex = 9;
            this.DescrLabel.Text = "Descr=";
            // 
            // NiceNameTextBox
            // 
            this.NiceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NiceNameTextBox.Location = new System.Drawing.Point(96, 13);
            this.NiceNameTextBox.Name = "NiceNameTextBox";
            this.NiceNameTextBox.ReadOnly = true;
            this.NiceNameTextBox.Size = new System.Drawing.Size(681, 20);
            this.NiceNameTextBox.TabIndex = 2;
            // 
            // InfoFileTextBox
            // 
            this.InfoFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoFileTextBox.Location = new System.Drawing.Point(96, 37);
            this.InfoFileTextBox.Name = "InfoFileTextBox";
            this.InfoFileTextBox.ReadOnly = true;
            this.InfoFileTextBox.Size = new System.Drawing.Size(681, 20);
            this.InfoFileTextBox.TabIndex = 4;
            // 
            // ReleaseDateTextBox
            // 
            this.ReleaseDateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReleaseDateTextBox.Location = new System.Drawing.Point(96, 61);
            this.ReleaseDateTextBox.Name = "ReleaseDateTextBox";
            this.ReleaseDateTextBox.ReadOnly = true;
            this.ReleaseDateTextBox.Size = new System.Drawing.Size(681, 20);
            this.ReleaseDateTextBox.TabIndex = 6;
            // 
            // TagsTextBox
            // 
            this.TagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TagsTextBox.Location = new System.Drawing.Point(96, 85);
            this.TagsTextBox.Name = "TagsTextBox";
            this.TagsTextBox.ReadOnly = true;
            this.TagsTextBox.Size = new System.Drawing.Size(681, 20);
            this.TagsTextBox.TabIndex = 8;
            // 
            // DescrTextBox
            // 
            this.DescrTextBox.AcceptsReturn = true;
            this.DescrTextBox.AcceptsTab = true;
            this.DescrTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescrTextBox.Location = new System.Drawing.Point(96, 109);
            this.DescrTextBox.Multiline = true;
            this.DescrTextBox.Name = "DescrTextBox";
            this.DescrTextBox.Size = new System.Drawing.Size(681, 264);
            this.DescrTextBox.TabIndex = 10;
            // 
            // BottomButtonsFLP
            // 
            this.BottomButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomButtonsFLP.Controls.Add(this.Cancel_Button);
            this.BottomButtonsFLP.Controls.Add(this.ExportButton);
            this.BottomButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomButtonsFLP.Location = new System.Drawing.Point(0, 375);
            this.BottomButtonsFLP.Name = "BottomButtonsFLP";
            this.BottomButtonsFLP.Size = new System.Drawing.Size(793, 40);
            this.BottomButtonsFLP.TabIndex = 0;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(709, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 1;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ExportButton
            // 
            this.ExportButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ExportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ExportButton.Location = new System.Drawing.Point(628, 8);
            this.ExportButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.ExportButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            // 
            // ExportFMIniForm
            // 
            this.AcceptButton = this.ExportButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(793, 415);
            this.Controls.Add(this.BottomButtonsFLP);
            this.Controls.Add(this.DescrTextBox);
            this.Controls.Add(this.TagsTextBox);
            this.Controls.Add(this.ReleaseDateTextBox);
            this.Controls.Add(this.InfoFileTextBox);
            this.Controls.Add(this.NiceNameTextBox);
            this.Controls.Add(this.DescrLabel);
            this.Controls.Add(this.TagsLabel);
            this.Controls.Add(this.ReleaseDateLabel);
            this.Controls.Add(this.InfoFileLabel);
            this.Controls.Add(this.NiceNameLabel);
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(410, 312);
            this.Name = "ExportFMIniForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export fm.ini";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportFMIniForm_FormClosing);
            this.BottomButtonsFLP.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NiceNameLabel;
        private System.Windows.Forms.Label InfoFileLabel;
        private System.Windows.Forms.Label ReleaseDateLabel;
        private System.Windows.Forms.Label TagsLabel;
        private System.Windows.Forms.Label DescrLabel;
        private System.Windows.Forms.TextBox NiceNameTextBox;
        private System.Windows.Forms.TextBox InfoFileTextBox;
        private System.Windows.Forms.TextBox ReleaseDateTextBox;
        private System.Windows.Forms.TextBox TagsTextBox;
        private System.Windows.Forms.TextBox DescrTextBox;
        private System.Windows.Forms.FlowLayoutPanel BottomButtonsFLP;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button ExportButton;
    }
}