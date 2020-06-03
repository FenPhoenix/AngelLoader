using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.CommonStatic;
using static FenGen.Methods;

namespace FenGen
{
    internal static class Games
    {
        internal static void Generate()
        {
            FillGamesEnum();
        }

        internal static void FillGamesEnum()
        {
            if (StateVars.GamesEnum == null)
            {
                string gameSupportFile = FindGameSupportFile();
                var gameSourceEnum = ReadGameSourceEnum(gameSupportFile);
                StateVars.GamesEnum = gameSourceEnum;
            }
        }

        private static string FindGameSupportFile()
        {
            const string locFileTag = "FenGen_GameSupport";

            var taggedFiles = new List<string>();

            var files = Directory.GetFiles(Core.ALProjectPath, "*.cs", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                using (var sr = new StreamReader(f))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.IsWhiteSpace()) continue;
                        string lts = line.TrimStart();

                        if (lts.Length > 0 && lts[0] != '#') break;

                        if (lts.StartsWith(@"#define") && lts.Length > 7 && char.IsWhiteSpace(lts[7]))
                        {
                            string tag = lts.Substring(7).Trim();

                            if (tag == locFileTag)
                            {
                                taggedFiles.Add(f);
                                break;
                            }
                        }
                    }
                }
            }

            #region Error reporting

            static string AddError(string msg, string add)
            {
                if (msg.IsEmpty()) msg = "Games gen: ERRORS:";
                msg += "\r\n" + add;
                return msg;
            }

            string error = "";
            if (taggedFiles.Count == 0) error = AddError(error, "-No tagged source file found");
            if (taggedFiles.Count > 1) error = AddError(error, "-Multiple tagged source files found");
            if (!error.IsEmpty()) ThrowErrorAndTerminate(error);

            #endregion

            return taggedFiles[0];
        }

        private static GameSourceEnum ReadGameSourceEnum(string file)
        {
            const string FenGenGameSourceEnumAttribute = "FenGenGameSourceEnum";

            var ret = new GameSourceEnum();

            string code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);

            var attrMarkedEnums = new List<EnumDeclarationSyntax>();

            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            foreach (SyntaxNode n in nodes)
            {
                if (!n.IsKind(SyntaxKind.EnumDeclaration)) continue;

                var enumItem = (EnumDeclarationSyntax)n;
                if (enumItem.AttributeLists.Count > 0 && enumItem.AttributeLists[0].Attributes.Count > 0)
                {
                    foreach (var attr in enumItem.AttributeLists[0].Attributes)
                    {
                        if (GetAttributeName(attr.Name.ToString(), FenGenGameSourceEnumAttribute))
                        {
                            attrMarkedEnums.Add(enumItem);
                        }
                    }
                }
            }

            if (attrMarkedEnums.Count > 1)
            {
                const string multipleUsesError = "ERROR: Multiple uses of attribute '" + FenGenGameSourceEnumAttribute + "'.";
                ThrowErrorAndTerminate(multipleUsesError);
            }
            else if (attrMarkedEnums.Count == 0)
            {
                const string noneFoundError = "ERROR: No uses of attribute '" + FenGenGameSourceEnumAttribute +
                                              "' (No marked game source enum found)";
                ThrowErrorAndTerminate(noneFoundError);
            }

            var gameSourceEnum = attrMarkedEnums[0];

            ret.Name = gameSourceEnum.Identifier.ToString().Trim();

            foreach (var member in gameSourceEnum.Members)
            {
                ret.Items.Add(member.Identifier.ToString());
            }

            return ret;
        }
    }
}
