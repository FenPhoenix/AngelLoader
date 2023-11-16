using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class PatchTabPage : Lazy_TabsBase
{
    private Lazy_PatchPage _page = null!;

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_PatchPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.Patch_NDSubs_CheckBox.CheckStateChanged += Patch_NDSubs_CheckBox_CheckStateChanged;
            _page.Patch_PostProc_CheckBox.CheckStateChanged += Patch_PostProc_CheckBox_CheckStateChanged;
            _page.Patch_NewMantle_CheckBox.CheckStateChanged += Patch_NewMantle_CheckBox_CheckStateChanged;

            _page.PatchRemoveDMLButton.PaintCustom += PatchRemoveDMLButton_Paint;
            _page.PatchRemoveDMLButton.Click += PatchRemoveDMLButton_Click;

            _page.PatchAddDMLButton.PaintCustom += PatchAddDMLButton_Paint;
            _page.PatchAddDMLButton.Click += PatchAddDMLButton_Click;

            _page.PatchOpenFMFolderButton.Click += PatchOpenFMFolderButton_Click;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void Localize()
    {
        if (!_constructed) return;

        _page.Patch_PerFMValues_Label.Text = LText.PatchTab.OptionOverrides;

        _page.Patch_NewMantle_CheckBox.Text = LText.PatchTab.NewMantle;
        _owner.MainToolTip.SetToolTip(
            _page.Patch_NewMantle_CheckBox,
            LText.PatchTab.NewMantle_ToolTip_Checked + "\r\n" +
            LText.PatchTab.NewMantle_ToolTip_Unchecked + "\r\n" +
            LText.PatchTab.NewMantle_ToolTip_NotSet
        );

        _page.Patch_PostProc_CheckBox.Text = LText.PatchTab.PostProc;
        _owner.MainToolTip.SetToolTip(
            _page.Patch_PostProc_CheckBox,
            LText.PatchTab.PostProc_ToolTip_Checked + "\r\n" +
            LText.PatchTab.PostProc_ToolTip_Unchecked + "\r\n" +
            LText.PatchTab.PostProc_ToolTip_NotSet
        );

        _page.Patch_NDSubs_CheckBox.Text = LText.PatchTab.Subtitles;
        _owner.MainToolTip.SetToolTip(
            _page.Patch_NDSubs_CheckBox,
            LText.PatchTab.Subtitles_ToolTip_Checked + "\r\n" +
            LText.PatchTab.Subtitles_ToolTip_Unchecked + "\r\n" +
            LText.PatchTab.Subtitles_ToolTip_NotSet + "\r\n\r\n" +
            LText.PatchTab.Subtitles_ToolTip_NewDarkNote
        );

        _page.PatchDMLPatchesLabel.Text = LText.PatchTab.DMLPatchesApplied;
        _owner.MainToolTip.SetToolTip(_page.PatchAddDMLButton, LText.PatchTab.AddDMLPatchToolTip);
        _owner.MainToolTip.SetToolTip(_page.PatchRemoveDMLButton, LText.PatchTab.RemoveDMLPatchToolTip);
        _page.PatchOpenFMFolderButton.Text = LText.PatchTab.OpenFMFolder;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            bool gameSupported = GameSupportsMods(fm.Game);

            if (gameSupported)
            {
                foreach (Control c in _page.Controls)
                {
                    if (c != _page.PatchMainPanel)
                    {
                        c.Enabled = true;
                    }
                }

                _page.Patch_NewMantle_CheckBox.SetFromNullableBool(fm.NewMantle);
                _page.Patch_PostProc_CheckBox.SetFromNullableBool(fm.PostProc);
                _page.Patch_NDSubs_CheckBox.SetFromNullableBool(fm.NDSubs);
            }
            else
            {
                DisablePatchNonDMLSection();
            }

            _page.PatchMainPanel.Enabled = true;

            if (fm.Installed && gameSupported)
            {
                ShowPatchInstalledOnlySection(enable: true);
            }
            else
            {
                HidePatchInstalledOnlySection();
            }

            _page.PatchDMLsPanel.Enabled = GameIsDark(fm.Game);

            if (GameIsDark(fm.Game) && fm.Installed)
            {
                _page.PatchMainPanel.Show();
                using (new UpdateRegion(_page.PatchDMLsListBox))
                {
                    _page.PatchDMLsListBox.Items.Clear();
                    (bool success, List<string> dmlFiles) = Core.GetDMLFiles(fm);
                    if (success)
                    {
                        foreach (string f in dmlFiles)
                        {
                            if (!f.IsEmpty()) _page.PatchDMLsListBox.Items.Add(f);
                        }
                    }
                }
            }
        }
        else
        {
            DisablePatchNonDMLSection();

            ShowPatchInstalledOnlySection(enable: false);
        }
    }

    #endregion

    #region Page

    private void Patch_NewMantle_CheckBox_CheckStateChanged(object? sender, EventArgs e)
    {
        if (_owner.EventsDisabled > 0) return;
        _owner.FMsDGV.GetMainSelectedFM().NewMantle = _page.Patch_NewMantle_CheckBox.ToNullableBool();
        Ini.WriteFullFMDataIni();
    }

    private void Patch_PostProc_CheckBox_CheckStateChanged(object? sender, EventArgs e)
    {
        if (_owner.EventsDisabled > 0) return;
        _owner.FMsDGV.GetMainSelectedFM().PostProc = _page.Patch_PostProc_CheckBox.ToNullableBool();
        Ini.WriteFullFMDataIni();
    }

    private void Patch_NDSubs_CheckBox_CheckStateChanged(object? sender, EventArgs e)
    {
        if (_owner.EventsDisabled > 0) return;
        _owner.FMsDGV.GetMainSelectedFM().NDSubs = _page.Patch_NDSubs_CheckBox.ToNullableBool();
        Ini.WriteFullFMDataIni();
    }

    private void PatchRemoveDMLButton_Click(object? sender, EventArgs e)
    {
        if (_page.PatchDMLsListBox.SelectedIndex == -1) return;

        bool success = Core.RemoveDML(_owner.FMsDGV.GetMainSelectedFM(), _page.PatchDMLsListBox.SelectedItem);
        if (!success) return;

        _page.PatchDMLsListBox.RemoveAndSelectNearest();
    }

    // @ViewBusinessLogic(PatchAddDMLButton_Click)
    private void PatchAddDMLButton_Click(object? sender, EventArgs e)
    {
        var dmlFiles = new List<string>();

        using (var d = new OpenFileDialog())
        {
            d.Multiselect = true;
            d.Filter = LText.BrowseDialogs.DMLFiles + "|*.dml";
            if (d.ShowDialogDark(this) != DialogResult.OK || d.FileNames.Length == 0) return;
            dmlFiles.AddRange(d.FileNames);
        }

        HashSetI itemsHashSet = _page.PatchDMLsListBox.ItemsAsStrings.ToHashSetI();

        using (new UpdateRegion(_page.PatchDMLsListBox))
        {
            foreach (string f in dmlFiles)
            {
                if (f.IsEmpty()) continue;

                bool success = Core.AddDML(_owner.FMsDGV.GetMainSelectedFM(), f);
                if (!success) return;

                string dmlFileName = Path.GetFileName(f);
                if (!itemsHashSet.Contains(dmlFileName))
                {
                    _page.PatchDMLsListBox.Items.Add(dmlFileName);
                }
            }
        }
    }

    private void PatchOpenFMFolderButton_Click(object? sender, EventArgs e) => Core.OpenFMFolder(_owner.FMsDGV.GetMainSelectedFM());

    private void PatchAddDMLButton_Paint(object? sender, PaintEventArgs e) => Images.PaintPlusButton(_page.PatchAddDMLButton, e);

    private void PatchRemoveDMLButton_Paint(object? sender, PaintEventArgs e) => Images.PaintMinusButton(_page.PatchRemoveDMLButton, e);

    private void DisablePatchNonDMLSection()
    {
        foreach (Control c in _page.Controls)
        {
            if (c != _page.PatchMainPanel)
            {
                c.Enabled = false;
            }

            switch (c)
            {
                case TextBox tb:
                    tb.Text = "";
                    break;
                case CheckBox chk:
                    if (chk.ThreeState)
                    {
                        chk.CheckState = CheckState.Indeterminate;
                    }
                    else
                    {
                        chk.Checked = false;
                    }
                    break;
            }
        }
    }

    private void HidePatchInstalledOnlySection()
    {
        _page.PatchDMLsListBox.Items.Clear();
        _page.PatchMainPanel.Hide();
    }

    private void ShowPatchInstalledOnlySection(bool enable)
    {
        _page.PatchDMLsListBox.Items.Clear();
        _page.PatchMainPanel.Show();
        _page.PatchMainPanel.Enabled = enable;
    }

    #endregion
}
