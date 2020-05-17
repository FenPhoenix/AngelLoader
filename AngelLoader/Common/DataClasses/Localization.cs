#define FenGen_LocalizationSource

using static AngelLoader.Attributes;

namespace AngelLoader.DataClasses
{
    // NOTE:
    // Articles can't really be localized because they need to apply to an FM's title and most are in English.
    // So I'm keeping the custom articles functionality just the way it is, that way another language's articles
    // can be added to the list.

    // TODO: Missing localized bits:
    // -Hitches with localizability:
    //  -Date and rating forms are not set up for easy resizability of controls

    /*
    For generating instantiable localization string class, convert:

    internal static class LText_Static
    {
        internal static class Foo
        {
            internal static string TitleText = "Foo-matic 2000";
        }
    }

    to:

    internal sealed class LText_Instantiable
    {
        internal readonly Foo_Section Foo = new Foo_Section();

        internal sealed class Foo_Section
        {
            internal string TitleText = "Foo-matic 2000";
        }
    }

    Reader method must be passed (or must instantiate and return) a localized string class instance.
    Instead of reading into LText.Whatever.Whatever, it will read into lTextParam.Whatever.Whatever etc.

    AngelLoader does not desperately need this; the only thing it would really buy us is a quicker previous-lang
    reset in the Settings window if you click Cancel after having selected a different language.
    However, the hypothetical visual localizer tool would probably want instantiable language objects. We can
    generate that from this and not even have to touch anything here.
    */

    [FenGenLocalizationClass]
    internal static class LText
    {
        // Notes:
        // -Attributes only work when applied to fields, not properties. I think I know why, but whatever, this
        //  is good enough for now.
        // -Comments don't support concatenation (try to fix later)
        // -Strings must be inside sub-classes or they won't be picked up (that's the nature of ini files)

        internal static class Meta
        {
            [FenGenComment(
@"This should be the name of this file's language in this file's language.
Example: English should be English, French should be Français, etc.")]
            internal static string TranslatedLanguageName = "English";
        }

        internal static class Global
        {
            internal static string OK = "OK";
            internal static string Cancel = "Cancel";
            internal static string BrowseEllipses = "Browse...";
            internal static string Add = "Add";
            internal static string AddEllipses = "Add...";
            internal static string Remove = "Remove";
            internal static string RemoveEllipses = "Remove...";
            internal static string Reset = "Reset";
            internal static string Autodetect = "Autodetect";
            internal static string SelectAll = "Select all";
            internal static string SelectNone = "Select none";
            [FenGenBlankLine]
            internal static string Unrated = "Unrated";
            internal static string None = "None";
            internal static string CustomTagInCategory = "<custom>";
            [FenGenBlankLine]
            internal static string KilobyteShort = "KB";
            internal static string MegabyteShort = "MB";
            internal static string GigabyteShort = "GB";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Global): Begin
            internal static string Thief1 = "Thief 1";
            internal static string Thief2 = "Thief 2";
            internal static string Thief3 = "Thief 3";
            internal static string SystemShock2 = "System Shock 2";
            [FenGenBlankLine]
            internal static string Thief1_Short = "T1";
            internal static string Thief2_Short = "T2";
            internal static string Thief3_Short = "T3";
            internal static string SystemShock2_Short = "SS2";
            [FenGenBlankLine]
            internal static string Thief1_Colon = "Thief 1:";
            internal static string Thief2_Colon = "Thief 2:";
            internal static string Thief3_Colon = "Thief 3:";
            internal static string SystemShock2_Colon = "System Shock 2:";
            // @GENGAMES (Localization - Global): End
        }

        internal static class BrowseDialogs
        {
            internal static string AllFiles = "All files (*.*)";
            internal static string ExeFiles = "Executable files (*.exe)";
            internal static string IniFiles = "ini files (*.ini)";
            internal static string DMLFiles = "NewDark .dml patch files (*.dml)";
        }

