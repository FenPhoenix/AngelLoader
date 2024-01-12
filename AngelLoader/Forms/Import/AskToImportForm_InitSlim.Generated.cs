namespace AngelLoader.Forms;

sealed partial class AskToImportForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.DarkLoaderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.FMSelButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.NewDarkLoaderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.DontImportButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.MessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SuspendLayout();
        // 
        // DarkLoaderButton
        // 
        this.DarkLoaderButton.Location = new System.Drawing.Point(96, 48);
        this.DarkLoaderButton.Size = new System.Drawing.Size(112, 23);
        this.DarkLoaderButton.TabIndex = 1;
        this.DarkLoaderButton.Click += new System.EventHandler(this.ImportButtons_Click);
        // 
        // FMSelButton
        // 
        this.FMSelButton.Location = new System.Drawing.Point(96, 72);
        this.FMSelButton.Size = new System.Drawing.Size(112, 23);
        this.FMSelButton.TabIndex = 2;
        this.FMSelButton.Click += new System.EventHandler(this.ImportButtons_Click);
        // 
        // NewDarkLoaderButton
        // 
        this.NewDarkLoaderButton.Location = new System.Drawing.Point(96, 96);
        this.NewDarkLoaderButton.Size = new System.Drawing.Size(112, 23);
        this.NewDarkLoaderButton.TabIndex = 3;
        this.NewDarkLoaderButton.Click += new System.EventHandler(this.ImportButtons_Click);
        // 
        // DontImportButton
        // 
        this.DontImportButton.AutoSize = true;
        this.DontImportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.DontImportButton.Location = new System.Drawing.Point(96, 128);
        this.DontImportButton.TabIndex = 4;
        this.DontImportButton.Click += new System.EventHandler(this.DontImportButton_Click);
        // 
        // MessageLabel
        // 
        this.MessageLabel.AutoSize = true;
        this.MessageLabel.Location = new System.Drawing.Point(24, 16);
        this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // AskToImportForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.DontImportButton;
        this.ClientSize = new System.Drawing.Size(302, 167);
        this.Controls.Add(this.MessageLabel);
        this.Controls.Add(this.DontImportButton);
        this.Controls.Add(this.NewDarkLoaderButton);
        this.Controls.Add(this.FMSelButton);
        this.Controls.Add(this.DarkLoaderButton);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
