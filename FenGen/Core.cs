// NOTE: This code is provided for completeness' sake. It was written to do the job quickly without regard to any
// kind of good coding practices whatsoever. Don't write a public-facing program this way. Thank you.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static FenGen.CommonStatic;
using static FenGen.Methods;

namespace FenGen
{
    internal sealed class Field
    {
        internal string Type;
        internal string Name;
        internal string IniName;
        internal string ListItemPrefix;
        internal ListType ListType = ListType.MultipleLines;
        internal ListDistinctType ListDistinctType = ListDistinctType.None;
        internal Dictionary<string, string> EnumMap = new Dictionary<string, string>();
        internal List<string> EnumValues = new List<string>();
        internal long? NumericEmpty;
        internal bool DoNotTrimValue;
        internal bool DoNotConvertDateTimeToLocal;
        internal string Comment;
        internal CustomCodeBlockNames CodeBlockToInsertAfter = CustomCodeBlockNames.None;
    }

    internal sealed class FieldList : List<Field>
    {
        internal bool WriteEmptyValues;
        //internal string Version;
    }

    internal enum GenType
    {
        FMData,
        Config,
        Language,
        Version,
        MainFormBacking,
        VisLoc
    }

    internal enum VersionType
    {
        Beta,
        PublicRelease
    }

    internal enum ListType
    {
        MultipleLines,
        CommaSeparated
    }

    internal enum ListDistinctType
    {
        None,
        Exact,
        CaseInsensitive
    }

    internal enum CustomCodeBlockNames
    {
        None,
        LegacyCustomResources
    }

    internal static class StateVars
    {
        internal static string TestFile;
        internal static bool WriteTestLangFile;
    }

    internal static class Core
    {
        private static readonly string[] NumericTypes =
        {
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "decimal"
        };

        private static class CustomCodeBlocks
        {
            internal static readonly string[] LegacyCustomResourceReads =
            {
                "                #region Old resource format - backward compatibility, we still have to be able to read it",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasMap=\"))",
                "                {",
                "                    string val = lineT.Substring(7);",
                "                    SetFMResource(fm, CustomResources.Map, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasAutomap=\"))",
                "                {",
                "                    string val = lineT.Substring(11);",
                "                    SetFMResource(fm, CustomResources.Automap, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasScripts=\"))",
                "                {",
                "                    string val = lineT.Substring(11);",
                "                    SetFMResource(fm, CustomResources.Scripts, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasTextures=\"))",
                "                {",
                "                    string val = lineT.Substring(12);",
                "                    SetFMResource(fm, CustomResources.Textures, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasSounds=\"))",
                "                {",
                "                    string val = lineT.Substring(10);",
                "                    SetFMResource(fm, CustomResources.Sounds, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasObjects=\"))",
                "                {",
                "                    string val = lineT.Substring(11);",
                "                    SetFMResource(fm, CustomResources.Objects, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasCreatures=\"))",
                "                {",
                "                    string val = lineT.Substring(13);",
                "                    SetFMResource(fm, CustomResources.Creatures, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasMotions=\"))",
                "                {",
                "                    string val = lineT.Substring(11);",
                "                    SetFMResource(fm, CustomResources.Motions, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasMovies=\"))",
                "                {",
                "                    string val = lineT.Substring(10);",
                "                    SetFMResource(fm, CustomResources.Movies, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                else if (lineT.StartsWithFast_NoNullChecks(\"HasSubtitles=\"))",
                "                {",
                "                    string val = lineT.Substring(13);",
                "                    SetFMResource(fm, CustomResources.Subtitles, val.EqualsTrue());",
                "                    resourcesFound = true;",
                "                }",
                "                #endregion"
            };
        }

        private static readonly List<GenType> GenTasks = new List<GenType>();

        private static FieldList Fields { get; } = new FieldList();

        private static List<string> DestTopLines { get; } = new List<string>();

        private static string[] TopCodeLines { get; } =
        {
            //"        private static bool StartsWithFast_NoNullChecks(this string str, string value)",
            //"        {",
            //"            if (str.Length < value.Length) return false;",
            //"",
            //"            for (int i = 0; i < value.Length; i++)",
            //"            {",
            //"                if (str[i] != value[i]) return false;",
            //"            }",
            //"",
            //"            return true;",
            //"        }",
            //""
        };