        internal static class AlertMessages
        {
            internal static string Alert = "Alert";
            internal static string Warning = "Warning";
            internal static string Error = "Error";
            internal static string Confirm = "Confirm";
            internal static string Uninstall = "Uninstall";
            internal static string BackUp = "Back up";
            internal static string DontBackUp = "Don't back up";
            internal static string DeleteFMArchive = "Delete FM archive";
            [FenGenBlankLine]
            internal static string DontAskAgain = "Don't ask again";
            [FenGenBlankLine]
            internal static string AppClosing_OperationInProgress = "An operation is in progress. Please cancel or wait for it to finish.";
            [FenGenBlankLine]
            internal static string WebSearchURL_ProblemOpening = "There was a problem opening the specified web search URL.";
            [FenGenBlankLine]
            internal static string Install_UnknownGameType = "This FM's game type is unknown, so it can't be installed.";
            internal static string Install_UnsupportedGameType = "This FM's game type is unsupported, so it can't be installed.";
            internal static string Install_ArchiveNotFound = "FM archive not found. Unable to install.";
            internal static string Install_ExecutableNotFound = "Executable file not specified or not found. Unable to install.";
            internal static string Install_FMInstallPathNotFound = "FM install path not specified or not found. Unable to install.";
            internal static string Install_GameIsRunning = "Game is running; unable to install. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal static string Uninstall_Confirm = "Are you sure you want to uninstall this FM?";
            internal static string Uninstall_GameIsRunning = "Game is running; unable to uninstall. Please exit the game and then try again.";
            internal static string Uninstall_FMAlreadyUninstalled = "This FM has already been uninstalled or its folder cannot be found. Mark it as uninstalled?";
            internal static string Uninstall_ArchiveNotFound = "This FM's archive file was not found! If you continue with uninstalling this FM, you won't be able to re-install it. Saves and screenshots will be backed up, but any other data will not. Are you sure you want to uninstall this FM?";
            internal static string Uninstall_UninstallNotCompleted = "The uninstall could not be completed. The FM will be marked as uninstalled but its folder may be in an unknown state.";
            internal static string Uninstall_BackupSavesAndScreenshots = "Back up saves and screenshots?";
            internal static string Uninstall_BackupAllData = "Back up all modified/added/removed files (including saves and screenshots)?";
            internal static string Uninstall_BackupChooseNoNote = "If you choose \"Don't back up\", then existing backups will remain, but they will not be updated.";
            internal static string Uninstall_FailedFullyOrPartially = "Uninstall failed fully or partially.";
            [FenGenBlankLine]
            internal static string FileConversion_GameIsRunning = "Game is running; unable to convert files. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal static string Play_ExecutableNotFound = "Executable file not specified or not found. Unable to play.";
            internal static string Play_GamePathNotFound = "Game path not found. Unable to play.";
            internal static string Play_ExecutableNotFoundFM = "Executable file not specified or not found. Unable to play FM.";
            internal static string Play_AnyGameIsRunning = "One or more supported games are already running. Please exit them first.";
            internal static string Play_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to play.";
            internal static string Play_ConfirmMessage = "Play FM?";
            [FenGenBlankLine]
            internal static string DromEd_ExecutableNotFound = "DromEd.exe was not found in the game directory. Unable to open FM.";
            internal static string ShockEd_ExecutableNotFound = "ShockEd.exe was not found in the game directory. Unable to open FM.";
            internal static string DromEd_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to open FM.";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Alerts - Dark multiplayer): Begin
            internal static string Thief2_Multiplayer_ExecutableNotFound = "Thief2MP.exe was not found in the game directory. Unable to play FM in multiplayer mode.";
            // @GENGAMES (Localization - Alerts - Dark multiplayer): End
            [FenGenBlankLine]
            internal static string Patch_AddDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to add patch.";
            internal static string Patch_AddDML_UnableToAdd = "Unable to add patch to fan mission folder.";
            internal static string Patch_RemoveDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to remove patch.";
            internal static string Patch_RemoveDML_UnableToRemove = "Unable to remove patch from fan mission folder.";
            internal static string Patch_FMFolderNotFound = "The FM's folder couldn't be found.";
            [FenGenBlankLine]
            internal static string Misc_SneakyOptionsIniNotFound = "A Thief: Deadly Shadows install exists, but SneakyOptions.ini couldn't be found. Make sure your Thief: Deadly Shadows install has been patched with the Sneaky Upgrade 1.1.9.1 or later.";
            internal static string Misc_FMMarkedInstalledButNotInstalled = "This FM is marked as installed, but its folder cannot be found. Mark it as uninstalled?";
            [FenGenBlankLine]
            internal static string Extract_ZipExtractFailedFullyOrPartially = "Zip extract failed fully or partially.";
            internal static string Extract_SevenZipExtractFailedFullyOrPartially = "7-zip extract failed fully or partially.";
            [FenGenBlankLine]
            internal static string Scan_ExceptionInScanOne = "There was a problem scanning the FM. See the log file for error details.";
            internal static string Scan_ExceptionInScanMultiple = "There was a problem scanning the FMs. See the log file for error details.";
            [FenGenBlankLine]
            internal static string FindFMs_ExceptionReadingFMDataIni = "There was a problem reading the FM data ini file. See the log file for error details.";
            [FenGenBlankLine]
            internal static string DeleteFM_UnableToDelete = "The following FM archive could not be deleted:";
        }

