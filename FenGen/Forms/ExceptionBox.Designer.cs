namespace FenGen.Forms;

sealed partial class ExceptionBox
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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.ExceptionLabel = new System.Windows.Forms.Label();
        this.ExceptionTextBox = new System.Windows.Forms.TextBox();
        this.OKButton = new System.Windows.Forms.Button();
        this.CopyButton = new System.Windows.Forms.Button();
        this.IconPictureBox = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // ExceptionLabel
        // 
        this.ExceptionLabel.AutoSize = true;
        this.ExceptionLabel.Location = new System.Drawing.Point(56, 16);
        this.ExceptionLabel.Name = "ExceptionLabel";
        this.ExceptionLabel.Size = new System.Drawing.Size(102, 13);
        this.ExceptionLabel.TabIndex = 0;
        this.ExceptionLabel.Text = "Exception occurred:";
        // 
        // ExceptionTextBox
        // 
        this.ExceptionTextBox.Location = new System.Drawing.Point(56, 40);
        this.ExceptionTextBox.Multiline = true;
        this.ExceptionTextBox.Name = "ExceptionTextBox";
        this.ExceptionTextBox.ReadOnly = true;
        this.ExceptionTextBox.Size = new System.Drawing.Size(568, 344);
        this.ExceptionTextBox.TabIndex = 1;
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.OKButton.Location = new System.Drawing.Point(544, 392);
        this.OKButton.Name = "OKButton";
        this.OKButton.Size = new System.Drawing.Size(75, 23);
        this.OKButton.TabIndex = 2;
        this.OKButton.Text = "OK";
        this.OKButton.UseVisualStyleBackColor = true;
        // 
        // CopyButton
        // 
        this.CopyButton.Location = new System.Drawing.Point(464, 392);
        this.CopyButton.Name = "CopyButton";
        this.CopyButton.Size = new System.Drawing.Size(75, 23);
        this.CopyButton.TabIndex = 2;
        this.CopyButton.Text = "Copy";
        this.CopyButton.UseVisualStyleBackColor = true;
        this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
        // 
        // IconPictureBox
        // 
        this.IconPictureBox.Location = new System.Drawing.Point(12, 48);
        this.IconPictureBox.Name = "IconPictureBox";
        this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
        this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.IconPictureBox.TabIndex = 3;
        this.IconPictureBox.TabStop = false;
        // 
        // ExceptionBox
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.OKButton;
        this.ClientSize = new System.Drawing.Size(624, 422);
        this.Controls.Add(this.IconPictureBox);
        this.Controls.Add(this.CopyButton);
        this.Controls.Add(this.OKButton);
        this.Controls.Add(this.ExceptionTextBox);
        this.Controls.Add(this.ExceptionLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ExceptionBox";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ExceptionBox";
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label ExceptionLabel;
    private System.Windows.Forms.TextBox ExceptionTextBox;
    private System.Windows.Forms.Button OKButton;
    private System.Windows.Forms.Button CopyButton;
    private System.Windows.Forms.PictureBox IconPictureBox;
}