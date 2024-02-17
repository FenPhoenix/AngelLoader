// FenGen - Fen's code generator for AngelLoader
// Not perfect by any means, and still with lots of hardcoded stuff that shouldn't be, but it gets the job done.

#region Snarky explanation on why we don't use the official Source Generators feature
/*
Yes, I know they have official Source Generators now. And using them, we could avoid the awkward requirement to
rebuild the entire solution to get this generator to do its work 100% correctly.
But, these Source Generators don't fit our needs.

To wit:

-They don't let us modify the project file, which we do in here in order to get rid of unused .resx files from
 the build. I mean that's understandable enough, the project file isn't "source code" really, and in an ideal
 world we wouldn't have to do the stupid hack of writing .resx excludes into the project file if WinForms would
 let us tell it to do so in a nicer way, so okay, but still we gotta do it.

-They inject code directly into the compiler rather than writing out a file and THEN compiling. Now technically,
 they do write out code to a "file" that gets generated and put into
 [main project]/Dependencies/Analyzers/[codegen project]/[generated file].
 This is extremely obscure and annoying to access, but the bigger problem is that currently (2021/12/18) this
 file does not actually get updated when the code generator changes until you restart Visual Studio. The actual
 generated code still gets updated and injected into the compiler fine, but the visible file does not update
 until you restart Visual Studio. This is a ridiculous deal-breaker and makes debugging totally untenable.

-One thing we could do is to generate the code, and write it out to an actual code file in the regular project
 structure AND THEN ALSO inject it into the compiler in the normal way. That way, we would end up with an exact
 copy of the generated code in a regular file that anyone could easily find and look at.
 However, this post (https://github.com/dotnet/roslyn/issues/49249#issuecomment-809807528) claims the following:
 "Source generators must not perform file I/O, either directly (e.g. File.Read) or indirectly (e.g. through git).
 If your generator has any I/O, the resulting behavior is undefined."
 It seems there may possibly be a way to tell it to put the generated file on disk alongside your project, but
 still trying to dig up a conclusive answer and/or explanation of how to do it.

Anyway, the whole thing is an exasperating - but unsurprising - mess, and as it almost always is, it's better,
simpler and easier to just write my own generator as an executable that runs prior to the main app build. That
way I know I control everything and can do what I want with no interference from people who think they know best.

So no complaining. This is my generator. Good day.
*/
#endregion

//#define PROFILING

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static FenGen.Misc;

namespace FenGen;

internal sealed class GameSourceEnum
{
    internal string Name = "";
    internal string GameIndexName = "";
    internal readonly List<string> GameEnumNames = new();
    internal readonly List<string> GameIndexEnumNames = new();
    internal readonly List<string> GamePrefixes = new();
    internal readonly List<string> SteamIds = new();
    internal readonly List<string> EditorNames = new();
    internal readonly List<bool> SupportsMods = new();
    internal readonly List<bool> SupportsImport = new();
    internal string EnumType = "";
}

internal sealed class LanguageSourceEnum
{
    internal string Name = "";
    internal string StringToEnumDictName = "";
    internal string LanguageIndexName = "";
    internal readonly List<string> LangEnumNames = new();
    internal readonly List<string> LangIndexEnumNames = new();
    internal readonly List<string> LangIndexEnumNamesLowercase = new();
    internal readonly List<string> LangCodes = new();
    internal readonly List<string> LangTranslatedNames = new();
    internal string EnumType = "";
}

internal sealed class DesignerCSFile(string fileName, bool splashScreen = false)
{
    internal readonly string FileName = fileName;
    internal readonly bool SplashScreen = splashScreen;

    public override string ToString() => FileName;
}

// Nasty global state that's really just here to avoid over-parameterization.
internal static class Cache
{
    #region Games

    private static string _gameSupportFile = "";
    internal static void SetGameSupportFile(string file) => _gameSupportFile = file;
    private static GameSourceEnum? _gamesEnum;
    internal static GameSourceEnum GamesEnum => _gamesEnum ??= Games.FillGamesEnum(_gameSupportFile);

    #endregion

    #region Language support

