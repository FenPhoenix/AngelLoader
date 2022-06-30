#define FenGen_DesignerSource

namespace AngelLoader.Forms
{
    partial class OriginalGameModsForm
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
            this.OrigGameModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.BottomFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // OrigGameModsControl
            // 
            this.OrigGameModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrigGameModsControl.AutoScroll = true;
            this.OrigGameModsControl.Location = new System.Drawing.Point(0, 0);
            this.OrigGameModsControl.Name = "OrigGameModsControl";
            this.OrigGameModsControl.Size = new System.Drawing.Size(527, 468);
            this.OrigGameModsControl.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(362, 0);
            this.OKButton.Margin = new System.Windows.Forms.Padding(0);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // BottomFLP
            // 
            this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(8, 469);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Size = new System.Drawing.Size(512, 23);
            this.BottomFLP.TabIndex = 2;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(437, 0);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(0);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 1;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OriginalGameModsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(527, 500);
            this.Controls.Add(this.BottomFLP);
            this.Controls.Add(this.OrigGameModsControl);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "OriginalGameModsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage mods for [Game]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OriginalGameMods_FormClosing);
            this.BottomFLP.ResumeLayout(false);
            this.ResumeLayout(false);

        }
#endif

        #endregion

        private CustomControls.ModsControl OrigGameModsControl;
        private CustomControls.DarkButton OKButton;
        private System.Windows.Forms.FlowLayoutPanel BottomFLP;
        private CustomControls.DarkButton Cancel_Button;
    }
}
