#define FenGen_LocalizationSource

using System.Diagnostics.CodeAnalysis;
using static AngelLoader.FenGenAttributes;

// @MULTISEL(Localization): Remove all unused fields in here
namespace AngelLoader.DataClasses
{
    // NOTE(Localization - articles):
    // Articles can't really be localized because they need to apply to an FM's title and most are in English.
    // So I'm keeping the custom articles functionality just the way it is, that way another language's articles
    // can be added to the list.

    // NOTE: Missing localized bits:
    // -Hitches with localizability:
    //  -Date and rating forms are not set up for easy resizability of controls

    [FenGenLocalizationSourceClass]
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal sealed class LText_Class
    {
        // Notes:
        // -Comments don't support concatenation (try to fix later)
        // -Strings must be inside the nested classes or they won't be picked up (that's the nature of ini files)
        // -Strings in here can be readonly because they're set via reflection.

        internal readonly Meta_Class Meta = new Meta_Class();
        internal readonly Global_Class Global = new Global_Class();
        internal readonly SplashScreen_Class SplashScreen = new SplashScreen_Class();
        internal readonly BrowseDialogs_Class BrowseDialogs = new BrowseDialogs_Class();
        internal readonly AlertMessages_Class AlertMessages = new AlertMessages_Class();
        internal readonly MainMenu_Class MainMenu = new MainMenu_Class();
        internal readonly AboutWindow_Class AboutWindow = new AboutWindow_Class();
        internal readonly GameVersionsWindow_Class GameVersionsWindow = new GameVersionsWindow_Class();
        //internal readonly GlobalFMStats_Class GlobalFMStats = new GlobalFMStats_Class();
        internal readonly FMDeletion_Class FMDeletion = new FMDeletion_Class();
        internal readonly Difficulties_Class Difficulties = new Difficulties_Class();
        internal readonly FilterBar_Class FilterBar = new FilterBar_Class();
        internal readonly FMsList_Class FMsList = new FMsList_Class();
        internal readonly StatisticsTab_Class StatisticsTab = new StatisticsTab_Class();
        internal readonly EditFMTab_Class EditFMTab = new EditFMTab_Class();
        internal readonly CommentTab_Class CommentTab = new CommentTab_Class();
        internal readonly TagsTab_Class TagsTab = new TagsTab_Class();
        internal readonly PatchTab_Class PatchTab = new PatchTab_Class();
        internal readonly ModsTab_Class ModsTab = new ModsTab_Class();
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
        internal readonly CharacterEncoding_Class CharacterEncoding = new CharacterEncoding_Class();
        internal readonly AddFMsToSet_Class AddFMsToSet = new AddFMsToSet_Class();

        internal sealed class Meta_Class
        {
            [FenGenComment(
            "This should be the name of this file's language in this file's language.",
            "Example: English should be English, French should be Français, etc."
            )]
            internal readonly string TranslatedLanguageName = "English";
        }

        internal sealed class Global_Class
        {
            internal readonly string OK = "OK";
            internal readonly string Cancel = "Cancel";
            internal readonly string Stop = "Stop";
            internal readonly string Continue = "Continue";
            internal readonly string Yes = "Yes";
            internal readonly string No = "No";
            internal readonly string Skip = "Skip";
            internal readonly string BrowseEllipses = "Browse...";
            internal readonly string Reset = "Reset";
            internal readonly string Autodetect = "Autodetect";
            internal readonly string SelectAll = "Select all";
            internal readonly string SelectNone = "Select none";
            [FenGenBlankLine]
            internal readonly string Unrated = "Unrated";
            internal readonly string None = "None";
            [FenGenBlankLine]
            internal readonly string KilobyteShort = "KB";
            internal readonly string MegabyteShort = "MB";
            internal readonly string GigabyteShort = "GB";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Global): Begin
            [FenGenGameSet("GetLocalizedGameName")]
            internal readonly string Thief1 = "Thief 1";
            internal readonly string Thief2 = "Thief 2";
            internal readonly string Thief3 = "Thief 3";
            internal readonly string SystemShock2 = "System Shock 2";
            [FenGenBlankLine]
            [FenGenGameSet("GetShortLocalizedGameName")]
            internal readonly string Thief1_Short = "T1";
            internal readonly string Thief2_Short = "T2";
            internal readonly string Thief3_Short = "T3";
            internal readonly string SystemShock2_Short = "SS2";
            [FenGenBlankLine]
            [FenGenGameSet("GetLocalizedGameNameColon")]
            internal readonly string Thief1_Colon = "Thief 1:";
            internal readonly string Thief2_Colon = "Thief 2:";
            internal readonly string Thief3_Colon = "Thief 3:";
            internal readonly string SystemShock2_Colon = "System Shock 2:";
            // @GENGAMES (Localization - Global): End
            [FenGenBlankLine]
            internal readonly string ZoomIn = "Zoom in (Ctrl++)";
            internal readonly string ZoomOut = "Zoom out (Ctrl+-)";
            internal readonly string ResetZoom = "Reset zoom (Ctrl+0)";
            [FenGenBlankLine]
            internal readonly string Exit = "Exit";
        }

        internal sealed class SplashScreen_Class
        {
            [FenGenComment(
                "Certain settings (settable in the Settings window) are required to be valid, and if they're invalid,",
                "the Settings window will be shown on startup so the user can fix them."
                )]
            internal readonly string CheckingRequiredSettingsFields = "Checking required settings fields...";
            [FenGenComment(
                "This means it's reading data from each specified game's config file(s).",
                "For example, this might include cam_mod.ini for Thief 1 and Thief 2, SneakyOptions.ini for Thief: Deadly Shadows, etc."
                )]
            internal readonly string ReadingGameConfigFiles = "Reading game config files...";
            [FenGenComment(
                "It's searching all FM archive directories and FM installed directories to see if any new FMs have been added",
                "since the last run, and if so, adding them to the database."
                )]
            internal readonly string SearchingForNewFMs = "Searching for new FMs...";
            internal readonly string LoadingMainApp = "Loading main app...";
        }

