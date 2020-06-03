// NOTE: This code is provided for completeness' sake. It was written to do the job quickly without regard to any
// kind of good coding practices whatsoever. Don't write a public-facing program this way. Thank you.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FenGen
{
    internal enum GenType
    {
        FMData,
        Config,
        Language,
        Version,
        MainFormBacking,
        GameSupport,
        VisLoc
    }

    internal enum VersionType
    {
        Beta,
        PublicRelease
    }

    internal sealed class GameSourceEnum
    {
        internal string Name = "";
        internal readonly List<string> Items = new List<string>();
    }

    internal static class StateVars
    {
        internal static string TestFile;
        internal static bool WriteTestLangFile;
        internal static GameSourceEnum GamesEnum;
    }

    internal static class Core
    {
        private static readonly List<GenType> GenTasks = new List<GenType>();

#if DEBUG
        private static MainForm View;
#endif

        internal static string ALSolutionPath;
        internal static string ALProjectPath;

        internal static void Init()
        {
            ALSolutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
            ALProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\AngelLoader"));

#if Release
            ReadArgsAndDoTasks();

            Environment.Exit(0);
#else
            View = new MainForm();
            View.Show();
#endif
        }

        private static void ExitIfRelease() => Environment.Exit(1);

        private static void ReadArgsAndDoTasks()
        {
#if DEBUG
            //return;
#endif

            // args[0] is always the application filename
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2) ExitIfRelease();

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-version=beta":
                        if (!GenTasks.Contains(GenType.Version))
                        {
                            GenTasks.Add(GenType.Version);
                            string sourceFile = Path.Combine(ALProjectPath, @"Properties\AssemblyInfo.cs");
                            VersionIncrement.Generate(sourceFile, VersionType.Beta);
                        }
                        break;
                    case "-version=public":
                        if (!GenTasks.Contains(GenType.Version))
                        {
                            GenTasks.Add(GenType.Version);
                            string sourceFile = Path.Combine(ALProjectPath, @"Properties\AssemblyInfo.cs");
                            VersionIncrement.Generate(sourceFile, VersionType.PublicRelease);
                        }
                        break;
                    case "-fmdata":
                        if (!GenTasks.Contains(GenType.FMData))
                        {
                            GenTasks.Add(GenType.FMData);
                            Games.FillGamesEnum();
                            string sourceFile = Path.Combine(ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
                            string destFile = Path.Combine(ALProjectPath, @"Ini\FMData.cs");
                            FMData.Generate(sourceFile, destFile);
                        }
                        break;
                    //case "-config": // Not implemented
                    //    if (!GenTasks.Contains(GenType.Config)) GenTasks.Add(GenType.Config);
                    //    break;
                    case "-language":
                        if (!GenTasks.Contains(GenType.Language))
                        {
                            GenTasks.Add(GenType.Language);
                            string langFile = Path.Combine(ALProjectPath, @"Languages\English.ini");
                            LanguageGen.Generate(langFile);
                        }
                        break;
                    case "-language_t":
                        if (!GenTasks.Contains(GenType.Language))
                        {
                            GenTasks.Add(GenType.Language);
                            string langFile = Path.Combine(ALProjectPath, @"Languages\English.ini");
                            StateVars.WriteTestLangFile = true;
                            StateVars.TestFile = @"C:\AngelLoader\Data\Languages\TestLang.ini";
                            LanguageGen.Generate(langFile);
                        }
                        break;
                    case "-game_support":
                        if (!GenTasks.Contains(GenType.GameSupport))
                        {
                            GenTasks.Add(GenType.GameSupport);
                            Games.Generate();
                        }
                        break;
                    case "-visloc":
                        // switched off for now
                        break;
                        if (!GenTasks.Contains(GenType.VisLoc))
                        {
                            GenTasks.Add(GenType.VisLoc);
                            VisLoc.Generate();
                        }
                        break;
                    default:
                        ExitIfRelease();
                        break;
                }
            }
        }
    }
}
