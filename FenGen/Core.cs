﻿// FenGen - Fen's code generator for AngelLoader
// Not perfect by any means, and still with lots of hardcoded stuff that shouldn't be, but it gets the job done.

//#define PROFILING

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static FenGen.Misc;

namespace FenGen
{
    internal static class GenMessages
    {
        internal const string Method = "// This method was autogenerated for maximum performance at runtime.";
    }

    internal sealed class GameSourceEnum
    {
        internal string Name = "";
        internal readonly List<string> GameEnumNames = new List<string>();
        internal readonly List<string> GameIndexEnumNames = new List<string>();
        internal readonly List<string> GamePrefixes = new List<string>();
    }

    // TODO: Nasty global state that's really just here to avoid over-parameterization.
    internal static class Cache
    {
        private static string _gameSupportFile = "";
        internal static void SetGameSupportFile(string file) => _gameSupportFile = file;
        private static GameSourceEnum? _gamesEnum;
        internal static GameSourceEnum GamesEnum => _gamesEnum ??= Games.FillGamesEnum(_gameSupportFile);

        private static List<string>? _csFiles;
        internal static List<string> CSFiles => _csFiles ??= Directory.GetFiles(Core.ALProjectPath, "*.cs", SearchOption.AllDirectories).ToList();

        internal static readonly List<string> DesignerCSFiles = new List<string>();

        internal static void Clear()
        {
            _gameSupportFile = "";
            _gamesEnum = null;
            _csFiles = null;
            DesignerCSFiles.Clear();
        }
    }

    internal static class GenAttributes
    {
        #region Serialization

        internal const string FenGenFMDataSourceClass = nameof(FenGenFMDataSourceClass);
        internal const string FenGenIgnore = nameof(FenGenIgnore);
        internal const string FenGenDoNotWrite = nameof(FenGenDoNotWrite);
        internal const string FenGenIniName = nameof(FenGenIniName);
        internal const string FenGenNumericEmpty = nameof(FenGenNumericEmpty);
        internal const string FenGenListType = nameof(FenGenListType);
        internal const string FenGenListDistinctType = nameof(FenGenListDistinctType);
        internal const string FenGenDoNotTrimValue = nameof(FenGenDoNotTrimValue);
        internal const string FenGenDoNotConvertDateTimeToLocal = nameof(FenGenDoNotConvertDateTimeToLocal);

        #endregion

        #region Localizable text

        internal const string FenGenLocalizationSourceClass = nameof(FenGenLocalizationSourceClass);
        internal const string FenGenLocalizationDestClass = nameof(FenGenLocalizationDestClass);
        internal const string FenGenComment = nameof(FenGenComment);
        internal const string FenGenBlankLine = nameof(FenGenBlankLine);

        #endregion

        #region Game support

        internal const string FenGenGameEnum = nameof(FenGenGameEnum);
        internal const string FenGenNotAGameType = nameof(FenGenNotAGameType);
        internal const string FenGenGamePrefixes = nameof(FenGenGamePrefixes);

        #endregion

        internal const string FenGenExcludeResx = nameof(FenGenExcludeResx);

        internal const string FenGenBuildDateDestClass = nameof(FenGenBuildDateDestClass);

        internal const string FenGenDoNotRemoveTextAttribute = nameof(FenGenDoNotRemoveTextAttribute);
        internal const string FenGenDoNotRemoveHeaderTextAttribute = nameof(FenGenDoNotRemoveHeaderTextAttribute);
        internal const string FenGenDoNotRemoveToolTipTextAttribute = nameof(FenGenDoNotRemoveToolTipTextAttribute);

        internal const string FenGenForceRemoveSizeAttribute = nameof(FenGenForceRemoveSizeAttribute);
    }

    internal static class Core
    {
        private enum GenType
        {
            FMData,
            Config,
            Language,
            LanguageAndAlsoCreateTestIni,
            EnableLangReflectionStyleGen,
            GameSupport,
            ExcludeResx,
            RestoreResx,
            AddBuildDate,
            RemoveBuildDate,
            GenSlimDesignerFiles
        }

