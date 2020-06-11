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

    [FenGenLocalizationSourceClass]
    internal sealed class LText_Class
    {
        // Notes:
        // -Comments don't support concatenation (try to fix later)
        // -Strings must be inside the nested classes or they won't be picked up (that's the nature of ini files)

        internal readonly Meta_Class Meta = new Meta_Class();
        internal readonly Global_Class Global = new Global_Class();
        internal readonly BrowseDialogs_Class BrowseDialogs = new BrowseDialogs_Class();
        internal readonly AlertMessages_Class AlertMessages = new AlertMessages_Class();
        internal readonly FMDeletion_Class FMDeletion = new FMDeletion_Class();
        internal readonly Difficulties_Class Difficulties = new Difficulties_Class();
        internal readonly FilterBar_Class FilterBar = new FilterBar_Class();
        internal readonly FMsList_Class FMsList = new FMsList_Class();
        internal readonly StatisticsTab_Class StatisticsTab = new StatisticsTab_Class();
        internal readonly EditFMTab_Class EditFMTab = new EditFMTab_Class();
        internal readonly CommentTab_Class CommentTab = new CommentTab_Class();
        internal readonly TagsTab_Class TagsTab = new TagsTab_Class();
        internal readonly PatchTab_Class PatchTab = new PatchTab_Class();
        internal readonly ReadmeArea_Class ReadmeArea = new ReadmeArea_Class();
        internal readonly PlayOriginalGameMenu_Class PlayOriginalGameMenu = new PlayOriginalGameMenu_Class();
        internal readonly MainButtons_Class MainButtons = new MainButtons_Class();
        internal readonly ProgressBox_Class ProgressBox = new ProgressBox_Class();
        internal readonly SettingsWindow_Class SettingsWindow = new SettingsWindow_Class();
        internal readonly DateFilterBox_Class DateFilterBox = new DateFilterBox_Class();
        internal readonly TagsFilterBox_Class TagsFilterBox = new TagsFilterBox_Class();
        internal readonly RatingFilterBox_Class RatingFilterBox = new RatingFilterBox_Class();
        internal readonly Importing_Class Importing = new Importing_Class();
        internal readonly ScanAllFMsBox_Class ScanAllFMsBox = new ScanAllFMsBox_Class();

        internal sealed class Meta_Class
        {
            [FenGenComment(
            "This should be the name of this file's language in this file's language.",
            "Example: English should be English, French should be Français, etc."
            )]
            internal string TranslatedLanguageName = "English";
        }

        internal sealed class Global_Class
        {
            internal string OK = "OK";
            internal string Cancel = "Cancel";
            internal string BrowseEllipses = "Browse...";
            internal string Add = "Add";
            internal string AddEllipses = "Add...";
            internal string Remove = "Remove";
            internal string RemoveEllipses = "Remove...";
            internal string Reset = "Reset";
            internal string Autodetect = "Autodetect";
            internal string SelectAll = "Select all";
            internal string SelectNone = "Select none";
            [FenGenBlankLine]
            internal string Unrated = "Unrated";
            internal string None = "None";
            internal string CustomTagInCategory = "<custom>";
            [FenGenBlankLine]
            internal string KilobyteShort = "KB";
            internal string MegabyteShort = "MB";
            internal string GigabyteShort = "GB";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Global): Begin
            internal string Thief1 = "Thief 1";
            internal string Thief2 = "Thief 2";
            internal string Thief3 = "Thief 3";
            internal string SystemShock2 = "System Shock 2";
            [FenGenBlankLine]
            internal string Thief1_Short = "T1";
            internal string Thief2_Short = "T2";
            internal string Thief3_Short = "T3";
            internal string SystemShock2_Short = "SS2";
            [FenGenBlankLine]
            internal string Thief1_Colon = "Thief 1:";
            internal string Thief2_Colon = "Thief 2:";
            internal string Thief3_Colon = "Thief 3:";
            internal string SystemShock2_Colon = "System Shock 2:";
            // @GENGAMES (Localization - Global): End
        }

        internal sealed class BrowseDialogs_Class
        {
            internal string AllFiles = "All files (*.*)";
            internal string ExeFiles = "Executable files (*.exe)";
            internal string IniFiles = "ini files (*.ini)";
            internal string DMLFiles = "NewDark .dml patch files (*.dml)";
        }

        internal sealed class AlertMessages_Class
        {
            internal string Alert = "Alert";
            internal string Warning = "Warning";
            internal string Error = "Error";
            internal string Confirm = "Confirm";
            internal string Uninstall = "Uninstall";
            internal string BackUp = "Back up";
            internal string DontBackUp = "Don't back up";
            internal string DeleteFMArchive = "Delete FM archive";
            [FenGenBlankLine]
            internal string DontAskAgain = "Don't ask again";
            [FenGenBlankLine]
            internal string AppClosing_OperationInProgress = "An operation is in progress. Please cancel or wait for it to finish.";
            [FenGenBlankLine]
            internal string WebSearchURL_ProblemOpening = "There was a problem opening the specified web search URL.";
            [FenGenBlankLine]
            internal string Install_UnknownGameType = "This FM's game type is unknown, so it can't be installed.";
            internal string Install_UnsupportedGameType = "This FM's game type is unsupported, so it can't be installed.";
            internal string Install_ArchiveNotFound = "FM archive not found. Unable to install.";
            internal string Install_ExecutableNotFound = "Executable file not specified or not found. Unable to install.";
            internal string Install_FMInstallPathNotFound = "FM install path not specified or not found. Unable to install.";
            internal string Install_GameIsRunning = "Game is running; unable to install. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal string Uninstall_Confirm = "Are you sure you want to uninstall this FM?";
            internal string Uninstall_GameIsRunning = "Game is running; unable to uninstall. Please exit the game and then try again.";
            internal string Uninstall_FMAlreadyUninstalled = "This FM has already been uninstalled or its folder cannot be found. Mark it as uninstalled?";
            internal string Uninstall_ArchiveNotFound = "This FM's archive file was not found! If you continue with uninstalling this FM, you won't be able to re-install it. Saves and screenshots will be backed up, but any other data will not. Are you sure you want to uninstall this FM?";
            internal string Uninstall_UninstallNotCompleted = "The uninstall could not be completed. The FM will be marked as uninstalled but its folder may be in an unknown state.";
            internal string Uninstall_BackupSavesAndScreenshots = "Back up saves and screenshots?";
            internal string Uninstall_BackupAllData = "Back up all modified/added/removed files (including saves and screenshots)?";
            internal string Uninstall_BackupChooseNoNote = "If you choose \"Don't back up\", then existing backups will remain, but they will not be updated.";
            internal string Uninstall_FailedFullyOrPartially = "Uninstall failed fully or partially.";
            [FenGenBlankLine]
            internal string FileConversion_GameIsRunning = "Game is running; unable to convert files. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal string Play_ExecutableNotFound = "Executable file not specified or not found. Unable to play.";
            internal string Play_GamePathNotFound = "Game path not found. Unable to play.";
            internal string Play_ExecutableNotFoundFM = "Executable file not specified or not found. Unable to play FM.";
            internal string Play_AnyGameIsRunning = "One or more supported games are already running. Please exit them first.";
            internal string Play_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to play.";
            internal string Play_ConfirmMessage = "Play FM?";
            [FenGenBlankLine]
            internal string DromEd_ExecutableNotFound = "DromEd.exe was not found in the game directory. Unable to open FM.";
            internal string ShockEd_ExecutableNotFound = "ShockEd.exe was not found in the game directory. Unable to open FM.";
            internal string DromEd_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to open FM.";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Alerts - Dark multiplayer): Begin
            internal string Thief2_Multiplayer_ExecutableNotFound = "Thief2MP.exe was not found in the game directory. Unable to play FM in multiplayer mode.";
            // @GENGAMES (Localization - Alerts - Dark multiplayer): End
            [FenGenBlankLine]
            internal string Patch_AddDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to add patch.";
            internal string Patch_AddDML_UnableToAdd = "Unable to add patch to fan mission folder.";
            internal string Patch_RemoveDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to remove patch.";
            internal string Patch_RemoveDML_UnableToRemove = "Unable to remove patch from fan mission folder.";
            internal string Patch_FMFolderNotFound = "The FM's folder couldn't be found.";
            [FenGenBlankLine]
            internal string Misc_SneakyOptionsIniNotFound = "A Thief: Deadly Shadows install exists, but SneakyOptions.ini couldn't be found. Make sure your Thief: Deadly Shadows install has been patched with the Sneaky Upgrade 1.1.9.1 or later.";
            internal string Misc_FMMarkedInstalledButNotInstalled = "This FM is marked as installed, but its folder cannot be found. Mark it as uninstalled?";
            [FenGenBlankLine]
            internal string Extract_ZipExtractFailedFullyOrPartially = "Zip extract failed fully or partially.";
            internal string Extract_SevenZipExtractFailedFullyOrPartially = "7-zip extract failed fully or partially.";
            [FenGenBlankLine]
            internal string Scan_ExceptionInScanOne = "There was a problem scanning the FM. See the log file for error details.";
            internal string Scan_ExceptionInScanMultiple = "There was a problem scanning the FMs. See the log file for error details.";
            [FenGenBlankLine]
            internal string FindFMs_ExceptionReadingFMDataIni = "There was a problem reading the FM data ini file. See the log file for error details.";
            [FenGenBlankLine]
            internal string DeleteFM_UnableToDelete = "The following FM archive could not be deleted:";
            [FenGenBlankLine]
            internal string Help_HelpFileNotFound = "Help file not found.";
            internal string Help_UnableToOpenHelpFile = "Unable to open help file.";
            //[FenGenBlankLine]
            //internal string Settings_Paths_BackupPathIsAnArchivePath = "Backup path is the same as one of the archive paths. Please set it to something different.";
        }

        internal sealed class FMDeletion_Class
        {
            internal string ArchiveNotFound = "This FM's archive could not be found. To delete this FM permanently, simply uninstall it.";
            internal string AboutToDelete = "The following FM archive is about to be deleted from disk:";
            internal string DuplicateArchivesFound = "Multiple archives with the same name were found. Please choose which archives(s) you want to delete.";
            internal string DeleteFM = "Delete FM";
            internal string DeleteFMs = "Delete FM(s)";
        }

        internal sealed class Difficulties_Class
        {
            internal string Easy = "Easy";
            internal string Normal = "Normal";
            internal string Hard = "Hard";
            internal string Expert = "Expert";
            internal string Extreme = "Extreme";
            internal string Impossible = "Impossible";
            internal string Unknown = "Unknown";
        }

        internal sealed class FilterBar_Class
        {
            internal string Title = "Title:";
            internal string Author = "Author:";
            [FenGenBlankLine]
            internal string ReleaseDateToolTip = "Release date";
            internal string LastPlayedToolTip = "Last played";
            internal string TagsToolTip = "Tags";
            internal string FinishedToolTip = "Finished";
            internal string UnfinishedToolTip = "Unfinished";
            internal string RatingToolTip = "Rating";
            [FenGenBlankLine]
            internal string ShowUnsupported = "Show FMs marked as \"unsupported game or non-FM archive\"";
            internal string ShowRecentAtTop = "Show recently added FMs at the top of the list";
            [FenGenBlankLine]
            internal string RefreshFromDiskButtonToolTip = "Refresh from disk";
            internal string RefreshFilteredListButtonToolTip = "Refresh filtered list";
            internal string ClearFiltersButtonToolTip = "Clear filters";
            internal string ResetLayoutButtonToolTip = "Reset layout";
        }

        internal sealed class FMsList_Class
        {
            internal string ZoomInToolTip = "Zoom in (Ctrl++)";
            internal string ZoomOutToolTip = "Zoom out (Ctrl+-)";
            internal string ResetZoomToolTip = "Reset zoom (Ctrl-0)";
            [FenGenBlankLine]
            internal string GameColumn = "Game";
            internal string InstalledColumn = "Installed";
            internal string TitleColumn = "Title";
            internal string ArchiveColumn = "Archive";
            internal string AuthorColumn = "Author";
            internal string SizeColumn = "Size";
            internal string RatingColumn = "Rating";
            internal string FinishedColumn = "Finished";
            internal string ReleaseDateColumn = "Release Date";
            internal string LastPlayedColumn = "Last Played";
            [FenGenComment("The date an FM was added to the list. Basically means the date you downloaded it and put it into your archives folder.")]
            internal string DateAddedColumn = "Date Added";
            internal string DisabledModsColumn = "Disabled Mods";
            internal string CommentColumn = "Comment";
            [FenGenBlankLine]
            internal string AllModsDisabledMessage = "* [All]";
            [FenGenBlankLine]
            internal string ColumnMenu_ResetAllColumnsToVisible = "Reset all columns to visible";
            internal string ColumnMenu_ResetAllColumnWidths = "Reset all column widths";
            internal string ColumnMenu_ResetAllColumnPositions = "Reset all column positions";
            [FenGenBlankLine]
            internal string FMMenu_PlayFM = "Play FM";
            internal string FMMenu_PlayFM_Multiplayer = "Play FM (multiplayer)";
            internal string FMMenu_InstallFM = "Install FM";
            internal string FMMenu_UninstallFM = "Uninstall FM";
            internal string FMMenu_DeleteFM = "Delete FM archive";
            internal string FMMenu_OpenInDromEd = "Open FM in DromEd";
            internal string FMMenu_OpenInShockEd = "Open FM in ShockEd";
            internal string FMMenu_Rating = "Rating";
            internal string FMMenu_FinishedOn = "Finished on";
            internal string FMMenu_ConvertAudio = "Convert audio";
            internal string FMMenu_ScanFM = "Scan FM";
            internal string FMMenu_WebSearch = "Web search";
            [FenGenBlankLine]
            internal string ConvertAudioMenu_ConvertWAVsTo16Bit = "Convert .wav files to 16 bit";
            internal string ConvertAudioMenu_ConvertOGGsToWAVs = "Convert .ogg files to .wav";
        }

        internal sealed class StatisticsTab_Class
        {
            internal string TabText = "Statistics";
            [FenGenBlankLine]
            internal string CustomResources = "Custom resources:";
            internal string CustomResourcesNotScanned = "Custom resources not scanned.";
            // @GENGAMES (Localization - Custom resource detection not supported): Begin
            internal string CustomResourcesNotSupportedForThief3 = "Custom resource detection is not supported for Thief 3 FMs.";
            // @GENGAMES (Localization - Custom resource detection not supported): End
            internal string NoFMSelected = "No FM selected.";
            [FenGenBlankLine]
            internal string Map = "Map";
            internal string Automap = "Automap";
            internal string Textures = "Textures";
            internal string Sounds = "Sounds";
            internal string Movies = "Movies";
            internal string Objects = "Objects";
            internal string Creatures = "Creatures";
            internal string Motions = "Motions";
            internal string Scripts = "Scripts";
            internal string Subtitles = "Subtitles";
            [FenGenBlankLine]
            internal string RescanCustomResources = "Rescan custom resources";
        }

        internal sealed class EditFMTab_Class
        {
            internal string TabText = "Edit FM";
            [FenGenBlankLine]
            internal string Title = "Title:";
            internal string Author = "Author:";
            internal string ReleaseDate = "Release date:";
            internal string LastPlayed = "Last played:";
            internal string Rating = "Rating:";
            internal string FinishedOn = "Finished on...";
            internal string DisabledMods = "Disabled mods:";
            internal string DisableAllMods = "Disable all mods";
            internal string PlayFMInThisLanguage = "Play FM in this language:";
            internal string DefaultLanguage = "Default";
            [FenGenBlankLine]
            internal string RescanTitleToolTip = "Rescan title";
            internal string RescanAuthorToolTip = "Rescan author";
            internal string RescanReleaseDateToolTip = "Rescan release date";
            internal string RescanLanguages = "Rescan for supported languages";
            internal string RescanForReadmes = "Rescan for readmes";
        }

        internal sealed class CommentTab_Class
        {
            internal string TabText = "Comment";
        }

        internal sealed class TagsTab_Class
        {
            internal string TabText = "Tags";
            [FenGenBlankLine]
            internal string AddTag = "Add tag";
            internal string AddFromList = "Add from list...";
            internal string RemoveTag = "Remove tag";
            [FenGenBlankLine]
            internal string AskRemoveCategory = "Remove category?";
            internal string AskRemoveTag = "Remove tag?";
        }

        internal sealed class PatchTab_Class
        {
            internal string TabText = "Patch & Customize";
            [FenGenBlankLine]
            internal string DMLPatchesApplied = ".dml patches applied to this FM:";
            internal string AddDMLPatchToolTip = "Add a new .dml patch to this FM";
            internal string RemoveDMLPatchToolTip = "Remove selected .dml patch from this FM";
            internal string FMNotInstalled = "FM must be installed in order to use this section.";
            internal string OpenFMFolder = "Open FM folder";
        }

        internal sealed class ReadmeArea_Class
        {
            internal string ViewHTMLReadme = "View HTML Readme";
            internal string ZoomInToolTip = "Zoom in (Ctrl++)";
            internal string ZoomOutToolTip = "Zoom out (Ctrl+-)";
            internal string ResetZoomToolTip = "Reset zoom (Ctrl+0)";
            internal string FullScreenToolTip = "Fullscreen";
            [FenGenBlankLine]
            internal string NoReadmeFound = "No readme found.";
            internal string UnableToLoadReadme = "Unable to load this readme.";
        }

        internal sealed class PlayOriginalGameMenu_Class
        {
            internal string Thief2_Multiplayer = "Thief 2 (multiplayer)";
        }

        internal sealed class MainButtons_Class
        {
            internal string PlayFM = "Play FM";
            internal string InstallFM = "Install FM";
            internal string UninstallFM = "Uninstall FM";
            internal string PlayOriginalGame = "Play original game...";
            internal string WebSearch = "Web search";
            internal string ScanAllFMs = "Scan all FMs...";
            internal string Import = "Import from...";
            internal string Settings = "Settings...";
        }

        internal sealed class ProgressBox_Class
        {
            internal string Scanning = "Scanning...";
            internal string InstallingFM = "Installing FM...";
            internal string UninstallingFM = "Uninstalling FM...";
            internal string ConvertingFiles = "Converting files...";
            internal string CheckingInstalledFMs = "Checking installed FMs...";
            internal string ReportScanningFirst = "Scanning ";
            internal string ReportScanningBetweenNumAndTotal = " of ";
            internal string ReportScanningLast = "...";
            internal string CancelingInstall = "Canceling install...";
            internal string ImportingFromDarkLoader = "Importing from DarkLoader...";
            internal string ImportingFromNewDarkLoader = "Importing from NewDarkLoader...";
            internal string ImportingFromFMSel = "Importing from FMSel...";
            internal string CachingReadmeFiles = "Caching readme files...";
            internal string DeletingFMArchive = "Deleting FM archive...";
        }

        internal sealed class SettingsWindow_Class
        {
            internal string TitleText = "Settings";
            internal string StartupTitleText = "AngelLoader Initial Setup";
            [FenGenBlankLine]
            internal string Paths_TabText = "Paths";
            internal string InitialSettings_TabText = "Initial Settings";
            [FenGenBlankLine]
            internal string Paths_PathsToGameExes = "Paths to game executables";
            // @GENGAMES (Localization - SettingsWindow - exe paths): Begin
            internal string Paths_DarkEngineGamesRequireNewDark = "* Thief 1, Thief 2 and System Shock 2 require NewDark.";
            internal string Paths_Thief3RequiresSneakyUpgrade = "* Thief 3 requires the Sneaky Upgrade 1.1.9.1 or above.";
            // @GENGAMES (Localization - SettingsWindow - exe paths): End
            [FenGenBlankLine]
            internal string Paths_SteamOptions = "Steam options";
            internal string Paths_PathToSteamExecutable = "Path to Steam executable (optional):";
            internal string Paths_LaunchTheseGamesThroughSteam = "If Steam exists, use it to launch these games:";
            [FenGenBlankLine]
            internal string Paths_Other = "Other";
            internal string Paths_BackupPath = "Backup path (required):";
            internal string Paths_FMArchivePaths = "FM archive paths";
            internal string Paths_IncludeSubfolders = "Include subfolders";
            internal string Paths_BackupPath_Info = "This is the directory that will be used for new backups of saves, screenshots, etc. when you uninstall a fan mission. This must be a different directory from any FM archive paths.";
            [FenGenBlankLine]
            internal string Paths_AddArchivePathToolTip = "Add archive path...";
            internal string Paths_RemoveArchivePathToolTip = "Remove selected archive path";
            [FenGenBlankLine]
            internal string Paths_ErrorSomePathsAreInvalid = "Some paths are invalid.";
            [FenGenBlankLine]
            internal string FMDisplay_TabText = "FM Display";
            [FenGenBlankLine]
            internal string FMDisplay_GameOrganization = "Game organization";
            internal string FMDisplay_GameOrganizationByTab = "Each game in its own tab";
            internal string FMDisplay_UseShortGameTabNames = "Use short names on game tabs";
            internal string FMDisplay_GameOrganizationOneList = "Everything in one list, and games are filters";
            [FenGenBlankLine]
            internal string FMDisplay_Sorting = "Sorting";
            [FenGenBlankLine]
            internal string FMDisplay_IgnoreArticles = "Ignore the following leading articles when sorting by title:";
            [FenGenBlankLine]
            internal string FMDisplay_MoveArticlesToEnd = "Move articles to the end of names when displaying them";
            [FenGenBlankLine]
            internal string FMDisplay_RatingDisplayStyle = "Rating display style";
            internal string FMDisplay_RatingDisplayStyleNDL = "NewDarkLoader (0-10 in increments of 1)";
            internal string FMDisplay_RatingDisplayStyleFMSel = "FMSel (0-5 in increments of 0.5)";
            internal string FMDisplay_RatingDisplayStyleUseStars = "Use stars";
            [FenGenBlankLine]
            internal string FMDisplay_DateFormat = "Date format";
            internal string FMDisplay_CurrentCultureShort = "System locale, short";
            internal string FMDisplay_CurrentCultureLong = "System locale, long";
            internal string FMDisplay_Custom = "Custom:";
            [FenGenBlankLine]
            internal string FMDisplay_RecentFMs = "Recent FMs";
            internal string FMDisplay_RecentFMs_MaxDays = "Maximum number of days to consider an FM \"recent\":";
            [FenGenBlankLine]
            internal string Other_TabText = "Other";
            [FenGenBlankLine]
            internal string Other_FMFileConversion = "FM file conversion";
            internal string Other_ConvertWAVsTo16BitOnInstall = "Convert .wavs to 16 bit on install";
            internal string Other_ConvertOGGsToWAVsOnInstall = "Convert .oggs to .wavs on install";
            [FenGenBlankLine]
            internal string Other_UninstallingFMs = "Uninstalling FMs";
            internal string Other_ConfirmBeforeUninstalling = "Confirm before uninstalling";
            internal string Other_WhenUninstallingBackUp = "When uninstalling, back up:";
            internal string Other_BackUpSavesAndScreenshotsOnly = "Saves and screenshots only";
            internal string Other_BackUpAllChangedFiles = "All changed files";
            internal string Other_BackUpAlwaysAsk = "Always ask";
            [FenGenBlankLine]
            internal string Other_Language = "Language";
            [FenGenBlankLine]
            internal string Other_WebSearch = "Web search";
            internal string Other_WebSearchURL = "Full URL to use when searching for an FM title:";
            internal string Other_WebSearchTitleVar = "$TITLE$ : the title of the FM";
            internal string Other_WebSearchResetToolTip = "Reset to default";
            [FenGenBlankLine]
            internal string Other_ConfirmPlayOnDCOrEnter = "Play FM on double-click / Enter";
            internal string Other_ConfirmPlayOnDCOrEnter_Ask = "Ask for confirmation";
            [FenGenBlankLine]
            internal string Other_ShowOrHideInterfaceElements = "Show or hide interface elements";
            internal string Other_HideUninstallButton = "Hide \"Install / Uninstall FM\" button (like FMSel)";
            internal string Other_HideFMListZoomButtons = "Hide FM list zoom buttons";
            [FenGenBlankLine]
            internal string Other_ReadmeBox = "Readme box";
            internal string Other_ReadmeUseFixedWidthFont = "Use a fixed-width font when displaying plain text";
        }

        internal sealed class DateFilterBox_Class
        {
            internal string ReleaseDateTitleText = "Set release date filter";
            internal string LastPlayedTitleText = "Set last played filter";
            [FenGenBlankLine]
            internal string From = "From:";
            internal string To = "To:";
            internal string NoMinimum = "(no minimum)";
            internal string NoMaximum = "(no maximum)";
        }

        internal sealed class TagsFilterBox_Class
        {
            internal string TitleText = "Set tags filter";
            [FenGenBlankLine]
            internal string MoveToAll = "All";
            internal string MoveToAny = "Any";
            internal string MoveToExclude = "Exclude";
            internal string Reset = "Reset";
            internal string IncludeAll = "Include All:";
            internal string IncludeAny = "Include Any:";
            internal string Exclude = "Exclude:";
            internal string ClearSelectedToolTip = "Clear selected";
            internal string ClearAllToolTip = "Clear all";
        }

        internal sealed class RatingFilterBox_Class
        {
            internal string TitleText = "Set rating filter";
            [FenGenBlankLine]
            internal string From = "From:";
            internal string To = "To:";
        }

        internal sealed class Importing_Class
        {
            internal string NothingWasImported = "Nothing was imported.";
            internal string SelectedFileIsNotAValidPath = "Selected file is not a valid path.";
            [FenGenBlankLine]
            internal string ImportFromDarkLoader_TitleText = "Import from DarkLoader";
            internal string DarkLoader_ChooseIni = "Choose DarkLoader.ini:";
            internal string DarkLoader_ImportFMData = "Import FM data";
            internal string DarkLoader_ImportSaves = "Import saves";
            internal string DarkLoader_SelectedFileIsNotDarkLoaderIni = "Selected file is not DarkLoader.ini.";
            internal string DarkLoader_SelectedDarkLoaderIniWasNotFound = "Selected DarkLoader.ini was not found.";
            internal string DarkLoader_NoArchiveDirsFound = "No archive directories were specified in DarkLoader.ini. Unable to import.";
            [FenGenBlankLine]
            internal string ImportFromNewDarkLoader_TitleText = "Import from NewDarkLoader";
            internal string ImportFromFMSel_TitleText = "Import from FMSel";
            internal string ChooseNewDarkLoaderIniFiles = "Choose NewDarkLoader .ini file(s):";
            internal string ChooseFMSelIniFiles = "Choose FMSel .ini file(s):";
            [FenGenBlankLine]
            internal string ImportData_Title = "Title";
            internal string ImportData_ReleaseDate = "Release date";
            internal string ImportData_LastPlayed = "Last played";
            internal string ImportData_Finished = "Finished";
            internal string ImportData_Comment = "Comment";
            internal string ImportData_Rating = "Rating";
            internal string ImportData_DisabledMods = "Disabled mods";
            internal string ImportData_Tags = "Tags";
            internal string ImportData_SelectedReadme = "Selected readme";
            internal string ImportData_Size = "Size";
        }

        internal sealed class ScanAllFMsBox_Class
        {
            internal string TitleText = "Scan all FMs";
            [FenGenBlankLine]
            internal string ScanAllFMsFor = "Scan all FMs for:";
            [FenGenBlankLine]
            internal string Title = "Title";
            internal string Author = "Author";
            internal string Game = "Game";
            internal string CustomResources = "Custom resources";
            internal string Size = "Size";
            internal string ReleaseDate = "Release date";
            internal string Tags = "Tags";
            [FenGenBlankLine]
            internal string Scan = "Scan";
            [FenGenBlankLine]
            internal string NothingWasScanned = "No options were selected; no FMs have been scanned.";
        }
    }
}
