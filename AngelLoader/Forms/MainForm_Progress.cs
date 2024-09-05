using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed partial class MainForm
{
    // Not great code really, but works.

    private ProgressBox? ProgressBox;

    // Note! If we WEREN'T always invoking this, we would want to have a lock around it!

    [MemberNotNull(nameof(ProgressBox))]
    private void ConstructProgressBox()
    {
        if (ProgressBox != null) return;

        ProgressBox = new ProgressBox(this) { Tag = LoadType.Lazy, Visible = false };
        Controls.Add(ProgressBox);
        ProgressBox.Anchor = AnchorStyles.None;
        ProgressBox.DarkModeEnabled = Global.Config.DarkMode;
        ProgressBox.SetSizeToDefault();
    }

    // Just always invoke these, because they're almost always called from another thread anyway. Keeps it
    // simple.

    // We don't cache the actions anymore, because we still ended up recreating the params object[] array
    // every time, which was in almost all cases _larger_ than the 32 bytes of an action. Also, it made our
    // parameters un-statically-checkable for correct number and types, since they were a variable-length
    // array of plain objects. So... yeah, not worth it.

    // Convenience methods for first show - they handle a few parameters for you
    #region Show methods

    public void ShowProgressBox_Single(
        string? message1 = null,
        string? message2 = null,
        ProgressType? progressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) => Invoke(() =>
    {
        ConstructProgressBox();
        ProgressBox.SetState(
            visible: true,
            size: ProgressSizeMode.Single,
            mainMessage1: message1 ?? "",
            mainMessage2: message2 ?? "",
            mainPercent: 0,
            mainProgressBarType: progressType ?? ProgressBox.DefaultProgressType,
            subMessage: "",
            subPercent: 0,
            subProgressBarType: ProgressType.Determinate,
            cancelButtonMessage: cancelMessage ?? ProgressBox.DefaultCancelMessage,
            cancelAction: cancelAction ?? NullAction);
    });

#if false
    public void ShowProgressBox_Double(
        string? mainMessage1 = null,
        string? mainMessage2 = null,
        ProgressType? mainProgressType = null,
        string? subMessage = null,
        ProgressType? subProgressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) => Invoke(() =>
    {
        ConstructProgressBox();
        ProgressBox.SetState(
            visible: true,
            size: ProgressSizeMode.Double,
            mainMessage1: mainMessage1 ?? "",
            mainMessage2: mainMessage2 ?? "",
            mainPercent: 0,
            mainProgressBarType: mainProgressType ?? ProgressBox.DefaultProgressType,
            subMessage: subMessage ?? "",
            subPercent: 0,
            subProgressBarType: subProgressType ?? ProgressBox.DefaultProgressType,
            cancelButtonMessage: cancelMessage ?? ProgressBox.DefaultCancelMessage,
            cancelAction: cancelAction ?? NullAction);
    });
#endif

    #endregion

    // Intended to be used after first show, for modifying the state
    #region Set methods

    public void SetProgressBoxState_Single(
        bool? visible = null,
        string? message1 = null,
        string? message2 = null,
        int? percent = null,
        ProgressType? progressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) => Invoke(() =>
    {
        ConstructProgressBox();
        ProgressBox.SetState(
            visible: visible,
            size: ProgressSizeMode.Single,
            mainMessage1: message1,
            mainMessage2: message2,
            mainPercent: percent,
            mainProgressBarType: progressType,
            subMessage: "",
            subPercent: 0,
            subProgressBarType: ProgressType.Determinate,
            cancelButtonMessage: cancelMessage,
            cancelAction: cancelAction);
    });

    public void SetProgressBoxState_Double(
        bool? visible = null,
        string? mainMessage1 = null,
        string? mainMessage2 = null,
        int? mainPercent = null,
        ProgressType? mainProgressType = null,
        string? subMessage = null,
        int? subPercent = null,
        ProgressType? subProgressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) => Invoke(() =>
    {
        ConstructProgressBox();
        ProgressBox.SetState(
            visible: visible,
            size: ProgressSizeMode.Double,
            mainMessage1: mainMessage1,
            mainMessage2: mainMessage2,
            mainPercent: mainPercent,
            mainProgressBarType: mainProgressType,
            subMessage: subMessage,
            subPercent: subPercent,
            subProgressBarType: subProgressType,
            cancelButtonMessage: cancelMessage,
            cancelAction: cancelAction);
    });

    public void SetProgressPercent(int percent) => Invoke(() =>
    {
        ConstructProgressBox();
        ProgressBox.SetState(
            visible: null,
            size: null,
            mainMessage1: null,
            mainMessage2: null,
            mainPercent: percent,
            mainProgressBarType: null,
            subMessage: null,
            subPercent: null,
            subProgressBarType: null,
            cancelButtonMessage: null,
            cancelAction: null);
    });

    public void SetProgressBoxState(
        bool? visible = null,
        ProgressSizeMode? size = null,
        string? mainMessage1 = null,
        string? mainMessage2 = null,
        int? mainPercent = null,
        ProgressType? mainProgressType = null,
        string? subMessage = null,
        int? subPercent = null,
        ProgressType? subProgressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) =>
        Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox.SetState(
                visible: visible,
                size: size,
                mainMessage1: mainMessage1,
                mainMessage2: mainMessage2,
                mainPercent: mainPercent,
                mainProgressBarType: mainProgressType,
                subMessage: subMessage,
                subPercent: subPercent,
                subProgressBarType: subProgressType,
                cancelButtonMessage: cancelMessage,
                cancelAction: cancelAction);
        });

    #endregion

    public void HideProgressBox() => Invoke(() => ProgressBox?.HideThis());

    public bool ProgressBoxVisible() => (bool)Invoke(() => ProgressBox is { Visible: true });

    private sealed class ProgressItemData
    {
        internal int Handle;
        internal string Text;
        internal int Percent;

        public ProgressItemData(string text, int percent, int handle)
        {
            Text = text;
            Percent = percent;
            Handle = handle;
        }
    }

    private readonly List<ProgressItemData> _progressItems = new();

    public void MultiItemProgress_Show(
        int rows,
        string? message1 = null,
        string? message2 = null,
        ProgressType? progressType = null,
        string? cancelMessage = null,
        Action? cancelAction = null) => Invoke(() =>
    {
        if (_MultiItemTestDGV.Visible) return;
        _progressItems.Clear();
        _MultiItemTestDGV.BringToFront();
        _MultiItemTestDGV.RowCount = rows;
        _MultiItemTestDGV.Show();
    });

    public void MultiItemProgress_Hide() => Invoke(() =>
    {
        _MultiItemTestDGV.Hide();
        _progressItems.Clear();
    });

    public int MultiItemProgress_GetNewItemHandle() => (int)Invoke(() =>
    {
        ProgressItemData item = new("", 0, 0);
        item.Handle = item.GetHashCode();
        _progressItems.Add(item);
        _MultiItemTestDGV.Refresh();
        return item.Handle;
    });

    public void MultiItemProgress_CloseItemHandle(int handle) => Invoke(() =>
    {
        for (int i = 0; i < _progressItems.Count; i++)
        {
            ProgressItemData item = _progressItems[i];
            if (item.Handle == handle)
            {
                _progressItems.RemoveAt(i);
                break;
            }
        }
        _MultiItemTestDGV.Refresh();
    });

    public void MultiItemProgress_SetItemData(int handle, string text, int percent) => Invoke(() =>
    {
        ProgressItemData? item = _progressItems.Find(x => x.Handle == handle);
        if (item == null) return;
        item.Text = text;
        item.Percent = percent;
        //_MultiItemTestDGV.Invalidate();
        _MultiItemTestDGV.InvalidateRow(_progressItems.IndexOf(item));
    });

    private void _MultiItemTestDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
        if (_progressItems.Count == 0) return;

        if (e.RowIndex > _progressItems.Count - 1)
        {
            e.Value = "";
            return;
        }

        ProgressItemData item = _progressItems[e.RowIndex];

        e.Value = item.Text + ", " + item.Percent.ToStrCur();
    }

}
