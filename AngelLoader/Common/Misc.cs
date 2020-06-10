using System;
using System.Collections.Generic;
using System.Text;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader
{
    public static partial class Misc
    {
        internal const string AppGuid = "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader";

        // Perf: so we only have to get it once
        internal static readonly int TopRightTabsCount = Enum.GetValues(typeof(TopRightTab)).Length;

        #region Global state

        internal static readonly ConfigData Config = new ConfigData();
        internal static LText_Class LText = new LText_Class();

        #region Categories and tags

        // These are the FMSel preset tags. Conforming to standards here.
        internal static readonly GlobalCatAndTagsList PresetTags = new GlobalCatAndTagsList(6)
        {
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "author", IsPreset = true } },
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "contest", IsPreset = true } },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "genre", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(5)
                {
                    new GlobalCatOrTag { Name = "action", IsPreset = true },
                    new GlobalCatOrTag { Name = "crime", IsPreset = true },
                    new GlobalCatOrTag { Name = "horror", IsPreset = true },
                    new GlobalCatOrTag { Name = "mystery", IsPreset = true },
                    new GlobalCatOrTag { Name = "puzzle", IsPreset = true }
                }
            },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "language", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(11)
                {
                    new GlobalCatOrTag { Name = "English", IsPreset = true },
                    new GlobalCatOrTag { Name = "Czech", IsPreset = true },
                    new GlobalCatOrTag { Name = "Dutch", IsPreset = true },
                    new GlobalCatOrTag { Name = "French", IsPreset = true },
                    new GlobalCatOrTag { Name = "German", IsPreset = true },
                    new GlobalCatOrTag { Name = "Hungarian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Italian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Japanese", IsPreset = true },
                    new GlobalCatOrTag { Name = "Polish", IsPreset = true },
                    new GlobalCatOrTag { Name = "Russian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Spanish", IsPreset = true }
                }
            },
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "series", IsPreset = true } },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "misc", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(6)
                {
                    new GlobalCatOrTag { Name = "campaign", IsPreset = true },
                    new GlobalCatOrTag { Name = "demo", IsPreset = true },
                    new GlobalCatOrTag { Name = "long", IsPreset = true },
                    new GlobalCatOrTag { Name = "other protagonist", IsPreset = true },
                    new GlobalCatOrTag { Name = "short", IsPreset = true },
                    new GlobalCatOrTag { Name = "unknown author", IsPreset = true }
                }
            }
        };

        // Don't say this = PresetTags; that will make it a reference and we don't want that. It will be deep
        // copied later.
        internal static readonly GlobalCatAndTagsList GlobalTags = new GlobalCatAndTagsList(6);

        #endregion

        #region FM lists

        // PERF_TODO: Set capacity later when we read FMData.ini and we count the [FM] entries in the file
        internal static readonly List<FanMission> FMDataIniList = new List<FanMission>();
        internal static readonly List<FanMission> FMsViewList = new List<FanMission>();

        // Super quick-n-cheap hack for perf: So we don't have to iterate the whole list looking for unscanned FMs.
        // This will contain indexes into FMDataIniList (not FMsViewList!)
        internal static readonly List<int> FMsViewListUnscanned = new List<int>();

        #endregion

        #endregion

        #region Enums and enum-like

        // Class instead of enum so we don't have to keep casting its fields
        [PublicAPI]
        internal static class ByteSize
        {
            internal const int KB = 1024;
            internal const int MB = KB * 1024;
            internal const int GB = MB * 1024;
        }

        // Public for param accessibility reasons or whatever
        public enum ProgressTasks
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

        public enum Direction { Left, Right, Up, Down }

        internal enum ReadmeType { PlainText, RichText, HTML, GLML }

        internal static class HelpSections
        {
            internal const string InitialSettings = "#initial_setup";
            internal const string PathsSettings = "#settings_paths_section";
            internal const string FMDisplaySettings = "#settings_fm_display_section";
            internal const string OtherSettings = "#settings_other_section";

            internal const string FMBackupPath = "#fm_backup_path";

            internal const string MainWindow = "#main_window";
            internal const string MissionList = "#mission_list";
            internal const string ColumnHeaderContextMenu = "#column_header_context_menu";
            internal const string FMContextMenu = "#fm_context_menu";

            internal const string StatsTab = "#stats_tab";
            internal const string EditFMTab = "#edit_fm_tab";
            internal const string CommentTab = "#comment_tab";
            internal const string TagsTab = "#tags_tab";
            internal const string PatchTab = "#patch_tab";

            internal const string ReadmeArea = "#readme_area";
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
        internal static readonly char[] CA_ForwardSlash = { '/' };
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

            //internal static ConfigVar CV_ForceFullScreen = new ConfigVar { Command = "-force_windowed" };
            //internal static ConfigVar CV_ForceWindowed = new ConfigVar { Command = "+force_windowed" };
            //internal static ConfigVar CV_ForceOldMantle = new ConfigVar { Command = "-new_mantle" };
            //internal static ConfigVar CV_ForceNewMantle = new ConfigVar { Command = "+new_mantle" };
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

        [PublicAPI]
        internal interface IEventDisabler
        {
            bool EventsDisabled { get; set; }
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

        [PublicAPI]
        internal interface IKeyPressDisabler
        {
            bool KeyPressesDisabled { get; set; }
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
    }
}
