using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FenGen.Forms;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FenGen;

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
            string key = keys[i];
            string value = values[i];
            string suffix = i < keys.Count - 1 ? "," : "";
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
        var targetClass = (ClassDeclarationSyntax)member;

        string targetClassString = targetClass.ToString();
        string classDeclLine = targetClassString.Substring(0, targetClassString.IndexOf('{'));

        code = code
            .Substring(0, targetClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
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
            foreach (var attrList in item.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
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

    internal sealed class AttrMarkedItem
    {
        internal readonly MemberDeclarationSyntax Member;
        internal readonly List<AttributeSyntax> Attributes = new();

        internal AttrMarkedItem(MemberDeclarationSyntax member)
        {
            Member = member;
        }
    }

    [PublicAPI]
    internal static List<AttrMarkedItem>
    GetAttrMarkedItems(SyntaxTree tree, SyntaxKind syntaxKind, params string[] attrNames)
    {
        var ret = new List<AttrMarkedItem>();

        var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
        foreach (SyntaxNode n in nodes)
        {
            if (!n.IsKind(syntaxKind)) continue;

            var item = (MemberDeclarationSyntax)n;
            foreach (var attrList in item.AttributeLists)
            {
                if (attrList.Attributes.Count > 0)
                {
                    var attrMarkedItem = new AttrMarkedItem(item);
                    ret.Add(attrMarkedItem);

                    foreach (var attr in attrList.Attributes)
                    {
                        foreach (string attrName in attrNames)
                        {
                            if (GetAttributeName(attr.Name.ToString(), attrName))
                            {
                                attrMarkedItem.Attributes.Add(attr);
                            }
                        }
                    }
                }
            }
        }

        return ret;
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

    // Needed because on Framework, Environment.Exit() is not marked with a [DoesNotReturn] attribute itself.
#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
    [ContractAnnotation("=> halt")]
    [DoesNotReturn]
    internal static void ThrowErrorAndTerminate(string message)
    {
        Trace.WriteLine("FenGen: " + message + "\r\nTerminating FenGen.");
        MessageBox.Show(message + "\r\n\r\nExiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(-999);
    }

    [ContractAnnotation("=> halt")]
    [DoesNotReturn]
    internal static void ThrowErrorAndTerminate(Exception ex)
    {
        Trace.WriteLine("FenGen: " + ex + "\r\nTerminating FenGen.");
        using (var f = new ExceptionBox(ex.ToString())) f.ShowDialog();
        Environment.Exit(-999);
    }
#pragma warning restore CS8763

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

    #region TryParse Invariant

    /// <summary>
    /// Calls <see langword="float"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Float"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="float"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Float_TryParseInv(string s, out float result)
    {
        return float.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="double"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Float"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="double"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Double_TryParseInv(string s, out double result)
    {
        return double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="int"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="int"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Int_TryParseInv(string s, out int result)
    {
        return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="uint"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="uint"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UInt_TryParseInv(string s, out uint result)
    {
        return uint.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="long"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="long"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Long_TryParseInv(string s, out long result)
    {
        return long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="ulong"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="ulong"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ULong_TryParseInv(string s, out ulong result)
    {
        return ulong.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    #endregion
}
