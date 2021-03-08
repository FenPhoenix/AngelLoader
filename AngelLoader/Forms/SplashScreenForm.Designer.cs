#define FenGen_GenSlimDesignerFromThis

namespace AngelLoader.Forms
{
    partial class SplashScreenForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

#if DEBUG
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreenForm));
            this.LogoTextPictureBox = new System.Windows.Forms.PictureBox();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.SplashScreenMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // LogoTextPictureBox
            // 
            this.LogoTextPictureBox.InitialImage = null;
            this.LogoTextPictureBox.Location = new System.Drawing.Point(200, 48);
            this.LogoTextPictureBox.Name = "LogoTextPictureBox";
            this.LogoTextPictureBox.Size = new System.Drawing.Size(290, 50);
            this.LogoTextPictureBox.TabIndex = 9;
            this.LogoTextPictureBox.TabStop = false;
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Location = new System.Drawing.Point(152, 48);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(48, 48);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.LogoPictureBox.TabIndex = 8;
            this.LogoPictureBox.TabStop = false;
            // 
            // SplashScreenMessageLabel
            // 
            this.SplashScreenMessageLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SplashScreenMessageLabel.Location = new System.Drawing.Point(0, 120);
            this.SplashScreenMessageLabel.Name = "SplashScreenMessageLabel";
            this.SplashScreenMessageLabel.Size = new System.Drawing.Size(648, 64);
            this.SplashScreenMessageLabel.TabIndex = 10;
            this.SplashScreenMessageLabel.Text = "Loading main app...";
            this.SplashScreenMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.SplashScreenMessageLabel.UseMnemonic = false;
            // 
            // SplashScreenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(648, 184);
            this.ControlBox = false;
            this.Controls.Add(this.SplashScreenMessageLabel);
            this.Controls.Add(this.LogoTextPictureBox);
            this.Controls.Add(this.LogoPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashScreenForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endif

        #endregion

        private System.Windows.Forms.PictureBox LogoTextPictureBox;
        private System.Windows.Forms.PictureBox LogoPictureBox;
        private CustomControls.DarkLabel SplashScreenMessageLabel;
    }
}