    private static string _langsSupportFile = "";
    internal static void SetLangSupportFile(string file) => _langsSupportFile = file;
    private static LanguageSourceEnum? _languageEnum;
    internal static LanguageSourceEnum LangsEnum => _languageEnum ??= LanguageSupport.FillLangsEnum(_langsSupportFile);

    #endregion

    private static List<string>? _csFiles;
    internal static List<string> CSFiles => _csFiles ??= Directory.GetFiles(Core.ALSolutionPath, "*.cs", SearchOption.AllDirectories).ToList();

    internal static readonly List<DesignerCSFile> DesignerCSFiles = new();

    internal static readonly string CurrentYear = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);

    internal static readonly List<string> TypeSourceFiles = new();

    internal static readonly List<string> RtfDupeDestFiles = new();

    internal static void Clear()
    {
        _gameSupportFile = "";
        _gamesEnum = null;

        _langsSupportFile = "";
        _languageEnum = null;

        _csFiles = null;
        DesignerCSFiles.Clear();

        TypeSourceFiles.Clear();

        RtfDupeDestFiles.Clear();
    }
}

internal static class GenAttributes
{
    #region Serialization

    internal const string FenGenFMDataSourceClass = nameof(FenGenFMDataSourceClass);
    internal const string FenGenFMDataDestClass = nameof(FenGenFMDataDestClass);
    internal const string FenGenIgnore = nameof(FenGenIgnore);
    internal const string FenGenDoNotWrite = nameof(FenGenDoNotWrite);
    internal const string FenGenIniName = nameof(FenGenIniName);
    internal const string FenGenNumericEmpty = nameof(FenGenNumericEmpty);
    internal const string FenGenListType = nameof(FenGenListType);
    internal const string FenGenDoNotTrimValue = nameof(FenGenDoNotTrimValue);
    internal const string FenGenDoNotConvertDateTimeToLocal = nameof(FenGenDoNotConvertDateTimeToLocal);

    #endregion

    internal const string FenGenFlagsSingleAssignment = nameof(FenGenFlagsSingleAssignment);
    internal const string FenGenReadmeEncoding = nameof(FenGenReadmeEncoding);

    #region Localizable text

    internal const string FenGenLocalizationSourceClass = nameof(FenGenLocalizationSourceClass);
    internal const string FenGenLocalizedGameNameGetterDestClass = nameof(FenGenLocalizedGameNameGetterDestClass);
    internal const string FenGenComment = nameof(FenGenComment);
    internal const string FenGenBlankLine = nameof(FenGenBlankLine);
    internal const string FenGenGameSet = nameof(FenGenGameSet);

    #endregion

    #region Game support

    internal const string FenGenGameEnum = nameof(FenGenGameEnum);
    internal const string FenGenGameSupportMainGenDestClass = nameof(FenGenGameSupportMainGenDestClass);
    internal const string FenGenGame = nameof(FenGenGame);

    #endregion

    #region Language support

    internal const string FenGenLanguageSupportDestClass = nameof(FenGenLanguageSupportDestClass);
    internal const string FenGenLanguageEnum = nameof(FenGenLanguageEnum);
    internal const string FenGenLanguage = nameof(FenGenLanguage);

    #endregion

    internal const string FenGenExcludeResx = nameof(FenGenExcludeResx);

    internal const string FenGenBuildDateDestClass = nameof(FenGenBuildDateDestClass);

    internal const string FenGenDoNotRemoveTextAttribute = nameof(FenGenDoNotRemoveTextAttribute);
    internal const string FenGenDoNotRemoveHeaderTextAttribute = nameof(FenGenDoNotRemoveHeaderTextAttribute);
    internal const string FenGenDoNotRemoveToolTipTextAttribute = nameof(FenGenDoNotRemoveToolTipTextAttribute);

    internal const string FenGenForceRemoveSizeAttribute = nameof(FenGenForceRemoveSizeAttribute);

    internal const string FenGenCurrentYearDestClassAttribute = nameof(FenGenCurrentYearDestClassAttribute);

    internal const string FenGenEnumCount = nameof(FenGenEnumCount);
    internal const string FenGenEnumNames = nameof(FenGenEnumNames);

