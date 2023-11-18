using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen;

internal static class FMData
{
    private const BindingFlags _bFlagsEnum = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private sealed class Field
    {
        internal string Type = "";
        internal string Name = "";
        internal string IniName = "";
        internal ListType ListType = ListType.MultipleLines;
        internal long? NumericEmpty;
        internal bool DoNotTrimValue;
        internal bool DoNotSubstring;
        internal bool DoNotConvertDateTimeToLocal;
        internal bool DoNotWrite;
        internal bool IsEnumAndSingleAssignment;
        internal bool IsReadmeEncoding;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum ListType
    {
        MultipleLines,
        CommaSeparated
    }

    private static readonly HashSet<string> _numericTypes = new(System.StringComparer.Ordinal)
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
        List<Field> fields = ReadSourceFields(sourceFile);

        var w = GetWriterForClass(destFile, GenAttributes.FenGenFMDataDestClass);

        const string obj = "fm";

        WriteReadSection(w, obj, fields);

        w.WL();

        WriteWriter(w, obj, fields);

        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }

    [MustUseReturnValue]
    private static List<Field> ReadSourceFields(string sourceFile)
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
                    switch (attr.Name.ToString())
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
                        case GenAttributes.FenGenReadmeEncoding:
                            field.IsReadmeEncoding = true;
                            break;
                        case GenAttributes.FenGenDoNotConvertDateTimeToLocal:
                            field.DoNotConvertDateTimeToLocal = true;
                            break;
                        case GenAttributes.FenGenDoNotTrimValue:
                            field.DoNotTrimValue = true;
                            break;
                        case GenAttributes.FenGenDoNotSubstring:
                            field.DoNotSubstring = true;
                            break;
                        case GenAttributes.FenGenNumericEmpty:
                        {
                            CheckParamCount(attr, 1);

                            // Have to do this ridiculous method of getting the value, because if the value is
                            // negative, we end up getting a PrefixUnaryExpressionSyntax rather than the entire
                            // number. But ToString() gives us the string version of the entire number. Argh...
                            string val = attr.ArgumentList!.Arguments[0].Expression.ToString();
                            Long_TryParseInv(val, out long result);
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

        var fields = new List<Field>();

        foreach (SyntaxNode item in fmDataClass.ChildNodes())
        {
            if (item.IsKind(SyntaxKind.FieldDeclaration) || item.IsKind(SyntaxKind.PropertyDeclaration))
            {
                var field = new Field();
                FillFieldFromAttributes((MemberDeclarationSyntax)item, field, out bool ignore);
                if (ignore) continue;

                field.Name = (item.IsKind(SyntaxKind.FieldDeclaration)
                    ? ((FieldDeclarationSyntax)item).Declaration.Variables[0].Identifier
                    : ((PropertyDeclarationSyntax)item).Identifier).Value!.ToString();
                field.Type = (item.IsKind(SyntaxKind.FieldDeclaration)
                    ? ((FieldDeclarationSyntax)item).Declaration.Type
                    : ((PropertyDeclarationSyntax)item).Type).ToString();
                fields.Add(field);
            }
        }

        return fields;
    }

