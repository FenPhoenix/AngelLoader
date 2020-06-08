﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            internal long? NumericEmpty;
            internal bool DoNotTrimValue;
            internal bool DoNotConvertDateTimeToLocal;
            internal string Comment = "";
            internal CustomCodeBlockNames CodeBlockToInsertAfter = CustomCodeBlockNames.None;

            internal Field Copy()
            {
                Field dest = new Field();

                // Meh... convenience wins
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                foreach (FieldInfo f in GetType().GetFields(flags))
                {
                    dest.GetType().GetField(f.Name, flags)!.SetValue(dest, f.GetValue(this));
                }

                return dest;
            }
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

        internal static void Generate(string sourceFile, string destFile)
        {
            var fields = ReadSourceFields(sourceFile);

            // Always do an atomic read operation, because we may want to open the same file for other purposes
            // in the middle of it (we had an access exception before)
            var lines = File.ReadAllLines(destFile);

            var destTopLines = new List<string>();

            int openBraces = 0;
            foreach (string line in lines)
            {
                string lineT = line.Trim();

                if (lineT.StartsWith("{")) openBraces++;
                destTopLines.Add(line);
                if (openBraces == 2)
                {
                    break;
                }
            }

            //var writeLines = new List<string>();
            var sb = new StringBuilder();

            foreach (string l in destTopLines) sb.AppendLine(l);

            //string obj = GenType == GenType.FMData ? "fm" : "config";
            const string obj = "fm";

            WriteReader(sb, obj, fields);

            sb.AppendLine("");

            WriteWriter(sb, obj, fields);

            // class
            sb.AppendLine(Indent(1) + "}");

            // namespace
            sb.AppendLine("}");

            File.WriteAllText(destFile, sb.ToString());
        }

        private static void WriteReader(StringBuilder sb, string obj, FieldList fields)
        {
            if (TopCodeLines.Length > 0)
            {
                sb.AppendLine(Indent(2) + GenMessages.SupportingCode);
                foreach (string l in TopCodeLines) sb.AppendLine(l);
            }

            sb.AppendLine(Indent(2) + GenMessages.Method);

            //var topLines = (GenType == GenType.FMData
            //    ? ReadFMDataIniTopLines
            //    : ReadConfigIniTopLines).ToList();

            string[] topLines = ReadFMDataIniTopLines;

            foreach (string l in topLines) sb.AppendLine(l);

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

            var w = new Generators.IndentingWriter(sb, startingIndent: 4);

            for (int i = 0; i < fields.Count; i++)
            {
                if (customCodeBlockToInsertAfterField != CustomCodeBlockNames.None)
                {
                    switch (customCodeBlockToInsertAfterField)
                    {
                        case CustomCodeBlockNames.LegacyCustomResources:
                            foreach (string line in CustomCodeBlocks.LegacyCustomResourceReads)
                            {
                                sb.AppendLine(line);
                            }
                            break;
                    }
                }

                var field = fields[i];
                string objDotField = obj + "." + field.Name;

                string optElse = i > 0 ? "else " : "";

                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                string lineTrimmedVersion = field.DoNotTrimValue ? "lineTS" : "lineT";

                w.WL(optElse + "if (lineT.StartsWithFast_NoNullChecks(\"" + fieldIniName + "=\"))");
                w.WL("{");
                w.WL("string val = " + lineTrimmedVersion + ".Substring(" + (fieldIniName.Length + 1) + ");");

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
                            w.WL("if (!string.IsNullOrEmpty(val))");
                            w.WL("{");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);");
                            w.WL("for (int a = 0; a < items.Length; a++)");
                            w.WL("{");
                            w.WL("string result = items[a].Trim();");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                    }
                    else if (NumericTypes.Contains(listType))
                    {
                        string floatArgs = GetFloatArgsRead(listType);
                        if (field.ListType == ListType.MultipleLines)
                        {
                            w.WL("if (" + objDotField + " == null)");
                            w.WL("{");
                            w.WL(objDotField + " = new List<" + listType + ">();");
                            w.WL("}");
                            w.WL("bool success = " + listType + ".TryParse(val, " + floatArgs + "out " + listType + " result);");
                            w.WL("if(success)");
                            w.WL("{");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);");
                            w.WL("for (int a = 0; a < items.Length; a++)");
                            w.WL("{");
                            w.WL("items[a] = items[a].Trim();");
                            w.WL("bool success = " + listType + ".TryParse(items[a], " + floatArgs + "out " + listType + " result);");
                            w.WL("if(success)");
                            w.WL("{");
                            w.WL(objListSet + "");
                            w.WL("}");
                            w.WL("}");
                        }
                    }
                }
                else if (field.Type == "string")
                {
                    w.WL(objDotField + " = val;");
                }
                else if (field.Type == "bool")
                {
                    w.WL(objDotField + " = val.EqualsTrue();");
                }
                else if (field.Type == "bool?")
                {
                    w.WL(objDotField + " =");
                    w.WL("!string.IsNullOrEmpty(val) ? val.EqualsTrue() : (bool?)null;");
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    if (field.NumericEmpty != null && field.NumericEmpty != 0)
                    {
                        w.WL("bool success = " + field.Type + ".TryParse(val, " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = success ? result : " + field.NumericEmpty + ";");
                    }
                    else
                    {
                        w.WL(field.Type + ".TryParse(val, " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = result;");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                        NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    w.WL("bool success = " + ftNonNull + ".TryParse(val, " + floatArgs + "out " + ftNonNull + " result);");
                    w.WL("if (success)");
                    w.WL("{");
                    w.WL(objDotField + " = result;");
                    w.WL("}");
                    w.WL("else");
                    w.WL("{");
                    w.WL(objDotField + " = null;");
                    w.WL("}");
                }
                else if (field.Type == Cache.GamesEnum.Name)
                {
                    w.WL("val = val.Trim();");

                    var gamesEnum = Cache.GamesEnum;

                    for (int gi = 1; gi < gamesEnum.GameEnumNames.Count; gi++)
                    {
                        string ifType = gi > 1 ? "else if" : "if";
                        string gameDotGameType = gamesEnum.Name + "." + gamesEnum.GameEnumNames[gi];
                        w.WL(ifType + " (val.EqualsI(\"" + gamesEnum.GameEnumNames[gi] + "\"))");
                        w.WL("{");
                        w.WL(objDotField + " = " + gameDotGameType + ";");
                        w.WL("}");
                    }
                    w.WL("else");
                    w.WL("{");
                    w.WL(objDotField + " = " + gamesEnum.Name + "." + gamesEnum.GameEnumNames[0] + ";");
                    w.WL("}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    w.WL(objDotField + ".UnixDateString = val;");
                }
                else if (field.Type == "DateTime?")
                {
                    w.WL("// PERF: Don't convert to local here; do it at display-time");
                    w.WL(objDotField + " = ConvertHexUnixDateToDateTime(val, convertToLocal: " +
                                  (!field.DoNotConvertDateTimeToLocal).ToString().ToLowerInvariant() + ");");
                }
                else if (field.Type == "CustomResources")
                {
                    // Totally shouldn't be hardcoded...
                    w.WL(obj + ".ResourcesScanned = !val.EqualsI(\"NotScanned\");");
                    w.WL("FillFMHasXFields(fm, val);");
                }

                // if
                w.WL("}");

                customCodeBlockToInsertAfterField = field.CodeBlockToInsertAfter;
            }

            w.WL("if (resourcesFound) fm.ResourcesScanned = true;");

            // for
            w.WL("}");

            // method ReadFMDataIni
            w.WL("}");
        }

        private static void WriteWriter(StringBuilder sb, string obj, FieldList fields)
        {
            sb.AppendLine(Indent(2) + GenMessages.Method);

            foreach (string l in WriteFMDataIniTopLines) sb.AppendLine(l);

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

            var w = new Generators.IndentingWriter(sb, startingIndent: 4);

            foreach (var field in fields)
            {
                string objDotField = obj + "." + field.Name;
                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                void swlSBAppend(string objField, string value, string suffix = "")
                {
                    if (!suffix.IsEmpty()) suffix = "." + suffix;
                    w.WL("sb.Append(\"" + objField + "=\");");
                    if (fieldIniName == "DateAdded")
                    {
                        // Disgusting >:(
                        w.WL("// Again, important to convert to local time here because we don't do it on startup.");
                    }
                    w.WL("sb.AppendLine(" + value + suffix + ");");
                }

                if (field.Type.StartsWith("List<"))
                {
                    bool listTypeIsString = field.Type == "List<string>";
                    if (field.ListType == ListType.MultipleLines)
                    {
                        string foreachType = listTypeIsString ? "string" : "var";
                        w.WL("foreach (" + foreachType + " s in " + objDotField + ")");
                        w.WL("{");

                        //if (Fields.WriteEmptyValues)
#if true
                        {
                            swlSBAppend(fieldIniName, "s", !listTypeIsString ? toString : "");
                        }
                        // Disabled for now, for AngelLoader-specific perf
#else
                        {
                            w.WL("if (!string.IsNullOrEmpty(s))");
                            w.WL("{");
                            w.WL("sb.AppendLine(\"" + fieldIniName + "=\" + s);");
                            w.WL("}");
                        }
#endif

                        w.WL("}");
                    }
                }
                else if (field.Type == "string")
                {
                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, objDotField);
                    }
                    else
                    {
                        w.WL("if (!string.IsNullOrEmpty(" + objDotField + "))");
                        w.WL("{");
                        swlSBAppend(fieldIniName, objDotField);
                        w.WL("}");
                    }
                }
                else if (field.Type == "bool")
                {
                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, objDotField, toString);
                    }
                    else
                    {
                        w.WL("if (" + objDotField + ")");
                        w.WL("{");
                        swlSBAppend(fieldIniName, objDotField, toString);
                        w.WL("}");
                    }
                }
                else if (field.Type == "bool?")
                {
                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, objDotField, toString);
                    }
                    else
                    {
                        w.WL("if (" + objDotField + " != null)");
                        w.WL("{");
                        swlSBAppend(fieldIniName, obj, toString);
                        w.WL("}");
                    }
                }
                else if (NumericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsWrite(field.Type);
                    if (!fields.WriteEmptyValues && field.NumericEmpty != null)
                    {
                        w.WL("if (" + objDotField + " != " + field.NumericEmpty + ")");
                        w.WL("{");
                        swlSBAppend(fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                        w.WL("}");
                    }
                    else
                    {
                        swlSBAppend(fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         NumericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsWrite(field.Type);
                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                    }
                    else
                    {
                        w.WL("if (" + objDotField + " != null)");
                        w.WL("{");
                        swlSBAppend(fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                        w.WL("}");
                    }
                }
                else if (field.Type == Cache.GamesEnum.Name)
                {
                    var gamesEnum = Cache.GamesEnum;

                    w.WL("switch (fm." + gamesEnum.Name + ")");
                    w.WL("{");
                    for (int gi = 1; gi < gamesEnum.GameEnumNames.Count; gi++)
                    {
                        if (gi == 1) w.WL("// Much faster to do this than Enum.ToString()");
                        w.WL("case " + gamesEnum.Name + "." + gamesEnum.GameEnumNames[gi] + ":");
                        w.WL("sb.AppendLine(\"" + gamesEnum.Name + "=" + gamesEnum.GameEnumNames[gi] + "\");");
                        w.WL("break;");
                    }
                    string gameDotGameTypeZero = gamesEnum.Name + "." + gamesEnum.GameEnumNames[0];
                    if (fields.WriteEmptyValues)
                    {
                        w.WL("case " + gameDotGameTypeZero + ":");
                        w.WL("sb.AppendLine(\"" + gamesEnum.Name + "=" + gameDotGameTypeZero + "\");");
                        w.WL("break;");
                    }
                    else
                    {
                        w.WL("// Don't handle " + gameDotGameTypeZero + " because we don't want to write out defaults");
                    }
                    w.WL("}");
                }
                else if (field.Type == "ExpandableDate")
                {
                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, objDotField, unixDateString);
                    }
                    else
                    {
                        w.WL("if (!string.IsNullOrEmpty(" + objDotField + ".UnixDateString))");
                        w.WL("{");
                        swlSBAppend(fieldIniName, objDotField, unixDateString);
                        w.WL("}");
                    }
                }
                else if (field.Type == "DateTime?")
                {
                    // If we DIDN'T convert before, we need to convert now
                    string val = !field.DoNotConvertDateTimeToLocal
                        ? "new DateTimeOffset((DateTime)" + objDotField + ").ToUnixTimeSeconds().ToString(\"X\")" :
                        "new DateTimeOffset(((DateTime)" + objDotField + ").ToLocalTime()).ToUnixTimeSeconds().ToString(\"X\")";

                    if (fields.WriteEmptyValues)
                    {
                        swlSBAppend(fieldIniName, val);
                    }
                    else
                    {
                        w.WL("if (" + objDotField + " != null)");
                        w.WL("{");
                        swlSBAppend(fieldIniName, val);
                        w.WL("}");
                    }
                }
                else if (field.Type == "CustomResources")
                {
                    w.WL("#if write_old_resources_style");
                    w.WL("if (fm.ResourcesScanned)");
                    w.WL("{");
                    w.WL("sb.AppendLine(\"HasMap=\" + FMHasResource(fm, CustomResources.Map).ToString());");
                    w.WL("sb.AppendLine(\"HasAutomap=\" + FMHasResource(fm, CustomResources.Automap).ToString());");
                    w.WL("sb.AppendLine(\"HasScripts=\" + FMHasResource(fm, CustomResources.Scripts).ToString());");
                    w.WL("sb.AppendLine(\"HasTextures=\" + FMHasResource(fm, CustomResources.Textures).ToString());");
                    w.WL("sb.AppendLine(\"HasSounds=\" + FMHasResource(fm, CustomResources.Sounds).ToString());");
                    w.WL("sb.AppendLine(\"HasObjects=\" + FMHasResource(fm, CustomResources.Objects).ToString());");
                    w.WL("sb.AppendLine(\"HasCreatures=\" + FMHasResource(fm, CustomResources.Creatures).ToString());");
                    w.WL("sb.AppendLine(\"HasMotions=\" + FMHasResource(fm, CustomResources.Motions).ToString());");
                    w.WL("sb.AppendLine(\"HasMovies=\" + FMHasResource(fm, CustomResources.Movies).ToString());");
                    w.WL("sb.AppendLine(\"HasSubtitles=\" + FMHasResource(fm, CustomResources.Subtitles).ToString());");
                    w.WL("}");
                    w.WL("#else");
                    w.WL("sb.Append(\"" + fieldIniName + "=\");");
                    w.WL("if (fm.ResourcesScanned)");
                    w.WL("{");
                    w.WL("CommaCombineHasXFields(fm, sb);");
                    w.WL("}");
                    w.WL("else");
                    w.WL("{");
                    w.WL("sb.AppendLine(\"NotScanned\");");
                    w.WL("}");
                    w.WL("#endif");
                }
            }

            // for
            w.WL("}");

            w.WL();
            w.WL("using var sw = new StreamWriter(fileName, false, Encoding.UTF8);");
            w.WL("sw.Write(sb.ToString());");

            // method WriteFMDataIni
            w.WL("}");
        }

        [MustUseReturnValue]
        private static FieldList ReadSourceFields(string sourceFile)
        {
            var code = File.ReadAllText(sourceFile);
            var tree = ParseTextFast(code);

            var (member, attr) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenFMDataSourceClass);
            var fmDataClass = (ClassDeclarationSyntax)member;
            var fields = new FieldList();
            CheckParamCount(attr, attr.Name.ToString(), 1);
            fields.WriteEmptyValues = (bool)((LiteralExpressionSyntax)attr.ArgumentList.Arguments[0].Expression)
                .Token
                .Value!;

            static void CheckParamCount(AttributeSyntax attr, string name, int count)
            {
                if (attr.ArgumentList == null ||
                    attr.ArgumentList.Arguments.Count != count)
                {
                    ThrowErrorAndTerminate(name + " has wrong number of parameters.");
                }
            }

            static void FillFieldFromAttributes(MemberDeclarationSyntax member, Field field, out bool ignore)
            {
                ignore = false;

                foreach (AttributeListSyntax attrList in member.AttributeLists)
                {
                    foreach (AttributeSyntax attr in attrList.Attributes)
                    {
                        string name = attr.Name.ToString();
                        if (name == GenAttributes.FenGenIgnore)
                        {
                            ignore = true;
                            return;
                        }
                        else if (name == GenAttributes.FenGenDoNotConvertDateTimeToLocal)
                        {
                            field.DoNotConvertDateTimeToLocal = true;
                        }
                        else if (name == GenAttributes.FenGenDoNotTrimValue)
                        {
                            field.DoNotTrimValue = true;
                        }
                        else if (name == GenAttributes.FenGenNumericEmpty)
                        {
                            CheckParamCount(attr, name, 1);

                            // Have to do this ridiculous method of getting the value, because if the value is
                            // negative, we end up getting a PrefixUnaryExpressionSyntax rather than the entire
                            // number. But ToString() gives us the string version of the entire number. Argh...
                            string val = (attr.ArgumentList!.Arguments[0].Expression).ToString();
                            long.TryParse(val, out long result);
                            field.NumericEmpty = result;
                        }
                        else if (name == GenAttributes.FenGenListType)
                        {
                            CheckParamCount(attr, name, 1);

                            string val = ((LiteralExpressionSyntax)attr.ArgumentList!.Arguments[0].Expression)
                                .Token
                                .Value!.ToString();

                            field.ListType = val == nameof(ListType.CommaSeparated)
                                ? ListType.CommaSeparated
                                : ListType.MultipleLines;
                        }
                        else if (name == GenAttributes.FenGenListDistinctType)
                        {
                            CheckParamCount(attr, name, 1);

                            string val = ((LiteralExpressionSyntax)attr.ArgumentList!.Arguments[0].Expression)
                                .Token
                                .Value!.ToString();

                            field.ListDistinctType = val switch
                            {
                                nameof(ListDistinctType.Exact) => ListDistinctType.Exact,
                                nameof(ListDistinctType.CaseInsensitive) => ListDistinctType.CaseInsensitive,
                                _ => ListDistinctType.None
                            };
                        }
                        else if (name == GenAttributes.FenGenIniName)
                        {
                            CheckParamCount(attr, name, 1);

                            field.IniName =
                                ((string)((LiteralExpressionSyntax)attr.ArgumentList!.Arguments[0].Expression)
                                    .Token
                                    .Value!)!;
                        }
                        else if (name == GenAttributes.FenGenInsertAfter)
                        {
                            CheckParamCount(attr, name, 1);

                            string val =
                                ((string)((LiteralExpressionSyntax)attr.ArgumentList!.Arguments[0].Expression)
                                    .Token
                                    .Value!)!;

                            field.CodeBlockToInsertAfter =
                                val == nameof(CustomCodeBlockNames.LegacyCustomResources)
                                    ? CustomCodeBlockNames.LegacyCustomResources
                                    : CustomCodeBlockNames.None;
                        }
                    }
                }
            }

            foreach (SyntaxNode item in fmDataClass.ChildNodes())
            {
                var tempField = new Field();
                if (item.IsKind(SyntaxKind.FieldDeclaration))
                {
                    var itemMember = (MemberDeclarationSyntax)item;

                    FillFieldFromAttributes(itemMember, tempField, out bool ignore);
                    if (ignore) continue;

                    var itemField = (FieldDeclarationSyntax)itemMember;
                    Field last = tempField.Copy();
                    last.Name = itemField.Declaration.Variables[0].Identifier.Value!.ToString();
                    last.Type = itemField.Declaration.Type.ToString();

                    fields.Add(last);
                }
                else if (item.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    var itemMember = (MemberDeclarationSyntax)item;

                    FillFieldFromAttributes(itemMember, tempField, out bool ignore);
                    if (ignore) continue;

                    var itemProp = (PropertyDeclarationSyntax)itemMember;
                    Field last = tempField.Copy();
                    last.Name = itemProp.Identifier.Value!.ToString();
                    last.Type = itemProp.Type.ToString();

                    fields.Add(last);
                }
            }

            return fields;
        }
    }
}
