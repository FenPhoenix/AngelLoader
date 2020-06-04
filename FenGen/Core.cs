﻿// NOTE: This code is provided for completeness' sake. It was written to do the job quickly without regard to any
// kind of good coding practices whatsoever. Don't write a public-facing program this way. Thank you.

//#define PROFILING

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using static FenGen.Misc;

namespace FenGen
{
    internal enum GenType
    {
        FMData,
        Config,
        Language,
        GameSupport,
        VisLoc
    }

    internal static class GenMessages
    {
        internal const string Method = @"// This method was autogenerated for maximum performance at runtime.";
        internal const string SupportingCode = @"// This supporting code was autogenerated.";
    }

    internal sealed class GameSourceEnum
    {
        internal string Name = "";
        internal readonly List<string> Items = new List<string>();
    }

    internal static class Cache
    {
        private static string _gameSupportFile = "";
        internal static void SetGameSupportFile(string file) => _gameSupportFile = file;
        private static GameSourceEnum? _gamesEnum;
        internal static GameSourceEnum GamesEnum => _gamesEnum ??= Games.FillGamesEnum(_gameSupportFile);
    }

    internal static class GenFileTags
    {
        internal const string LocalizationSource = "FenGen_LocalizationSource";
        internal const string LocalizationDest = "FenGen_LocalizationDest";
        internal const string GameSupport = "FenGen_GameSupport";
        internal const string FMDataSource = "FenGen_FMDataSource";
        internal const string FMDataDest = "FenGen_FMDataDest";
    }

    internal static class GenTaskArgs
    {
        internal const string FMData = "-fmdata";
        internal const string Language = "-language";
        internal const string LanguageAndTest = "-language_t";
        internal const string GameSupport = "-game_support";
    }

    internal static class Core
    {
        private static readonly int GenTaskCount = Enum.GetValues(typeof(GenType)).Length;

        private static readonly bool[] _genTasksActive = new bool[GenTaskCount];

        private static void SetGenTaskActive(GenType genType) => _genTasksActive[(int)genType] = true;
        private static bool GenTaskActive(GenType genType) => _genTasksActive[(int)genType];

        private static bool AnyGenTasksActive()
        {
            for (int i = 0; i < _genTasksActive.Length; i++)
            {
                if (_genTasksActive[i]) return true;
            }
            return false;
        }

#if DEBUG
        private static MainForm View;
#endif

        internal static readonly string ALSolutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
        internal static readonly string ALProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\AngelLoader"));

        // PERF_TODO: Roslyn is so slow it's laughable. It takes 1.5 seconds just to run InitWorkspaceStuff() alone.
        // That's not even counting doing any actual work with it, which adds even more slug time.
        // Go back to manually doing it all, and just organize AL so the gen can find things without too much
        // brittleness.
        // -ParseText() seems to have like a 200ms init time the first time you call it. See what I goddamn mean.

        internal static void Init()
        {
#if Release
            ReadArgsAndDoTasks();

            Environment.Exit(0);
#else
            View = new MainForm();
            View.Show();
#endif
        }

        private static void ExitIfRelease() => Environment.Exit(1);

        [PublicAPI]
        internal static void ReadArgsAndDoTasks()
        {
#if DEBUG
            //return;
#endif

            // args[0] is always the application filename
            //string[] args = Environment.GetCommandLineArgs();

#if PROFILING
            string[] args =
            {
                Environment.GetCommandLineArgs()[0],
                GenTaskArgs.FMData,
                GenTaskArgs.LanguageAndTest
            };
#else
            string[] args = Environment.GetCommandLineArgs();
#endif

            if (args.Length < 2) ExitIfRelease();

            bool generateLangTestFile = false;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case GenTaskArgs.FMData:
                        SetGenTaskActive(GenType.FMData);
                        break;
                    case GenTaskArgs.Language:
                    case GenTaskArgs.LanguageAndTest:
                        SetGenTaskActive(GenType.Language);
                        generateLangTestFile = args[i] == GenTaskArgs.LanguageAndTest;
                        break;
                    case GenTaskArgs.GameSupport:
                        SetGenTaskActive(GenType.GameSupport);
                        break;
                }

