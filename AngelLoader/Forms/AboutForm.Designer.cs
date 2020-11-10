namespace AngelLoader.Forms
{
    partial class AboutForm
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

#if DEBUG
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.GitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.LicenseTextBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.SevenZipLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SevenZipSharpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.FFmpegLinkLabel = new System.Windows.Forms.LinkLabel();
            this.FFmpegDotNetLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SimpleHelpersDotNetLinkLabel = new System.Windows.Forms.LinkLabel();
            this.UdeNetStandardLinkLabel = new System.Windows.Forms.LinkLabel();
            this.OokiiDialogsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.NetCore3SysIOCompLinkLabel = new System.Windows.Forms.LinkLabel();
            this.AngelLoaderUsesLabel = new System.Windows.Forms.Label();
            this.LogoTextPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Location = new System.Drawing.Point(16, 16);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(48, 48);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.LogoPictureBox.TabIndex = 0;
            this.LogoPictureBox.TabStop = false;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VersionLabel.Location = new System.Drawing.Point(352, 40);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(71, 21);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "[version]";
            // 
            // GitHubLinkLabel
            // 
            this.GitHubLinkLabel.AutoSize = true;
            this.GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GitHubLinkLabel.Location = new System.Drawing.Point(72, 72);
            this.GitHubLinkLabel.Name = "GitHubLinkLabel";
            this.GitHubLinkLabel.Size = new System.Drawing.Size(269, 16);
            this.GitHubLinkLabel.TabIndex = 2;
            this.GitHubLinkLabel.TabStop = true;
            this.GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
            this.GitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // LicenseTextBox
            // 
            this.LicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.LicenseTextBox.Location = new System.Drawing.Point(32, 112);
            this.LicenseTextBox.Multiline = true;
            this.LicenseTextBox.Name = "LicenseTextBox";
            this.LicenseTextBox.ReadOnly = true;
            this.LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LicenseTextBox.Size = new System.Drawing.Size(464, 240);
            this.LicenseTextBox.TabIndex = 3;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Location = new System.Drawing.Point(445, 535);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // SevenZipLinkLabel
            // 
            this.SevenZipLinkLabel.AutoSize = true;
            this.SevenZipLinkLabel.Location = new System.Drawing.Point(32, 392);
            this.SevenZipLinkLabel.Name = "SevenZipLinkLabel";
            this.SevenZipLinkLabel.Size = new System.Drawing.Size(31, 13);
            this.SevenZipLinkLabel.TabIndex = 5;
            this.SevenZipLinkLabel.TabStop = true;
            this.SevenZipLinkLabel.Text = "7-Zip";
            this.SevenZipLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // SevenZipSharpLinkLabel
            // 
            this.SevenZipSharpLinkLabel.AutoSize = true;
            this.SevenZipSharpLinkLabel.Location = new System.Drawing.Point(32, 408);
            this.SevenZipSharpLinkLabel.Name = "SevenZipSharpLinkLabel";
            this.SevenZipSharpLinkLabel.Size = new System.Drawing.Size(129, 13);
            this.SevenZipSharpLinkLabel.TabIndex = 6;
            this.SevenZipSharpLinkLabel.TabStop = true;
            this.SevenZipSharpLinkLabel.Text = "SquidBox.SevenZipSharp";
            this.SevenZipSharpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // FFmpegLinkLabel
            // 
            this.FFmpegLinkLabel.AutoSize = true;
            this.FFmpegLinkLabel.Location = new System.Drawing.Point(32, 425);
            this.FFmpegLinkLabel.Name = "FFmpegLinkLabel";
            this.FFmpegLinkLabel.Size = new System.Drawing.Size(39, 13);
            this.FFmpegLinkLabel.TabIndex = 7;
            this.FFmpegLinkLabel.TabStop = true;
            this.FFmpegLinkLabel.Text = "ffmpeg";
            this.FFmpegLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // FFmpegDotNetLinkLabel
            // 
            this.FFmpegDotNetLinkLabel.AutoSize = true;
            this.FFmpegDotNetLinkLabel.Location = new System.Drawing.Point(32, 441);
            this.FFmpegDotNetLinkLabel.Name = "FFmpegDotNetLinkLabel";
            this.FFmpegDotNetLinkLabel.Size = new System.Drawing.Size(70, 13);
            this.FFmpegDotNetLinkLabel.TabIndex = 8;
            this.FFmpegDotNetLinkLabel.TabStop = true;
            this.FFmpegDotNetLinkLabel.Text = "FFmpeg.NET";
            this.FFmpegDotNetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // SimpleHelpersDotNetLinkLabel
            // 
            this.SimpleHelpersDotNetLinkLabel.AutoSize = true;
            this.SimpleHelpersDotNetLinkLabel.Location = new System.Drawing.Point(32, 457);
            this.SimpleHelpersDotNetLinkLabel.Name = "SimpleHelpersDotNetLinkLabel";
            this.SimpleHelpersDotNetLinkLabel.Size = new System.Drawing.Size(94, 13);
            this.SimpleHelpersDotNetLinkLabel.TabIndex = 9;
            this.SimpleHelpersDotNetLinkLabel.TabStop = true;
            this.SimpleHelpersDotNetLinkLabel.Text = "SimpleHelpers.Net";
            this.SimpleHelpersDotNetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // UdeNetStandardLinkLabel
            // 
            this.UdeNetStandardLinkLabel.AutoSize = true;
            this.UdeNetStandardLinkLabel.Location = new System.Drawing.Point(32, 472);
            this.UdeNetStandardLinkLabel.Name = "UdeNetStandardLinkLabel";
            this.UdeNetStandardLinkLabel.Size = new System.Drawing.Size(90, 13);
            this.UdeNetStandardLinkLabel.TabIndex = 10;
            this.UdeNetStandardLinkLabel.TabStop = true;
            this.UdeNetStandardLinkLabel.Text = "Ude.NetStandard";
            this.UdeNetStandardLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // OokiiDialogsLinkLabel
            // 
            this.OokiiDialogsLinkLabel.AutoSize = true;
            this.OokiiDialogsLinkLabel.Location = new System.Drawing.Point(32, 488);
            this.OokiiDialogsLinkLabel.Name = "OokiiDialogsLinkLabel";
            this.OokiiDialogsLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.OokiiDialogsLinkLabel.TabIndex = 11;
            this.OokiiDialogsLinkLabel.TabStop = true;
            this.OokiiDialogsLinkLabel.Text = "Ookii Dialogs";
            this.OokiiDialogsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // NetCore3SysIOCompLinkLabel
            // 
            this.NetCore3SysIOCompLinkLabel.AutoSize = true;
            this.NetCore3SysIOCompLinkLabel.Location = new System.Drawing.Point(32, 504);
            this.NetCore3SysIOCompLinkLabel.Name = "NetCore3SysIOCompLinkLabel";
            this.NetCore3SysIOCompLinkLabel.Size = new System.Drawing.Size(180, 13);
            this.NetCore3SysIOCompLinkLabel.TabIndex = 12;
            this.NetCore3SysIOCompLinkLabel.TabStop = true;
            this.NetCore3SysIOCompLinkLabel.Text = ".NET Core 3 System.IO.Compression";
            this.NetCore3SysIOCompLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // AngelLoaderUsesLabel
            // 
            this.AngelLoaderUsesLabel.AutoSize = true;
            this.AngelLoaderUsesLabel.Location = new System.Drawing.Point(32, 368);
            this.AngelLoaderUsesLabel.Name = "AngelLoaderUsesLabel";
            this.AngelLoaderUsesLabel.Size = new System.Drawing.Size(95, 13);
            this.AngelLoaderUsesLabel.TabIndex = 4;
            this.AngelLoaderUsesLabel.Text = "AngelLoader uses:";
            // 
            // LogoTextPictureBox
            // 
            this.LogoTextPictureBox.Image = global::AngelLoader.Properties.Resources.About;
            this.LogoTextPictureBox.InitialImage = null;
            this.LogoTextPictureBox.Location = new System.Drawing.Point(64, 16);
            this.LogoTextPictureBox.Name = "LogoTextPictureBox";
            this.LogoTextPictureBox.Size = new System.Drawing.Size(290, 50);
            this.LogoTextPictureBox.TabIndex = 7;
            this.LogoTextPictureBox.TabStop = false;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.OKButton;
            this.ClientSize = new System.Drawing.Size(529, 567);
            this.Controls.Add(this.LogoTextPictureBox);
            this.Controls.Add(this.AngelLoaderUsesLabel);
            this.Controls.Add(this.NetCore3SysIOCompLinkLabel);
            this.Controls.Add(this.OokiiDialogsLinkLabel);
            this.Controls.Add(this.UdeNetStandardLinkLabel);
            this.Controls.Add(this.SimpleHelpersDotNetLinkLabel);
            this.Controls.Add(this.FFmpegDotNetLinkLabel);
            this.Controls.Add(this.FFmpegLinkLabel);
            this.Controls.Add(this.SevenZipSharpLinkLabel);
            this.Controls.Add(this.SevenZipLinkLabel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.LicenseTextBox);
            this.Controls.Add(this.GitHubLinkLabel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.LogoPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About AngelLoader";
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
#endif

        private System.Windows.Forms.PictureBox LogoPictureBox;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.LinkLabel GitHubLinkLabel;
        private System.Windows.Forms.TextBox LicenseTextBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.LinkLabel SevenZipLinkLabel;
        private System.Windows.Forms.LinkLabel SevenZipSharpLinkLabel;
        private System.Windows.Forms.LinkLabel FFmpegLinkLabel;
        private System.Windows.Forms.LinkLabel FFmpegDotNetLinkLabel;
        private System.Windows.Forms.LinkLabel SimpleHelpersDotNetLinkLabel;
        private System.Windows.Forms.LinkLabel UdeNetStandardLinkLabel;
        private System.Windows.Forms.LinkLabel OokiiDialogsLinkLabel;
        private System.Windows.Forms.LinkLabel NetCore3SysIOCompLinkLabel;
        private System.Windows.Forms.Label AngelLoaderUsesLabel;
        private System.Windows.Forms.PictureBox LogoTextPictureBox;
    }
}