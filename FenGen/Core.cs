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
        internal string WriteName;
        internal string ListItemPrefix;
        internal ListType ListType = ListType.MultipleLines;
        internal ListDistinctType ListDistinctType = ListDistinctType.None;
        internal Dictionary<string, string> EnumMap = new Dictionary<string, string>();
        internal List<string> EnumValues = new List<string>();
        internal long? NumericEmpty;
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
        MainFormBacking
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
            "            var iniLines = File.ReadAllLines(fileName, Encoding.UTF8);",
            "",
            "            if (fmsList.Count > 0) fmsList.Clear();",
            "",
            "            bool fmsListIsEmpty = true;",
            "",
            "            foreach (var line in iniLines)",
            "            {",
            "                var lineT = line.TrimStart();",
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
            "                // Comment chars (;) and blank lines will be rejected implicitly.",
            "                // Since they're rare cases, checking for them would only slow us down.",
            "",
            "                var fm = fmsList[fmsList.Count - 1];",
            ""
        };

#if enable_config_gen
        private static string[] ReadConfigIniTopLines { get; } =
        {
            "        internal static ConfigData ReadConfigIni(string fileName)",
            "        {",
            "            var config = new ConfigData();",
            "",
            "            var iniLines = File.ReadAllLines(fileName, Encoding.UTF8);",
            "",
            "            foreach (var line in iniLines)",
            "            {",
            "                var lineT = line.TrimStart();",
            ""
        };
#endif

        private static string[] WriteFMDataIniTopLines { get; } =
        {
            "        internal static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)",
            "        {",
            "            using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))",
            "            {",
            "                foreach (var fm in fmDataList)",
            "                {",
            "                    sw.WriteLine(\"[FM]\");",
            ""
        };

        private static List<string> ReaderKeepLines { get; } = new List<string>();
        private static List<string> WriterKeepLines { get; } = new List<string>();

#if DEBUG
        private static MainForm View;
