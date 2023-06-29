using System;
using System.Collections.Generic;
using System.Drawing;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class AddTagLLDropDown : IDarkable
{
    private bool _constructed;

    private readonly MainForm _form;
    private readonly TagsTabPage _page;
    private readonly Lazy_TagsPage _realPage;

    private DarkListBox _listBox = null!;
    internal DarkListBox ListBox
    {
        get
        {
            Construct();
            return _listBox;
        }
    }

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (!_constructed) return;

            _listBox.DarkModeEnabled = _darkModeEnabled;
        }
    }

    internal AddTagLLDropDown(MainForm form, TagsTabPage page, Lazy_TagsPage realPage)
    {
        _form = form;
        _page = page;
        _realPage = realPage;
    }

    private void Construct()
    {
        if (_constructed) return;

        _listBox = new DarkListBox
        {
            Tag = LoadType.Lazy,

            Scrollable = true,
            TabIndex = 3,
            Visible = false,

            DarkModeEnabled = _darkModeEnabled
        };

        _listBox.SelectedIndexChanged += _page.AddTagListBox_SelectedIndexChanged;
        _listBox.KeyDown += _page.AddTagTextBoxOrListBox_KeyDown;
        _listBox.Leave += _page.AddTagTextBoxOrListBox_Leave;
        _listBox.MouseUp += _page.AddTagListBox_MouseUp;

        _form.EverythingPanel.Controls.Add(_listBox);

        _constructed = true;
    }

    internal void SetItemsAndShow(List<string> list)
    {
        Construct();

        using (new UpdateRegion(_listBox))
        {
            _listBox.Items.Clear();
            foreach (string item in list) _listBox.Items.Add(item);
        }

        Point p = _form.PointToClient_Fast(_realPage.AddTagTextBox.PointToScreen_Fast(new Point(0, 0)));
        _listBox.Location = p with { Y = p.Y + _realPage.AddTagTextBox.Height };
        _listBox.Size = new Size(Math.Max(_realPage.AddTagTextBox.Width, 256), 225);

        _listBox.BringToFront();
        _listBox.Show();
    }

    internal bool Visible => _constructed && _listBox.Visible;
    internal bool Focused => _constructed && _listBox.Focused;

    internal void HideAndClear()
    {
        if (!_constructed) return;

        _listBox.Hide();
        _listBox.Items.Clear();
    }
}