        private static readonly Dictionary<string, GenType>
        _argToTaskMap = new Dictionary<string, GenType>
        {
            { "-fmdata", GenType.FMData },
            { "-language", GenType.Language },
            { "-language_t", GenType.LanguageAndAlsoCreateTestIni },
            { "-enable_lang_reflection_style_gen", GenType.EnableLangReflectionStyleGen },
            { "-game_support", GenType.GameSupport },
            { "-exclude_resx", GenType.ExcludeResx },
            { "-restore_resx", GenType.RestoreResx },
            { "-add_build_date", GenType.AddBuildDate },
            { "-remove_build_date", GenType.RemoveBuildDate },
            { "-gen_slim_designer_files", GenType.GenSlimDesignerFiles }
        };

        // Only used for debug, so we can explicitly place test arguments into the set
#if DEBUG || PROFILING
        private static string GetArg(GenType genType)
        {
            foreach (var item in _argToTaskMap)
            {
                if (item.Value == genType) return item.Key;
            }

            throw new ArgumentException(nameof(GetArg) + ": No matching arg string found for genType");
        }
#endif

        private static class GenFileTags
        {
            internal const string LocalizationSource = "FenGen_LocalizationSource";
            internal const string LocalizationDest = "FenGen_LocalizationDest";
            internal const string GameSupport = "FenGen_GameSupport";
            internal const string FMDataSource = "FenGen_FMDataSource";
            internal const string FMDataDest = "FenGen_FMDataDest";
            internal const string DesignerSource = "FenGen_DesignerSource";
            internal const string BuildDate = "FenGen_BuildDateDest";
        }

        private static readonly int _genTaskCount = Enum.GetValues(typeof(GenType)).Length;

#if DEBUG
        private static Forms.MainForm View = null!;
#endif

        internal static readonly string ALSolutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
        internal static readonly string ALProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\AngelLoader"));
        internal static readonly string ALProjectFile = Path.Combine(ALProjectPath, "AngelLoader.csproj");

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
            View = new Forms.MainForm();
            View.Show();
#endif
        }

        private static void ExitIfRelease() => Environment.Exit(1);

        [PublicAPI]
        internal static void ReadArgsAndDoTasks()
        {
            try
            {
                ReadArgsAndDoTasksInternal();
            }
            catch (Exception ex)
            {
                ThrowErrorAndTerminate(ex);
            }
            finally
            {
                Cache.Clear();
            }
        }

