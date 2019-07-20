using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;

namespace AngelLoader.Forms
{
    public partial class FilterTagsForm : Form
    {
        private readonly Bitmap _arrowRightBmp = new Bitmap(7, 7, PixelFormat.Format32bppPArgb);

        private readonly GlobalCatAndTagsList SourceTags = new GlobalCatAndTagsList();
        internal readonly TagsFilter TagsFilter = new TagsFilter();

        internal FilterTagsForm(GlobalCatAndTagsList sourceTags, TagsFilter tagsFilter)
        {
            InitializeComponent();

            #region Arrow buttons setup

            // Because this form isn't loading on startup, I'm being lazy here and just creating a bitmap
            using (var g = Graphics.FromImage(_arrowRightBmp))
            {
                var arrowPolygon = new[]
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

            sourceTags.DeepCopyTo(SourceTags);
            tagsFilter.DeepCopyTo(TagsFilter);

            Localize();
        }

        internal void Localize()
        {
            Text = LText.TagsFilterBox.TitleText;

            IncludeAllLabel.Text = LText.TagsFilterBox.IncludeAll;
            IncludeAnyLabel.Text = LText.TagsFilterBox.IncludeAny;
            ExcludeLabel.Text = LText.TagsFilterBox.Exclude;

            for (int i = 0; i < 3; i++)
            {
                var c1b = i == 0 ? RemoveSelectedAndButton : i == 1 ? RemoveSelectedOrButton : RemoveSelectedNotButton;
                var cab = i == 0 ? RemoveAllAndButton : i == 1 ? RemoveAllOrButton : RemoveAllNotButton;
                MainToolTip.SetToolTip(c1b, LText.TagsFilterBox.ClearSelectedToolTip);
                MainToolTip.SetToolTip(cab, LText.TagsFilterBox.ClearAllToolTip);
            }

            // PERF_TODO: Lots of wasted time and duplicate sizing here
            AndButton.SetTextAutoSize(LText.TagsFilterBox.MoveToAll, AndButton.Width);
            OrButton.SetTextAutoSize(LText.TagsFilterBox.MoveToAny, OrButton.Width);
            NotButton.SetTextAutoSize(LText.TagsFilterBox.MoveToExclude, NotButton.Width);
            var newWidthAll = Math.Max(Math.Max(AndButton.Width, OrButton.Width), NotButton.Width);
            for (int i = 0; i < 3; i++)
            {
                var button = i == 0 ? AndButton : i == 1 ? OrButton : NotButton;
                button.Width = newWidthAll;
                button.CenterH(MoveButtonsPanel);
            }

            ResetButton.SetTextAutoSize(LText.TagsFilterBox.Reset, ResetButton.Width);
            OKButton.SetTextAutoSize(LText.Global.OK, OKButton.Width);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void FilterTagsForm_Load(object sender, EventArgs e)
        {
            var tv = OriginTreeView;

            SourceTags.SortAndMoveMiscToEnd();

            foreach (var catAndTags in SourceTags)
            {
                tv.Nodes.Add(catAndTags.Category.Name);
                var last = tv.Nodes[tv.Nodes.Count - 1];
                foreach (var tag in catAndTags.Tags) last.Nodes.Add(tag.Name);
            }

            tv.ExpandAll();
            tv.SelectedNode = tv.Nodes[0];

            if (TagsFilter.AndTags.Count > 0) FillTreeView(TagsFilter.AndTags);
            if (TagsFilter.OrTags.Count > 0) FillTreeView(TagsFilter.OrTags);
            if (TagsFilter.NotTags.Count > 0) FillTreeView(TagsFilter.NotTags);
        }

        #region Find box

        private static TreeNode FindFirstCatNodeStartingWithText(TreeView tv, string text)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                if (node.Text.StartsWithI(text)) return node;
            }
            return null;
        }

