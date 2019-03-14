namespace AngelLoader.Common
{
    // TODO: Missing localized bits:
    // -Tags form
    // -Articles (probably just remove the customizability and just have them be defined per-language)
    // -Alert boxes
    internal static class LText_bak
    {
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

            internal static string Unrated = "Unrated";
            internal static string None = "None";
            internal static string CustomTagInCategory = "<custom>";

            internal static string Error = "Error";

            internal static string KilobyteShort = "KB";
            internal static string MegabyteShort = "MB";
            internal static string GigabyteShort = "GB";
        }

        internal static class BrowseDialogs
        {
            internal static string AllFiles = "All files (*.*)";
            internal static string ExeFiles = "Executable files (*.exe)";
            internal static string IniFiles = "ini files (*.ini)";
        }

        internal static class AlertMessages
        {
            internal static string Alert = "Alert";
            internal static string WebSearchURLIsInvalid = "The specified site to search is not a valid URL.";
        }

        internal static class Difficulties
        {
            internal static string Easy = "Easy";
            internal static string Normal = "Normal";
            internal static string Hard = "Hard";
            internal static string Expert = "Expert";
            internal static string Extreme = "Extreme";
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

            internal static string Title = "Title:";
            internal static string Author = "Author:";

            internal static string ReleaseDateToolTip = "Release date";
            internal static string LastPlayedToolTip = "Last played";
            internal static string TagsToolTip = "Tags";
            internal static string FinishedToolTip = "Finished";
            internal static string UnfinishedToolTip = "Unfinished";
            internal static string RatingToolTip = "Rating";

            internal static string ShowJunk = "Show junk";
        }

        internal static string RefreshFilteredListButtonToolTip = "Refresh filtered list";
        internal static string ClearFiltersButtonToolTip = "Clear filters";
        internal static string ResetLayoutButtonToolTip = "Reset layout";

        internal static class FMsListColumns
        {
            internal static string Game = "Game";
            internal static string Installed = "Installed";
            internal static string Title = "Title";
            internal static string Archive = "Archive";
            internal static string Author = "Author";
            internal static string Size = "Size";
            internal static string Rating = "Rating";
            internal static string Finished = "Finished";
            internal static string ReleaseDate = "Release Date";
            internal static string LastPlayed = "Last Played";
            internal static string DisabledMods = "Disabled Mods";
            internal static string Comment = "Comment";
        }

        internal static string FMsListAllModsDisabledMessage = "* [All]";

        internal static class FMContextMenu
        {
            internal static string PlayFM = "Play FM";
            internal static string InstallFM = "Install FM";
            internal static string UninstallFM = "Uninstall FM";
            internal static string Rating = "Rating";
            internal static string FinishedOn = "Finished on";
            internal static string Tasks = "Tasks";
        }

        internal static class TasksMenu
        {
            internal static string ConvertWAVsTo16Bit = "Convert .wav files to 16 bit";
            internal static string ConvertOGGsToWAVs = "Convert .ogg files to .wav";
        }

        internal static class PlayOriginalGameMenu
        {
            internal static string Thief1 = "Thief 1";
            internal static string Thief2 = "Thief 2";
            internal static string Thief3 = "Thief 3";
        }

        internal static class StatisticsTab
        {
            internal static string TabText = "Statistics";

            internal static string CustomResources = "Custom resources:";
            internal static string CustomResourcesNotScanned = "Custom resources not scanned.";
            internal static string CustomResourcesNotSupportedForThief3 = "Custom resource detection is not supported for Thief 3 FMs.";
            internal static string NoFMSelected = "No FM selected.";

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
        }

        internal static class EditFMTab
        {
            internal static string TabText = "Edit FM";

            internal static string Title = "Title:";
            internal static string Author = "Author:";
            internal static string ReleaseDate = "Release date:";
            internal static string LastPlayed = "Last played:";
            internal static string Rating = "Rating:";
            internal static string FinishedOn = "Finished on...";
            internal static string DisabledMods = "Disabled mods:";
            internal static string DisableAllMods = "Disable all mods";
        }

        internal static class CommentTab
        {
            internal static string TabText = "Comment";
        }

        internal static class TagsTab
        {
            internal static string TabText = "Tags";
            internal static string AddTag = "Add tag";
            internal static string AddFromList = "Add from list...";
            internal static string RemoveTag = "Remove tag";
        }

        internal static class ReadmeArea
        {
            internal static string ViewHTMLReadme = "View HTML Readme";
            internal static string ResetZoomToolTip = "Reset zoom";
            internal static string FullScreenToolTip = "Full screen";
            internal static string ExitFullScreenToolTip = "Exit full screen";

            internal static string NoReadmeFound = "No readme found.";
            internal static string UnableToLoadReadme = "Unable to load this readme.";
        }

        internal static class BottomArea
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
        }

        // Better hold off on this until the Settings window is more or less finalized
        internal static class SettingsWindow
        {
            internal static string TitleText = "Settings";
            internal static string StartupTitleText = "AngelLoader Initial Setup";

            internal static class PathsTab
            {
                internal static string TabText = "Paths";

                internal static string PathsToGameExes = "Paths to game executables";
                internal static string Thief1 = "Thief 1:";
                internal static string Thief2 = "Thief 2:";
                internal static string Thief3 = "Thief 3:";
                internal static string Other = "Other";
                internal static string BackupPath = "Backup path for saves and screenshots:";
                internal static string FMArchivePaths = "FM archive paths";
                internal static string IncludeSubfolders = "Include subfolders";

                internal static string AddArchivePathToolTip = "Add archive path...";
                internal static string RemoveArchivePathToolTip = "Remove selected archive path";

                internal static string ErrorSomePathsAreInvalid = "Some paths are invalid.";
            }

            internal static class FMDisplayTab
            {
                internal static string TabText = "FM Display";

                internal static string GameOrganization = "Game organization";
                internal static string GameOrganizationByTab = "Each game in its own tab";
                internal static string GameOrganizationOneList = "Everything in one list, and games are filters";

                internal static string Sorting = "Sorting";

                internal static string IgnoreArticles = "Ignore the following leading articles when sorting by title:";

                internal static string DefaultArticles = "a, an, the";

                internal static string MoveArticlesToEnd = "Move articles to the end of names when displaying them";

                internal static string RatingDisplayStyle = "Rating display style";
                internal static string RatingDisplayStyleNDL = "NewDarkLoader (0-10 in increments of 1)";
                internal static string RatingDisplayStyleFMSel = "FMSel (0-5 in increments of 0.5)";
                internal static string RatingDisplayStyleUseStars = "Use stars";

                internal static string DateFormat = "Date format";
                internal static string CurrentCultureShort = "Current culture short";
                internal static string CurrentCultureLong = "Current culture long";
                internal static string Custom = "Custom:";

                internal static string ErrorInvalidDateFormat = "Invalid date format.";
                internal static string ErrorDateOutOfRange = "The date and time is outside the range of dates supported by the calendar used by the current culture.";
            }

            internal static class OtherTab
            {
                internal static string TabText = "Other";

                internal static string FMFileConversion = "FM file conversion";
                internal static string ConvertWAVsTo16BitOnInstall = "Convert .wavs to 16 bit on install";
                internal static string ConvertOGGsToWAVsOnInstall = "Convert .oggs to .wavs on install";

                internal static string BackUpSaves = "Back up saves and screenshots when uninstalling";
                internal static string BackUpAlwaysAsk = "Always ask";
                internal static string BackUpAlwaysBackUp = "Always back up";

                internal static string Language = "Language";

                internal static string LanguageTakeEffectNote = "This selection will take effect when you click OK.";

                internal static string WebSearch = "Web search";
                internal static string WebSearchURL = "Full URL to use when searching for an FM title:";
                internal static string WebSearchTitleVar = "$TITLE$ : the title of the FM";
                internal static string WebSearchResetToolTip = "Reset to default";
            }
        }

        internal static class DateFilterBox
        {
            internal static string ReleaseDateTitleText = "Set release date filter";
            internal static string LastPlayedTitleText = "Set last played filter";

            internal static string From = "From:";
            internal static string To = "To:";
            internal static string NoMinimum = "(no minimum)";
            internal static string NoMaximum = "(no maximum)";
        }

        internal static class SetRatingFilterBox
        {
            internal static string TitleText = "Set rating filter";
            internal static string From = "From:";
            internal static string To = "To:";
        }
    }

    internal static class LText
    {
        internal static class Global
        {
            internal static string OK = "█OK";
            internal static string Cancel = "█Cancel";
            internal static string BrowseEllipses = "█Browse...";
            internal static string Add = "█Add";
            internal static string AddEllipses = "█Add...";
            internal static string Remove = "█Remove";
            internal static string RemoveEllipses = "█Remove...";
            internal static string Reset = "█Reset";

            internal static string Unrated = "█Unrated";
            internal static string None = "█None";
            internal static string CustomTagInCategory = "█<custom>";

            internal static string Error = "█Error";

            internal static string KilobyteShort = "█KB";
            internal static string MegabyteShort = "█MB";
            internal static string GigabyteShort = "█GB";
        }

        internal static class BrowseDialogs
        {
            internal static string AllFiles = "█All files (*.*)";
            internal static string ExeFiles = "█Executable files (*.exe)";
            internal static string IniFiles = "█ini files (*.ini)";
        }

        internal static class AlertMessages
        {
            internal static string Alert = "█Alert";
            internal static string WebSearchURLIsInvalid = "█The specified site to search is not a valid URL.";
        }

        internal static class Difficulties
        {
            internal static string Easy = "█Easy";
            internal static string Normal = "█Normal";
            internal static string Hard = "█Hard";
            internal static string Expert = "█Expert";
            internal static string Extreme = "█Extreme";
        }

        internal static class GameTabs
        {
            internal static string Thief1 = "█Thief 1";
            internal static string Thief2 = "█Thief 2";
            internal static string Thief3 = "█Thief 3";
        }

        internal static class FilterBar
        {
            internal static string Thief1ToolTip = "█Thief 1";
            internal static string Thief2ToolTip = "█Thief 2";
            internal static string Thief3ToolTip = "█Thief 3";

            internal static string Title = "█Title:";
            internal static string Author = "█Author:";

            internal static string ReleaseDateToolTip = "█Release date";
            internal static string LastPlayedToolTip = "█Last played";
            internal static string TagsToolTip = "█Tags";
            internal static string FinishedToolTip = "█Finished";
            internal static string UnfinishedToolTip = "█Unfinished";
            internal static string RatingToolTip = "█Rating";

            internal static string ShowJunk = "█Show junk";
        }

        internal static string RefreshFilteredListButtonToolTip = "█Refresh filtered list";
        internal static string ClearFiltersButtonToolTip = "█Clear filters";
        internal static string ResetLayoutButtonToolTip = "█Reset layout";

        internal static class FMsListColumns
        {
            internal static string Game = "█Game";
            internal static string Installed = "█Installed";
            internal static string Title = "█Title";
            internal static string Archive = "█Archive";
            internal static string Author = "█Author";
            internal static string Size = "█Size";
            internal static string Rating = "█Rating";
            internal static string Finished = "█Finished";
            internal static string ReleaseDate = "█Release Date";
            internal static string LastPlayed = "█Last Played";
            internal static string DisabledMods = "█Disabled Mods";
            internal static string Comment = "█Comment";
        }

        internal static string FMsListAllModsDisabledMessage = "█* [All]";

        internal static class FMContextMenu
        {
            internal static string PlayFM = "█Play FM";
            internal static string InstallFM = "█Install FM";
            internal static string UninstallFM = "█Uninstall FM";
            internal static string Rating = "█Rating";
            internal static string FinishedOn = "█Finished on";
            internal static string Tasks = "█Tasks";
        }

        internal static class TasksMenu
        {
            internal static string ConvertWAVsTo16Bit = "█Convert .wav files to 16 bit";
            internal static string ConvertOGGsToWAVs = "█Convert .ogg files to .wav";
        }

        internal static class PlayOriginalGameMenu
        {
            internal static string Thief1 = "█Thief 1";
            internal static string Thief2 = "█Thief 2";
            internal static string Thief3 = "█Thief 3";
        }

        internal static class StatisticsTab
        {
            internal static string TabText = "█Statistics";

            internal static string CustomResources = "█Custom resources:";
            internal static string CustomResourcesNotScanned = "█Custom resources not scanned.";
            internal static string CustomResourcesNotSupportedForThief3 = "█Custom resource detection is not supported for Thief 3 FMs.";
            internal static string NoFMSelected = "█No FM selected.";

            internal static string Map = "█Map";
            internal static string Automap = "█Automap";
            internal static string Textures = "█Textures";
            internal static string Sounds = "█Sounds";
            internal static string Movies = "█Movies";
            internal static string Objects = "█Objects";
            internal static string Creatures = "█Creatures";
            internal static string Motions = "█Motions";
            internal static string Scripts = "█Scripts";
            internal static string Subtitles = "█Subtitles";
        }

        internal static class EditFMTab
        {
            internal static string TabText = "█Edit FM";

            internal static string Title = "█Title:";
            internal static string Author = "█Author:";
            internal static string ReleaseDate = "█Release date:";
            internal static string LastPlayed = "█Last played:";
            internal static string Rating = "█Rating:";
            internal static string FinishedOn = "█Finished on...";
            internal static string DisabledMods = "█Disabled mods:";
            internal static string DisableAllMods = "█Disable all mods";
        }

        internal static class CommentTab
        {
            internal static string TabText = "█Comment";
        }

        internal static class TagsTab
        {
            internal static string TabText = "█Tags";
            internal static string AddTag = "█Add tag";
            internal static string AddFromList = "█Add from list...";
            internal static string RemoveTag = "█Remove tag";
        }

        internal static class ReadmeArea
        {
            internal static string ViewHTMLReadme = "█View HTML Readme";
            internal static string ResetZoomToolTip = "█Reset zoom";
            internal static string FullScreenToolTip = "█Full screen";
            internal static string ExitFullScreenToolTip = "█Exit full screen";

            internal static string NoReadmeFound = "█No readme found.";
            internal static string UnableToLoadReadme = "█Unable to load this readme.";
        }

        internal static class BottomArea
        {
            internal static string PlayFM = "█Play FM";
            internal static string InstallFM = "█Install FM";
            internal static string UninstallFM = "█Uninstall FM";
            internal static string PlayOriginalGame = "█Play original game...";
            internal static string WebSearch = "█Web search";
            internal static string ScanAllFMs = "█Scan all FMs...";
            internal static string Import = "█Import from...";
            internal static string Settings = "█Settings...";
        }

        internal static class ProgressBox
        {
            internal static string Scanning = "█Scanning...";
            internal static string InstallingFM = "█Installing FM...";
            internal static string UninstallingFM = "█Uninstalling FM...";
            internal static string ConvertingFiles = "█Converting files...";
            internal static string CheckingInstalledFMs = "█Checking installed FMs...";
            internal static string ReportScanningFirst = "█Scanning ";
            internal static string ReportScanningBetweenNumAndTotal = "█ of ";
            internal static string ReportScanningLast = "█...";
            internal static string CancelingInstall = "█Canceling install...";
        }

        // Better hold off on this until the Settings window is more or less finalized
        internal static class SettingsWindow
        {
            internal static string TitleText = "█Settings";
            internal static string StartupTitleText = "█AngelLoader Initial Setup";

            internal static class PathsTab
            {
                internal static string TabText = "█Paths";

                internal static string PathsToGameExes = "█Paths to game executables";
                internal static string Thief1 = "█Thief 1:";
                internal static string Thief2 = "█Thief 2:";
                internal static string Thief3 = "█Thief 3:";
                internal static string Other = "█Other";
                internal static string BackupPath = "█Backup path for saves and screenshots:";
                internal static string FMArchivePaths = "█FM archive paths";
                internal static string IncludeSubfolders = "█Include subfolders";

                internal static string AddArchivePathToolTip = "█Add archive path...";
                internal static string RemoveArchivePathToolTip = "█Remove selected archive path";

                internal static string ErrorSomePathsAreInvalid = "█Some paths are invalid.";
            }

            internal static class FMDisplayTab
            {
                internal static string TabText = "█FM Display";

                internal static string GameOrganization = "█Game organization";
                internal static string GameOrganizationByTab = "█Each game in its own tab";
                internal static string GameOrganizationOneList = "█Everything in one list, and games are filters";

                internal static string Sorting = "█Sorting";

                internal static string IgnoreArticles = "█Ignore the following leading articles when sorting by title:";

                internal static string DefaultArticles = "█a, an, the";

                internal static string MoveArticlesToEnd = "█Move articles to the end of names when displaying them";

                internal static string RatingDisplayStyle = "█Rating display style";
                internal static string RatingDisplayStyleNDL = "█NewDarkLoader (0-10 in increments of 1)";
                internal static string RatingDisplayStyleFMSel = "█FMSel (0-5 in increments of 0.5)";
                internal static string RatingDisplayStyleUseStars = "█Use stars";

                internal static string DateFormat = "█Date format";
                internal static string CurrentCultureShort = "█Current culture short";
                internal static string CurrentCultureLong = "█Current culture long";
                internal static string Custom = "█Custom:";

                internal static string ErrorInvalidDateFormat = "█Invalid date format.";
                internal static string ErrorDateOutOfRange = "█The date and time is outside the range of dates supported by the calendar used by the current culture.";
            }

            internal static class OtherTab
            {
                internal static string TabText = "█Other";

                internal static string FMFileConversion = "█FM file conversion";
                internal static string ConvertWAVsTo16BitOnInstall = "█Convert .wavs to 16 bit on install";
                internal static string ConvertOGGsToWAVsOnInstall = "█Convert .oggs to .wavs on install";

                internal static string BackUpSaves = "█Back up saves and screenshots when uninstalling";
                internal static string BackUpAlwaysAsk = "█Always ask";
                internal static string BackUpAlwaysBackUp = "█Always back up";

                internal static string Language = "█Language";

                internal static string LanguageTakeEffectNote = "█This selection will take effect when you click OK.";

                internal static string WebSearch = "█Web search";
                internal static string WebSearchURL = "█Full URL to use when searching for an FM title:";
                internal static string WebSearchTitleVar = "█$TITLE$ : the title of the FM";
                internal static string WebSearchResetToolTip = "█Reset to default";
            }
        }

        internal static class DateFilterBox
        {
            internal static string ReleaseDateTitleText = "█Set release date filter";
            internal static string LastPlayedTitleText = "█Set last played filter";

            internal static string From = "█From:";
            internal static string To = "█To:";
            internal static string NoMinimum = "█(no minimum)";
            internal static string NoMaximum = "█(no maximum)";
        }

        internal static class SetRatingFilterBox
        {
            internal static string TitleText = "█Set rating filter";
            internal static string From = "█From:";
            internal static string To = "█To:";
        }
    }
}