        private static void ReadArgsAndDoTasksInternal()
        {
            // args[0] is always the application filename

#if DEBUG || PROFILING
            string[] args =
            {
                Environment.GetCommandLineArgs()[0],
                GetArg(GenType.FMData),
                GetArg(GenType.LanguageAndAlsoCreateTestIni),
                GetArg(GenType.EnableLangReflectionStyleGen),
                GetArg(GenType.ExcludeResx),
                //GetArg(GenType.RestoreResx),
                GetArg(GenType.AddBuildDate),
                GetArg(GenType.GenSlimDesignerFiles)
            };
#else
            string[] args = Environment.GetCommandLineArgs();
#endif

            if (args.Length < 2) ExitIfRelease();

            bool[] _genTasksActive = new bool[_genTaskCount];

            #region Local functions

            void SetGenTaskActive(GenType genType) => _genTasksActive[(int)genType] = true;

            bool GenTaskActive(GenType genType) => _genTasksActive[(int)genType];

            bool LangTaskActive() => GenTaskActive(GenType.Language) ||
                                     GenTaskActive(GenType.LanguageAndAlsoCreateTestIni);

            bool AnyGenTasksActive()
            {
                for (int i = 0; i < _genTasksActive.Length; i++)
                {
                    if (_genTasksActive[i]) return true;
                }
                return false;
            }

            #endregion

            for (int i = 1; i < args.Length; i++)
            {
                if (_argToTaskMap.TryGetValue(args[i], out GenType genType)) SetGenTaskActive(genType);
            }

            if (!AnyGenTasksActive())
            {
                ExitIfRelease();
                return;
            }

            bool forceFindRequiredFiles = false;
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
            if (LangTaskActive())
            {
                genFileTags.Add(GenFileTags.LocalizationSource);
                genFileTags.Add(GenFileTags.LocalizationDest);
            }
            if (GenTaskActive(GenType.AddBuildDate) || GenTaskActive(GenType.RemoveBuildDate))
            {
                genFileTags.Add(GenFileTags.BuildDate);
            }
            if (GenTaskActive(GenType.GenSlimDesignerFiles))
            {
                forceFindRequiredFiles = true;
            }

            Dictionary<string, string>? taggedFilesDict = null;
            if (forceFindRequiredFiles || genFileTags.Count > 0)
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
            if (LangTaskActive())
            {
                string englishIni = Path.Combine(ALProjectPath, @"Languages\English.ini");
                string testLangIni = GenTaskActive(GenType.LanguageAndAlsoCreateTestIni)
                    ? @"C:\AngelLoader_dev_3053BA21\Data\Languages\TestLang.ini"
                    : "";
                Language.Generate(
                    taggedFilesDict![GenFileTags.LocalizationSource],
                    taggedFilesDict![GenFileTags.LocalizationDest],
                    englishIni,
                    testLangIni,
                    writeReflectionStyle: GenTaskActive(GenType.EnableLangReflectionStyleGen));
            }
            if (GenTaskActive(GenType.GameSupport))
            {
                Games.Generate();
            }
            if (GenTaskActive(GenType.ExcludeResx))
            {
                ExcludeResx.GenerateExclude();
            }
            if (GenTaskActive(GenType.RestoreResx))
            {
                ExcludeResx.GenerateRestore();
            }
            if (GenTaskActive(GenType.AddBuildDate))
            {
                BuildDateGen.Generate(taggedFilesDict![GenFileTags.BuildDate]);
            }
            if (GenTaskActive(GenType.RemoveBuildDate))
            {
                BuildDateGen.Generate(taggedFilesDict![GenFileTags.BuildDate], remove: true);
            }
            if (GenTaskActive(GenType.GenSlimDesignerFiles))
            {
                DesignerGen.Generate();
            }
        }

        [MustUseReturnValue]
        private static Dictionary<string, string>
        FindRequiredCodeFiles(List<string> genFileTags)
        {
            var taggedFiles = InitializedArray<List<string>>(genFileTags.Count);

            foreach (string f in Cache.CSFiles)
            {
                using var sr = new StreamReader(f);

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    string lts = line.TrimStart();
                    if (lts.IsWhiteSpace() || lts.StartsWith("//")) continue;

                    if (lts[0] != '#') break;

                    if (lts.StartsWithPlusWhiteSpace("#define"))
                    {
                        string tag = lts.Substring(7).Trim();
                        if (tag == GenFileTags.DesignerSource)
                        {
                            if (f.EndsWithI(".Designer.cs"))
                            {
                                Cache.DesignerCSFiles.Add(f);
                            }
                            else
                            {
                                ThrowErrorAndTerminate(GenFileTags.DesignerSource +
                                                       " found in a file not ending in .Designer.cs.");
                            }
                            continue;
                        }

                        for (int i = 0; i < genFileTags.Count; i++)
                        {
                            if (tag == genFileTags[i])
                            {
                                taggedFiles[i].Add(f);
                                break;
                            }
                        }
                    }
                }
            }

            #region Error reporting

            static string AddError(string msg, string add) => msg + (msg.IsEmpty() ? "" : "\r\n") + add;

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

            var ret = new Dictionary<string, string>(genFileTags.Count);
            for (int i = 0; i < genFileTags.Count; i++)
            {
                ret.Add(genFileTags[i], taggedFiles[i][0]);
            }
            return ret;
        }
    }
}
