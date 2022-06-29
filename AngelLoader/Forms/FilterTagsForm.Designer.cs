﻿#define FenGen_DesignerSource

namespace AngelLoader.Forms
{
    sealed partial class FilterTagsForm
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
            this.components = new System.ComponentModel.Container();
            this.OriginTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
            this.AndTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
            this.OrTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
            this.NotTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
            this.AndButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OrButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.NotButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.IncludeAllLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.IncludeAnyLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ExcludeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.FilterLabelsPanel = new System.Windows.Forms.Panel();
            this.RemoveAllNotButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RemoveAllOrButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RemoveAllAndButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RemoveSelectedNotButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RemoveSelectedOrButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RemoveSelectedAndButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.BottomButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.MoveButtonsPanel = new System.Windows.Forms.Panel();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.FindTagTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.FilterLabelsPanel.SuspendLayout();
            this.BottomButtonsFLP.SuspendLayout();
            this.MoveButtonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OriginTreeView
            // 
            this.OriginTreeView.AlwaysDrawNodesFocused = true;
            this.OriginTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OriginTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.OriginTreeView.HideSelection = false;
            this.OriginTreeView.Location = new System.Drawing.Point(8, 32);
            this.OriginTreeView.Name = "OriginTreeView";
            this.OriginTreeView.Size = new System.Drawing.Size(224, 624);
            this.OriginTreeView.TabIndex = 0;
            this.OriginTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OriginTreeView_AfterSelect);
            // 
            // AndTreeView
            // 
            this.AndTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.AndTreeView.HideSelection = false;
            this.AndTreeView.Location = new System.Drawing.Point(344, 24);
            this.AndTreeView.Name = "AndTreeView";
            this.AndTreeView.Size = new System.Drawing.Size(224, 632);
            this.AndTreeView.TabIndex = 3;
            // 
            // OrTreeView
            // 
            this.OrTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OrTreeView.HideSelection = false;
            this.OrTreeView.Location = new System.Drawing.Point(576, 24);
            this.OrTreeView.Name = "OrTreeView";
            this.OrTreeView.Size = new System.Drawing.Size(224, 632);
            this.OrTreeView.TabIndex = 4;
            // 
            // NotTreeView
            // 
            this.NotTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.NotTreeView.HideSelection = false;
            this.NotTreeView.Location = new System.Drawing.Point(808, 24);
            this.NotTreeView.Name = "NotTreeView";
            this.NotTreeView.Size = new System.Drawing.Size(224, 632);
            this.NotTreeView.TabIndex = 5;
            // 
            // AndButton
            // 
            this.AndButton.AutoSize = true;
            this.AndButton.MinimumSize = new System.Drawing.Size(80, 23);
            this.AndButton.Location = new System.Drawing.Point(16, 0);
            this.AndButton.Name = "AndButton";
            this.AndButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.AndButton.Size = new System.Drawing.Size(80, 23);
            this.AndButton.TabIndex = 0;
            this.AndButton.Text = "All";
            this.AndButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AndButton.UseVisualStyleBackColor = true;
            this.AndButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            this.AndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
            // 
            // OrButton
            // 
            this.OrButton.AutoSize = true;
            this.OrButton.MinimumSize = new System.Drawing.Size(80, 23);
            this.OrButton.Location = new System.Drawing.Point(16, 23);
            this.OrButton.Name = "OrButton";
            this.OrButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.OrButton.Size = new System.Drawing.Size(80, 23);
            this.OrButton.TabIndex = 1;
            this.OrButton.Text = "Any";
            this.OrButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OrButton.UseVisualStyleBackColor = true;
            this.OrButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            this.OrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
            // 
            // NotButton
            // 
            this.NotButton.AutoSize = true;
            this.NotButton.MinimumSize = new System.Drawing.Size(80, 23);
            this.NotButton.Location = new System.Drawing.Point(16, 46);
            this.NotButton.Name = "NotButton";
            this.NotButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.NotButton.Size = new System.Drawing.Size(80, 23);
            this.NotButton.TabIndex = 2;
            this.NotButton.Text = "Exclude";
            this.NotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NotButton.UseVisualStyleBackColor = true;
            this.NotButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            this.NotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
            // 
            // IncludeAllLabel
            // 
            this.IncludeAllLabel.AutoSize = true;
            this.IncludeAllLabel.Location = new System.Drawing.Point(0, 8);
            this.IncludeAllLabel.Name = "IncludeAllLabel";
            this.IncludeAllLabel.Size = new System.Drawing.Size(59, 13);
            this.IncludeAllLabel.TabIndex = 0;
            this.IncludeAllLabel.Text = "Include All:";
            // 
            // IncludeAnyLabel
            // 
            this.IncludeAnyLabel.AutoSize = true;
            this.IncludeAnyLabel.Location = new System.Drawing.Point(232, 8);
            this.IncludeAnyLabel.Name = "IncludeAnyLabel";
            this.IncludeAnyLabel.Size = new System.Drawing.Size(66, 13);
            this.IncludeAnyLabel.TabIndex = 3;
            this.IncludeAnyLabel.Text = "Include Any:";
            // 
            // ExcludeLabel
            // 
            this.ExcludeLabel.AutoSize = true;
            this.ExcludeLabel.Location = new System.Drawing.Point(464, 8);
            this.ExcludeLabel.Name = "ExcludeLabel";
            this.ExcludeLabel.Size = new System.Drawing.Size(48, 13);
            this.ExcludeLabel.TabIndex = 6;
            this.ExcludeLabel.Text = "Exclude:";
            // 
            // FilterLabelsPanel
            // 
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllNotButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllOrButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllAndButton);
            this.FilterLabelsPanel.Controls.Add(this.IncludeAllLabel);
            this.FilterLabelsPanel.Controls.Add(this.ExcludeLabel);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedNotButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedOrButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedAndButton);
            this.FilterLabelsPanel.Controls.Add(this.IncludeAnyLabel);
            this.FilterLabelsPanel.Location = new System.Drawing.Point(344, 0);
            this.FilterLabelsPanel.Name = "FilterLabelsPanel";
            this.FilterLabelsPanel.Size = new System.Drawing.Size(688, 24);
            this.FilterLabelsPanel.TabIndex = 2;
            // 
            // RemoveAllNotButton
            // 
            this.RemoveAllNotButton.Location = new System.Drawing.Point(666, 0);
            this.RemoveAllNotButton.Name = "RemoveAllNotButton";
            this.RemoveAllNotButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllNotButton.TabIndex = 8;
            this.RemoveAllNotButton.UseVisualStyleBackColor = true;
            this.RemoveAllNotButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllNotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveAllOrButton
            // 
            this.RemoveAllOrButton.Location = new System.Drawing.Point(434, 0);
            this.RemoveAllOrButton.Name = "RemoveAllOrButton";
            this.RemoveAllOrButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllOrButton.TabIndex = 5;
            this.RemoveAllOrButton.UseVisualStyleBackColor = true;
            this.RemoveAllOrButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllOrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveAllAndButton
            // 
            this.RemoveAllAndButton.Location = new System.Drawing.Point(202, 0);
            this.RemoveAllAndButton.Name = "RemoveAllAndButton";
            this.RemoveAllAndButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllAndButton.TabIndex = 2;
            this.RemoveAllAndButton.UseVisualStyleBackColor = true;
            this.RemoveAllAndButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllAndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveSelectedNotButton
            // 
            this.RemoveSelectedNotButton.Location = new System.Drawing.Point(643, 0);
            this.RemoveSelectedNotButton.Name = "RemoveSelectedNotButton";
            this.RemoveSelectedNotButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedNotButton.TabIndex = 7;
            this.RemoveSelectedNotButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedNotButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedNotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // RemoveSelectedOrButton
            // 
            this.RemoveSelectedOrButton.Location = new System.Drawing.Point(411, 0);
            this.RemoveSelectedOrButton.Name = "RemoveSelectedOrButton";
            this.RemoveSelectedOrButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedOrButton.TabIndex = 4;
            this.RemoveSelectedOrButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedOrButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedOrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // RemoveSelectedAndButton
            // 
            this.RemoveSelectedAndButton.Location = new System.Drawing.Point(179, 0);
            this.RemoveSelectedAndButton.Name = "RemoveSelectedAndButton";
            this.RemoveSelectedAndButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedAndButton.TabIndex = 1;
            this.RemoveSelectedAndButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedAndButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedAndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(877, 3);
            this.OKButton.Margin = new System.Windows.Forms.Padding(13, 3, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(958, 3);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 3;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.ResetButton.Location = new System.Drawing.Point(788, 3);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 0;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // BottomButtonsFLP
            // 
            this.BottomButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BottomButtonsFLP.Controls.Add(this.Cancel_Button);
            this.BottomButtonsFLP.Controls.Add(this.OKButton);
            this.BottomButtonsFLP.Controls.Add(this.ResetButton);
            this.BottomButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomButtonsFLP.Location = new System.Drawing.Point(0, 660);
            this.BottomButtonsFLP.Name = "BottomButtonsFLP";
            this.BottomButtonsFLP.Size = new System.Drawing.Size(1040, 32);
            this.BottomButtonsFLP.TabIndex = 6;
            this.BottomButtonsFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.BottomButtonsFLP_Paint);
            // 
            // MoveButtonsPanel
            // 
            this.MoveButtonsPanel.Controls.Add(this.AndButton);
            this.MoveButtonsPanel.Controls.Add(this.OrButton);
            this.MoveButtonsPanel.Controls.Add(this.NotButton);
            this.MoveButtonsPanel.Location = new System.Drawing.Point(232, 104);
            this.MoveButtonsPanel.Name = "MoveButtonsPanel";
            this.MoveButtonsPanel.Size = new System.Drawing.Size(112, 552);
            this.MoveButtonsPanel.TabIndex = 1;
            // 
            // FindTagTextBox
            // 
            this.FindTagTextBox.Location = new System.Drawing.Point(8, 8);
            this.FindTagTextBox.Name = "FindTagTextBox";
            this.FindTagTextBox.Size = new System.Drawing.Size(224, 20);
            this.FindTagTextBox.TabIndex = 7;
            this.FindTagTextBox.TextChanged += new System.EventHandler(this.FindTagTextBox_TextChanged);
            // 
            // FilterTagsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(1040, 692);
            this.Controls.Add(this.FindTagTextBox);
            this.Controls.Add(this.MoveButtonsPanel);
            this.Controls.Add(this.BottomButtonsFLP);
            this.Controls.Add(this.FilterLabelsPanel);
            this.Controls.Add(this.NotTreeView);
            this.Controls.Add(this.OrTreeView);
            this.Controls.Add(this.AndTreeView);
            this.Controls.Add(this.OriginTreeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1056, 32767);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1056, 242);
            this.Name = "FilterTagsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Tags Filter";
            this.FilterLabelsPanel.ResumeLayout(false);
            this.FilterLabelsPanel.PerformLayout();
            this.BottomButtonsFLP.ResumeLayout(false);
            this.BottomButtonsFLP.PerformLayout();
            this.MoveButtonsPanel.ResumeLayout(false);
            this.MoveButtonsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endif

        #endregion

        private AngelLoader.Forms.CustomControls.DarkTreeView OriginTreeView;
        private AngelLoader.Forms.CustomControls.DarkTreeView AndTreeView;
        private AngelLoader.Forms.CustomControls.DarkTreeView OrTreeView;
        private AngelLoader.Forms.CustomControls.DarkTreeView NotTreeView;
        private AngelLoader.Forms.CustomControls.DarkButton AndButton;
        private AngelLoader.Forms.CustomControls.DarkButton OrButton;
        private AngelLoader.Forms.CustomControls.DarkButton NotButton;
        private AngelLoader.Forms.CustomControls.DarkLabel IncludeAllLabel;
        private AngelLoader.Forms.CustomControls.DarkLabel IncludeAnyLabel;
        private AngelLoader.Forms.CustomControls.DarkLabel ExcludeLabel;
        private System.Windows.Forms.Panel FilterLabelsPanel;
        private AngelLoader.Forms.CustomControls.DarkButton OKButton;
        private AngelLoader.Forms.CustomControls.DarkButton Cancel_Button;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveSelectedAndButton;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveAllAndButton;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveAllNotButton;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveAllOrButton;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveSelectedNotButton;
        private AngelLoader.Forms.CustomControls.DarkButton RemoveSelectedOrButton;
        private AngelLoader.Forms.CustomControls.DarkButton ResetButton;
        private System.Windows.Forms.FlowLayoutPanel BottomButtonsFLP;
        private System.Windows.Forms.Panel MoveButtonsPanel;
        private System.Windows.Forms.ToolTip MainToolTip;
        private AngelLoader.Forms.CustomControls.DarkTextBox FindTagTextBox;
    }
}