        internal sealed class BrowseDialogs_Class
        {
            internal readonly string AllFiles = "All files (*.*)";
            internal readonly string ExeFiles = "Executable files (*.exe)";
            internal readonly string IniFiles = "ini files (*.ini)";
            internal readonly string DMLFiles = "NewDark .dml patch files (*.dml)";
        }

        internal sealed class AlertMessages_Class
        {
            internal readonly string Alert = "Alert";
            internal readonly string Warning = "Warning";
            internal readonly string Error = "Error";
            internal readonly string Confirm = "Confirm";
            internal readonly string Uninstall = "Uninstall";
            internal readonly string LeaveInstalled = "Leave installed";
            internal readonly string BackUp = "Back up";
            internal readonly string DontBackUp = "Don't back up";
            [FenGenComment(
                "This is displayed in the title bar of the \"Delete FM archive\" confirmation dialog box.")]
            internal readonly string DeleteFMArchive = "Delete FM archive";
            internal readonly string DeleteFMArchives = "Delete FM archives";
            [FenGenBlankLine]
            internal readonly string DontAskAgain = "Don't ask again";
            [FenGenBlankLine]
            internal readonly string AppClosing_OperationInProgress = "An operation is in progress. Please cancel or wait for it to finish.";
            [FenGenBlankLine]
            internal readonly string WebSearchURL_ProblemOpening = "There was a problem opening the specified web search URL.";
            [FenGenBlankLine]
            internal readonly string Install_ArchiveNotFound = "FM archive not found. Unable to install.";
            [FenGenComment(
                "These messages will be displayed with the game name and then the message itself.",
                "Example:",
                "\"Thief 2:",
                "Game executable file not specified or not found. Unable to install.\"")]
            internal readonly string Install_ExecutableNotFound = "Game executable file not specified or not found. Unable to install FM.";
            [FenGenComment(
                "\"FM install path\" means the folder in which FMs are installed, which is usually \"[Game directory]\\FMs\"",
                "for Thief 1, Thief 2, and System Shock 2 (for example \"C:\\Games\\Thief2\\FMs\") and usually",
                "\"C:\\ProgramData\\Thief 3 Sneaky Upgrade\\Installed FMs\" for Thief 3.")]
            internal readonly string Install_FMInstallPathNotFound = "FM install path not specified or not found. Unable to install FM.";
            internal readonly string Install_GameIsRunning = "Game is running; unable to install FM. Please exit the game and then try again.";
            internal readonly string Install_DiskSpaceCalculationFailed = "Could not calculate whether there is enough free disk space to install FMs to the following drives:";
            internal readonly string Install_NotEnoughFreeDiskSpace = "There is not enough free disk space to install FMs to the following drives:";
            internal readonly string Install_ContinueAfterErrorWarning = "If you continue, the installation will probably fail.";
            [FenGenBlankLine]
            internal readonly string Uninstall_Confirm = "Are you sure you want to uninstall this FM?";
            internal readonly string Uninstall_Confirm_Multiple = "Are you sure you want to uninstall these FMs?";
            internal readonly string Uninstall_GameIsRunning = "Game is running; unable to uninstall FM. Please exit the game and then try again.";
            internal readonly string Uninstall_FMAlreadyUninstalled = "This FM has already been uninstalled or its folder cannot be found. Mark it as uninstalled?";
            internal readonly string Uninstall_ArchiveNotFound = "This FM's archive file was not found! If you continue with uninstalling this FM, you won't be able to re-install it. Saves and screenshots will be backed up, but any other data will not. Are you sure you want to uninstall this FM?";
            // @MULTISEL(Uninstall not completed message): We should put this back in somewhere
            internal readonly string Uninstall_UninstallNotCompleted = "The uninstall could not be completed. The FM will be marked as uninstalled but its folder may be in an unknown state.";
            internal readonly string Uninstall_BackupSavesAndScreenshots = "Back up saves and screenshots?";
            internal readonly string Uninstall_BackupAllData = "Back up all modified/added/removed files (including saves and screenshots)?";
            internal readonly string Uninstall_BackupChooseNoNote = "If you choose \"Don't back up\", then existing backups will remain, but they will not be updated.";
            internal readonly string Uninstall_FailedFullyOrPartially = "Uninstall failed fully or partially.";
            [FenGenComment(
                "During the FM install process, either the user canceled the install or the install failed, and during the attempt to remove the leftover",
                "FM installed folder, an error occurred.")]
            internal readonly string InstallRollback_FMInstallFolderDeleteFail = "The following folder could not be removed:";
            [FenGenBlankLine]
            internal readonly string FileConversion_GameIsRunning = "Game is running; unable to convert files. Please exit the game and then try again.";
            [FenGenBlankLine]
            internal readonly string Play_ExecutableNotFound = "Game executable file not specified or not found. Unable to play.";
            internal readonly string Play_ExecutableNotFoundFM = "Game executable file not specified or not found. Unable to play FM.";
            internal readonly string Play_AnyGameIsRunning = "One or more supported games are already running. Please exit them first.";
            internal readonly string Play_ConfirmMessage = "Do you want to play this FM?";
            internal readonly string Play_InstallAndPlayConfirmMessage = "Do you want to install and play this FM?";
            internal readonly string Install_ConfirmSingular = "Do you want to install this FM?";
            [FenGenComment(
                "In English, if you had 5 FMs selected for example, it would say \"Do you want to install these 5 FMs?\".",
                "Structure these lines as appropriate for your language. You can leave one or the other blank if the",
                "number should go at the start or at the end of the final line.",
                "Make sure to leave a space at the end of the first line and start of the second line if they're non-empty, as demonstrated.")]
            internal readonly string Install_ConfirmPlural_BeforeNumber = "Do you want to install these ";
            internal readonly string Install_ConfirmPlural_AfterNumber = " FMs?";
            [FenGenBlankLine]
            internal readonly string DromEd_ExecutableNotFound = "DromEd.exe was not found in the game directory. Unable to open FM.";
            internal readonly string ShockEd_ExecutableNotFound = "ShockEd.exe was not found in the game directory. Unable to open FM.";
            [FenGenBlankLine]
            // @GENGAMES (Localization - Alerts - Dark multiplayer): Begin
            internal readonly string Thief2_Multiplayer_ExecutableNotFound = "Thief2MP.exe was not found in the game directory. Unable to play FM in multiplayer mode.";
            // @GENGAMES (Localization - Alerts - Dark multiplayer): End
            [FenGenBlankLine]
            [FenGenComment(
                "The generic \"Unable to add/remove patch\" messages are for when the folder was found but there was some other error that prevented",
                "the add/remove operation.")]
            internal readonly string Patch_AddDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to add patch.";
            internal readonly string Patch_AddDML_UnableToAdd = "Unable to add patch to fan mission folder.";
            internal readonly string Patch_RemoveDML_InstallDirNotFound = "This FM's installed folder cannot be found. Unable to remove patch.";
            internal readonly string Patch_RemoveDML_UnableToRemove = "Unable to remove patch from fan mission folder.";
            [FenGenComment(
                "This error message is displayed when the users clicks the \"Open FM folder\" button and the folder cannot be found.")]
            internal readonly string Patch_FMFolderNotFound = "The FM's folder couldn't be found.";
            [FenGenBlankLine]
            internal readonly string Misc_SneakyOptionsIniNotFound = "A Thief: Deadly Shadows install exists, but SneakyOptions.ini couldn't be found. Make sure your Thief: Deadly Shadows install has been patched with the Sneaky Upgrade 1.1.9.1 or later.";
            internal readonly string Misc_FMMarkedInstalledButNotInstalled = "This FM is marked as installed, but its folder cannot be found. Mark it as uninstalled?";
            [FenGenBlankLine]
            internal readonly string Extract_ZipExtractFailedFullyOrPartially = "Zip extraction failed fully or partially.";
            internal readonly string Extract_SevenZipExtractFailedFullyOrPartially = "7-zip extraction failed fully or partially.";
            [FenGenBlankLine]
            internal readonly string Scan_ExceptionInScanOne = "There was a problem scanning the FM. See the log file for error details.";
            internal readonly string Scan_ExceptionInScanMultiple = "There was a problem scanning the FMs. See the log file for error details.";
            [FenGenBlankLine]
            internal readonly string FindFMs_ExceptionReadingFMDataIni = "There was a problem reading the FM data ini file. See the log file for error details.";
            [FenGenBlankLine]
            internal readonly string DeleteFM_UnableToDelete = "The following FM archive could not be deleted:";
            [FenGenBlankLine]
            internal readonly string Help_HelpFileNotFound = "Help file not found.";
            internal readonly string Help_UnableToOpenHelpFile = "Unable to open help file.";
            [FenGenBlankLine]
            internal readonly string AddFM_UnableToCopy = "Unable to copy FM.";
            [FenGenBlankLine]
            [FenGenComment("Text for the button in the Error dialog box that lets the user view the error log file.")]
            internal readonly string Error_ViewLog = "View log";
            [FenGenBlankLine]
            internal readonly string FinishedOnUnknown_MultiFMChange = "All selected FMs' finished states are about to be removed and replaced with 'Unknown'. Are you sure want to do this?";
        }

