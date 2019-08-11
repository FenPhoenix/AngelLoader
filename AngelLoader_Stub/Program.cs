using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

/*
 We have NewDark executables call out to this stub program, which provides the NewDark game with data that has
 been passed to it by AngelLoader via a temp file. We do it this way in order to support AngelLoader being
 standalone, but we would do it this way even if that wasn't the case, due to some unfortunate unexplained
 tech voodoo (the bad kind) that happens when NewDark games call out to C# code. Short version, when called
 into by unmanaged code and ONLY when called into by unmanaged code, the C# app acts like it's running on an
 earlier version of the .NET Framework, sort of, but enough to cause problems. You can take the exact same dll,
 call into it from managed code, and everything's fine. I debugged and tested the hell out of it, tried every
 possible theory I could, and I came up utterly empty-handed as to why it happens. Good thing being standalone
 neatly sidesteps the entire issue. Take that, gremlins.

 However, because of this, we want to do as little as possible in here and not do anything fancy that might not
 work as expected.

 Note: From the default cam_mod.ini file:

 ---snip---

 FM selection can also be done with command-line options (which override mod.ini)
   -fm        : to start the FM Selector
   -fm=name   : to start game with 'name' as active FM

 ---snip---

 If we just wanted to play FMs and be done with it, we could just pass fm=[name] on the command line and avoid
 having to have this stub altogether. But unfortunately you can't pass anything else on the command line (disabled
 mods etc.) so we have to have this.
 
 Or at least no other command-line options are listed anywhere that I can find.

 UPDATE: As of 2019/3/31, we only use this stub if we actually need to pass mod excludes. Otherwise, we just call
 the game and pass it the FM on the command line, as that's much cleaner.
*/
namespace AngelLoader_Stub
{
    #region NewDark FM selector API data

    // All comments are as they appear in fm_selector_api.txt in the NewDark package.

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches FMSel API naming")]
    internal enum eFMSelReturn
    {
        // run selected FM 'data->sName' (0-len string to run without an FM)
        kSelFMRet_OK = 0,

        // cancel FM selection and start game as-is (no FM or if defined in cam_mod.ini use that)
        kSelFMRet_Cancel = -1,

        // abort and quit game
        kSelFMRet_ExitGame = 1
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches FMSel API naming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal struct sFMSelectorData
    {
        // sizeof(sFMSelectorData)
        internal int nStructSize;

        // game version string as returned by AppName() (ie. in the form "Thief 2 Final 1.19")
        internal string sGameVersion;

        // supplied initial FM root path (the FM selector may change this)
        internal IntPtr sRootPath;
        internal int nMaxRootLen;

        // buffer to copy the selected FM name
        internal IntPtr sName;
        internal int nMaxNameLen;

        // set to non-zero when selector is invoked after game exit (if requested during game start)
        internal int bExitedGame;

        // FM selector should set this to non-zero if it wants to be invoked after game exits (only done for FMs)
        internal int bRunAfterGame;

        // optional list of paths to exclude from mod_path/uber_mod_path in + separated format and like the config
        // vars, or if "*" all mod paths are excluded (leave buffer empty for no excludes)
        // the specified exclude paths work as if they had a "*\" wildcard prefix
        internal IntPtr sModExcludePaths;
        internal int nMaxModExcludeLen;

        // language setting for FM (set by the FM selector when an FM is selected), may be empty if FM has no
        // language specific resources
        // when 'bForceLanguage' is 0 this is used to ensure an FM runs correctly even if it doesn't support
        // the game's current language setting (set by the "language" config var)
        // when 'bForceLanguage' is 1 this is used to force a language (that must be supported by the FM) other
        // than the game's current language
        internal IntPtr sLanguage;
        internal int nLanguageLen;
        internal int bForceLanguage;
    }

