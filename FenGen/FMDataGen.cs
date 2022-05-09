﻿using System.Collections.Generic;
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
            internal bool IsEnumAndSingleAssignment;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class FieldList : List<Field>
        {
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

        private static bool IsDecimal(string value) =>
            value
                is "float"
                or "float?"
                or "double"
                or "double?"
                or "decimal"
                or "decimal?";

        internal static void Generate(string sourceFile, string destFile)
        {
            FieldList fields = ReadSourceFields(sourceFile);

            var w = GetWriterForClass(destFile, GenAttributes.FenGenFMDataDestClass);

            const string obj = "fm";

            WriteReadSection(w, obj, fields);

            w.WL();

            WriteWriter(w, obj, fields);

            w.CloseClassAndNamespace();

            File.WriteAllText(destFile, w.ToString());
        }

        [MustUseReturnValue]
        private static FieldList ReadSourceFields(string sourceFile)
        {
            #region Local functions

            static void CheckParamCount(AttributeSyntax attr, int count)
            {
                if (count == 0 && (attr.ArgumentList == null || attr.ArgumentList.Arguments.Count == 0))
                {
                    return;
                }

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
                            case GenAttributes.FenGenFlagsSingleAssignment:
                                field.IsEnumAndSingleAssignment = true;
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

            CheckParamCount(classAttr, 0);

            var fields = new FieldList();

            foreach (SyntaxNode item in fmDataClass.ChildNodes())
            {
                if (item.IsKind(SyntaxKind.FieldDeclaration) || item.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    var last = new Field();
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

        private static void WriteReadSection(CodeWriters.IndentingWriter w, string obj, FieldList fields)
        {
            static string GetFloatArgsRead(string fieldType) =>
                IsDecimal(fieldType)
                    ? "NumberStyles.Float, NumberFormatInfo.InvariantInfo, "
                    : "";

            const string val = "val";

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

                w.WL("private static void FMData_" + fieldIniName + "_Set(FanMission " + obj + ", string " + val + ", int eqIndex)");
                w.WL("{");

                if (field.Type != "bool")
                {
                    w.WL(val + " = " + val + ".Substring(eqIndex + 1);");
                }

                if (!field.DoNotTrimValue)
                {
                    w.WL(val + " = " + val + ".Trim();");
                }
                else
                {
                    w.WL("// We require this value to be untrimmed");
                }

                if (field.Type.StartsWith("List<"))
                {
                    string listType = field.Type.Substring(field.Type.IndexOf('<')).TrimStart('<').TrimEnd('>');

                    var ldt = field.ListDistinctType;

                    string varToAdd = listType == "string" && field.ListType == ListType.MultipleLines
                        ? val
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
                            w.WL("if (!string.IsNullOrEmpty(" + val + "))");
                            w.WL("{");
                            w.WL(objListSet);
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = " + val + ".Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);");
                            w.WL("for (int a = 0; a < items.Length; a++)");
                            w.WL("{");
                            w.WL("string result = items[a].Trim();");
                            w.WL(objListSet);
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
                            w.WL("bool success = " + listType + ".TryParse(" + val + ", " + floatArgs + "out " + listType + " result);");
                            w.WL("if(success)");
                            w.WL("{");
                            w.WL(objListSet);
                            w.WL("}");
                        }
                        else
                        {
                            w.WL(objDotField + ".Clear();");
                            w.WL("string[] items = " + val + ".Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);");
                            w.WL("for (int a = 0; a < items.Length; a++)");
                            w.WL("{");
                            w.WL("items[a] = items[a].Trim();");
                            w.WL("bool success = " + listType + ".TryParse(items[a], " + floatArgs + "out " + listType + " result);");
                            w.WL("if(success)");
                            w.WL("{");
                            w.WL(objListSet);
                            w.WL("}");
                            w.WL("}");
                        }
                    }
                }
                else if (field.Type == "string")
                {
                    w.WL(objDotField + " = " + val + ";");
                }
                else if (field.Type == "bool")
                {
                    w.WL(objDotField + " = " + val + ".EndEqualsTrue();");
                }
                else if (field.Type == "bool?")
                {
                    w.WL(objDotField + " = !string.IsNullOrEmpty(" + val + ") ? " + val + ".EqualsTrue() : (bool?)null;");
                }
                else if (_numericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    if (field.NumericEmpty != null && field.NumericEmpty != 0)
                    {
                        w.WL("bool success = " + field.Type + ".TryParse(" + val + ", " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = success ? result : " + field.NumericEmpty + ";");
                    }
                    else
                    {
                        w.WL(field.Type + ".TryParse(" + val + ", " + floatArgs + "out " + field.Type + " result);");
                        w.WL(objDotField + " = result;");
                    }
                }
                else if (field.Type[field.Type.Length - 1] == '?' &&
                         _numericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
                {
                    string floatArgs = GetFloatArgsRead(field.Type);
                    string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                    w.WL("bool success = " + ftNonNull + ".TryParse(" + val + ", " + floatArgs + "out " + ftNonNull + " result);");
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
                        w.WL(ifType + " (" + val + ".EqualsI(\"" + gamesEnum.GameEnumNames[gi] + "\"))");
                        w.WL("{");
                        w.WL(objDotField + " = " + gameDotGameType + ";");
                        w.WL("}");
                    }
                    w.WL("else");
                    w.WL("{");
                    w.WL(objDotField + " = " + gamesEnum.Name + "." + gamesEnum.GameEnumNames[0] + ";");
                    w.WL("}");
                }
                else if (field.Type == Cache.LangsEnum.Name)
                {
                    var le = Cache.LangsEnum;
                    if (field.IsEnumAndSingleAssignment)
                    {
                        w.WL("if (" + le.StringToEnumDictName + ".TryGetValue(" + val + ", out var result))");
                        w.WL("{");
                        w.WL(objDotField + " = result;");
                        w.WL("}");
                    }
                    else
                    {
                        w.WL("SetFMLanguages(" + obj + ", " + val + ");");
                    }
                }
                else if (field.Type == "ExpandableDate")
                {
                    w.WL(objDotField + ".UnixDateString = " + val + ";");
                }
                else if (field.Type == "DateTime?")
                {
                    w.WL("// PERF: Don't convert to local here; do it at display-time");
                    w.WL(objDotField + " = ConvertHexUnixDateToDateTime(" + val + ", convertToLocal: " +
                         (!field.DoNotConvertDateTimeToLocal).ToString().ToLowerInvariant() + ");");
                }
                else if (field.Type == "CustomResources")
                {
                    // Totally shouldn't be hardcoded...
                    w.WL(obj + ".ResourcesScanned = !" + val + ".EqualsI(\"NotScanned\");");
                    w.WL("FillFMHasXFields(" + obj + ", " + val + ");");
                }

                w.WL("}"); // end of setter method
                w.WL();
            }

            string[] customResourceFieldNames =
            {
                "HasMap",
                "HasAutomap",
                "HasScripts",
                "HasTextures",
                "HasSounds",
                "HasObjects",
                "HasCreatures",
                "HasMotions",
                "HasMovies",
                "HasSubtitles"
            };

            w.WL("#region " + _oldResourceFormatMessage);
            w.WL();
            foreach (string item in customResourceFieldNames)
            {
                w.WL("private static void FMData_" + item + "_Set(FanMission " + obj + ", string " + val + ", int eqIndex)");
                w.WL("{");
                w.WL("    SetFMResource(" + obj + ", CustomResources." + item.Substring(3) + ", " + val + ".EndEqualsTrue());");
                w.WL("    " + obj + ".ResourcesScanned = true;");
                w.WL("}");
                w.WL();
            }
            w.WL("#endregion");
            w.WL();

            var dictFields = fields.ToList();
            foreach (string item in customResourceFieldNames)
            {
                dictFields.Add(new Field { Name = item });
            }

            w.WL("// Source dictionary for manual conversion - until we can get better gperf automation going");
            w.WL("#if false");
            w.WL("private static readonly Dictionary<string, Action<FanMission, string>> _actionDict_FMData = new()");
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
            w.WL("#endif");
            w.WL();
            w.WL("#endregion");
        }

        // @LANGS: Carry as much data from the source enum as you can: type (uint) etc.
        private static void WriteWriter(CodeWriters.IndentingWriter w, string obj, FieldList fields)
        {
            static void WriteEnumSingle(
                CodeWriters.IndentingWriter writer,
                string obj,
                string enumName,
                string fieldName,
                string fieldIniName,
                List<string> enumNames,
                bool writeValuesLowercase = false)
            {
                writer.WL("switch (" + obj + "." + fieldName + ")");
                writer.WL("{");
                for (int gi = 1; gi < enumNames.Count; gi++)
                {
                    if (gi == 1) writer.WL("// Much faster to do this than Enum.ToString()");
                    writer.WL("case " + enumName + "." + enumNames[gi] + ":");
                    writer.WL("sb.AppendLine(\"" + fieldIniName + "=" + (writeValuesLowercase ? enumNames[gi].ToLowerInvariant() : enumNames[gi]) + "\");");
                    writer.WL("break;");
                }
                string enumDotEnumTypeZero = enumName + "." + enumNames[0];
                writer.WL("// Don't handle " + enumDotEnumTypeZero + " because we don't want to write out defaults");
                writer.WL("}");
            }

            w.WL("#region Generated code for writer");
            w.WL();

            w.WLs(new[]
            {
                "private static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)",
                "{",
                "    var sb = new StringBuilder();",
                "",
                "    foreach (FanMission " + obj + " in fmDataList)",
                "    {",
                "        sb.AppendLine(\"[FM]\");",
                ""
            });

            const string toString = "ToString()";
            const string unixDateString = "UnixDateString";

            static string GetFloatArgsWrite(string fieldType) =>
                IsDecimal(fieldType)
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

                        swlSBAppend(fieldIniName, "s", !listTypeIsString ? toString : "");
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
                    w.WL("if (!string.IsNullOrEmpty(" + objDotField + "))");
                    w.WL("{");
                    swlSBAppend(fieldIniName, objDotField);
                    w.WL("}");
                }
                else if (field.Type == "bool")
                {
                    w.WL("if (" + objDotField + ")");
                    w.WL("{");
                    // For bools, there's only two possible values and if we're not writing it out if it's
                    // false, we know if we ARE writing it out then it can only be true, so just put a string
                    // literal in there and don't do ToString() (mem, perf)
                    w.WL("sb.AppendLine(\"" + fieldIniName + "=True\");");
                    w.WL("}");
                }
                else if (field.Type == "bool?")
                {
                    w.WL("if (" + objDotField + " != null)");
                    w.WL("{");
                    swlSBAppend(fieldIniName, obj, toString);
                    w.WL("}");
                }
                else if (_numericTypes.Contains(field.Type))
                {
                    string floatArgs = GetFloatArgsWrite(field.Type);
                    if (field.NumericEmpty != null)
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
                    w.WL("if (" + objDotField + " != null)");
                    w.WL("{");
                    swlSBAppend(fieldIniName, objDotField, "ToString(" + floatArgs + ")");
                    w.WL("}");
                }
                else if (field.Type == Cache.GamesEnum.Name)
                {
                    WriteEnumSingle(
                        writer: w,
                        obj: obj,
                        enumName: Cache.GamesEnum.Name,
                        fieldIniName: fieldIniName,
                        fieldName: field.Name,
                        enumNames: Cache.GamesEnum.GameEnumNames);
                }
                else if (field.Type == Cache.LangsEnum.Name)
                {
                    if (field.IsEnumAndSingleAssignment)
                    {
                        WriteEnumSingle(
                            writer: w,
                            obj: obj,
                            enumName: Cache.LangsEnum.Name,
                            fieldName: field.Name,
                            fieldIniName: fieldIniName,
                            enumNames: Cache.LangsEnum.LangEnumNames,
                            writeValuesLowercase: true);
                    }
                    else
                    {
                        w.WL("if (" + objDotField + " != 0)");
                        w.WL("{");
                        w.WL("sb.Append(\"" + fieldIniName + "=\");");
                        w.WL("CommaCombineLanguageFlags(sb, " + objDotField + ");");
                        w.WL("}");
                    }
                }
                else if (field.Type == "ExpandableDate")
                {
                    w.WL("if (!string.IsNullOrEmpty(" + objDotField + ".UnixDateString))");
                    w.WL("{");
                    swlSBAppend(fieldIniName, objDotField, unixDateString);
                    w.WL("}");
                }
                else if (field.Type == "DateTime?")
                {
                    // If we DIDN'T convert before, we need to convert now
                    string val = !field.DoNotConvertDateTimeToLocal
                        ? "new DateTimeOffset((DateTime)" + objDotField + ").ToUnixTimeSeconds().ToString(\"X\")"
                        : "new DateTimeOffset(((DateTime)" + objDotField + ").ToLocalTime()).ToUnixTimeSeconds().ToString(\"X\")";

                    w.WL("if (" + objDotField + " != null)");
                    w.WL("{");
                    swlSBAppend(fieldIniName, val);
                    w.WL("}");
                }
                else if (field.Type == "CustomResources")
                {
                    w.WL("#if write_old_resources_style");
                    w.WL("if (" + obj + ".ResourcesScanned)");
                    w.WL("{");
                    w.WL("sb.AppendLine(\"HasMap=\" + FMHasResource(" + obj + ", CustomResources.Map).ToString());");
                    w.WL("sb.AppendLine(\"HasAutomap=\" + FMHasResource(" + obj + ", CustomResources.Automap).ToString());");
                    w.WL("sb.AppendLine(\"HasScripts=\" + FMHasResource(" + obj + ", CustomResources.Scripts).ToString());");
                    w.WL("sb.AppendLine(\"HasTextures=\" + FMHasResource(" + obj + ", CustomResources.Textures).ToString());");
                    w.WL("sb.AppendLine(\"HasSounds=\" + FMHasResource(" + obj + ", CustomResources.Sounds).ToString());");
                    w.WL("sb.AppendLine(\"HasObjects=\" + FMHasResource(" + obj + ", CustomResources.Objects).ToString());");
                    w.WL("sb.AppendLine(\"HasCreatures=\" + FMHasResource(" + obj + ", CustomResources.Creatures).ToString());");
                    w.WL("sb.AppendLine(\"HasMotions=\" + FMHasResource(" + obj + ", CustomResources.Motions).ToString());");
                    w.WL("sb.AppendLine(\"HasMovies=\" + FMHasResource(" + obj + ", CustomResources.Movies).ToString());");
                    w.WL("sb.AppendLine(\"HasSubtitles=\" + FMHasResource(" + obj + ", CustomResources.Subtitles).ToString());");
                    w.WL("}");
                    w.WL("#else");
                    w.WL("sb.Append(\"" + fieldIniName + "=\");");
                    w.WL("if (" + obj + ".ResourcesScanned)");
                    w.WL("{");
                    w.WL("CommaCombineHasXFields(" + obj + ", sb);");
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
            w.WL();
            w.WL("#endregion");
        }
    }
}
