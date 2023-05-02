using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    private Func<string>? _errorTextGetter;
    public void SetErrorTextGetter(Func<string> errorTextGetter) => _errorTextGetter = errorTextGetter;

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

        CheckList.SetCautionVisiblePredicate(() => ShowImportantCheckBox.Checked);
    }

    public void Localize(string headerText)
    {
        HeaderLabel.Text = headerText;
        ShowImportantCheckBox.Text = LText.ModsTab.ShowImportantMods;
        EnableAllButton.Text = LText.ModsTab.EnableAll;
        DisableNonImportantButton.Text = LText.ModsTab.DisableAll;
        MainToolTip.SetToolTip(DisableNonImportantButton, LText.ModsTab.DisableAllToolTip);
        DisabledModsLabel.Text = LText.ModsTab.DisabledMods;
        if (CheckList.InErrorState && _errorTextGetter != null)
        {
            CheckList.SetErrorText(_errorTextGetter.Invoke());
        }
    }

    private void Commit()
    {
        string[] disabledMods = DisabledModsTextBox.Text.Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries);

        var modNames = new DictionaryI<int>(CheckList.CheckItems.Length);

        for (int i = 0; i < CheckList.CheckItems.Length; i++)
        {
            DarkCheckList.CheckItem checkItem = CheckList.CheckItems[i];
            modNames[checkItem.Text] = i;
        }

        bool[] checkedStates = InitializedArray(CheckList.CheckItems.Length, true);

        foreach (string mod in disabledMods)
        {
            if (modNames.TryGetValue(mod, out int index))
            {
                checkedStates[index] = false;
            }
        }

        CheckList.SetItemCheckedStates(checkedStates);

        DisabledModsUpdated?.Invoke(this, EventArgs.Empty);
    }

    [PublicAPI]
    public void Set(GameIndex gameIndex, string disabledMods) => Set(GameIndexToGame(gameIndex), disabledMods);

    [PublicAPI]
    public void Set(Game game, string disabledMods)
    {
        try
        {
            CheckList.SuspendDrawing();

            DisabledModsTextBox.Text = disabledMods;

            CheckList.ClearList();

            if (!game.ConvertsToModSupporting(out GameIndex gameIndex)) return;

            List<Mod> mods = Config.GetMods(gameIndex);

            HashSetI disabledModsList = disabledMods
                .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                .ToHashSetI();

            var checkItems = new DarkCheckList.CheckItem[mods.Count];

            for (int i = 0; i < mods.Count; i++)
            {
                Mod mod = mods[i];
                checkItems[i] = new DarkCheckList.CheckItem(
                    @checked: !disabledModsList.Contains(mod.InternalName),
                    text: mod.InternalName,
                    caution: mod.IsUber);
            }

            CheckList.FillList(checkItems, LText.ModsTab.ImportantModsCaution);
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

    private void EnableAllButton_Click(object sender, EventArgs e)
    {
        using (new DisableEvents(this))
        {
            foreach (Control control in CheckList.Controls)
            {
                if (control is CheckBox checkBox)
                {
                    checkBox.Checked = true;
                }
            }
        }

        UpdateDisabledMods();
    }

    private void DisableNonImportantButton_Click(object sender, EventArgs e)
    {
        using (new DisableEvents(this))
        {
            foreach (Control control in CheckList.Controls)
            {
                if (control is CheckBox checkBox && !DarkCheckList.IsControlCaution(checkBox))
                {
                    checkBox.Checked = false;
                }
            }
        }

        UpdateDisabledMods();
    }

    private void UpdateDisabledMods()
    {
        string disabledMods = "";

        foreach (DarkCheckList.CheckItem item in CheckList.CheckItems)
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
        CheckList.ShowCautionSection(ShowImportantCheckBox.Checked);
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

    private void CheckList_ItemCheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
        UpdateDisabledMods();
    }
}
