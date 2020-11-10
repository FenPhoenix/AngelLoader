using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    public sealed partial class AboutForm
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitComponentManual()
        {
            LogoPictureBox = new PictureBox();
            VersionLabel = new Label();
            GitHubLinkLabel = new LinkLabel();
            LicenseTextBox = new TextBox();
            OKButton = new Button();
            SevenZipLinkLabel = new LinkLabel();
            SevenZipSharpLinkLabel = new LinkLabel();
            FFmpegLinkLabel = new LinkLabel();
            FFmpegDotNetLinkLabel = new LinkLabel();
            SimpleHelpersDotNetLinkLabel = new LinkLabel();
            UdeNetStandardLinkLabel = new LinkLabel();
            OokiiDialogsLinkLabel = new LinkLabel();
            NetCore3SysIOCompLinkLabel = new LinkLabel();
            AngelLoaderUsesLabel = new Label();
            LogoTextPictureBox = new PictureBox();
            OKFlowLayoutPanel = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(LogoTextPictureBox)).BeginInit();
            OKFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // LogoPictureBox
            // 
            LogoPictureBox.Location = new Point(16, 16);
            LogoPictureBox.Size = new Size(48, 48);
            LogoPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            LogoPictureBox.TabIndex = 0;
            LogoPictureBox.TabStop = false;
            // 
            // VersionLabel
            // 
            VersionLabel.AutoSize = true;
            VersionLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            VersionLabel.Location = new Point(352, 40);
            VersionLabel.Size = new Size(71, 21);
            VersionLabel.TabIndex = 1;
            // 
            // GitHubLinkLabel
            // 
            GitHubLinkLabel.AutoSize = true;
            GitHubLinkLabel.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            GitHubLinkLabel.Location = new Point(72, 72);
            GitHubLinkLabel.Size = new Size(269, 16);
            GitHubLinkLabel.TabIndex = 2;
            GitHubLinkLabel.TabStop = true;
            GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
            GitHubLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // LicenseTextBox
            // 
            LicenseTextBox.BackColor = SystemColors.Window;
            LicenseTextBox.Location = new Point(32, 112);
            LicenseTextBox.Multiline = true;
            LicenseTextBox.ReadOnly = true;
            LicenseTextBox.ScrollBars = ScrollBars.Vertical;
            LicenseTextBox.Size = new Size(464, 240);
            LicenseTextBox.TabIndex = 3;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.DialogResult = DialogResult.Cancel;
            OKButton.Margin = new Padding(3, 8, 9, 3);
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.TabIndex = 0;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // SevenZipLinkLabel
            // 
            SevenZipLinkLabel.AutoSize = true;
            SevenZipLinkLabel.Location = new Point(32, 392);
            SevenZipLinkLabel.TabIndex = 5;
            SevenZipLinkLabel.TabStop = true;
            SevenZipLinkLabel.Text = "7-Zip";
            SevenZipLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // SevenZipSharpLinkLabel
            // 
            SevenZipSharpLinkLabel.AutoSize = true;
            SevenZipSharpLinkLabel.Location = new Point(32, 408);
            SevenZipSharpLinkLabel.TabIndex = 6;
            SevenZipSharpLinkLabel.TabStop = true;
            SevenZipSharpLinkLabel.Text = "SquidBox.SevenZipSharp";
            SevenZipSharpLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // FFmpegLinkLabel
            // 
            FFmpegLinkLabel.AutoSize = true;
            FFmpegLinkLabel.Location = new Point(32, 425);
            FFmpegLinkLabel.TabIndex = 7;
            FFmpegLinkLabel.TabStop = true;
            FFmpegLinkLabel.Text = "ffmpeg";
            FFmpegLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // FFmpegDotNetLinkLabel
            // 
            FFmpegDotNetLinkLabel.AutoSize = true;
            FFmpegDotNetLinkLabel.Location = new Point(32, 441);
            FFmpegDotNetLinkLabel.TabIndex = 8;
            FFmpegDotNetLinkLabel.TabStop = true;
            FFmpegDotNetLinkLabel.Text = "FFmpeg.NET";
            FFmpegDotNetLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // SimpleHelpersDotNetLinkLabel
            // 
            SimpleHelpersDotNetLinkLabel.AutoSize = true;
            SimpleHelpersDotNetLinkLabel.Location = new Point(32, 457);
            SimpleHelpersDotNetLinkLabel.TabIndex = 9;
            SimpleHelpersDotNetLinkLabel.TabStop = true;
            SimpleHelpersDotNetLinkLabel.Text = "SimpleHelpers.Net";
            SimpleHelpersDotNetLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // UdeNetStandardLinkLabel
            // 
            UdeNetStandardLinkLabel.AutoSize = true;
            UdeNetStandardLinkLabel.Location = new Point(32, 472);
            UdeNetStandardLinkLabel.TabIndex = 10;
            UdeNetStandardLinkLabel.TabStop = true;
            UdeNetStandardLinkLabel.Text = "Ude.NetStandard";
            UdeNetStandardLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // OokiiDialogsLinkLabel
            // 
            OokiiDialogsLinkLabel.AutoSize = true;
            OokiiDialogsLinkLabel.Location = new Point(32, 488);
            OokiiDialogsLinkLabel.TabIndex = 11;
            OokiiDialogsLinkLabel.TabStop = true;
            OokiiDialogsLinkLabel.Text = "Ookii Dialogs";
            OokiiDialogsLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // NetCore3SysIOCompLinkLabel
            // 
            NetCore3SysIOCompLinkLabel.AutoSize = true;
            NetCore3SysIOCompLinkLabel.Location = new Point(32, 504);
            NetCore3SysIOCompLinkLabel.TabIndex = 12;
            NetCore3SysIOCompLinkLabel.TabStop = true;
            NetCore3SysIOCompLinkLabel.Text = ".NET Core 3 System.IO.Compression";
            NetCore3SysIOCompLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabels_LinkClicked);
            // 
            // AngelLoaderUsesLabel
            // 
            AngelLoaderUsesLabel.AutoSize = true;
            AngelLoaderUsesLabel.Location = new Point(32, 368);
            AngelLoaderUsesLabel.TabIndex = 4;
            // 
            // LogoTextPictureBox
            // 
            LogoTextPictureBox.Image = Resources.About;
            LogoTextPictureBox.Location = new Point(64, 16);
            LogoTextPictureBox.Size = new Size(290, 50);
            LogoTextPictureBox.TabIndex = 7;
            LogoTextPictureBox.TabStop = false;
            // 
            // OKFlowLayoutPanel
            // 
            OKFlowLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKFlowLayoutPanel.Controls.Add(OKButton);
            OKFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            OKFlowLayoutPanel.Location = new Point(0, 527);
            OKFlowLayoutPanel.Size = new Size(529, 40);
            OKFlowLayoutPanel.TabIndex = 0;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            CancelButton = OKButton;
            ClientSize = new Size(529, 567);
            Controls.Add(OKFlowLayoutPanel);
            Controls.Add(LogoTextPictureBox);
            Controls.Add(AngelLoaderUsesLabel);
            Controls.Add(NetCore3SysIOCompLinkLabel);
            Controls.Add(OokiiDialogsLinkLabel);
            Controls.Add(UdeNetStandardLinkLabel);
            Controls.Add(SimpleHelpersDotNetLinkLabel);
            Controls.Add(FFmpegDotNetLinkLabel);
            Controls.Add(FFmpegLinkLabel);
            Controls.Add(SevenZipSharpLinkLabel);
            Controls.Add(SevenZipLinkLabel);
            Controls.Add(LicenseTextBox);
            Controls.Add(GitHubLinkLabel);
            Controls.Add(VersionLabel);
            Controls.Add(LogoPictureBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = " ";
            ((System.ComponentModel.ISupportInitialize)(LogoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(LogoTextPictureBox)).EndInit();
            OKFlowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
