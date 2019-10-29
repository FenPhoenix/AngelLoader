using System;

namespace FenGen
{
    internal static class Methods
    {
        private const string Tab = "    ";

        internal static string Indent(int num)
        {
            var ret = "";
            for (int i = 0; i < num; i++) ret += Tab;
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
    }
}
