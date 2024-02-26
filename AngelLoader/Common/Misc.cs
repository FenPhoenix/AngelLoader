using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

public static partial class Misc
{
    //internal const int AppConfigVersion = 1;
    // Commented so that old versions of AngelLoader will ignore it
    //internal const string ConfigVersionHeader = ";@Version:";

    #region Enums and enum-like

    public enum TDM_FileChanged
    {
        MissionInfo,
        CurrentFM
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized
    }

    public enum MBoxIcon
    {
        None,
        Error,
        Warning,
        Information
    }

    public enum MBoxButtons
    {
        OK,
        OKCancel,
        YesNoCancel,
        YesNo
    }

    public enum MBoxButton
    {
        Yes,
        No,
        Cancel
    }

    public enum ProgressSizeMode
    {
        Single,
        Double
    }

    public enum ProgressType
    {
        Determinate,
        Indeterminate
    }

    public enum ImportType
    {
        DarkLoader,
        FMSel,
        NewDarkLoader
    }

    /*
    Wri is a "secret" format - it's a first-class type internally, but we don't document our support for
    it because it's really just a hack for Lucrative Opportunity and the format is not supported in full.
    Any other .wri files besides the one from Lucrative Opportunity are likely to be displayed incorrectly.
    And we don't want to give anyone any ideas to use this format (not like any modern program I know of
    can even write the .wri format, but still).
    */
    public enum ReadmeType { PlainText, RichText, HTML, GLML, Wri }

    public enum ReadmeState { HTML, PlainText, OtherSupported, LoadError, InitialReadmeChooser }

    public enum ReadmeLocalizableMessage { None, NoReadmeFound, UnableToLoadReadme }

    internal enum AudioConvert { MP3ToWAV, OGGToWAV, WAVToWAV16 }

    // Non-consts for file size; these aren't perf-critical at all
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class HelpSections
    {
        internal static readonly string InitialSettings = "#initial_setup";

#pragma warning disable IDE0300 // Simplify collection initialization
        [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
        internal static readonly string[] SettingsPages = new string[SettingsTabCount]
        {
            "#settings_paths_section",
            "#settings_appearance_section",
            "#settings_other_section",
            "#settings_thief_buddy_section",
            "#settings_update_section"
        };
#pragma warning restore IDE0300 // Simplify collection initialization

        internal static readonly string MainWindow = "#main_window";
        internal static readonly string MissionList = "#mission_list";
        internal static readonly string ColumnHeaderContextMenu = "#column_header_context_menu";
        internal static readonly string FMContextMenu = "#fm_context_menu";

        internal static readonly string StatsTab = "#stats_tab";
        internal static readonly string EditFMTab = "#edit_fm_tab";
        internal static readonly string CommentTab = "#comment_tab";
        internal static readonly string TagsTab = "#tags_tab";
        internal static readonly string PatchTab = "#patch_tab";
        internal static readonly string ModsTab = "#mods_tab";

        internal static readonly string ReadmeArea = "#readme_area";

        internal static readonly string ScanAllFMs = "#scan_all_fms";

        internal static readonly string MainMenu = "#main_menu";

        internal static readonly string GameVersions = "#game_versions";
    }

    #endregion

    internal static readonly SortDirection[] ColumnDefaultSortDirections = new SortDirection[ColumnCount];

    static Misc()
    {
        ColumnDefaultSortDirections[(int)Column.LastPlayed] = SortDirection.Descending;
    }

    internal static readonly ReadOnlyCollection<string> ValidDateFormats =
        new(new[] { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" });

    internal static readonly Action NullAction = static () => { };

    internal static readonly Task VoidTask = Task.CompletedTask;

    internal static class Defaults
    {
        #region Main window

        internal static readonly Size MainWindowSize = new Size(1280, 720);

        internal static readonly Point MainWindowLocation = new Point(50, 50);

        internal const int ColumnWidth = 100;
        internal const int MinColumnWidth = 25;

        internal const float TopSplitterPercent = 0.741f;
        internal const float MainSplitterPercent = 0.4425f;

        // @NET5: Remember to change this to match the new font
        internal const float FMsListFontSizeInPoints = 8.25f;

        #endregion

        #region Settings window

        internal static readonly Size SettingsWindowSize = new Size(710, 708);
        internal const int SettingsWindowSplitterDistance = 155;

        #endregion

        internal const string DateCustomFormat1 = "dd";
        internal const string DateCustomSeparator1 = "/";
        internal const string DateCustomFormat2 = "MM";
        internal const string DateCustomSeparator2 = "/";
        internal const string DateCustomFormat3 = "yyyy";
        internal const string DateCustomSeparator3 = "";
        internal const string DateCustomFormat4 = "";

        [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
        internal static readonly ReadOnlyCollection<string> WebSearchUrls = new(new string[SupportedGameCount]
        {
            "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com",
            "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com",
            "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com",
            "https://www.google.com/search?q=\"$TITLE$\" site:systemshock.org",
            "https://www.google.com/search?q=\"$TITLE$\" site:thedarkmod.com"
        });

        internal const uint DaysRecent = 15;
        internal const uint MaxDaysRecent = 99999;
    }

    internal const int StreamCopyBufferSize = 81920;
    internal const int FileStreamBufferSize = 4096;

    // Another leak of view implementation details into here (GDI+/Bitmap supported formats)
    // @ScreenshotDisplay: NewDark games can also have .pcx, and TDM can also have .tga
    // Neither are supported by Bitmap, so, you're kinda outta luck on those.
    public static readonly string[] SupportedScreenshotExtensions =
    {
        // Common/likely ones first
        ".png",
        ".bmp",
        ".jpg",
        ".jpeg",
        ".gif",
        ".tif",
        ".tiff"
    };

    public sealed class FMInstalledDirModificationScope : IDisposable
    {
        /*
        IMPORTANT! @THREADING(FMInstalledDirModificationScope):
        If we ever make it so that things can go in parallel (install/uninstall, scan, delete, etc.), this will
        no longer be safe! We're threading noobs so we don't know if volatile will solve the problem or what.
        Needs testing.
        */
        private static int _count;

        private readonly bool[] _originalValues = new bool[SupportedGameCount];

        public FMInstalledDirModificationScope()
        {
            if (_count == 0)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    ScreenshotWatcher watcher = Config.GetScreenshotWatcher(gameIndex);
                    _originalValues[i] = watcher.EnableWatching;
                    watcher.EnableWatching = false;
                }
            }
            _count++;
        }

        public void Dispose()
        {
            if (_count == 1)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    ScreenshotWatcher watcher = Config.GetScreenshotWatcher(gameIndex);
                    watcher.EnableWatching = _originalValues[i];
                }
            }
            _count = (_count - 1).ClampToZero();
        }
    }
}
