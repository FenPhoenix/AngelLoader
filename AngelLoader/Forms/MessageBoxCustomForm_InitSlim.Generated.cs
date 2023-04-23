namespace AngelLoader.Forms;

partial class MessageBoxCustomForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.MessageTopLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.IconPictureBox = new System.Windows.Forms.PictureBox();
        this.ContentTLP = new System.Windows.Forms.TableLayoutPanel();
        this.MainFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.ChoiceListBox = new AngelLoader.Forms.CustomControls.DarkListBox();
        this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.SelectAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.MessageBottomLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.OuterTLP = new System.Windows.Forms.TableLayoutPanel();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
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
        this.MessageTopLabel.Margin = new System.Windows.Forms.Padding(0, 18, 3, 21);
        // 
        // IconPictureBox
        // 
        this.IconPictureBox.Location = new System.Drawing.Point(21, 21);
        this.IconPictureBox.Margin = new System.Windows.Forms.Padding(21, 21, 0, 3);
        this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
        this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
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
        this.ContentTLP.Margin = new System.Windows.Forms.Padding(0);
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
        this.MainFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.MainFLP.Margin = new System.Windows.Forms.Padding(0);
        this.MainFLP.Size = new System.Drawing.Size(510, 294);
        this.MainFLP.TabIndex = 0;
        // 
        // ChoiceListBox
        // 
        this.ChoiceListBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
        this.ChoiceListBox.MultiSelect = true;
        this.ChoiceListBox.Scrollable = true;
        this.ChoiceListBox.TabIndex = 1;
        this.ChoiceListBox.SelectedIndexChanged += new System.EventHandler(this.ChoiceListBox_SelectedIndexChanged);
        // 
        // SelectButtonsFLP
        // 
        this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
        this.SelectButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.SelectButtonsFLP.Margin = new System.Windows.Forms.Padding(0);
        this.SelectButtonsFLP.Size = new System.Drawing.Size(493, 23);
        this.SelectButtonsFLP.TabIndex = 2;
        // 
        // SelectAllButton
        // 
        this.SelectAllButton.AutoSize = true;
        this.SelectAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SelectAllButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0);
        this.SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SelectAllButton.TabIndex = 0;
        this.SelectAllButton.UseVisualStyleBackColor = true;
        this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
        // 
        // MessageBottomLabel
        // 
        this.MessageBottomLabel.AutoSize = true;
        this.MessageBottomLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 21);
        // 
        // OuterTLP
        // 
        this.OuterTLP.ColumnCount = 1;
        this.OuterTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.OuterTLP.Controls.Add(this.ContentTLP, 0, 0);
        this.OuterTLP.Controls.Add(this.BottomFLP, 0, 1);
        this.OuterTLP.Dock = System.Windows.Forms.DockStyle.Fill;
        this.OuterTLP.Margin = new System.Windows.Forms.Padding(0);
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
        this.BottomFLP.Margin = new System.Windows.Forms.Padding(0);
        this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.BottomFLP.Size = new System.Drawing.Size(570, 42);
        this.BottomFLP.TabIndex = 1;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.UseVisualStyleBackColor = true;
        // 
        // OKButton
        // 
        this.OKButton.AutoSize = true;
        this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
        this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.OKButton.TabIndex = 1;
        this.OKButton.UseVisualStyleBackColor = true;
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
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
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
}
