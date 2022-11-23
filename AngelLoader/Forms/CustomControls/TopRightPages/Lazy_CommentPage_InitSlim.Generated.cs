﻿namespace AngelLoader.Forms.CustomControls
{
    sealed partial class Lazy_CommentPage
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitSlim()
        {
            this.CommentTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.SuspendLayout();
            // 
            // CommentTextBox
            // 
            this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommentTextBox.Location = new System.Drawing.Point(8, 8);
            this.CommentTextBox.Multiline = true;
            this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommentTextBox.Size = new System.Drawing.Size(511, 266);
            this.CommentTextBox.TabIndex = 33;
            // 
            // Lazy_CommentPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CommentTextBox);
            this.Size = new System.Drawing.Size(526, 284);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