#endif

        internal static string ALProjectPath;

        internal static void Init()
        {
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
                            var sourceFile = Path.Combine(ALProjectPath, @"Properties\AssemblyInfo.cs");
                            VersionIncrement.Generate(sourceFile, VersionType.Beta);
                        }
                        break;
                    case "-version=public":
                        if (!GenTasks.Contains(GenType.Version))
                        {
                            GenTasks.Add(GenType.Version);
                            var sourceFile = Path.Combine(ALProjectPath, @"Properties\AssemblyInfo.cs");
                            VersionIncrement.Generate(sourceFile, VersionType.PublicRelease);
                        }
                        break;
                    case "-fmdata":
                        // Switched this off due to the extensive manual edits for new HasResources format
                        break;
                        if (!GenTasks.Contains(GenType.FMData))
                        {
                            GenTasks.Add(GenType.FMData);
                            var sourceFile = Path.Combine(ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
                            var destFile = Path.Combine(ALProjectPath, @"Ini\FMData.cs");
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
                            var langFile = Path.Combine(ALProjectPath, @"Languages\English.ini");
                            LanguageGen.Generate(langFile);
                        }
                        break;
                    case "-language_t":
                        if (!GenTasks.Contains(GenType.Language))
                        {
                            GenTasks.Add(GenType.Language);
                            var langFile = Path.Combine(ALProjectPath, @"Languages\English.ini");
                            StateVars.WriteTestLangFile = true;
                            StateVars.TestFile = @"C:\AngelLoader\Data\Languages\TestLang.ini";
                            LanguageGen.Generate(langFile);
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

            //var className = GenType == GenType.FMData ? "FanMission" : "ConfigData";
            var className = "FanMission";
            ReadSourceFields(className, sourceFile);

            using (var sr = new StreamReader(destFile))
            {
                int openBraces = 0;
                bool inClass = false;
                bool inReaderKeepBlock = false;
                bool inWriterKeepBlock = false;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;

                    var lineT = line.Trim();

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
                            var attr = lineT.Substring(2).Trim();
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
                        var attr = lineT.Substring(2).Trim();
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

            foreach (var l in DestTopLines) sw.WriteLine(l);

            //var obj = GenType == GenType.FMData ? "fm" : "config";
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
                foreach (var l in TopCodeLines) sw.WriteLine(l);
            }

            sw.WriteLine(Indent(2) + AutogeneratedMessage);

            //var topLines = (GenType == GenType.FMData
            //    ? ReadFMDataIniTopLines
            //    : ReadConfigIniTopLines).ToList();

            var topLines = ReadFMDataIniTopLines;

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

            foreach (var l in topLines) sw.WriteLine(l);

            bool wroteHasXFields = false;

            for (var i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];
                var objDotField = obj + "." + field.Name;

                string optElse = i > 0 ? "else " : "";

                var fieldWriteName = field.WriteName.IsEmpty() ? field.Name : field.WriteName;

                sw.WriteLine(
                    Indent(4) +
                    optElse +
                    "if (lineT.StartsWithFast_NoNullChecks(\"" + fieldWriteName + "=\"))\r\n" +
                    Indent(4) + "{\r\n" +
                    Indent(5) + "var val = lineT.Substring(" + (fieldWriteName.Length + 1) + ");");

                //sw.WriteLine(
                //    Indent(4) +
                //    optElse +
                //    "if (!" + field.Name + "Read && lineT.StartsWithFast_NoNullChecks(\"" + fieldWriteName + "=\"))\r\n" +
                //    Indent(4) + "{\r\n" +
                //    Indent(5) + field.Name + "Read = true;\r\n" +
                //    Indent(5) + "var val = lineT.Substring(" + (fieldWriteName.Length + 1) + ");");

                if (field.Type.StartsWith("List<"))
                {
                    var listType = field.Type.Substring(field.Type.IndexOf('<')).TrimStart('<').TrimEnd('>');

                    var ldt = field.ListDistinctType;

                    var varToAdd = listType == "string" && field.ListType == ListType.MultipleLines
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
                                Indent(5) + "var items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);\r\n" +
                                Indent(5) + "for (var a = 0; a < items.Length; a++)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "var result = items[a].Trim();\r\n" +
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
                                Indent(5) + "var items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);\r\n" +
                                Indent(5) + "for (var a = 0; a < items.Length; a++)\r\n" +
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
                    var ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    sw.WriteLine(Indent(5) + "var success = " + ftNonNull + ".TryParse(val, out " + ftNonNull + " result);\r\n" +
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
                    sw.WriteLine(Indent(5) + "var success = long.TryParse(\r\n" +
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
                }

                // if
                sw.WriteLine(Indent(4) + "}");
            }

            foreach (var line in ReaderKeepLines) sw.WriteLine(line);

            // for
            sw.WriteLine(Indent(3) + "}");

            // method ReadFMDataIni
            sw.WriteLine(Indent(2) + "}");
        }

        private static void WriteWriter(StreamWriter sw, string obj)
        {
            sw.WriteLine(Indent(2) + AutogeneratedMessage);

            foreach (var l in WriteFMDataIniTopLines) sw.WriteLine(l);

            bool wroteHasXValues = false;

            foreach (var field in Fields)
            {
                var objDotField = obj + "." + field.Name;

                var fieldWriteName = field.WriteName.IsEmpty() ? field.Name : field.WriteName;

                //if (field.Type == "List<string>")
                if (field.Type.StartsWith("List<"))
                {
                    if (field.ListType == ListType.MultipleLines)
                    {
                        sw.WriteLine(
                            Indent(5) + "foreach (var s in " + objDotField + ")\r\n" +
                            Indent(5) + "{");

                        //if (Fields.WriteEmptyValues)
#if true
                        {
                            sw.WriteLine(Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + s);");
                        }
                        // Disabled for now, for AngelLoader-specific perf
#else
                        {
                            sw.WriteLine(Indent(6) + "if (!string.IsNullOrEmpty(s))\r\n" +
                                         Indent(6) + "{\r\n" +
                                         Indent(7) + "sw.WriteLine(\"" + fieldWriteName + "=\" + s);\r\n" +
                                         Indent(6) + "}");
                        }
#endif

                        sw.WriteLine(Indent(5) + "}");
                    }
                }
                else if (field.Type == "string")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ");");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (!string.IsNullOrEmpty(" + objDotField + "))\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ");\r\n" +
                            Indent(5) + "}");
                    }
                }
                else if (field.Type == "bool")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                            ".ToString());");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (" + objDotField + ")\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                            ".ToString());\r\n" +
                            Indent(5) + "}");
                    }
                }
                else if (field.Type == "bool?")
                {
                    // Dumb special-case for the moment
                    if (fieldWriteName.StartsWith("Has"))
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
                    {
                        if (Fields.WriteEmptyValues)
                        {
                            sw.WriteLine(
                                Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                                ".ToString());");
                        }
                        else
                        {
                            sw.WriteLine(
                                Indent(5) + "if (" + objDotField + " != null)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                                ".ToString());\r\n" +
                                Indent(5) + "}");
                        }
                    }
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    var optCulture = field.Type == "float" || field.Type == "double" || field.Type == "decimal"
                        ? "CultureInfo.InvariantCulture"
                        : "";

                    if (!Fields.WriteEmptyValues && field.NumericEmpty != null)
                    {
                        sw.WriteLine(
                            Indent(5) + "if (" + objDotField + " != " + field.NumericEmpty + ")\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".ToString(" + optCulture + "));\r\n" +
                            Indent(5) + "}");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".ToString(" + optCulture + "));");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    var optCulture = field.Type == "float" || field.Type == "double" || field.Type == "decimal"
                        ? "CultureInfo.InvariantCulture"
                        : "";

                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".ToString(" + optCulture + "));");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (" + objDotField + " != null)\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".ToString(" + optCulture + "));\r\n" +
                            Indent(5) + "}");
                    }
                }
                else if (field.Type == "Game")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                            ".ToString());");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (" + objDotField + " != Game.Null)\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField +
                            ".ToString());\r\n" +
                            Indent(5) + "}");
                    }
                }
                else if (field.Type == "ExpandableDate")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".UnixDateString);");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (!string.IsNullOrEmpty(" + objDotField + ".UnixDateString))\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + " + objDotField + ".UnixDateString);\r\n" +
                            Indent(5) + "}");
                    }
                }
                else if (field.Type == "DateTime?")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        sw.WriteLine(
                            Indent(5) + "var val = new DateTimeOffset((DateTime)" + objDotField +
                            ").ToUnixTimeSeconds().ToString(\"X\");\r\n" +
                            Indent(5) + "sw.WriteLine(\"" + fieldWriteName + "=\" + val);");
                    }
                    else
                    {
                        sw.WriteLine(
                            Indent(5) + "if (" + objDotField + " != null)\r\n" +
                            Indent(5) + "{\r\n" +
                            Indent(6) + "var val = new DateTimeOffset((DateTime)" + objDotField +
                            ").ToUnixTimeSeconds().ToString(\"X\");\r\n" +
                            Indent(6) + "sw.WriteLine(\"" + fieldWriteName + "=\" + val);\r\n" +
                            Indent(5) + "}");
                    }
                }
            }

            foreach (var line in WriterKeepLines) sw.WriteLine(line);

            // for
            sw.WriteLine(Indent(4) + "}");

            // using
            sw.WriteLine(Indent(3) + "}");

            // method WriteFMDataIni
            sw.WriteLine(Indent(2) + "}");
        }

        private static void ReadSourceFields(string className, string sourceFile)
        {
            var sourceLines = File.ReadAllLines(sourceFile);

            bool inClass = false;

            bool doNotSerializeNextLine = false;
            string writeNameForThisField = null;
            string listItemPrefixForThisField = null;
            var listTypeForThisField = ListType.MultipleLines;
            var listDistinctTypeForThisField = ListDistinctType.None;
            var enumMapForThisField = new Dictionary<string, string>();
            var enumValuesForThisField = new List<string>();
            long? numericEmptyForThisField = null;

            for (var i = 0; i < sourceLines.Length; i++)
            {
                var line = sourceLines[i].Trim();

                if (!inClass)
                {
                    var lineIsClassDef = line.EndsWith("class " + className);

                    if (i > 0 && lineIsClassDef)
                    {
                        var prevLine = sourceLines[i - 1].Trim();
                        if (prevLine.StartsWith("//") && prevLine.Substring(2).Trim().StartsWith("[FenGen:") &&
                            prevLine[prevLine.Length - 1] == ']')
                        {
                            int ind;
                            var kvp = prevLine.Substring(ind = prevLine.IndexOf(':') + 1, prevLine.Length - ind - 1);
                            if (kvp.Contains("="))
                            {
                                var key = kvp.Substring(0, kvp.IndexOf('=')).Trim();
                                var val = kvp.Substring(kvp.IndexOf('=') + 1).Trim();
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
                    var attr = line.Substring(2).Trim();
                    if (attr.StartsWith("[FenGen:"))
                    {
                        if (attr == "[FenGen:DoNotSerialize]")
                        {
                            doNotSerializeNextLine = true;
                            continue;
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("EnumValues="))
                        {
                            var valString = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            var valArray = valString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            for (var li = 0; li < valArray.Length; li++)
                            {
                                var item = valArray[i];
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
                            var kvpString = attr.Substring(attr.IndexOf(':', ind) + 1).TrimEnd(']');
                            var kvpArray = kvpString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            for (var li = 0; li < kvpArray.Length; li++)
                            {
                                var item = kvpArray[li];
                                item = item.Trim();
                                if (item.Contains('='))
                                {
                                    var key = item.Substring(0, item.IndexOf('=')).Trim();
                                    var val = item.Substring(item.IndexOf('=') + 1).Trim();
                                    enumMapForThisField[key] = val;
                                }
                            }

                            continue;
                        }

                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("NumericEmpty="))
                        {
                            var val = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            if (long.TryParse(val, out long result)) numericEmptyForThisField = result;
                            continue;
                        }

                        var kvp = attr.Substring(ind = attr.IndexOf(':') + 1, attr.Length - ind - 1);
                        if (kvp.Contains("="))
                        {
                            var key = kvp.Substring(0, kvp.IndexOf('=')).Trim();
                            var val = kvp.Substring(kvp.IndexOf('=') + 1).Trim();

                            switch (key)
                            {
                                case "WriteName":
                                    writeNameForThisField = val;
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

                var index = line.IndexOf('{');
                if (index == -1) index = line.IndexOf('=');
                if (index == -1) index = line.IndexOf(';');
                if (index == -1) continue;

                var line2 = line.Substring(0, index).Trim();

                index = line2.LastIndexOf(' ');
                if (index == -1) continue;

                Fields.Add(new Field());
                var last = Fields[Fields.Count - 1];

                if (!writeNameForThisField.IsEmpty())
                {
                    last.WriteName = writeNameForThisField;
                    writeNameForThisField = null;
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

                last.ListType = listTypeForThisField;
                listTypeForThisField = ListType.MultipleLines;
                last.ListDistinctType = listDistinctTypeForThisField;
                listDistinctTypeForThisField = ListDistinctType.None;

                last.Name = line2.Substring(index + 1).Trim();

                var temp = StripPrefixes(line2);

                last.Type = temp.Substring(0, temp.LastIndexOf(' '));
            }
        }
    }
}
