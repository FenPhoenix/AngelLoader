using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using SevenZip;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMLanguages
    {
        internal const string DefaultLangKey = "default";

        // This is for passing to the game via the stub to match FMSel's behavior (Dark only)
        internal static readonly string[]
        Supported =
        {
            "english",    // en, eng (must be first)
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

        internal static readonly Dictionary<string, string>
        LangCodes = new Dictionary<string, string>(11)
        {
            { "english", "en" },
            { "czech", "cz" },
            { "dutch", "nl" },
            { "french", "fr" },
            { "german", "de" },
            { "hungarian", "hu" },
            { "italian", "it" },
            { "japanese", "ja" },
            { "polish", "pl" },
            { "russian", "ru" },
            { "spanish", "es" }
        };

        internal static readonly Dictionary<string, string>
        AltLangCodes = new Dictionary<string, string>(2)
        {
            { "en", "eng" },
            { "ja", "jp" }
        };

        // For manual selection of language for playing an FM
        internal static readonly Dictionary<string, string>
        Translated = new Dictionary<string, string>(11)
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

        internal static List<string> SortLangsToSpec(List<string> langs)
        {
            var ret = new List<string>();

            // Return a list of all found languages, sorted in the same order as FMSupportedLanguages
            // (matching FMSel behavior)
            if (langs.Count > 0)
            {
                for (int i = 0; i < Supported.Length; i++)
                {
                    string sl = Supported[i];
                    if (langs.ContainsI(sl)) ret.Add(sl);
                }
            }

            return ret;
        }

        internal static (string Language, bool ForceLanguage)
        GetDarkFMLanguage(GameIndex game, string fmArchive, string fmInstalledDir)
        {
            string sLanguage;
            bool bForceLanguage;

            var (_, fmLanguage, _, _, _) = Core.GetInfoFromCamModIni(Config.GetGamePath(game), out _, langOnly: true);

            // bForceLanguage gets set to something specific in every possible case, effectively meaning the
            // fm_language_forced value is always ignored. Weird, but FMSel's code does exactly this, so meh?
            // NOTE: Although I'm using FMSel from ND 1.26 as a reference, ND 1.27's is exactly the same.
            string fmInstPath = Path.Combine(Config.GetFMInstallPath(game), fmInstalledDir);
            if (!fmLanguage.IsEmpty())
            {
                var fmSupportedLangs = GetFMSupportedLanguages(fmArchive, fmInstPath, earlyOutOnEnglish: false);
                if (fmSupportedLangs.ContainsI(fmLanguage))
                {
                    // FMSel doesn't set this because it's already getting it from the game meaning it's set
                    // already, but we have to set it ourselves because we're getting it manually
                    sLanguage = fmLanguage;

                    bForceLanguage = true;
                }
                else
                {
                    // language not supported, use fallback
                    sLanguage = fmSupportedLangs.Count > 0 ? fmSupportedLangs[0] : "";
                    bForceLanguage = false;
                }
            }
            else
            {
                /*
                 So, if I'm reading the API notes right, it looks like NewDark actually picks the right language
                 automatically if it can find it in DARKINST.CFG. We set sLanguage to either:
                 -force that language to be used if it's available (otherwise we use the below crappily-guessed
                 fallback), or
                 -give it a crappily-guessed fallback value so that if NewDark can't find a language, we at least
                 have SOMETHING to give it so text won't be blank, even though it's likely it'll be the wrong
                 language if it isn't English.
                */

                // FMSel's comment:
                // determine FM default language (used if the FM doesn't support the language set in dark by the "language" cfg var)

                // FMSel's comment:
                // determine if FM has languages defined, if it does and english is among them, then english is the fallback
                // if english is not among them then pick another, if no languages are found then no fallback language will
                // be defined

                var langs = GetFMSupportedLanguages(fmArchive, fmInstPath, earlyOutOnEnglish: true);

                // Use first available language (which will be English if English is available)
                sLanguage = langs.Count == 0 ? "" : langs[0];
                bForceLanguage = false;
            }

            return (sLanguage, bForceLanguage);
        }

        internal static void FillFMSupportedLangs(FanMission fm, bool removeEnglish = true)
        {
            string fmInstPath = Path.Combine(Config.GetFMInstallPath(GameToGameIndex(fm.Game)), fm.InstalledDir);
            List<string> langs = new List<string>();
            if (FMIsReallyInstalled(fm))
            {
                try
                {
                    langs = GetFMSupportedLanguagesFromInstDir(fmInstPath, earlyOutOnEnglish: false);
                }
                catch (Exception ex)
                {
                    Log("Exception trying to detect language folders in installed dir for fm '" +
                        fm.Archive + "' (inst dir '" + fm.InstalledDir + "')", ex);
                }
            }
            else
            {
                try
                {
                    (_, langs) = GetFMSupportedLanguagesFromArchive(fm.Archive, earlyOutOnEnglish: false);
                }
                catch (Exception ex)
                {
                    Log("Exception trying to detect language folders in archive for fm '" +
                        fm.Archive + "' (inst dir '" + fm.InstalledDir + "')", ex);
                }
            }

            fm.Langs = "";
            if (langs.Count > 0)
            {
                langs = SortLangsToSpec(langs);

                if (removeEnglish && langs[0].EqualsI("english")) langs.RemoveAt(0);
                for (int i = 0; i < langs.Count; i++)
                {
                    if (i > 0) fm.Langs += ",";
                    fm.Langs += langs[i];
                }
            }

            fm.LangsScanned = true;
        }

        private static List<string> GetFMSupportedLanguagesFromInstDir(string fmInstPath, bool earlyOutOnEnglish)
        {
            // Get initial list of base FM dirs the normal way: we don't want to count these as lang dirs even if
            // they're named such (matching FMSel behavior)
            var searchList = FastIO.GetDirsTopOnly(fmInstPath, "*", ignoreReparsePoints: true);
            if (searchList.Count == 0) return new List<string>();

            #region Move key dirs to end of list (priority)

            // Searching folders is horrendously slow, so prioritize folders most likely to contain lang dirs so
            // if we find English, we end up earlying-out much faster

            for (int i = 0; i < 3; i++)
            {
                string keyDir = i switch { 0 => "/books", 1 => "/intrface", _ => "/strings" };

                for (int j = 0; j < searchList.Count; j++)
                {
                    if (j < searchList.Count - 1 &&
                        searchList[j].PathEndsWithI(keyDir))
                    {
                        string item = searchList[j];
                        searchList.RemoveAt(j);
                        searchList.Add(item);
                        break;
                    }
                }
            }

            #endregion

            var langsFoundList = new List<string>(Supported.Length);

            while (searchList.Count > 0)
            {
                string bdPath = searchList[searchList.Count - 1];
                searchList.RemoveAt(searchList.Count - 1);
                bool englishFound = FastIO.SearchDirForLanguages(bdPath, searchList, langsFoundList, earlyOutOnEnglish);
                // Matching FMSel behavior: early-out on English
                if (earlyOutOnEnglish && englishFound) return new List<string> { "English" };
            }

            return SortLangsToSpec(langsFoundList);
        }

        private static (bool Success, List<string> Languages)
        GetFMSupportedLanguagesFromArchive(string archiveName, bool earlyOutOnEnglish)
        {
            // @DIRSEP: '/' conversion in here due to string.IndexOf()

            (bool, List<string>) failed = (false, new List<string>());

            string archivePath = FindFMArchive(archiveName);
            if (archivePath.IsEmpty()) return failed;

            var ret = new List<string>();

            bool[] FoundLangInArchive = new bool[Supported.Length];
            // Pre-concat each string only once for perf
            string[] SLangsFSPrefixed = new string[Supported.Length];
            for (int i = 0; i < Supported.Length; i++) SLangsFSPrefixed[i] = "/" + Supported[i];

            FMScanner.FastZipReader.ZipArchiveFast? zipArchive = null;
            SevenZipExtractor? sevenZipArchive = null;
            try
            {
                // TODO: A custom IndexOf() similar to StartsWithOrEndsWithIFast() would probably shave even more time off.

                bool fmIsZip = archivePath.ExtIsZip();

                if (fmIsZip)
                {
                    zipArchive = new FMScanner.FastZipReader.ZipArchiveFast(
                        new FileStream(archivePath, FileMode.Open, FileAccess.Read), leaveOpen: false);
                }
                else
                {
                    sevenZipArchive = new SevenZipExtractor(archivePath);
                }

                int filesCount = fmIsZip ? zipArchive!.Entries.Count : (int)sevenZipArchive!.FilesCount;
                for (int i = 0; i < filesCount; i++)
                {
                    string fn = fmIsZip
                        // ZipArchiveFast guarantees full names to never contain backslashes
                        ? zipArchive!.Entries[i].FullName
                        // For some reason ArchiveFileData[i].FileName is like 20x faster than ArchiveFileNames[i]
                        : sevenZipArchive!.ArchiveFileData[i].FileName.ToForwardSlashes();

                    for (int j = 0; j < Supported.Length; j++)
                    {
                        string sl = Supported[j];
                        if (!FoundLangInArchive[j])
                        {
                            // Do as few string operations as humanly possible
                            int index = fn.IndexOf(SLangsFSPrefixed[j], StringComparison.OrdinalIgnoreCase);
                            if (index < 1) continue;

                            if ((fn.Length > index + sl.Length + 1 && fn[index + sl.Length + 1] == '/') ||
                                (index == (fn.Length - sl.Length) - 1))
                            {
                                if (earlyOutOnEnglish && j == 0) return (true, new List<string> { "English" });
                                ret.Add(sl);
                                FoundLangInArchive[j] = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Scanning archive '" + archivePath + "' for languages failed.", ex);
                return (false, new List<string>());
            }
            finally
            {
                zipArchive?.Dispose();
                sevenZipArchive?.Dispose();
            }

            return (true, ret);
        }

        // Per FMSel behavior, check the archive first, then installed dir.
        // They kind of switch places in speed depending if the cache is warm or cold, but...
        // TODO: @Robustness (FM languages archives vs. installed dirs)
        // IMO the installed dir should be considered the definitive version for our purposes here, as its language
        // dirs may in theory be different from the archive. I'm imagining someone unzipping a lang zip in their
        // installed folder but the game is looking at the archive and so doesn't use it. I may want to just get
        // rid of the archive scan fast-paths later and just eat the cost of the dir scan.
        private static List<string> GetFMSupportedLanguages(string archive, string fmInstPath, bool earlyOutOnEnglish)
        {
            var (Success, Languages) = GetFMSupportedLanguagesFromArchive(archive, earlyOutOnEnglish);
            try
            {
                return Success ? Languages : GetFMSupportedLanguagesFromInstDir(fmInstPath, earlyOutOnEnglish);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(GetFMSupportedLanguagesFromInstDir) + ". Unable to run the language selection; language may be wrong.", ex);
                return new List<string>();
            }
        }
    }
}