                if (!AnyGenTasksActive())
                {
                    ExitIfRelease();
                    return;
                }
            }

            var genFileTags = new List<string>();
            bool gameSupportRequested = GenTaskActive(GenType.FMData) || GenTaskActive(GenType.GameSupport);
            if (gameSupportRequested)
            {
                genFileTags.Add(GenFileTags.GameSupport);
            }
            if (GenTaskActive(GenType.FMData))
            {
                genFileTags.Add(GenFileTags.FMDataSource);
                genFileTags.Add(GenFileTags.FMDataDest);
            }
            if (GenTaskActive(GenType.Language))
            {
                genFileTags.Add(GenFileTags.LocalizationSource);
                genFileTags.Add(GenFileTags.LocalizationDest);
            }

            Dictionary<string, string>? taggedFilesDict = null;
            if (genFileTags.Count > 0)
            {
                taggedFilesDict = FindRequiredCodeFiles(genFileTags);
            }

            if (gameSupportRequested)
            {
                Cache.SetGameSupportFile(taggedFilesDict![GenFileTags.GameSupport]);
            }

            if (GenTaskActive(GenType.FMData))
            {
                FMData.Generate(
                    taggedFilesDict![GenFileTags.FMDataSource],
                    taggedFilesDict![GenFileTags.FMDataDest]);
            }
            if (GenTaskActive(GenType.Language))
            {
                string englishIni = Path.Combine(ALProjectPath, @"Languages\English.ini");
                string testLangIni = generateLangTestFile
                    ? @"C:\AngelLoader\Data\Languages\TestLang.ini"
                    : "";
                LanguageGen.Generate(
                    taggedFilesDict![GenFileTags.LocalizationSource],
                    taggedFilesDict![GenFileTags.LocalizationDest],
                    englishIni,
                    testLangIni);
            }
            if (GenTaskActive(GenType.GameSupport))
            {
                Games.Generate();
            }
        }

        [MustUseReturnValue]
        private static Dictionary<string, string>
        FindRequiredCodeFiles(List<string> genFileTags)
        {
            var taggedFiles = new List<string>[genFileTags.Count];
            for (int i = 0; i < taggedFiles.Length; i++)
            {
                taggedFiles[i] = new List<string>();
            }

            string[] files = Directory.GetFiles(ALProjectPath, "*.cs", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                using (var sr = new StreamReader(f))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.IsWhiteSpace()) continue;
                        string lts = line.TrimStart();

                        if (lts.Length > 0 && lts[0] != '#') break;

                        if (lts.StartsWith("#define") && lts.Length > 7 && char.IsWhiteSpace(lts[7]))
                        {
                            string tag = lts.Substring(7).Trim();

                            for (var index = 0; index < genFileTags.Count; index++)
                            {
                                string genFileTag = genFileTags[index];
                                if (tag == genFileTag)
                                {
                                    taggedFiles[index].Add(f);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            #region Error reporting

            static string AddError(string msg, string add)
            {
                if (msg.IsEmpty()) msg = "ERRORS:";
                msg += "\r\n" + add;
                return msg;
            }

            string error = "";
            for (int i = 0; i < taggedFiles.Length; i++)
            {
                if (taggedFiles[i].Count == 0)
                {
                    error = AddError(error, "-No file found with '#define " + genFileTags[i] + "' at top");
                }
                else if (taggedFiles[i].Count > 1)
                {
                    error = AddError(error, "-Multiple files found with '#define " + genFileTags[i] + "' at top");
                }
            }
            if (!error.IsEmpty()) ThrowErrorAndTerminate(error);

            #endregion

            var ret = new Dictionary<string, string>();
            for (int i = 0; i < genFileTags.Count; i++)
            {
                ret.Add(genFileTags[i], taggedFiles[i][0]);
            }
            return ret;
        }
    }
}