        internal static class FMDeletion
        {
            internal static string ArchiveNotFound = "This FM's archive could not be found. To delete this FM permanently, simply uninstall it.";
            internal static string AboutToDelete = "The following FM archive is about to be deleted from disk:";
            internal static string DuplicateArchivesFound = "Multiple archives with the same name were found. Please choose which archives(s) you want to delete.";
            internal static string DeleteFM = "Delete FM";
            internal static string DeleteFMs = "Delete FM(s)";
        }

        internal static class Difficulties
        {
            internal static string Easy = "Easy";
            internal static string Normal = "Normal";
            internal static string Hard = "Hard";
            internal static string Expert = "Expert";
            internal static string Extreme = "Extreme";
            internal static string Impossible = "Impossible";
            internal static string Unknown = "Unknown";
        }

        internal static class FilterBar
        {
            internal static string Title = "Title:";
            internal static string Author = "Author:";
            [FenGenBlankLine]
            internal static string ReleaseDateToolTip = "Release date";
            internal static string LastPlayedToolTip = "Last played";
            internal static string TagsToolTip = "Tags";
            internal static string FinishedToolTip = "Finished";
            internal static string UnfinishedToolTip = "Unfinished";
            internal static string RatingToolTip = "Rating";
            [FenGenBlankLine]
            internal static string ShowUnsupported = "Show FMs marked as \"unsupported game or non-FM archive\"";
            internal static string ShowRecentAtTop = "Show recently added FMs at the top of the list";
            [FenGenBlankLine]
            internal static string RefreshFromDiskButtonToolTip = "Refresh from disk";
            internal static string RefreshFilteredListButtonToolTip = "Refresh filtered list";
            internal static string ClearFiltersButtonToolTip = "Clear filters";
            internal static string ResetLayoutButtonToolTip = "Reset layout";
        }

