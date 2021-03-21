using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI.Ookii.Dialogs;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    internal interface IView : ISettingsChangeableWindow, IEventDisabler, IKeyPressDisabler, IMessageFilter
    {
        #region Progress box

        void ShowProgressBox(ProgressTask progressTask, bool suppressShow = false);
        void HideProgressBox();
        void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName);
        void ReportFMExtractProgress(int percent);
        void ReportCachingProgress(int percent);
        void SetCancelingFMInstall();

        #endregion

        #region Get column sort state

        Column GetCurrentSortedColumnIndex();

        SortOrder GetCurrentSortDirection();

        #endregion

        #region Init and show

        /// <summary>
        /// This can be called while the FindFMs() thread is running; it doesn't interfere.
        /// </summary>
        void InitThreadable();
        /// <summary>
        /// Call this only after the FindFMs() thread has finished.
        /// </summary>
        /// <returns></returns>
        Task FinishInitAndShow(List<int> fmsViewListUnscanned);

        void ShowOnly();

        #endregion

        #region Show or hide UI elements

        void ShowFMsListZoomButtons(bool visible);

        void ShowInstallUninstallButton(bool enabled);

        void ShowExitButton(bool enabled);

        #endregion

        #region Filter

        Task SortAndSetFilter(SelectedFM? selFM = null, bool forceDisplayFM = false, bool keepSelection = false,
                              bool gameTabSwitch = false);

        Filter GetFilter();

        string GetTitleFilter();

        string GetAuthorFilter();

        bool[] GetGameFiltersEnabledStates();

        bool GetFinishedFilter();

        bool GetUnfinishedFilter();

        bool GetShowUnsupportedFilter();

        List<int> GetFilterShownIndexList();

        bool GetShowRecentAtTop();

        void ClearUIAndCurrentInternalFilter();

        void ChangeGameOrganization(bool startup = false);

        #endregion

        #region Debug

#if DEBUG || (Release_Testing && !RT_StartupOnly)
        string GetDebug1Text();
        string GetDebug2Text();
        void SetDebug1Text(string value);
        void SetDebug2Text(string value);
#endif

        #endregion

        #region Row count

        void SetRowCount(int count);

        #endregion

        #region Invoke

        object InvokeSync(Delegate method);

        //object InvokeSync(Delegate method, params object[] args);

        #endregion

        #region Refresh

        void RefreshSelectedFM(bool rowOnly = false);

        void RefreshFMsListKeepSelection();

        #endregion

        #region Dialogs

        void ShowAlert(string message, string title);

        bool AskToContinue(string message, string title, bool noIcon = false);

        (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
                                        string? yes, string? no, ButtonType? defaultButton = null);

        (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, MessageBoxIcon icon, bool showDontAskAgain,
            string yes, string no, string cancel, DarkTaskDialog.Button? defaultButton = null);

        #endregion

        void Block(bool block);

        void ChangeReadmeBoxFont(bool useFixed);

        void ChangeGameTabNameShortness(bool useShort, bool refreshFilterBarPositionIfNeeded);

        SelectedFM? GetSelectedFMPosInfo();

        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);
#if !ReleaseBeta && !ReleasePublic
        void UpdateGameScreenShotModes();
#endif
    }
}
