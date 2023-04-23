#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class User_DL_ImportControls
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

    #region Component Designer generated code

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.AutodetectCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ChooseDarkLoaderIniLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.DarkLoaderIniTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.DarkLoaderIniBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SuspendLayout();
        // 
        // AutodetectCheckBox
        // 
        this.AutodetectCheckBox.AutoSize = true;
        this.AutodetectCheckBox.Checked = true;
        this.AutodetectCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.AutodetectCheckBox.Location = new System.Drawing.Point(8, 32);
        this.AutodetectCheckBox.Name = "AutodetectCheckBox";
        this.AutodetectCheckBox.Size = new System.Drawing.Size(78, 17);
        this.AutodetectCheckBox.TabIndex = 1;
        this.AutodetectCheckBox.Text = "Autodetect";
        this.AutodetectCheckBox.UseVisualStyleBackColor = true;
        this.AutodetectCheckBox.CheckedChanged += new System.EventHandler(this.AutodetectCheckBox_CheckedChanged);
        // 
        // ChooseDarkLoaderIniLabel
        // 
        this.ChooseDarkLoaderIniLabel.AutoSize = true;
        this.ChooseDarkLoaderIniLabel.Location = new System.Drawing.Point(8, 8);
        this.ChooseDarkLoaderIniLabel.Name = "ChooseDarkLoaderIniLabel";
        this.ChooseDarkLoaderIniLabel.Size = new System.Drawing.Size(118, 13);
        this.ChooseDarkLoaderIniLabel.TabIndex = 0;
        this.ChooseDarkLoaderIniLabel.Text = "Choose DarkLoader.ini:";
        // 
        // DarkLoaderIniTextBox
        // 
        this.DarkLoaderIniTextBox.Location = new System.Drawing.Point(8, 56);
        this.DarkLoaderIniTextBox.Name = "DarkLoaderIniTextBox";
        this.DarkLoaderIniTextBox.ReadOnly = true;
        this.DarkLoaderIniTextBox.Size = new System.Drawing.Size(440, 20);
        this.DarkLoaderIniTextBox.TabIndex = 2;
        // 
        // DarkLoaderIniBrowseButton
        // 
        this.DarkLoaderIniBrowseButton.AutoSize = true;
        this.DarkLoaderIniBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.DarkLoaderIniBrowseButton.Enabled = false;
        this.DarkLoaderIniBrowseButton.Location = new System.Drawing.Point(448, 55);
        this.DarkLoaderIniBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.DarkLoaderIniBrowseButton.Name = "DarkLoaderIniBrowseButton";
        this.DarkLoaderIniBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.DarkLoaderIniBrowseButton.Size = new System.Drawing.Size(75, 23);
        this.DarkLoaderIniBrowseButton.TabIndex = 3;
        this.DarkLoaderIniBrowseButton.Text = "Browse...";
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
        this.Name = "User_DL_ImportControls";
        this.Size = new System.Drawing.Size(531, 88);
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkCheckBox AutodetectCheckBox;
    private AngelLoader.Forms.CustomControls.DarkLabel ChooseDarkLoaderIniLabel;
    private AngelLoader.Forms.CustomControls.DarkTextBox DarkLoaderIniTextBox;
    private AngelLoader.Forms.CustomControls.DarkButton DarkLoaderIniBrowseButton;
}
