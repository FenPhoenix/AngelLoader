﻿using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.DataClasses;
using SevenZip;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMLanguages
    {
        #region Public fields

        internal const string DefaultLangKey = "default";

        #endregion

        #region Methods

        internal static List<string> SortLangsToSpec(HashSetI langsHash)
        {
            var ret = new List<string>(LanguageSupport.Supported.Length);

            // Return a list of all found languages, sorted in the same order as FMSupportedLanguages
            // (matching FMSel behavior)
            if (langsHash.Count > 0)
            {
                for (int i = 0; i < LanguageSupport.Supported.Length; i++)
                {
                    string sl = LanguageSupport.Supported[i];
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

            var (_, fmLanguage, _, _, _) = GameConfigFiles.GetInfoFromCamModIni(Config.GetGamePath(game), out _, langOnly: true);

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

        // @BetterErrors(FillFMSupportedLangs())
        internal static void FillFMSupportedLangs(FanMission fm)
        {
            // We should already have checked before getting here, but just for safety...
            if (!GameIsDark(fm.Game)) return;

            string fmInstPath = Path.Combine(Config.GetFMInstallPath(GameToGameIndex(fm.Game)), fm.InstalledDir);
            List<string> langs;
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
                    fm.LangsScanned = false;
                    return;
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
                    fm.LangsScanned = false;
                    return;
                }
            }

            if (langs.Count > 0)
            {
                langs = SortLangsToSpec(langs.ToHashSetI());
                fm.Langs = string.Join(",", langs);
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

            var langsFoundList = new HashSetI(LanguageSupport.Supported.Length);

            while (searchList.Count > 0)
            {
                string bdPath = searchList[searchList.Count - 1];
                searchList.RemoveAt(searchList.Count - 1);
                bool englishFound = FastIO.SearchDirForLanguages(LanguageSupport.LangsHash, bdPath, searchList, langsFoundList, earlyOutOnEnglish);
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

            string archivePath = FMArchives.FindFirstMatch(archiveName);
            if (archivePath.IsEmpty()) return failed;

            var ret = new List<string>(LanguageSupport.Supported.Length);

            bool[] FoundLangInArchive = new bool[LanguageSupport.Supported.Length];

            FMScanner.FastZipReader.ZipArchiveFast? zipArchive = null;
            SevenZipExtractor? sevenZipArchive = null;
            try
            {
                bool fmIsZip = archivePath.ExtIsZip();

                if (fmIsZip)
                {
                    using var bundle = new FMScanner.FastZipReader.ZipReusableBundle();
                    zipArchive = new FMScanner.FastZipReader.ZipArchiveFast(File.OpenRead(archivePath), bundle);
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

                    for (int j = 0; j < LanguageSupport.Supported.Length; j++)
                    {
                        string sl = LanguageSupport.Supported[j];
                        if (!FoundLangInArchive[j])
                        {
                            // Do as few string operations as humanly possible
                            int index = fn.IndexOf(LanguageSupport.FSPrefixedLangs[j], StringComparison.OrdinalIgnoreCase);
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
            (bool success, List<string> languages) = GetFMSupportedLanguagesFromArchive(archive, earlyOutOnEnglish);
            try
            {
                return success ? languages : GetFMSupportedLanguagesFromInstDir(fmInstPath, earlyOutOnEnglish);
            }
            catch (Exception ex)
            {
                Log(nameof(GetFMSupportedLanguagesFromInstDir) + ": Unable to run the language selection; language may be wrong.", ex);
                return new List<string>();
            }
        }

        #endregion
    }
}