        internal static class FMsList
        {
            internal static string ZoomInToolTip = "Zoom in (Ctrl++)";
            internal static string ZoomOutToolTip = "Zoom out (Ctrl+-)";
            internal static string ResetZoomToolTip = "Reset zoom (Ctrl-0)";
            [FenGenBlankLine]
            internal static string GameColumn = "Game";
            internal static string InstalledColumn = "Installed";
            internal static string TitleColumn = "Title";
            internal static string ArchiveColumn = "Archive";
            internal static string AuthorColumn = "Author";
            internal static string SizeColumn = "Size";
            internal static string RatingColumn = "Rating";
            internal static string FinishedColumn = "Finished";
            internal static string ReleaseDateColumn = "Release Date";
            internal static string LastPlayedColumn = "Last Played";
            [FenGenComment("The date an FM was added to the list. Basically means the date you downloaded it and put it into your archives folder.")]
            internal static string DateAddedColumn = "Date Added";
            internal static string DisabledModsColumn = "Disabled Mods";
            internal static string CommentColumn = "Comment";
            [FenGenBlankLine]
            internal static string AllModsDisabledMessage = "* [All]";
            [FenGenBlankLine]
            internal static string ColumnMenu_ResetAllColumnsToVisible = "Reset all columns to visible";
            internal static string ColumnMenu_ResetAllColumnWidths = "Reset all column widths";
            internal static string ColumnMenu_ResetAllColumnPositions = "Reset all column positions";
            [FenGenBlankLine]
            internal static string FMMenu_PlayFM = "Play FM";
            internal static string FMMenu_PlayFM_Multiplayer = "Play FM (multiplayer)";
            internal static string FMMenu_InstallFM = "Install FM";
            internal static string FMMenu_UninstallFM = "Uninstall FM";
            internal static string FMMenu_DeleteFM = "Delete FM archive";
            internal static string FMMenu_OpenInDromEd = "Open FM in DromEd";
            internal static string FMMenu_OpenInShockEd = "Open FM in ShockEd";
            internal static string FMMenu_Rating = "Rating";
            internal static string FMMenu_FinishedOn = "Finished on";
            internal static string FMMenu_ConvertAudio = "Convert audio";
            internal static string FMMenu_ScanFM = "Scan FM";
            internal static string FMMenu_WebSearch = "Web search";
            [FenGenBlankLine]
            internal static string ConvertAudioMenu_ConvertWAVsTo16Bit = "Convert .wav files to 16 bit";
            internal static string ConvertAudioMenu_ConvertOGGsToWAVs = "Convert .ogg files to .wav";
        }

        internal static class StatisticsTab
        {
            internal static string TabText = "Statistics";
            [FenGenBlankLine]
            internal static string CustomResources = "Custom resources:";
            internal static string CustomResourcesNotScanned = "Custom resources not scanned.";
            // @GENGAMES (Localization - Custom resource detection not supported): Begin
            internal static string CustomResourcesNotSupportedForThief3 = "Custom resource detection is not supported for Thief 3 FMs.";
            // @GENGAMES (Localization - Custom resource detection not supported): End
            internal static string NoFMSelected = "No FM selected.";
            [FenGenBlankLine]
            internal static string Map = "Map";
            internal static string Automap = "Automap";
            internal static string Textures = "Textures";
            internal static string Sounds = "Sounds";
            internal static string Movies = "Movies";
            internal static string Objects = "Objects";
            internal static string Creatures = "Creatures";
            internal static string Motions = "Motions";
            internal static string Scripts = "Scripts";
            internal static string Subtitles = "Subtitles";
            [FenGenBlankLine]
            internal static string RescanCustomResources = "Rescan custom resources";
        }

        internal static class EditFMTab
        {
            internal static string TabText = "Edit FM";
            [FenGenBlankLine]
            internal static string Title = "Title:";
            internal static string Author = "Author:";
            internal static string ReleaseDate = "Release date:";
            internal static string LastPlayed = "Last played:";
            internal static string Rating = "Rating:";
            internal static string FinishedOn = "Finished on...";
            internal static string DisabledMods = "Disabled mods:";
            internal static string DisableAllMods = "Disable all mods";
            internal static string PlayFMInThisLanguage = "Play FM in this language:";
            internal static string DefaultLanguage = "Default";
            [FenGenBlankLine]
            internal static string RescanTitleToolTip = "Rescan title";
            internal static string RescanAuthorToolTip = "Rescan author";
            internal static string RescanReleaseDateToolTip = "Rescan release date";
            internal static string RescanLanguages = "Rescan for supported languages";
            internal static string RescanForReadmes = "Rescan for readmes";
        }

        internal static class CommentTab
        {
            internal static string TabText = "Comment";
        }

        internal static class TagsTab
        {
            internal static string TabText = "Tags";
            [FenGenBlankLine]
            internal static string AddTag = "Add tag";
            internal static string AddFromList = "Add from list...";
            internal static string RemoveTag = "Remove tag";
            [FenGenBlankLine]
            internal static string AskRemoveCategory = "Remove category?";
            internal static string AskRemoveTag = "Remove tag?";
        }

