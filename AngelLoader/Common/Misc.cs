﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;

namespace AngelLoader
{
    public static partial class Misc
    {
        // ReSharper disable once ConvertToConstant.Global
        internal static readonly string AppGuid = "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader";

        internal const int AppConfigVersion = 1;
        // Commented so that old versions of AngelLoader will ignore it
        internal const string ConfigVersionHeader = ";@Version:";

        internal static readonly int ColumnsCount = Enum.GetValues(typeof(Column)).Length;
        internal static readonly int HideableFilterControlsCount = Enum.GetValues(typeof(HideableFilterControls)).Length;
        public static readonly int ZoomTypesCount = Enum.GetValues(typeof(Zoom)).Length;

        #region Enums and enum-like

        public enum Zoom { In, Out, Reset }

        // Public for param accessibility reasons or whatever
        public enum ProgressTask
        {
            FMScan,
            InstallFM,
            UninstallFM,
            ConvertFiles,
            ImportFromDarkLoader,
            ImportFromNDL,
            ImportFromFMSel,
            CacheFM,
            DeleteFMArchive
        }

        // Has to be public so it can be passed to a public constructor on a form
        public enum ImportType
        {
            DarkLoader,
            FMSel,
            NewDarkLoader
        }

        public enum Direction { Left, Right, Up, Down }

        internal enum ReadmeType { PlainText, RichText, HTML, GLML }

        internal enum AudioConvert { MP3ToWAV, OGGToWAV, WAVToWAV16 }

        /// <summary>
        /// Set a control's tag to this to tell the darkable control dictionary filler to ignore it if True.
        /// </summary>
        public enum LazyLoaded { True, False }