        // Working, subject to change
        internal sealed class MainMenu_Class
        {
            internal readonly string MainMenuToolTip = "Main menu";
            internal readonly string GameVersions = "Game versions...";
            //internal readonly string GlobalFMStats = "Global FM stats...";
            internal readonly string Import = "Import";
            internal readonly string ScanAllFMs = "Scan all FMs...";
            internal readonly string ViewHelpFile = "View help file";
            internal readonly string About = "About AngelLoader";
        }

        internal sealed class AboutWindow_Class
        {
            internal readonly string TitleText = "About AngelLoader";
            [FenGenComment(
                "This is the header for the list of third-party libraries and portions of code that AngelLoader uses.",
                "In the About window, it looks like:",
                "\"AngelLoader uses:",
                "7-Zip",
                "ffmpeg",
                "etc.\"")]
            internal readonly string AngelLoaderUses = "AngelLoader uses:";
        }

        internal sealed class GameVersionsWindow_Class
        {
            [FenGenComment(
                "This is where game versions are displayed.",
                "For Thief 1, Thief 2, and System Shock 2, the NewDark version will be displayed.",
                "For Thief 3, the Sneaky Upgrade version will be displayed.",
                "Versions are detected by looking in the game executable (for T1, T2, SS2) or the Sneaky.dll file (for T3).",
                "Error messages relate to not being able to find the appropriate .exe/.dll file, or not being able to find",
                "a version in said file.")]
            [FenGenBlankLine]
            internal readonly string TitleText = "Game versions";
            [FenGenBlankLine]
            [FenGenComment(
                "An exe file has been specified for this game, but the file cannot be found.")]
            internal readonly string Error_GameExeNotFound = "Game executable not found";
            [FenGenBlankLine]
            [FenGenComment(
                "An exe file has been specified for Thief 3, but Sneaky.dll cannot be found.")]
            internal readonly string Error_SneakyDllNotFound = "Sneaky.dll not found";
            [FenGenBlankLine]
            [FenGenComment(
                "A version could not be found inside the .exe/.dll file.")]
            internal readonly string Error_GameVersionNotFound = "Version not found";
        }