        internal static class PatchTab
        {
            internal static string TabText = "Patch & Customize";
            [FenGenBlankLine]
            internal static string DMLPatchesApplied = ".dml patches applied to this FM:";
            internal static string AddDMLPatchToolTip = "Add a new .dml patch to this FM";
            internal static string RemoveDMLPatchToolTip = "Remove selected .dml patch from this FM";
            internal static string FMNotInstalled = "FM must be installed in order to use this section.";
            internal static string OpenFMFolder = "Open FM folder";
        }

        internal static class ReadmeArea
        {
            internal static string ViewHTMLReadme = "View HTML Readme";
            internal static string ZoomInToolTip = "Zoom in (Ctrl++)";
            internal static string ZoomOutToolTip = "Zoom out (Ctrl+-)";
            internal static string ResetZoomToolTip = "Reset zoom (Ctrl+0)";
            internal static string FullScreenToolTip = "Fullscreen";
            [FenGenBlankLine]
            internal static string NoReadmeFound = "No readme found.";
            internal static string UnableToLoadReadme = "Unable to load this readme.";
        }

        internal static class PlayOriginalGameMenu
        {
            internal static string Thief2_Multiplayer = "Thief 2 (multiplayer)";
        }

        internal static class MainButtons
        {
            internal static string PlayFM = "Play FM";
            internal static string InstallFM = "Install FM";
            internal static string UninstallFM = "Uninstall FM";
            internal static string PlayOriginalGame = "Play original game...";
            internal static string WebSearch = "Web search";
            internal static string ScanAllFMs = "Scan all FMs...";
            internal static string Import = "Import from...";
            internal static string Settings = "Settings...";
        }

        internal static class ProgressBox
        {
            internal static string Scanning = "Scanning...";
            internal static string InstallingFM = "Installing FM...";
            internal static string UninstallingFM = "Uninstalling FM...";
            internal static string ConvertingFiles = "Converting files...";
            internal static string CheckingInstalledFMs = "Checking installed FMs...";
            internal static string ReportScanningFirst = "Scanning ";
            internal static string ReportScanningBetweenNumAndTotal = " of ";
            internal static string ReportScanningLast = "...";
            internal static string CancelingInstall = "Canceling install...";
            internal static string ImportingFromDarkLoader = "Importing from DarkLoader...";
            internal static string ImportingFromNewDarkLoader = "Importing from NewDarkLoader...";
            internal static string ImportingFromFMSel = "Importing from FMSel...";
            internal static string CachingReadmeFiles = "Caching readme files...";
            internal static string DeletingFMArchive = "Deleting FM archive...";
        }

