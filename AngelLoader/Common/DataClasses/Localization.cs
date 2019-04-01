using static AngelLoader.Common.Attributes;

namespace AngelLoader.Common.DataClasses
{
    // NOTE:
    // Articles can't really be localized because they need to apply to an FM's title and most are in English.
    // So I'm keeping the custom articles functionality just the way it is, that way another language's articles
    // can be added to the list.

    // TODO: Missing localized bits:
    // -Remove tag / remove all tags MessageBoxes (make less annoying)
    // -Hitches with localizability:
    //  -Date and rating forms are not set up for easy resizability of controls

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
            [FenGenComment("This should be the name of this file's language in this file's language.\r\nExample: English should be English, French should be Français, etc.")]
            internal static string LanguageName = "English";
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
            [FenGenBlankLine]
            internal static string Unrated = "Unrated";
            internal static string None = "None";
            internal static string CustomTagInCategory = "<custom>";
            [FenGenBlankLine]
            internal static string KilobyteShort = "KB";
            internal static string MegabyteShort = "MB";
            internal static string GigabyteShort = "GB";
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
            [FenGenBlankLine]
            internal static string AppClosing_OperationInProgress = "An operation is in progress. Please cancel or wait for it to finish.";
            [FenGenBlankLine]
            internal static string WebSearchURL_IsInvalid = "The specified site to search is not a valid URL.";
            internal static string WebSearchURL_ProblemOpening = "There was a problem opening the specified web search URL.";
            [FenGenBlankLine]
            internal static string Install_UnknownGameType = "This FM's game type is unknown, so it can't be installed.";
            internal static string Install_UnsupportedGameType = "This FM's game type is unsupported, so it can't be installed.";
            internal static string Install_ArchiveNotFound = "FM archive not found. Unable to install.";
            internal static string Install_ExecutableNotFound = "Executable file not specified or not found. Unable to install.";
            internal static string Install_FMInstallPathNotFound = "FM install path not specified or not found. Unable to install.";
            internal static string Install_GameIsRunning = "Game is running; unable to install. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal static string Uninstall_GameIsRunning = "Game is running; unable to uninstall. Please exit the game and then try again.";
            internal static string Uninstall_FMAlreadyUninstalled = "This FM has already been uninstalled or its folder cannot be found. Mark it as uninstalled?";
            internal static string Uninstall_ArchiveNotFound = "This FM's archive file was not found! If you continue with uninstalling this FM, you won't be able to re-install it. Saves and screenshots will be backed up, but any other data will not. Click Yes if this is okay, or No to cancel the uninstall.";
            internal static string Uninstall_UninstallNotCompleted = "The uninstall could not be completed. The FM will be marked as uninstalled but its folder may be in an unknown state.";
            internal static string Uninstall_BackupSavesAndScreenshots = "Back up saves and screenshots?";
            internal static string Uninstall_BackupAllData = "Back up all modified/added/removed files (including saves and screenshots)?";
            internal static string Uninstall_FailedFullyOrPartially = "Uninstall failed fully or partially.";
            [FenGenBlankLine]
            internal static string FileConversion_GameIsRunning = "Game is running; unable to convert files. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal static string Play_ExecutableNotFound = "Executable file not specified or not found. Unable to play.";
            internal static string Play_GamePathNotFound = "Game path not found. Unable to play.";
            internal static string Play_ExecutableNotFoundFM = "Executable file not specified or not found. Unable to play FM.";
            internal static string Play_GameIsRunning = "Game is already running. Exit it first!";
            internal static string Play_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to play.";
            [FenGenBlankLine]
            internal static string DromEd_ExecutableNotFound = "DromEd.exe was not found in the game directory. Unable to open FM.";
            internal static string DromEd_UnknownGameType = "Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to open FM.";
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
            internal static string Extract_ZipExtractFailedFullyOrPartially ="Zip extract failed fully or partially.";
            internal static string Extract_SevenZipExtractFailedFullyOrPartially ="7-zip extract failed fully or partially.";
        }

        internal static class Difficulties
        {
            internal static string Easy = "Easy";
            internal static string Normal = "Normal";
            internal static string Hard = "Hard";
            internal static string Expert = "Expert";
            internal static string Extreme = "Extreme";
            internal static string Unknown = "Unknown";
        }

        internal static class GameTabs
        {
            internal static string Thief1 = "Thief 1";
            internal static string Thief2 = "Thief 2";
            internal static string Thief3 = "Thief 3";
        }

        internal static class FilterBar
        {
            internal static string Thief1ToolTip = "Thief 1";
            internal static string Thief2ToolTip = "Thief 2";
            internal static string Thief3ToolTip = "Thief 3";
            [FenGenBlankLine]
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
            internal static string ShowJunk = "Show junk";
            [FenGenBlankLine]
            internal static string RefreshFilteredListButtonToolTip = "Refresh filtered list";
            internal static string ClearFiltersButtonToolTip = "Clear filters";
            internal static string ResetLayoutButtonToolTip = "Reset layout";
        }

        internal static class FMsList
        {
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
            internal static string FMMenu_InstallFM = "Install FM";
            internal static string FMMenu_UninstallFM = "Uninstall FM";
            internal static string FMMenu_OpenInDromEd = "Open FM in DromEd";
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
            internal static string CustomResourcesNotSupportedForThief3 = "Custom resource detection is not supported for Thief 3 FMs.";
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
            [FenGenBlankLine]
            internal static string RescanTitleToolTip = "Rescan title";
            internal static string RescanAuthorToolTip = "Rescan author";
            internal static string RescanReleaseDateToolTip = "Rescan release date";
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
            internal static string DMLPatchesApplied = ".dml patches applied to this FM:";
            internal static string AddDMLPatchToolTip = "Add a new .dml patch to this FM";
            internal static string RemoveDMLPatchToolTip = "Remove selected .dml patch from this FM";
            internal static string FMNotInstalled = "FM must be installed in order to use this section.";
            internal static string OpenFMFolder = "Open FM folder";
        }

        internal static class ReadmeArea
        {
            internal static string ViewHTMLReadme = "View HTML Readme";
            internal static string ZoomInToolTip = "Zoom in";
            internal static string ZoomOutToolTip = "Zoom out";
            internal static string ResetZoomToolTip = "Reset zoom";
            internal static string FullScreenToolTip = "Fullscreen";
            [FenGenBlankLine]
            internal static string NoReadmeFound = "No readme found.";
            internal static string UnableToLoadReadme = "Unable to load this readme.";
        }

        internal static class PlayOriginalGameMenu
        {
            internal static string Thief1 = "Thief 1";
            internal static string Thief2 = "Thief 2";
            internal static string Thief3 = "Thief 3";
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
            internal static string Paths_Thief1 = "Thief 1:";
            internal static string Paths_Thief2 = "Thief 2:";
            internal static string Paths_Thief3 = "Thief 3:";

            internal static string Paths_Thief1AndThief2RequireNewDark = "* Thief 1 and Thief 2 require NewDark.";

            internal static string Paths_Thief3RequiresSneakyUpgrade = "* Thief 3 requires the Sneaky Upgrade 1.1.9.1 or above.";
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
            internal static string FMDisplay_CurrentCultureShort = "Current culture short";
            internal static string FMDisplay_CurrentCultureLong = "Current culture long";
            internal static string FMDisplay_Custom = "Custom:";
            [FenGenBlankLine]
            internal static string FMDisplay_ErrorInvalidDateFormat = "Invalid date format.";
            internal static string FMDisplay_ErrorDateOutOfRange = "The date and time is outside the range of dates supported by the calendar used by the current culture.";
            [FenGenBlankLine]
            internal static string Other_TabText = "Other";
            [FenGenBlankLine]
            internal static string Other_FMFileConversion = "FM file conversion";
            internal static string Other_ConvertWAVsTo16BitOnInstall = "Convert .wavs to 16 bit on install";
            internal static string Other_ConvertOGGsToWAVsOnInstall = "Convert .oggs to .wavs on install";
            [FenGenBlankLine]
            internal static string Other_BackUpSaves = "Back up data when uninstalling";
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
            internal static string Thief1 = "Thief 1";
            internal static string Thief2 = "Thief 2";
            internal static string Thief3 = "Thief 3";
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
            internal static string SelectAll = "Select all";
            internal static string SelectNone = "Select none";
            internal static string Scan = "Scan";
            [FenGenBlankLine]
            internal static string NothingWasScanned = "No options were selected; no FMs have been scanned.";
        }
    }
}