        /*
        internal sealed class GlobalFMStats_Class
        {
            internal readonly string TitleText = "Global FM stats";
            [FenGenComment(
                "The number of FMs that are in the FMData.ini file.")]
            internal readonly string FMsInDatabase = "FMs in database:";
            [FenGenComment(
                "The number of FMs that actually exist across all specified archive directories and installed FM directories.")]
            internal readonly string AvailableFMs = "Available FMs:";
            [FenGenComment(
                "FMs that have not been scanned for game type.")]
            internal readonly string Unscanned = "Unscanned:";
            [FenGenComment(
                "FMs that have been scanned and detected as \"unsupported game or non-FM archive\".")]
            internal readonly string Unsupported = "Invalid or unsupported:";
        }
        */

        internal sealed class FMDeletion_Class
        {
            internal readonly string ArchiveNotFound = "This FM's archive could not be found. To delete this FM permanently, simply uninstall it.";
            internal readonly string ArchiveNotFound_All = "None of the selected FMs' archives could be found. To delete these FMs permanently, simply uninstall them.";
            internal readonly string AskToUninstallFMFirst = "This FM is installed. Uninstall it first?";
            internal readonly string AboutToDelete = "The following FM archive is about to be deleted from disk:";
            internal readonly string DuplicateArchivesFound = "Multiple archives with the same name were found. Please choose which archives(s) you want to delete.";
            [FenGenComment(
                "One of these will be displayed on a button on the \"Delete FM archive\" dialog box, depending if one or multiple FM archives were found.")]
            internal readonly string DeleteFM = "Delete FM";
            internal readonly string DeleteFMs = "Delete FM(s)";
        }

        internal sealed class Difficulties_Class
        {
            [FenGenComment(
                "Thief 1 and Thief 2 difficulties are: Normal, Hard, Expert, Extreme.",
                "Thief 3 difficulties are: Easy, Normal, Hard, Expert.",
                "System Shock 2 difficulties are: Easy, Normal, Hard, Impossible.",
                "\"Extreme\" is not a real difficulty for Thief 1 and Thief 2, but is included for DarkLoader compatibility.")]
            internal readonly string Easy = "Easy";
            internal readonly string Normal = "Normal";
            internal readonly string Hard = "Hard";
            internal readonly string Expert = "Expert";
            internal readonly string Extreme = "Extreme";
            internal readonly string Impossible = "Impossible";
            internal readonly string Unknown = "Unknown";
        }

        internal sealed class FilterBar_Class
        {
            [FenGenComment(
                "Depending on whether game organization is set to \"each game in its own tab\" or \"one list and games are filters\",",
                "one or the other of these messages will be shown.")]
            internal readonly string ShowHideGameFilterMenu_Tabs_ToolTip = "Show or hide game tabs";
            internal readonly string ShowHideGameFilterMenu_Filters_ToolTip = "Show or hide game filters";
            [FenGenBlankLine]
            internal readonly string Title = "Title:";
            internal readonly string Author = "Author:";
            [FenGenBlankLine]
            internal readonly string ReleaseDateToolTip = "Release date";
            internal readonly string LastPlayedToolTip = "Last played";
            internal readonly string TagsToolTip = "Tags";
            internal readonly string FinishedToolTip = "Finished";
            internal readonly string UnfinishedToolTip = "Unfinished";
            internal readonly string RatingToolTip = "Rating";
            [FenGenBlankLine]
            internal readonly string ShowUnsupported = "Show FMs marked as \"unsupported game or non-FM archive\"";
            internal readonly string ShowUnavailable = "Show unavailable FMs";
            internal readonly string ShowRecentAtTop = "Show recently added FMs at the top of the list";
            [FenGenBlankLine]
            internal readonly string RefreshFMsListButtonToolTip = "Refresh FMs list";
            internal readonly string RefreshFiltersButtonToolTip = "Refresh filters";
            internal readonly string ClearFiltersButtonToolTip = "Clear filters";
            internal readonly string ResetLayoutButtonToolTip = "Reset layout";
            [FenGenBlankLine]
            internal readonly string ShowHideMenuToolTip = "Show or hide filter controls";
            internal readonly string ShowHideMenu_Title = "Title";
            internal readonly string ShowHideMenu_Author = "Author";
            internal readonly string ShowHideMenu_ReleaseDate = "Release date";
            internal readonly string ShowHideMenu_LastPlayed = "Last played";
            internal readonly string ShowHideMenu_Tags = "Tags";
            internal readonly string ShowHideMenu_FinishedState = "Finished state";
            internal readonly string ShowHideMenu_Rating = "Rating";
            internal readonly string ShowHideMenu_ShowUnsupported = "Show unsupported";
            internal readonly string ShowHideMenu_ShowUnavailable = "Show unavailable";
            internal readonly string ShowHideMenu_ShowRecentAtTop = "Show recent at top";
        }