        internal static class SettingsWindow
        {
            internal static string TitleText = "Settings";
            internal static string StartupTitleText = "AngelLoader Initial Setup";
            [FenGenBlankLine]
            internal static string Paths_TabText = "Paths";
            internal static string InitialSettings_TabText = "Initial Settings";
            [FenGenBlankLine]
            internal static string Paths_PathsToGameExes = "Paths to game executables";
            // @GENGAMES (Localization - SettingsWindow - exe paths): Begin
            internal static string Paths_DarkEngineGamesRequireNewDark = "* Thief 1, Thief 2 and System Shock 2 require NewDark.";
            internal static string Paths_Thief3RequiresSneakyUpgrade = "* Thief 3 requires the Sneaky Upgrade 1.1.9.1 or above.";
            // @GENGAMES (Localization - SettingsWindow - exe paths): End
            [FenGenBlankLine]
            internal static string Paths_SteamOptions = "Steam options";
            internal static string Paths_PathToSteamExecutable = "Path to Steam executable (optional):";
            internal static string Paths_LaunchTheseGamesThroughSteam = "If Steam exists, use it to launch these games:";
            [FenGenBlankLine]
            internal static string Paths_Other = "Other";
            internal static string Paths_BackupPath = "FM backup path:";
            internal static string Paths_FMArchivePaths = "FM archive paths";
            internal static string Paths_IncludeSubfolders = "Include subfolders";
            [FenGenBlankLine]
            internal static string Paths_AddArchivePathToolTip = "Add archive path...";
            internal static string Paths_RemoveArchivePathToolTip = "Remove selected archive path";
            [FenGenBlankLine]
            internal static string Paths_ErrorSomePathsAreInvalid = "Some paths are invalid.";
            [FenGenBlankLine]
            internal static string FMDisplay_TabText = "FM Display";
            [FenGenBlankLine]
            internal static string FMDisplay_GameOrganization = "Game organization";
            internal static string FMDisplay_GameOrganizationByTab = "Each game in its own tab";
            internal static string FMDisplay_UseShortGameTabNames = "Use short names on game tabs";
            internal static string FMDisplay_GameOrganizationOneList = "Everything in one list, and games are filters";
            [FenGenBlankLine]
            internal static string FMDisplay_Sorting = "Sorting";
            [FenGenBlankLine]
            internal static string FMDisplay_IgnoreArticles = "Ignore the following leading articles when sorting by title:";
            [FenGenBlankLine]
            internal static string FMDisplay_MoveArticlesToEnd = "Move articles to the end of names when displaying them";
            [FenGenBlankLine]
            internal static string FMDisplay_RatingDisplayStyle = "Rating display style";
            internal static string FMDisplay_RatingDisplayStyleNDL = "NewDarkLoader (0-10 in increments of 1)";
            internal static string FMDisplay_RatingDisplayStyleFMSel = "FMSel (0-5 in increments of 0.5)";
            internal static string FMDisplay_RatingDisplayStyleUseStars = "Use stars";
            [FenGenBlankLine]
            internal static string FMDisplay_DateFormat = "Date format";
            internal static string FMDisplay_CurrentCultureShort = "System locale, short";
            internal static string FMDisplay_CurrentCultureLong = "System locale, long";
            internal static string FMDisplay_Custom = "Custom:";
            [FenGenBlankLine]
            internal static string FMDisplay_ErrorInvalidDateFormat = "Invalid date format.";
            internal static string FMDisplay_ErrorDateOutOfRange = "The date and time is outside the range of dates supported by the calendar used by the system locale.";
            [FenGenBlankLine]
            internal static string FMDisplay_RecentFMs = "Recent FMs";
            internal static string FMDisplay_RecentFMs_MaxDays = "Maximum number of days to consider an FM \"recent\":";
            [FenGenBlankLine]
            internal static string Other_TabText = "Other";
            [FenGenBlankLine]
            internal static string Other_FMFileConversion = "FM file conversion";
            internal static string Other_ConvertWAVsTo16BitOnInstall = "Convert .wavs to 16 bit on install";
            internal static string Other_ConvertOGGsToWAVsOnInstall = "Convert .oggs to .wavs on install";
            [FenGenBlankLine]
            internal static string Other_UninstallingFMs = "Uninstalling FMs";
            internal static string Other_ConfirmBeforeUninstalling = "Confirm before uninstalling";
            internal static string Other_WhenUninstallingBackUp = "When uninstalling, back up:";
            internal static string Other_BackUpSavesAndScreenshotsOnly = "Saves and screenshots only";
            internal static string Other_BackUpAllChangedFiles = "All changed files";
            internal static string Other_BackUpAlwaysAsk = "Always ask";
            [FenGenBlankLine]
            internal static string Other_Language = "Language";
            [FenGenBlankLine]
            internal static string Other_WebSearch = "Web search";
            internal static string Other_WebSearchURL = "Full URL to use when searching for an FM title:";
            internal static string Other_WebSearchTitleVar = "$TITLE$ : the title of the FM";
            internal static string Other_WebSearchResetToolTip = "Reset to default";
            [FenGenBlankLine]
            internal static string Other_ConfirmPlayOnDCOrEnter = "Play FM on double-click / Enter";
            internal static string Other_ConfirmPlayOnDCOrEnter_Ask = "Ask for confirmation";
            [FenGenBlankLine]
            internal static string Other_ShowOrHideInterfaceElements = "Show or hide interface elements";
            internal static string Other_HideUninstallButton = "Hide \"Install / Uninstall FM\" button (like FMSel)";
            internal static string Other_HideFMListZoomButtons = "Hide FM list zoom buttons";
            [FenGenBlankLine]
            internal static string Other_ReadmeBox = "Readme box";
            internal static string Other_ReadmeUseFixedWidthFont = "Use a fixed-width font when displaying plain text";
        }

