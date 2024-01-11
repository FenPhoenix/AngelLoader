#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class AskToImportForm
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
            this.DarkLoaderButton.Name = "DarkLoaderButton";
            this.DarkLoaderButton.Size = new System.Drawing.Size(112, 23);
            this.DarkLoaderButton.TabIndex = 1;
            this.DarkLoaderButton.Text = "DarkLoader";
            this.DarkLoaderButton.Click += new System.EventHandler(this.ImportButtons_Click);
            // 
            // FMSelButton
            // 
            this.FMSelButton.Location = new System.Drawing.Point(96, 72);
            this.FMSelButton.Name = "FMSelButton";
            this.FMSelButton.Size = new System.Drawing.Size(112, 23);
            this.FMSelButton.TabIndex = 2;
            this.FMSelButton.Text = "FMSel";
            this.FMSelButton.Click += new System.EventHandler(this.ImportButtons_Click);
            // 
            // NewDarkLoaderButton
            // 
            this.NewDarkLoaderButton.Location = new System.Drawing.Point(96, 96);
            this.NewDarkLoaderButton.Name = "NewDarkLoaderButton";
            this.NewDarkLoaderButton.Size = new System.Drawing.Size(112, 23);
            this.NewDarkLoaderButton.TabIndex = 3;
            this.NewDarkLoaderButton.Text = "NewDarkLoader";
            this.NewDarkLoaderButton.Click += new System.EventHandler(this.ImportButtons_Click);
            // 
            // DontImportButton
            // 
            this.DontImportButton.AutoSize = true;
            this.DontImportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.DontImportButton.Location = new System.Drawing.Point(96, 128);
            this.DontImportButton.Name = "DontImportButton";
            this.DontImportButton.Size = new System.Drawing.Size(112, 23);
            this.DontImportButton.TabIndex = 4;
            this.DontImportButton.Text = "Don\'t import";
            this.DontImportButton.Click += new System.EventHandler(this.DontImportButton_Click);
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Location = new System.Drawing.Point(24, 16);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(257, 13);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.Text = "Do you want to import your data from another loader?";
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
            this.Name = "AskToImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import";
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion
    private CustomControls.DarkButton DarkLoaderButton;
    private CustomControls.DarkButton FMSelButton;
    private CustomControls.DarkButton NewDarkLoaderButton;
    private CustomControls.DarkButton DontImportButton;
    private CustomControls.DarkLabel MessageLabel;
}