        internal sealed class FMsList_Class
        {
            [FenGenBlankLine]
            internal readonly string GameColumn = "Game";
            internal readonly string InstalledColumn = "Installed";
            internal readonly string TitleColumn = "Title";
            internal readonly string ArchiveColumn = "Archive";
            internal readonly string AuthorColumn = "Author";
            internal readonly string SizeColumn = "Size";
            internal readonly string RatingColumn = "Rating";
            internal readonly string FinishedColumn = "Finished";
            internal readonly string ReleaseDateColumn = "Release Date";
            internal readonly string LastPlayedColumn = "Last Played";
            [FenGenComment(
                "The date an FM was added to the list. Basically means the date you downloaded it and put it into your archives folder.")]
            internal readonly string DateAddedColumn = "Date Added";
            internal readonly string DisabledModsColumn = "Disabled Mods";
            internal readonly string CommentColumn = "Comment";
            [FenGenBlankLine]
            internal readonly string AllModsDisabledMessage = "* [All]";
            [FenGenBlankLine]
            internal readonly string ColumnMenu_ResetAllColumnsToVisible = "Reset all columns to visible";
            internal readonly string ColumnMenu_ResetAllColumnWidths = "Reset all column widths";
            internal readonly string ColumnMenu_ResetAllColumnPositions = "Reset all column positions";
            [FenGenBlankLine]
            internal readonly string FMMenu_PlayFM = "Play FM";
            internal readonly string FMMenu_PlayFM_Multiplayer = "Play FM (multiplayer)";
            internal readonly string FMMenu_InstallFM = "Install FM";
            internal readonly string FMMenu_InstallFMs = "Install FMs";
            internal readonly string FMMenu_UninstallFM = "Uninstall FM";
            internal readonly string FMMenu_UninstallFMs = "Uninstall FMs";
            internal readonly string FMMenu_PinFM = "Pin to top";
            internal readonly string FMMenu_UnpinFM = "Unpin from top";
            internal readonly string FMMenu_DeleteFM = "Delete FM archive";
            internal readonly string FMMenu_DeleteFMs = "Delete FM archives";
            internal readonly string FMMenu_OpenInDromEd = "Open FM in DromEd";
            internal readonly string FMMenu_OpenInShockEd = "Open FM in ShockEd";
            internal readonly string FMMenu_OpenFMFolder = "Open FM folder";
            internal readonly string FMMenu_Rating = "Rating";
            internal readonly string FMMenu_FinishedOn = "Finished on";
            internal readonly string FMMenu_ConvertAudio = "Convert audio";
            internal readonly string FMMenu_ScanFM = "Scan FM";
            [FenGenComment(
                "This should have an ellipsis after it (...) because it opens up a dialog box, whereas the singular",
                "one above doesn't.")]
            internal readonly string FMMenu_ScanFMs = "Scan FMs...";
            internal readonly string FMMenu_WebSearch = "Web search";
            [FenGenBlankLine]
            internal readonly string ConvertAudioMenu_ConvertWAVsTo16Bit = "Convert .wav files to 16 bit";
            internal readonly string ConvertAudioMenu_ConvertOGGsToWAVs = "Convert .ogg files to .wav";
            [FenGenBlankLine]
            // @MULTISEL(Localization): Try to put this line somewhere better sorted?
            [FenGenComment(
                "When multiple FMs are selected, the top-right tabs area will be hidden and this message will appear",
                "in its place.")]
            internal readonly string TopRight_MultipleFMsSelected = "Multiple FMs selected.";
        }

        internal sealed class StatisticsTab_Class
        {
            internal readonly string TabText = "Statistics";
            [FenGenBlankLine]
            internal readonly string CustomResources = "Custom resources:";
            internal readonly string CustomResourcesNotScanned = "Custom resources not scanned.";
            // @GENGAMES (Localization - Custom resource detection not supported): Begin
            internal readonly string CustomResourcesNotSupportedForThief3 = "Custom resource detection is not supported for Thief 3 FMs.";
            // @GENGAMES (Localization - Custom resource detection not supported): End
            internal readonly string NoFMSelected = "No FM selected.";
            [FenGenBlankLine]
            internal readonly string Map = "Map";
            internal readonly string Automap = "Automap";
            internal readonly string Textures = "Textures";
            internal readonly string Sounds = "Sounds";
            internal readonly string Movies = "Movies";
            internal readonly string Objects = "Objects";
            internal readonly string Creatures = "Creatures";
            internal readonly string Motions = "Motions";
            internal readonly string Scripts = "Scripts";
            internal readonly string Subtitles = "Subtitles";
            [FenGenBlankLine]
            internal readonly string RescanCustomResources = "Rescan custom resources";
        }

        internal sealed class EditFMTab_Class
        {
            internal readonly string TabText = "Edit FM";
            [FenGenBlankLine]
            internal readonly string Title = "Title:";
            internal readonly string Author = "Author:";
            internal readonly string ReleaseDate = "Release date:";
            internal readonly string LastPlayed = "Last played:";
            internal readonly string Rating = "Rating:";
            internal readonly string FinishedOn = "Finished on...";
            internal readonly string PlayFMInThisLanguage = "Play FM in this language:";
            internal readonly string DefaultLanguage = "Default";
            [FenGenBlankLine]
            internal readonly string RescanTitleToolTip = "Rescan title";
            internal readonly string RescanAuthorToolTip = "Rescan author";
            internal readonly string RescanReleaseDateToolTip = "Rescan release date";
            internal readonly string RescanLanguages = "Rescan for supported languages";
            internal readonly string RescanForReadmes = "Rescan for readmes";
        }

        internal sealed class CommentTab_Class
        {
            internal readonly string TabText = "Comment";
        }

        internal sealed class TagsTab_Class
        {
            internal readonly string TabText = "Tags";
            [FenGenBlankLine]
            internal readonly string AddTag = "Add tag";
            internal readonly string AddFromList = "Add from list...";
            [FenGenComment(
                "Each category in the \"Add from list...\" menu will have this as a menu item that the user can click",
                "to create a new tag in that category.")]
            internal readonly string CustomTagInCategory = "<custom>";
            internal readonly string RemoveTag = "Remove tag";
            [FenGenBlankLine]
            internal readonly string AskRemoveCategory = "Remove category?";
            internal readonly string AskRemoveTag = "Remove tag?";
        }

        internal sealed class PatchTab_Class
        {
            internal readonly string TabText = "Patch & Customize";
            [FenGenBlankLine]
            internal readonly string DMLPatchesApplied = ".dml patches applied to this FM:";
            internal readonly string AddDMLPatchToolTip = "Add a new .dml patch to this FM";
            internal readonly string RemoveDMLPatchToolTip = "Remove selected .dml patch from this FM";
            internal readonly string FMNotInstalled = "FM must be installed in order to use this section.";
            internal readonly string OpenFMFolder = "Open FM folder";
        }

