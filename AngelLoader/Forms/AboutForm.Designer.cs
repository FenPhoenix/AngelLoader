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
            this.VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.GitHubLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.LicenseTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.SevenZipLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.SevenZipSharpLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.FFmpegLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.FFmpegDotNetLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.SimpleHelpersDotNetLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.UdeNetStandardLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.OokiiDialogsLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.NetCore3SysIOCompLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.AngelLoaderUsesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.LogoTextPictureBox = new System.Windows.Forms.PictureBox();
            this.OKFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.BuildDateLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.DarkUILinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.EasyHookLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.OpenSansLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).BeginInit();
            this.OKFlowLayoutPanel.SuspendLayout();
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
            this.VersionLabel.Location = new System.Drawing.Point(352, 26);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(71, 21);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "[version]";
            // 
            // GitHubLinkLabel
            // 
            this.GitHubLinkLabel.AutoSize = true;
            this.GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GitHubLinkLabel.Location = new System.Drawing.Point(72, 68);
            this.GitHubLinkLabel.Name = "GitHubLinkLabel";
            this.GitHubLinkLabel.Size = new System.Drawing.Size(269, 16);
            this.GitHubLinkLabel.TabIndex = 3;
            this.GitHubLinkLabel.TabStop = true;
            this.GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
            this.GitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // LicenseTextBox
            // 
            this.LicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.LicenseTextBox.DarkModeReadOnlyColorsAreDefault = true;
            this.LicenseTextBox.Location = new System.Drawing.Point(32, 108);
            this.LicenseTextBox.Multiline = true;
            this.LicenseTextBox.Name = "LicenseTextBox";
            this.LicenseTextBox.ReadOnly = true;
            this.LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LicenseTextBox.Size = new System.Drawing.Size(464, 240);
            this.LicenseTextBox.TabIndex = 4;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Location = new System.Drawing.Point(445, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // SevenZipLinkLabel
            // 
            this.SevenZipLinkLabel.AutoSize = true;
            this.SevenZipLinkLabel.Location = new System.Drawing.Point(32, 388);
            this.SevenZipLinkLabel.Name = "SevenZipLinkLabel";
            this.SevenZipLinkLabel.Size = new System.Drawing.Size(31, 13);
            this.SevenZipLinkLabel.TabIndex = 6;
            this.SevenZipLinkLabel.TabStop = true;
            this.SevenZipLinkLabel.Text = "7-Zip";
            this.SevenZipLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // SevenZipSharpLinkLabel
            // 
            this.SevenZipSharpLinkLabel.AutoSize = true;
            this.SevenZipSharpLinkLabel.Location = new System.Drawing.Point(32, 404);
            this.SevenZipSharpLinkLabel.Name = "SevenZipSharpLinkLabel";
            this.SevenZipSharpLinkLabel.Size = new System.Drawing.Size(129, 13);
            this.SevenZipSharpLinkLabel.TabIndex = 7;
            this.SevenZipSharpLinkLabel.TabStop = true;
            this.SevenZipSharpLinkLabel.Text = "SquidBox.SevenZipSharp";
            this.SevenZipSharpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // FFmpegLinkLabel
            // 
            this.FFmpegLinkLabel.AutoSize = true;
            this.FFmpegLinkLabel.Location = new System.Drawing.Point(32, 420);
            this.FFmpegLinkLabel.Name = "FFmpegLinkLabel";
            this.FFmpegLinkLabel.Size = new System.Drawing.Size(39, 13);
            this.FFmpegLinkLabel.TabIndex = 8;
            this.FFmpegLinkLabel.TabStop = true;
            this.FFmpegLinkLabel.Text = "ffmpeg";
            this.FFmpegLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // FFmpegDotNetLinkLabel
            // 
            this.FFmpegDotNetLinkLabel.AutoSize = true;
            this.FFmpegDotNetLinkLabel.Location = new System.Drawing.Point(32, 436);
            this.FFmpegDotNetLinkLabel.Name = "FFmpegDotNetLinkLabel";
            this.FFmpegDotNetLinkLabel.Size = new System.Drawing.Size(70, 13);
            this.FFmpegDotNetLinkLabel.TabIndex = 9;
            this.FFmpegDotNetLinkLabel.TabStop = true;
            this.FFmpegDotNetLinkLabel.Text = "FFmpeg.NET";
            this.FFmpegDotNetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // SimpleHelpersDotNetLinkLabel
            // 
            this.SimpleHelpersDotNetLinkLabel.AutoSize = true;
            this.SimpleHelpersDotNetLinkLabel.Location = new System.Drawing.Point(32, 452);
            this.SimpleHelpersDotNetLinkLabel.Name = "SimpleHelpersDotNetLinkLabel";
            this.SimpleHelpersDotNetLinkLabel.Size = new System.Drawing.Size(94, 13);
            this.SimpleHelpersDotNetLinkLabel.TabIndex = 10;
            this.SimpleHelpersDotNetLinkLabel.TabStop = true;
            this.SimpleHelpersDotNetLinkLabel.Text = "SimpleHelpers.Net";
            this.SimpleHelpersDotNetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // UdeNetStandardLinkLabel
            // 
            this.UdeNetStandardLinkLabel.AutoSize = true;
            this.UdeNetStandardLinkLabel.Location = new System.Drawing.Point(32, 468);
            this.UdeNetStandardLinkLabel.Name = "UdeNetStandardLinkLabel";
            this.UdeNetStandardLinkLabel.Size = new System.Drawing.Size(90, 13);
            this.UdeNetStandardLinkLabel.TabIndex = 11;
            this.UdeNetStandardLinkLabel.TabStop = true;
            this.UdeNetStandardLinkLabel.Text = "Ude.NetStandard";
            this.UdeNetStandardLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // OokiiDialogsLinkLabel
            // 
            this.OokiiDialogsLinkLabel.AutoSize = true;
            this.OokiiDialogsLinkLabel.Location = new System.Drawing.Point(32, 484);
            this.OokiiDialogsLinkLabel.Name = "OokiiDialogsLinkLabel";
            this.OokiiDialogsLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.OokiiDialogsLinkLabel.TabIndex = 12;
            this.OokiiDialogsLinkLabel.TabStop = true;
            this.OokiiDialogsLinkLabel.Text = "Ookii Dialogs";
            this.OokiiDialogsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // NetCore3SysIOCompLinkLabel
            // 
            this.NetCore3SysIOCompLinkLabel.AutoSize = true;
            this.NetCore3SysIOCompLinkLabel.Location = new System.Drawing.Point(32, 500);
            this.NetCore3SysIOCompLinkLabel.Name = "NetCore3SysIOCompLinkLabel";
            this.NetCore3SysIOCompLinkLabel.Size = new System.Drawing.Size(180, 13);
            this.NetCore3SysIOCompLinkLabel.TabIndex = 13;
            this.NetCore3SysIOCompLinkLabel.TabStop = true;
            this.NetCore3SysIOCompLinkLabel.Text = ".NET Core 3 System.IO.Compression";
            this.NetCore3SysIOCompLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // AngelLoaderUsesLabel
            // 
            this.AngelLoaderUsesLabel.AutoSize = true;
            this.AngelLoaderUsesLabel.Location = new System.Drawing.Point(32, 364);
            this.AngelLoaderUsesLabel.Name = "AngelLoaderUsesLabel";
            this.AngelLoaderUsesLabel.Size = new System.Drawing.Size(95, 13);
            this.AngelLoaderUsesLabel.TabIndex = 5;
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
            // OKFlowLayoutPanel
            // 
            this.OKFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKFlowLayoutPanel.Location = new System.Drawing.Point(0, 523);
            this.OKFlowLayoutPanel.Name = "OKFlowLayoutPanel";
            this.OKFlowLayoutPanel.Size = new System.Drawing.Size(529, 40);
            this.OKFlowLayoutPanel.TabIndex = 0;
            // 
            // BuildDateLabel
            // 
            this.BuildDateLabel.AutoSize = true;
            this.BuildDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BuildDateLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.BuildDateLabel.Location = new System.Drawing.Point(352, 50);
            this.BuildDateLabel.Name = "BuildDateLabel";
            this.BuildDateLabel.Size = new System.Drawing.Size(34, 13);
            this.BuildDateLabel.TabIndex = 2;
            this.BuildDateLabel.Text = "[date]";
            // 
            // DarkUILinkLabel
            // 
            this.DarkUILinkLabel.AutoSize = true;
            this.DarkUILinkLabel.Location = new System.Drawing.Point(232, 388);
            this.DarkUILinkLabel.Name = "DarkUILinkLabel";
            this.DarkUILinkLabel.Size = new System.Drawing.Size(41, 13);
            this.DarkUILinkLabel.TabIndex = 14;
            this.DarkUILinkLabel.TabStop = true;
            this.DarkUILinkLabel.Text = "DarkUI";
            this.DarkUILinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // EasyHookLinkLabel
            // 
            this.EasyHookLinkLabel.AutoSize = true;
            this.EasyHookLinkLabel.Location = new System.Drawing.Point(232, 404);
            this.EasyHookLinkLabel.Name = "EasyHookLinkLabel";
            this.EasyHookLinkLabel.Size = new System.Drawing.Size(56, 13);
            this.EasyHookLinkLabel.TabIndex = 15;
            this.EasyHookLinkLabel.TabStop = true;
            this.EasyHookLinkLabel.Text = "EasyHook";
            this.EasyHookLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // OpenSansLinkLabel
            // 
            this.OpenSansLinkLabel.AutoSize = true;
            this.OpenSansLinkLabel.Location = new System.Drawing.Point(232, 420);
            this.OpenSansLinkLabel.Name = "OpenSansLinkLabel";
            this.OpenSansLinkLabel.Size = new System.Drawing.Size(60, 13);
            this.OpenSansLinkLabel.TabIndex = 15;
            this.OpenSansLinkLabel.TabStop = true;
            this.OpenSansLinkLabel.Text = "Open Sans";
            this.OpenSansLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.OKButton;
            this.ClientSize = new System.Drawing.Size(529, 563);
            this.Controls.Add(this.BuildDateLabel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.OKFlowLayoutPanel);
            this.Controls.Add(this.LogoTextPictureBox);
            this.Controls.Add(this.AngelLoaderUsesLabel);
            this.Controls.Add(this.NetCore3SysIOCompLinkLabel);
            this.Controls.Add(this.OokiiDialogsLinkLabel);
            this.Controls.Add(this.UdeNetStandardLinkLabel);
            this.Controls.Add(this.SimpleHelpersDotNetLinkLabel);
            this.Controls.Add(this.FFmpegDotNetLinkLabel);
            this.Controls.Add(this.FFmpegLinkLabel);
            this.Controls.Add(this.SevenZipSharpLinkLabel);
            this.Controls.Add(this.OpenSansLinkLabel);
            this.Controls.Add(this.EasyHookLinkLabel);
            this.Controls.Add(this.DarkUILinkLabel);
            this.Controls.Add(this.SevenZipLinkLabel);
            this.Controls.Add(this.LicenseTextBox);
            this.Controls.Add(this.GitHubLinkLabel);
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
            this.OKFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
#endif

        private System.Windows.Forms.PictureBox LogoPictureBox;
        private AngelLoader.Forms.CustomControls.DarkLabel VersionLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel GitHubLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkTextBox LicenseTextBox;
        private AngelLoader.Forms.CustomControls.DarkButton OKButton;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel SevenZipLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel SevenZipSharpLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel FFmpegLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel FFmpegDotNetLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel SimpleHelpersDotNetLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel UdeNetStandardLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel OokiiDialogsLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel NetCore3SysIOCompLinkLabel;
        private AngelLoader.Forms.CustomControls.DarkLabel AngelLoaderUsesLabel;
        private System.Windows.Forms.PictureBox LogoTextPictureBox;
        private System.Windows.Forms.FlowLayoutPanel OKFlowLayoutPanel;
        private AngelLoader.Forms.CustomControls.DarkLabel BuildDateLabel;
        private AngelLoader.Forms.CustomControls.DarkLinkLabel DarkUILinkLabel;
        private CustomControls.DarkLinkLabel EasyHookLinkLabel;
        private CustomControls.DarkLinkLabel OpenSansLinkLabel;
    }
}