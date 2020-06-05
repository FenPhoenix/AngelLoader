using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static FenGen.Misc;

namespace FenGen
{
    internal static class FMData
    {
        private sealed class Field
        {
            internal string Type = "";
            internal string Name = "";
            internal string IniName = "";
            internal string ListItemPrefix = "";
            internal ListType ListType = ListType.MultipleLines;
            internal ListDistinctType ListDistinctType = ListDistinctType.None;
            internal Dictionary<string, string> EnumMap = new Dictionary<string, string>();
            internal List<string> EnumValues = new List<string>();
            internal long? NumericEmpty;
            internal bool DoNotTrimValue;
            internal bool DoNotConvertDateTimeToLocal;
            internal string Comment = "";
            internal CustomCodeBlockNames CodeBlockToInsertAfter = CustomCodeBlockNames.None;
        }

        private sealed class FieldList : List<Field>
        {
            internal bool WriteEmptyValues;
            //internal string Version;
        }

        private enum ListType
        {
            MultipleLines,
            CommaSeparated
        }

        private enum ListDistinctType
        {
            None,
            Exact,
            CaseInsensitive
        }

        private enum CustomCodeBlockNames
        {
            None,
            LegacyCustomResources
        }

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

        internal static void Generate(string sourceFile, string destFile)
        {
#if DEBUG
            //GenType = GenType.FMData;
            //GenType = GenType.Language;
#endif

            //string className = GenType == GenType.FMData ? "FanMission" : "ConfigData";
            const string className = "FanMission";
            ReadSourceFields(className, sourceFile);

            // Always do an atomic read operation, because we may want to open the same file
            // for other purposes in the middle of it (we had an access exception before)
            var lines = File.ReadAllLines(destFile);

            int openBraces = 0;
            bool inClass = false;
            bool inReaderKeepBlock = false;
            bool inWriterKeepBlock = false;
            foreach (string line in lines)
            {
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

            var writeLines = new List<string>();

            foreach (string l in DestTopLines) writeLines.Add(l);

            //string obj = GenType == GenType.FMData ? "fm" : "config";
            const string obj = "fm";

            WriteReader(writeLines, obj);

            writeLines.Add("");

            WriteWriter(writeLines, obj);

            // class
            writeLines.Add(Indent(1) + "}");

            // namespace
            writeLines.Add("}");

            File.WriteAllLines(destFile, writeLines);
        }

        private static void WriteReader(List<string> wl, string obj)
        {
            if (TopCodeLines.Length > 0)
            {
                wl.Add(Indent(2) + GenMessages.SupportingCode);
                foreach (string l in TopCodeLines) wl.Add(l);
            }

            wl.Add(Indent(2) + GenMessages.Method);

            //var topLines = (GenType == GenType.FMData
            //    ? ReadFMDataIniTopLines
            //    : ReadConfigIniTopLines).ToList();

            string[] topLines = ReadFMDataIniTopLines;

            foreach (string l in topLines) wl.Add(l);

            CustomCodeBlockNames customCodeBlockToInsertAfterField = CustomCodeBlockNames.None;

            static string GetFloatArgsRead(string fieldType) =>
                fieldType == "float" ||
                fieldType == "float?" ||
                fieldType == "double" ||
                fieldType == "double?" ||
                fieldType == "decimal" ||
                fieldType == "decimal?"
                    ? "NumberStyles.Float, NumberFormatInfo.InvariantInfo, "
                    : "";

            for (int i = 0; i < Fields.Count; i++)
            {
                if (customCodeBlockToInsertAfterField != CustomCodeBlockNames.None)
                {
                    switch (customCodeBlockToInsertAfterField)
                    {
                        case CustomCodeBlockNames.LegacyCustomResources:
                            foreach (string line in CustomCodeBlocks.LegacyCustomResourceReads)
                            {
                                wl.Add(line);
                            }
                            break;
                    }
                }

                var field = Fields[i];
                string objDotField = obj + "." + field.Name;

                string optElse = i > 0 ? "else " : "";

                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                string lineTrimmedVersion = field.DoNotTrimValue ? "lineTS" : "lineT";

                wl.Add(
                    Indent(4) +
                    optElse +
                    "if (lineT.StartsWithFast_NoNullChecks(\"" + fieldIniName + "=\"))\r\n" +
                    Indent(4) + "{\r\n" +
                    Indent(5) + "string val = " + lineTrimmedVersion + ".Substring(" + (fieldIniName.Length + 1) + ");");

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
                            //sw.Add(
                            //    Indent(5) + "if (" + objDotField + " == null)\r\n" +
                            //    Indent(5) + "{\r\n" +
                            //    Indent(6) + objDotField + " = new List<string>();\r\n" +
                            //    Indent(5) + "}\r\n" +
                            //    Indent(5) + "if (!string.IsNullOrEmpty(val))\r\n" +
                            //    Indent(5) + "{\r\n" +
                            //    Indent(6) + objListSet + "\r\n" +
                            //    Indent(5) + "}");

                            wl.Add(
                                Indent(5) + "if (!string.IsNullOrEmpty(val))\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objListSet + "\r\n" +
                                Indent(5) + "}");
                        }
                        else
                        {
                            wl.Add(
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
                        string floatArgs = GetFloatArgsRead(listType);
                        if (field.ListType == ListType.MultipleLines)
                        {
                            wl.Add(
                                Indent(5) + "if (" + objDotField + " == null)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objDotField + " = new List<" + listType + ">();\r\n" +
                                Indent(5) + "}\r\n" +
                                Indent(5) + "bool success = " + listType + ".TryParse(val, " + floatArgs + "out " + listType + " result);\r\n" +
                                Indent(5) + "if(success)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + objListSet + "\r\n" +
                                Indent(5) + "}");
                        }
                        else
                        {
                            wl.Add(
                                Indent(5) + objDotField + ".Clear();\r\n" +
                                Indent(5) + "string[] items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);\r\n" +
                                Indent(5) + "for (int a = 0; a < items.Length; a++)\r\n" +
                                Indent(5) + "{\r\n" +
                                Indent(6) + "items[a] = items[a].Trim();\r\n" +
                                Indent(6) + "bool success = " + listType + ".TryParse(items[a], " + floatArgs + "out " + listType + " result);\r\n" +
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
                    wl.Add(Indent(5) + objDotField + " = val;");
                }
                else if (field.Type == "bool")
                {
                    wl.Add(Indent(5) + objDotField + " = val.EqualsTrue();");
                }
                else if (field.Type == "bool?")
                {
                    wl.Add(Indent(5) + objDotField + " =\r\n" +
                                 Indent(6) + "!string.IsNullOrEmpty(val) ? val.EqualsTrue() : (bool?)null;");
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    if (field.NumericEmpty != null && field.NumericEmpty != 0)
                    {
                        wl.Add(Indent(5) + "bool success = " + field.Type + ".TryParse(val, " + floatArgs + "out " + field.Type + " result);");
                        wl.Add(Indent(5) + objDotField + " = success ? result : " + field.NumericEmpty + ";");
                    }
                    else
                    {
                        wl.Add(Indent(5) + field.Type + ".TryParse(val, " + floatArgs + "out " + field.Type + " result);\r\n" +
                                     Indent(5) + objDotField + " = result;");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                        NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    wl.Add(Indent(5) + "bool success = " + ftNonNull + ".TryParse(val, " + floatArgs + "out " + ftNonNull + " result);\r\n" +
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
                    wl.Add(Indent(5) + "val = val.Trim();");

                    var gamesEnum = Cache.GamesEnum;

                    for (int gi = 1; gi < gamesEnum.GameEnumNames.Count; gi++)
                    {
                        string ifType = gi > 1 ? "else if" : "if";
                        string gameDotGameType = gamesEnum.Name + "." + gamesEnum.GameEnumNames[gi];
                        wl.Add(Indent(5) + ifType + " (val.EqualsI(\"" + gamesEnum.GameEnumNames[gi] + "\"))");
                        wl.Add(Indent(5) + "{");
                        wl.Add(Indent(6) + objDotField + " = " + gameDotGameType + ";");
                        wl.Add(Indent(5) + "}");
                    }
                    wl.Add(Indent(5) + "else");
                    wl.Add(Indent(5) + "{");
                    wl.Add(Indent(6) + objDotField + " = " + gamesEnum.Name + "." + gamesEnum.GameEnumNames[0] + ";");
                    wl.Add(Indent(5) + "}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    wl.Add(Indent(5) + objDotField + ".UnixDateString = val;");
                }
                else if (field.Type == "DateTime?")
                {
                    wl.Add(Indent(5) + "// PERF: Don't convert to local here; do it at display-time\r\n" +
                                 Indent(5) + objDotField + " = ConvertHexUnixDateToDateTime(val, convertToLocal: " +
                                 (!field.DoNotConvertDateTimeToLocal).ToString().ToLowerInvariant() + ");");
                }
                else if (field.Type == "CustomResources")
                {
                    // Totally shouldn't be hardcoded...
                    wl.Add(Indent(5) + obj + ".ResourcesScanned = !val.EqualsI(\"NotScanned\");\r\n" +
                                 Indent(5) + "FillFMHasXFields(fm, val);");
                }

                // if
                wl.Add(Indent(4) + "}");

                customCodeBlockToInsertAfterField = field.CodeBlockToInsertAfter;
            }

            wl.Add(Indent(4) + "if (resourcesFound) fm.ResourcesScanned = true;");

            foreach (string line in ReaderKeepLines) wl.Add(line);

            // for
            wl.Add(Indent(3) + "}");

            // method ReadFMDataIni
            wl.Add(Indent(2) + "}");
        }

        private static void WriteWriter(List<string> wl, string obj)
        {
            wl.Add(Indent(2) + GenMessages.Method);

            foreach (string l in WriteFMDataIniTopLines) wl.Add(l);

            const string toString = "ToString()";
            const string unixDateString = "UnixDateString";

            static string GetFloatArgsWrite(string fieldType) =>
                fieldType == "float" ||
                fieldType == "float?" ||
                fieldType == "double" ||
                fieldType == "double?" ||
                fieldType == "decimal" ||
                fieldType == "decimal?"
                    ? "NumberFormatInfo.InvariantInfo"
                    : "";

            foreach (var field in Fields)
            {
                string objDotField = obj + "." + field.Name;
                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                void swlSBAppend(int indent, string objField, string value, string suffix = "")
                {
                    if (!suffix.IsEmpty()) suffix = "." + suffix;
                    wl.Add(Indent(indent) + "sb.Append(\"" + objField + "=\");");
                    if (fieldIniName == "DateAdded")
                    {
                        // Disgusting >:(
                        wl.Add(Indent(indent) + "// Again, important to convert to local time here because we don't do it on startup.");
                    }
                    wl.Add(Indent(indent) + "sb.AppendLine(" + value + suffix + ");");
                }

                if (field.Type.StartsWith("List<"))
                {
                    bool listTypeIsString = field.Type == "List<string>";
                    if (field.ListType == ListType.MultipleLines)
                    {
                        string foreachType = listTypeIsString ? "string" : "var";
                        wl.Add(Indent(4) + "foreach (" + foreachType + " s in " + objDotField + ")");
                        wl.Add(Indent(4) + "{");

                        //if (Fields.WriteEmptyValues)
#if true
                        {
                            swlSBAppend(5, fieldIniName, "s", !listTypeIsString ? toString : "");
                        }
                        // Disabled for now, for AngelLoader-specific perf
#else
                        {
                            sw.Add(Indent(6) + "if (!string.IsNullOrEmpty(s))\r\n" +
                                         Indent(6) + "{\r\n" +
                                         Indent(7) + "sw.WriteLine(\"" + fieldIniName + "=\" + s);\r\n" +
                                         Indent(6) + "}");
                        }
#endif

                        wl.Add(Indent(4) + "}");
                    }
                }
                else if (field.Type == "string")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        wl.Add(Indent(4) + "sw.WriteLine(\"" + fieldIniName + "=\" + " + objDotField + ");");
                        swlSBAppend(4, fieldIniName, objDotField);
                    }
                    else
                    {
                        wl.Add(Indent(4) + "if (!string.IsNullOrEmpty(" + objDotField + "))");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField);
                        wl.Add(Indent(4) + "}");
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
                        wl.Add(Indent(4) + "if (" + objDotField + ")");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, toString);
                        wl.Add(Indent(4) + "}");
                    }
                }
                else if (field.Type == "bool?")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, toString);
                    }
                    else
                    {
                        wl.Add(Indent(4) + "if (" + objDotField + " != null)");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, obj, toString);
                        wl.Add(Indent(4) + "}");
                    }
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsWrite(field.Type);
                    if (!Fields.WriteEmptyValues && field.NumericEmpty != null)
                    {
                        wl.Add(Indent(4) + "if (" + objDotField + " != " + field.NumericEmpty + ")");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                        wl.Add(Indent(4) + "}");
                    }
                    else
                    {
                        swlSBAppend(4, fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsWrite(field.Type);
                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                    }
                    else
                    {
                        wl.Add(Indent(4) + "if (" + objDotField + " != null)");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                        wl.Add(Indent(4) + "}");
                    }
                }
                else if (field.Type == Cache.GamesEnum.Name)
                {
                    var gamesEnum = Cache.GamesEnum;

                    wl.Add(Indent(4) + "switch (fm." + gamesEnum.Name + ")");
                    wl.Add(Indent(4) + "{");
                    for (int gi = 1; gi < gamesEnum.GameEnumNames.Count; gi++)
                    {
                        if (gi == 1) wl.Add(Indent(5) + "// Much faster to do this than Enum.ToString()");
                        wl.Add(Indent(5) + "case " + gamesEnum.Name + "." + gamesEnum.GameEnumNames[gi] + ":");
                        wl.Add(Indent(6) + "sb.AppendLine(\"" + gamesEnum.Name + "=" + gamesEnum.GameEnumNames[gi] + "\");");
                        wl.Add(Indent(6) + "break;");
                    }
                    string gameDotGameTypeZero = gamesEnum.Name + "." + gamesEnum.GameEnumNames[0];
                    if (Fields.WriteEmptyValues)
                    {
                        wl.Add(Indent(5) + "case " + gameDotGameTypeZero + ":");
                        wl.Add(Indent(6) + "sb.AppendLine(\"" + gameDotGameTypeZero + "\");");
                        wl.Add(Indent(6) + "break;");
                    }
                    else
                    {
                        wl.Add(Indent(6) + "// Don't handle " + gameDotGameTypeZero + " because we don't want to write out defaults");
                    }
                    wl.Add(Indent(4) + "}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    if (Fields.WriteEmptyValues)
                    {
                        swlSBAppend(4, fieldIniName, objDotField, unixDateString);
                    }
                    else
                    {
                        wl.Add(Indent(4) + "if (!string.IsNullOrEmpty(" + objDotField + ".UnixDateString))");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, objDotField, unixDateString);
                        wl.Add(Indent(4) + "}");
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
                        wl.Add(Indent(4) + "if (" + objDotField + " != null)");
                        wl.Add(Indent(4) + "{");
                        swlSBAppend(5, fieldIniName, val);
                        wl.Add(Indent(4) + "}");
                    }
                }
                else if (field.Type == "CustomResources")
                {
                    wl.Add("#if write_old_resources_style");
                    wl.Add(Indent(4) + "if (fm.ResourcesScanned)");
                    wl.Add(Indent(4) + "{");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasMap=\" + FMHasResource(fm, CustomResources.Map).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasAutomap=\" + FMHasResource(fm, CustomResources.Automap).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasScripts=\" + FMHasResource(fm, CustomResources.Scripts).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasTextures=\" + FMHasResource(fm, CustomResources.Textures).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasSounds=\" + FMHasResource(fm, CustomResources.Sounds).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasObjects=\" + FMHasResource(fm, CustomResources.Objects).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasCreatures=\" + FMHasResource(fm, CustomResources.Creatures).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasMotions=\" + FMHasResource(fm, CustomResources.Motions).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasMovies=\" + FMHasResource(fm, CustomResources.Movies).ToString());");
                    wl.Add(Indent(4) + "sb.AppendLine(\"HasSubtitles=\" + FMHasResource(fm, CustomResources.Subtitles).ToString());");
                    wl.Add(Indent(4) + "}");
                    wl.Add("#else");
                    wl.Add(Indent(4) + "sb.Append(\"" + fieldIniName + "=\");");
                    wl.Add(Indent(4) + "if (fm.ResourcesScanned)");
                    wl.Add(Indent(4) + "{");
                    wl.Add(Indent(5) + "CommaCombineHasXFields(fm, sb);");
                    wl.Add(Indent(4) + "}");
                    wl.Add(Indent(4) + "else");
                    wl.Add(Indent(4) + "{");
                    wl.Add(Indent(5) + "sb.AppendLine(\"NotScanned\");");
                    wl.Add(Indent(4) + "}");
                    wl.Add("#endif");
                }
            }

