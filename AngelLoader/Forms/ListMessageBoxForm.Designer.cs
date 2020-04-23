namespace AngelLoader.Forms
{
    partial class ListMessageBoxForm
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
            this.MessageTopLabel = new System.Windows.Forms.Label();
            this.IconPictureBox = new System.Windows.Forms.PictureBox();
            this.ContentTLP = new System.Windows.Forms.TableLayoutPanel();
            this.MainFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.ChoiceListBox = new System.Windows.Forms.ListBox();
            this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.SelectAllButton = new System.Windows.Forms.Button();
            this.MessageBottomLabel = new System.Windows.Forms.Label();
            this.OuterTLP = new System.Windows.Forms.TableLayoutPanel();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
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
            this.MessageTopLabel.Location = new System.Drawing.Point(0, 0);
            this.MessageTopLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.MessageTopLabel.Name = "MessageTopLabel";
            this.MessageTopLabel.Size = new System.Drawing.Size(74, 13);
            this.MessageTopLabel.TabIndex = 0;
            this.MessageTopLabel.Text = "[messageTop]";
            // 
            // IconPictureBox
            // 
            this.IconPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.IconPictureBox.Location = new System.Drawing.Point(21, 3);
            this.IconPictureBox.Margin = new System.Windows.Forms.Padding(21, 3, 0, 3);
            this.IconPictureBox.Name = "IconPictureBox";
            this.IconPictureBox.Size = new System.Drawing.Size(32, 282);
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
            this.ContentTLP.Location = new System.Drawing.Point(3, 3);
            this.ContentTLP.Name = "ContentTLP";
            this.ContentTLP.RowCount = 1;
            this.ContentTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentTLP.Size = new System.Drawing.Size(564, 288);
            this.ContentTLP.TabIndex = 2;
            // 
            // MainFLP
            // 
            this.MainFLP.Controls.Add(this.MessageTopLabel);
            this.MainFLP.Controls.Add(this.ChoiceListBox);
            this.MainFLP.Controls.Add(this.SelectButtonsFLP);
            this.MainFLP.Controls.Add(this.MessageBottomLabel);
            this.MainFLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.MainFLP.Location = new System.Drawing.Point(63, 3);
            this.MainFLP.Name = "MainFLP";
            this.MainFLP.Size = new System.Drawing.Size(498, 282);
            this.MainFLP.TabIndex = 2;
            // 
            // ChoiceListBox
            // 
            this.ChoiceListBox.FormattingEnabled = true;
            this.ChoiceListBox.HorizontalScrollbar = true;
            this.ChoiceListBox.Location = new System.Drawing.Point(3, 16);
            this.ChoiceListBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.ChoiceListBox.Name = "ChoiceListBox";
            this.ChoiceListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.ChoiceListBox.Size = new System.Drawing.Size(493, 95);
            this.ChoiceListBox.TabIndex = 1;
            // 
            // SelectButtonsFLP
            // 
            this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
            this.SelectButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.SelectButtonsFLP.Location = new System.Drawing.Point(3, 111);
            this.SelectButtonsFLP.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.SelectButtonsFLP.Name = "SelectButtonsFLP";
            this.SelectButtonsFLP.Size = new System.Drawing.Size(493, 22);
            this.SelectButtonsFLP.TabIndex = 2;
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.Location = new System.Drawing.Point(418, 0);
            this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Size = new System.Drawing.Size(75, 23);
            this.SelectAllButton.TabIndex = 0;
            this.SelectAllButton.Text = "Select all";
            this.SelectAllButton.UseVisualStyleBackColor = true;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // MessageBottomLabel
            // 
            this.MessageBottomLabel.AutoSize = true;
            this.MessageBottomLabel.Location = new System.Drawing.Point(3, 136);
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
            this.OuterTLP.Name = "OuterTLP";
            this.OuterTLP.RowCount = 2;
            this.OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OuterTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.OuterTLP.Size = new System.Drawing.Size(570, 336);
            this.OuterTLP.TabIndex = 3;
            // 
            // BottomFLP
            // 
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(3, 297);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Size = new System.Drawing.Size(564, 36);
            this.BottomFLP.TabIndex = 3;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Location = new System.Drawing.Point(486, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(405, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // ListMessageBoxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 336);
            this.Controls.Add(this.OuterTLP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ListMessageBoxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ListMessageBoxForm";
            this.Load += new System.EventHandler(this.ListMessageBoxForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
            this.ContentTLP.ResumeLayout(false);
            this.MainFLP.ResumeLayout(false);
            this.MainFLP.PerformLayout();
            this.SelectButtonsFLP.ResumeLayout(false);
            this.OuterTLP.ResumeLayout(false);
            this.BottomFLP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label MessageTopLabel;
        private System.Windows.Forms.PictureBox IconPictureBox;
        private System.Windows.Forms.TableLayoutPanel ContentTLP;
        private System.Windows.Forms.TableLayoutPanel OuterTLP;
        private System.Windows.Forms.FlowLayoutPanel BottomFLP;
        private System.Windows.Forms.FlowLayoutPanel MainFLP;
        private System.Windows.Forms.ListBox ChoiceListBox;
        private System.Windows.Forms.Label MessageBottomLabel;
        private System.Windows.Forms.FlowLayoutPanel SelectButtonsFLP;
        private System.Windows.Forms.Button SelectAllButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
    }
}