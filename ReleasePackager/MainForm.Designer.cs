namespace ReleasePackager;

sealed partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.ReleaseNotesTextBox = new System.Windows.Forms.TextBox();
            this.ReleaseNotesTTLGTextBox = new System.Windows.Forms.TextBox();
            this.ReleaseNotesLabel = new System.Windows.Forms.Label();
            this.ReleaseNotesTTLGLabel = new System.Windows.Forms.Label();
            this.ReleaseNotesMarkdownRawTextBox = new System.Windows.Forms.TextBox();
            this.ReleaseNotesMarkdownRawLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ReleaseNotesTextBox
            // 
            this.ReleaseNotesTextBox.Location = new System.Drawing.Point(16, 32);
            this.ReleaseNotesTextBox.Multiline = true;
            this.ReleaseNotesTextBox.Name = "ReleaseNotesTextBox";
            this.ReleaseNotesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ReleaseNotesTextBox.Size = new System.Drawing.Size(696, 280);
            this.ReleaseNotesTextBox.TabIndex = 0;
            this.ReleaseNotesTextBox.TextChanged += new System.EventHandler(this.ReleaseNotesTextBox_TextChanged);
            // 
            // ReleaseNotesTTLGTextBox
            // 
            this.ReleaseNotesTTLGTextBox.Location = new System.Drawing.Point(720, 32);
            this.ReleaseNotesTTLGTextBox.Multiline = true;
            this.ReleaseNotesTTLGTextBox.Name = "ReleaseNotesTTLGTextBox";
            this.ReleaseNotesTTLGTextBox.ReadOnly = true;
            this.ReleaseNotesTTLGTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ReleaseNotesTTLGTextBox.Size = new System.Drawing.Size(696, 280);
            this.ReleaseNotesTTLGTextBox.TabIndex = 0;
            // 
            // ReleaseNotesLabel
            // 
            this.ReleaseNotesLabel.AutoSize = true;
            this.ReleaseNotesLabel.Location = new System.Drawing.Point(16, 16);
            this.ReleaseNotesLabel.Name = "ReleaseNotesLabel";
            this.ReleaseNotesLabel.Size = new System.Drawing.Size(78, 13);
            this.ReleaseNotesLabel.TabIndex = 1;
            this.ReleaseNotesLabel.Text = "Release notes:";
            // 
            // ReleaseNotesTTLGLabel
            // 
            this.ReleaseNotesTTLGLabel.AutoSize = true;
            this.ReleaseNotesTTLGLabel.Location = new System.Drawing.Point(720, 16);
            this.ReleaseNotesTTLGLabel.Name = "ReleaseNotesTTLGLabel";
            this.ReleaseNotesTTLGLabel.Size = new System.Drawing.Size(115, 13);
            this.ReleaseNotesTTLGLabel.TabIndex = 1;
            this.ReleaseNotesTTLGLabel.Text = "Release notes (TTLG):";
            // 
            // ReleaseNotesMarkdownRawTextBox
            // 
            this.ReleaseNotesMarkdownRawTextBox.Location = new System.Drawing.Point(720, 336);
            this.ReleaseNotesMarkdownRawTextBox.Multiline = true;
            this.ReleaseNotesMarkdownRawTextBox.Name = "ReleaseNotesMarkdownRawTextBox";
            this.ReleaseNotesMarkdownRawTextBox.ReadOnly = true;
            this.ReleaseNotesMarkdownRawTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ReleaseNotesMarkdownRawTextBox.Size = new System.Drawing.Size(696, 280);
            this.ReleaseNotesMarkdownRawTextBox.TabIndex = 0;
            // 
            // ReleaseNotesMarkdownRawLabel
            // 
            this.ReleaseNotesMarkdownRawLabel.AutoSize = true;
            this.ReleaseNotesMarkdownRawLabel.Location = new System.Drawing.Point(720, 320);
            this.ReleaseNotesMarkdownRawLabel.Name = "ReleaseNotesMarkdownRawLabel";
            this.ReleaseNotesMarkdownRawLabel.Size = new System.Drawing.Size(157, 13);
            this.ReleaseNotesMarkdownRawLabel.TabIndex = 1;
            this.ReleaseNotesMarkdownRawLabel.Text = "Release notes (Markdown raw):";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1431, 670);
            this.Controls.Add(this.ReleaseNotesMarkdownRawLabel);
            this.Controls.Add(this.ReleaseNotesTTLGLabel);
            this.Controls.Add(this.ReleaseNotesLabel);
            this.Controls.Add(this.ReleaseNotesMarkdownRawTextBox);
            this.Controls.Add(this.ReleaseNotesTTLGTextBox);
            this.Controls.Add(this.ReleaseNotesTextBox);
            this.Name = "MainForm";
            this.Text = "AngelLoader Release Packager";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox ReleaseNotesTextBox;
    private System.Windows.Forms.TextBox ReleaseNotesTTLGTextBox;
    private System.Windows.Forms.Label ReleaseNotesLabel;
    private System.Windows.Forms.Label ReleaseNotesTTLGLabel;
    private System.Windows.Forms.TextBox ReleaseNotesMarkdownRawTextBox;
    private System.Windows.Forms.Label ReleaseNotesMarkdownRawLabel;
}