    #endregion

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal sealed class ArgsFileData
    {
        // These need to be non-null
        internal string SelectedFMName = "";
        internal string DisabledMods = "";
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal enum StubResponseError
    {
        RootTooLong,
        NameTooLong,
        ModExcludeTooLong,
        LanguageTooLong
    }

    [UsedImplicitly]
    internal static class Program
    {
        private static readonly string BaseTempPath = Path.Combine(Path.GetTempPath(), "AngelLoader");
        private static readonly string StubCommTempPath = Path.Combine(BaseTempPath, "Stub");
        private static readonly string ArgsFilePath = Path.Combine(StubCommTempPath, "al_stub_args.tmp");
        private static readonly string ResponseFilePath = Path.Combine(StubCommTempPath, "al_stub_response.tmp");

        private static void ReadArgsFile(string path, ArgsFileData argsFileData)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null || !line.Contains("=")) continue;

                    var key = line.Substring(0, line.IndexOf('='));
                    var value = line.Substring(line.IndexOf('=') + 1);

                    // Reflection cause who cares, it's not a bottleneck
                    var p = argsFileData.GetType().GetField(key, BindingFlags.IgnoreCase |
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.Instance);
                    if (p != null) p.SetValue(argsFileData, value);
                }
            }
        }

        private static void WriteResponseErrorFile(string path, List<StubResponseError> errors)
        {
            if (errors == null || errors.Count == 0) return;

            using (var sw = new StreamWriter(path, append: false))
            {
                foreach (var error in errors)
                {
                    sw.WriteLine(error.ToString());
                }
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "SelectFM")]
        [STAThread]
        [UsedImplicitly]
        // This doesn't need to be public. It can even be private and still work. But, since I have incomplete
        // knowledge of what goes on with this semi-undocumented exporting stuff, and no knowledge at all of what
        // NewDark's calling code looks like, I'm being abundantly cautious...
        public static int SelectFM([MarshalAs(UnmanagedType.Struct)] ref sFMSelectorData data)
        {
            // data.sGameVersion:
            // Might eventually use
            // Note: actually don't think this is of any use, since we already know what game we're running.
            // Maybe for getting the NewDark version?

            // data.sRootPath:
            // Will almost certainly never use
            // If AngelLoader was running by being called directly into by the game as intended, then this would
            // be a really useful way to not have to scan cam_mod.ini for the FM installed path. But as it stands
            // we need to know that path long before even getting here, so it's not very useful.

            // data.sLanguage / data.bForceLanguage:
            // Might use if it will help with multi-language stuff

            // TODO: Potential encoding nastiness alert: byte arrays might not be right

            // Using these
            var sName = new byte[data.nMaxNameLen];
            var sModExcludePaths = new byte[data.nMaxModExcludeLen];

            var fmInstalledFolderName = "";
            var disabledMods = "";
            if (File.Exists(ArgsFilePath))
            {
                var cmdFileData = new ArgsFileData();
                try
                {
                    ReadArgsFile(ArgsFilePath, cmdFileData);
                    File.Delete(ArgsFilePath);

                    #region Check for errors

                    var errors = new List<StubResponseError>();
                    if (cmdFileData.SelectedFMName.Length > 30)
                    {
                        errors.Add(StubResponseError.NameTooLong);
                    }
                    if (cmdFileData.DisabledMods.Length > data.nMaxModExcludeLen)
                    {
                        errors.Add(StubResponseError.ModExcludeTooLong);
                    }

                    if (errors.Count > 0)
                    {
                        WriteResponseErrorFile(ResponseFilePath, errors);
                        return (int)eFMSelReturn.kSelFMRet_ExitGame;
                    }

                    #endregion
                }
                catch (Exception)
                {
                    return (int)eFMSelReturn.kSelFMRet_Cancel;
                }

                fmInstalledFolderName = cmdFileData.SelectedFMName;
                disabledMods = cmdFileData.DisabledMods;
            }

            eFMSelReturn FMSelReturnValue;

            // If no FM folder specified, play the original game
            if (string.IsNullOrEmpty(fmInstalledFolderName))
            {
                FMSelReturnValue = eFMSelReturn.kSelFMRet_Cancel;
            }
            else
            {
                sName = Encoding.ASCII.GetBytes(fmInstalledFolderName + "\0");
                sModExcludePaths = Encoding.ASCII.GetBytes(disabledMods + "\0");

                FMSelReturnValue = eFMSelReturn.kSelFMRet_OK;
            }

            Marshal.Copy(sName, 0, data.sName, sName.Length);
            Marshal.Copy(sModExcludePaths, 0, data.sModExcludePaths, sModExcludePaths.Length);

            // Never call us back; we're standalone and don't need it
            data.bRunAfterGame = 0;

            return (int)FMSelReturnValue;
        }
    }
}
