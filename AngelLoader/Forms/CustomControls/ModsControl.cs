using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class ModsControl : UserControl, IEventDisabler
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

#if DEBUG

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new bool AutoScroll { get; set; }

#endif

    public ModsControl()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
        Tag = LoadType.Lazy;

        CheckList.Paint += CheckList_Paint;
    }

    #region CheckList

    private Rectangle _cautionRectangle = Rectangle.Empty;

    private enum ItemType { Caution }

    internal void SoftClearList()
    {
        CheckItems = Array.Empty<CheckItem>();

        foreach (DarkCheckBox cb in _checkBoxes)
        {
            cb.Hide();
            cb.Tag = null;
        }

        _cautionLabel?.Hide();
        _cautionRectangle = Rectangle.Empty;
    }

    private void SetList(CheckItem[] items, string cautionText)
    {
        const int x = 18;

        bool firstCautionDone = false;

        int y = 0;
        int firstCautionY = 0;

        for (int i = 0; i < items.Length; i++, y += 20)
        {
            CheckItem item = items[i];

            if (!firstCautionDone && item.Caution)
            {
                firstCautionDone = true;
                firstCautionY = y;
            }

            DarkCheckBox cb = _checkBoxes[i];
            cb.Text = item.Text + (item.Caution ? " *" : "");
            cb.Location = new Point(x, 4 + y);
            using (new DisableEvents(this))
            {
                cb.Checked = item.Checked;
            }

            if (item.Caution)
            {
                cb.Tag = ItemType.Caution;
                cb.Visible = ShowImportantCheckBox.Checked;
                cb.SetFontStyle(FontStyle.Italic);
                cb.BackColor = Color.MistyRose;
                cb.DarkModeBackColor = DarkColors.Fen_RedHighlight;
            }
            else
            {
                cb.Tag = null;
                cb.Visible = true;
                cb.SetFontStyle(FontStyle.Regular);
                cb.BackColor = Color.Transparent;
                cb.DarkModeBackColor = null;
            }
        }

        for (int i = items.Length; i < _checkBoxes.Length; i++)
        {
            DarkCheckBox cb = _checkBoxes[i];
            cb.Visible = false;
            cb.Tag = null;
        }

        if (firstCautionDone)
        {
            CautionLabel.Visible = ShowImportantCheckBox.Checked;
            CautionLabel.Location = new Point(4, 8 + y);

            RefreshCautionLabelText(cautionText);

            _cautionRectangle = new Rectangle(
                4,
                4 + firstCautionY,
                0, // Width will be set on draw for manual "anchoring"
                (4 + y) - (4 + firstCautionY)
            );
        }
        else
        {
            _cautionRectangle = Rectangle.Empty;
        }

        CheckItems = items;
    }

    private void RecreateList(int maxCheckBoxCount)
    {
        try
        {
            SuspendLayout();

            CheckList.Controls.DisposeAndClear();
            _checkBoxes.DisposeAll();
            CheckItems = Array.Empty<CheckItem>();

            _checkBoxes = new DarkCheckBox[maxCheckBoxCount];
            for (int i = 0; i < _checkBoxes.Length; i++)
            {
                DarkCheckBox cb = new()
                {
                    AutoSize = true
                };
                _checkBoxes[i] = cb;
                CheckList.Controls.Add(cb);
                cb.CheckedChanged += OnItemsCheckedChanged;
            }

            _cautionLabel = null;

            CheckList.RefreshDarkMode();
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    internal void RefreshCautionLabelText(string text)
    {
        if (_cautionLabel != null)
        {
            _cautionLabel.Text = "* " + text;
        }
    }

    private void OnItemsCheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;

        var s = (DarkCheckBox)sender;

        int checkBoxIndex = Array.IndexOf(_checkBoxes, s, 0, CheckItems.Length);
        if (checkBoxIndex == -1) return;

        CheckItems[checkBoxIndex].Checked = s.Checked;

        UpdateDisabledMods();
    }

    private DarkCheckBox[] _checkBoxes = Array.Empty<DarkCheckBox>();
    private CheckItem[] CheckItems = Array.Empty<CheckItem>();

    private DarkLabel? _cautionLabel;
    private DarkLabel CautionLabel
    {
        get
        {
            if (_cautionLabel == null)
            {
                _cautionLabel = new DarkLabel
                {
                    Tag = ItemType.Caution,
                    AutoSize = true,
                    ForeColor = Color.Maroon,
                    DarkModeForeColor = DarkColors.Fen_CautionText
                };
                CheckList.Controls.Add(_cautionLabel);
                _cautionLabel.DarkModeEnabled = CheckList._darkModeEnabled;
            }

            return _cautionLabel;
        }
    }

    // @Mods(Recycle CheckItems somehow, or get rid of the need for it?)
    private sealed class CheckItem
    {
        internal bool Checked;
        internal readonly string Text;
        internal readonly bool Caution;

        internal CheckItem(bool @checked, string text, bool caution)
        {
            Checked = @checked;
            Text = text;
            Caution = caution;
        }
    }


    private void CheckList_Paint(object sender, PaintEventArgs e)
    {
        if (_cautionRectangle != Rectangle.Empty && ShowImportantCheckBox.Checked)
        {
            _cautionRectangle.Width = CheckList.ClientRectangle.Width - 8;
            e.Graphics.FillRectangle(CheckList._darkModeEnabled ? DarkColors.Fen_RedHighlightBrush : Brushes.MistyRose, _cautionRectangle);
        }
    }

    #endregion

    public void Localize(string headerText)
    {
        HeaderLabel.Text = headerText;
        ShowImportantCheckBox.Text = LText.ModsTab.ShowImportantMods;
        EnableAllButton.Text = LText.ModsTab.EnableAll;
        DisableNonImportantButton.Text = LText.ModsTab.DisableAll;
        MainToolTip.SetToolTip(DisableNonImportantButton, LText.ModsTab.DisableAllToolTip);
        DisabledModsLabel.Text = LText.ModsTab.DisabledMods;
    }

    private void Commit()
    {
        string[] disabledMods = DisabledModsTextBox.Text.Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries);

        var modNames = new DictionaryI<int>(CheckItems.Length);

        for (int i = 0; i < CheckItems.Length; i++)
        {
            CheckItem checkItem = CheckItems[i];
            modNames[checkItem.Text] = i;
        }

        bool[] checkedStates = InitializedArray(CheckItems.Length, true);

        foreach (string mod in disabledMods)
        {
            if (modNames.TryGetValue(mod, out int index))
            {
                checkedStates[index] = false;
            }
        }

        if (checkedStates.Length == CheckItems.Length)
        {
            using (new DisableEvents(this))
            {
                for (int i = 0; i < checkedStates.Length; i++)
                {
                    SetChecked(i, checkedStates[i]);
                }
            }
        }

        DisabledModsUpdated?.Invoke(this, EventArgs.Empty);
    }

    internal void SetAndRecreateList(GameIndex gameIndex, string disabledMods) =>
        SetInternal(GameIndexToGame(gameIndex), disabledMods, forceRecreateList: true);

    internal void Set(Game game, string disabledMods) =>
        SetInternal(game, disabledMods, forceRecreateList: false);

    private void SetInternal(Game game, string disabledMods, bool forceRecreateList)
    {
        try
        {
            CheckList.SuspendDrawing();

            DisabledModsTextBox.Text = disabledMods;

            if (!game.ConvertsToModSupporting(out GameIndex gameIndex))
            {
                SoftClearList();
                return;
            }

            List<Mod> mods = Config.GetMods(gameIndex);

            HashSetI disabledModsHash = disabledMods
                .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                .ToHashSetI();

            var checkItems = new CheckItem[mods.Count];

            for (int i = 0; i < mods.Count; i++)
            {
                Mod mod = mods[i];
                checkItems[i] = new CheckItem(
                    @checked: !disabledModsHash.Contains(mod.InternalName),
                    text: mod.InternalName,
                    caution: mod.IsUber);
            }

            int maxModCount = 0;

            if (forceRecreateList || Config.ModsChanged)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    int modsCount = Config.GetMods((GameIndex)i).Count;
                    if (modsCount > maxModCount)
                    {
                        maxModCount = modsCount;
                    }
                }

                RecreateList(maxModCount);
                /*
                IMPORTANT: DO NOT set ModsChanged to false if we're on the original-game-window path, or we get a crash on the mods tab after!
                If we have not yet loaded the Mods tab, and we go into the original game settings window, then
                close, then load the mods tab, we would crash with index out of range.
                */
                if (!forceRecreateList) Config.ModsChanged = false;
            }

            if (checkItems.Length == 0)
            {
                SoftClearList();
            }
            else
            {
                SetList(checkItems, LText.ModsTab.ImportantModsCaution);
            }
        }
        finally
        {
            CheckList.ResumeDrawing();
        }
    }

    [PublicAPI]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public event EventHandler? DisabledModsTextBoxTextChanged;

    [PublicAPI]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public event EventHandler? DisabledModsUpdated;

    private void SetChecked(int index, bool value)
    {
        _checkBoxes[index].Checked = value;
        CheckItems[index].Checked = value;
    }

    private void EnableAllButton_Click(object sender, EventArgs e)
    {
        using (new DisableEvents(this))
        {
            for (int i = 0; i < CheckItems.Length; i++)
            {
                SetChecked(i, true);
            }
        }

        UpdateDisabledMods();
    }

    private void DisableNonImportantButton_Click(object sender, EventArgs e)
    {
        using (new DisableEvents(this))
        {
            for (int i = 0; i < CheckItems.Length; i++)
            {
                DarkCheckBox checkBox = _checkBoxes[i];
                if (checkBox.Tag is not ItemType.Caution)
                {
                    SetChecked(i, false);
                }
            }
        }

        UpdateDisabledMods();
    }

    private void UpdateDisabledMods()
    {
        string disabledMods = "";

        foreach (CheckItem item in CheckItems)
        {
            if (!item.Checked)
            {
                if (!disabledMods.IsEmpty()) disabledMods += "+";
                disabledMods += item.Text;
            }
        }

        using (new DisableEvents(this))
        {
            DisabledModsTextBox.Text = disabledMods;
        }

        DisabledModsUpdated?.Invoke(CheckList, EventArgs.Empty);
    }

    private void ShowImportantCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;

        if (CheckItems.Length == 0)
        {
            _cautionLabel?.Hide();
        }
        else
        {
            foreach (Control c in CheckList.Controls)
            {
                if (c.Tag is ItemType.Caution)
                {
                    c.Visible = ShowImportantCheckBox.Checked;
                }
            }
        }

        CheckList.Refresh();
    }

    private void DisabledModsTextBox_TextChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
        DisabledModsTextBoxTextChanged?.Invoke(DisabledModsTextBox, e);
    }

    private void DisabledModsTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Commit();
        }
    }

    private void DisabledModsTextBox_Leave(object sender, EventArgs e)
    {
        Commit();
    }
}
