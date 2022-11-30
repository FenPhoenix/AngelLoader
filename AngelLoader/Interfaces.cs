﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader
{
    #region DisableEvents

    /*
     Implement the interface on your form, and put guard clauses on all your event handlers that you want to
     be disableable:

     if (EventsDisabled) return;

     Then whenever you want to disable those event handlers, just make a using block:

     using (new DisableEvents(this))
     {
     }

     Inside this block, put any code that changes the state of the controls in such a way that would normally
     run their event handlers. The guard clauses will exit them before anything happens. Problem solved. And
     much better than a nasty wall of Control.Event1 -= Control_Event1; Control.Event1 += Control_Event1; etc.,
     and has the added bonus of guaranteeing a reset of the value due to the using block.
    */

    public interface IEventDisabler
    {
        bool EventsDisabled { get; }
        int EventsDisabledCount { get; set; }
    }

    internal sealed class DisableEvents_Reference : IDisposable
    {
        private readonly IEventDisabler Obj;
        internal DisableEvents_Reference(IEventDisabler obj)
        {
            Obj = obj;
            Obj.EventsDisabledCount++;
        }

        public void Dispose() => Obj.EventsDisabledCount = (Obj.EventsDisabledCount - 1).ClampToZero();
    }

    internal readonly ref struct DisableEvents
    {
        private readonly bool _active;
        private readonly IEventDisabler Obj;
        public DisableEvents(IEventDisabler obj, bool active = true)
        {
            _active = active;
            Obj = obj;

            if (_active) Obj.EventsDisabledCount++;
        }

        public void Dispose()
        {
            if (_active) Obj.EventsDisabledCount = (Obj.EventsDisabledCount - 1).ClampToZero();
        }
    }
    #endregion

    #region DisableZeroSelectCode

    public interface IZeroSelectCodeDisabler
    {
        bool ZeroSelectCodeDisabled { get; }
        int ZeroSelectCodeDisabledCount { get; set; }
    }

    internal readonly ref struct DisableZeroSelectCode
    {
        private readonly IZeroSelectCodeDisabler Obj;
        internal DisableZeroSelectCode(IZeroSelectCodeDisabler obj)
        {
            Obj = obj;
            Obj.ZeroSelectCodeDisabledCount++;
        }

        public void Dispose() => Obj.ZeroSelectCodeDisabledCount = (Obj.ZeroSelectCodeDisabledCount - 1).ClampToZero();
    }

    #endregion

    public interface IDarkContextMenuOwner
    {
        bool ViewBlocked { get; }
        IContainer GetComponents();
    }

    public interface ISettingsChangeableWindow
    {
        void Localize();
        void SetTheme(VisualTheme theme);
        void SetWaitCursor(bool value);
    }

    public interface IDarkable
    {
        bool DarkModeEnabled { set; }
    }

    public interface ISplashScreen
    {
        bool VisibleCached { get; }
        void ProgrammaticClose();
        void SetCheckAtStoredMessageWidth();
        void SetCheckMessageWidth(string message);
        void SetMessage(string message);
        void Show(VisualTheme theme);
        void Hide();
        void Dispose();
        void LockPainting(bool enabled);
    }

    // The splash screen is extremely un-thread-safe by design (because it's a UI we have to update while another
    // UI is loading), so we pass as this interface to guarantee that nobody can do anything dangerous with it.
    public interface ISplashScreen_Safe
    {
        void Hide();
    }

    public interface IListControlWithBackingItems
    {
        void AddFullItem(string backingItem, string item);
        void ClearFullItems();

        #region Disabled until needed

#if false

        int BackingIndexOf(string item);
        string SelectedBackingItem();
        void SelectBackingIndexOf(string item);

#endif

        #endregion

        void BeginUpdate();
        void EndUpdate();
    }

    public interface IViewEnvironment
    {
        string ProductVersion { get; }
        void ApplicationExit();
        IDialogs GetDialogs();
        ISplashScreen GetSplashScreen();
        IView GetView();
        void PreprocessRTFReadme(ConfigData config, List<FanMission> fmsViewList, List<FanMission> fmsViewListUnscanned);
        (bool Accepted, ConfigData OutConfig) ShowSettingsWindow(ISettingsChangeableWindow? view, ConfigData inConfig, bool startup, bool cleanStart);
    }

    public interface IDialogs
    {
        (MBoxButton ButtonPressed, bool CheckBoxChecked) ShowMultiChoiceDialog(string message, string title, MBoxIcon icon, string? yes, string? no, string? cancel = null, bool yesIsDangerous = false, string? checkBoxText = null, MBoxButton defaultButton = MBoxButton.Yes);
        void ShowError_ViewOwned(string message, string? title = null, MBoxIcon icon = MBoxIcon.Error);
        void ShowError(string message, string? title = null, MBoxIcon icon = MBoxIcon.Error);
        void ShowAlert(string message, string title, MBoxIcon icon = MBoxIcon.Warning);
        void ShowAlert_Stock(string message, string title, MBoxButtons buttons, MBoxIcon icon);
        (bool Accepted, List<string> SelectedItems) ShowListDialog(string messageTop, string messageBottom, string title, MBoxIcon icon, string okText, string cancelText, bool okIsDangerous, string[] choiceStrings, bool multiSelectionAllowed);
    }

    public interface IView : ISettingsChangeableWindow, IEventDisabler, IDarkContextMenuOwner, IZeroSelectCodeDisabler
    {
#if !ReleaseBeta && !ReleasePublic
        void UpdateGameScreenShotModes();
#endif

        #region Debug

#if DEBUG || (Release_Testing && !RT_StartupOnly)
        string GetDebug1Text();
        string GetDebug2Text();
        void SetDebug1Text(string value);
        void SetDebug2Text(string value);
#endif

        #endregion

        #region Progress box

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Null parameters mean explicitly set the defaults.
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        /// <param name="progressType"></param>
        /// <param name="cancelMessage"></param>
        /// <param name="cancelAction"></param>
        void ShowProgressBox_Single(string? message1 = null, string? message2 = null, ProgressType? progressType = null, string? cancelMessage = null, Action? cancelAction = null);

        #region Disabled until needed

#if false

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Null parameters mean explicitly set the defaults.
        /// </summary>
        /// <param name="mainMessage1"></param>
        /// <param name="mainMessage2"></param>
        /// <param name="mainProgressType"></param>
        /// <param name="subMessage"></param>
        /// <param name="subProgressType"></param>
        /// <param name="cancelMessage"></param>
        /// <param name="cancelAction"></param>
        void ShowProgressBox_Double(string? mainMessage1 = null, string? mainMessage2 = null, ProgressType? mainProgressType = null, string? subMessage = null, ProgressType? subProgressType = null, string? cancelMessage = null, Action? cancelAction = null);

#endif

        #endregion

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Null parameters mean no change.
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        /// <param name="percent"></param>
        /// <param name="progressType"></param>
        /// <param name="cancelMessage"></param>
        /// <param name="cancelAction"></param>
        void SetProgressBoxState_Single(bool? visible = null, string? message1 = null, string? message2 = null, int? percent = null, ProgressType? progressType = null, string? cancelMessage = null, Action? cancelAction = null);

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Null parameters mean no change.
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="mainMessage1"></param>
        /// <param name="mainMessage2"></param>
        /// <param name="mainPercent"></param>
        /// <param name="mainProgressType"></param>
        /// <param name="subMessage"></param>
        /// <param name="subPercent"></param>
        /// <param name="subProgressType"></param>
        /// <param name="cancelMessage"></param>
        /// <param name="cancelAction"></param>
        void SetProgressBoxState_Double(bool? visible = null, string? mainMessage1 = null, string? mainMessage2 = null, int? mainPercent = null, ProgressType? mainProgressType = null, string? subMessage = null, int? subPercent = null, ProgressType? subProgressType = null, string? cancelMessage = null, Action? cancelAction = null);

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Just sets the percent, leaving all other fields unchanged.
        /// </summary>
        /// <param name="percent"></param>
        void SetProgressPercent(int percent);

        /// <summary>
        /// Use this is you need to be more detailed than any of the tighter-scoped methods.
        /// <para/>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// <para/>
        /// Null parameters mean no change.
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="size"></param>
        /// <param name="mainMessage1"></param>
        /// <param name="mainMessage2"></param>
        /// <param name="mainPercent"></param>
        /// <param name="mainProgressType"></param>
        /// <param name="subMessage"></param>
        /// <param name="subPercent"></param>
        /// <param name="subProgressType"></param>
        /// <param name="cancelMessage"></param>
        /// <param name="cancelAction"></param>
        void SetProgressBoxState(bool? visible = null, ProgressSizeMode? size = null, string? mainMessage1 = null, string? mainMessage2 = null, int? mainPercent = null, ProgressType? mainProgressType = null, string? subMessage = null, int? subPercent = null, ProgressType? subProgressType = null, string? cancelMessage = null, Action? cancelAction = null);

        /// <summary>
        /// This method call is auto-invoked, so no need to wrap it manually.
        /// </summary>
        void HideProgressBox();

        #endregion

        #region Get column sort state

        Column GetCurrentSortedColumnIndex();

        SortDirection GetCurrentSortDirection();

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
        Task FinishInitAndShow(List<FanMission> fmsViewListUnscanned, ISplashScreen_Safe splashScreen);

        void Show();

        #endregion

        #region Show or hide UI elements

        void ShowFMsListZoomButtons(bool visible);

        void ShowInstallUninstallButton(bool enabled, bool startup = false);

        void ShowWebSearchButton(bool enabled, bool startup = false);

        void ShowExitButton(bool enabled, bool startup = false);

        #endregion

        #region Filter

        Task SortAndSetFilter(SelectedFM? selectedFM = null, bool forceDisplayFM = false,
                              bool keepSelection = false, bool gameTabSwitch = false,
                              bool landImmediate = false, bool keepMultiSelection = false);

        Filter GetFilter();

        string GetTitleFilter();

        string GetAuthorFilter();

        GameSupport.Game GetGameFiltersEnabled();

        bool GetFinishedFilter();

        bool GetUnfinishedFilter();

        bool GetShowUnsupportedFilter();

        bool GetShowUnavailableFMsFilter();

        List<int> GetFilterShownIndexList();

        bool GetShowRecentAtTop();

        void ClearUIAndCurrentInternalFilter();

        void ChangeGameOrganization(bool startup = false);

        #endregion

        #region Row count

        int GetRowCount();

        void SetRowCount(int count);

        #endregion

        #region Invoke

        object Invoke(Delegate method);

        //object Invoke(Delegate method, params object[] args);

        #endregion

        #region Refresh

        void RefreshFM(FanMission fm, bool rowOnly = false);

        void RefreshFMsListRowsOnlyKeepSelection();

        void RefreshAllSelectedFMs_Full();

        void RefreshMainSelectedFMRow_Fast();

        void RefreshAllSelectedFMs_UpdateInstallState();

        #endregion

        #region Readme

        void SetReadmeState(ReadmeState state, List<string>? readmeFilesForChooser = null);

        Encoding? LoadReadmeContent(string path, ReadmeType fileType, Encoding? encoding);

        void SetReadmeLocalizableMessage(ReadmeLocalizableMessage messageType);

        Encoding? ChangeReadmeEncoding(Encoding? encoding);

        void SetSelectedEncoding(Encoding encoding);

        void ChangeReadmeBoxFont(bool useFixed);

        void ClearReadmesList();

        void ReadmeListFillAndSelect(List<string> readmeFiles, string readme);

        void ShowReadmeChooser(bool visible);

        void ShowInitialReadmeChooser(bool visible);

        #endregion

        #region Get FM info

        FanMission[] GetSelectedFMs();

        List<FanMission> GetSelectedFMs_InOrder_List();

        SelectedFM? GetMainSelectedFMPosInfo();

        FanMission? GetMainSelectedFMOrNull();

        FanMission? GetMainSelectedFMOrNull_Fast();

        FanMission? GetFMFromIndex(int index);

        SelectedFM? GetFMPosInfoFromIndex(int index);

        bool MultipleFMsSelected();

        bool RowSelected(int index);

        int GetMainSelectedRowIndex();

        #endregion

        #region Dialogs

        (bool Accepted, FMScanner.ScanOptions ScanOptions, bool NoneSelected) ShowScanAllFMsWindow(bool selected);

        (bool Accepted, string IniFile, bool ImportFMData, bool ImportTitle, bool ImportSize, bool ImportComment, bool ImportReleaseDate, bool ImportLastPlayed, bool ImportFinishedOn, bool ImportSaves) ShowDarkLoaderImportWindow();

        (bool Accepted, List<string> IniFiles, bool ImportTitle, bool ImportReleaseDate, bool ImportLastPlayed, bool ImportComment, bool ImportRating, bool ImportDisabledMods, bool ImportTags, bool ImportSelectedReadme, bool ImportFinishedOn, bool ImportSize) ShowImportFromMultipleInisForm(ImportType importType);

        #endregion

        #region UI enabled

        bool GetUIEnabled();

        void SetUIEnabled(bool value);

        #endregion

        void Block(bool block);

        void ChangeGameTabNameShortness(bool useShort, bool refreshFilterBarPositionIfNeeded);

        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);

        void SetPinnedMenuState(bool pinned);

        void SetPlayOriginalGameControlsState();

        void UpdateAllFMUIDataExceptReadme(FanMission fm);

        void ActivateThisInstance();

        bool AbleToAcceptDragDrop();

        void SetAvailableFMCount();
    }
}
