namespace AngelLoader.Forms
{
    partial class SplashScreenForm
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
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
            this.LogoTextPictureBox.Size = new System.Drawing.Size(290, 50);
            this.LogoTextPictureBox.TabIndex = 9;
            this.LogoTextPictureBox.TabStop = false;
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Location = new System.Drawing.Point(152, 48);
            this.LogoPictureBox.Size = new System.Drawing.Size(48, 48);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.LogoPictureBox.TabIndex = 8;
            this.LogoPictureBox.TabStop = false;
            // 
            // SplashScreenMessageLabel
            // 
            this.SplashScreenMessageLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SplashScreenMessageLabel.Location = new System.Drawing.Point(0, 120);
            this.SplashScreenMessageLabel.Size = new System.Drawing.Size(648, 64);
            this.SplashScreenMessageLabel.TabIndex = 10;
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
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
