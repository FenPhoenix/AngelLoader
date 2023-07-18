using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AL_Common.FastZipReader;
using AngelLoader.DataClasses;
using SharpCompress.Archives.SevenZip;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class FMLanguages
{
    #region Public fields

    internal const string DefaultLangKey = "default";

    #endregion

    #region Methods

    private static List<string> SortLangsToSpec(HashSetI langsHash)
    {
        var ret = new List<string>(SupportedLanguageCount);

        // Return a list of all found languages, sorted in the same order as FMSupportedLanguages
        // (matching FMSel behavior)
        if (langsHash.Count > 0)
        {
            for (int i = 0; i < SupportedLanguageCount; i++)
            {
                string sl = SupportedLanguages[i];
                if (langsHash.Contains(sl)) ret.Add(sl);
            }
        }

        return ret;
    }

    internal static (string Language, bool ForceLanguage)
    GetDarkFMLanguage(GameIndex game, string fmArchive, string fmInstalledDir)
    {
        string sLanguage;
        bool bForceLanguage;

        var (_, fmLanguage, _, _, _, _) =
            GameConfigFiles.GetInfoFromCamModIni(
                Config.GetGamePath(game),
                langOnly: true,
                returnAllLines: false);

        // bForceLanguage gets set to something specific in every possible case, effectively meaning the
        // fm_language_forced value is always ignored. Weird, but FMSel's code does exactly this, so meh?
        // Although I'm using FMSel from ND 1.26 as a reference, ND 1.27's is exactly the same.
        string fmInstPath = Path.Combine(Config.GetFMInstallPath(game), fmInstalledDir);
        if (!fmLanguage.IsEmpty())
        {
            List<string> fmSupportedLangs = GetFMSupportedLanguages(fmArchive, fmInstPath, earlyOutOnEnglish: false);
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

            List<string> langs = GetFMSupportedLanguages(fmArchive, fmInstPath, earlyOutOnEnglish: true);

            // Use first available language (which will be English if English is available)
            sLanguage = langs.Count == 0 ? "" : langs[0];
            bForceLanguage = false;
        }

        return (sLanguage, bForceLanguage);
    }

    internal static void FillFMSupportedLangs(FanMission fm)
    {
        if (!GameIsDark(fm.Game)) return;

        List<string>? langs;
        if (FMIsReallyInstalled(fm, out string fmInstalledPath))
        {
            if (!TrySetLangsFromInstalledDir(fm, fmInstalledPath, out langs) &&
                !TrySetLangsFromArchive(fm, out langs))
            {
                fm.Langs = Language.Default;
                fm.LangsScanned = false;
                return;
            }
        }
        else
        {
            if (!TrySetLangsFromArchive(fm, out langs))
            {
                fm.Langs = Language.Default;
                fm.LangsScanned = false;
                return;
            }
        }

        if (langs.Count > 0)
        {
            for (int i = 0; i < langs.Count; i++)
            {
                string lang = langs[i];
                if (LangStringsToEnums.TryGetValue(lang, out Language language))
                {
                    fm.Langs |= language;
                }
            }
        }

        fm.LangsScanned = true;

        return;

        #region Local functions

        static bool TrySetLangsFromArchive(FanMission fm, [NotNullWhen(true)] out List<string>? langs)
        {
            try
            {
                (_, langs) = GetFMSupportedLanguagesFromArchive(fm.Archive, earlyOutOnEnglish: false);
                return true;
            }
            catch (Exception ex)
            {
                LogFMInfo(fm, ErrorText.ExDetLangIn + "archive.", ex);
                langs = null;
                return false;
            }
        }

        static bool TrySetLangsFromInstalledDir(FanMission fm, string fmInstPath, [NotNullWhen(true)] out List<string>? langs)
        {
            try
            {
                langs = GetFMSupportedLanguagesFromInstDir(fmInstPath, earlyOutOnEnglish: false);
                return true;
            }
            catch (Exception ex)
            {
                LogFMInfo(fm, ErrorText.ExTry + ErrorText.ExDetLangIn + "installed dir.", ex);
                langs = null;
                return false;
            }
        }

        #endregion
    }

    private static List<string> GetFMSupportedLanguagesFromInstDir(string fmInstPath, bool earlyOutOnEnglish)
    {
        // Get initial list of base FM dirs the normal way: we don't want to count these as lang dirs even if
        // they're named such (matching FMSel behavior)
        List<string> searchList = FastIO.GetDirsTopOnly(fmInstPath, "*", ignoreReparsePoints: true);
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

        var langsFoundList = new HashSetI(SupportedLanguageCount);

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

        string archivePath = FMArchives.FindFirstMatch(archiveName, FMArchives.GetFMArchivePaths());
        if (archivePath.IsEmpty()) return failed;

        var ret = new List<string>(SupportedLanguageCount);

        bool[] foundLangInArchive = new bool[SupportedLanguageCount];

        try
        {
            bool fmIsZip = archivePath.ExtIsZip();

            static (bool EarlyOutEnglish, List<string>? Languages)
            Search(string fn, bool earlyOutOnEnglish, bool[] foundLangInArchive, List<string> ret)
            {
                for (int j = 0; j < SupportedLanguageCount; j++)
                {
                    string sl = SupportedLanguages[j];
                    if (!foundLangInArchive[j])
                    {
                        // Do as few string operations as humanly possible
                        int index = fn.IndexOf(FSPrefixedLangs[j], StringComparison.OrdinalIgnoreCase);
                        if (index < 1) continue;

                        if ((fn.Length > index + sl.Length + 1 && fn[index + sl.Length + 1] == '/') ||
                            (index == (fn.Length - sl.Length) - 1))
                        {
                            if (earlyOutOnEnglish && j == 0) return (true, new List<string> { "English" });
                            ret.Add(sl);
                            foundLangInArchive[j] = true;
                        }
                    }
                }

                return (false, null);
            }

            if (fmIsZip)
            {
                using var zipArchive = new ZipArchiveFast(File_OpenReadFast(archivePath), allowUnsupportedEntries: true);
                int filesCount = zipArchive.Entries.Count;
                for (int i = 0; i < filesCount; i++)
                {
                    // ZipArchiveFast guarantees full names to never contain backslashes
                    string fn = zipArchive.Entries[i].FullName;
                    var result = Search(fn, earlyOutOnEnglish, foundLangInArchive, ret);
                    if (result.EarlyOutEnglish) return (true, result.Languages!);
                }
            }
            else
            {
                using var fs = File_OpenReadFast(archivePath);
                using var sevenZipArchive = new SevenZipArchive(fs);
                foreach (SevenZipArchiveEntry entry in sevenZipArchive.Entries)
                {
                    if (entry.IsAnti) continue;

                    string fn = entry.FileName.ToForwardSlashes();
                    var result = Search(fn, earlyOutOnEnglish, foundLangInArchive, ret);
                    if (result.EarlyOutEnglish) return (true, result.Languages!);
                }
            }
        }
        catch (Exception ex)
        {
            Log("Scanning archive '" + archivePath + "' for languages failed.", ex);
            return (false, new List<string>());
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
        (bool success, List<string> languages) = GetFMSupportedLanguagesFromArchive(archive, earlyOutOnEnglish);
        try
        {
            return success ? languages : GetFMSupportedLanguagesFromInstDir(fmInstPath, earlyOutOnEnglish);
        }
        catch (Exception ex)
        {
            Log(nameof(GetFMSupportedLanguagesFromInstDir) + ": " + ErrorText.Un + "run the language selection; language may be wrong.", ex);
            return new List<string>();
        }
    }

    #endregion
}