    internal const string FenGenEnumDataDestClass = nameof(FenGenEnumDataDestClass);

    internal const string FenGenRtfDuplicateSourceClass = nameof(FenGenRtfDuplicateSourceClass);
    internal const string FenGenRtfDuplicateDestClass = nameof(FenGenRtfDuplicateDestClass);

    internal const string FenGenTreatAsList = nameof(FenGenTreatAsList);
    internal const string FenGenPlaceAfterKey = nameof(FenGenPlaceAfterKey);
}

internal static class Core
{
    private enum GenType
    {
        FMData,
        Config,
        Language,
        LanguageAndAlsoCreateTestIni,
        GameSupport,
        ExcludeResx,
        RestoreResx,
        AddBuildDate,
        RemoveBuildDate,
        GenSlimDesignerFiles,
        GenCopyright,
        GenEnumData,
        RtfCodeDupe
    }

    private static readonly Dictionary<string, GenType>
    _argToTaskMap = new(StringComparer.Ordinal)
    {
        { "-fmd", GenType.FMData },
        { "-lang", GenType.Language },
        { "-lang_t", GenType.LanguageAndAlsoCreateTestIni },
        { "-game", GenType.GameSupport },
        { "-resx_e", GenType.ExcludeResx },
        { "-resx_r", GenType.RestoreResx },
        { "-bd", GenType.AddBuildDate },
        { "-bd_r", GenType.RemoveBuildDate },
        { "-des", GenType.GenSlimDesignerFiles },
        { "-cr", GenType.GenCopyright },
        { "-ed", GenType.GenEnumData },
        { "-rtf_d", GenType.RtfCodeDupe },
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

    private static class DefineHeaders
    {
        internal const string LocalizationSource = "FenGen_LocalizationSource";
        internal const string UpdaterLocalizationSource = "FenGen_UpdaterLocalizationSource";
        internal const string GameSupportSource = "FenGen_GameSupportSource";
        internal const string LanguageSupportSource = "FenGen_LanguageSupportSource";
        internal const string LanguageSupportDest = "FenGen_LanguageSupportDest";
        internal const string FMDataSource = "FenGen_FMDataSource";
        internal const string FMDataDest = "FenGen_FMDataDest";
        internal const string DesignerSource = "FenGen_DesignerSource";
        internal const string DesignerSource_SplashScreen = "FenGen_DesignerSource_SplashScreen";
        internal const string BuildDate = "FenGen_BuildDateDest";
        internal const string GameSupportMainGenDest = "FenGen_GameSupportMainGenDest";
        internal const string LocalizedGameNameGetterDest = "FenGen_LocalizedGameNameGetterDest";
        internal const string CurrentYearDest = "FenGen_CurrentYearDest";

        internal const string TypeSource = "FenGen_TypeSource";
        internal const string EnumDataDest = "FenGen_EnumDataDest";

        internal const string RtfDuplicateSource = "FenGen_RtfDuplicateSource";
        internal const string RtfDuplicateDest = "FenGen_RtfDuplicateDest";
    }

    private static readonly int _genTaskCount = Enum.GetValues(typeof(GenType)).Length;

#if DEBUG
    private static Forms.MainForm View = null!;
#endif

    internal static readonly string ALSolutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\..\"));
    internal static readonly string ALProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\..\AngelLoader"));
    internal static readonly string ALCommonProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\..\AL_Common"));
    internal static readonly string FenGenProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\..\FenGen"));
    internal static readonly string ALProjectFile = Path.Combine(ALProjectPath, "AngelLoader.csproj");

    // Roslyn is so slow it's laughable. It takes 1.5 seconds just to run InitWorkspaceStuff() alone.
    // That's not even counting doing any actual work with it, which adds even more slug time.
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

    private static void Exit() => Environment.Exit(1);

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
            GetArg(GenType.ExcludeResx),
            //GetArg(GenType.RestoreResx),
            GetArg(GenType.AddBuildDate),
            GetArg(GenType.GenSlimDesignerFiles),
            GetArg(GenType.GameSupport),
            GetArg(GenType.GenCopyright),
            GetArg(GenType.GenEnumData),
            GetArg(GenType.RtfCodeDupe)
        };
#else
        string[] args = Environment.GetCommandLineArgs();
#endif

