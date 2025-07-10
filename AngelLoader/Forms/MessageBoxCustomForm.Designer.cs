#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class MessageBoxCustomForm
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
        this.MainFLP = new System.Windows.Forms.Panel();
        this.ChoiceListBox = new AngelLoader.Forms.CustomControls.DarkListBox();
        this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.SelectAllButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.MessageBottomLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.OuterTLP = new System.Windows.Forms.TableLayoutPanel();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
        this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
        this.ContentTLP.SuspendLayout();
        this.MainFLP.SuspendLayout();
        this.SelectButtonsFLP.SuspendLayout();
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
        this.MainFLP.Controls.Add(this.MessageTopLabel);
        this.MainFLP.Controls.Add(this.ChoiceListBox);
        this.MainFLP.Controls.Add(this.SelectButtonsFLP);
        this.MainFLP.Controls.Add(this.MessageBottomLabel);
        this.MainFLP.Dock = System.Windows.Forms.DockStyle.Fill;
        this.MainFLP.Location = new System.Drawing.Point(60, 0);
        this.MainFLP.Margin = new System.Windows.Forms.Padding(0);
        this.MainFLP.Name = "MainFLP";
        this.MainFLP.Size = new System.Drawing.Size(510, 294);
        this.MainFLP.TabIndex = 0;
        // 
        // ChoiceListBox
        // 
        this.ChoiceListBox.Location = new System.Drawing.Point(0, 55);
        this.ChoiceListBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
        this.ChoiceListBox.MultiSelect = true;
        this.ChoiceListBox.Name = "ChoiceListBox";
        this.ChoiceListBox.Scrollable = true;
        this.ChoiceListBox.Size = new System.Drawing.Size(493, 95);
        this.ChoiceListBox.TabIndex = 1;
        this.ChoiceListBox.SelectedIndexChanged += new System.EventHandler(this.ChoiceListBox_SelectedIndexChanged);
        // 
        // SelectButtonsFLP
        // 
        this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
        this.SelectButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.SelectButtonsFLP.Location = new System.Drawing.Point(0, 150);
        this.SelectButtonsFLP.Margin = new System.Windows.Forms.Padding(0);
        this.SelectButtonsFLP.Name = "SelectButtonsFLP";
        this.SelectButtonsFLP.Size = new System.Drawing.Size(493, 23);
        this.SelectButtonsFLP.TabIndex = 2;
        // 
        // SelectAllButton
        // 
        this.SelectAllButton.Location = new System.Drawing.Point(418, 0);
        this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0);
        this.SelectAllButton.Name = "SelectAllButton";
        this.SelectAllButton.TabIndex = 0;
        this.SelectAllButton.Text = "Select all";
        this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
        // 
        // MessageBottomLabel
        // 
        this.MessageBottomLabel.AutoSize = true;
        this.MessageBottomLabel.Location = new System.Drawing.Point(0, 176);
        this.MessageBottomLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 21);
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
        this.BottomFLP.Dock = System.Windows.Forms.DockStyle.Fill;
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
        // MessageBoxCustomForm
        // 
        this.AcceptButton = this.Cancel_Button;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.SystemColors.Window;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(570, 336);
        this.Controls.Add(this.OuterTLP);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "MessageBoxCustomForm";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "MessageBoxCustomForm";
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
        this.ContentTLP.ResumeLayout(false);
        this.MainFLP.ResumeLayout(false);
        this.MainFLP.PerformLayout();
        this.SelectButtonsFLP.ResumeLayout(false);
        this.SelectButtonsFLP.PerformLayout();
        this.OuterTLP.ResumeLayout(false);
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkLabel MessageTopLabel;
    private System.Windows.Forms.PictureBox IconPictureBox;
    private System.Windows.Forms.TableLayoutPanel ContentTLP;
    private System.Windows.Forms.TableLayoutPanel OuterTLP;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private System.Windows.Forms.Panel MainFLP;
    [AL_Common.FenGenAttributes.FenGenForceRemoveSizeAttribute]
    private AngelLoader.Forms.CustomControls.DarkListBox ChoiceListBox;
    private AngelLoader.Forms.CustomControls.DarkLabel MessageBottomLabel;
    private System.Windows.Forms.FlowLayoutPanel SelectButtonsFLP;
    private AngelLoader.Forms.CustomControls.StandardButton SelectAllButton;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
}