        private static string[] ReadFMDataIniTopLines { get; } =
        {
            "        internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList)",
            "        {",
            "            string[] iniLines = File.ReadAllLines(fileName, Encoding.UTF8);",
            "",
            "            if (fmsList.Count > 0) fmsList.Clear();",
            "",
            "            bool fmsListIsEmpty = true;",
            "",
            "            foreach (string line in iniLines)",
            "            {",
            "                string lineTS = line.TrimStart();",
            "                string lineT = lineTS.TrimEnd();",
            "",
            "                if (lineT.Length > 0 && lineT[0] == '[')",
            "                {",
            "                    if (lineT.Length >= 4 && lineT[1] == 'F' && lineT[2] == 'M' && lineT[3] == ']')",
            "                    {",
            "                        fmsList.Add(new FanMission());",
            "                        if (fmsListIsEmpty) fmsListIsEmpty = false;",
            "                    }",
            "",
            "                    continue;",
            "                }",
            "",
            "                if (fmsListIsEmpty) continue;",
            "",
            "                bool resourcesFound = false;",
            "",
            "                // Comment chars (;) and blank lines will be rejected implicitly.",
            "                // Since they're rare cases, checking for them would only slow us down.",
            "",
            "                FanMission fm = fmsList[fmsList.Count - 1];",
            ""
        };

#if enable_config_gen
        private static string[] ReadConfigIniTopLines { get; } =
        {
            "        internal static ConfigData ReadConfigIni(string fileName)",
            "        {",
            "            var config = new ConfigData();",
            "",
            "            string[] iniLines = File.ReadAllLines(fileName, Encoding.UTF8);",
            "",
            "            foreach (string line in iniLines)",
            "            {",
            "                string lineT = line.TrimStart();",
            ""
        };
#endif

        private static string[] WriteFMDataIniTopLines { get; } =
        {
            "        private static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)",
            "        {",
            "            // Averaged over the 1573 FMs in my FMData.ini file (in new HasResources format)",
            "            const int averageFMEntryCharCount = 378;",
            "            var sb = new StringBuilder(averageFMEntryCharCount * fmDataList.Count);",
            "",
            "            foreach (FanMission fm in fmDataList)",
            "            {",
            "                sb.AppendLine(\"[FM]\");",
            ""
        };