        private static TreeNode FindFirstCatAndTagStartingWithText(TreeView tv, string tag, string cat)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                if (node.Text == tag && node.Nodes.Count > 0)
                {
                    foreach (TreeNode tagNode in node.Nodes)
                    {
                        if (tagNode.Text.StartsWithI(cat)) return tagNode;
                    }
                }
            }
            return null;
        }

        private void FindTagTextBox_TextChanged(object sender, EventArgs e)
        {
            var text = FindTagTextBox.Text.Replace(" ", "").Replace("\t", "");
            if (!text.Contains(':'))
            {
                var node = FindFirstCatNodeStartingWithText(OriginTreeView, text);
                if (node != null) OriginTreeView.SelectedNode = node;
            }
            else
            {
                var index = text.IndexOf(':');
                if (index > 0)
                {
                    string cat = text.Substring(0, index), tag = text.Substring(index + 1);
                    var node = FindFirstCatAndTagStartingWithText(OriginTreeView, cat, tag);
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
                tags == TagsFilter.NotTags ? NotTreeView :
                null;

            Debug.Assert(tv != null, "FillTreeView: tv == null");

            tv.SuspendDrawing();
            tv.Nodes.Clear();
            foreach (var catAndTags in tags)
            {
                tv.Nodes.Add(catAndTags.Category);
                //tv.Nodes.Add(new TreeNode { BackColor = SystemColors.Highlight, Text = catAndTags.Category });
                var last = tv.Nodes[tv.Nodes.Count - 1];
                foreach (var tag in catAndTags.Tags) last.Nodes.Add(tag);
            }

            tv.ExpandAll();
            tv.ResumeDrawing();

            CheckTagInAny();
        }

        private void AddTagsButtons_Click(object sender, EventArgs e)
        {
            var o = OriginTreeView;

            if (o.SelectedNode == null) return;

            var filteredTags =
                sender == AndButton ? TagsFilter.AndTags :
                sender == OrButton ? TagsFilter.OrTags :
                sender == NotButton ? TagsFilter.NotTags :
                null;

            Debug.Assert(filteredTags != null, "AddTagsButtons_Click: filteredTags == null");

            // Parent node = category, child node = tag
            bool isCategory = o.SelectedNode.Parent == null;
            var cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent.Text;

            CatAndTags match = null;
            for (int i = 0; i < filteredTags.Count; i++)
            {
                if (filteredTags[i].Category == cat) match = filteredTags[i];
            }
            if (match == null)
            {
                filteredTags.Add(new CatAndTags { Category = cat });
                if (!isCategory)
                {
                    var last = filteredTags[filteredTags.Count - 1];
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
                    var tag = o.SelectedNode.Text;
                    if (!match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }
            }

            FillTreeView(filteredTags);
        }

        private void RemoveSelectedButtons_Click(object sender, EventArgs e)
        {
            var tags =
                sender == RemoveSelectedAndButton ? TagsFilter.AndTags :
                sender == RemoveSelectedOrButton ? TagsFilter.OrTags :
                sender == RemoveSelectedNotButton ? TagsFilter.NotTags :
                null;

            Debug.Assert(tags != null, "RemoveSelectedButtons_Click: tags == null");

            var tv =
                sender == RemoveSelectedAndButton ? AndTreeView :
                sender == RemoveSelectedOrButton ? OrTreeView :
                sender == RemoveSelectedNotButton ? NotTreeView :
                null;

            Debug.Assert(tv != null, "RemoveSelectedButtons_Click: tv == null");

            if (tv.SelectedNode == null) return;

            // Parent node (category)
            if (tv.SelectedNode.Parent == null)
            {
                var cat = tags.FirstOrDefault(x => x.Category == tv.SelectedNode.Text);
                if (cat != null) tags.Remove(cat);
            }
            // Child node (tag)
            else
            {
                var cat = tags.FirstOrDefault(x => x.Category == tv.SelectedNode.Parent.Text);
                var tag = cat?.Tags.FirstOrDefault(x => x == tv.SelectedNode.Text);
                if (tag != null)
                {
                    cat.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) tags.Remove(cat);
                }
            }

            FillTreeView(tags);
        }

        private void RemoveAllButtons_Click(object sender, EventArgs e)
        {
            var tags =
                sender == RemoveAllAndButton ? TagsFilter.AndTags :
                sender == RemoveAllOrButton ? TagsFilter.OrTags :
                sender == RemoveAllNotButton ? TagsFilter.NotTags :
                null;

            Debug.Assert(tags != null, "RemoveSelectedButtons_Click: tags == null");

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
            var cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent.Text;

            bool tagInAny = false;

            for (int t = 0; t < 3; t++)
            {
                var filteredTags =
                    t == 0 ? TagsFilter.AndTags :
                    t == 1 ? TagsFilter.OrTags :
                    t == 2 ? TagsFilter.NotTags :
                    null;

                Debug.Assert(filteredTags != null, "OriginTreeView_AfterSelect: filteredTags == null");

                CatAndTags match = null;
                for (int i = 0; i < filteredTags.Count; i++)
                {
                    if (filteredTags[i].Category == cat) match = filteredTags[i];
                }
                if (match != null)
                {
                    if (isCategory || match.Tags.ContainsI(o.SelectedNode.Text) ||
                        (match.Category == o.SelectedNode.Parent.Text && match.Tags.Count == 0))
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

        private void RemoveButtons_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintMinusButton(((Button)sender).Enabled, e);

        private void RemoveAllButtons_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintExButton(((Button)sender).Enabled, e);
    }
}
