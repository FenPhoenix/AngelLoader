using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FenGen.Forms;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FenGen
{
    internal static partial class Misc
    {
        private static readonly CSharpParseOptions _parseOptions = new(
            languageVersion: LanguageVersion.Latest,
            documentationMode: DocumentationMode.None,
            SourceCodeKind.Regular);

        // Try to make Roslyn be as non-slow as possible. It's still going to be slow, but hey...
        internal static SyntaxTree ParseTextFast(string text) => CSharpSyntaxTree.ParseText(text, _parseOptions);

        #region Common gen utils

        internal static void WriteListBody(
            CodeWriters.IndentingWriter w,
            List<string> list,
            bool addQuotes = false,
            bool isEnum = false)
        {
            string quote = addQuotes ? "\"" : "";

            w.WL("{");
            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];
                string suffix = i < list.Count - 1 ? "," : "";
                w.WL(quote + item + quote + suffix);
            }
            w.WL("}" + (isEnum ? "" : ";"));
            w.WL();
        }

        internal static void WriteDictionaryBody(
            CodeWriters.IndentingWriter w,
            List<string> keys,
            List<string> values,
            bool keysQuoted = false,
            bool valuesQuoted = false)
        {
            string keyQuote = keysQuoted ? "\"" : "";
            string valueQuote = valuesQuoted ? "\"" : "";

            w.WL("{");
            for (int i = 0; i < keys.Count; i++)
            {
                //string item = list[i];
                string key = keys[i];
                string value = values[i];
                string suffix = i < keys.Count - 1 ? "," : "";
                //w.WL(quote + item + quote + suffix);
                w.WL("{ " + keyQuote + key + keyQuote + ", " + valueQuote + value + valueQuote + " }" + suffix);
            }
            w.WL("};");
            w.WL();
        }

        private static (string CodeBlock, bool FileScopedNamespace)
        GetCodeBlock(string file, string genAttr)
        {
            string code = File.ReadAllText(file);
            SyntaxTree tree = ParseTextFast(code);

            bool fileScopedNamespace = false;

            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            foreach (SyntaxNode n in nodes)
            {
                if (n is BaseNamespaceDeclarationSyntax)
                {
                    fileScopedNamespace = n is FileScopedNamespaceDeclarationSyntax;
                    break;
                }
            }

            var (member, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, genAttr);
            var iniClass = (ClassDeclarationSyntax)member;

            string iniClassString = iniClass.ToString();
            string classDeclLine = iniClassString.Substring(0, iniClassString.IndexOf('{'));

            code = code
                .Substring(0, iniClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
                .TrimEnd() + "\r\n";

            return (code, fileScopedNamespace);
        }

        internal static CodeWriters.IndentingWriter GetWriterForClass(string destFile, string classAttribute)
        {
            (string codeBlock, bool fileScopedNamespace) = GetCodeBlock(destFile, classAttribute);
            var w = new CodeWriters.IndentingWriter(startingIndent: fileScopedNamespace ? 0 : 1, fileScopedNamespace);
            w.AppendRawString(codeBlock);
            w.WL("{");
            return w;
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
            int index;
            while ((index = match.IndexOf('.')) > -1)
            {
                match = match.Substring(index + 1);
            }

            // We have to handle this quirk where you can leave off the "Attribute" suffix - Roslyn won't handle
            // it for us
            const string attr = "Attribute";

            if (match.EndsWithO(attr)) match = match.Substring(0, match.LastIndexOf(attr, StringComparison.Ordinal));
            if (name.EndsWithO(attr)) name = name.Substring(0, name.LastIndexOf(attr, StringComparison.Ordinal));

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

        internal static void WriteXml(XmlDocument xml)
        {
            List<string> lines;
            using (var strW = new StringWriter())
            {
                var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, OmitXmlDeclaration = true };
                using (var xmlWriter = XmlWriter.Create(strW, settings))
                {
                    xml.Save(xmlWriter);
                }

                lines = strW
                    .ToString()
                    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                    .ToList();
            }

            // Remove consecutive whitespace lines (leaving only one-in-a-row at most).
            // This gets rid of the garbage left behind from removing the old nodes (whitespace lines).
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsWhiteSpace())
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        if (lines[j].IsWhiteSpace())
                        {
                            lines.RemoveAt(j);
                            j--;
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            using var sw = new StreamWriter(Core.ALProjectFile, append: false, Encoding.UTF8);
            for (int i = 0; i < lines.Count; i++)
            {
                if (i == lines.Count - 1)
                {
                    sw.Write(lines[i]);
                }
                else
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }
    }
}