        internal sealed class ModsTab_Class
        {
            internal readonly string TabText = "Mods";
            internal readonly string Header = "Enable or disable these mods for this FM:";
            internal readonly string ImportantModsCaution = "These mods should not be disabled unless you know what you're doing.";
            [FenGenComment(
                "This is for the checkbox that toggles showing \"important\" mods, which is to say, mods that",
                "are necessary for correct functioning of the game and/or fan missions and should not be disabled",
                "under normal circumstances.")]
            internal readonly string ShowImportantMods = "Show important mods";
            internal readonly string EnableAll = "Enable all";
            internal readonly string DisabledMods = "Disabled mods:";
        }

        internal sealed class ReadmeArea_Class
        {
            internal readonly string ViewHTMLReadme = "View HTML Readme";
            internal readonly string FullScreenToolTip = "Fullscreen";
            internal readonly string CharacterEncoding = "Character encoding";
            [FenGenBlankLine]
            internal readonly string NoReadmeFound = "No readme found.";
            internal readonly string UnableToLoadReadme = "Unable to load this readme.";
        }

        // @GENGAMES (Localization - PlayOriginalGameMenu_Class) - Begin
        internal sealed class PlayOriginalGameMenu_Class
        {
            internal readonly string Thief2_Multiplayer = "Thief 2 (multiplayer)";
            [FenGenGameSet("GetLocalizedGamePlayOriginalText")]
            internal readonly string Thief1_PlayOriginal = "Play Thief 1 with no FM";
            internal readonly string Thief2_PlayOriginal = "Play Thief 2 with no FM";
            internal readonly string Thief3_PlayOriginal = "Play Thief 3 with no FM";
            internal readonly string SystemShock2_PlayOriginal = "Play System Shock 2 with no FM";
        }
        // @GENGAMES (Localization - PlayOriginalGameMenu_Class) - End

        internal sealed class MainButtons_Class
        {
            internal readonly string PlayFM = "Play FM";
            // @MULTISEL: Combine the button and the menu strings into one place?
            internal readonly string InstallFM = "Install FM";
            internal readonly string UninstallFM = "Uninstall FM";
            internal readonly string InstallFMs = "Install FMs";
            internal readonly string UninstallFMs = "Uninstall FMs";
            internal readonly string PlayOriginalGame = "Play without FM...";
            internal readonly string WebSearch = "Web search";
            internal readonly string Settings = "Settings...";
        }

        internal sealed class ProgressBox_Class
        {
            internal readonly string InstallingFM = "Installing FM...";
            internal readonly string InstallingFMs = "Installing FMs...";
            internal readonly string PreparingToInstall = "Preparing to install...";
            internal readonly string CancelingInstall = "Canceling install...";
            [FenGenComment("It's removing any leftover files from an FM install that didn't succeed")]
            internal readonly string CleaningUpFailedInstall = "Cleaning up failed install...";
            [FenGenComment("For example, we could be restoring backed up saves and screenshots for this FM")]
            internal readonly string RestoringBackup = "Restoring backup...";
            [FenGenBlankLine]
            internal readonly string UninstallingFM = "Uninstalling FM...";
            internal readonly string UninstallingFMs = "Uninstalling FMs...";
            // @MULTISEL(Localization): Not sure if I'm going to use this
            internal readonly string PreparingToUninstall = "Preparing to uninstall...";
            [FenGenBlankLine]
            internal readonly string ConvertingFiles = "Converting audio files...";
            [FenGenBlankLine]
            internal readonly string PreparingToScanFMs = "Preparing to scan...";
            internal readonly string Scanning = "Scanning...";
            internal readonly string ReportScanningFirst = "Scanning ";
            internal readonly string ReportScanningBetweenNumAndTotal = " of ";
            internal readonly string ReportScanningLast = "...";
            [FenGenBlankLine]
            internal readonly string ImportingFromDarkLoader = "Importing from DarkLoader...";
            internal readonly string ImportingFromNewDarkLoader = "Importing from NewDarkLoader...";
            internal readonly string ImportingFromFMSel = "Importing from FMSel...";
            [FenGenBlankLine]
            internal readonly string CachingReadmeFiles = "Caching readme files...";
            internal readonly string DeletingFMArchive = "Deleting FM archive...";
            internal readonly string CheckingFreeSpace = "Checking free disk space...";
        }

