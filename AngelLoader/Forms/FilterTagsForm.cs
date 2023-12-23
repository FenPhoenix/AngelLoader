using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class FilterTagsForm : DarkFormBase
{
    private readonly Control[] _separatorPaintControls;

    internal readonly TagsFilter TagsFilter = new();

    internal FilterTagsForm(FMCategoriesCollection sourceTags, TagsFilter tagsFilter)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        _separatorPaintControls = new Control[] { OKButton };

        tagsFilter.DeepCopyTo(TagsFilter);

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();

        ControlUtils.FillTreeViewFromTags(OriginTreeView, sourceTags, sort: true, selectFirst: true);

        if (TagsFilter.AndTags.Count > 0) FillTreeView(TagsFilter.AndTags);
        if (TagsFilter.OrTags.Count > 0) FillTreeView(TagsFilter.OrTags);
        if (TagsFilter.NotTags.Count > 0) FillTreeView(TagsFilter.NotTags);
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

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

        // IMPORTANT: These CANNOT be GrowAndShrink because we need to manually grow them here! (special case)
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

    private void FillTreeView(FMCategoriesCollection tags)
    {
        DarkTreeView tv =
            tags == TagsFilter.AndTags ? AndTreeView :
            tags == TagsFilter.OrTags ? OrTreeView :
            NotTreeView;

        ControlUtils.FillTreeViewFromTags(tv, tags, sort: true);

        CheckTagInAny();
    }

    private void AddTagsButtons_Click(object sender, EventArgs e)
    {
        DarkTreeView o = OriginTreeView;

        if (o.SelectedNode == null) return;

        FMCategoriesCollection filteredTags =
            sender == AndButton ? TagsFilter.AndTags :
            sender == OrButton ? TagsFilter.OrTags :
            TagsFilter.NotTags;

        // Parent node = category, child node = tag
        bool isCategory = o.SelectedNode.Parent == null;
        string cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent!.Text;

        if (filteredTags.TryGetValue(cat, out FMTagsCollection tags))
        {
            if (isCategory)
            {
                tags.Clear();
            }
            else
            {
                tags.Add(o.SelectedNode.Text);
            }
        }
        else
        {
            var item = new FMTagsCollection();
            filteredTags.Add(cat, item);
            if (!isCategory)
            {
                item.Add(o.SelectedNode.Text);
            }
        }

        FillTreeView(filteredTags);
    }

    private void RemoveSelectedButtons_Click(object sender, EventArgs e)
    {
        FMCategoriesCollection tags =
            sender == RemoveSelectedAndButton ? TagsFilter.AndTags :
            sender == RemoveSelectedOrButton ? TagsFilter.OrTags :
            TagsFilter.NotTags;

        DarkTreeView tv =
            sender == RemoveSelectedAndButton ? AndTreeView :
            sender == RemoveSelectedOrButton ? OrTreeView :
            NotTreeView;

        if (tv.SelectedNode == null) return;

        // Parent node (category)
        if (tv.SelectedNode.Parent == null)
        {
            tags.Remove(tv.SelectedNode.Text);
        }
        // Child node (tag)
        else
        {
            string cat = tv.SelectedNode.Parent.Text;

            if (tags.TryGetValue(cat, out FMTagsCollection tagsList))
            {
                tagsList.Remove(tv.SelectedNode.Text);
                if (tagsList.Count == 0)
                {
                    tags.Remove(cat);
                }
            }
        }

        FillTreeView(tags);
    }

    private void RemoveAllButtons_Click(object sender, EventArgs e)
    {
        FMCategoriesCollection tags =
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
        DarkTreeView o = OriginTreeView;

        if (o.SelectedNode == null) return;

        // Parent node = category, child node = tag
        bool isCategory = o.SelectedNode.Parent == null;
        string cat = isCategory ? o.SelectedNode.Text : o.SelectedNode.Parent!.Text;

        bool tagInAny = false;

        for (int t = 0; t < 3; t++)
        {
            FMCategoriesCollection filteredTags = t switch
            {
                0 => TagsFilter.AndTags,
                1 => TagsFilter.OrTags,
                _ => TagsFilter.NotTags
            };

            if (filteredTags.TryGetValue(cat, out FMTagsCollection tagsList) &&
                (isCategory || tagsList.Contains(o.SelectedNode.Text) ||
                 (cat == o.SelectedNode.Parent!.Text && tagsList.Count == 0)))
            {
                tagInAny = true;
            }
        }

        AndButton.Enabled = !tagInAny;
        OrButton.Enabled = !tagInAny;
        NotButton.Enabled = !tagInAny;
    }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private void RemoveButtons_Paint(object sender, PaintEventArgs e) => Images.PaintMinusButton((Button)sender, e);

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private void RemoveAllButtons_Paint(object sender, PaintEventArgs e) => Images.PaintXButton((Button)sender, e);

    private void BottomFLP_Paint(object sender, PaintEventArgs e)
    {
        Images.PaintControlSeparators(
            e: e,
            pixelsFromVerticalEdges: 5,
            items: _separatorPaintControls);
    }

    private void ArrowButtons_Paint(object sender, PaintEventArgs e)
    {
        Button button = (Button)sender;
        Images.PaintArrow7x4(
            g: e.Graphics,
            direction: Direction.Right,
            area: new Rectangle(0, 0, 15, button.Height),
            controlEnabled: button.Enabled
        );
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.F)
        {
            FindTagTextBox.Focus();
            FindTagTextBox.SelectAll();
        }
        base.OnKeyDown(e);
    }
}
