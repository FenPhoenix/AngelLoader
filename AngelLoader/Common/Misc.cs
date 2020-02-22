using System;
using System.Collections.Generic;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader
{
    public static partial class Misc
    {
        internal const string AppGuid = "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader";

        // Perf: so we only have to get it once
        internal static readonly int TopRightTabsCount = Enum.GetValues(typeof(TopRightTab)).Length;

        internal static readonly ConfigData Config = new ConfigData();

#if false

        // Used for detecting NewDark version of NewDark executables
        private static readonly byte[] ProductVersionBytes = Encoding.ASCII.GetBytes(new[]
        {
            'P', '\0', 'r', '\0', 'o', '\0', 'd', '\0', 'u', '\0', 'c', '\0', 't', '\0',
            'V', '\0', 'e', '\0', 'r', '\0', 's', '\0', 'i', '\0', 'o', '\0', 'n', '\0', '\0', '\0'
        });

#endif

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

        #region FMs lists

        internal static readonly List<FanMission> FMsViewList = new List<FanMission>();
        internal static readonly List<FanMission> FMDataIniList = new List<FanMission>();

        // Super quick-n-cheap hack for perf: So we don't have to iterate the whole list looking for unscanned FMs.
        // This will contain indexes into FMDataIniList (not FMsViewList!)
        internal static readonly List<int> ViewListUnscanned = new List<int>();

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

        [PublicAPI]
        internal enum Selector
        {
            FMSel,
            NewDarkLoader,
            AngelLoader
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
            CacheFM
        }

        public enum Direction { Left, Right, Up, Down }

        #endregion

        #region Preset char arrays

        // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
        internal static readonly char[] CA_Comma = { ',' };
        internal static readonly char[] CA_Semicolon = { ';' };
        internal static readonly char[] CA_CommaSemicolon = { ',', ';' };
        internal static readonly char[] CA_CommaSpace = { ',', ' ' };
        internal static readonly char[] CA_Backslash = { '\\' };
        internal static readonly char[] CA_ForwardSlash = { '/' };
        internal static readonly char[] CA_BS_FS = { '\\', '/' };
        internal static readonly char[] CA_SysDirSep = { System.IO.Path.DirectorySeparatorChar };

        #endregion

        // This is for passing to the game via the stub to match FMSel's behavior (Dark only)
        internal static readonly string[] FMSupportedLanguages =
        {
            "english",    // en, must be first
            "czech",      // cz
            "dutch",      // nl
            "french",     // fr
            "german",     // de
            "hungarian",  // hu
            "italian",    // it
            "japanese",   // ja, jp
            "polish",     // pl
            "russian",    // ru
            "spanish"     // es
        };

        // For manual selection of language for playing an FM
        internal static readonly Dictionary<string, string>
        FMLangsTranslated = new Dictionary<string, string>
        {
            { "english", "English" },
            { "czech", "Čeština" },
            { "dutch", "Nederlands" },
            { "french", "Français" },
            { "german", "Deutsch" },
            { "hungarian", "Magyar" },
            { "italian", "Italiano" },
            { "japanese", "日本語" },
            { "polish", "Polski" },
            { "russian", "Русский" },
            { "spanish", "Español" }
        };

        internal static class Defaults
        {
            internal const int MainWindowX = 50;
            internal const int MainWindowY = 50;

            internal const int ColumnWidth = 100;
            internal const int MinColumnWidth = 25;

            internal const float TopSplitterPercent = 0.741f;
            internal const float MainSplitterPercent = 0.4425f;

            internal const string WebSearchUrl = "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com";

            internal const uint MaxDaysRecent = 99999;

            //internal static ConfigVar CV_ForceFullScreen = new ConfigVar { Command = "-force_windowed" };
            //internal static ConfigVar CV_ForceWindowed = new ConfigVar { Command = "+force_windowed" };
            //internal static ConfigVar CV_ForceOldMantle = new ConfigVar { Command = "-new_mantle" };
            //internal static ConfigVar CV_ForceNewMantle = new ConfigVar { Command = "+new_mantle" };
        }

        #region FM cache (readmes etc.)

        // We might want to add other things (thumbnails etc.) later, so it's a class
        internal class CacheData
        {
            internal readonly List<string> Readmes;
            internal CacheData() => Readmes = new List<string>();
            internal CacheData(List<string> readmes) => Readmes = readmes;
        }

        internal enum ReadmeType { PlainText, RichText, HTML, GLML }

        #endregion

        #region Interfaces

        #region DisableEvents

        /*
         Implement the interface on your form, and put guard clauses on all your event handlers that you want to be
         disableable:

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
