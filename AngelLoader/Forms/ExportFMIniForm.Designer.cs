namespace AngelLoader.Forms
{
    partial class ExportFMIniForm
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
            this.SuspendLayout();
            // 
            // NiceNameLabel
            // 
            this.NiceNameLabel.AutoSize = true;
            this.NiceNameLabel.Location = new System.Drawing.Point(16, 16);
            this.NiceNameLabel.Name = "NiceNameLabel";
            this.NiceNameLabel.Size = new System.Drawing.Size(63, 13);
            this.NiceNameLabel.TabIndex = 0;
            this.NiceNameLabel.Text = "NiceName=";
            // 
            // InfoFileLabel
            // 
            this.InfoFileLabel.AutoSize = true;
            this.InfoFileLabel.Location = new System.Drawing.Point(16, 40);
            this.InfoFileLabel.Name = "InfoFileLabel";
            this.InfoFileLabel.Size = new System.Drawing.Size(47, 13);
            this.InfoFileLabel.TabIndex = 0;
            this.InfoFileLabel.Text = "InfoFile=";
            // 
            // ReleaseDateLabel
            // 
            this.ReleaseDateLabel.AutoSize = true;
            this.ReleaseDateLabel.Location = new System.Drawing.Point(16, 64);
            this.ReleaseDateLabel.Name = "ReleaseDateLabel";
            this.ReleaseDateLabel.Size = new System.Drawing.Size(75, 13);
            this.ReleaseDateLabel.TabIndex = 0;
            this.ReleaseDateLabel.Text = "ReleaseDate=";
            // 
            // TagsLabel
            // 
            this.TagsLabel.AutoSize = true;
            this.TagsLabel.Location = new System.Drawing.Point(16, 88);
            this.TagsLabel.Name = "TagsLabel";
            this.TagsLabel.Size = new System.Drawing.Size(37, 13);
            this.TagsLabel.TabIndex = 0;
            this.TagsLabel.Text = "Tags=";
            // 
            // DescrLabel
            // 
            this.DescrLabel.AutoSize = true;
            this.DescrLabel.Location = new System.Drawing.Point(16, 112);
            this.DescrLabel.Name = "DescrLabel";
            this.DescrLabel.Size = new System.Drawing.Size(41, 13);
            this.DescrLabel.TabIndex = 0;
            this.DescrLabel.Text = "Descr=";
            // 
            // NiceNameTextBox
            // 
            this.NiceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NiceNameTextBox.Location = new System.Drawing.Point(96, 16);
            this.NiceNameTextBox.Name = "NiceNameTextBox";
            this.NiceNameTextBox.ReadOnly = true;
            this.NiceNameTextBox.Size = new System.Drawing.Size(696, 20);
            this.NiceNameTextBox.TabIndex = 1;
            // 
            // InfoFileTextBox
            // 
            this.InfoFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoFileTextBox.Location = new System.Drawing.Point(96, 40);
            this.InfoFileTextBox.Name = "InfoFileTextBox";
            this.InfoFileTextBox.ReadOnly = true;
            this.InfoFileTextBox.Size = new System.Drawing.Size(696, 20);
            this.InfoFileTextBox.TabIndex = 1;
            // 
            // ReleaseDateTextBox
            // 
            this.ReleaseDateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReleaseDateTextBox.Location = new System.Drawing.Point(96, 64);
            this.ReleaseDateTextBox.Name = "ReleaseDateTextBox";
            this.ReleaseDateTextBox.ReadOnly = true;
            this.ReleaseDateTextBox.Size = new System.Drawing.Size(696, 20);
            this.ReleaseDateTextBox.TabIndex = 1;
            // 
            // TagsTextBox
            // 
            this.TagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TagsTextBox.Location = new System.Drawing.Point(96, 88);
            this.TagsTextBox.Name = "TagsTextBox";
            this.TagsTextBox.ReadOnly = true;
            this.TagsTextBox.Size = new System.Drawing.Size(696, 20);
            this.TagsTextBox.TabIndex = 1;
            // 
            // DescrTextBox
            // 
            this.DescrTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescrTextBox.Location = new System.Drawing.Point(96, 112);
            this.DescrTextBox.Name = "DescrTextBox";
            this.DescrTextBox.ReadOnly = true;
            this.DescrTextBox.Size = new System.Drawing.Size(696, 20);
            this.DescrTextBox.TabIndex = 1;
            // 
            // ExportFMIniForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
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
            this.Name = "ExportFMIniForm";
            this.Text = "ExportFMIniForm";
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
    }
}