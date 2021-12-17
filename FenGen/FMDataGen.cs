using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class FMData
    {
        private const BindingFlags _bFlagsEnum = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private sealed class Field
        {
            internal string Type = "";
            internal string Name = "";
            internal string IniName = "";
            internal ListType ListType = ListType.MultipleLines;
            internal ListDistinctType ListDistinctType = ListDistinctType.None;
            internal long? NumericEmpty;
            internal bool DoNotTrimValue;
            internal bool DoNotConvertDateTimeToLocal;
            internal bool DoNotWrite;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class FieldList : List<Field>
        {
            internal bool WriteEmptyValues;
            //internal string Version;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private enum ListType
        {
            MultipleLines,
            CommaSeparated
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private enum ListDistinctType
        {
            None,
            Exact,
            CaseInsensitive
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private enum CustomCodeBlockNames
        {
            None,
            LegacyCustomResources,
            OldDisableAllMods
        }

        private static readonly HashSet<string> _numericTypes = new()
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

        private const string _oldResourceFormatMessage = "Old resource format - backward compatibility, we still have to be able to read it";

        private static class CustomCodeBlocks
        {
            internal static readonly string[] LegacyCustomResourceReads =
            {
                "#region " + _oldResourceFormatMessage,
                "",
                "private static void FMData_HasMap_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Map, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasAutomap_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Automap, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasScripts_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Scripts, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasTextures_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Textures, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasSounds_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Sounds, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasObjects_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Objects, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasCreatures_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Creatures, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasMotions_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Motions, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasMovies_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Movies, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "private static void FMData_HasSubtitles_Set(FanMission fm, string valTrimmed, string valRaw)",
                "{",
                "    SetFMResource(fm, CustomResources.Subtitles, valTrimmed.EqualsTrue());",
                "    fm.ResourcesScanned = true;",
                "}",
                "",
                "#endregion"
            };
        }

        private static readonly string[] _writeFMDataIniTopLines =
        {
            "private static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)",
            "{",
            "    // Averaged over the 1573 FMs in my FMData.ini file (in new HasResources format)",
            "    const int averageFMEntryCharCount = 378;",
            "    var sb = new StringBuilder(averageFMEntryCharCount * fmDataList.Count);",
            "",
            "    foreach (FanMission fm in fmDataList)",
            "    {",
            "        sb.AppendLine(\"[FM]\");",
            ""
        };

        internal static void Generate(string sourceFile, string destFile)
        {
            FieldList fields = ReadSourceFields(sourceFile);

            string codeBlock = GetCodeBlock(destFile, GenAttributes.FenGenFMDataDestClass);

            var w = new CodeWriters.IndentingWriter(startingIndent: 1);

            w.AppendRawString(codeBlock);
            w.WL("{");

            const string obj = "fm";

            WriteReader(w, obj, fields);

            w.WL();

            WriteWriter(w, obj, fields);

            // class
            w.WL("}");

            // namespace
            w.WL("}");

            File.WriteAllText(destFile, w.ToString());
        }

        [MustUseReturnValue]
        private static FieldList ReadSourceFields(string sourceFile)
        {
            #region Local functions

            static void CheckParamCount(AttributeSyntax attr, int count)
            {
                if (attr.ArgumentList == null || attr.ArgumentList.Arguments.Count != count)
                {
                    ThrowErrorAndTerminate(attr.Name + " has wrong number of parameters.");
                }
            }

            static void FillFieldFromAttributes(MemberDeclarationSyntax member, Field field, out bool ignore)
            {
                static string GetStringValue(AttributeSyntax attr) =>
                    ((LiteralExpressionSyntax)attr.ArgumentList!.Arguments[0].Expression)
                    .Token
                    .Value!.ToString();

                ignore = false;

                foreach (AttributeListSyntax attrList in member.AttributeLists)
                {
                    foreach (AttributeSyntax attr in attrList.Attributes)
                    {
                        string name = attr.Name.ToString();
                        switch (name)
                        {
                            case GenAttributes.FenGenIgnore:
                                ignore = true;
                                return;
                            case GenAttributes.FenGenDoNotWrite:
                                field.DoNotWrite = true;
                                break;
                            case GenAttributes.FenGenDoNotConvertDateTimeToLocal:
                                field.DoNotConvertDateTimeToLocal = true;
                                break;
                            case GenAttributes.FenGenDoNotTrimValue:
                                field.DoNotTrimValue = true;
                                break;
                            case GenAttributes.FenGenNumericEmpty:
                                {
                                    CheckParamCount(attr, 1);

                                    // Have to do this ridiculous method of getting the value, because if the value is
                                    // negative, we end up getting a PrefixUnaryExpressionSyntax rather than the entire
                                    // number. But ToString() gives us the string version of the entire number. Argh...
                                    string val = attr.ArgumentList!.Arguments[0].Expression.ToString();
                                    long.TryParse(val, out long result);
                                    field.NumericEmpty = result;
                                    break;
                                }
                            case GenAttributes.FenGenListType:
                                {
                                    CheckParamCount(attr, 1);

                                    string val = GetStringValue(attr);

                                    FieldInfo? enumField = typeof(ListType).GetField(val, _bFlagsEnum);
                                    if (enumField != null) field.ListType = (ListType)enumField.GetValue(null);
                                    break;
                                }
                            case GenAttributes.FenGenListDistinctType:
                                {
                                    CheckParamCount(attr, 1);

                                    string val = GetStringValue(attr);

                                    FieldInfo? enumField = typeof(ListDistinctType).GetField(val, _bFlagsEnum);
                                    if (enumField != null) field.ListDistinctType = (ListDistinctType)enumField.GetValue(null);
                                    break;
                                }
                            case GenAttributes.FenGenIniName:
                                CheckParamCount(attr, 1);

                                field.IniName = GetStringValue(attr);
                                break;
                        }
                    }
                }
            }

            #endregion

            string code = File.ReadAllText(sourceFile);
            SyntaxTree tree = ParseTextFast(code);

            var (member, classAttr) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenFMDataSourceClass);
            var fmDataClass = (ClassDeclarationSyntax)member;

            CheckParamCount(classAttr, 1);

            var fields = new FieldList
            {
                WriteEmptyValues =
                    (bool)((LiteralExpressionSyntax)classAttr.ArgumentList!.Arguments[0].Expression)
                    .Token
                    .Value!
            };

            foreach (SyntaxNode item in fmDataClass.ChildNodes())
            {
                if (item.IsKind(SyntaxKind.FieldDeclaration) || item.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    Field last = new Field();
                    FillFieldFromAttributes((MemberDeclarationSyntax)item, last, out bool ignore);
                    if (ignore) continue;

                    last.Name = (item.IsKind(SyntaxKind.FieldDeclaration)
                        ? ((FieldDeclarationSyntax)item).Declaration.Variables[0].Identifier
                        : ((PropertyDeclarationSyntax)item).Identifier).Value!.ToString();
                    last.Type = (item.IsKind(SyntaxKind.FieldDeclaration)
                        ? ((FieldDeclarationSyntax)item).Declaration.Type
                        : ((PropertyDeclarationSyntax)item).Type).ToString();

                    fields.Add(last);
                }
            }

            return fields;
        }

        private static void WriteReader(CodeWriters.IndentingWriter w, string obj, FieldList fields)
        {
            static string GetFloatArgsRead(string fieldType) =>
                fieldType
                    is "float"
                    or "float?"
                    or "double"
                    or "double?"
                    or "decimal"
                    or "decimal?"
                    ? "NumberStyles.Float, NumberFormatInfo.InvariantInfo, "
                    : "";

            w.WL("#region Generated code for reader");
            w.WL();
            w.WL("// This nonsense is to allow for keys to be looked up in a dictionary rather than running ten thousand");
            w.WL("// if statements on every line.");
            w.WL();

            for (int i = 0; i < fields.Count; i++)
            {
                Field field = fields[i];
                string objDotField = obj + "." + field.Name;

                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                string valVar = field.DoNotTrimValue ? "valRaw" : "valTrimmed";

                w.WL("private static void FMData_" + fieldIniName + "_Set(FanMission fm, string valTrimmed, string valRaw)");
                w.WL("{");

                if (field.Type.StartsWith("List<"))
                {
                    string listType = field.Type.Substring(field.Type.IndexOf('<')).TrimStart('<').TrimEnd('>');

                    var ldt = field.ListDistinctType;

                    string varToAdd = listType == "string" && field.ListType == ListType.MultipleLines
                        ? "valTrimmed"
                        : "result";

                    string ignoreCaseString = ldt == ListDistinctType.CaseInsensitive && listType == "string"
                        ? ", StringComparer.OrdinalIgnoreCase"
                        : "";

                    string objListSet = "";
                    if (ldt is ListDistinctType.Exact or ListDistinctType.CaseInsensitive)
                    {
                        objListSet = "if (!" + objDotField + ".Contains(" + varToAdd + ignoreCaseString + ")) ";
                    }
                    objListSet += objDotField + ".Add(" + varToAdd + ");";

                    if (listType == "string")
                    {
                        if (field.ListType == ListType.MultipleLines)
                        {
                            w.WL("if (!string.IsNullOrEmpty(valTrimmed))");
                            w.WL("{");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = valTrimmed.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);");
                            w.WL("for (int a = 0; a < items.Length; a++)");
                            w.WL("{");
                            w.WL("string result = items[a].Trim();");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                    }
                    else if (_numericTypes.Contains(listType))
                    {
                        string floatArgs = GetFloatArgsRead(listType);
                        if (field.ListType == ListType.MultipleLines)
                        {
                            w.WL("if (" + objDotField + " == null)");
                            w.WL("{");
                            w.WL(objDotField + " = new List<" + listType + ">();");
                            w.WL("}");
                            w.WL("bool success = " + listType + ".TryParse(valTrimmed, " + floatArgs + "out " + listType + " result);");
                            w.WL("if(success)");
                            w.WL("{");
                            w.WL(objListSet + "");
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = valTrimmed.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);");
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
                    w.WL(objDotField + " = " + valVar + ";");
                }
                else if (field.Type == "bool")
                {
                    w.WL(objDotField + " = valTrimmed.EqualsTrue();");
                }
                else if (field.Type == "bool?")
                {
                    w.WL(objDotField + " =");
                    w.WL("!string.IsNullOrEmpty(valTrimmed) ? valTrimmed.EqualsTrue() : (bool?)null;");
                }
                else if (_numericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    if (field.NumericEmpty != null && field.NumericEmpty != 0)
                    {
                        w.WL("bool success = " + field.Type + ".TryParse(valTrimmed, " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = success ? result : " + field.NumericEmpty + ";");
                    }
                    else
                    {
                        w.WL(field.Type + ".TryParse(valTrimmed, " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = result;");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         _numericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    w.WL("bool success = " + ftNonNull + ".TryParse(valTrimmed, " + floatArgs + "out " + ftNonNull + " result);");
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
                    var gamesEnum = Cache.GamesEnum;

                    for (int gi = 1; gi < gamesEnum.GameEnumNames.Count; gi++)
                    {
                        string ifType = gi > 1 ? "else if" : "if";
                        string gameDotGameType = gamesEnum.Name + "." + gamesEnum.GameEnumNames[gi];
                        w.WL(ifType + " (valTrimmed.EqualsI(\"" + gamesEnum.GameEnumNames[gi] + "\"))");
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
                    w.WL(objDotField + ".UnixDateString = valTrimmed;");
                }
                else if (field.Type == "DateTime?")
                {
                    w.WL("// PERF: Don't convert to local here; do it at display-time");
                    w.WL(objDotField + " = ConvertHexUnixDateToDateTime(valTrimmed, convertToLocal: " +
                         (!field.DoNotConvertDateTimeToLocal).ToString().ToLowerInvariant() + ");");
                }
                else if (field.Type == "CustomResources")
                {
                    // Totally shouldn't be hardcoded...
                    w.WL(obj + ".ResourcesScanned = !valTrimmed.EqualsI(\"NotScanned\");");
                    w.WL("FillFMHasXFields(fm, valTrimmed);");
                }
                else if (field.Type == "DisableModsSwitches")
                {
                    w.WL("FillDisableModsSwitches(fm, valTrimmed);");
                }

                w.WL("}"); // end of setter method
                w.WL();
            }

            w.WLs(CustomCodeBlocks.LegacyCustomResourceReads);
            w.WL();

            var dictFields = fields.ToList();
            dictFields.Add(new Field { Name = "HasMap" });
            dictFields.Add(new Field { Name = "HasAutomap" });
            dictFields.Add(new Field { Name = "HasScripts" });
            dictFields.Add(new Field { Name = "HasTextures" });
            dictFields.Add(new Field { Name = "HasSounds" });
            dictFields.Add(new Field { Name = "HasObjects" });
            dictFields.Add(new Field { Name = "HasCreatures" });
            dictFields.Add(new Field { Name = "HasMotions" });
            dictFields.Add(new Field { Name = "HasMovies" });
            dictFields.Add(new Field { Name = "HasSubtitles" });

            w.WL("private static readonly Dictionary<string, Action<FanMission, string, string>> _actionDict_FMData = new()");
            w.WL("{");
            for (int i = 0; i < dictFields.Count; i++)
            {
                Field field = dictFields[i];
                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;
                string comma = i == dictFields.Count - 1 ? "" : ",";
                w.WL("{ \"" + fieldIniName + "\", FMData_" + fieldIniName + "_Set }" + comma);

                if (i < dictFields.Count - 1 && dictFields[i + 1].Name == "HasMap")
                {
                    w.WL();
                    w.WL("#region " + _oldResourceFormatMessage);
                    w.WL();
                }
                else if (i == dictFields.Count - 1)
                {
                    w.WL();
                    w.WL("#endregion");
                }
            }
            w.WL("};");
            w.WL();
            w.WL("#endregion");
        }

        private static void WriteWriter(CodeWriters.IndentingWriter w, string obj, FieldList fields)
        {
            w.WL("#region Generated code for writer");
            w.WL();

            w.WLs(_writeFMDataIniTopLines);

            const string toString = "ToString()";
            const string unixDateString = "UnixDateString";

            static string GetFloatArgsWrite(string fieldType) =>
                fieldType
                    is "float"
                    or "float?"
                    or "double"
                    or "double?"
                    or "decimal"
                    or "decimal?"
                    ? "NumberFormatInfo.InvariantInfo"
                    : "";

            foreach (Field field in fields)
            {
                string objDotField = obj + "." + field.Name;
                string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;

                if (field.DoNotWrite) continue;

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
                    else
                    {
                        // WriteEmptyValues check disabled here as well to match the above
                        swlSBAppend(fieldIniName, "CommaCombine(" + objDotField + ")");
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
                else if (_numericTypes.Contains(field.Type))
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
                         _numericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
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
                        ? "new DateTimeOffset((DateTime)" + objDotField + ").ToUnixTimeSeconds().ToString(\"X\")"
                        : "new DateTimeOffset(((DateTime)" + objDotField + ").ToLocalTime()).ToUnixTimeSeconds().ToString(\"X\")";

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
                else if (field.Type == "DisableModsSwitches")
                {
                    if (fields.WriteEmptyValues)
                    {
                        w.WL("sb.Append(\"" + fieldIniName + "=\");");
                        w.WL("CommaCombineDisableModsSwitches(fm, sb);");
                    }
                    else
                    {
                        w.WL("if(" + objDotField + " != DisableModsSwitches.None)");
                        w.WL("{");
                        w.WL("sb.Append(\"" + fieldIniName + "=\");");
                        w.WL("CommaCombineDisableModsSwitches(fm, sb);");
                        w.WL("}");
                    }
                }
            }

            // for
            w.WL("}");

            w.WL();
            w.WL("using var sw = new StreamWriter(fileName, false, Encoding.UTF8);");
            w.WL("sw.Write(sb.ToString());");

            // method WriteFMDataIni
            w.WL("}");
            w.WL();
            w.WL("#endregion");
        }
    }
}
