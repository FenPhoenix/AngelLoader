using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AngelLoader.DataClasses;

namespace AngelLoader
{
    public static partial class Misc
    {
        // ReSharper disable once ConvertToConstant.Global
        internal static readonly string AppGuid = "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader";

        internal static readonly int ColumnsCount = Enum.GetValues(typeof(Column)).Length;
        internal static readonly int HideableFilterControlsCount = Enum.GetValues(typeof(HideableFilterControls)).Length;
        public static readonly int ZoomTypesCount = Enum.GetValues(typeof(Zoom)).Length;

        #region Enums and enum-like

        // Class instead of enum so we don't have to keep casting its fields
        internal static class ByteSize
        {
            internal const int KB = 1024;
            internal const int MB = KB * 1024;
            internal const int GB = MB * 1024;
        }

        public enum Zoom { In, Out, Reset }

        // Public for param accessibility reasons or whatever
        public enum ProgressTask
        {
            ScanAllFMs,
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

        // Non-consts for file size; these aren't perf-critical at all
        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        internal static class HelpSections
        {
            internal static readonly string InitialSettings = "#initial_setup";
            internal static readonly string PathsSettings = "#settings_paths_section";
            internal static readonly string FMDisplaySettings = "#settings_fm_display_section";
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

            internal static readonly string ReadmeArea = "#readme_area";
        }

        #endregion

        #region Preset char arrays

        internal static readonly byte[] RTFHeaderBytes = Encoding.ASCII.GetBytes(@"{\rtf1");

        // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
        internal static readonly char[] CA_Comma = { ',' };
        internal static readonly char[] CA_Semicolon = { ';' };
        internal static readonly char[] CA_CommaSemicolon = { ',', ';' };
        internal static readonly char[] CA_CommaSpace = { ',', ' ' };
        internal static readonly char[] CA_Backslash = { '\\' };
        //internal static readonly char[] CA_ForwardSlash = { '/' };
        internal static readonly char[] CA_BS_FS = { '\\', '/' };
        internal static readonly char[] CA_BS_FS_Space = { '\\', '/', ' ' };

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

        #region Interfaces

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

        internal interface IEventDisabler
        {
            bool EventsDisabled { set; }
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

        #endregion

        #region DisableKeyPresses

        internal interface IKeyPressDisabler
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

        internal interface ILocalizable
        {
            void Localize();
        }

        #endregion

        // IMPORTANT: Put these AFTER every other static field has been initialized!
        // Otherwise, these things' constructors might refer back to this class and get a field that may not have
        // been initialized. Ugh.
        #region Global mutable state

        internal static readonly ConfigData Config = new ConfigData();

        internal static LText_Class LText = new LText_Class();

        // Preset tags will be deep copied to this list later
        internal static readonly CatAndTagsList GlobalTags = new CatAndTagsList(PresetTags.Count);

        #region FM lists

        // PERF_TODO: Set capacity later when we read FMData.ini and we count the [FM] entries in the file
        internal static readonly List<FanMission> FMDataIniList = new List<FanMission>();
        internal static readonly List<FanMission> FMsViewList = new List<FanMission>();

        #endregion

        #region Cheap hacks

        // Stupid hack for perf and nice UX when deleting FMs (we filter out deleted ones until the next find from
        // disk, when we remove them properly)
        internal static bool OneOrMoreFMsAreMarkedDeleted;

        #endregion

        #endregion
    }
}