        internal static class DateFilterBox
        {
            internal static string ReleaseDateTitleText = "Set release date filter";
            internal static string LastPlayedTitleText = "Set last played filter";
            [FenGenBlankLine]
            internal static string From = "From:";
            internal static string To = "To:";
            internal static string NoMinimum = "(no minimum)";
            internal static string NoMaximum = "(no maximum)";
        }

        internal static class TagsFilterBox
        {
            internal static string TitleText = "Set tags filter";
            [FenGenBlankLine]
            internal static string MoveToAll = "All";
            internal static string MoveToAny = "Any";
            internal static string MoveToExclude = "Exclude";
            internal static string Reset = "Reset";
            internal static string IncludeAll = "Include All:";
            internal static string IncludeAny = "Include Any:";
            internal static string Exclude = "Exclude:";
            internal static string ClearSelectedToolTip = "Clear selected";
            internal static string ClearAllToolTip = "Clear all";
        }

        internal static class RatingFilterBox
        {
            internal static string TitleText = "Set rating filter";
            [FenGenBlankLine]
            internal static string From = "From:";
            internal static string To = "To:";
        }

        internal static class Importing
        {
            internal static string NothingWasImported = "Nothing was imported.";
            internal static string SelectedFileIsNotAValidPath = "Selected file is not a valid path.";
            [FenGenBlankLine]
            internal static string ImportFromDarkLoader_TitleText = "Import from DarkLoader";
            internal static string DarkLoader_ChooseIni = "Choose DarkLoader.ini:";
            internal static string DarkLoader_ImportFMData = "Import FM data";
            internal static string DarkLoader_ImportSaves = "Import saves";
            internal static string DarkLoader_SelectedFileIsNotDarkLoaderIni = "Selected file is not DarkLoader.ini.";
            internal static string DarkLoader_SelectedDarkLoaderIniWasNotFound = "Selected DarkLoader.ini was not found.";
            internal static string DarkLoader_NoArchiveDirsFound = "No archive directories were specified in DarkLoader.ini. Unable to import.";
            [FenGenBlankLine]
            internal static string ImportFromNewDarkLoader_TitleText = "Import from NewDarkLoader";
            internal static string ImportFromFMSel_TitleText = "Import from FMSel";
            internal static string ChooseNewDarkLoaderIniFiles = "Choose NewDarkLoader .ini file(s):";
            internal static string ChooseFMSelIniFiles = "Choose FMSel .ini file(s):";
            [FenGenBlankLine]
            internal static string ImportData_Title = "Title";
            internal static string ImportData_ReleaseDate = "Release date";
            internal static string ImportData_LastPlayed = "Last played";
            internal static string ImportData_Finished = "Finished";
            internal static string ImportData_Comment = "Comment";
            internal static string ImportData_Rating = "Rating";
            internal static string ImportData_DisabledMods = "Disabled mods";
            internal static string ImportData_Tags = "Tags";
            internal static string ImportData_SelectedReadme = "Selected readme";
            internal static string ImportData_Size = "Size";
        }

        internal static class ScanAllFMsBox
        {
            internal static string TitleText = "Scan all FMs";
            [FenGenBlankLine]
            internal static string ScanAllFMsFor = "Scan all FMs for:";
            [FenGenBlankLine]
            internal static string Title = "Title";
            internal static string Author = "Author";
            internal static string Game = "Game";
            internal static string CustomResources = "Custom resources";
            internal static string Size = "Size";
            internal static string ReleaseDate = "Release date";
            internal static string Tags = "Tags";
            [FenGenBlankLine]
            internal static string Scan = "Scan";
            [FenGenBlankLine]
            internal static string NothingWasScanned = "No options were selected; no FMs have been scanned.";
        }
    }
}
