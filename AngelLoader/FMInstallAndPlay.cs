using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using AngelLoader.WinAPI.Ookii.Dialogs;
using SevenZip;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;
#pragma warning disable CS8509 // Switch expression doesn't handle all possible inputs

namespace AngelLoader
{
    internal static class FMInstallAndPlay
    {
        private static CancellationTokenSource ExtractCts = new CancellationTokenSource();

        internal static async Task InstallOrUninstall(FanMission fm) => await (fm.Installed ? UninstallFM(fm) : InstallFM(fm));

        internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                return;
            }

            if (playMP && fm.Game != Game.Thief2)
            {
                Log("playMP was true, but fm.Game was not Thief 2.\r\n" +
                    "fm: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                return;
            }

            if (askConfIfRequired && Config.ConfirmPlayOnDCOrEnter)
            {
                var (cancel, dontAskAgain) = Core.View.AskToContinueYesNoCustomStrings(LText.AlertMessages.Play_ConfirmMessage,
                    LText.AlertMessages.Confirm, icon: null, showDontAskAgain: true, yes: null, no: null);

                if (cancel) return;

                Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
            }

            if (!fm.Installed && !await InstallFM(fm)) return;

            if (playMP && fm.Game == Game.Thief2 && GetT2MultiplayerExe().IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound, LText.AlertMessages.Alert);
                return;
            }

            if (PlayFM(fm, playMP))
            {
                fm.LastPlayed.DateTime = DateTime.Now;
                await Core.View.RefreshSelectedFM(refreshReadme: false);
            }
        }

        #region Play / open

        internal static bool PlayOriginalGame(GameIndex game, bool playMP = false)
        {
            var (success, gameExe, gamePath) = CheckAndReturnFinalGameExeAndGamePath(game, playingOriginalGame: true, playMP);
            if (!success) return false;
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gamePath);

#if !ReleaseBeta && !ReleasePublic
            string args = Config.ForceWindowed ? "+force_windowed" : "";
#else
            string args = "";