        internal sealed class SettingsWindow_Class
        {
            internal readonly string TitleText = "Settings";
            internal readonly string StartupTitleText = "AngelLoader Initial Setup";
            [FenGenBlankLine]
            internal readonly string Paths_TabText = "Paths";
            internal readonly string InitialSettings_TabText = "Initial Settings";
            [FenGenBlankLine]
            internal readonly string Paths_PathsToGameExes = "Paths to game executables";
            // @GENGAMES (Localization - SettingsWindow - exe paths): Begin
            internal readonly string Paths_DarkEngineGamesRequireNewDark = "* Thief 1, Thief 2 and System Shock 2 require NewDark.";
            internal readonly string Paths_Thief3RequiresSneakyUpgrade = "* Thief 3 requires the Sneaky Upgrade 1.1.9.1 or above.";
            // @GENGAMES (Localization - SettingsWindow - exe paths): End
            [FenGenBlankLine]
            internal readonly string Paths_SteamOptions = "Steam options";
            internal readonly string Paths_PathToSteamExecutable = "Path to Steam executable (optional):";
            internal readonly string Paths_LaunchTheseGamesThroughSteam = "If Steam exists, use it to launch these games:";
            [FenGenBlankLine]
            internal readonly string Paths_Other = "Other";
            internal readonly string Paths_BackupPath = "Backup path (required):";
            internal readonly string Paths_FMArchivePaths = "FM archive paths";
            [FenGenComment(
                "This is a checkbox in the \"FM archive paths\" section. If checked, then all subfolders of all specified",
                "FM archive paths will also be searched for FM archives; otherwise, only exactly the specified FM archive",
                "paths will be searched.")]
            internal readonly string Paths_IncludeSubfolders = "Include subfolders";
            internal readonly string Paths_BackupPath_Info = "This is the directory that will be used for new backups of saves, screenshots, etc. when you uninstall a fan mission. This must be a different directory from any FM archive paths.";
            [FenGenBlankLine]
            internal readonly string Paths_AddArchivePathToolTip = "Add archive path...";
            internal readonly string Paths_RemoveArchivePathToolTip = "Remove selected archive path";
            [FenGenBlankLine]
            internal readonly string Paths_ErrorSomePathsAreInvalid = "Some paths are invalid.";
            [FenGenBlankLine]
            internal readonly string Appearance_TabText = "Appearance";
            [FenGenBlankLine]
            internal readonly string Appearance_Language = "Language";
            [FenGenBlankLine]
            internal readonly string Appearance_Theme = "Theme";
            internal readonly string Appearance_Theme_Classic = "Classic";
            internal readonly string Appearance_Theme_Dark = "Dark";
            [FenGenBlankLine]
            internal readonly string Appearance_FMsList = "FMs list";
            [FenGenBlankLine]
            internal readonly string Appearance_GameOrganization = "Game organization:";
            internal readonly string Appearance_GameOrganizationByTab = "Each game in its own tab";
            internal readonly string Appearance_UseShortGameTabNames = "Use short names on game tabs";
            internal readonly string Appearance_GameOrganizationOneList = "Everything in one list, and games are filters";
            [FenGenBlankLine]
            internal readonly string Appearance_Sorting = "Sorting";
            internal readonly string Appearance_IgnoreArticles = "Ignore the following leading articles when sorting by title:";
            internal readonly string Appearance_MoveArticlesToEnd = "Move articles to the end of names when displaying them";
            [FenGenBlankLine]
            internal readonly string Appearance_RatingDisplayStyle = "Rating display style";
            internal readonly string Appearance_RatingDisplayStyleNDL = "NewDarkLoader (0-10 in increments of 1)";
            internal readonly string Appearance_RatingDisplayStyleFMSel = "FMSel (0-5 in increments of 0.5)";
            internal readonly string Appearance_RatingDisplayStyleUseStars = "Use stars";
            [FenGenBlankLine]
            internal readonly string Appearance_DateFormat = "Date format";
            internal readonly string Appearance_CurrentCultureShort = "System locale, short";
            internal readonly string Appearance_CurrentCultureLong = "System locale, long";
            internal readonly string Appearance_Custom = "Custom:";
            [FenGenBlankLine]
            internal readonly string Appearance_RecentFMs = "Recent FMs";
            internal readonly string Appearance_RecentFMs_MaxDays = "Maximum number of days to consider an FM \"recent\":";
            [FenGenBlankLine]
            internal readonly string Appearance_ShowOrHideInterfaceElements = "Show or hide interface elements";
            internal readonly string Appearance_HideUninstallButton = "Hide \"Install / Uninstall FM\" button (like FMSel)";
            internal readonly string Appearance_HideFMListZoomButtons = "Hide FM list zoom buttons";
            internal readonly string Appearance_HideExitButton = "Hide exit button";
            [FenGenBlankLine]
            internal readonly string Appearance_ReadmeBox = "Readme box";
            internal readonly string Appearance_ReadmeUseFixedWidthFont = "Use a fixed-width font when displaying plain text";
            [FenGenBlankLine]
            internal readonly string Appearance_PlayWithoutFM = "Play without FM";
            internal readonly string Appearance_PlayWithoutFM_SingleButton = "Single button with menu";
            internal readonly string Appearance_PlayWithoutFM_MultiButton = "Multiple buttons";
            [FenGenBlankLine]
            internal readonly string Other_TabText = "Other";
            [FenGenBlankLine]
            internal readonly string Other_FMFileConversion = "FM file conversion";
            internal readonly string Other_ConvertWAVsTo16BitOnInstall = "Convert .wavs to 16 bit on install";
            internal readonly string Other_ConvertOGGsToWAVsOnInstall = "Convert .oggs to .wavs on install";
            [FenGenBlankLine]
            internal readonly string Other_InstallingFMs = "Installing FMs";
            internal readonly string Other_ConfirmBeforeInstallingFM = "Confirm before installing:";
            internal readonly string Other_InstallConfirm_Always = "Always";
            internal readonly string Other_InstallConfirm_OnlyForMultipleFMs = "Only for multiple FMs";
            internal readonly string Other_InstallConfirm_Never = "Never";
            [FenGenBlankLine]
            internal readonly string Other_UninstallingFMs = "Uninstalling FMs";
            internal readonly string Other_ConfirmBeforeUninstalling = "Confirm before uninstalling";
            internal readonly string Other_WhenUninstallingBackUp = "When uninstalling, back up:";
            internal readonly string Other_BackUpSavesAndScreenshotsOnly = "Saves and screenshots only";
            internal readonly string Other_BackUpAllChangedFiles = "All changed files";
            internal readonly string Other_BackUpAlwaysAsk = "Always ask";
            [FenGenBlankLine]
            internal readonly string Other_WebSearch = "Web search";
            internal readonly string Other_WebSearchURL = "Full URL to use when searching for an FM title:";
            [FenGenComment(
                "$TITLE$ is a keyword that the user can place into the URL to signify that the current FM's title should",
                "be placed there. It should not be translated (it must always remain $TITLE$).")]
            internal readonly string Other_WebSearchTitleVar = "$TITLE$ : the title of the FM";
            internal readonly string Other_WebSearchResetToolTip = "Reset to default";
            [FenGenBlankLine]
            internal readonly string Other_ConfirmPlayOnDCOrEnter = "Play FM on double-click / Enter";
            internal readonly string Other_ConfirmPlayOnDCOrEnter_Ask = "Ask for confirmation";
        }

