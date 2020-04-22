using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI.Ookii.Dialogs;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    [PublicAPI]
    internal interface IView : ILocalizable
    {
        #region Progress box

        void ShowProgressBox(ProgressTasks progressTask, bool suppressShow = false);
        void HideProgressBox();
        void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName);
        void ReportFMExtractProgress(int percent);
        void ReportCachingProgress(int percent);
        void SetCancelingFMInstall();

        #endregion

        int CurrentSortedColumnIndex { get; }
        SortOrder CurrentSortDirection { get; }
        bool ShowRecentAtTop { get; }
        void ShowFMsListZoomButtons(bool visible);
        void ShowInstallUninstallButton(bool enabled);
        void ClearUIAndCurrentInternalFilter();
        void ChangeGameOrganization(bool startup = false);
        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);
        /// <summary>
        /// This can be called while the FindFMs() thread is running; it doesn't interfere.
        /// </summary>
        void InitThreadable();
        /// <summary>
        /// Call this only after the FindFMs() thread has finished.
        /// </summary>
        /// <returns></returns>
        Task FinishInitAndShow();
        Filter GetFilter();
        string GetTitleFilter();
        string GetAuthorFilter();
        bool[] GetGameFilters();
        bool GetFinishedFilter();
        bool GetUnfinishedFilter();
        bool GetShowUnsupportedFilter();
        List<int> GetFilterShownIndexList();
        void SetFiltered(bool value);
#if DEBUG || (Release_Testing && !RT_StartupOnly)
        string GetDebug1Text();
        string GetDebug2Text();
        void SetDebug1Text(string value);
        void SetDebug2Text(string value);
#endif
        int GetRowCount();
        void SetRowCount(int count);
        void ShowOnly();
        void ShowAlert(string message, string title);
        object InvokeSync(Delegate method);
        object InvokeSync(Delegate method, params object[] args);
        object InvokeAsync(Delegate method);
        object InvokeAsync(Delegate method, params object[] args);
        void Block(bool block);
        Task RefreshSelectedFM(bool refreshReadme);
        void RefreshSelectedFMRowOnly();
        void RefreshFMsListKeepSelection();

        Task SortAndSetFilter(SelectedFM? selFM = null, bool forceDisplayFM = false, bool keepSelection = false,
                              bool gameTabSwitch = false);
        bool AskToContinue(string message, string title, bool noIcon = false);

        (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
                                        string? yes, string? no);

        (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
                                             string yes, string no, string cancel);

        void ChangeReadmeBoxFont(bool useFixed);

        void ChangeGameTabNameShortness(bool refreshFilterBarPositionIfNeeded);
    }
}
