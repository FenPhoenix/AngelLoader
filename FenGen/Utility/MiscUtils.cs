using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using FenGen.Forms;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FenGen
{
    internal static partial class Misc
    {
        private static readonly CSharpParseOptions _parseOptions = new CSharpParseOptions(
            languageVersion: LanguageVersion.Latest,
            documentationMode: DocumentationMode.None,
            SourceCodeKind.Regular);

        // Try to make Roslyn be as non-slow as possible. It's still going to be slow, but hey...
        internal static SyntaxTree ParseTextFast(string text) => CSharpSyntaxTree.ParseText(text, _parseOptions);

        #region Common gen utils

        [PublicAPI]
        internal static string GetCodeBlock(string destFile, string genAttr)
        {
            string code = File.ReadAllText(destFile);
            SyntaxTree tree = ParseTextFast(code);

            var (member, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, genAttr);
            var iniClass = (ClassDeclarationSyntax)member;

            string iniClassString = iniClass.ToString();
            string classDeclLine = iniClassString.Substring(0, iniClassString.IndexOf('{'));

            return code
                .Substring(0, iniClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
                .TrimEnd() + "\r\n";
        }

        [PublicAPI]
        internal static bool HasAttribute(MemberDeclarationSyntax member, string attrName)
        {
            SeparatedSyntaxList<AttributeSyntax> attributes;
            if (member.AttributeLists.Count > 0 &&
                (attributes = member.AttributeLists[0].Attributes).Count > 0)
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    if (GetAttributeName(attrName, attributes[i].Name.ToString()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [PublicAPI]
        internal static (MemberDeclarationSyntax Member, AttributeSyntax Attribute)
        GetAttrMarkedItem(SyntaxTree tree, SyntaxKind syntaxKind, string attrName)
        {
            var attrMarkedItems = new List<MemberDeclarationSyntax>();
            AttributeSyntax? retAttr = null;

            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            foreach (SyntaxNode n in nodes)
            {
                if (!n.IsKind(syntaxKind)) continue;

                var item = (MemberDeclarationSyntax)n;
                if (item.AttributeLists.Count > 0 && item.AttributeLists[0].Attributes.Count > 0)
                {
                    foreach (var attr in item.AttributeLists[0].Attributes)
                    {
                        if (GetAttributeName(attr.Name.ToString(), attrName))
                        {
                            attrMarkedItems.Add(item);
                            retAttr = attr;
                        }
                    }
                }
            }

            if (attrMarkedItems.Count > 1)
            {
                ThrowErrorAndTerminate("Multiple uses of attribute '" + attrName + "'.");
            }
            else if (attrMarkedItems.Count == 0)
            {
                ThrowErrorAndTerminate("No uses of attribute '" + attrName + "'.");
            }

            return (attrMarkedItems[0], retAttr!);
        }

        /// <summary>
        /// Matches an attribute, ignoring the "Attribute" suffix if it exists in either string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool GetAttributeName(string name, string match)
        {
            // We have to handle this quirk where you can leave off the "Attribute" suffix - Roslyn won't handle
            // it for us
            const string attr = "Attribute";

            if (match.EndsWith(attr)) match = match.Substring(0, match.LastIndexOf(attr, StringComparison.Ordinal));
            if (name.EndsWith(attr)) name = name.Substring(0, name.LastIndexOf(attr, StringComparison.Ordinal));

            return name == match;
        }

        #endregion

        #region Throw and terminate

        [ContractAnnotation("=> halt")]
        internal static void ThrowErrorAndTerminate(string message)
        {
            Trace.WriteLine("FenGen: " + message + "\r\nTerminating FenGen.");
            MessageBox.Show(message + "\r\n\r\nExiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-999);
        }

        [ContractAnnotation("=> halt")]
        internal static void ThrowErrorAndTerminate(Exception ex)
        {
            Trace.WriteLine("FenGen: " + ex + "\r\nTerminating FenGen.");
            using (var f = new ExceptionBox(ex.ToString())) f.ShowDialog();
            Environment.Exit(-999);
        }

        #endregion

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> with all elements initialized to non-null.
        /// Because even with the whole nullable reference types ballyhoo,
        /// you still get nulls-by-default in arrays with nary a warning whatsoever.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        internal static T[] InitializedArray<T>(int length) where T : new()
        {
            T[] ret = new T[length];
            for (int i = 0; i < length; i++) ret[i] = new T();
            return ret;
        }
    }
}