        internal sealed class DateFilterBox_Class
        {
            internal readonly string ReleaseDateTitleText = "Set release date filter";
            internal readonly string LastPlayedTitleText = "Set last played filter";
            [FenGenBlankLine]
            internal readonly string From = "From:";
            internal readonly string To = "To:";
            internal readonly string NoMinimum = "(no minimum)";
            internal readonly string NoMaximum = "(no maximum)";
        }

        internal sealed class TagsFilterBox_Class
        {
            internal readonly string TitleText = "Set tags filter";
            [FenGenBlankLine]
            internal readonly string MoveToAll = "All";
            internal readonly string MoveToAny = "Any";
            internal readonly string MoveToExclude = "Exclude";
            internal readonly string Reset = "Reset";
            internal readonly string IncludeAll = "Include All:";
            internal readonly string IncludeAny = "Include Any:";
            internal readonly string Exclude = "Exclude:";
            internal readonly string ClearSelectedToolTip = "Clear selected";
            internal readonly string ClearAllToolTip = "Clear all";
        }

        internal sealed class RatingFilterBox_Class
        {
            internal readonly string TitleText = "Set rating filter";
            [FenGenBlankLine]
            internal readonly string From = "From:";
            internal readonly string To = "To:";
        }

        internal sealed class Importing_Class
        {
            internal readonly string NothingWasImported = "Nothing was imported.";
            internal readonly string SelectedFileIsNotAValidPath = "Selected file is not a valid path.";
            [FenGenBlankLine]
            internal readonly string ImportFromDarkLoader_TitleText = "Import from DarkLoader";
            internal readonly string DarkLoader_ChooseIni = "Choose DarkLoader.ini:";
            internal readonly string DarkLoader_ImportFMData = "Import FM data";
            internal readonly string DarkLoader_ImportSaves = "Import saves";
            internal readonly string DarkLoader_SelectedFileIsNotDarkLoaderIni = "Selected file is not DarkLoader.ini.";
            internal readonly string DarkLoader_SelectedDarkLoaderIniWasNotFound = "Selected DarkLoader.ini was not found.";
            internal readonly string DarkLoader_NoArchiveDirsFound = "No archive directories were specified in DarkLoader.ini. Unable to import.";
            [FenGenBlankLine]
            internal readonly string ImportFromNewDarkLoader_TitleText = "Import from NewDarkLoader";
            internal readonly string ImportFromFMSel_TitleText = "Import from FMSel";
            internal readonly string ChooseNewDarkLoaderIniFiles = "Choose NewDarkLoader .ini file(s):";
            internal readonly string ChooseFMSelIniFiles = "Choose FMSel .ini file(s):";
            [FenGenBlankLine]
            internal readonly string ImportData_Title = "Title";
            internal readonly string ImportData_ReleaseDate = "Release date";
            internal readonly string ImportData_LastPlayed = "Last played";
            internal readonly string ImportData_Finished = "Finished";
            internal readonly string ImportData_Comment = "Comment";
            internal readonly string ImportData_Rating = "Rating";
            internal readonly string ImportData_DisabledMods = "Disabled mods";
            internal readonly string ImportData_Tags = "Tags";
            internal readonly string ImportData_SelectedReadme = "Selected readme";
            internal readonly string ImportData_Size = "Size";
        }

        internal sealed class ScanAllFMsBox_Class
        {
            internal readonly string TitleText = "Scan all FMs";
            [FenGenBlankLine]
            internal readonly string ScanAllFMsFor = "Scan all FMs for:";
            [FenGenBlankLine]
            internal readonly string Title = "Title";
            internal readonly string Author = "Author";
            internal readonly string Game = "Game";
            internal readonly string CustomResources = "Custom resources";
            internal readonly string Size = "Size";
            internal readonly string ReleaseDate = "Release date";
            internal readonly string Tags = "Tags";
            [FenGenBlankLine]
            internal readonly string Scan = "Scan";
            [FenGenBlankLine]
            internal readonly string NothingWasScanned = "No options were selected; no FMs have been scanned.";
        }

        internal sealed class CharacterEncoding_Class
        {
            internal readonly string AutodetectNow = "Autodetect now";
            internal readonly string Category_Arabic = "Arabic";
            internal readonly string Category_Baltic = "Baltic";
            internal readonly string Category_CentralEuropean = "Central European";
            internal readonly string Category_Chinese = "Chinese";
            internal readonly string Category_Cyrillic = "Cyrillic";
            internal readonly string Category_EasternEuropean = "Eastern European";
            internal readonly string Category_Greek = "Greek";
            internal readonly string Category_Hebrew = "Hebrew";
            internal readonly string Category_Japanese = "Japanese";
            internal readonly string Category_Korean = "Korean";
            internal readonly string Category_Latin = "Latin";
            internal readonly string Category_NorthernEuropean = "Northern European";
            internal readonly string Category_Taiwan = "Taiwan";
            internal readonly string Category_Thai = "Thai";
            internal readonly string Category_Turkish = "Turkish";
            internal readonly string Category_UnitedStates = "United States";
            internal readonly string Category_Vietnamese = "Vietnamese";
            internal readonly string Category_WesternEuropean = "Western European";
            internal readonly string Category_Other = "Other";
        }

        internal sealed class AddFMsToSet_Class
        {
            // @vNext: Working text - testing purposes
            internal readonly string AddFM_DialogTitle = "Add FM";
            internal readonly string AddFMs_DialogTitle = "Add FMs";
            internal readonly string AddFM_Dialog_AskMessage = "The following FM is about to be added:";
            internal readonly string AddFMs_Dialog_AskMessage = "The following FMs are about to be added:";
            internal readonly string AddFM_Dialog_ChooseArchiveDir = "You have multiple FM archive directories. Please choose one to copy this FM to.";
            internal readonly string AddFMs_Dialog_ChooseArchiveDir = "You have multiple FM archive directories. Please choose one to copy these FMs to.";
            internal readonly string AddFM_Add = "Add";
        }
    }
}
