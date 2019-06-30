using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using Ookii.Dialogs.WinForms;
using static AngelLoader.CustomControls.ProgressPanel;

namespace AngelLoader.Forms
{
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
        void ShowFMsListZoomButtons(bool visible);
        void ShowInstallUninstallButton(bool enabled);
        Task ClearAllUIAndInternalFilters();
        void ChangeGameOrganization();
        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);
        void RefreshFMsListKeepSelection();
        Task SortAndSetFilter(bool suppressRefresh = false, bool forceRefreshReadme = false,
            bool forceSuppressSelectionChangedEvent = false, bool suppressSuspendResume = false);
        void Init();
        int GetRowCount();
        void SetRowCount(int count);
        void Show();
        void ShowAlert(string message, string title);
        object InvokeSync(Delegate method);
        object InvokeSync(Delegate method, params object[] args);
        object InvokeAsync(Delegate method);
        object InvokeAsync(Delegate method, params object[] args);
        void Block(bool block);
        Task RefreshFMsList(bool refreshReadme, bool suppressSelectionChangedEvent = false,
            bool suppressSuspendResume = false);
        Task RefreshSelectedFM(bool refreshReadme, bool refreshGridRowOnly = false);
        Task RefreshSelectedFMRowOnly();
        bool AskToContinue(string message, string title, bool noIcon = false);

        (bool Cancel, bool DontAskAgain)
            AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon,
                bool showDontAskAgain, string yes, string no);

        (bool Cancel, bool Continue, bool DontAskAgain)
            AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon,
                bool showDontAskAgain, string yes, string no, string cancel);

        void ChangeReadmeBoxFont(bool useFixed);
    }
}