        // Non-consts for file size; these aren't perf-critical at all
        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        internal static class HelpSections
        {
            internal static readonly string InitialSettings = "#initial_setup";
            internal static readonly string PathsSettings = "#settings_paths_section";
            internal static readonly string AppearanceSettings = "#settings_appearance_section";
            internal static readonly string OtherSettings = "#settings_other_section";

            //internal static readonly string FMBackupPath = "#fm_backup_path";

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

        public class SOH2 : HashSet<string>
        {
            private readonly List<string> _list;

            public SOH2() : base(StringComparer.OrdinalIgnoreCase)
            {
                _list = new List<string>();
            }

            public SOH2(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
            {
                _list = new List<string>(capacity);
            }

            public new void Add(string tag)
            {
                if (base.Add(tag))
                {
                    _list.Add(tag.ToLowerInvariant());
                }
            }

            public new void Remove(string category)
            {
                base.Remove(category);
                _list.Remove(category.ToLowerInvariant());
            }

            public void RemoveAt(int index)
            {
                string item = _list[index];
                base.Remove(item);
            }

            public string this[int index] => _list[index];

            public void SortCaseInsensitive()
            {
                _list.Sort(StringComparer.OrdinalIgnoreCase);
            }
        }

        public class SOD2 : Dictionary<string, SOH2>
        {
            private readonly List<string> _list;

            public SOD2() : base(StringComparer.OrdinalIgnoreCase)
            {
                _list = new List<string>();
            }

            public SOD2(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
            {
                _list = new List<string>(capacity);
            }

            public new void Add(string category, SOH2 tags)
            {
                if (!base.ContainsKey(category))
                {
                    base[category] = tags;
                    _list.Add(category.ToLowerInvariant());
                }
            }

            public new bool Remove(string category)
            {
                _list.Remove(category.ToLowerInvariant());
                return base.Remove(category);
            }

            public bool RemoveAt(int index)
            {
                string item = _list[index];
                return base.Remove(item);
            }

            public string this[int index] => _list[index];

            public new void Clear()
            {
                base.Clear();
                _list.Clear();
            }

            public void DeepCopyTo(SOD2 dest)
            {
                dest.Clear();
                for (int i = 0; i < _list.Count; i++)
                {
                    string category = _list[i];
                    SOH2 srcTags = base[category];
                    var destTags = new SOH2(srcTags.Count);
                    for (int j = 0; j < srcTags.Count; j++)
                    {
                        destTags.Add(srcTags[j]);
                    }
                    dest.Add(category, destTags);
                }
            }

            internal void SortAndMoveMiscToEnd()
            {
                if (Count == 0) return;

                _list.Sort(StringComparer.OrdinalIgnoreCase);

                foreach (var item in this)
                {
                    item.Value.SortCaseInsensitive();
                }

                if (_list[Count - 1] == "misc") return;

                for (int i = 0; i < Count; i++)
                {
                    string item = _list[i];
                    if (_list[i] == "misc")
                    {
                        _list.Remove(item);
                        _list.Add(item);
                        return;
                    }
                }
            }
        }

        /*
        public class SortableOrderedDictionary<TKey, TValue> : IEnumerable<TKey>
        {
            private readonly Dictionary<TKey, TValue> _dict;
            private readonly List<TKey> _list;

            public SortableOrderedDictionary()
            {
                _dict = new Dictionary<TKey, TValue>();
                _list = new List<TKey>();
            }

            public SortableOrderedDictionary(int capacity)
            {
                _dict = new Dictionary<TKey, TValue>(capacity);
                _list = new List<TKey>(capacity);
            }

            public void Add(TKey key, TValue value)
            {
                if (!_dict.ContainsKey(key))
                {
                    _dict[key] = value;
                    _list.Add(key);
                }
            }

            public void Remove(TKey key)
            {
                if (_dict.Remove(key))
                {
                    _list.Remove(key);
                }
            }

            public void Clear()
            {
                _dict.Clear();
                _list.Clear();
            }

            public int Count => _list.Count;

            public bool TryGetValue(TKey equalValue, out TValue value) => _dict.TryGetValue(equalValue, out value);

            //public bool Contains(TKey item) => _dict.Contains(item);

            public TKey? Find(Predicate<TKey> match) => _list.Find(match);

            public void Sort() => _list.Sort();

            public void Sort(IComparer<TKey> comparer) => _list.Sort(comparer);

            public void Sort(int index, int count, IComparer<TKey> comparer) => _list.Sort(index, count, comparer);

            public void Sort(Comparison<TKey> comparison) => _list.Sort(comparison);

            public TKey this[int index] => _list[index];

            public IEnumerator<TKey> GetEnumerator() => _list.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class SortableOrderedHashSet<T> : IEnumerable<T>
        {
            private readonly HashSet<T> _hashSet;
            private readonly List<T> _list;

            public SortableOrderedHashSet()
            {
                _hashSet = new HashSet<T>();
                _list = new List<T>();
            }

            public SortableOrderedHashSet(int capacity)
            {
                _hashSet = new HashSet<T>(capacity);
                _list = new List<T>(capacity);
            }

            public void Add(T key)
            {
                if (_hashSet.Add(key))
                {
                    _list.Add(key);
                }
            }

            public void Remove(T key)
            {
                if (_hashSet.Remove(key))
                {
                    _list.Remove(key);
                }
            }

            public void Clear()
            {
                _hashSet.Clear();
                _list.Clear();
            }

            public int Count => _list.Count;

            public bool TryGetValue(T equalValue, out T actualValue) => _hashSet.TryGetValue(equalValue, out actualValue);

            public bool Contains(T item) => _hashSet.Contains(item);

            public T? Find(Predicate<T> match) => _list.Find(match);

            public void Sort() => _list.Sort();

            public void Sort(IComparer<T> comparer) => _list.Sort(comparer);

            public void Sort(int index, int count, IComparer<T> comparer) => _list.Sort(index, count, comparer);

            public void Sort(Comparison<T> comparison) => _list.Sort(comparison);

            public T this[int index] => _list[index];

            public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        */

        internal static readonly string[] ValidDateFormatList = { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" };

        internal static class Defaults
        {
            #region Main window

            internal const int MainWindowWidth = 1280;
            internal const int MainWindowHeight = 720;

            internal const int MainWindowX = 50;
            internal const int MainWindowY = 50;

            internal const int ColumnWidth = 100;
            internal const int MinColumnWidth = 25;

            internal const float TopSplitterPercent = 0.741f;
            internal const float MainSplitterPercent = 0.4425f;

            internal const float FMsListFontSizeInPoints = 8.25f;

            #endregion

            #region Settings window

            internal const int SettingsWindowWidth = 710;
            internal const int SettingsWindowHeight = 708;
            internal const int SettingsWindowSplitterDistance = 155;

            #endregion

            internal const string DateCustomFormat1 = "dd";
            internal const string DateCustomSeparator1 = "/";
            internal const string DateCustomFormat2 = "MM";
            internal const string DateCustomSeparator2 = "/";
            internal const string DateCustomFormat3 = "yyyy";
            internal const string DateCustomSeparator3 = "";
            internal const string DateCustomFormat4 = "";

            internal const string WebSearchUrl = "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com";

            internal const uint DaysRecent = 15;
            internal const uint MaxDaysRecent = 99999;
        }

        // IMPORTANT: Put these AFTER every other static field has been initialized!
        // Otherwise, these things' constructors might refer back to this class and get a field that may not have
        // been initialized. Ugh.
        #region Global mutable state

        internal static readonly ConfigData Config = new ConfigData();

        // This one is sort of quasi-immutable: its fields are readonly (they're loaded by reflection) but the
        // object itself is not readonly, so that the reader can start with a fresh instance with default values
        // for all the fields it doesn't find a new value for.
        internal static LText_Class LText = new LText_Class();

        // Preset tags will be deep copied to this list later
        internal static readonly SOD2 GlobalTags = new SOD2(PresetTags.Count);

        #region FM lists

        // PERF_TODO: Set capacity later when we read FMData.ini and we count the [FM] entries in the file
        internal static readonly List<FanMission> FMDataIniList = new List<FanMission>();
        internal static readonly List<FanMission> FMsViewList = new List<FanMission>();

        #endregion

        #endregion
    }
}
