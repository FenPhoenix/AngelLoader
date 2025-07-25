﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
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
        CurrentFM,
    }

    public enum SortDirection
    {
        Ascending,
        Descending,
    }

    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized,
    }

    public enum MBoxIcon
    {
        None,
        Error,
        Warning,
        Information,
    }

    public enum MBoxButtons
    {
        OK,
        OKCancel,
        YesNoCancel,
        YesNo,
    }

    public enum MBoxButton
    {
        Yes,
        No,
        Cancel,
    }

    public enum ProgressSizeMode
    {
        Single,
        Double,
    }

    public enum ProgressType
    {
        Determinate,
        Indeterminate,
    }

    public enum ImportType
    {
        DarkLoader,
        FMSel,
        NewDarkLoader,
    }

    /*
    Wri is a "secret" format - it's a first-class type internally, but we don't document our support for
    it because it's really just a hack for Lucrative Opportunity and the format is not supported in full.
    Any other .wri files besides the one from Lucrative Opportunity are likely to be displayed incorrectly.
    And we don't want to give anyone any ideas to use this format (not like any modern program I know of
    can even write the .wri format, but still).
    */
    public enum ReadmeType { PlainText, RichText, HTML, GLML, Wri }

    public enum ReadmeLocalizableMessage { None, NoReadmeFound, UnableToLoadReadme }

    internal enum AudioConvert { MP3ToWAV, OGGToWAV, WAVToWAV16 }

    // Non-consts for file size; these aren't perf-critical at all
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class HelpSections
    {
        internal static readonly string InitialSettings = "#initial_setup";

        internal static readonly string[] SettingsPages = RunFunc(static () =>
        {
            string[] array = new string[SettingsTabCount];
            array[(int)SettingsTab.Paths] = "#settings_paths_section";
            array[(int)SettingsTab.Appearance] = "#settings_appearance_section";
            array[(int)SettingsTab.AudioFiles] = "#settings_audio_files_section";
            array[(int)SettingsTab.Other] = "#settings_other_section";
            array[(int)SettingsTab.ThiefBuddy] = "#settings_thief_buddy_section";
            array[(int)SettingsTab.Update] = "#settings_update_section";
            array[(int)SettingsTab.IOThreading] = "#settings_io_threading_section";

            Utils.AssertR(array.All(static x => x != null), nameof(SettingsPages) + " is missing at least one item");

            return array;
        });

        internal static readonly string MainWindow = "#main_window";
        internal static readonly string MissionList = "#mission_list";
        internal static readonly string ColumnHeaderContextMenu = "#column_header_context_menu";
        internal static readonly string FMContextMenu = "#fm_context_menu";

#pragma warning disable IDE0300 // Simplify collection initialization
        [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
        private static readonly string[] FMTabs = new string[FMTabCount]
        {
            "#stats_tab",
            "#edit_fm_tab",
            "#comment_tab",
            "#tags_tab",
            "#patch_tab",
            "#mods_tab",
            "#screenshots_tab",
        };
#pragma warning restore IDE0300 // Simplify collection initialization

        internal static string GetFMTab(FMTab fmTab) => FMTabs[(int)fmTab];

        internal static readonly string ReadmeArea = "#readme_area";

        internal static readonly string ScanAllFMs = "#scan_all_fms";

        internal static readonly string MainMenu = "#main_menu";

        internal static readonly string GameVersions = "#game_versions";
    }

    #endregion

    internal static readonly SortDirection[] ColumnDefaultSortDirections = new SortDirection[ColumnCount];

    internal static readonly int CoreCount = Environment.ProcessorCount.ClampToMin(1);

    static Misc()
    {
        ColumnDefaultSortDirections[(int)Column.ReleaseDate] = SortDirection.Descending;
        ColumnDefaultSortDirections[(int)Column.LastPlayed] = SortDirection.Descending;
        ColumnDefaultSortDirections[(int)Column.DateAdded] = SortDirection.Descending;
        ColumnDefaultSortDirections[(int)Column.PlayTime] = SortDirection.Descending;
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
        internal const float BottomSplitterPercent = 0.741f;

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
            "https://www.google.com/search?q=\"$TITLE$\" site:thedarkmod.com",
        });

        internal const uint DaysRecent = 15;
        internal const uint MaxDaysRecent = 99999;
    }

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
        ".tiff",
    };

    // ReSharper disable once RedundantExplicitArraySize
    public static readonly Func<string>[] FMTabTextLocalizedStrings = new Func<string>[FMTabCount]
    {
        static () => LText.StatisticsTab.TabText,
        static () => LText.EditFMTab.TabText,
        static () => LText.CommentTab.TabText,
        static () => LText.TagsTab.TabText,
        static () => LText.PatchTab.TabText,
        static () => LText.ModsTab.TabText,
        static () => LText.ScreenshotsTab.TabText,
    };

    // ReSharper disable once RedundantExplicitArraySize
    public static readonly Func<string>[] ColumnLocalizedStrings = new Func<string>[ColumnCount]
    {
#if DateAccTest
        static () => "Date accuracy",
#endif
        static () => LText.FMsList.GameColumn,
        static () => LText.FMsList.InstalledColumn,
        static () => LText.FMsList.MissionCountColumn,
        static () => LText.FMsList.TitleColumn,
        static () => LText.FMsList.ArchiveColumn,
        static () => LText.FMsList.AuthorColumn,
        static () => LText.FMsList.SizeColumn,
        static () => LText.FMsList.RatingColumn,
        static () => LText.FMsList.FinishedColumn,
        static () => LText.FMsList.ReleaseDateColumn,
        static () => LText.FMsList.LastPlayedColumn,
        static () => LText.FMsList.DateAddedColumn,
        static () => LText.FMsList.PlayTimeColumn,
        static () => LText.FMsList.DisabledModsColumn,
        static () => LText.FMsList.CommentColumn,
    };

    // ReSharper disable once RedundantExplicitArraySize
    public static readonly Func<string>[] CustomResourceLocalizedStrings = new Func<string>[CustomResourcesCount - 1]
    {
        static () => LText.StatisticsTab.Map,
        static () => LText.StatisticsTab.Automap,
        static () => LText.StatisticsTab.Scripts,
        static () => LText.StatisticsTab.Textures,
        static () => LText.StatisticsTab.Sounds,
        static () => LText.StatisticsTab.Objects,
        static () => LText.StatisticsTab.Creatures,
        static () => LText.StatisticsTab.Motions,
        static () => LText.StatisticsTab.Movies,
        static () => LText.StatisticsTab.Subtitles,
    };

    [StructLayout(LayoutKind.Auto)]
    public readonly struct PerGameGoFlags
    {
        private readonly bool[] _array;

        /// <summary>
        /// The internal array starts out all <see langword="false"/>.
        /// </summary>
        public PerGameGoFlags() => _array = new bool[SupportedGameCount];

        public bool this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public static PerGameGoFlags AllTrue()
        {
            PerGameGoFlags ret = new();
            for (int i = 0; i < SupportedGameCount; i++)
            {
                ret[i] = true;
            }
            return ret;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct NonEmptyList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;

        private NonEmptyList(List<T> list)
        {
            _list = list;
            Count = _list.Count;
            Single = Count == 1;
        }

        public readonly int Count;

        public readonly bool Single;

        /// <summary>
        /// The internal list will be a reference copy of what you pass in. Use this method if you know the source
        /// list won't change, and you want to avoid the extra allocation.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [MustUseReturnValue]
        public static bool TryCreateFrom_Ref(List<T>? list, out NonEmptyList<T> result)
        {
            if (list?.Count > 0)
            {
                result = new NonEmptyList<T>(list);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

#if false
        /// <summary>
        /// The internal list will be an element-wise copy of what you pass in. Use this method if the source
        /// might change and you want to guarantee the internal list won't, at the expense of the extra allocation.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [MustUseReturnValue]
        public static bool TryCreateFrom_Copy(List<T>? list, out NonEmptyList<T> result)
        {
            if (list?.Count > 0)
            {
                List<T> copy = new(list.Count);
                copy.AddRange(list);
                result = new NonEmptyList<T>(copy);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// The internal list will be an element-wise copy of what you pass in. Use this method if the source
        /// might change and you want to guarantee the internal list won't, at the expense of the extra allocation.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [MustUseReturnValue]
        public static bool TryCreateFrom_Copy(T[]? array, out NonEmptyList<T> result)
        {
            if (array?.Length > 0)
            {
                result = new NonEmptyList<T>(array.ToList());
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
#endif

        [MustUseReturnValue]
        public static NonEmptyList<T> CreateFrom(T item) => new(new List<T>(1) { item });

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[index];
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class ThreadingData
    {
        internal readonly int Threads;
        internal readonly IOThreadingLevel Level;

        public ThreadingData(int threads, IOThreadingLevel level)
        {
            Threads = threads.ClampToMin(1);
            Level = level;
        }

        public override string ToString() =>
            "{ " + nameof(Threads) + ": " + Threads.ToStrInv() + ", " + nameof(Level) + ": " + Level + " }";
    }

    public enum IOPathType
    {
        File,
        Directory,
    }

    public sealed class SettingsDriveData
    {
        public readonly string OriginalPath;
        public string Root = "";
        public DriveMultithreadingLevel MultithreadingLevel = DriveMultithreadingLevel.Single;
        public string ModelName = "";

        public SettingsDriveData(string originalPath)
        {
            OriginalPath = originalPath;
        }

        public override string ToString()
        {
            return "----" + $"{NL}" +
                   nameof(OriginalPath) + ": " + OriginalPath + $"{NL}" +
                   nameof(Root) + ": " + Root + $"{NL}" +
                   nameof(MultithreadingLevel) + ": " + MultithreadingLevel + $"{NL}";
        }
    }

    public sealed class ThreadablePath
    {
        public readonly string OriginalPath;
        public string Root = "";
        public readonly IOPathType IOPathType;
        public DriveMultithreadingLevel DriveMultithreadingLevel = DriveMultithreadingLevel.Single;
        public readonly ThreadablePathType ThreadablePathType;
        /// <summary>
        /// Only used if <see cref="T:ThreadablePathType"/> is <see cref="ThreadablePathType.FMInstallPath"/>.
        /// Otherwise its value should be ignored.
        /// </summary>
        public readonly GameIndex GameIndex;

        public ThreadablePath(string originalPath, IOPathType ioPathType, ThreadablePathType threadablePathType)
        {
            OriginalPath = originalPath;
            IOPathType = ioPathType;
            ThreadablePathType = threadablePathType;
            GameIndex = default;
        }

        public ThreadablePath(string originalPath, IOPathType ioPathType, ThreadablePathType threadablePathType, GameIndex gameIndex)
        {
            OriginalPath = originalPath;
            IOPathType = ioPathType;
            ThreadablePathType = threadablePathType;
            GameIndex = gameIndex;
        }

        public override string ToString()
        {
            return "----" + $"{NL}" +
                   nameof(OriginalPath) + ": " + OriginalPath + $"{NL}" +
                   nameof(Root) + ": " + Root + $"{NL}" +
                   nameof(IOPathType) + ": " + IOPathType + $"{NL}" +
                   nameof(DriveMultithreadingLevel) + ": " + DriveMultithreadingLevel + $"{NL}" +
                   nameof(ThreadablePathType) + ": " + ThreadablePathType + $"{NL}" +
                   nameof(GameIndex) + ": " + GameIndex + $"{NL}";
        }
    }

    [PublicAPI]
    public sealed class DriveLetterDictionary
    {
        private Dictionary<char, DriveMultithreadingLevel> _dict;

        public DriveLetterDictionary() => _dict = new Dictionary<char, DriveMultithreadingLevel>();

        public DriveLetterDictionary(int capacity) => _dict = new Dictionary<char, DriveMultithreadingLevel>(capacity);

        public int Count => _dict.Count;

        public DriveMultithreadingLevel this[char key]
        {
            get => _dict[key.ToAsciiUpper()];
            set
            {
                if (key.IsAsciiAlpha())
                {
                    _dict[key.ToAsciiUpper()] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DriveMultithreadingLevel AddAndReturn(char key, DriveMultithreadingLevel value)
        {
            return _dict.AddAndReturn(key.ToAsciiUpper(), value);
        }

        public void Add(char key, DriveMultithreadingLevel value)
        {
            if (key.IsAsciiAlpha())
            {
                _dict.Add(key.ToAsciiUpper(), value);
            }
        }

        public bool ContainsKey(char key) => _dict.ContainsKey(key.ToAsciiUpper());

        public bool Remove(char key) => _dict.Remove(key.ToAsciiUpper());

        public bool TryGetValue(char key, out DriveMultithreadingLevel value) => _dict.TryGetValue(key.ToAsciiUpper(), out value);

        public void CopyTo_NoClearDest(DriveLetterDictionary dest)
        {
            foreach (var item in _dict)
            {
                dest[item.Key.ToAsciiUpper()] = item.Value;
            }
        }

        public KeyValuePair<char, DriveMultithreadingLevel>[] ToArray_SortedByKey()
        {
            return _dict.OrderBy(static x => x.Key).ToArray();
        }
    }
}