            foreach (string line in WriterKeepLines) wl.Add(line);

            // for
            wl.Add(Indent(3) + "}");

            wl.Add("");
            wl.Add(Indent(3) + "using var sw = new StreamWriter(fileName, false, Encoding.UTF8);");
            wl.Add(Indent(3) + "sw.Write(sb.ToString());");

            // method WriteFMDataIni
            wl.Add(Indent(2) + "}");
        }

        private static void ReadSourceFields(string className, string sourceFile)
        {
            string[] sourceLines = File.ReadAllLines(sourceFile);

            bool inClass = false;

            bool doNotSerializeNextLine = false;
            string iniNameForThisField = "";
            string listItemPrefixForThisField = "";
            var listTypeForThisField = ListType.MultipleLines;
            var listDistinctTypeForThisField = ListDistinctType.None;
            var enumMapForThisField = new Dictionary<string, string>();
            var enumValuesForThisField = new List<string>();
            long? numericEmptyForThisField = null;
            bool doNotTrimValueForThisField = false;
            bool doNotConvertDateTimeToLocalForThisField = false;
            string commentForThisField = "";
            var codeBlockToInsertAfterThisField = CustomCodeBlockNames.None;

            static string GetAttrParam(string value)
            {
                int index1;
                int indexOfParen = value.IndexOf('(');
                string ret = value
                    .Substring(index1 = indexOfParen + 1, value.LastIndexOf(')') - index1)
                    .Trim();

                int retLength = ret.Length;
                if (retLength >= 2 && ret[0] == '\"' && ret[retLength - 1] == '\"')
                {
                    ret = ret == "\"\"" ? "" : ret.Substring(1, ret.Length - 2);
                }
                return ret;
            }

            for (int i = 0; i < sourceLines.Length; i++)
            {
                string lineT = sourceLines[i].Trim();

                if (!inClass)
                {
                    bool lineIsClassDef = lineT.EndsWith("class " + className);

                    if (i > 0 && lineIsClassDef)
                    {
                        string prevLineT = sourceLines[i - 1].Trim();

                        if (prevLineT.StartsWith("[") && prevLineT.EndsWith("]"))
                        {
                            var indexOfParen = prevLineT.IndexOf('(');
                            if (indexOfParen > -1)
                            {
                                string attr = prevLineT.Trim('[', ']');
                                string attrNamePart = attr.Substring(0, indexOfParen);
                                if (GetAttributeName(attrNamePart, GenAttributes.FenGenWriteEmptyValues))
                                {
                                    string attrParam = GetAttrParam(attr);
                                    Fields.WriteEmptyValues = attrParam.EqualsTrue();
                                }
                            }
                        }
                    }

                    if (lineIsClassDef) inClass = true;
                    continue;
                }

                if (doNotSerializeNextLine)
                {
                    if (!string.IsNullOrWhiteSpace(lineT)) doNotSerializeNextLine = false;
                    continue;
                }

                if (lineT == "}") break;

                if (lineT.StartsWith("[") && lineT.EndsWith("]"))
                {
                    string attr = lineT.Trim('[', ']');
                    int indexOfParen = attr.IndexOf('(');

                    if (GetAttributeName(attr, GenAttributes.FenGenIgnore))
                    {
                        doNotSerializeNextLine = true;
                        continue;
                    }
                    else if (GetAttributeName(attr, GenAttributes.FenGenDoNotConvertDateTimeToLocal))
                    {
                        doNotConvertDateTimeToLocalForThisField = true;
                        continue;
                    }
                    else if (GetAttributeName(attr, GenAttributes.FenGenDoNotTrimValue))
                    {
                        doNotTrimValueForThisField = true;
                        continue;
                    }
                    else if (indexOfParen > -1)
                    {
                        string attrNamePart = attr.Substring(0, indexOfParen);
                        string attrParam = GetAttrParam(attr);
                        if (GetAttributeName(attrNamePart, GenAttributes.FenGenNumericEmpty))
                        {
                            if (long.TryParse(attrParam, out long result))
                            {
                                numericEmptyForThisField = result;
                            }
                            continue;
                        }
                        else if (GetAttributeName(attrNamePart, GenAttributes.FenGenListType))
                        {
                            listTypeForThisField = attrParam == nameof(ListType.CommaSeparated)
                                ? ListType.CommaSeparated
                                : ListType.MultipleLines;
                            continue;
                        }
                        else if (GetAttributeName(attrNamePart, GenAttributes.FenGenIniName))
                        {
                            iniNameForThisField = attrParam;
                            continue;
                        }
                        else if (GetAttributeName(attrNamePart, GenAttributes.FenGenInsertAfter))
                        {
                            codeBlockToInsertAfterThisField =
                                attrParam == nameof(CustomCodeBlockNames.LegacyCustomResources)
                                    ? CustomCodeBlockNames.LegacyCustomResources
                                    : CustomCodeBlockNames.None;
                        }
                    }
                }

                // TODO: Aim is to remove this "comment attribute" section and have all of them be real attributes
                if (lineT.StartsWith("//"))
                {
                    string attr = lineT.Substring(2).Trim();
                    if (attr.StartsWith("[FenGen:"))
                    {
                        if (attr.Substring(attr.IndexOf(':') + 1).StartsWith("Comment="))
                        {
                            string val = attr.Substring(attr.IndexOf('=') + 1).TrimEnd(']');
                            commentForThisField = !val.IsWhiteSpace() ? val : "";
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

                        string kvp = attr.Substring(ind = attr.IndexOf(':') + 1, attr.Length - ind - 1);
                        if (kvp.Contains("="))
                        {
                            string key = kvp.Substring(0, kvp.IndexOf('=')).Trim();
                            string val = kvp.Substring(kvp.IndexOf('=') + 1).Trim();

                            switch (key)
                            {
                                case "ListItemPrefix":
                                    listItemPrefixForThisField = val;
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

                int index = lineT.IndexOf('{');
                if (index == -1) index = lineT.IndexOf('=');
                if (index == -1) index = lineT.IndexOf(';');
                if (index == -1) continue;

                string line2 = lineT.Substring(0, index).Trim();

                index = line2.LastIndexOf(' ');
                if (index == -1) continue;

                Fields.Add(new Field());
                var last = Fields[Fields.Count - 1];

                if (!iniNameForThisField.IsEmpty())
                {
                    last.IniName = iniNameForThisField;
                    iniNameForThisField = "";
                }
                if (!listItemPrefixForThisField.IsEmpty())
                {
                    last.ListItemPrefix = listItemPrefixForThisField;
                    listItemPrefixForThisField = "";
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
                    commentForThisField = "";
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
