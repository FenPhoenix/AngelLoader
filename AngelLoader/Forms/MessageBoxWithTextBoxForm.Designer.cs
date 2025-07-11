#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class MessageBoxWithTextBoxForm
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
            this.MessageTopLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.IconPictureBox = new System.Windows.Forms.PictureBox();
            this.ContentTLP = new System.Windows.Forms.TableLayoutPanel();
            this.MainFLP = new AngelLoader.Forms.CustomControls.PanelCustom();
            this.MainTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.MessageBottomLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.OuterTLP = new System.Windows.Forms.TableLayoutPanel();
            this.BottomFLP = new AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
            this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
            this.VerificationCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
            this.ContentTLP.SuspendLayout();
            this.MainFLP.SuspendLayout();
            this.OuterTLP.SuspendLayout();
            this.BottomFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // MessageTopLabel
            // 
            this.MessageTopLabel.AutoSize = true;
            this.MessageTopLabel.Location = new System.Drawing.Point(0, 18);
            this.MessageTopLabel.Margin = new System.Windows.Forms.Padding(0, 18, 3, 21);
            this.MessageTopLabel.Name = "MessageTopLabel";
            this.MessageTopLabel.Size = new System.Drawing.Size(74, 13);
            this.MessageTopLabel.TabIndex = 0;
            this.MessageTopLabel.Text = "[messageTop]";
            // 
            // IconPictureBox
            // 
            this.IconPictureBox.Location = new System.Drawing.Point(21, 21);
            this.IconPictureBox.Margin = new System.Windows.Forms.Padding(21, 21, 0, 3);
            this.IconPictureBox.Name = "IconPictureBox";
            this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
            this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IconPictureBox.TabIndex = 1;
            this.IconPictureBox.TabStop = false;
            // 
            // ContentTLP
            // 
            this.ContentTLP.BackColor = System.Drawing.SystemColors.Window;
            this.ContentTLP.ColumnCount = 2;
            this.ContentTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.ContentTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentTLP.Controls.Add(this.IconPictureBox, 0, 0);
            this.ContentTLP.Controls.Add(this.MainFLP, 1, 0);
            this.ContentTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentTLP.Location = new System.Drawing.Point(0, 0);
            this.ContentTLP.Margin = new System.Windows.Forms.Padding(0);
            this.ContentTLP.Name = "ContentTLP";
            this.ContentTLP.RowCount = 1;
            this.ContentTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentTLP.Size = new System.Drawing.Size(570, 294);
            this.ContentTLP.TabIndex = 0;
            // 
            // MainFLP
            // 
            this.MainFLP.Controls.Add(this.MainTextBox);
            this.MainFLP.Controls.Add(this.MessageTopLabel);
            this.MainFLP.Controls.Add(this.MessageBottomLabel);
            this.MainFLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainFLP.Location = new System.Drawing.Point(60, 0);
            this.MainFLP.Margin = new System.Windows.Forms.Padding(0);
            this.MainFLP.Name = "MainFLP";
            this.MainFLP.Size = new System.Drawing.Size(510, 294);
            this.MainFLP.TabIndex = 0;
            // 
            // MainTextBox
            // 
            this.MainTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTextBox.DarkModeReadOnlyColorsAreDefault = true;
            this.MainTextBox.Location = new System.Drawing.Point(0, 55);
            this.MainTextBox.Multiline = true;
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.ReadOnly = true;
            this.MainTextBox.Size = new System.Drawing.Size(493, 95);
            this.MainTextBox.TabIndex = 4;
            // 
            // MessageBottomLabel
            // 
            this.MessageBottomLabel.AutoSize = true;
            this.MessageBottomLabel.Location = new System.Drawing.Point(0, 176);
            this.MessageBottomLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 21);
            this.MessageBottomLabel.Name = "MessageBottomLabel";
            this.MessageBottomLabel.Size = new System.Drawing.Size(88, 13);
            this.MessageBottomLabel.TabIndex = 3;
            this.MessageBottomLabel.Text = "[messageBottom]";
            // 
            // OuterTLP
            // 
            this.OuterTLP.ColumnCount = 1;
            this.OuterTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OuterTLP.Controls.Add(this.ContentTLP, 0, 0);
            this.OuterTLP.Controls.Add(this.BottomFLP, 0, 1);
            this.OuterTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OuterTLP.Location = new System.Drawing.Point(0, 0);
            this.OuterTLP.Margin = new System.Windows.Forms.Padding(0);
            this.OuterTLP.Name = "OuterTLP";
            this.OuterTLP.RowCount = 2;
            this.OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.OuterTLP.Size = new System.Drawing.Size(570, 336);
            this.OuterTLP.TabIndex = 3;
            // 
            // BottomFLP
            // 
            this.BottomFLP.BackColor = System.Drawing.SystemColors.Control;
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(0, 294);
            this.BottomFLP.Margin = new System.Windows.Forms.Padding(0);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.BottomFLP.Size = new System.Drawing.Size(570, 42);
            this.BottomFLP.TabIndex = 1;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(480, 9);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(395, 9);
            this.OKButton.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            // 
            // VerificationCheckBox
            // 
            this.VerificationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.VerificationCheckBox.AutoSize = true;
            this.VerificationCheckBox.Location = new System.Drawing.Point(13, 309);
            this.VerificationCheckBox.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
            this.VerificationCheckBox.Name = "VerificationCheckBox";
            this.VerificationCheckBox.Size = new System.Drawing.Size(57, 17);
            this.VerificationCheckBox.TabIndex = 4;
            this.VerificationCheckBox.Text = "Check";
            // 
            // MessageBoxWithTextBoxForm
            // 
            this.AcceptButton = this.Cancel_Button;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(570, 336);
            this.Controls.Add(this.VerificationCheckBox);
            this.Controls.Add(this.OuterTLP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageBoxWithTextBoxForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MessageBoxCustomForm";
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
            this.ContentTLP.ResumeLayout(false);
            this.MainFLP.ResumeLayout(false);
            this.MainFLP.PerformLayout();
            this.OuterTLP.ResumeLayout(false);
            this.BottomFLP.ResumeLayout(false);
            this.BottomFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkLabel MessageTopLabel;
    private System.Windows.Forms.PictureBox IconPictureBox;
    private System.Windows.Forms.TableLayoutPanel ContentTLP;
    private System.Windows.Forms.TableLayoutPanel OuterTLP;
    private AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom BottomFLP;
    private AngelLoader.Forms.CustomControls.PanelCustom MainFLP;
    private AngelLoader.Forms.CustomControls.DarkLabel MessageBottomLabel;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private CustomControls.DarkTextBox MainTextBox;
    private CustomControls.DarkCheckBox VerificationCheckBox;
}
