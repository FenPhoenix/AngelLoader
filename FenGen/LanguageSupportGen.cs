﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class LanguageSupport
    {
        internal static LanguageSourceEnum FillLangsEnum(string file) => ReadSourceFile(file);

        internal static void Generate(string destFile)
        {
            WriteDestFile(destFile);
        }

        private static LanguageSourceEnum ReadSourceFile(string file)
        {
            var ret = new LanguageSourceEnum
            {
                StringToEnumDictName = "LangStringsToEnums"
            };

            string code = File.ReadAllText(file);
            SyntaxTree tree = ParseTextFast(code);

            var d = GetAttrMarkedItem(tree, SyntaxKind.EnumDeclaration, GenAttributes.FenGenLanguageEnum);
            var langEnum = (EnumDeclarationSyntax)d.Member;
            AttributeSyntax langEnumAttr = d.Attribute;
            if (langEnumAttr.ArgumentList == null || langEnumAttr.ArgumentList.Arguments.Count == 0)
            {
                ThrowErrorAndTerminate(nameof(GenAttributes.FenGenLanguageEnum) + " had 0 args");
            }

            ret.LanguageIndexName = ((LiteralExpressionSyntax)langEnumAttr.ArgumentList!.Arguments[0].Expression).Token.ValueText;
            ret.Name = langEnum.Identifier.ToString().Trim();

            for (int i = 0; i < langEnum.Members.Count; i++)
            {
                var member = langEnum.Members[i];
                string memberName = member.Identifier.ToString();
                ret.LangEnumNames.Add(memberName);
                if (!HasAttribute(member, GenAttributes.FenGenIgnore))
                {
                    ret.LangIndexEnumNames.Add(memberName);
                    ret.LangIndexEnumNamesLowercase.Add(memberName.ToLowerInvariant());

                    AttributeSyntax? langAttr = member
                        .AttributeLists[0]
                        .Attributes
                        .FirstOrDefault(x => x.Name.ToString() == GenAttributes.FenGenLanguage);

                    if (langAttr != null)
                    {
                        if (langAttr.ArgumentList == null || langAttr.ArgumentList.Arguments.Count < 2)
                        {
                            ThrowErrorAndTerminate(nameof(GenAttributes.FenGenLanguage) + " had < 2 args");
                        }

                        string codeArg =
                            ((LiteralExpressionSyntax)langAttr.ArgumentList!.Arguments[0].Expression).Token
                            .ValueText;
                        string translatedNameArg =
                            ((LiteralExpressionSyntax)langAttr.ArgumentList!.Arguments[1].Expression).Token
                            .ValueText;

                        ret.LangCodes.Add(codeArg);
                        ret.LangTranslatedNames.Add(translatedNameArg);
                    }
                }
            }

            return ret;
        }

        private static void WriteDestFile(string destFile)
        {
            var w = GetWriterForClass(destFile, GenAttributes.FenGenLanguageSupportDestClass);

            var langEnum = Cache.LangsEnum;

            int count = langEnum.LangIndexEnumNames.Count;

            w.WL("#region Autogenerated language support code");
            w.WL();

            w.WL("public static readonly int SupportedLanguageCount = Enum.GetValues(typeof(" + langEnum.LanguageIndexName + ")).Length;");
            w.WL();

            w.WL("public enum " + langEnum.LanguageIndexName + " : uint");
            WriteListBody(w, langEnum.LangIndexEnumNames, isEnum: true);

            w.WL("public static readonly string[] SupportedLanguages =");
            WriteListBody(w, langEnum.LangIndexEnumNamesLowercase, addQuotes: true);

            w.WL("private static string[]? _fspl;");
            w.WL("public static string[] FSPrefixedLangs");
            w.WL("{");
            w.WL("get");
            w.WL("{");
            w.WL("if (_fspl == null)");
            w.WL("{");
            w.WL("_fspl = new string[" + count + "];");
            w.WL("for (int i = 0; i < " + count + "; i++)");
            w.WL("{");
            w.WL("_fspl[i] = \"/\" + SupportedLanguages[i];");
            w.WL("}");
            w.WL("}");
            w.WL();
            w.WL("return _fspl;");
            w.WL("}");
            w.WL("}");
            w.WL();

            w.WL("// Even though we have the perfect hash, this one is required for things that need case-insensitivity");
            w.WL("// in the keys!");
            w.WL("public static readonly DictionaryI<" + langEnum.Name + "> " + langEnum.StringToEnumDictName + " = new(" + count + ")");
            var values = new List<string>(count);
            foreach (var lang in langEnum.LangIndexEnumNames)
            {
                values.Add(langEnum.Name + "." + lang);
            }
            WriteDictionaryBody(w, langEnum.LangIndexEnumNamesLowercase, values, keysQuoted: true);

            var codeValues = new List<string>();
            foreach (string codeItem in langEnum.LangCodes)
            {
                string[] codes = codeItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string value = "new[] { ";
                for (int i = 0; i < codes.Length; i++)
                {
                    string code = codes[i];
                    value += "\"" + code + "\"" + (i < codes.Length - 1 ? ", " : " ");
                }
                value += "}";
                codeValues.Add(value);
            }
            w.WL("public static readonly string[][] LangCodes =");
            WriteListBody(w, codeValues);

            w.WL("public static readonly string[] LangTranslatedNames =");
            WriteListBody(w, langEnum.LangTranslatedNames, addQuotes: true);

            #region LanguageToLanguageIndex

            w.WL("/// <summary>");
            w.WL("/// Converts a " + Cache.LangsEnum.Name + " to a " + Cache.LangsEnum.LanguageIndexName + ". *Narrowing conversion, so make sure the language has been checked for convertibility first!");
            w.WL("/// </summary>");
            w.WL("/// <param name=\"language\"></param>");
            w.WL("public static " + Cache.LangsEnum.LanguageIndexName + " LanguageToLanguageIndex(" + Cache.LangsEnum.Name + " language)");
            w.WL("{");
            w.WL("Misc.AssertR(language != " + Cache.LangsEnum.Name + "." + Cache.LangsEnum.LangEnumNames[0] + ", nameof(language) + \" was out of range: \" + language);");
            w.WL();
            w.WL("return language switch");
            w.WL("{");
            for (int i = 0; i < Cache.LangsEnum.LangIndexEnumNames.Count; i++)
            {
                string prefix = i < Cache.LangsEnum.LangIndexEnumNames.Count - 1 ? Cache.LangsEnum.Name + "." + Cache.LangsEnum.LangIndexEnumNames[i] : "_";
                string suffix = i < Cache.LangsEnum.LangIndexEnumNames.Count - 1 ? "," : "";
                w.WL(prefix + " => " + Cache.LangsEnum.LanguageIndexName + "." + Cache.LangsEnum.LangIndexEnumNames[i] + suffix);
            }
            w.WL("};");
            w.WL("}");

            #endregion

            #region LanguageIndexToLanguage

            w.WL("/// <summary>");
            w.WL("/// Converts a " + Cache.LangsEnum.LanguageIndexName + " to a " + Cache.LangsEnum.Name + ". Widening conversion, so it will always succeed.");
            w.WL("/// </summary>");
            w.WL("/// <param name=\"languageIndex\"></param>");
            w.WL("public static " + Cache.LangsEnum.Name + " LanguageIndexToLanguage(" + Cache.LangsEnum.LanguageIndexName + " languageIndex) => languageIndex switch");
            w.WL("{");
            for (int i = 0; i < Cache.LangsEnum.LangIndexEnumNames.Count; i++)
            {
                string prefix = i < Cache.LangsEnum.LangIndexEnumNames.Count - 1 ? Cache.LangsEnum.LanguageIndexName + "." + Cache.LangsEnum.LangIndexEnumNames[i] : "_";
                string suffix = i < Cache.LangsEnum.LangIndexEnumNames.Count - 1 ? "," : "";
                w.WL(prefix + " => " + Cache.LangsEnum.Name + "." + Cache.LangsEnum.LangIndexEnumNames[i] + suffix);
            }
            w.WL("};");
            w.WL();

            #endregion

            w.WL("public static string GetLanguageString(LanguageIndex index) => SupportedLanguages[(uint)index];");
            w.WL();
            w.WL("public static string GetLanguageStringUnsafe(Language language) => SupportedLanguages[(uint)LanguageToLanguageIndex(language)];");
            w.WL();

            w.WL("#endregion");

            w.CloseClassAndNamespace();

            File.WriteAllText(destFile, w.ToString());
        }
    }
}