        private static List<string> ReaderKeepLines { get; } = new List<string>();
        private static List<string> WriterKeepLines { get; } = new List<string>();

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
                        // Switched this off due to the extensive manual edits for new HasResources format
                        //break;
                        if (!GenTasks.Contains(GenType.FMData))
                        {
                            GenTasks.Add(GenType.FMData);
                            string sourceFile = Path.Combine(ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
                            string destFile = Path.Combine(ALProjectPath, @"Ini\FMData.cs");
                            GenerateFMData(sourceFile, destFile);
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

        internal static void GenerateFMData(string sourceFile, string destFile)
        {
#if DEBUG
            //GenType = GenType.FMData;
            //GenType = GenType.Language;
#endif

            //string className = GenType == GenType.FMData ? "FanMission" : "ConfigData";
            const string className = "FanMission";
            ReadSourceFields(className, sourceFile);

            using (var sr = new StreamReader(destFile))
            {
                int openBraces = 0;
                bool inClass = false;
                bool inReaderKeepBlock = false;
                bool inWriterKeepBlock = false;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line == null) continue;

                    string lineT = line.Trim();

                    if (!inClass)
                    {
                        if (lineT.StartsWith("{")) openBraces++;
                        DestTopLines.Add(line);
                        if (openBraces == 2) inClass = true;
                        continue;
                    }

                    if (inReaderKeepBlock || inWriterKeepBlock)
                    {
                        if (lineT.StartsWith("//"))
                        {
                            string attr = lineT.Substring(2).Trim();
                            switch (attr)
                            {
                                case "[FenGen:EndReaderKeepBlock]":
                                    ReaderKeepLines.Add(line);
                                    inReaderKeepBlock = false;
                                    break;
                                case "[FenGen:EndWriterKeepBlock]":
                                    WriterKeepLines.Add(line);
                                    inWriterKeepBlock = false;
                                    break;
                                default:
                                    if (inReaderKeepBlock)
                                    {
                                        ReaderKeepLines.Add(line);
                                    }
                                    else if (inWriterKeepBlock)
                                    {
                                        WriterKeepLines.Add(line);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (inReaderKeepBlock)
                            {
                                ReaderKeepLines.Add(line);
                            }
                            else
                            {
                                WriterKeepLines.Add(line);
                            }
                        }

                        continue;
                    }

                    if (lineT.StartsWith("//"))
                    {
                        string attr = lineT.Substring(2).Trim();
                        switch (attr)
                        {
                            case "[FenGen:BeginReaderKeepBlock]":
                                ReaderKeepLines.Add(line);
                                inReaderKeepBlock = true;
                                continue;
                            case "[FenGen:BeginWriterKeepBlock]":
                                WriterKeepLines.Add(line);
                                inWriterKeepBlock = true;
                                continue;
                        }
                    }
                }
            }

            using var sw = new StreamWriter(destFile, append: false);

            foreach (string l in DestTopLines) sw.WriteLine(l);

            //string obj = GenType == GenType.FMData ? "fm" : "config";
            const string obj = "fm";

            WriteReader(sw, obj);

            sw.WriteLine();

            WriteWriter(sw, obj);

            // class
            sw.WriteLine(Indent(1) + "}");

            // namespace
            sw.WriteLine("}");
        }

        private static void WriteReader(StreamWriter sw, string obj)
        {
            if (TopCodeLines.Length > 0)
            {
                sw.WriteLine(Indent(2) + TopCodeMessage);
                foreach (string l in TopCodeLines) sw.WriteLine(l);
            }

            sw.WriteLine(Indent(2) + AutogeneratedMessage);

            //var topLines = (GenType == GenType.FMData
            //    ? ReadFMDataIniTopLines
            //    : ReadConfigIniTopLines).ToList();

            string[] topLines = ReadFMDataIniTopLines;

            //if (topLines.SequenceEqual(ReadFMDataIniTopLines.ToList()))
            //{
            //    for (int tli = 0; tli < topLines.Count; tli++)
            //    {
            //        var line = topLines[tli];
            //        if (line.Trim().StartsWith("// [FenGen:DefineBools]"))
            //        {
            //            var spaces = new string(' ', line.IndexOf('/'));
            //            topLines.RemoveAt(tli);
            //            //tli--;

            //            foreach (var field in Fields)
            //            {
            //                topLines.Insert(tli, spaces + "bool " + field.Name + "Read = false;");
            //                tli++;
            //            }
            //        }

            //        if (line.Trim().StartsWith("// [FenGen:ResetBools]"))
            //        {
            //            var spaces = new string(' ', line.IndexOf('/'));
            //            topLines.RemoveAt(tli);

            //            foreach (var field in Fields)
            //            {
            //                topLines.Insert(tli, spaces + field.Name + "Read = false;");
            //                tli++;
            //            }
            //        }
            //    }
            //}

            foreach (string l in topLines) sw.WriteLine(l);

            bool wroteHasXFields = false;

            CustomCodeBlockNames customCodeBlockToInsertAfterField = CustomCodeBlockNames.None;

            for (int i = 0; i < Fields.Count; i++)
            {
                if (customCodeBlockToInsertAfterField != CustomCodeBlockNames.None)
                {
                    switch (customCodeBlockToInsertAfterField)
                    {
                        case CustomCodeBlockNames.LegacyCustomResources:
                            foreach (string line in CustomCodeBlocks.LegacyCustomResourceReads)
                            {
                                sw.WriteLine(line);
                            }
                            break;
                    }
                }

                var field = Fields[i];
                string objDotField = obj + "." + field.Name;

                string optElse = i > 0 ? "else " : "";

                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                string lineTrimmedVersion = field.DoNotTrimValue ? "lineTS" : "lineT";

                sw.WriteLine(
                    Indent(4) +
                    optElse +
                    "if (lineT.StartsWithFast_NoNullChecks(\"" + fieldIniName + "=\"))\r\n" +
                    Indent(4) + "{\r\n" +
                    Indent(5) + "string val = " + lineTrimmedVersion + ".Substring(" + (fieldIniName.Length + 1) + ");");

                //sw.WriteLine(
                //    Indent(4) +
                //    optElse +
                //    "if (!" + field.Name + "Read && lineT.StartsWithFast_NoNullChecks(\"" + fieldIniName + "=\"))\r\n" +
                //    Indent(4) + "{\r\n" +
                //    Indent(5) + field.Name + "Read = true;\r\n" +
                //    Indent(5) + "string val = lineT.Substring(" + (fieldIniName.Length + 1) + ");");

                if (field.Type.StartsWith("List<"))
                {
                    string listType = field.Type.Substring(field.Type.IndexOf('<')).TrimStart('<').TrimEnd('>');

                    var ldt = field.ListDistinctType;

                    string varToAdd = listType == "string" && field.ListType == ListType.MultipleLines
                        ? "val"
                        : "result";

                    string objListSet = ldt switch
                    {
                        ListDistinctType.None => (objDotField + ".Add(" + varToAdd + ");"),
                        ListDistinctType.Exact => ("if (!" + objDotField + ".Contains(" + varToAdd + ")) " + objDotField + ".Add(" + varToAdd + ");"),
                        ListDistinctType.CaseInsensitive => (listType == "string"
                            ? "if (!" + objDotField + ".Contains(" + varToAdd + ", StringComparer.OrdinalIgnoreCase)) " + objDotField + ".Add(" + varToAdd + ");"
                            : "if (!" + objDotField + ".Contains(" + varToAdd + ")) " + objDotField + ".Add(" + varToAdd + ");"),
                        _ => (objDotField + ".Add(" + varToAdd + ");")
                    };

                    if (listType == "string")
                    {
                        if (field.ListType == ListType.MultipleLines)
                        {
                            // Null-check version - just use readonly List<string> blah = new List<string>();
                            // to avoid having to check null
                            //sw.WriteLine(
                            //    Indent(5) + "if (" + objDotField + " == null)\r\n" +
                            //    Indent(5) + "{\r\n" +
                            //    Indent(6) + objDotField + " = new List<string>();\r\n" +
                            //    Indent(5) + "}\r\n" +
                            //    Indent(5) + "if (!string.IsNullOrEmpty(val))\r\n" +
                            //    Indent(5) + "{\r\n" +
                            //    Indent(6) + objListSet + "\r\n" +
                            //    Indent(5) + "}");

                            sw.WriteLine(
                                Indent(5) + "if (!string.IsNullOrEmpty(val))\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objListSet + "\r\n" +
                                Indent(5) + "}");
                        }
                        else
                        {
                            sw.WriteLine(
                                Indent(5) + objDotField + ".Clear();\r\n" +
                                Indent(5) + "string[] items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);\r\n" +
                                Indent(5) + "for (int a = 0; a < items.Length; a++)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "string result = items[a].Trim();\r\n" +
                                Indent(6) + objListSet + "\r\n" +
                                Indent(5) + "}");
                        }
                    }
                    else if (NumericTypes.Contains(listType))
                    {
                        if (field.ListType == ListType.MultipleLines)
                        {
                            sw.WriteLine(
                                Indent(5) + "if (" + objDotField + " == null)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objDotField + " = new List<" + listType + ">();\r\n" +
                                Indent(5) + "}\r\n" +
                                Indent(5) + "bool success = " + listType + ".TryParse(val, out " + listType + " result);\r\n" +
                                Indent(5) + "if(success)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objListSet + "\r\n" +
                                Indent(5) + "}");
                        }
                        else
                        {
                            sw.WriteLine(
                                Indent(5) + objDotField + ".Clear();\r\n" +
                                Indent(5) + "string[] items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);\r\n" +
                                Indent(5) + "for (int a = 0; a < items.Length; a++)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "items[a] = items[a].Trim();\r\n" +
                                Indent(6) + "bool success = " + listType + ".TryParse(items[a], out " + listType + " result);\r\n" +
                                Indent(6) + "if(success)\r\n" +
                                Indent(6) + "{\r\n" +
                                Indent(7) + objListSet + "\r\n" +
                                Indent(6) + "}\r\n" +
                                Indent(5) + "}\r\n");
                        }
                    }
                }
                else if (field.Type == "string")
                {
                    sw.WriteLine(Indent(5) + objDotField + " = val;");
                }
                else if (field.Type == "bool")
                {
                    sw.WriteLine(Indent(5) + objDotField + " = val.EqualsTrue();");
                }
                else if (field.Type == "bool?")
                {
                    if (field.Name.StartsWith("Has"))
                    {
                        if (!wroteHasXFields)
                        {
                            sw.WriteLine(Indent(5) + "if (!string.IsNullOrEmpty(val))\r\n" +
                                         Indent(5) + "{\r\n" +
                                         Indent(6) + "FillFMHasXFields(fm, val);\r\n" +
                                         Indent(5) + "}");
                            wroteHasXFields = true;
                        }
                    }

                    // Also still do this, cause we need to still read these for backward compatibility
                    sw.WriteLine(Indent(5) + objDotField + " =\r\n" +
                                 Indent(6) + "!string.IsNullOrEmpty(val) ? val.EqualsTrue() : (bool?)null;");
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    if (field.NumericEmpty != null && field.NumericEmpty != 0)
                    {
                        sw.WriteLine(Indent(5) + "bool success = " + field.Type + ".TryParse(val, out " + field.Type + " result);");
                        sw.WriteLine(Indent(5) + objDotField + " = success ? result : " + field.NumericEmpty + ";");
                    }
                    else
                    {
                        sw.WriteLine(Indent(5) + field.Type + ".TryParse(val, out " + field.Type + " result);\r\n" +
                                     Indent(5) + objDotField + " = result;");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                        NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    sw.WriteLine(Indent(5) + "bool success = " + ftNonNull + ".TryParse(val, out " + ftNonNull + " result);\r\n" +
                                 Indent(5) + "if (success)\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = result;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = null;\r\n" +
                                 Indent(5) + "}");
                }
                else if (field.Type == "Game")
                {
                    // TODO:
                    // This is not automated enough. If I add another thing to the enum, it WON'T be picked up
                    // here unless I specifically add it. :(
                    sw.WriteLine(Indent(5) + "val = val.Trim();\r\n" +
                                 Indent(5) + "if (val.EqualsI(nameof(Game.Thief1)))\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.Thief1;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else if (val.EqualsI(nameof(Game.Thief2)))\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.Thief2;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else if (val.EqualsI(nameof(Game.Thief3)))\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.Thief3;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else if (val.EqualsI(nameof(Game.SS2)))\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.SS2;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else if (val.EqualsI(nameof(Game.Unsupported)))\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.Unsupported;\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = Game.Null;\r\n" +
                                 Indent(5) + "}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    sw.WriteLine(Indent(5) + objDotField + ".UnixDateString = val;");
                }
                else if (field.Type == "DateTime?")
                {

                    sw.WriteLine(Indent(5) + "// PERF: Don't convert to local here; do it at display-time\r\n" +
                                 Indent(5) + objDotField + " = ConvertHexUnixDateToDateTime(val, convertToLocal: " +
                                 (!field.DoNotConvertDateTimeToLocal).ToString().ToLowerInvariant() + ");");

                    /*
                    sw.WriteLine(Indent(5) + "bool success = long.TryParse(\r\n" +
                                 Indent(6) + "val,\r\n" +
                                 Indent(6) + "NumberStyles.HexNumber,\r\n" +
                                 Indent(6) + "DateTimeFormatInfo.InvariantInfo,\r\n" +
                                 Indent(6) + "out long result);\r\n" +
                                 "\r\n" +
                                 Indent(5) + "if (success)\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + "try\r\n" +
                                 Indent(6) + "{\r\n" +
                                 Indent(7) + "var dateTime = DateTimeOffset\r\n" +
                                 Indent(8) + ".FromUnixTimeSeconds(result)\r\n" +
                                 Indent(8) + ".DateTime\r\n" +
                                 Indent(8) + ".ToLocalTime();\r\n" +
                                 "\r\n" +
                                 Indent(7) + objDotField + " = (DateTime?)dateTime;\r\n" +
                                 Indent(6) + "}\r\n" +
                                 Indent(6) + "catch (ArgumentOutOfRangeException)\r\n" +
                                 Indent(6) + "{\r\n" +
                                 Indent(7) + objDotField + " = null;\r\n" +
                                 Indent(6) + "}\r\n" +
                                 Indent(5) + "}\r\n" +
                                 Indent(5) + "else\r\n" +
                                 Indent(5) + "{\r\n" +
                                 Indent(6) + objDotField + " = null;\r\n" +
                                 Indent(5) + "}");
                    */
                }
                else if (field.Type == "CustomResources")
                {
                    // Totally shouldn't be hardcoded...
                    sw.WriteLine(Indent(5) + obj + ".ResourcesScanned = !val.EqualsI(\"NotScanned\");\r\n" +
                                 Indent(5) + "FillFMHasXFields(fm, val);");
                }

                // if
                sw.WriteLine(Indent(4) + "}");

                customCodeBlockToInsertAfterField = field.CodeBlockToInsertAfter;
            }

            sw.WriteLine(Indent(4) + "if (resourcesFound) fm.ResourcesScanned = true;");

            foreach (string line in ReaderKeepLines) sw.WriteLine(line);

            // for
            sw.WriteLine(Indent(3) + "}");

            // method ReadFMDataIni
            sw.WriteLine(Indent(2) + "}");
        }

        private static void WriteWriter(StreamWriter sw, string obj)
        {
            sw.WriteLine(Indent(2) + AutogeneratedMessage);

            foreach (string l in WriteFMDataIniTopLines) sw.WriteLine(l);

            bool wroteHasXValues = false;


            const string toString = "ToString()";
            const string unixDateString = "UnixDateString";

            foreach (var field in Fields)
            {
                string objDotField = obj + "." + field.Name;
                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                void swlSBAppend(int indent, string objField, string value, string suffix = "")
                {
                    if (!suffix.IsEmpty()) suffix = "." + suffix;
                    sw.WriteLine(Indent(indent) + "sb.Append(\"" + objField + "=\");");
                    if (fieldIniName == "DateAdded")
                    {
                        // Disgusting >:(
                        sw.WriteLine(Indent(indent) + "// Again, important to convert to local time here because we don't do it on startup.");
                    }
                    sw.WriteLine(Indent(indent) + "sb.AppendLine(" + value + suffix + ");");
                }


                //if (field.Type == "List<string>")
                if (field.Type.StartsWith("List<"))
                {
                    bool listTypeIsString = field.Type == "List<string>";
                    if (field.ListType == ListType.MultipleLines)
                    {
                        string foreachType = listTypeIsString ? "string" : "var";
                        sw.WriteLine(Indent(4) + "foreach (" + foreachType + " s in " + objDotField + ")");
                        sw.WriteLine(Indent(4) + "{");

                        //if (Fields.WriteEmptyValues)
#if true
                        {
                            swlSBAppend(5, fieldIniName, "s", !listTypeIsString ? toString : "");
                        }
                        // Disabled for now, for AngelLoader-specific perf
#else
                        {
                            sw.WriteLine(Indent(6) + "if (!string.IsNullOrEmpty(s))\r\n" +
                                         Indent(6) + "{\r\n" +
                                         Indent(7) + "sw.WriteLine(\"" + fieldIniName + "=\" + s);\r\n" +
                                         Indent(6) + "}");
                        }
#endif

                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "string")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(Indent(4) + "sw.WriteLine(\"" + fieldIniName + "=\" + " + objDotField + ");");
                        swlSBAppend(4, fieldIniName, objDotField);
                    }
                    else
                    {
                        sw.WriteLine(Indent(4) + "if (!string.IsNullOrEmpty(" + objDotField + "))");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField);
                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "bool")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, toString);
                    }
                    else
                    {
                        sw.WriteLine(Indent(4) + "if (" + objDotField + ")");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, toString);
                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "bool?")
                {
#if false
                    // Dumb special-case for the moment
                    if (fieldIniName.StartsWith("Has"))
                    {
                        if (!wroteHasXValues)
                        {
                            sw.WriteLine(
                                Indent(5) + "if (Misc.FMCustomResourcesScanned(fm))\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "sw.WriteLine(\"HasResources=\" + CommaCombineHasXFields(fm));\r\n" +
                                Indent(5) + "}\r\n" +
                                Indent(5) + "else\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "sw.WriteLine(\"HasResources=None\");\r\n" +
                                Indent(5) + "}");
                            wroteHasXValues = true;
                        }
                    }
                    else
#endif
                    {
                        if (Fields.WriteEmptyValues)
                        {
                            swlSBAppend(4, fieldIniName, objDotField, toString);
                        }
                        else
                        {
                            sw.WriteLine(Indent(4) + "if (" + objDotField + " != null)");
                            sw.WriteLine(Indent(4) + "{");
                            swlSBAppend(5, fieldIniName, obj, toString);
                            sw.WriteLine(Indent(4) + "}");
                        }
                    }
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    string optCulture = field.Type == "float" || field.Type == "double" || field.Type == "decimal"
                        ? "CultureInfo.InvariantCulture"
                        : "";

                    if (!Fields.WriteEmptyValues && field.NumericEmpty != null)
                    {
                        sw.WriteLine(Indent(4) + "if (" + objDotField + " != " + field.NumericEmpty + ")");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, "ToString(" + optCulture + ")");
                        sw.WriteLine(Indent(4) + "}");
                    }
                    else
                    {
                        swlSBAppend(4, fieldIniName, objDotField, "ToString(" + optCulture + ")");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string optCulture = field.Type == "float" || field.Type == "double" || field.Type == "decimal"
                        ? "CultureInfo.InvariantCulture"
                        : "";

                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, "ToString(" + optCulture + ")");
                    }
                    else
                    {
                        sw.WriteLine(Indent(4) + "if (" + objDotField + " != null)");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, "ToString(" + optCulture + ")");
                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "Game")
                {
                    // TODO: With all the GameSupport stuff, this is completely out of step. Put my foot down and fix this! (and the other one too)
                    sw.WriteLine(Indent(4) + "switch (fm.Game)");
                    sw.WriteLine(Indent(4) + "{");
                    sw.WriteLine(Indent(5) + "// Much faster to do this than Enum.ToString()");
                    sw.WriteLine(Indent(5) + "case Game.Thief1:");
                    sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=Thief1\");");
                    sw.WriteLine(Indent(6) + "break;");
                    sw.WriteLine(Indent(5) + "case Game.Thief2:");
                    sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=Thief2\");");
                    sw.WriteLine(Indent(6) + "break;");
                    sw.WriteLine(Indent(5) + "case Game.Thief3:");
                    sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=Thief3\");");
                    sw.WriteLine(Indent(6) + "break;");
                    sw.WriteLine(Indent(5) + "case Game.SS2:");
                    sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=SS2\");");
                    sw.WriteLine(Indent(6) + "break;");
                    sw.WriteLine(Indent(5) + "case Game.Unsupported:");
                    sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=Unsupported\");");
                    sw.WriteLine(Indent(6) + "break;");
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(Indent(5) + "case Game.Null:");
                        sw.WriteLine(Indent(6) + "sb.AppendLine(\"Game=Null\");");
                        sw.WriteLine(Indent(6) + "break;");
                    }
                    else
                    {
                        sw.WriteLine(Indent(6) + "// Don't handle Game.Null because we don't want to write out defaults");
                    }
                    sw.WriteLine(Indent(4) + "}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, unixDateString);
                    }
                    else
                    {
                        sw.WriteLine(Indent(4) + "if (!string.IsNullOrEmpty(" + objDotField + ".UnixDateString))");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, unixDateString);
                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "DateTime?")
                {
                    // If we DIDN'T convert before, we need to convert now
                    string val = !field.DoNotConvertDateTimeToLocal
                        ? "new DateTimeOffset((DateTime)" + objDotField + ").ToUnixTimeSeconds().ToString(\"X\")" :
                        "new DateTimeOffset(((DateTime)" + objDotField + ").ToLocalTime()).ToUnixTimeSeconds().ToString(\"X\")";

                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, val);
                    }
                    else
                    {
                        sw.WriteLine(Indent(4) + "if (" + objDotField + " != null)");
                        sw.WriteLine(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, val);
                        sw.WriteLine(Indent(4) + "}");
                    }
                }
                else if (field.Type == "CustomResources")
                {
                    sw.WriteLine("#if write_old_resources_style");
                    sw.WriteLine(Indent(4) + "if (fm.ResourcesScanned)");
                    sw.WriteLine(Indent(4) + "{");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasMap=\" + FMHasResource(fm, CustomResources.Map).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasAutomap=\" + FMHasResource(fm, CustomResources.Automap).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasScripts=\" + FMHasResource(fm, CustomResources.Scripts).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasTextures=\" + FMHasResource(fm, CustomResources.Textures).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasSounds=\" + FMHasResource(fm, CustomResources.Sounds).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasObjects=\" + FMHasResource(fm, CustomResources.Objects).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasCreatures=\" + FMHasResource(fm, CustomResources.Creatures).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasMotions=\" + FMHasResource(fm, CustomResources.Motions).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasMovies=\" + FMHasResource(fm, CustomResources.Movies).ToString());");
                    sw.WriteLine(Indent(4) + "sb.AppendLine(\"HasSubtitles=\" + FMHasResource(fm, CustomResources.Subtitles).ToString());");
                    sw.WriteLine(Indent(4) + "}");
                    sw.WriteLine("#else");
                    sw.WriteLine(Indent(4) + "sb.Append(\"" + fieldIniName + "=\");");
                    sw.WriteLine(Indent(4) + "if (fm.ResourcesScanned)");
                    sw.WriteLine(Indent(4) + "{");
                    sw.WriteLine(Indent(5) + "CommaCombineHasXFields(fm, sb);");
                    sw.WriteLine(Indent(4) + "}");
                    sw.WriteLine(Indent(4) + "else");
                    sw.WriteLine(Indent(4) + "{");
                    sw.WriteLine(Indent(5) + "sb.AppendLine(\"NotScanned\");");
                    sw.WriteLine(Indent(4) + "}");
                    sw.WriteLine("#endif");
                }
            }

            foreach (string line in WriterKeepLines) sw.WriteLine(line);

            // for
            sw.WriteLine(Indent(3) + "}");

            sw.WriteLine();
            sw.WriteLine(Indent(3) + "using var sw = new StreamWriter(fileName, false, Encoding.UTF8);");
            sw.WriteLine(Indent(3) + "sw.Write(sb.ToString());");

            // method WriteFMDataIni
            sw.WriteLine(Indent(2) + "}");
        }

        private static void ReadSourceFields(string className, string sourceFile)
        {
            string[] sourceLines = File.ReadAllLines(sourceFile);

            bool inClass = false;

            bool doNotSerializeNextLine = false;
            string iniNameForThisField = null;
            string listItemPrefixForThisField = null;
            var listTypeForThisField = ListType.MultipleLines;
            var listDistinctTypeForThisField = ListDistinctType.None;
            var enumMapForThisField = new Dictionary<string, string>();
            var enumValuesForThisField = new List<string>();
            long? numericEmptyForThisField = null;
            bool doNotTrimValueForThisField = false;
            bool doNotConvertDateTimeToLocalForThisField = false;
            string commentForThisField = null;
            var codeBlockToInsertAfterThisField = CustomCodeBlockNames.None;

            for (int i = 0; i < sourceLines.Length; i++)
            {
                string line = sourceLines[i].Trim();

                if (!inClass)
                {
                    bool lineIsClassDef = line.EndsWith("class " + className);

                    if (i > 0 && lineIsClassDef)
                    {
                        string prevLine = sourceLines[i - 1].Trim();
                        if (prevLine.StartsWith("//") && prevLine.Substring(2).Trim().StartsWith("[FenGen:") &&
                            prevLine[prevLine.Length - 1] == ']')
                        {
                            int ind;
                            string kvp = prevLine.Substring(ind = prevLine.IndexOf(':') + 1, prevLine.Length - ind - 1);
                            if (kvp.Contains("="))
                            {
                                string key = kvp.Substring(0, kvp.IndexOf('=')).Trim();
                                string val = kvp.Substring(kvp.IndexOf('=') + 1).Trim();
                                if (key == "WriteEmptyValues")
                                {
                                    Fields.WriteEmptyValues = val.EqualsTrue();
                                }
                            }
                        }
                    }

                    if (lineIsClassDef) inClass = true;
                    continue;
                }

                if (doNotSerializeNextLine)
                {
                    if (!string.IsNullOrWhiteSpace(line)) doNotSerializeNextLine = false;
                    continue;
                }

                if (line == "}") break;

                if (line.StartsWith("//"))
                {
                    string attr = line.Substring(2).Trim();
                    if (attr.StartsWith("[FenGen:"))
                    {
                        if (attr == "[FenGen:DoNotSerialize]")
                        {
                            doNotSerializeNextLine = true;
                            continue;
                        }

                        if (attr == "[FenGen:DoNotTrimValue]")
                        {
                            doNotTrimValueForThisField = true;
                            continue;
                        }

                        if (attr == "[FenGen:DoNotConvertDateTimeToLocal]")
                        {
                            doNotConvertDateTimeToLocalForThisField = true;
                            continue;
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("InsertAfter="))
                        {
                            string val = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            if (val == nameof(CustomCodeBlockNames.LegacyCustomResources))
                            {
                                codeBlockToInsertAfterThisField = CustomCodeBlockNames.LegacyCustomResources;
                                continue;
                            }
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("Comment="))
                        {
                            string val = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            commentForThisField = !val.IsWhiteSpace() ? val : null;
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("EnumValues="))
                        {
                            string valString = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            string[] valArray = valString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int li = 0; li < valArray.Length; li++)
                            {
                                string item = valArray[i];
                                item = item.Trim();
                                if (!enumValuesForThisField.Contains(item))
                                {
                                    enumValuesForThisField.Add(item);
                                }
                            }

                            continue;
                        }

                        int ind;
                        if (attr.Substring(ind = attr.IndexOf(':') + 1).StartsWith("EnumMap:"))
                        {
                            string kvpString = attr.Substring(attr.IndexOf(':', ind) + 1).TrimEnd(']');
                            string[] kvpArray = kvpString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int li = 0; li < kvpArray.Length; li++)
                            {
                                string item = kvpArray[li];
                                item = item.Trim();
                                if (item.Contains('='))
                                {
                                    string key = item.Substring(0, item.IndexOf('=')).Trim();
                                    string val = item.Substring(item.IndexOf('=') + 1).Trim();
                                    enumMapForThisField[key] = val;
                                }
                            }

                            continue;
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("NumericEmpty="))
                        {
                            string val = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            if (long.TryParse(val, out long result)) numericEmptyForThisField = result;
                            continue;
                        }

                        string kvp = attr.Substring(ind = attr.IndexOf(':') + 1, attr.Length - ind - 1);
                        if (kvp.Contains("="))
                        {
                            string key = kvp.Substring(0, kvp.IndexOf('=')).Trim();
                            string val = kvp.Substring(kvp.IndexOf('=') + 1).Trim();

                            switch (key)
                            {
                                case "IniName":
                                    iniNameForThisField = val;
                                    break;
                                case "ListItemPrefix":
                                    listItemPrefixForThisField = val;
                                    break;
                                case "ListType":
                                    listTypeForThisField = val == "CommaSeparated"
                                            ? ListType.CommaSeparated
                                            : ListType.MultipleLines;
                                    break;
                                case "ListDistinctType":
                                    listDistinctTypeForThisField =
                                        val == "None" ? ListDistinctType.None :
                                        val == "Exact" ? ListDistinctType.Exact :
                                        val == "CaseInsensitive" ? ListDistinctType.CaseInsensitive :
                                        ListDistinctType.None;
                                    break;
                            }
                        }

                    }
                    continue;
                }

                int index = line.IndexOf('{');
                if (index == -1) index = line.IndexOf('=');
                if (index == -1) index = line.IndexOf(';');
                if (index == -1) continue;

                string line2 = line.Substring(0, index).Trim();

                index = line2.LastIndexOf(' ');
                if (index == -1) continue;

                Fields.Add(new Field());
                var last = Fields[Fields.Count - 1];

                if (!iniNameForThisField.IsEmpty())
                {
                    last.IniName = iniNameForThisField;
                    iniNameForThisField = null;
                }
                if (!listItemPrefixForThisField.IsEmpty())
                {
                    last.ListItemPrefix = listItemPrefixForThisField;
                    listItemPrefixForThisField = null;
                }
                if (enumMapForThisField.Count > 0)
                {
                    last.EnumMap = enumMapForThisField;
                    enumMapForThisField.Clear();
                }
                if (enumValuesForThisField.Count > 0)
                {
                    last.EnumValues = enumValuesForThisField;
                    enumValuesForThisField.Clear();
                }
                if (numericEmptyForThisField != null)
                {
                    last.NumericEmpty = numericEmptyForThisField;
                    numericEmptyForThisField = null;
                }
                if (!commentForThisField.IsEmpty())
                {
                    last.Comment = commentForThisField;
                    commentForThisField = null;
                }
                if (codeBlockToInsertAfterThisField != CustomCodeBlockNames.None)
                {
                    last.CodeBlockToInsertAfter = codeBlockToInsertAfterThisField;
                    codeBlockToInsertAfterThisField = CustomCodeBlockNames.None;
                }

                last.DoNotTrimValue = doNotTrimValueForThisField;
                doNotTrimValueForThisField = false;
                last.DoNotConvertDateTimeToLocal = doNotConvertDateTimeToLocalForThisField;
                doNotConvertDateTimeToLocalForThisField = false;
                last.ListType = listTypeForThisField;
                listTypeForThisField = ListType.MultipleLines;
                last.ListDistinctType = listDistinctTypeForThisField;
                listDistinctTypeForThisField = ListDistinctType.None;

                last.Name = line2.Substring(index + 1).Trim();

                string temp = StripPrefixes(line2);

                last.Type = temp.Substring(0, temp.LastIndexOf(' '));
            }
        }
    }
}
