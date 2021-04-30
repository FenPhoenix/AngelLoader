using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

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
            VersionLabel = new DarkLabel();
            GitHubLinkLabel = new DarkLinkLabel();
            LicenseTextBox = new DarkTextBox();
            OKButton = new DarkButton();
            SevenZipLinkLabel = new DarkLinkLabel();
            SevenZipSharpLinkLabel = new DarkLinkLabel();
            FFmpegLinkLabel = new DarkLinkLabel();
            FFmpegDotNetLinkLabel = new DarkLinkLabel();
            SimpleHelpersDotNetLinkLabel = new DarkLinkLabel();
            UdeNetStandardLinkLabel = new DarkLinkLabel();
            OokiiDialogsLinkLabel = new DarkLinkLabel();
            NetCore3SysIOCompLinkLabel = new DarkLinkLabel();
            AngelLoaderUsesLabel = new DarkLabel();
            LogoTextPictureBox = new PictureBox();
            OKFlowLayoutPanel = new FlowLayoutPanel();
            BuildDateLabel = new DarkLabel();
            DarkUILinkLabel = new DarkLinkLabel();
            EasyHookLinkLabel = new DarkLinkLabel();
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
            VersionLabel.Location = new Point(352, 26);
            VersionLabel.TabIndex = 1;
            // 
            // GitHubLinkLabel
            // 
            GitHubLinkLabel.AutoSize = true;
            GitHubLinkLabel.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            GitHubLinkLabel.Location = new Point(72, 68);
            GitHubLinkLabel.TabIndex = 3;
            GitHubLinkLabel.TabStop = true;
            GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
            GitHubLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // LicenseTextBox
            // 
            LicenseTextBox.BackColor = SystemColors.Window;
            LicenseTextBox.DarkModeReadOnlyColorsAreDefault = true;
            LicenseTextBox.Location = new Point(32, 108);
            LicenseTextBox.Multiline = true;
            LicenseTextBox.ReadOnly = true;
            LicenseTextBox.ScrollBars = ScrollBars.Vertical;
            LicenseTextBox.Size = new Size(464, 240);
            LicenseTextBox.TabIndex = 4;
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
            SevenZipLinkLabel.Location = new Point(32, 388);
            SevenZipLinkLabel.TabIndex = 6;
            SevenZipLinkLabel.TabStop = true;
            SevenZipLinkLabel.Text = "7-Zip";
            SevenZipLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // SevenZipSharpLinkLabel
            // 
            SevenZipSharpLinkLabel.AutoSize = true;
            SevenZipSharpLinkLabel.Location = new Point(32, 404);
            SevenZipSharpLinkLabel.TabIndex = 7;
            SevenZipSharpLinkLabel.TabStop = true;
            SevenZipSharpLinkLabel.Text = "SquidBox.SevenZipSharp";
            SevenZipSharpLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // FFmpegLinkLabel
            // 
            FFmpegLinkLabel.AutoSize = true;
            FFmpegLinkLabel.Location = new Point(32, 421);
            FFmpegLinkLabel.TabIndex = 8;
            FFmpegLinkLabel.TabStop = true;
            FFmpegLinkLabel.Text = "ffmpeg";
            FFmpegLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // FFmpegDotNetLinkLabel
            // 
            FFmpegDotNetLinkLabel.AutoSize = true;
            FFmpegDotNetLinkLabel.Location = new Point(32, 437);
            FFmpegDotNetLinkLabel.TabIndex = 9;
            FFmpegDotNetLinkLabel.TabStop = true;
            FFmpegDotNetLinkLabel.Text = "FFmpeg.NET";
            FFmpegDotNetLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // SimpleHelpersDotNetLinkLabel
            // 
            SimpleHelpersDotNetLinkLabel.AutoSize = true;
            SimpleHelpersDotNetLinkLabel.Location = new Point(32, 453);
            SimpleHelpersDotNetLinkLabel.TabIndex = 10;
            SimpleHelpersDotNetLinkLabel.TabStop = true;
            SimpleHelpersDotNetLinkLabel.Text = "SimpleHelpers.Net";
            SimpleHelpersDotNetLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // UdeNetStandardLinkLabel
            // 
            UdeNetStandardLinkLabel.AutoSize = true;
            UdeNetStandardLinkLabel.Location = new Point(32, 468);
            UdeNetStandardLinkLabel.TabIndex = 11;
            UdeNetStandardLinkLabel.TabStop = true;
            UdeNetStandardLinkLabel.Text = "Ude.NetStandard";
            UdeNetStandardLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // OokiiDialogsLinkLabel
            // 
            OokiiDialogsLinkLabel.AutoSize = true;
            OokiiDialogsLinkLabel.Location = new Point(32, 484);
            OokiiDialogsLinkLabel.TabIndex = 12;
            OokiiDialogsLinkLabel.TabStop = true;
            OokiiDialogsLinkLabel.Text = "Ookii Dialogs";
            OokiiDialogsLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // NetCore3SysIOCompLinkLabel
            // 
            NetCore3SysIOCompLinkLabel.AutoSize = true;
            NetCore3SysIOCompLinkLabel.Location = new Point(32, 500);
            NetCore3SysIOCompLinkLabel.TabIndex = 13;
            NetCore3SysIOCompLinkLabel.TabStop = true;
            NetCore3SysIOCompLinkLabel.Text = ".NET Core 3 System.IO.Compression";
            NetCore3SysIOCompLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // AngelLoaderUsesLabel
            // 
            AngelLoaderUsesLabel.AutoSize = true;
            AngelLoaderUsesLabel.Location = new Point(32, 364);
            AngelLoaderUsesLabel.TabIndex = 5;
            // 
            // LogoTextPictureBox
            // 
            // Image gets set later
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
            OKFlowLayoutPanel.Location = new Point(0, 523);
            OKFlowLayoutPanel.Size = new Size(529, 40);
            OKFlowLayoutPanel.TabIndex = 0;
            // 
            // BuildDateLabel
            // 
            BuildDateLabel.AutoSize = true;
            BuildDateLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            // ForeColor gets set later
            BuildDateLabel.Location = new Point(352, 50);
            BuildDateLabel.TabIndex = 2;
            // 
            // DarkUILinkLabel
            // 
            DarkUILinkLabel.AutoSize = true;
            DarkUILinkLabel.Location = new Point(232, 388);
            DarkUILinkLabel.TabIndex = 14;
            DarkUILinkLabel.TabStop = true;
            DarkUILinkLabel.Text = "DarkUI";
            DarkUILinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // EasyHookLinkLabel
            // 
            EasyHookLinkLabel.AutoSize = true;
            EasyHookLinkLabel.Location = new Point(232, 404);
            EasyHookLinkLabel.TabIndex = 15;
            EasyHookLinkLabel.TabStop = true;
            EasyHookLinkLabel.Text = "EasyHook";
            EasyHookLinkLabel.LinkClicked += LinkLabels_LinkClicked;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            CancelButton = OKButton;
            ClientSize = new Size(529, 563);
            Controls.Add(BuildDateLabel);
            Controls.Add(VersionLabel);
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
            Controls.Add(EasyHookLinkLabel);
            Controls.Add(DarkUILinkLabel);
            Controls.Add(SevenZipLinkLabel);
            Controls.Add(LicenseTextBox);
            Controls.Add(GitHubLinkLabel);
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
