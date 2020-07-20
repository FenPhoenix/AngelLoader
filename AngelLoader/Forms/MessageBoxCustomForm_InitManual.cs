using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class MessageBoxCustomForm
    {
        private void InitComponentManual()
        {
            MessageTopLabel = new Label();
            IconPictureBox = new PictureBox();
            ContentTLP = new TableLayoutPanel();
            MainFLP = new FlowLayoutPanel();
            ChoiceListBox = new ListBox();
            SelectButtonsFLP = new FlowLayoutPanel();
            SelectAllButton = new Button();
            MessageBottomLabel = new Label();
            OuterTLP = new TableLayoutPanel();
            BottomFLP = new FlowLayoutPanel();
            Cancel_Button = new Button();
            OKButton = new Button();
            ((ISupportInitialize)IconPictureBox).BeginInit();
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
            MessageTopLabel.Margin = new Padding(0, 18, 3, 21);
            MessageTopLabel.TabIndex = 0;
            // 
            // IconPictureBox
            // 
            IconPictureBox.Location = new Point(21, 21);
            IconPictureBox.Margin = new Padding(21, 21, 0, 3);
            IconPictureBox.Size = new Size(32, 32);
            IconPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            IconPictureBox.TabIndex = 1;
            IconPictureBox.TabStop = false;
            // 
            // ContentTLP
            // 
            ContentTLP.BackColor = SystemColors.Window;
            ContentTLP.ColumnCount = 2;
            ContentTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            ContentTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ContentTLP.Controls.Add(IconPictureBox, 0, 0);
            ContentTLP.Controls.Add(MainFLP, 1, 0);
            ContentTLP.Dock = DockStyle.Fill;
            ContentTLP.Location = new Point(0, 0);
            ContentTLP.Margin = new Padding(0);
            ContentTLP.RowCount = 1;
            ContentTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentTLP.Size = new Size(570, 294);
            ContentTLP.TabIndex = 0;
            // 
            // MainFLP
            // 
            MainFLP.Controls.Add(MessageTopLabel);
            MainFLP.Controls.Add(ChoiceListBox);
            MainFLP.Controls.Add(SelectButtonsFLP);
            MainFLP.Controls.Add(MessageBottomLabel);
            MainFLP.Dock = DockStyle.Fill;
            MainFLP.FlowDirection = FlowDirection.TopDown;
            MainFLP.Location = new Point(60, 0);
            MainFLP.Margin = new Padding(0);
            MainFLP.Size = new Size(510, 294);
            MainFLP.TabIndex = 0;
            // 
            // ChoiceListBox
            // 
            ChoiceListBox.FormattingEnabled = true;
            ChoiceListBox.HorizontalScrollbar = true;
            ChoiceListBox.IntegralHeight = false;
            ChoiceListBox.Margin = new Padding(0, 3, 3, 0);
            ChoiceListBox.SelectionMode = SelectionMode.MultiExtended;
            ChoiceListBox.TabIndex = 1;
            ChoiceListBox.SelectedIndexChanged += ChoiceListBox_SelectedIndexChanged;
            // 
            // SelectButtonsFLP
            // 
            SelectButtonsFLP.Controls.Add(SelectAllButton);
            SelectButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            SelectButtonsFLP.Margin = new Padding(0);
            SelectButtonsFLP.Size = new Size(493, 23);
            SelectButtonsFLP.TabIndex = 2;
            // 
            // SelectAllButton
            // 
            SelectAllButton.AutoSize = true;
            SelectAllButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SelectAllButton.MinimumSize = new Size(75, 23);
            SelectAllButton.Margin = new Padding(0);
            SelectAllButton.Padding = new Padding(6, 0, 6, 0);
            SelectAllButton.TabIndex = 0;
            SelectAllButton.UseVisualStyleBackColor = true;
            SelectAllButton.Click += SelectAllButton_Click;
            // 
            // MessageBottomLabel
            // 
            MessageBottomLabel.AutoSize = true;
            MessageBottomLabel.Margin = new Padding(0, 3, 3, 21);
            MessageBottomLabel.TabIndex = 3;
            // 
            // OuterTLP
            // 
            OuterTLP.ColumnCount = 1;
            OuterTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            OuterTLP.Controls.Add(ContentTLP, 0, 0);
            OuterTLP.Controls.Add(BottomFLP, 0, 1);
            OuterTLP.Dock = DockStyle.Fill;
            OuterTLP.Location = new Point(0, 0);
            OuterTLP.Margin = new Padding(0);
            OuterTLP.RowCount = 2;
            OuterTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            OuterTLP.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            OuterTLP.Size = new Size(570, 336);
            OuterTLP.TabIndex = 3;
            // 
            // BottomFLP
            // 
            BottomFLP.BackColor = SystemColors.Control;
            BottomFLP.Controls.Add(Cancel_Button);
            BottomFLP.Controls.Add(OKButton);
            BottomFLP.Dock = DockStyle.Fill;
            BottomFLP.FlowDirection = FlowDirection.RightToLeft;
            BottomFLP.Location = new Point(0, 294);
            BottomFLP.Margin = new Padding(0);
            BottomFLP.Padding = new Padding(0, 0, 10, 0);
            BottomFLP.Size = new Size(570, 42);
            BottomFLP.TabIndex = 1;
            // 
            // Cancel_Button
            // 
            Cancel_Button.AutoSize = true;
            Cancel_Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Cancel_Button.MinimumSize = new Size(75, 23);
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Margin = new Padding(5, 9, 5, 3);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Margin = new Padding(5, 9, 5, 3);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.TabIndex = 1;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // MessageBoxCustomForm
            // 
            AcceptButton = Cancel_Button;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            CancelButton = Cancel_Button;
            ClientSize = new Size(570, 336);
            Controls.Add(OuterTLP);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += MessageBoxCustomForm_FormClosing;
            ((ISupportInitialize)IconPictureBox).EndInit();
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
