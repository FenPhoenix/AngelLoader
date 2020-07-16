using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class FilterTagsForm
    {
        private void InitComponentManual()
        {
            components = new System.ComponentModel.Container();
            OriginTreeView = new CustomControls.TreeViewCustom();
            AndTreeView = new TreeView();
            OrTreeView = new TreeView();
            NotTreeView = new TreeView();
            AndButton = new Button();
            OrButton = new Button();
            NotButton = new Button();
            IncludeAllLabel = new Label();
            IncludeAnyLabel = new Label();
            ExcludeLabel = new Label();
            FilterLabelsPanel = new Panel();
            RemoveAllNotButton = new Button();
            RemoveAllOrButton = new Button();
            RemoveAllAndButton = new Button();
            RemoveSelectedNotButton = new Button();
            RemoveSelectedOrButton = new Button();
            RemoveSelectedAndButton = new Button();
            OKButton = new Button();
            Cancel_Button = new Button();
            ResetButton = new Button();
            BottomButtonsFLP = new FlowLayoutPanel();
            MoveButtonsPanel = new Panel();
            MainToolTip = new ToolTip(components);
            FindTagTextBox = new TextBox();
            FilterLabelsPanel.SuspendLayout();
            BottomButtonsFLP.SuspendLayout();
            MoveButtonsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // OriginTreeView
            // 
            OriginTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            OriginTreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            OriginTreeView.HideSelection = false;
            OriginTreeView.Location = new System.Drawing.Point(8, 32);
            OriginTreeView.Size = new System.Drawing.Size(224, 624);
            OriginTreeView.TabIndex = 0;
            OriginTreeView.AfterSelect += OriginTreeView_AfterSelect;
            // 
            // AndTreeView
            // 
            AndTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            AndTreeView.HideSelection = false;
            AndTreeView.Location = new System.Drawing.Point(344, 24);
            AndTreeView.Size = new System.Drawing.Size(224, 632);
            AndTreeView.TabIndex = 3;
            // 
            // OrTreeView
            // 
            OrTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            OrTreeView.HideSelection = false;
            OrTreeView.Location = new System.Drawing.Point(576, 24);
            OrTreeView.Size = new System.Drawing.Size(224, 632);
            OrTreeView.TabIndex = 4;
            // 
            // NotTreeView
            // 
            NotTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            NotTreeView.HideSelection = false;
            NotTreeView.Location = new System.Drawing.Point(808, 24);
            NotTreeView.Size = new System.Drawing.Size(224, 632);
            NotTreeView.TabIndex = 5;
            // 
            // AndButton
            // 
            AndButton.AutoSize = true;
            AndButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            AndButton.Location = new System.Drawing.Point(16, 0);
            AndButton.TabIndex = 0;
            AndButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            AndButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            AndButton.UseVisualStyleBackColor = true;
            AndButton.Click += AddTagsButtons_Click;
            // 
            // OrButton
            // 
            OrButton.AutoSize = true;
            OrButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            OrButton.Location = new System.Drawing.Point(16, 23);
            OrButton.TabIndex = 1;
            OrButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            OrButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            OrButton.UseVisualStyleBackColor = true;
            OrButton.Click += AddTagsButtons_Click;
            // 
            // NotButton
            // 
            NotButton.AutoSize = true;
            NotButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            NotButton.Location = new System.Drawing.Point(16, 46);
            NotButton.TabIndex = 2;
            NotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            NotButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            NotButton.UseVisualStyleBackColor = true;
            NotButton.Click += AddTagsButtons_Click;
            // 
            // IncludeAllLabel
            // 
            IncludeAllLabel.AutoSize = true;
            IncludeAllLabel.Location = new System.Drawing.Point(0, 8);
            IncludeAllLabel.TabIndex = 0;
            // 
            // IncludeAnyLabel
            // 
            IncludeAnyLabel.AutoSize = true;
            IncludeAnyLabel.Location = new System.Drawing.Point(232, 8);
            IncludeAnyLabel.TabIndex = 3;
            // 
            // ExcludeLabel
            // 
            ExcludeLabel.AutoSize = true;
            ExcludeLabel.Location = new System.Drawing.Point(464, 8);
            ExcludeLabel.TabIndex = 6;
            // 
            // FilterLabelsPanel
            // 
            FilterLabelsPanel.Controls.Add(RemoveAllNotButton);
            FilterLabelsPanel.Controls.Add(RemoveAllOrButton);
            FilterLabelsPanel.Controls.Add(RemoveAllAndButton);
            FilterLabelsPanel.Controls.Add(IncludeAllLabel);
            FilterLabelsPanel.Controls.Add(ExcludeLabel);
            FilterLabelsPanel.Controls.Add(RemoveSelectedNotButton);
            FilterLabelsPanel.Controls.Add(RemoveSelectedOrButton);
            FilterLabelsPanel.Controls.Add(RemoveSelectedAndButton);
            FilterLabelsPanel.Controls.Add(IncludeAnyLabel);
            FilterLabelsPanel.Location = new System.Drawing.Point(344, 0);
            FilterLabelsPanel.Size = new System.Drawing.Size(688, 24);
            FilterLabelsPanel.TabIndex = 2;
            // 
            // RemoveAllNotButton
            // 
            RemoveAllNotButton.Location = new System.Drawing.Point(666, 0);
            RemoveAllNotButton.Size = new System.Drawing.Size(23, 23);
            RemoveAllNotButton.TabIndex = 8;
            RemoveAllNotButton.UseVisualStyleBackColor = true;
            RemoveAllNotButton.Click += RemoveAllButtons_Click;
            RemoveAllNotButton.Paint += RemoveAllButtons_Paint;
            // 
            // RemoveAllOrButton
            // 
            RemoveAllOrButton.Location = new System.Drawing.Point(434, 0);
            RemoveAllOrButton.Size = new System.Drawing.Size(23, 23);
            RemoveAllOrButton.TabIndex = 5;
            RemoveAllOrButton.UseVisualStyleBackColor = true;
            RemoveAllOrButton.Click += RemoveAllButtons_Click;
            RemoveAllOrButton.Paint += RemoveAllButtons_Paint;
            // 
            // RemoveAllAndButton
            // 
            RemoveAllAndButton.Location = new System.Drawing.Point(202, 0);
            RemoveAllAndButton.Size = new System.Drawing.Size(23, 23);
            RemoveAllAndButton.TabIndex = 2;
            RemoveAllAndButton.UseVisualStyleBackColor = true;
            RemoveAllAndButton.Click += RemoveAllButtons_Click;
            RemoveAllAndButton.Paint += RemoveAllButtons_Paint;
            // 
            // RemoveSelectedNotButton
            // 
            RemoveSelectedNotButton.Location = new System.Drawing.Point(643, 0);
            RemoveSelectedNotButton.Size = new System.Drawing.Size(23, 23);
            RemoveSelectedNotButton.TabIndex = 7;
            RemoveSelectedNotButton.UseVisualStyleBackColor = true;
            RemoveSelectedNotButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedNotButton.Paint += RemoveButtons_Paint;
            // 
            // RemoveSelectedOrButton
            // 
            RemoveSelectedOrButton.Location = new System.Drawing.Point(411, 0);
            RemoveSelectedOrButton.Size = new System.Drawing.Size(23, 23);
            RemoveSelectedOrButton.TabIndex = 4;
            RemoveSelectedOrButton.UseVisualStyleBackColor = true;
            RemoveSelectedOrButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedOrButton.Paint += RemoveButtons_Paint;
            // 
            // RemoveSelectedAndButton
            // 
            RemoveSelectedAndButton.Location = new System.Drawing.Point(179, 0);
            RemoveSelectedAndButton.Size = new System.Drawing.Size(23, 23);
            RemoveSelectedAndButton.TabIndex = 1;
            RemoveSelectedAndButton.UseVisualStyleBackColor = true;
            RemoveSelectedAndButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedAndButton.Paint += RemoveButtons_Paint;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKButton.AutoSize = true;
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.TabIndex = 2;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Cancel_Button.AutoSize = true;
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 3;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            ResetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ResetButton.AutoSize = true;
            ResetButton.Margin = new Padding(3, 3, 11, 3);
            ResetButton.Padding = new Padding(6, 0, 6, 0);
            ResetButton.TabIndex = 0;
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // BottomButtonsFLP
            // 
            BottomButtonsFLP.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            BottomButtonsFLP.Controls.Add(Cancel_Button);
            BottomButtonsFLP.Controls.Add(OKButton);
            BottomButtonsFLP.Controls.Add(ResetButton);
            BottomButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            BottomButtonsFLP.Location = new System.Drawing.Point(0, 660);
            BottomButtonsFLP.Size = new System.Drawing.Size(1040, 32);
            BottomButtonsFLP.TabIndex = 6;
            BottomButtonsFLP.Paint += BottomButtonsFLP_Paint;
            // 
            // MoveButtonsPanel
            // 
            MoveButtonsPanel.Controls.Add(AndButton);
            MoveButtonsPanel.Controls.Add(OrButton);
            MoveButtonsPanel.Controls.Add(NotButton);
            MoveButtonsPanel.Location = new System.Drawing.Point(232, 104);
            MoveButtonsPanel.Size = new System.Drawing.Size(112, 552);
            MoveButtonsPanel.TabIndex = 1;
            // 
            // FindTagTextBox
            // 
            FindTagTextBox.Location = new System.Drawing.Point(8, 8);
            FindTagTextBox.Size = new System.Drawing.Size(224, 20);
            FindTagTextBox.TabIndex = 7;
            FindTagTextBox.TextChanged += FindTagTextBox_TextChanged;
            // 
            // FilterTagsForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new System.Drawing.Size(1040, 692);
            Controls.Add(FindTagTextBox);
            Controls.Add(MoveButtonsPanel);
            Controls.Add(BottomButtonsFLP);
            Controls.Add(FilterLabelsPanel);
            Controls.Add(NotTreeView);
            Controls.Add(OrTreeView);
            Controls.Add(AndTreeView);
            Controls.Add(OriginTreeView);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1056, 32767);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1056, 242);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Load += FilterTagsForm_Load;
            FilterLabelsPanel.ResumeLayout(false);
            FilterLabelsPanel.PerformLayout();
            BottomButtonsFLP.ResumeLayout(false);
            BottomButtonsFLP.PerformLayout();
            MoveButtonsPanel.ResumeLayout(false);
            MoveButtonsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
