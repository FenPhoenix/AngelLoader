using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class FilterTagsForm : Form
    {
        private readonly Bitmap _arrowRightBmp = new Bitmap(7, 7, PixelFormat.Format32bppPArgb);

        private readonly CatAndTagsList _sourceTags;

        internal readonly TagsFilter TagsFilter = new TagsFilter();

        internal FilterTagsForm(CatAndTagsList sourceTags, TagsFilter tagsFilter)
        {
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif

            // @NET5: Force MS Sans Serif
            Font = ControlExtensions.LegacyMSSansSerif();

            _sourceTags = new CatAndTagsList(sourceTags.Count);

            #region Arrow buttons setup

            // Because this form isn't loading on startup, I'm being lazy here and just creating a bitmap
            using (var g = Graphics.FromImage(_arrowRightBmp))
            {
                Point[] arrowPolygon =
                {
                    // -1 works?! (and for some reason is needed?!)
                    new Point(2, -1),
                    new Point(2, 7),
                    new Point(6, 3)
                };
                g.FillPolygon(Brushes.Black, arrowPolygon);
            }

            AndButton.Image = _arrowRightBmp;
            OrButton.Image = _arrowRightBmp;
            NotButton.Image = _arrowRightBmp;

            #endregion

            sourceTags.DeepCopyTo(_sourceTags);
            tagsFilter.DeepCopyTo(TagsFilter);

            Localize();
        }

        private void Localize()
        {
            Text = LText.TagsFilterBox.TitleText;

            IncludeAllLabel.Text = LText.TagsFilterBox.IncludeAll;
            IncludeAnyLabel.Text = LText.TagsFilterBox.IncludeAny;
            ExcludeLabel.Text = LText.TagsFilterBox.Exclude;

            for (int i = 0; i < 3; i++)
            {
                #region Set i-dependent values

                Button c1b = i switch { 0 => RemoveSelectedAndButton, 1 => RemoveSelectedOrButton, _ => RemoveSelectedNotButton };
                Button cab = i switch { 0 => RemoveAllAndButton, 1 => RemoveAllOrButton, _ => RemoveAllNotButton };

                #endregion

                MainToolTip.SetToolTip(c1b, LText.TagsFilterBox.ClearSelectedToolTip);
                MainToolTip.SetToolTip(cab, LText.TagsFilterBox.ClearAllToolTip);
            }

            // NOTE: These CANNOT be GrowAndShrink because we need to manually grow them here! (special case)
            AndButton.SetTextAutoSize(LText.TagsFilterBox.MoveToAll);
            OrButton.SetTextAutoSize(LText.TagsFilterBox.MoveToAny);
            NotButton.SetTextAutoSize(LText.TagsFilterBox.MoveToExclude);
            int newWidthAll = Math.Max(Math.Max(AndButton.Width, OrButton.Width), NotButton.Width);
            for (int i = 0; i < 3; i++)
            {
                Button button = i switch { 0 => AndButton, 1 => OrButton, _ => NotButton };

                button.Width = newWidthAll;
                button.CenterH(MoveButtonsPanel);
            }

            ResetButton.Text = LText.TagsFilterBox.Reset;
            OKButton.Text = LText.Global.OK;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void FilterTagsForm_Load(object sender, EventArgs e)
        {
            var tv = OriginTreeView;

            _sourceTags.SortAndMoveMiscToEnd();

            foreach (CatAndTags catAndTags in _sourceTags)
            {
                tv.Nodes.Add(catAndTags.Category);
                var last = tv.Nodes[tv.Nodes.Count - 1];
                foreach (string tag in catAndTags.Tags) last.Nodes.Add(tag);
            }

            tv.ExpandAll();
            tv.SelectedNode = tv.Nodes[0];

            if (TagsFilter.AndTags.Count > 0) FillTreeView(TagsFilter.AndTags);
            if (TagsFilter.OrTags.Count > 0) FillTreeView(TagsFilter.OrTags);
            if (TagsFilter.NotTags.Count > 0) FillTreeView(TagsFilter.NotTags);
        }

        #region Find box

        private static TreeNode? FindFirstCatNodeStartingWithText(TreeView tv, string text)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                if (node.Text.StartsWithI(text)) return node;
            }
            return null;
        }

        private static TreeNode? FindFirstCatAndTagStartingWithText(TreeView tv, string cat, string tag)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                if (node.Text == cat && node.Nodes.Count > 0)
                {
                    foreach (TreeNode tagNode in node.Nodes)
                    {
                        if (tagNode.Text.StartsWithI(tag)) return tagNode;
                    }
                }
            }
            return null;
        }

        private void FindTagTextBox_TextChanged(object sender, EventArgs e)
        {
            string text = FindTagTextBox.Text.Replace(" ", "").Replace("\t", "");
            if (!text.Contains(':'))
            {
                TreeNode? node = FindFirstCatNodeStartingWithText(OriginTreeView, text);
                if (node != null) OriginTreeView.SelectedNode = node;
            }
            else
            {
                int index = text.IndexOf(':');
                if (index > 0)
                {
                    string cat = text.Substring(0, index), tag = text.Substring(index + 1);
                    TreeNode? node = FindFirstCatAndTagStartingWithText(OriginTreeView, cat, tag);
                    if (node != null) OriginTreeView.SelectedNode = node;
                }
            }
        }

        #endregion

        private void FillTreeView(CatAndTagsList tags)
        {
            var tv =
                tags == TagsFilter.AndTags ? AndTreeView :
                tags == TagsFilter.OrTags ? OrTreeView :
                NotTreeView;

            tv.SuspendDrawing();
            tv.Nodes.Clear();
            foreach (CatAndTags catAndTags in tags)
            {
                tv.Nodes.Add(catAndTags.Category);
                var last = tv.Nodes[tv.Nodes.Count - 1];
                foreach (string tag in catAndTags.Tags) last.Nodes.Add(tag);
            }

            tv.ExpandAll();
            tv.ResumeDrawing();

            CheckTagInAny();
        }

        private void AddTagsButtons_Click(object sender, EventArgs e)
        {
            var o = OriginTreeView;

            if (o.SelectedNode == null) return;

            CatAndTagsList filteredTags =
                sender == AndButton ? TagsFilter.AndTags :
                sender == OrButton ? TagsFilter.OrTags :
                TagsFilter.NotTags;

            // Parent node = category, child node = tag
            bool isCategory = o.SelectedNode.Parent == null;
            string cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent!.Text;

            CatAndTags? match = null;
            for (int i = 0; i < filteredTags.Count; i++)
            {
                if (filteredTags[i].Category == cat) match = filteredTags[i];
            }
            if (match == null)
            {
                filteredTags.Add(new CatAndTags { Category = cat });
                if (!isCategory)
                {
                    CatAndTags last = filteredTags[filteredTags.Count - 1];
                    last.Tags.Add(o.SelectedNode.Text);
                }
            }
            else
            {
                if (isCategory)
                {
                    match.Tags.Clear();
                }
                else
                {
                    string tag = o.SelectedNode.Text;
                    if (!match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }
            }

            FillTreeView(filteredTags);
        }

        private void RemoveSelectedButtons_Click(object sender, EventArgs e)
        {
            CatAndTagsList tags =
                sender == RemoveSelectedAndButton ? TagsFilter.AndTags :
                sender == RemoveSelectedOrButton ? TagsFilter.OrTags :
                TagsFilter.NotTags;

            var tv =
                sender == RemoveSelectedAndButton ? AndTreeView :
                sender == RemoveSelectedOrButton ? OrTreeView :
                NotTreeView;

            if (tv.SelectedNode == null) return;

            // Parent node (category)
            if (tv.SelectedNode.Parent == null)
            {
                CatAndTags? cat = tags.Find(x => x.Category == tv.SelectedNode.Text);
                if (cat != null) tags.Remove(cat);
            }
            // Child node (tag)
            else
            {
                CatAndTags? cat = tags.Find(x => x.Category == tv.SelectedNode.Parent.Text);
                string? tag = cat?.Tags.Find(x => x == tv.SelectedNode.Text);
                if (tag != null)
                {
                    cat!.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) tags.Remove(cat);
                }
            }

            FillTreeView(tags);
        }

        private void RemoveAllButtons_Click(object sender, EventArgs e)
        {
            CatAndTagsList tags =
                sender == RemoveAllAndButton ? TagsFilter.AndTags :
                sender == RemoveAllOrButton ? TagsFilter.OrTags :
                TagsFilter.NotTags;

            tags.Clear();

            FillTreeView(tags);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            TagsFilter.Clear();
            FillTreeView(TagsFilter.AndTags);
            FillTreeView(TagsFilter.OrTags);
            FillTreeView(TagsFilter.NotTags);
        }

        private void OriginTreeView_AfterSelect(object sender, TreeViewEventArgs e) => CheckTagInAny();

        private void CheckTagInAny()
        {
            var o = OriginTreeView;

            if (o.SelectedNode == null) return;

            // Parent node = category, child node = tag
            bool isCategory = o.SelectedNode.Parent == null;
            string cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent!.Text;

            bool tagInAny = false;

            for (int t = 0; t < 3; t++)
            {
                CatAndTagsList filteredTags = t switch
                {
                    0 => TagsFilter.AndTags,
                    1 => TagsFilter.OrTags,
                    _ => TagsFilter.NotTags
                };

                CatAndTags? match = null;
                for (int i = 0; i < filteredTags.Count; i++)
                {
                    if (filteredTags[i].Category == cat) match = filteredTags[i];
                }
                if (match != null)
                {
                    if (isCategory || match.Tags.ContainsI(o.SelectedNode.Text) ||
                        (match.Category == o.SelectedNode.Parent!.Text && match.Tags.Count == 0))
                    {
                        tagInAny = true;
                        break;
                    }
                }
            }

            AndButton.Enabled = !tagInAny;
            OrButton.Enabled = !tagInAny;
            NotButton.Enabled = !tagInAny;
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void RemoveButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintMinusButton((Button)sender, e);

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void RemoveAllButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintExButton((Button)sender, e);

        private void BottomButtonsFLP_Paint(object sender, PaintEventArgs e)
        {
            Pen s1Pen = Application.RenderWithVisualStyles ? ControlPainter.Sep1Pen : ControlPainter.Sep1PenC;
            const int y1 = 4;
            const int y2 = 23;
            {
                int bx = OKButton.Location.X;
                int sep1x = bx - 8;
                int sep2x = bx - 7;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(ControlPainter.Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _arrowRightBmp?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
