using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class TagsTabPage : Lazy_TabsBase
{
    private Lazy_TagsPage _page = null!;

    #region Lazy-loaded subcontrols

    private AddTagLLDropDown AddTagLLDropDown = null!;
    private DynamicItemsLLMenu AddTagLLMenu = null!;

    #endregion

    #region Theme

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool DarkModeEnabled
    {
        get => base.DarkModeEnabled;
        set
        {
            // These need to be up here or they don't theme right
            if (_constructed)
            {
                AddTagLLDropDown.DarkModeEnabled = DarkModeEnabled;
                AddTagLLMenu.DarkModeEnabled = DarkModeEnabled;
            }

            if (DarkModeEnabled == value) return;
            base.DarkModeEnabled = value;
        }
    }

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_TagsPage>();

        AddTagLLDropDown = new AddTagLLDropDown(_owner, this, _page);
        AddTagLLMenu = new DynamicItemsLLMenu(_owner);

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.AddTagButton.Click += AddTagButton_Click;
            _page.AddTagTextBox.TextChanged += AddTagTextBox_TextChanged;
            _page.AddTagTextBox.KeyDown += AddTagTextBoxOrListBox_KeyDown;
            _page.AddTagTextBox.Leave += AddTagTextBoxOrListBox_Leave;
            _page.RemoveTagButton.Click += RemoveTagButton_Click;
            _page.AddTagFromListButton.Click += AddTagFromListButton_Click;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void Localize()
    {
        if (!_constructed) return;

        _page.AddTagButton.SetTextForTextBoxButtonCombo(_page.AddTagTextBox, LText.TagsTab.AddTag);
        _page.AddTagFromListButton.Text = LText.TagsTab.AddFromList;
        _page.RemoveTagButton.Text = LText.TagsTab.RemoveTag;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();
        if (fm != null)
        {
            foreach (Control c in _page.Controls) c.Enabled = true;
            FillFMTags(fm.Tags, sort: false);
            _page.AddTagTextBox.Text = "";
        }
        else
        {
            _page.AddTagTextBox.Text = "";
            _page.TagsTreeView.Nodes.Clear();
            foreach (Control c in _page.Controls) c.Enabled = false;
        }
    }

    #endregion

    #region Page

    private void FillFMTags(FMCategoriesCollection fmTags, bool sort)
    {
        if (!_constructed) return;
        ControlUtils.FillTreeViewFromTags(_page.TagsTreeView, fmTags, sort: sort);
    }

    // null! checks - industrial strength protection against stupid event handler firing in the component init method...
    // (But now that we've reorganized to be lazy-loaded, it might well not matter. Still, does no harm.)
    internal bool AddTagLLDropDownVisible()
    {
        return _constructed && AddTagLLDropDown != null! && AddTagLLDropDown.Visible;
    }

    internal void HideAndClearAddTagLLDropDown()
    {
        if (_constructed && AddTagLLDropDown != null!)
        {
            AddTagLLDropDown.HideAndClear();
        }
    }

    internal bool AddTagLLDropDownFocused()
    {
        return _constructed && AddTagLLDropDown.Focused;
    }

    internal bool CursorOverAddTagLLDropDown(bool fullArea = false)
    {
        return _constructed && _owner.CursorOverControl(AddTagLLDropDown.ListBox, fullArea);
    }

    internal bool CursorOverAddTagTextBox(bool fullArea = false)
    {
        return _constructed && _owner.CursorOverControl(_page.AddTagTextBox, fullArea);
    }

    internal bool CursorOverAddTagButton(bool fullArea = false)
    {
        return _constructed && _owner.CursorOverControl(_page.AddTagButton, fullArea);
    }

    // Robustness for if the user presses tab to get away, rather than clicking
    internal void AddTagTextBoxOrListBox_Leave(object sender, EventArgs e)
    {
        if ((sender == _page.AddTagTextBox && !AddTagLLDropDown.Focused) ||
            (AddTagLLDropDown.Visible &&
             sender == AddTagLLDropDown.ListBox && !_page.AddTagTextBox.Focused))
        {
            AddTagLLDropDown.HideAndClear();
        }
    }

    private void AddTagTextBox_TextChanged(object sender, EventArgs e)
    {
        if (_owner.EventsDisabled > 0) return;

        List<string> list = FMTags.GetMatchingTagsList(_page.AddTagTextBox.Text);
        if (list.Count == 0)
        {
            AddTagLLDropDown.HideAndClear();
        }
        else
        {
            AddTagLLDropDown.SetItemsAndShow(list);
        }
    }

    internal void AddTagTextBoxOrListBox_KeyDown(object sender, KeyEventArgs e)
    {
        DarkListBox box = AddTagLLDropDown.ListBox;

        switch (e.KeyCode)
        {
            case Keys.Up when box.Items.Count > 0:
                // We can't do a switch expression on the second one, so keep them both the same for consistency
                // ReSharper disable once ConvertConditionalTernaryExpressionToSwitchExpression
                box.SelectedIndex =
                    box.SelectedIndex == -1 ? box.Items.Count - 1 :
                    box.SelectedIndex == 0 ? -1 :
                    box.SelectedIndex - 1;
                // We need this call to make the thing scroll...
                if (box.SelectedIndex > -1) box.EnsureVisible(box.SelectedIndex);
                e.Handled = true;
                break;
            case Keys.Down when box.Items.Count > 0:
                box.SelectedIndex =
                    box.SelectedIndex == -1 ? 0 :
                    box.SelectedIndex == box.Items.Count - 1 ? -1 :
                    box.SelectedIndex + 1;
                if (box.SelectedIndex > -1) box.EnsureVisible(box.SelectedIndex);
                e.Handled = true;
                break;
            case Keys.Enter:
                string catAndTag = box.SelectedIndex == -1 ? _page.AddTagTextBox.Text : box.SelectedItem;
                AddTagOperation(_owner.FMsDGV.GetMainSelectedFM(), catAndTag);
                break;
            default:
                if (sender == box) _page.AddTagTextBox.Focus();
                break;
        }
    }

    internal void AddTagListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (AddTagLLDropDown.ListBox.SelectedIndex == -1) return;

        using (new DisableEvents(_owner))
        {
            _page.AddTagTextBox.Text = AddTagLLDropDown.ListBox.SelectedItem;
        }

        if (_page.AddTagTextBox.Text.Length > 0)
        {
            _page.AddTagTextBox.SelectionStart = _page.AddTagTextBox.Text.Length;
        }
    }

    private (string Category, string Tag)
    SelectedCategoryAndTag()
    {
        TreeNode selNode = _page.TagsTreeView.SelectedNode;
        TreeNode parent;

        return selNode == null
            ? ("", "")
            : (parent = selNode.Parent) == null
                ? (selNode.Text, "")
                : (parent.Text, selNode.Text);
    }

    private void RemoveTagButton_Click(object sender, EventArgs e) => RemoveTagOperation();

    internal void AddTagListBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (AddTagLLDropDown.ListBox.SelectedIndex > -1)
        {
            AddTagOperation(_owner.FMsDGV.GetMainSelectedFM(), AddTagLLDropDown.ListBox.SelectedItem);
        }
    }

    private void ClearTagsSearchBox()
    {
        _page.AddTagTextBox.Clear();
        AddTagLLDropDown.HideAndClear();
    }

    private void AddTagButton_Click(object sender, EventArgs e)
    {
        AddTagOperation(_owner.FMsDGV.GetMainSelectedFM(), _page.AddTagTextBox.Text);
    }

    // @ViewBusinessLogic (AddTagFromListButton_Click - lots of menu items and event hookups)
    private void AddTagFromListButton_Click(object sender, EventArgs e)
    {
        GlobalTags.SortAndMoveMiscToEnd();

        var addTagMenuItems = new ToolStripItem[GlobalTags.Count];
        for (int i = 0; i < GlobalTags.Count; i++)
        {
            CatAndTagsList item = GlobalTags[i];

            if (item.Tags.Count == 0)
            {
                var catItem = new ToolStripMenuItemWithBackingText(item.Category + ":");
                catItem.Click += AddTagMenuEmptyItem_Click;
                addTagMenuItems[i] = catItem;
            }
            else
            {
                var catItem = new ToolStripMenuItemWithBackingText(item.Category);
                addTagMenuItems[i] = catItem;

                if (item.Category != PresetTags.MiscCategory)
                {
                    var customItem = new ToolStripMenuItemWithBackingText(LText.TagsTab.CustomTagInCategory);
                    customItem.Click += AddTagMenuCustomItem_Click;
                    catItem.DropDownItems.Add(customItem);
                    catItem.DropDownItems.Add(new ToolStripSeparator());
                }

                foreach (string tag in item.Tags)
                {
                    var tagItem = new ToolStripMenuItemWithBackingText(tag);

                    tagItem.Click += item.Category == PresetTags.MiscCategory
                        ? AddTagMenuMiscItem_Click
                        : AddTagMenuItem_Click;

                    catItem.DropDownItems.Add(tagItem);
                }
            }
        }

        AddTagLLMenu.ClearAndFillMenu(addTagMenuItems);

        ControlUtils.ShowMenu(AddTagLLMenu.Menu, _page.AddTagFromListButton, MenuPos.LeftDown);
    }

    private void AddTagMenuItem_Click(object sender, EventArgs e)
    {
        var item = (ToolStripMenuItemWithBackingText)sender;
        if (item.HasDropDownItems) return;

        var cat = (ToolStripMenuItemWithBackingText?)item.OwnerItem;
        if (cat == null) return;

        AddTagOperation(_owner.FMsDGV.GetMainSelectedFM(), cat.BackingText + ": " + item.BackingText);
    }

    private void AddTagMenuCustomItem_Click(object sender, EventArgs e)
    {
        var item = (ToolStripMenuItemWithBackingText)sender;

        var cat = (ToolStripMenuItemWithBackingText?)item.OwnerItem;
        if (cat == null) return;

        _page.AddTagTextBox.SetTextAndMoveCursorToEnd(cat.BackingText + ": ");
    }

    private void AddTagMenuMiscItem_Click(object sender, EventArgs e) => _page.AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItemWithBackingText)sender).BackingText);

    private void AddTagMenuEmptyItem_Click(object sender, EventArgs e) => _page.AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItemWithBackingText)sender).BackingText + " ");

    private void AddTagOperation(FanMission fm, string catAndTag)
    {
        if (!catAndTag.CharCountIsAtLeast(':', 2) && !catAndTag.IsWhiteSpace())
        {
            FMTags.AddTagToFM(fm, catAndTag);
            Ini.WriteFullFMDataIni();
            FillFMTags(fm.Tags, sort: true);
        }

        ClearTagsSearchBox();
    }

    private void RemoveTagOperation()
    {
        FanMission? fm = _owner.GetMainSelectedFMOrNull();
        if (fm == null) return;

        (string catText, string tagText) = SelectedCategoryAndTag();
        if (catText.IsEmpty() && tagText.IsEmpty()) return;

        bool isCategory = tagText.IsEmpty();
        bool success = FMTags.RemoveTagFromFM(fm, catText, tagText, isCategory);
        if (success)
        {
            FillFMTags(fm.Tags, sort: true);
        }
    }

    #endregion
}
