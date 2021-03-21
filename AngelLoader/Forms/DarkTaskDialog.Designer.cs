﻿#define FenGen_GenSlimDesignerFromThis

namespace AngelLoader.Forms
{
    partial class DarkTaskDialog
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

#if DEBUG
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.IconPictureBox = new System.Windows.Forms.PictureBox();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.NoButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.YesButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.VerificationCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
            this.BottomFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // IconPictureBox
            // 
            this.IconPictureBox.Location = new System.Drawing.Point(10, 10);
            this.IconPictureBox.Name = "IconPictureBox";
            this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
            this.IconPictureBox.TabIndex = 0;
            this.IconPictureBox.TabStop = false;
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Location = new System.Drawing.Point(52, 18);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(55, 13);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.Text = "[message]";
            // 
            // BottomFLP
            // 
            this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.NoButton);
            this.BottomFLP.Controls.Add(this.YesButton);
            this.BottomFLP.Controls.Add(this.VerificationCheckBox);
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(0, 169);
            this.BottomFLP.Margin = new System.Windows.Forms.Padding(0);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.BottomFLP.Size = new System.Drawing.Size(532, 42);
            this.BottomFLP.TabIndex = 1;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.DarkModeBackColor = null;
            this.Cancel_Button.DarkModeHoverColor = null;
            this.Cancel_Button.DarkModePressedColor = null;
            this.Cancel_Button.Location = new System.Drawing.Point(444, 9);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 2;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseMnemonic = false;
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // NoButton
            // 
            this.NoButton.AutoSize = true;
            this.NoButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.NoButton.DarkModeBackColor = null;
            this.NoButton.DarkModeHoverColor = null;
            this.NoButton.DarkModePressedColor = null;
            this.NoButton.Location = new System.Drawing.Point(363, 9);
            this.NoButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.NoButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.NoButton.Name = "NoButton";
            this.NoButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.NoButton.Size = new System.Drawing.Size(75, 23);
            this.NoButton.TabIndex = 1;
            this.NoButton.Text = "No";
            this.NoButton.UseMnemonic = false;
            this.NoButton.UseVisualStyleBackColor = true;
            // 
            // YesButton
            // 
            this.YesButton.AutoSize = true;
            this.YesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.YesButton.DarkModeBackColor = null;
            this.YesButton.DarkModeHoverColor = null;
            this.YesButton.DarkModePressedColor = null;
            this.YesButton.Location = new System.Drawing.Point(282, 9);
            this.YesButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.YesButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.YesButton.Name = "YesButton";
            this.YesButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.YesButton.Size = new System.Drawing.Size(75, 23);
            this.YesButton.TabIndex = 0;
            this.YesButton.Text = "Yes";
            this.YesButton.UseMnemonic = false;
            this.YesButton.UseVisualStyleBackColor = true;
            // 
            // VerificationCheckBox
            // 
            this.VerificationCheckBox.AutoSize = true;
            this.VerificationCheckBox.Location = new System.Drawing.Point(205, 13);
            this.VerificationCheckBox.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.VerificationCheckBox.Name = "VerificationCheckBox";
            this.VerificationCheckBox.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.VerificationCheckBox.Size = new System.Drawing.Size(69, 17);
            this.VerificationCheckBox.TabIndex = 3;
            this.VerificationCheckBox.Text = "Check";
            this.VerificationCheckBox.UseMnemonic = false;
            this.VerificationCheckBox.UseVisualStyleBackColor = true;
            // 
            // DarkTaskDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 211);
            this.Controls.Add(this.BottomFLP);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.IconPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DarkTaskDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DarkTaskDialog";
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
            this.BottomFLP.ResumeLayout(false);
            this.BottomFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endif

        #endregion

        private System.Windows.Forms.PictureBox IconPictureBox;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.FlowLayoutPanel BottomFLP;
        private CustomControls.DarkButton Cancel_Button;
        private CustomControls.DarkButton NoButton;
        private CustomControls.DarkButton YesButton;
        private CustomControls.DarkCheckBox VerificationCheckBox;
    }
}