    private static void WriteReadSection(CodeWriters.IndentingWriter w, string obj, List<Field> fields)
    {
        static string GetTryParseArgsRead(string fieldType) =>
            IsDecimal(fieldType)
                ? "NumberStyles.Float, NumberFormatInfo.InvariantInfo, "
                : "NumberStyles.Integer, NumberFormatInfo.InvariantInfo, ";
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

            w.WL("private static void FMData_" + fieldIniName + "_Set(FanMission " + obj + ", ReadOnlySpan<char> " + val + ")");
            w.WL("{");

            if (!field.DoNotTrimValue)
            {
                w.WL(val + " = " + val + ".Trim();");
            }
            else
            {
                w.WL("// We require this value to be untrimmed");
            }

            if (field.IsReadmeEncoding)
            {
                w.WL("AddReadmeEncoding(" + obj + ", " + val + ");");
            }
            else if (field.Type.StartsWithO("List<"))
            {
                string listType = field.Type.Substring(field.Type.IndexOf('<')).TrimStart('<').TrimEnd('>');

                string varToAdd = listType == "string" && field.ListType == ListType.MultipleLines
                    ? val
                    : "result";

                string toString = listType == "string" ? ".ToString()" : "";
                string objListSet = objDotField + ".Add(" + varToAdd + toString + ");";

                if (listType == "string")
                {
                    if (field.ListType == ListType.MultipleLines)
                    {
                        //w.WL("if (!string.IsNullOrEmpty(" + val + "))");
                        w.WL("if (!val.IsEmpty)");
                        w.WL("{");
                        w.WL(objListSet);
                        w.WL("}");
                    }
                    else
                    {
                        w.WL(objDotField + ".Clear();");
                        w.WL("string[] items = " + val + ".ToString().Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);");
                        w.WL("for (int a = 0; a < items.Length; a++)");
                        w.WL("{");
                        w.WL("string result = items[a].Trim();");
                        w.WL(objListSet);
                        w.WL("}");
                    }
                }
                else if (_numericTypes.Contains(listType))
                {
                    string tryParseArgs = GetTryParseArgsRead(listType);
                    if (field.ListType == ListType.MultipleLines)
                    {
                        w.WL("if (" + objDotField + " == null)");
                        w.WL("{");
                        w.WL(objDotField + " = new List<" + listType + ">();");
                        w.WL("}");
                        w.WL("bool success = " + listType + ".TryParse(" + val + ", " + tryParseArgs + "out " + listType + " result);");
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
                        w.WL("bool success = " + listType + ".TryParse(items[a], " + tryParseArgs + "out " + listType + " result);");
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
                w.WL(objDotField + " = " + val + ".ToString();");
            }
            else if (field.Type == "bool")
            {
                w.WL(objDotField + " = " + val + ".EqualsTrue();");
            }
            else if (field.Type == "bool?")
            {
                w.WL(objDotField + " = " + val + ".EqualsTrue() ? true : " + val + ".EqualsFalse() ? false : (bool?)null;");
            }
            else if (_numericTypes.Contains(field.Type))
            {
                string tryParseArgs = GetTryParseArgsRead(field.Type);
                string tryParseLine = field.Type + ".TryParse(" + val + ", " + tryParseArgs + "out " + field.Type + " result);";
                if (field.NumericEmpty != null && field.NumericEmpty != 0)
                {
                    w.WL("bool success = " + tryParseLine);
                    w.WL(objDotField + " = success ? result : " + ((long)field.NumericEmpty).ToStrInv() + ";");
                }
                else
                {
                    w.WL(tryParseLine);
                    w.WL(objDotField + " = result;");
                }
            }
            else if (field.Type[field.Type.Length - 1] == '?' &&
                     _numericTypes.Contains(field.Type.Substring(0, field.Type.Length - 1)))
            {
                string tryParseArgs = GetTryParseArgsRead(field.Type);
                string ftNonNull = field.Type.Substring(0, field.Type.Length - 1);
                w.WL("bool success = " + ftNonNull + ".TryParse(" + val + ", " + tryParseArgs + "out " + ftNonNull + " result);");
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
                if (field.IsEnumAndSingleAssignment)
                {
                    w.WL("// @NET5: Get rid of this allocation");
                    w.WL("if (Langs_TryGetValue(" + val + ".ToString(), 0, " + val + ".Length, out var result))");
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
                w.WL(objDotField + ".UnixDateString = " + val + ".ToString();");
            }
            else if (field.Type == "DateTime?")
            {
                w.WL(objDotField + " = ConvertHexUnixDateToDateTime(" + val + ");");
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
            w.WL("private static void FMData_" + item + "_Set(FanMission " + obj + ", ReadOnlySpan<char> " + val + ")");
            w.WL("{");
            w.WL(obj + ".SetResource(CustomResources." + item.Substring(3) + ", " + val + ".EqualsTrue());");
            w.WL(obj + ".ResourcesScanned = true;");
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

        w.WL("private readonly unsafe struct FMData_DelegatePointerWrapper");
        w.WL("{");
        w.WL("internal readonly delegate*<FanMission, ReadOnlySpan<char>, void> Action;");
        w.WL();
        w.WL("internal FMData_DelegatePointerWrapper(delegate*<FanMission, ReadOnlySpan<char>, void> action)");
        w.WL("{");
        w.WL("Action = action;");
        w.WL("}");
        w.WL("}");
        w.WL();

        w.WL("private static readonly unsafe Dictionary<ReadOnlyMemory<char>, FMData_DelegatePointerWrapper> _actionDict_FMData = new(new MemoryStringComparer())");
        w.WL("{");
        for (int i = 0; i < dictFields.Count; i++)
        {
            Field field = dictFields[i];
            string fieldIniName = field.IniName.IsEmpty() ? field.Name : field.IniName;
            string comma = i == dictFields.Count - 1 ? "" : ",";
            w.WL("{ \"" + fieldIniName + "\".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_" + fieldIniName + "_Set) }" + comma);

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

    // @LANGS: Carry as much data from the source enum as you can: type (uint) etc.
    private static void WriteWriter(CodeWriters.IndentingWriter w, string obj, List<Field> fields)
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
                writer.WL("sb.Append(\"" + fieldIniName + "\").Append('=').AppendLine(\"" + (writeValuesLowercase ? enumNames[gi].ToLowerInvariant() : enumNames[gi]) + "\");");
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
            "private static void WriteFMDataIni(List<FanMission> fmDataList, List<FanMission> fmDataListTDM, string fileName)",
            "{",
            "var sb = new StringBuilder();",
            "",
            "static void AddFMToSB(FanMission fm, StringBuilder sb)",
            "{",
            "sb.AppendLine(\"[FM]\");",
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
                w.WL("sb.Append(\"" + objField + "\").Append('=');");
                w.WL("sb.AppendLine(" + value + suffix + ");");
            }

            if (field.IsReadmeEncoding)
            {
                w.WL("foreach (var item in " + objDotField + ")");
                w.WL("{");
                w.WL("sb.Append(\"" + fieldIniName + "\").Append('=');");
                w.WL("sb.Append(item.Key).Append(',').AppendLine(item.Value.ToString());");
                w.WL("}");
            }
            else if (field.Type.StartsWithO("List<"))
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
                w.WL("sb.Append(\"" + fieldIniName + "\").AppendLine(\"=True\");");
                w.WL("}");
            }
            else if (field.Type == "bool?")
            {
                w.WL("if (" + objDotField + " != null)");
                w.WL("{");
                swlSBAppend(fieldIniName, objDotField, toString);
                w.WL("}");
            }
            else if (_numericTypes.Contains(field.Type))
            {
                string floatArgs = GetFloatArgsWrite(field.Type);
                if (field.NumericEmpty != null)
                {
                    w.WL("if (" + objDotField + " != " + ((long)field.NumericEmpty).ToStrInv() + ")");
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
                    fieldName: field.Name,
                    fieldIniName: fieldIniName,
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
                    w.WL("sb.Append(\"" + fieldIniName + "\").Append('=');");
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
                w.WL("sb.Append(\"" + fieldIniName + "\").Append('=');");
                w.WL("if (" + obj + ".ResourcesScanned)");
                w.WL("{");
                w.WL("CommaCombineHasXFields(" + objDotField + ", sb);");
                w.WL("}");
                w.WL("else");
                w.WL("{");
                w.WL("sb.AppendLine(\"NotScanned\");");
                w.WL("}");
                w.WL("#endif");
            }
        }

        // AddFMToSB
        w.WL("}");
        w.WL();
        w.WL("foreach (FanMission fm in fmDataList)");
        w.WL("{");
        w.WL("AddFMToSB(fm, sb);");
        w.WL("}");
        w.WL();
        w.WL("foreach (FanMission fm in fmDataListTDM)");
        w.WL("{");
        w.WL("AddFMToSB(fm, sb);");
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