        if (args.Length < 2) Exit();

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
            Exit();
            return;
        }

        bool forceFindRequiredFiles = false;
        var defineHeaders = new List<string>();
        bool gameSupportRequested = GenTaskActive(GenType.FMData) ||
                                    GenTaskActive(GenType.GameSupport) ||
                                    LangTaskActive();

        // Just always do it...
        bool langSupportRequested = true;

        if (langSupportRequested)
        {
            defineHeaders.Add(DefineHeaders.LanguageSupportSource);
        }

        if (gameSupportRequested)
        {
            defineHeaders.Add(DefineHeaders.GameSupportSource);
        }
        // Only do this if we're writing out game support stuff proper, not just reading them
        if (GenTaskActive(GenType.GameSupport))
        {
            defineHeaders.Add(DefineHeaders.GameSupportMainGenDest);
        }
        if (GenTaskActive(GenType.FMData))
        {
            defineHeaders.Add(DefineHeaders.FMDataSource);
            defineHeaders.Add(DefineHeaders.FMDataDest);
        }
        if (LangTaskActive())
        {
            defineHeaders.Add(DefineHeaders.LocalizationSource);
            defineHeaders.Add(DefineHeaders.UpdaterLocalizationSource);
            defineHeaders.Add(DefineHeaders.LocalizedGameNameGetterDest);
            defineHeaders.Add(DefineHeaders.LanguageSupportSource);
            defineHeaders.Add(DefineHeaders.LanguageSupportDest);
        }
        if (GenTaskActive(GenType.AddBuildDate) || GenTaskActive(GenType.RemoveBuildDate))
        {
            defineHeaders.Add(DefineHeaders.BuildDate);
        }
        if (GenTaskActive(GenType.GenSlimDesignerFiles))
        {
            forceFindRequiredFiles = true;
        }
        if (GenTaskActive(GenType.GenCopyright))
        {
            defineHeaders.Add(DefineHeaders.CurrentYearDest);
        }
        if (GenTaskActive(GenType.GenEnumData))
        {
            defineHeaders.Add(DefineHeaders.EnumDataDest);
        }

        if (GenTaskActive(GenType.RtfCodeDupe))
        {
            defineHeaders.Add(DefineHeaders.RtfDuplicateSource);
        }

        var taggedFilesDict = new Dictionary<string, string>(StringComparer.Ordinal);
        if (forceFindRequiredFiles || defineHeaders.Count > 0)
        {
            taggedFilesDict = FindRequiredCodeFiles(defineHeaders.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
        }

        if (gameSupportRequested)
        {
            Cache.SetGameSupportFile(taggedFilesDict[DefineHeaders.GameSupportSource]);
        }
        if (langSupportRequested)
        {
            Cache.SetLangSupportFile(taggedFilesDict[DefineHeaders.LanguageSupportSource]);
        }

        if (GenTaskActive(GenType.FMData))
        {
            FMData.Generate(
                taggedFilesDict[DefineHeaders.FMDataSource],
                taggedFilesDict[DefineHeaders.FMDataDest]);
        }
        if (LangTaskActive())
        {
            static string GetTestLangPath(string fileName)
            {
                try
                {
                    // Only generate the test lang file into what may be a production folder on my own
                    // machine, not everyone else's
                    string? val = Environment.GetEnvironmentVariable(
                        "AL_FEN_PERSONAL_DEV_3053BA21",
                        EnvironmentVariableTarget.Machine);
                    return val?.EqualsTrue() == true
                        ? Path.Combine(@"C:\AngelLoader\Data\Languages\", fileName)
                        : "";
                }
                catch
                {
                    return "";
                }
            }

            string englishIni = Path.Combine(ALProjectPath, @"Languages\English.ini");
            (string testLangIni, string testLangLongIni) = GenTaskActive(GenType.LanguageAndAlsoCreateTestIni)
                ? (GetTestLangPath("TestLang.ini"), GetTestLangPath("TestLangLong.ini"))
                : ("", "");

            Language.Generate(
                mainSourceFile: taggedFilesDict[DefineHeaders.LocalizationSource],
                updaterSourceFile: taggedFilesDict[DefineHeaders.UpdaterLocalizationSource],
                perGameLangGetterDestFile: taggedFilesDict[DefineHeaders.LocalizedGameNameGetterDest],
                langIniFile: englishIni,
                testLangIniFile: testLangIni,
                testLang2IniFile: testLangLongIni);

            LanguageSupport.Generate(destFile: taggedFilesDict[DefineHeaders.LanguageSupportDest]);
        }
        if (GenTaskActive(GenType.GameSupport))
        {
            Games.Generate(taggedFilesDict[DefineHeaders.GameSupportMainGenDest]);
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
            BuildDateGen.Generate(taggedFilesDict[DefineHeaders.BuildDate]);
        }
        if (GenTaskActive(GenType.RemoveBuildDate))
        {
            BuildDateGen.Generate(taggedFilesDict[DefineHeaders.BuildDate], remove: true);
        }
        if (GenTaskActive(GenType.GenSlimDesignerFiles))
        {
            DesignerGen.Generate();
        }
        if (GenTaskActive(GenType.GenCopyright))
        {
            CopyrightGen.Generate(taggedFilesDict[DefineHeaders.CurrentYearDest]);
        }
        if (GenTaskActive(GenType.GenEnumData))
        {
            EnumDataGen.Generate(taggedFilesDict[DefineHeaders.EnumDataDest]);
        }
        if (GenTaskActive(GenType.RtfCodeDupe))
        {
            RtfDupeCodeGen.Generate(taggedFilesDict[DefineHeaders.RtfDuplicateSource]);
        }
    }

    [MustUseReturnValue]
    private static Dictionary<string, string>
    FindRequiredCodeFiles(List<string> defineHeaders)
    {
        var taggedFiles = InitializedArray<List<string>>(defineHeaders.Count);

        foreach (string f in Cache.CSFiles)
        {
            using var sr = new StreamReader(f);

            while (sr.ReadLine() is { } line)
            {
                string lts = line.TrimStart();
                if (lts.IsWhiteSpace() || lts.StartsWithO("//")) continue;

                if (lts[0] != '#') break;

                if (lts.StartsWithPlusWhiteSpace("#define"))
                {
                    string tag = lts.Substring(7).Trim();
                    if (tag
                        is DefineHeaders.DesignerSource
                        or DefineHeaders.DesignerSource_SplashScreen)
                    {
                        if (f.EndsWithI(".Designer.cs"))
                        {
                            Cache.DesignerCSFiles.Add(
                                new DesignerCSFile(
                                    fileName: f,
                                    splashScreen: tag == DefineHeaders.DesignerSource_SplashScreen
                                ));
                        }
                        else
                        {
                            ThrowErrorAndTerminate(DefineHeaders.DesignerSource +
                                                   " found in a file not ending in .Designer.cs.");
                        }
                        continue;
                    }
                    else if (tag == DefineHeaders.TypeSource)
                    {
                        Cache.TypeSourceFiles.Add(f);
                        continue;
                    }
                    else if (tag == DefineHeaders.RtfDuplicateDest)
                    {
                        Cache.RtfDupeDestFiles.Add(f);
                        continue;
                    }

                    for (int i = 0; i < defineHeaders.Count; i++)
                    {
                        if (tag == defineHeaders[i])
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
                error = AddError(error, "-No file found with '#define " + defineHeaders[i] + "' at top");
            }
            else if (taggedFiles[i].Count > 1)
            {
                error = AddError(error, "-Multiple files found with '#define " + defineHeaders[i] + "' at top");
            }
        }
        if (!error.IsEmpty()) ThrowErrorAndTerminate(error);

        #endregion

        var ret = new Dictionary<string, string>(defineHeaders.Count, StringComparer.Ordinal);
        for (int i = 0; i < defineHeaders.Count; i++)
        {
            ret.Add(defineHeaders[i], taggedFiles[i][0]);
        }
        return ret;
    }
}
