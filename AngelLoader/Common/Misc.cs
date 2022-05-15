﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AngelLoader.DataClasses;

namespace AngelLoader
{
    public static partial class Misc
    {
        //internal const int AppConfigVersion = 1;
        // Commented so that old versions of AngelLoader will ignore it
        //internal const string ConfigVersionHeader = ";@Version:";

        internal static readonly int ColumnsCount = Enum.GetValues(typeof(Column)).Length;
        internal static readonly int HideableFilterControlsCount = Enum.GetValues(typeof(HideableFilterControls)).Length;
        public static readonly int ZoomTypesCount = Enum.GetValues(typeof(Zoom)).Length;

        #region Enums and enum-like

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

        public enum MBoxButtons
        {
            OK,
            OKCancel,
            YesNoCancel,
            YesNo,
        }

        public enum MBoxIcon
        {
            None,
            Error,
            Hand,
            Stop,
            Question,
            Exclamation,
            Warning,
            Asterisk,
            Information
        }

        public enum Zoom { In, Out, Reset }

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

        public enum ProgressCancelType
        {
            Cancel,
            Stop
        }

        public static readonly Action NullAction = () => { };

        public enum ImportType
        {
            DarkLoader,
            FMSel,
            NewDarkLoader
        }

        public enum Direction { Left, Right, Up, Down }

        // Wri is a "secret" format - it's a first-class type internally, but we don't document our support for
        // it because it's really just a hack for Lucrative Opportunity and the format is not supported in full.
        // Any other .wri files besides the one from Lucrative Opportunity are likely to be displayed incorrectly.
        // And we don't want to give anyone any ideas to use this format (not like any modern program I know of
        // can even write the .wri format, but still).
        public enum ReadmeType { PlainText, RichText, HTML, GLML, Wri }

        public enum ReadmeState { HTML, PlainText, OtherSupported, LoadError, InitialReadmeChooser }

        public enum ReadmeLocalizableMessage { None, NoReadmeFound, UnableToLoadReadme }

        internal enum AudioConvert { MP3ToWAV, OGGToWAV, WAVToWAV16 }

        /// <summary>
        /// Set a control's tag to this to tell the darkable control dictionary filler to ignore it.
        /// </summary>
        public enum LoadType { Lazy }

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

            // @NET5: Remember to change this to match the new font
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
        internal static readonly FMCategoriesCollection GlobalTags = new FMCategoriesCollection(PresetTags.Count);

        #region FM lists

        // Init to 0 capacity so they don't allocate a 4-byte backing array or whatever, cause we're going to
        // reallocate them right away anyway.
        internal static readonly List<FanMission> FMDataIniList = new List<FanMission>(0);
        internal static readonly List<FanMission> FMsViewList = new List<FanMission>(0);

        #endregion

        #endregion
    }
}
