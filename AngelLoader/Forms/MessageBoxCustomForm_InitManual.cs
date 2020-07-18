namespace AngelLoader.Forms
{
    public sealed partial class MessageBoxCustomForm
    {
        // Button widths are kept defined because we pass them as minimum values to the text autosizer
        private void InitComponentManual()
        {
            MessageTopLabel = new System.Windows.Forms.Label();
            IconPictureBox = new System.Windows.Forms.PictureBox();
            ContentTLP = new System.Windows.Forms.TableLayoutPanel();
            MainFLP = new System.Windows.Forms.FlowLayoutPanel();
            ChoiceListBox = new System.Windows.Forms.ListBox();
            SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            SelectAllButton = new System.Windows.Forms.Button();
            MessageBottomLabel = new System.Windows.Forms.Label();
            OuterTLP = new System.Windows.Forms.TableLayoutPanel();
            BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            Cancel_Button = new System.Windows.Forms.Button();
            OKButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)IconPictureBox).BeginInit();
            ContentTLP.SuspendLayout();
            MainFLP.SuspendLayout();
            SelectButtonsFLP.SuspendLayout();
            OuterTLP.SuspendLayout();
            BottomFLP.SuspendLayout();
            SuspendLayout();
            // 
            // MessageTopLabel
            // 
            MessageTopLabel.AutoSize = true;
            MessageTopLabel.Margin = new System.Windows.Forms.Padding(0, 18, 3, 21);
            MessageTopLabel.TabIndex = 0;
            // 
            // IconPictureBox
            // 
            IconPictureBox.Location = new System.Drawing.Point(21, 21);
            IconPictureBox.Margin = new System.Windows.Forms.Padding(21, 21, 0, 3);
            IconPictureBox.Size = new System.Drawing.Size(32, 32);
            IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            IconPictureBox.TabIndex = 1;
            IconPictureBox.TabStop = false;
            // 
            // ContentTLP
            // 
            ContentTLP.BackColor = System.Drawing.SystemColors.Window;
            ContentTLP.ColumnCount = 2;
            ContentTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            ContentTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            ContentTLP.Controls.Add(IconPictureBox, 0, 0);
            ContentTLP.Controls.Add(MainFLP, 1, 0);
            ContentTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            ContentTLP.Location = new System.Drawing.Point(0, 0);
            ContentTLP.Margin = new System.Windows.Forms.Padding(0);
            ContentTLP.RowCount = 1;
            ContentTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            ContentTLP.Size = new System.Drawing.Size(570, 294);
            ContentTLP.TabIndex = 0;
            // 
            // MainFLP
            // 
            MainFLP.Controls.Add(MessageTopLabel);
            MainFLP.Controls.Add(ChoiceListBox);
            MainFLP.Controls.Add(SelectButtonsFLP);
            MainFLP.Controls.Add(MessageBottomLabel);
            MainFLP.Dock = System.Windows.Forms.DockStyle.Fill;
            MainFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            MainFLP.Location = new System.Drawing.Point(60, 0);
            MainFLP.Margin = new System.Windows.Forms.Padding(0);
            MainFLP.Size = new System.Drawing.Size(510, 294);
            MainFLP.TabIndex = 0;
            // 
            // ChoiceListBox
            // 
            ChoiceListBox.FormattingEnabled = true;
            ChoiceListBox.HorizontalScrollbar = true;
            ChoiceListBox.IntegralHeight = false;
            ChoiceListBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            ChoiceListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            ChoiceListBox.TabIndex = 1;
            ChoiceListBox.SelectedIndexChanged += ChoiceListBox_SelectedIndexChanged;
            // 
            // SelectButtonsFLP
            // 
            SelectButtonsFLP.Controls.Add(SelectAllButton);
            SelectButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            SelectButtonsFLP.Margin = new System.Windows.Forms.Padding(0);
            SelectButtonsFLP.Size = new System.Drawing.Size(493, 23);
            SelectButtonsFLP.TabIndex = 2;
            // 
            // SelectAllButton
            // 
            SelectAllButton.AutoSize = true;
            SelectAllButton.Margin = new System.Windows.Forms.Padding(0);
            SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            SelectAllButton.TabIndex = 0;
            SelectAllButton.UseVisualStyleBackColor = true;
            SelectAllButton.Click += SelectAllButton_Click;
            // 
            // MessageBottomLabel
            // 
            MessageBottomLabel.AutoSize = true;
            MessageBottomLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 21);
            MessageBottomLabel.TabIndex = 3;
            // 
            // OuterTLP
            // 
            OuterTLP.ColumnCount = 1;
            OuterTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            OuterTLP.Controls.Add(ContentTLP, 0, 0);
            OuterTLP.Controls.Add(BottomFLP, 0, 1);
            OuterTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            OuterTLP.Location = new System.Drawing.Point(0, 0);
            OuterTLP.Margin = new System.Windows.Forms.Padding(0);
            OuterTLP.RowCount = 2;
            OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            OuterTLP.Size = new System.Drawing.Size(570, 336);
            OuterTLP.TabIndex = 3;
            // 
            // BottomFLP
            // 
            BottomFLP.BackColor = System.Drawing.SystemColors.Control;
            BottomFLP.Controls.Add(Cancel_Button);
            BottomFLP.Controls.Add(OKButton);
            BottomFLP.Dock = System.Windows.Forms.DockStyle.Fill;
            BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            BottomFLP.Location = new System.Drawing.Point(0, 294);
            BottomFLP.Margin = new System.Windows.Forms.Padding(0);
            BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            BottomFLP.Size = new System.Drawing.Size(570, 42);
            BottomFLP.TabIndex = 1;
            // 
            // Cancel_Button
            // 
            Cancel_Button.AutoSize = true;
            Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Cancel_Button.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
            Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.AutoSize = true;
            OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            OKButton.Margin = new System.Windows.Forms.Padding(5, 9, 5, 3);
            OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            OKButton.TabIndex = 1;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // MessageBoxCustomForm
            // 
            AcceptButton = Cancel_Button;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Window;
            CancelButton = Cancel_Button;
            ClientSize = new System.Drawing.Size(570, 336);
            Controls.Add(OuterTLP);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            FormClosing += MessageBoxCustomForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)IconPictureBox).EndInit();
            ContentTLP.ResumeLayout(false);
            MainFLP.ResumeLayout(false);
            MainFLP.PerformLayout();
            SelectButtonsFLP.ResumeLayout(false);
            SelectButtonsFLP.PerformLayout();
            OuterTLP.ResumeLayout(false);
            BottomFLP.ResumeLayout(false);
            BottomFLP.PerformLayout();
            ResumeLayout(false);
        }
    }
}
