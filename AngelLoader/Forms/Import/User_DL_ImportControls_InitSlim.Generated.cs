namespace AngelLoader.Forms;

sealed partial class User_DL_ImportControls
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.AutodetectCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ChooseDarkLoaderIniLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.DarkLoaderIniTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.DarkLoaderIniBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.SuspendLayout();
        // 
        // AutodetectCheckBox
        // 
        this.AutodetectCheckBox.AutoSize = true;
        this.AutodetectCheckBox.Checked = true;
        this.AutodetectCheckBox.Location = new System.Drawing.Point(8, 32);
        this.AutodetectCheckBox.TabIndex = 1;
        this.AutodetectCheckBox.CheckedChanged += new System.EventHandler(this.AutodetectCheckBox_CheckedChanged);
        // 
        // ChooseDarkLoaderIniLabel
        // 
        this.ChooseDarkLoaderIniLabel.AutoSize = true;
        this.ChooseDarkLoaderIniLabel.Location = new System.Drawing.Point(8, 8);
        // 
        // DarkLoaderIniTextBox
        // 
        this.DarkLoaderIniTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.DarkLoaderIniTextBox.Location = new System.Drawing.Point(8, 56);
        this.DarkLoaderIniTextBox.ReadOnly = true;
        this.DarkLoaderIniTextBox.Size = new System.Drawing.Size(440, 20);
        this.DarkLoaderIniTextBox.TabIndex = 2;
        // 
        // DarkLoaderIniBrowseButton
        // 
        this.DarkLoaderIniBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.DarkLoaderIniBrowseButton.Enabled = false;
        this.DarkLoaderIniBrowseButton.Location = new System.Drawing.Point(448, 55);
        this.DarkLoaderIniBrowseButton.TabIndex = 3;
        this.DarkLoaderIniBrowseButton.Click += new System.EventHandler(this.DarkLoaderIniBrowseButton_Click);
        // 
        // User_DL_ImportControls
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.AutodetectCheckBox);
        this.Controls.Add(this.ChooseDarkLoaderIniLabel);
        this.Controls.Add(this.DarkLoaderIniTextBox);
        this.Controls.Add(this.DarkLoaderIniBrowseButton);
        this.Size = new System.Drawing.Size(531, 88);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
