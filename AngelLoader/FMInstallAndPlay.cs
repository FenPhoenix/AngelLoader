using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Ini;
using AngelLoader.WinAPI;
using AngelLoader.WinAPI.Ookii.Dialogs;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.FMBackupAndRestore;
#pragma warning disable 8509 // Switch expression doesn't handle all possible inputs

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
            var (success, gameExe, gamePath) = GetGameExeAndPath(game, LText.AlertMessages.Play_ExecutableNotFound);
            if (!success) return false;

            // Even though we're not actually loading an FM, we still want to set us as the selector so that our
            // stub can explicitly tell Thief to play without an FM. Otherwise, if another selector was specified,
            // then that selector would start upon running of the game exe, which would be bad.
            SetUsAsSelector(game, gameExe, gamePath);

            // When the stub finds nothing in the stub comm folder, it will just start the game with no FM
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            string args = "";
            var sv = GetSteamValues(game, playMP);
            if (sv.Success) (_, gameExe, gamePath, args) = sv;

            // TODO: Decide what to do about explicit play-original etc.
            WriteStubCommFile(null, playOriginalGame: true);

            StartExe(gameExe, gamePath, args);

            return true;
        }

        private static bool PlayFM(FanMission fm, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game)) return false;

            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.Play_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var game = GameToGameIndex(fm.Game);

            var (success, gameExe, gamePath) = GetGameExeAndPath(game, LText.AlertMessages.Play_ExecutableNotFoundFM, playMP);
            if (!success) return false;

            // Always do this for robustness, see below
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gameExe, gamePath);

            string steamArgs = "";
            var sv = GetSteamValues(game, playMP);
            if (sv.Success) (_, gameExe, gamePath, steamArgs) = sv;

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

            var args = !steamArgs.IsEmpty() ? steamArgs : "-fm";

            WriteStubCommFile(fm, playOriginalGame: false);

            StartExe(gameExe, gamePath, args);

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        internal static bool OpenFMInEditor(FanMission fm)
        {
            #region Checks (specific to DromEd)

            if (!GameIsDark(fm.Game))
            {
                Log("Game is not Dark, is unknown, or is unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                return false;
            }

            // TODO: This doesn't get hit anymore on account of the GameIsDark() check above
            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.DromEd_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var game = GameToGameIndex(fm.Game);

            var gameExe = Config.GetGameExe(game);
            if (gameExe.IsEmpty())
            {
                Log("gameExe is empty for " + fm.Game, stackTrace: true);
                return false;
            }

            var editorExe = GetEditorExe(game);
            if (editorExe.IsEmpty())
            {
                Core.View.ShowAlert(fm.Game == Game.SS2
                    ? LText.AlertMessages.ShockEd_ExecutableNotFound
                    : LText.AlertMessages.DromEd_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return false;

            #endregion

            // Just in case, and for consistency
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gameExe, gamePath);

            // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
            StartExe(editorExe, gamePath, "-fm=\"" + fm.InstalledDir + "\"");

            return true;
        }

        #endregion

        #region Helpers

        // So, we need to search the FM installed dir for language dirs no matter what we end up doing.
        // FMSel's code is hard to make out some of the details, but it looks like it may just be searching every
        // subfolder for any folder named after a language, then adding them to a list, earlying-out if it finds
        // English.
        // It also looks like it's picking the first language it finds as a fallback. That doesn't sound right.
        // Have to look at it more closely...

        internal static List<string> GetFMSupportedLanguages(string fmInstPath, bool earlyOutOnEnglish)
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
                var keyDir = i switch { 0 => "books", 1 => "intrface", _ => "strings" };

                for (int j = 0; j < searchList.Count; j++)
                {
                    if (j < searchList.Count - 1 && searchList[j].EndsWithI(Path.DirectorySeparatorChar + keyDir))
                    {
                        var item = searchList[j];
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

            var ret = new List<string>();

            // Return a list of all found languages, sorted in the same order as FMSupportedLanguages
            // (matching FMSel behavior)
            if (langsFoundList.Count > 0)
            {
                for (int i = 0; i < FMSupportedLanguages.Length; i++)
                {
                    string sl = FMSupportedLanguages[i];
                    if (langsFoundList.ContainsI(sl)) ret.Add(sl);
                }
            }

            return ret;
        }

        // TODO: Write archive (zip, 7z) searchers (should be way, way easier)

        private static void WriteStubCommFile(FanMission? fm, bool playOriginalGame)
        {
            // BUG: TODO: AngelLoader doesn't handle languages correctly. See FMSel code. Port this behavior asap!

            string sLanguage = "";
            bool bForceLanguage = false;

            GameIndex game;
            /*
             TODO: Sneaky Tweaker has a UI option to change the language priority.
             Read that value out of SneakyOptions.ini and use that instead of FMSupportedLanguages order. But note
             there's only 7: English, German, Italian, French, Russian, Polish, Spanish.
             So it's missing Czech, Dutch, Hungarian, Japanese. Neither here nor there, but good to note.
             SU's FMSel is closed-source so I don't know if it's doing a full-dir-structure scan like ND's FMsel,
             or if it's just looking in Books or something, but a full search would probably be okay(?)
             Or does the priorities list negate the need for that? I dunno, I'm tired. Check into this later.
             TODO: Do a comprehensive manual look at all folder names in all T3 FMs to try and suss this out.
            */
            if (fm != null && GameIsDark(game = GameToGameIndex(fm.Game)))
            {
                string gamePath = Path.GetDirectoryName(Config.GetGameExe(game));
                var (_, fmLanguage, fmLanguageForced, _) = Core.GetInfoFromCamModIni(gamePath, out _);

                // bForceLanguage gets set to something specific in every possible case, effectively meaning the
                // fm_language_forced value is always ignored. Weird, but FMSel's code does exactly this, so meh?
                // TODO: Verify FMSel 1.27's code is the same (I'm looking at 1.26 right now)
                var fmInstPath = Path.Combine(Config.GetFMInstallPath(game), fm.InstalledDir);
                if (!fmLanguage.IsEmpty())
                {

                    var fmSupportedLangs = GetFMSupportedLanguages(fmInstPath, earlyOutOnEnglish: false);
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
                     So, if I'm reading the API notes right, it looks like NewDark actually picks the right
                     language automatically if it can find it in DARKINST.CFG. We set sLanguage either:
                     -to force that language to be used if it's available (otherwise we use the below crappily-
                      guessed fallback), or
                     -to give it a crappily-guessed fallback value so that if NewDark can't find a language, we
                      at least have SOMETHING to give it so text won't be blank, even though it's likely it'll be
                      the wrong language if it isn't English.
                    */
                    // TODO: If I wanted, I could easily make a power-user option to pick the language to play the FM with
                    // ---

                    // FMSel's comment:
                    // determine FM default language (used if the FM doesn't support the language set in dark by the "language" cfg var)

                    // FMSel's comment:
                    // determine if FM has languages defined, if it does and english is among them, then english is the fallback
                    // if english is not among them then pick another, if no languages are found then no fallback language will
                    // be defined

                    var langs = GetFMSupportedLanguages(fmInstPath, earlyOutOnEnglish: true);

                    // Use first available language (which will be English if English is available)
                    sLanguage = langs.Count == 0 ? "" : langs[0];
                    bForceLanguage = false;
                }
            }

            try
            {
                // IMPORTANT: Encoding MUST be set to Default, otherwise the C++ stub won't read it properly
                using var sw = new StreamWriter(Paths.StubCommFilePath, append: false, Encoding.Default);
                sw.WriteLine("PlayOriginalGame=" + playOriginalGame);
                if (fm != null)
                {
                    sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                    sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
                    sw.WriteLine("Language=" + sLanguage);
                    sw.WriteLine("ForceLanguage=" + bForceLanguage);
                }
            }
            catch (Exception ex)
            {
                Log("Exception writing stub file " + Paths.StubFileName, ex);
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

        private static (bool Success, string gameExe, string gamePath)
        GetGameExeAndPath(GameIndex gameIndex, string exeNotFoundMessage, bool playMP = false)
        {
            (bool, string, string) failed = (false, "", "");

            var gameExe = Config.GetGameExe(gameIndex);

            #region Exe: Fail if blank or not found

            var gameName = GetGameNameFromGameType(gameIndex);

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
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

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty())
            {
                Core.View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GamePathNotFound,
                    LText.AlertMessages.Alert);
                return failed;
            }

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            return (true, gameExe, gamePath);
        }

        private static (bool Success, string GameExe, string GamePath, string Args)
        GetSteamValues(GameIndex game, bool playMP)
        {
            // Multiplayer means starting Thief2MP.exe, so we can't really run it through Steam because Steam
            // will start Thief2.exe
            if (!playMP &&
                !GetGameSteamId(game).IsEmpty() && Config.GetUseSteamSwitch(game) &&
                Config.LaunchGamesWithSteam && !Config.SteamExe.IsEmpty() && File.Exists(Config.SteamExe))
            {
                string gameExe = Config.SteamExe;
                string gamePath = Path.GetDirectoryName(Config.SteamExe);
                string args = "-applaunch " + GetGameSteamId(game);

                return (true, gameExe, gamePath, args);
            }
            else
            {
                return (false, "", "", "");
            }
        }

        #endregion

        #region Set us as selector

        private static void SetUsAsSelector(GameIndex game, string gameExe, string gamePath)
        {
            bool success = GameIsDark(game) ? SetDarkFMSelector(game, gameExe, gamePath) : SetT3FMSelector();
            if (!success)
            {
                Log("Unable to set us as the selector for " + gameExe + " (" +
                    (GameIsDark(game) ? nameof(SetDarkFMSelector) : nameof(SetT3FMSelector)) +
                    " returned false)", stackTrace: true);
            }
        }

        private static string FindPreviousLoader(List<string> lines, string fmSelectorKey, string stubPath,
            string gamePath)
        {
            // Handle relative paths
            // TODO: Duplicate code
            static string GetFullPath(string _gamePath, string path)
            {
                if (!path.IsEmpty() &&
                    (path.StartsWithFast_NoNullChecks(".\\") || path.StartsWithFast_NoNullChecks("..\\") ||
                     path.StartsWithFast_NoNullChecks("./") || path.StartsWithFast_NoNullChecks("../")))
                {
                    try
                    {
                        return Paths.RelativeToAbsolute(_gamePath, path);
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
                string selFullPath;
                // try-catch cause of Path.Combine() maybe trying to combine invalid-for-path strings
                // In .NET Core, we could use Path.Join() to avoid throwing
                try
                {
                    return (line.StartsWithI(fmSelectorKey) && line.Length > fmSelectorKey.Length &&
                            char.IsWhiteSpace(line[fmSelectorKey.Length]) &&
                            (selectorFileName = line.Substring(fmSelectorKey.Length + 1).ToBackSlashes()).EndsWithI(".dll") &&
                            !selectorFileName.EqualsI(stubPath) &&
                            !(selFullPath = GetFullPath(gamePath, selectorFileName)).IsEmpty() &&
                            File.Exists(Path.Combine(gamePath, selectorFileName)))
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
                    do { lt = lt.TrimStart(';').Trim(); } while (lt.Length > 0 && lt[0] == ';');

                    if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                    {
                        if (!commentedSelectorsList.ContainsI(selectorFileName)) commentedSelectorsList.Add(selectorFileName);
                    }
                }
                else if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                {
                    if (!selectorsList.ContainsI(selectorFileName)) selectorsList.Add(selectorFileName);
                }
            }

            return
                selectorsList.Count > 0 ? selectorsList[selectorsList.Count - 1] :
                commentedSelectorsList.Count > 0 ? commentedSelectorsList[commentedSelectorsList.Count - 1] :
                "fmsel.dll";
        }

        // 2019-10-16: We also now force the loader to start in the config files rather than just on the command
        // line. This is to support Steam launching, because Steam can't take game-specific command line arguments.

        // TODO: Make AngelLoader detect if there's only ONE other commented fm_selector line; if there is, then reset to that one. Otherwise, just reset to fmsel.dll.
        internal static bool SetDarkFMSelector(GameIndex game, string gameExe, string gamePath, bool resetSelector = false)
        {
            const string fmSelectorKey = "fm_selector";
            const string fmCommentLine = "always start the FM Selector (if one is present)";

            var camModIni = Path.Combine(gamePath, "cam_mod.ini");
            if (!File.Exists(camModIni))
            {
                Log("cam_mod.ini not found for " + gameExe, stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camModIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading cam_mod.ini for " + gameExe, ex);
                return false;
            }

            string stubPath = Path.Combine(Paths.Startup, Paths.StubFileName).ToBackSlashes();

            if (resetSelector)
            {
                // If the loader is now something other than us, then leave it be and don't change anything
                var tempSelectorsList = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();

                    string selectorFileName;
                    if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length]) &&
                        (selectorFileName = lt.Substring(fmSelectorKey.Length + 1).ToBackSlashes()).EndsWithI(".dll"))
                    {
                        if (!tempSelectorsList.ContainsI(selectorFileName)) tempSelectorsList.Add(selectorFileName);
                    }
                }

                if (tempSelectorsList.Count > 0 &&
                   !tempSelectorsList[tempSelectorsList.Count - 1].EqualsI(stubPath))
                {
                    return true;
                }
            }

            // Confirmed NewDark can read fm_selector values with both forward and backward slashes

            // The loader is us, so use our saved previous loader or lacking that, make a best-effort guess
            var startupFMSelectorLines = Config.GetStartupFMSelectorLines(game);
            string selectorPath = resetSelector
                ? FindPreviousLoader(startupFMSelectorLines.Count > 0 ? startupFMSelectorLines : lines,
                    fmSelectorKey, stubPath, gamePath)
                : stubPath;

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
            bool fmLineFound = false;
            int fmCommentLineIndex = -1;
            bool loaderIsAlreadyUs = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var lt = lines[i].TrimStart();

                do
                {
                    lt = lt.TrimStart(';').Trim();
                } while (lt.Length > 0 && lt[0] == ';');

                // Steam robustness: get rid of any fan mission specifiers in here
                // line is "fm BrokenTriad_1_0" for example
                if (lt.StartsWithI("fm") && lt.Length > 2 && char.IsWhiteSpace(lt[2]) &&
                    lt.Substring(2).Trim().Length > 0)
                {
                    if (!lines[i].TrimStart().StartsWith(";")) lines[i] = ";" + lines[i];
                }

                if (fmCommentLineIndex == -1 && lt.EqualsI(fmCommentLine)) fmCommentLineIndex = i;

                if (!fmLineFound && lt.EqualsI("fm"))
                {
                    if (lines[i].TrimStart().StartsWith(";")) lines[i] = "fm";
                    fmLineFound = true;
                }

                if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                    char.IsWhiteSpace(lt[fmSelectorKey.Length]) && lt
                        .Substring(fmSelectorKey.Length + 1).TrimStart().ToBackSlashes()
                        .EqualsI(selectorPath.ToBackSlashes()))
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
                    if (!lines[i].TrimStart().StartsWith(";")) lines[i] = ";" + lines[i];
                    lastSelKeyIndex = i;
                }
            }

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

            if (!fmLineFound)
            {
                if (fmCommentLineIndex == -1 || fmCommentLineIndex == lines.Count - 1)
                {
                    lines.Add("");
                    lines.Add("; " + fmCommentLine);
                    lines.Add("fm");
                }
                else
                {
                    lines.Insert(fmCommentLineIndex + 1, "fm");
                }
            }

            try
            {
                File.WriteAllLines(camModIni, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing cam_mod.ini for " + gameExe, ex);
                return false;
            }

            return true;
        }

        // If only you could do this with a command-line switch. You can say -fm to always start with the loader,
        // and you can say -fm=name to always start with the named FM, but you can't specify WHICH loader to use
        // on the command line. Only way to do it is through a file. Meh.
        internal static bool SetT3FMSelector(bool resetSelector = false)
        {
            const string externSelectorKey = "ExternSelector=";
            const string alwaysShowKey = "AlwaysShow=";
            const string fanMissionKey = "FanMission=";
            bool existingExternSelectorKeyOverwritten = false;
            bool existingAlwaysShowKeyOverwritten = false;
            int insertLineIndex = -1;

            var ini = Paths.GetSneakyOptionsIni();
            if (ini.IsEmpty())
            {
                Log("Couldn't set us as the loader for Thief: Deadly Shadows because SneakyOptions.ini could not be found", stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(ini, Encoding.Default).ToList();
            }
            catch (Exception ex)
            {
                Log("Problem reading SneakyOptions.ini", ex);
                return false;
            }

            // Confirmed SU can read the selector value with both forward and backward slashes

            var stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            string selectorPath;

            #region Reset loader

            if (resetSelector)
            {
                var startupFMSelectorLines = Config.GetStartupFMSelectorLines(GameIndex.Thief3);
                var t3Data = Core.GetInfoFromT3();
                // If loader is not us, leave it be
                if (!t3Data.PrevFMSelectorValue.ToBackSlashes().EqualsI(stubPath) &&
                    !(startupFMSelectorLines.Count > 0 &&
                     startupFMSelectorLines[0].EqualsI(Paths.StubFileName)))
                {
                    return true;
                }
                else if (startupFMSelectorLines.Count > 0 &&
                    startupFMSelectorLines[0].EqualsI(Paths.StubFileName) ||
                    t3Data.PrevFMSelectorValue.IsEmpty())
                {
                    selectorPath = "fmsel.dll";
                }
                else
                {
                    selectorPath = startupFMSelectorLines.Count == 0 ||
                                   !startupFMSelectorLines[0].ToBackSlashes().EqualsI(stubPath)
                        ? startupFMSelectorLines[0]
                        : "fmsel.dll";
                }
            }
            else
            {
                selectorPath = stubPath;
            }

            #endregion

            for (var i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    var lt = lines[i + 1].Trim();
                    if (!existingExternSelectorKeyOverwritten &&
                        lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + selectorPath;
                        existingExternSelectorKeyOverwritten = true;
                    }
                    else if (!existingAlwaysShowKeyOverwritten &&
                        lt.StartsWithI(alwaysShowKey))
                    {
                        lines[i + 1] = alwaysShowKey + "true";
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

            if (!existingExternSelectorKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, externSelectorKey + selectorPath);
            }

            if (!existingAlwaysShowKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, alwaysShowKey + "true");
            }

            try
            {
                File.WriteAllLines(ini, lines, Encoding.Default);
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

            Debug.Assert(!fm.Installed, "!fm.Installed");

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

            var fmArchivePath = FindFMArchive(fm.Archive);

            if (fmArchivePath.IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var gameExe = Config.GetGameExeUnsafe(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (!File.Exists(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var instBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

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

            var fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

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

            Ini.Ini.WriteFullFMDataIni();

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

            // TODO: Put up a "Restoring saves and screenshots" box here to avoid the "converting files" one lasting beyond its time?
            try
            {
                await RestoreSavesAndScreenshots(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(RestoreSavesAndScreenshots), ex);
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

                    var fs = new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read);

                    using var archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false);

                    int filesCount = archive.Entries.Count;
                    for (var i = 0; i < filesCount; i++)
                    {
                        var entry = archive.Entries[i];

                        var fileName = entry.FullName.ToBackSlashes();

                        if (fileName[fileName.Length - 1] == '\\') continue;

                        if (fileName.Contains('\\'))
                        {
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                fileName.Substring(0, fileName.LastIndexOf('\\'))));
                        }

                        var extractedName = Path.Combine(fmInstalledPath, fileName);
                        entry.ExtractToFile(extractedName, overwrite: true);

                        UnSetReadOnly(Path.Combine(fmInstalledPath, extractedName));

                        int percent = (100 * (i + 1)) / filesCount;

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

            var gameExe = Config.GetGameExeUnsafe(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning, LText.AlertMessages.Alert);
                return;
            }

            Core.View.ShowProgressBox(ProgressTasks.UninstallFM);

            try
            {
                var fmInstalledPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);

                var fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    var yes = Core.View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        await Core.View.RefreshSelectedFM(refreshReadme: false);
                    }
                    return;
                }

                var fmArchivePath = await Task.Run(() => FindFMArchive(fm.Archive));

                if (fmArchivePath.IsEmpty())
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
                    var message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
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

                Ini.Ini.WriteFullFMDataIni();
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
                var triedReadOnlyRemove = false;

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
                            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                            {
                                new FileInfo(f).IsReadOnly = false;
                            }

                            foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
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