#endif
            string workingPath = Config.GetGamePath(game);
            var sv = GetSteamValues(game, playMP);
            if (sv.Success) (_, gameExe, workingPath, args) = sv;

            WriteStubCommFile(null, gamePath);

            StartExe(gameExe, workingPath, args);

            return true;
        }

        private static bool PlayFM(FanMission fm, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Core.View.ShowAlert(LText.AlertMessages.Play_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var game = GameToGameIndex(fm.Game);

            var (success, gameExe, gamePath) = CheckAndReturnFinalGameExeAndGamePath(game, playingOriginalGame: false, playMP);
            if (!success) return false;

            // Always do this for robustness, see below
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gamePath);

            string steamArgs = "";
            string workingPath = Config.GetGamePath(game);
            var sv = GetSteamValues(game, playMP);
            if (sv.Success) (_, gameExe, workingPath, steamArgs) = sv;

            // 2019-10-31: Always use the stub now, in prep for matching FMSel's language stuff

            // BUG: Possible stub comm file not being deleted in the following scenario:
            // You launch a game through Steam, but the game doesn't actually launch (because you don't have
            // it in your Steam library or any other situation in which it gets cancelled). Because the game
            // never runs, it never deletes the stub comm file. The next time the game runs, it finds the stub
            // file and loads up whatever FM was specified. This won't happen if you launch an FM or original
            // game from AngelLoader, as we delete or overwrite the stub file ourselves before playing anything,
            // but if you were to run the game manually, it would load whatever FM was specified in the stub
            // once, and then delete it, so if you ran it again it would properly start the original game and
            // everything would be fine again.
            // I could solve it if there was a way to detect if we were being launched through Steam. I don't
            // know if there is, but then I could just specify a Steam=True line in the stub file, and then
            // if we're being launched through steam we read and act on it as usual, but if we're not, then
            // we just delete it and ignore.
            // I'll have to buy the games on Steam to test this. Or just buy one so I can have one game that
            // works and one that doesn't, so I can test both paths.

#if !ReleaseBeta && !ReleasePublic
            string args = Config.ForceWindowed ? "+force_windowed -fm" : "-fm";
#else
            string args = !steamArgs.IsEmpty() ? steamArgs : "-fm";
#endif

            GenerateMissFlagFileIfRequired(fm);

            WriteStubCommFile(fm, gamePath);

            StartExe(gameExe, workingPath, args);

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        internal static bool OpenFMInEditor(FanMission fm)
        {
            #region Checks (specific to DromEd)

            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is not Dark, is unknown, or is unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Core.View.ShowAlert(LText.AlertMessages.DromEd_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            // This is different from the above: The above is just checking if the game is known, while this is
            // checking if it's Dark specifically, because we don't support Thief 3 for editor opening.
            // This should never happen because our menu item is supposed to be hidden for Thief 3 FMs.
            if (!GameIsDark(fm.Game)) return false;

            var game = GameToGameIndex(fm.Game);

            string gamePath = Config.GetGamePath(game);
            if (gamePath.IsEmpty())
            {
                Log("Game path is empty for " + fm.Game, stackTrace: true);
                return false;
            }

            string editorExe = GetEditorExe(game);
            if (editorExe.IsEmpty())
            {
                Core.View.ShowAlert(fm.Game == Game.SS2
                    ? LText.AlertMessages.ShockEd_ExecutableNotFound
                    : LText.AlertMessages.DromEd_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            // Just in case, and for consistency
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            // TODO: We don't need to do this here, right?
            SetUsAsSelector(game, gamePath);

            // Since we don't use the stub currently, set this here
            // TODO: DromEd game mode doesn't even work for me anymore. Black screen no matter what. So I can't test if we need languages.
            SetCamCfgLanguage(gamePath, "");

            // Why not
            GenerateMissFlagFileIfRequired(fm);

            // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
            StartExe(editorExe, gamePath, "-fm=\"" + fm.InstalledDir + "\"");

            return true;
        }

        #endregion

        #region Helpers

        #region Find FM's supported languages

        internal static List<string> SortLangsToSpec(List<string> langs)
        {
            var ret = new List<string>();

            // Return a list of all found languages, sorted in the same order as FMSupportedLanguages
            // (matching FMSel behavior)
            if (langs.Count > 0)
            {
                for (int i = 0; i < FMSupportedLanguages.Length; i++)
                {
                    string sl = FMSupportedLanguages[i];
                    if (langs.ContainsI(sl)) ret.Add(sl);
                }
            }

            return ret;
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

            var langsFoundList = new List<string>(FMSupportedLanguages.Length);

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

            bool[] FoundLangInArchive = new bool[FMSupportedLanguages.Length];
            // Pre-concat each string only once for perf
            string[] SLangsFSPrefixed = new string[FMSupportedLanguages.Length];
            for (int i = 0; i < FMSupportedLanguages.Length; i++) SLangsFSPrefixed[i] = "/" + FMSupportedLanguages[i];

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

                    for (int j = 0; j < FMSupportedLanguages.Length; j++)
                    {
                        string sl = FMSupportedLanguages[j];
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

        private static (string Language, bool ForceLanguage)
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
                var fmSupportedLangs =
                    GetFMSupportedLanguages(fmArchive, fmInstPath, earlyOutOnEnglish: false);
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

        #endregion

        private static void WriteStubCommFile(FanMission? fm, string gamePath)
        {
            string sLanguage = "";
            bool? bForceLanguage = null;

            if (fm == null)
            {
                SetCamCfgLanguage(gamePath, "");
            }
            else if (GameIsDark(fm.Game))
            {
                bool langIsDefault = fm.SelectedLang.EqualsI(DefaultLangKey);
                if (langIsDefault)
                {
                    // For Dark, we have to do this semi-manual stuff.
                    (sLanguage, bForceLanguage) = GetDarkFMLanguage(GameToGameIndex(fm.Game), fm.Archive, fm.InstalledDir);
                }
                else
                {
                    sLanguage = fm.SelectedLang;
                    bForceLanguage = true;
                }

                SetCamCfgLanguage(gamePath, langIsDefault ? "" : fm.SelectedLang);
            }

            // For Thief 3, Sneaky Upgrade does the entire language thing for me, Builder bless snobel once again.
            // I just can't tell you how much I appreciate how much work SU does for me, even right down to handling
            // the "All The World's a Stage" custom sound extract thing.
            // So, I don't have to do anything whatsoever here, just pass blank and carry on. Awesome!

            try
            {
                // IMPORTANT:
                // Encoding MUST be "new UTF8Encoding(false, true)" or the C++ stub won't read it (it doesn't
                // handle the byte order mark).
                using var sw = new StreamWriter(Paths.StubCommFilePath, append: false,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
                sw.WriteLine("PlayOriginalGame=" + (fm == null));
                if (fm != null)
                {
                    sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                    sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
                    // Pass blank if we have nothing, so the stub will leave whatever was in there before
                    if (!sLanguage.IsEmpty()) sw.WriteLine("Language=" + sLanguage);
                    if (bForceLanguage != null) sw.WriteLine("ForceLanguage=" + (bool)bForceLanguage);
                }
            }
            catch (Exception ex)
            {
                Log("Exception writing stub file " + Paths.StubFileName, ex);
            }
        }

        // TODO: Pop up actual dialogs here if we fail, because we do NOT want scraps of the wrong language left
        internal static void SetCamCfgLanguage(string gamePath, string lang)
        {
            const string fmLanguage = "fm_language";
            const string fmLanguageForced = "fm_language_forced";

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamCfg, out string camCfg))
            {
                Log(Paths.CamCfg + " not found for " + gamePath, stackTrace: true);
                return;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camCfg).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading " + Paths.CamModIni + " for " + gamePath, ex);
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.StartsWithI(fmLanguage) || lineT.StartsWithI(fmLanguageForced))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }

            if (!lang.IsEmpty())
            {
                lines.Add(fmLanguage + " " + lang);
                lines.Add(fmLanguageForced + " 1");
            }

            try
            {
                File.WriteAllLines(camCfg, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing " + Paths.CamModIni + " for " + gamePath, ex);
                return;
            }
        }

        private static void StartExe(string exe, string workingPath, string args)
        {
            try
            {
                ProcessStart_UseShellExecute(new ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = workingPath,
                    Arguments = !args.IsEmpty() ? args : ""
                });
            }
            catch (Exception ex)
            {
                Log("Exception starting " + exe + "\r\n" +
                    "workingPath: " + workingPath + "\r\n" +
                    "args: " + args, ex);
            }
        }

        private static (bool Success, string GameExe, string GamePath)
        CheckAndReturnFinalGameExeAndGamePath(GameIndex gameIndex, bool playingOriginalGame, bool playMP = false)
        {
            var failed = (Success: false, GameExe: "", GamePath: "");

            string gameName = GetLocalizedGameName(gameIndex);

            string gameExe = Config.GetGameExe(gameIndex);

            #region Fail if game path is blank

            string gamePath = Config.GetGamePath(gameIndex);
            if (gamePath.IsEmpty())
            {
                Core.View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GamePathNotFound,
                    LText.AlertMessages.Alert);
                return failed;
            }

            #endregion

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            #region Exe: Fail if blank or not found

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
                string exeNotFoundMessage = playingOriginalGame
                    ? LText.AlertMessages.Play_ExecutableNotFound
                    : LText.AlertMessages.Play_ExecutableNotFoundFM;
                Core.View.ShowAlert(gameName + ":\r\n" + exeNotFoundMessage, LText.AlertMessages.Alert);
                return failed;
            }

            #endregion

            #region Exe: Fail if already running

            if (GameIsRunning(gameExe, checkAllGames: true))
            {
                Core.View.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
                return failed;
            }

            #endregion

            return (true, gameExe, gamePath);
        }

        private static (bool Success, string SteamExe, string SteamWorkingPath, string Args)
        GetSteamValues(GameIndex game, bool playMP)
        {
            // Multiplayer means starting Thief2MP.exe, so we can't really run it through Steam because Steam
            // will start Thief2.exe
            if (!playMP &&
                !GetGameSteamId(game).IsEmpty() && Config.GetUseSteamSwitch(game) &&
                Config.LaunchGamesWithSteam && !Config.SteamExe.IsEmpty() && File.Exists(Config.SteamExe))
            {
                string workingPath = Path.GetDirectoryName(Config.SteamExe);
                string args = "-applaunch " + GetGameSteamId(game);

                return (true, Config.SteamExe, workingPath, args);
            }
            else
            {
                return (false, "", "", "");
            }
        }

        #endregion

        private static void GenerateMissFlagFileIfRequired(FanMission fm)
        {
            // Only T1 and T2 have/require missflag.str
            if (fm.Game != Game.Thief1 && fm.Game != Game.Thief2) return;

            string instFMsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            string fmInstalledPath;
            try
            {
                fmInstalledPath = Path.Combine(instFMsBasePath, fm.InstalledDir);

                if (!Directory.Exists(fmInstalledPath)) return;

                string stringsPath = Path.Combine(fmInstalledPath, "strings");
                string missFlagFile = Path.Combine(stringsPath, "missflag.str");

                bool MissFlagFilesExist()
                {
                    if (!Directory.Exists(stringsPath)) return false;
                    // Missflag.str could be in a subdirectory too! Don't make a new one in that case!
                    string[] missFlag = Directory.GetFiles(stringsPath, "missflag.str", SearchOption.AllDirectories);
                    return missFlag.Length > 0;
                }

                if (MissFlagFilesExist()) return;

                string[] misFiles = Directory.GetFiles(fmInstalledPath, "miss*.mis", SearchOption.TopDirectoryOnly);
                var misNums = new List<int>();
                foreach (string mf in misFiles)
                {
                    Match m = Regex.Match(mf, @"miss(?<Num>\d+).mis", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (m.Success && int.TryParse(m.Groups["Num"].Value, out int result))
                    {
                        misNums.Add(result);
                    }
                }

                if (misNums.Count == 0) return;

                misNums.Sort();

                Directory.CreateDirectory(stringsPath);

                int lastMisNum = misNums[misNums.Count - 1];

                var missFlagLines = new List<string>();
                for (int i = 1; i <= lastMisNum; i++)
                {
                    string curLine = "miss_" + i + ": ";
                    if (misNums.Contains(i))
                    {
                        curLine += "\"no_briefing, no_loadout";
                        if (i == lastMisNum) curLine += ", end";
                        curLine += "\"";
                    }
                    else
                    {
                        curLine += "\"skip\"";
                    }

                    missFlagLines.Add(curLine);
                }

                File.WriteAllLines(missFlagFile, missFlagLines, new UTF8Encoding(false, true));
            }
            catch (Exception ex)
            {
                Log(nameof(GenerateMissFlagFileIfRequired) + ": Exception trying to write missflag.str file", ex);
                return;
            }
        }

        #region Set us as selector

        private static void SetUsAsSelector(GameIndex game, string gamePath)
        {
            bool success = GameIsDark(game) ? SetDarkFMSelector(game, gamePath) : SetT3FMSelector();
            if (!success)
            {
                Log("Unable to set us as the selector for " + Config.GetGameExe(game) + " (" +
                    (GameIsDark(game) ? nameof(SetDarkFMSelector) : nameof(SetT3FMSelector)) +
                    " returned false)", stackTrace: true);
            }
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        private static string FindPreviousSelector(List<string> lines, string fmSelectorKey, string stubPath,
            string gamePath)
        {
            // Handle relative paths
            static string GetFullPath(string _gamePath, string path)
            {
                if (PathIsRelative(path))
                {
                    try
                    {
                        return RelativeToAbsolute(_gamePath, path);
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

                return path;
            }

            string TryGetOtherSelectorSpecifier(string line)
            {
                string selectorFileName;
                // try-catch cause of Path.Combine() maybe trying to combine invalid-for-path strings
                // In .NET Core, we could use Path.Join() to avoid throwing
                try
                {
                    return line.StartsWithI(fmSelectorKey) && line.Length > fmSelectorKey.Length &&
                           char.IsWhiteSpace(line[fmSelectorKey.Length]) &&
                           (selectorFileName = line.Substring(fmSelectorKey.Length + 1)).EndsWithI(".dll") &&
                           !selectorFileName.PathEqualsI(stubPath) &&
                           !GetFullPath(gamePath, selectorFileName).IsEmpty() &&
                           File.Exists(Path.Combine(gamePath, selectorFileName))
                        ? selectorFileName
                        : "";
                }
                catch
                {
                    return "";
                }
            }

            var selectorsList = new List<string>();
            var commentedSelectorsList = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                string lt = lines[i].Trim();

                string selectorFileName;
                if (lt.Length > 0 && lt[0] == ';')
                {
                    do { lt = lt.TrimStart(CA_Semicolon).Trim(); } while (lt.Length > 0 && lt[0] == ';');

                    if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                    {
                        if (!commentedSelectorsList.PathContainsI(selectorFileName)) commentedSelectorsList.Add(selectorFileName);
                    }
                }
                else if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                {
                    if (!selectorsList.PathContainsI(selectorFileName)) selectorsList.Add(selectorFileName);
                }
            }

            return
                selectorsList.Count > 0 ? selectorsList[selectorsList.Count - 1] :
                commentedSelectorsList.Count > 0 ? commentedSelectorsList[commentedSelectorsList.Count - 1] :
                Paths.FMSelDll;
        }

        // 2019-10-16: We also now force the loader to start in the config files rather than just on the command
        // line. This is to support Steam launching, because Steam can't take game-specific command line arguments.

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static bool SetDarkFMSelector(GameIndex game, string gamePath, bool resetSelector = false)
        {
            const string fmSelectorKey = "fm_selector";
            const string fmCommentLine = "always start the FM Selector (if one is present)";

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                Log(Paths.CamModIni + " not found for " + Config.GetGameExe(game), stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camModIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading " + Paths.CamModIni + " for " + Config.GetGameExe(game), ex);
                return false;
            }

            string stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            bool changeLoaderIfResetting = true;

            if (resetSelector)
            {
                // NOTE: We're reading cam_mod.ini right here to grab these new values, that's why we're not
                // calling the regular cam_mod.ini reader method. Don't panic.

                // If the loader is now something other than us, then leave it be and don't change anything
                var tempSelectorsList = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();

                    string selectorFileName;
                    if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length]) &&
                        (selectorFileName = lt.Substring(fmSelectorKey.Length + 1)).EndsWithI(".dll"))
                    {
                        if (!tempSelectorsList.PathContainsI(selectorFileName)) tempSelectorsList.Add(selectorFileName);
                    }
                }

                if (tempSelectorsList.Count > 0 &&
                   !tempSelectorsList[tempSelectorsList.Count - 1].PathEqualsI(stubPath))
                {
                    changeLoaderIfResetting = false;
                }
            }

            // Confirmed NewDark can read fm_selector values with both forward and backward slashes

            // The loader is us, so use our saved previous loader or lacking that, make a best-effort guess
            var startupFMSelectorLines = Config.GetStartupFMSelectorLines(game);
            string selectorPath = resetSelector
                ? FindPreviousSelector(startupFMSelectorLines.Count > 0 ? startupFMSelectorLines : lines,
                    fmSelectorKey, stubPath, gamePath)
                : stubPath;

            bool prevAlwaysLoadSelector = Config.GetStartupAlwaysStartSelector(game);

            /*
             Conforms to the way NewDark reads it:
             - Zero or more whitespace characters allowed at the start of the line (before the key)
             - The key-value separator is one or more whitespace characters
             - Keys are case-insensitive
             - If duplicate keys exist, later ones replace earlier ones
             - Comment lines start with ;
             - No section headers
            */
            int lastSelKeyIndex = -1;
            int fmLineLastIndex = -1;
            int fmCommentLineIndex = -1;
            bool loaderIsAlreadyUs = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string lt = lines[i].TrimStart();

                do { lt = lt.TrimStart(CA_Semicolon).Trim(); } while (lt.Length > 0 && lt[0] == ';');

                // Steam robustness: get rid of any fan mission specifiers in here
                // line is "fm BrokenTriad_1_0" for example
                if (lt.StartsWithI("fm") && lt.Length > 2 && char.IsWhiteSpace(lt[2]) &&
                    lt.Substring(2).Trim().Length > 0)
                {
                    if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + lines[i];
                }

                if (fmCommentLineIndex == -1 && lt.EqualsI(fmCommentLine)) fmCommentLineIndex = i;

                if (fmLineLastIndex == -1 && lt.EqualsI("fm"))
                {
                    if (!resetSelector)
                    {
                        if (lines[i].TrimStart()[0] == ';') lines[i] = "fm";
                    }
                    else
                    {
                        if (prevAlwaysLoadSelector)
                        {
                            if (lines[i].TrimStart()[0] == ';') lines[i] = "fm";
                        }
                        else
                        {
                            if (lines[i].TrimStart()[0] != ';') lines[i] = ";fm";
                        }
                    }
                    fmLineLastIndex = i;
                }
                if (!resetSelector || changeLoaderIfResetting)
                {
                    if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length]) && lt
                            .Substring(fmSelectorKey.Length + 1).TrimStart()
                            .PathEqualsI(selectorPath))
                    {
                        if (loaderIsAlreadyUs)
                        {
                            lines.RemoveAt(i);
                            i--;
                            lastSelKeyIndex = (lastSelKeyIndex - 1).Clamp(-1, int.MaxValue);
                        }
                        else
                        {
                            lines[i] = fmSelectorKey + " " + selectorPath;
                            loaderIsAlreadyUs = true;
                        }
                        continue;
                    }

                    if (lt.EqualsI(fmSelectorKey) ||
                        (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length])))
                    {
                        if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + lines[i];
                        lastSelKeyIndex = i;
                    }
                }
            }

            if (!resetSelector || changeLoaderIfResetting)
            {
                if (!loaderIsAlreadyUs)
                {
                    if (lastSelKeyIndex == -1 || lastSelKeyIndex == lines.Count - 1)
                    {
                        lines.Add(fmSelectorKey + " " + selectorPath);
                    }
                    else
                    {
                        lines.Insert(lastSelKeyIndex + 1, fmSelectorKey + " " + selectorPath);
                    }
                }
            }

            if (fmLineLastIndex == -1)
            {
                if (fmCommentLineIndex == -1 || fmCommentLineIndex == lines.Count - 1)
                {
                    lines.Add("");
                    lines.Add("; " + fmCommentLine);
                    if (!resetSelector)
                    {
                        lines.Add("fm");
                    }
                    else
                    {
                        lines.Add(prevAlwaysLoadSelector ? "fm" : ";fm");
                    }
                }
                else
                {
                    if (!resetSelector)
                    {
                        lines.Insert(fmCommentLineIndex + 1, "fm");
                    }
                    else
                    {
                        lines.Insert(fmCommentLineIndex + 1, prevAlwaysLoadSelector ? "fm" : ";fm");
                    }
                }
            }

            try
            {
                File.WriteAllLines(camModIni, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing " + Paths.CamModIni + " for " + Config.GetGameExe(game), ex);
                return false;
            }

            return true;
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static bool SetT3FMSelector(bool resetSelector = false)
        {
            const string externSelectorKey = "ExternSelector=";
            const string alwaysShowKey = "AlwaysShow=";
            const string fanMissionKey = "FanMission=";
            bool existingExternSelectorKeyOverwritten = false;
            bool existingAlwaysShowKeyOverwritten = false;
            int insertLineIndex = -1;

            string soIni = Paths.GetSneakyOptionsIni();
            if (soIni.IsEmpty())
            {
                Log("Couldn't set us as the loader for Thief: Deadly Shadows because SneakyOptions.ini could not be found", stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(soIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Problem reading SneakyOptions.ini", ex);
                return false;
            }

            // Confirmed SU can read the selector value with both forward and backward slashes

            string stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            string selectorPath;

            bool changeLoaderIfResetting = true;

            #region Reset loader

            // TODO: @CourteousBehavior: Save and restore the "always start with FM" line(s)
            // Probably nobody uses this feature, but maybe we should do it for completeness?
            if (resetSelector)
            {
                var startupFMSelectorLines = Config.GetStartupFMSelectorLines(GameIndex.Thief3);
                string prevFMSelectorValue = "";

                #region Read the previous loader value

                for (int i = 0; i < lines.Count; i++)
                {
                    if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (lt.StartsWithI(externSelectorKey))
                        {
                            prevFMSelectorValue = lt.Substring(lt.IndexOf('=') + 1);
                            break;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        i++;
                    }
                    break;
                }

                #endregion

                // If loader is not us, leave it be
                if (!prevFMSelectorValue.PathEqualsI(stubPath) &&
                    !(startupFMSelectorLines.Count > 0 &&
                     startupFMSelectorLines[0].PathEqualsI(Paths.StubFileName)))
                {
                    selectorPath = "";
                    changeLoaderIfResetting = false;
                }
                else if (startupFMSelectorLines.Count > 0 &&
                    startupFMSelectorLines[0].PathEqualsI(Paths.StubFileName) ||
                    prevFMSelectorValue.IsEmpty())
                {
                    selectorPath = Paths.FMSelDll;
                }
                else
                {
                    selectorPath = startupFMSelectorLines.Count == 0 ||
                                   !startupFMSelectorLines[0].PathEqualsI(stubPath)
                        ? startupFMSelectorLines[0]
                        : Paths.FMSelDll;
                }
            }
            else
            {
                selectorPath = stubPath;
            }

            #endregion

            bool prevAlwaysShowValue = Config.GetStartupAlwaysStartSelector(GameIndex.Thief3);

            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    string lt = lines[i + 1].Trim();
                    if ((!resetSelector || changeLoaderIfResetting) &&
                        !existingExternSelectorKeyOverwritten &&
                        lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + selectorPath;
                        existingExternSelectorKeyOverwritten = true;
                    }
                    else if (!existingAlwaysShowKeyOverwritten &&
                        lt.StartsWithI(alwaysShowKey))
                    {
                        lines[i + 1] = alwaysShowKey + (resetSelector && !prevAlwaysShowValue ? "false" : "true");
                        existingAlwaysShowKeyOverwritten = true;
                    }
                    // Steam robustness: get rid of any fan mission specifiers in here
                    else if (lt.StartsWithI(fanMissionKey))
                    {
                        lines[i + 1] = fanMissionKey;
                    }

                    if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']') break;

                    i++;
                }
                break;
            }

            if ((!resetSelector || changeLoaderIfResetting) &&
                !existingExternSelectorKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, externSelectorKey + selectorPath);
            }

            if (!existingAlwaysShowKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, alwaysShowKey + "true");
            }

            try
            {
                File.WriteAllLines(soIni, lines);
            }
            catch (Exception ex)
            {
                Log("Problem writing SneakyOptions.ini", ex);
                return false;
            }

            return true;
        }

        #endregion

        #region Install

        internal static async Task<bool> InstallFM(FanMission fm)
        {
            #region Checks

            AssertR(!fm.Installed, "fm.Installed == false");

            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            if (fm.Game == Game.Unsupported)
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_UnsupportedGameType, LText.AlertMessages.Alert);
                return false;
            }

            string fmArchivePath = FindFMArchive(fm.Archive);

            if (fmArchivePath.IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            AssertR(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            string gameExe = Config.GetGameExeUnsafe(fm.Game);
            string gameName = GetLocalizedGameName(fm.Game);
            if (!File.Exists(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            string instBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            if (!Directory.Exists(instBasePath))
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_FMInstallPathNotFound, LText.AlertMessages.Alert);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            string fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            ExtractCts = new CancellationTokenSource();

            Core.View.ShowProgressBox(ProgressTasks.InstallFM);

            // Framework zip extracting is much faster, so use it if possible
            bool canceled = fmArchivePath.ExtIsZip()
                ? !await InstallFMZip(fmArchivePath, fmInstalledPath)
                : !await InstallFMSevenZip(fmArchivePath, fmInstalledPath);

            if (canceled)
            {
                Core.View.SetCancelingFMInstall();
                await Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(fmInstalledPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        Log("Unable to delete FM installed directory " + fmInstalledPath, ex);
                    }
                });
                Core.View.HideProgressBox();
                return false;
            }

            fm.Installed = true;

            Ini.WriteFullFMDataIni();

            try
            {
                using var sw = new StreamWriter(Path.Combine(fmInstalledPath, Paths.FMSelInf), append: false);
                await sw.WriteLineAsync("Name=" + fm.InstalledDir);
                await sw.WriteLineAsync("Archive=" + fm.Archive);
            }
            catch (Exception ex)
            {
                Log("Couldn't create " + Paths.FMSelInf + " in " + fmInstalledPath, ex);
            }

            // Only Dark engine games need audio conversion
            if (GameIsDark(fm.Game))
            {
                try
                {
                    Core.View.ShowProgressBox(ProgressTasks.ConvertFiles);

                    await FMAudio.ConvertMP3sToWAVs(fm);
                    if (Config.ConvertOGGsToWAVsOnInstall) await FMAudio.ConvertOGGsToWAVs(fm, false);
                    if (Config.ConvertWAVsTo16BitOnInstall) await FMAudio.ConvertWAVsTo16Bit(fm, false);
                }
                catch (Exception ex)
                {
                    Log("Exception in audio conversion", ex);
                }
            }

            // Don't be lazy about this; there can be no harm and only benefits by doing it right away
            GenerateMissFlagFileIfRequired(fm);

            // TODO: Put up a "Restoring saves and screenshots" box here to avoid the "converting files" one lasting beyond its time?
            try
            {
                await RestoreFM(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(RestoreFM), ex);
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            // Not doing RefreshSelectedFMRowOnly() because that wouldn't update the install/uninstall buttons
            await Core.View.RefreshSelectedFM(refreshReadme: false);

            return true;
        }

        private static async Task<bool> InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    using var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                        ZipArchiveMode.Read, leaveOpen: false);

                    int filesCount = archive.Entries.Count;
                    for (int i = 0; i < filesCount; i++)
                    {
                        var entry = archive.Entries[i];

                        string fileName = entry.FullName;

                        if (fileName[fileName.Length - 1].IsDirSep()) continue;

                        if (fileName.ContainsDirSep())
                        {
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                fileName.Substring(0, fileName.LastIndexOfDirSep())));
                        }

                        string extractedName = Path.Combine(fmInstalledPath, fileName);
                        entry.ExtractToFile(extractedName, overwrite: true);

                        UnSetReadOnly(Path.Combine(fmInstalledPath, extractedName));

                        int percent = GetPercentFromValue(i + 1, filesCount);

                        Core.View.InvokeAsync(new Action(() => Core.View.ReportFMExtractProgress(percent)));

                        if (ExtractCts.Token.IsCancellationRequested)
                        {
                            canceled = true;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception while installing zip " + fmArchivePath + " to " + fmInstalledPath, ex);
                    Core.View.InvokeSync(new Action(() =>
                        Core.View.ShowAlert(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                            LText.AlertMessages.Alert)));
                }
            });

            return !canceled;
        }

        private static async Task<bool> InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    using var extractor = new SevenZipExtractor(fmArchivePath);

                    extractor.Extracting += (sender, e) =>
                    {
                        if (!canceled && ExtractCts.Token.IsCancellationRequested)
                        {
                            canceled = true;
                        }
                        if (canceled)
                        {
                            Core.View.InvokeAsync(new Action(Core.View.SetCancelingFMInstall));
                            return;
                        }
                        Core.View.InvokeAsync(new Action(() => Core.View.ReportFMExtractProgress(e.PercentDone)));
                    };

                    extractor.FileExtractionFinished += (sender, e) =>
                    {
                        // We're extracting all the files, so we don't need to do an index check here.
                        if (!e.FileInfo.IsDirectory)
                        {
                            // We don't need to set timestamps because we're using ExtractArchive(), but we
                            // call this to remove the ReadOnly attribute
                            // TODO: Unset readonly for directories too
                            SetFileAttributesFromSevenZipEntry(e.FileInfo, Path.Combine(fmInstalledPath, e.FileInfo.FileName));
                        }

                        if (ExtractCts.Token.IsCancellationRequested)
                        {
                            Core.View.InvokeAsync(new Action(Core.View.SetCancelingFMInstall));
                            canceled = true;
                            e.Cancel = true;
                        }
                    };

                    try
                    {
                        extractor.ExtractArchive(fmInstalledPath);
                    }
                    catch (Exception ex)
                    {
                        // Throws a weird exception even if everything's fine
                        Log("extractor.ExtractArchive(fmInstalledPath) exception (probably ignorable)", ex);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception extracting 7z " + fmArchivePath + " to " + fmInstalledPath, ex);
                    Core.View.InvokeSync(new Action(() =>
                       Core.View.ShowAlert(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                           LText.AlertMessages.Alert)));
                }
            });

            return !canceled;
        }

        internal static void CancelInstallFM() => ExtractCts.CancelIfNotDisposed();

        #endregion

        #region Uninstall

        private static async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm.Game)) return;

            if (Config.ConfirmUninstall)
            {
                var (cancel, dontAskAgain) =
                    Core.View.AskToContinueYesNoCustomStrings(LText.AlertMessages.Uninstall_Confirm,
                        LText.AlertMessages.Confirm, TaskDialogIcon.Warning, showDontAskAgain: true,
                        LText.AlertMessages.Uninstall, LText.Global.Cancel);

                if (cancel) return;

                Config.ConfirmUninstall = !dontAskAgain;
            }

            string gameExe = Config.GetGameExeUnsafe(fm.Game);
            string gameName = GetLocalizedGameName(fm.Game);
            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning, LText.AlertMessages.Alert);
                return;
            }

            Core.View.ShowProgressBox(ProgressTasks.UninstallFM);

            try
            {
                string fmInstalledPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);

                bool fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    bool yes = Core.View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        await Core.View.RefreshSelectedFM(refreshReadme: false);
                    }
                    return;
                }

                string fmArchivePath = await Task.Run(() => FindFMArchive(fm.Archive));

                if (fmArchivePath!.IsEmpty())
                {
                    var (cancel, _) = Core.View.AskToContinueYesNoCustomStrings(
                        LText.AlertMessages.Uninstall_ArchiveNotFound, LText.AlertMessages.Warning,
                        TaskDialogIcon.Warning, showDontAskAgain: false, LText.AlertMessages.Uninstall,
                        LText.Global.Cancel);

                    if (cancel) return;
                }

                // If fm.Archive is blank, then fm.InstalledDir will be used for the backup file name instead.
                // This file will be included in the search when restoring, and the newest will be taken as
                // usual.

                // fm.Archive can be blank at this point when all of the following conditions are true:
                // -fm is installed
                // -fm does not have fmsel.inf in its installed folder (or its fmsel.inf is blank or invalid)
                // -fm was not in the database on startup
                // -the folder where the FM's archive is located is not in Config.FMArchivePaths (or its sub-
                //  folders if that option is enabled)

                // It's not particularly likely, but it could happen if the user had NDL-installed FMs (which
                // don't have fmsel.inf), started AngelLoader for the first time, didn't specify the right
                // archive folder on initial setup, and hasn't imported from NDL by this point.

                if (Config.BackupAlwaysAsk)
                {
                    string message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                        ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                        : LText.AlertMessages.Uninstall_BackupAllData;
                    var (cancel, cont, dontAskAgain) =
                        Core.View.AskToContinueWithCancelCustomStrings(
                            message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                            LText.AlertMessages.Confirm, null, showDontAskAgain: true,
                            LText.AlertMessages.BackUp, LText.AlertMessages.DontBackUp, LText.Global.Cancel);

                    if (cancel) return;

                    Config.BackupAlwaysAsk = !dontAskAgain;
                    if (cont) await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }
                else
                {
                    await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                if (!await DeleteFMInstalledDirectory(fmInstalledPath))
                {
                    // TODO: Make option to open the folder in Explorer and delete it manually?
                    Core.View.ShowAlert(LText.AlertMessages.Uninstall_UninstallNotCompleted, LText.AlertMessages.Alert);
                }

                fm.Installed = false;

                // NewDarkLoader still truncates its Thief 3 install names, but the "official" way is not to
                // do it for Thief 3. If the user already has FMs that were installed with NewDarkLoader, we
                // just read in the truncated names and treat them as normal for compatibility purposes. But
                // if we've just uninstalled the mission, then we can safely convert InstalledDir back to full
                // un-truncated form for future use.
                if (fm.Game == Game.Thief3 && !fm.Archive.IsEmpty())
                {
                    fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate: false);
                }

                Ini.WriteFullFMDataIni();
                await Core.View.RefreshSelectedFM(refreshReadme: false);
            }
            catch (Exception ex)
            {
                Log("Exception uninstalling FM " + fm.Archive + ", " + fm.InstalledDir, ex);
                Core.View.ShowAlert(LText.AlertMessages.Uninstall_FailedFullyOrPartially, LText.AlertMessages.Alert);
            }
            finally
            {
                Core.View.HideProgressBox();
            }
        }

        private static async Task<bool> DeleteFMInstalledDirectory(string path)
        {
            return await Task.Run(() =>
            {
                bool triedReadOnlyRemove = false;

                // Failsafe cause this is nasty
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        Directory.Delete(path, recursive: true);
                        return true;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (triedReadOnlyRemove) return false;

                            // FMs installed by us will not have any readonly attributes set, so we work on the
                            // assumption that this is the rarer case and only do this extra work if we need to.
                            foreach (string f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                            {
                                new FileInfo(f).IsReadOnly = false;
                            }

                            foreach (string d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                            {
                                new DirectoryInfo(d).Attributes = FileAttributes.Normal;
                            }

                            triedReadOnlyRemove = true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }

                return false;
            });
        }

        #endregion
    }
}
