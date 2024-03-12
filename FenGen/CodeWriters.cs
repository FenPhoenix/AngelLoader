using System.Text;
using JetBrains.Annotations;

namespace FenGen;

internal static class CodeWriters
{
    [PublicAPI]
    internal sealed class IndentingWriter
    {
        private int _nextIndent;
        private readonly bool _fileScopedNamespace;
        private readonly StringBuilder _sb;

        private static string Indent(int num)
        {
            const string tab = "    ";
            string ret = "";
            for (int i = 0; i < num; i++)
            {
                ret += tab;
            }
            return ret;
        }

        internal void IncrementIndent() => _nextIndent++;
        internal void DecrementIndent() => _nextIndent--;

        internal IndentingWriter(int startingIndent, bool fileScopedNamespace)
        {
            _nextIndent = startingIndent;
            _fileScopedNamespace = fileScopedNamespace;
            _sb = new StringBuilder();
        }

        internal IndentingWriter(int startingIndent) : this(startingIndent, false) { }

        internal IndentingWriter(bool fileScopedNamespace) : this(0, fileScopedNamespace) { }

        internal IndentingWriter() : this(0, false) { }

        private bool _inSwitchStatement;
        private bool _inFirstCaseStatement;

        internal void AppendRawString(string str) => _sb.Append(str);

        internal void WL(string str = "")
        {
            // TODO: switch statement handler doesn't handle nested curly brace blocks
            int curIndent = _nextIndent;

            string strT = str.TrimStart();
            if (!strT.StartsWithO("//")) strT = strT.TrimEnd();
            if (strT == "{")
            {
                _nextIndent++;
            }
            else if (strT is "}" or "};")
            {
                _nextIndent--;
                curIndent = _nextIndent;
                if (_inSwitchStatement)
                {
                    _inSwitchStatement = false;
                    curIndent--;
                    _nextIndent--;
                }
            }
            else if (strT.StartsWithPlusWhiteSpace("switch"))
            {
                _inSwitchStatement = true;
                _inFirstCaseStatement = true;
            }
            else if (strT.StartsWithPlusWhiteSpace("case"))
            {
                _nextIndent++;
                if (_inSwitchStatement)
                {
                    if (_inFirstCaseStatement)
                    {
                        _inFirstCaseStatement = false;
                    }
                    else
                    {
                        curIndent--;
                        _nextIndent--;
                    }
                }
            }

            bool noIndent = strT.IsWhiteSpace() ||
                            (strT.StartsWithO("#") &&
                             !strT.StartsWithPlusWhiteSpace("#region") &&
                             strT != "#region" &&
                             !strT.StartsWithPlusWhiteSpace("#endregion") &&
                             strT != "#endregion");
            string indent = noIndent ? "" : Indent(curIndent);
            _sb.Append(indent).AppendLine(strT);
        }

        internal void WLs(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                WL(lines[i]);
            }
        }

        internal void CloseClassAndNamespace()
        {
            WL("}");
            if (!_fileScopedNamespace)
            {
                WL("}");
            }
        }

        public override string ToString() => _sb.ToString();
    }
}
