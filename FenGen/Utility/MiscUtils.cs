using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        internal static string Indent(int num)
        {
            const string tab = "    ";
            string ret = "";
            for (int i = 0; i < num; i++) ret += tab;
            return ret;
        }

        /// <summary>
        /// Matches an attribute, ignoring the "Attribute" suffix if it exists in either string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        internal static bool GetAttributeName(string name, string match)
        {
            // We have to handle this quirk where you can leave off the "Attribute" suffix - Roslyn won't handle
            // it for us
            const string attr = "Attribute";

            if (match.EndsWith(attr)) match = match.Substring(0, match.LastIndexOf(attr, StringComparison.Ordinal));
            if (name.EndsWith(attr)) name = name.Substring(0, name.LastIndexOf(attr, StringComparison.Ordinal));

            return name == match;
        }

        internal static string StripPrefixes(string line)
        {
            string[] prefixes = { "private", "internal", "protected", "public", "static", "readonly", "dynamic" };
            while (prefixes.Any(x => line.StartsWithI(x + ' ')))
            {
                foreach (string pre in prefixes)
                {
                    if (line.StartsWithI(pre + ' '))
                    {
                        line = line.Substring(pre.Length + 1).TrimStart();
                    }
                }
            }
            return line;
        }

        internal static void ThrowErrorAndTerminate(string message)
        {
            Trace.WriteLine("FenGen: " + message + "\r\nTerminating FenGen.");
            MessageBox.Show(message + "\r\n\r\n" + @"Exiting.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-999);
        }

        #region Clamping

        /// <summary>
        /// Clamps a number to between min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value)
        {
            return Math.Max(value, 0);
        }

        #endregion
    }
}
