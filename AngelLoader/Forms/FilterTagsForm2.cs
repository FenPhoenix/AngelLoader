using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Forms
{
    public partial class FilterTagsForm2 : Form, ILocalizable
    {
        private readonly List<GlobalCatAndTags> SourceTags = new List<GlobalCatAndTags>();
        internal readonly TagsFilter TagsFilter = new TagsFilter();

        internal FilterTagsForm2(List<GlobalCatAndTags> sourceTags, TagsFilter tagsFilter)
        {
            InitializeComponent();

            Methods.DeepCopyGlobalTags(sourceTags, SourceTags);

            Methods.DeepCopyTagsFilter(tagsFilter, TagsFilter);

            SetUITextToLocalized();
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            Text = LText.TagsFilterBox.TitleText;
            IncludeAllLabel.Text = LText.TagsFilterBox.IncludeAll;
            IncludeAnyLabel.Text = LText.TagsFilterBox.IncludeAny;
            ExcludeLabel.Text = LText.TagsFilterBox.Exclude;
            AndButton.Text = LText.TagsFilterBox.MoveToAll;
            OrButton.Text = LText.TagsFilterBox.MoveToAny;
            NotButton.Text = LText.TagsFilterBox.MoveToExclude;
            ResetButton.Text = LText.TagsFilterBox.Reset;
            OKButton.SetL10nText(LText.Global.OK, OKButton.Width);
            Cancel_Button.SetL10nText(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void FilterTagsForm2_Load(object sender, EventArgs e)
        {
            var tv = OriginTreeView;

            SourceTags.SortCat();

            foreach (var catAndTags in SourceTags)
            {
                tv.Nodes.Add(catAndTags.Category.Name);
                var last = tv.Nodes[tv.Nodes.Count - 1];
                foreach (var tag in catAndTags.Tags) last.Nodes.Add(tag.Name);
            }

            tv.ExpandAll();
            tv.SelectedNode = tv.Nodes[0];

            FillTreeView(TagsFilter.AndTags);
            FillTreeView(TagsFilter.OrTags);
            FillTreeView(TagsFilter.NotTags);
        }

        private void FillTreeView(List<CatAndTags> tags)
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

        private void OriginTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CheckTagInAny();
        }

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
    }
}
