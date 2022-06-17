using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.LanguageSupport;
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
        bool EventsDisabled { set; }
    }

    public interface IZeroSelectCodeDisabler
    {
        bool ZeroSelectCodeDisabled { set; }
    }

    internal sealed class DisableEvents : IDisposable
    {
        private readonly IEventDisabler Obj;
        internal DisableEvents(IEventDisabler obj)
        {
            Obj = obj;
            Obj.EventsDisabled = true;
        }

        public void Dispose() => Obj.EventsDisabled = false;
    }

    internal sealed class DisableZeroSelectCode : IDisposable
    {
        private readonly IZeroSelectCodeDisabler Obj;
        internal DisableZeroSelectCode(IZeroSelectCodeDisabler obj)
        {
            Obj = obj;
            Obj.ZeroSelectCodeDisabled = true;
        }

        public void Dispose() => Obj.ZeroSelectCodeDisabled = false;
    }

    #endregion

    #region DisableKeyPresses

    public interface IKeyPressDisabler
    {
        bool KeyPressesDisabled { set; }
    }

    internal sealed class DisableKeyPresses : IDisposable
    {
        private readonly IKeyPressDisabler Obj;

        internal DisableKeyPresses(IKeyPressDisabler obj)
        {
            Obj = obj;
            Obj.KeyPressesDisabled = true;
        }

        public void Dispose() => Obj.KeyPressesDisabled = false;
    }

    #endregion

    public interface ISettingsChangeableWindow
    {
        void Localize();
        void SetTheme(VisualTheme theme);
        void SetWaitCursor(bool value);
    }

    [PublicAPI]
    public interface IDarkable
    {
        bool DarkModeEnabled { get; set; }
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
        int BackingIndexOf(string item);
        string SelectedBackingItem();
        void SelectBackingIndexOf(string item);
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
        (bool Accepted, ConfigData OutConfig) ShowSettingsWindow(ISettingsChangeableWindow? view, ConfigData inConfig, bool startup, bool cleanStart);
    }

    public interface IDialogs
    {
        (MBoxButton ButtonPressed, bool CheckBoxChecked) ShowMultiChoiceDialog(string message, string title, MBoxIcon icon, string? yes, string? no, string? cancel = null, bool yesIsDangerous = false, string? checkBoxText = null, MBoxButton defaultButton = MBoxButton.Yes);
        void ShowError_ViewOwned(string message);
        void ShowError(string message);
        void ShowAlert(string message, string title, MBoxIcon icon = MBoxIcon.Warning);
        void ShowAlert_Stock(string message, string title, MBoxButtons buttons, MBoxIcon icon);
    }

    public interface IView : ISettingsChangeableWindow, IEventDisabler, IKeyPressDisabler, IZeroSelectCodeDisabler
    {
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
        Task FinishInitAndShow(List<int> fmsViewListUnscanned, ISplashScreen_Safe splashScreen);

        void ShowOnly();

        #endregion

        #region Show or hide UI elements

        void ShowFMsListZoomButtons(bool visible);

        void ShowInstallUninstallButton(bool enabled);

        void ShowExitButton(bool enabled);

        #endregion

        #region Filter

        Task SortAndSetFilter(SelectedFM? selectedFM = null, bool forceDisplayFM = false,
                              bool keepSelection = false, bool gameTabSwitch = false,
                              bool landImmediate = false, bool keepMultiSelection = false);

        Filter GetFilter();

        string GetTitleFilter();

        string GetAuthorFilter();

        bool[] GetGameFiltersEnabledStates();

        bool GetFinishedFilter();

        bool GetUnfinishedFilter();

        bool GetShowUnsupportedFilter();

        bool GetShowUnavailableFMsFilter();

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

        #endregion

        void Block(bool block);

        void ChangeReadmeBoxFont(bool useFixed);

        void ChangeGameTabNameShortness(bool useShort, bool refreshFilterBarPositionIfNeeded);

        SelectedFM? GetMainSelectedFMPosInfo();

        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);

#if !ReleaseBeta && !ReleasePublic
        void UpdateGameScreenShotModes();
#endif

        FanMission? GetMainSelectedFMOrNull();
        FanMission[] GetSelectedFMs();
        bool MultipleFMsSelected();
        (string Category, string Tag) SelectedCategoryAndTag();
        void DisplayFMTags(FMCategoriesCollection fmTags);
        void ClearTagsSearchBox();
        void SetPinnedMenuState(bool pinned);
        int GetRowCount();
        int GetMainSelectedRowIndex();
        SelectedFM? GetFMPosInfoFromIndex(int index);
        bool RowSelected(int index);
        string GetFMCommentText();
        void ClearLanguagesList();
        void AddLanguagesToList(List<KeyValuePair<string, string>> langPairs);
        // @LANGS(backing items): We could allow the backing items to be enum values
        // That way we don't even have to store the lang strings in the combobox backing list
        /// <summary>
        /// Sets the selected item in the language list.
        /// </summary>
        /// <param name="language"></param>
        /// <returns>The selected language, or default if a match was not found.</returns>
        Language SetSelectedLanguage(Language language);
        Language GetMainSelectedLanguage();
        void SetPlayOriginalGameControlsState();
        void ClearReadmesList();
        void UpdateAllFMUIDataExceptReadme(FanMission fm);
        void ReadmeListFillAndSelect(List<string> readmeFiles, string readme);
        void ShowReadmeChooser(bool visible);
        void ShowInitialReadmeChooser(bool visible);
        void ActivateThisInstance();
        FanMission? GetFMFromIndex(int index);
        FanMission[] GetSelectedFMs_InOrder();
        List<FanMission> GetSelectedFMs_InOrder_List();

        #region Dialogs

        (bool Accepted, FMScanner.ScanOptions ScanOptions, bool NoneSelected) ShowScanAllFMsWindow();

        (bool Accepted, List<string> SelectedItems) ShowCustomDialog(string messageTop, string messageBottom, string title, MBoxIcon icon, string okText, string cancelText, bool okIsDangerous, string[]? choiceStrings = null, bool multiSelectionAllowed = true);

        (bool Accepted, string IniFile, bool ImportFMData, bool ImportTitle, bool ImportSize, bool ImportComment, bool ImportReleaseDate, bool ImportLastPlayed, bool ImportFinishedOn, bool ImportSaves) ShowDarkLoaderImportWindow();

        (bool Accepted, List<string> IniFiles, bool ImportTitle, bool ImportReleaseDate, bool ImportLastPlayed, bool ImportComment, bool ImportRating, bool ImportDisabledMods, bool ImportTags, bool ImportSelectedReadme, bool ImportFinishedOn, bool ImportSize) ShowImportFromMultipleInisForm(ImportType importType);

        #endregion

        bool AbleToAcceptDragDrop();
        FanMission? GetMainSelectedFMOrNull_Fast();
        bool GetUIEnabled();
        void SetUIEnabled(bool value);
    }
}
