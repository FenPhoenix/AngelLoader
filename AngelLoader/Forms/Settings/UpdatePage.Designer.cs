#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class UpdatePage
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
            this.PagePanel = new System.Windows.Forms.Panel();
            this.UpdateOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.CheckForUpdatesOnStartupCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.PagePanel.SuspendLayout();
            this.UpdateOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.UpdateOptionsGroupBox);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.TabIndex = 0;
            // 
            // UpdateOptionsGroupBox
            // 
            this.UpdateOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateOptionsGroupBox.Controls.Add(this.CheckForUpdatesOnStartupCheckBox);
            this.UpdateOptionsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.UpdateOptionsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.UpdateOptionsGroupBox.Name = "UpdateOptionsGroupBox";
            this.UpdateOptionsGroupBox.Size = new System.Drawing.Size(424, 56);
            this.UpdateOptionsGroupBox.TabIndex = 1;
            this.UpdateOptionsGroupBox.TabStop = false;
            this.UpdateOptionsGroupBox.Text = "Update options";
            // 
            // CheckForUpdatesOnStartupCheckBox
            // 
            this.CheckForUpdatesOnStartupCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckForUpdatesOnStartupCheckBox.Location = new System.Drawing.Point(16, 16);
            this.CheckForUpdatesOnStartupCheckBox.Name = "CheckForUpdatesOnStartupCheckBox";
            this.CheckForUpdatesOnStartupCheckBox.Size = new System.Drawing.Size(392, 32);
            this.CheckForUpdatesOnStartupCheckBox.TabIndex = 0;
            this.CheckForUpdatesOnStartupCheckBox.Text = "Check for updates on startup";
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 48);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
            this.DummyAutoScrollPanel.TabIndex = 0;
            // 
            // UpdatePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "UpdatePage";
            this.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.ResumeLayout(false);
            this.UpdateOptionsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal CustomControls.DarkGroupBox UpdateOptionsGroupBox;
    internal System.Windows.Forms.Control DummyAutoScrollPanel;
    internal CustomControls.DarkCheckBox CheckForUpdatesOnStartupCheckBox;
}
