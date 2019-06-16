namespace AngelLoader.Forms
{
    partial class PlayWithOptionsForm
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
            this.CommandLineTextBox = new System.Windows.Forms.TextBox();
            this.CommandLineGroupBox = new System.Windows.Forms.GroupBox();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.CommandLineGroupBox.SuspendLayout();
            this.BottomFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // CommandLineTextBox
            // 
            this.CommandLineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandLineTextBox.Location = new System.Drawing.Point(16, 24);
            this.CommandLineTextBox.Name = "CommandLineTextBox";
            this.CommandLineTextBox.Size = new System.Drawing.Size(625, 20);
            this.CommandLineTextBox.TabIndex = 0;
            // 
            // CommandLineGroupBox
            // 
            this.CommandLineGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandLineGroupBox.Controls.Add(this.CommandLineTextBox);
            this.CommandLineGroupBox.Location = new System.Drawing.Point(8, 264);
            this.CommandLineGroupBox.Name = "CommandLineGroupBox";
            this.CommandLineGroupBox.Size = new System.Drawing.Size(657, 64);
            this.CommandLineGroupBox.TabIndex = 1;
            this.CommandLineGroupBox.TabStop = false;
            this.CommandLineGroupBox.Text = "Command line";
            // 
            // BottomFLP
            // 
            this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(0, 410);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Size = new System.Drawing.Size(672, 30);
            this.BottomFLP.TabIndex = 2;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(594, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(513, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // PlayWithOptionsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(672, 440);
            this.Controls.Add(this.BottomFLP);
            this.Controls.Add(this.CommandLineGroupBox);
            this.Name = "PlayWithOptionsForm";
            this.Text = "Play with options";
            this.CommandLineGroupBox.ResumeLayout(false);
            this.CommandLineGroupBox.PerformLayout();
            this.BottomFLP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox CommandLineTextBox;
        private System.Windows.Forms.GroupBox CommandLineGroupBox;
        private System.Windows.Forms.FlowLayoutPanel BottomFLP;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
    }
}