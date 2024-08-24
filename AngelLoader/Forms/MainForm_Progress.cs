using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed partial class MainForm
{
    // Not great code really, but works.

    private ProgressPanel? ProgressBox;

    // Note! If we WEREN'T always invoking this, we would want to have a lock around it!

    [MemberNotNull(nameof(ProgressBox))]
    private void ConstructProgressBox()
    {
        if (ProgressBox != null) return;

        ProgressBox = new ProgressPanel(this) { Tag = LoadType.Lazy, Visible = false };
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
            mainProgressBarType: progressType ?? ProgressPanel.DefaultProgressType,
            subMessage: "",
            subPercent: 0,
            subProgressBarType: ProgressType.Determinate,
            cancelButtonMessage: cancelMessage ?? ProgressPanel.DefaultCancelMessage,
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
            mainProgressBarType: mainProgressType ?? ProgressPanel.DefaultProgressType,
            subMessage: subMessage ?? "",
            subPercent: 0,
            subProgressBarType: subProgressType ?? ProgressPanel.DefaultProgressType,
            cancelButtonMessage: cancelMessage ?? ProgressPanel.DefaultCancelMessage,
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
}
