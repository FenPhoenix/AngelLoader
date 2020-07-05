using System.Text;
using static FenGen.Misc;

namespace FenGen
{
    internal static class Generators
    {
        internal sealed class IndentingWriter
        {
            private int _nextIndent;
            private readonly StringBuilder _sb;

            internal IndentingWriter(StringBuilder sb, int startingIndent)
            {
                _sb = sb;
                _nextIndent = startingIndent;
            }

            private bool _inSwitchStatement;
            private bool _inFirstCaseStatement;

            internal void WL(string str = "")
            {
                // TODO: switch statement handler doesn't handle nested curly brace blocks
                int curIndent = _nextIndent;

                string strT = str.Trim();
                if (strT == "{")
                {
                    _nextIndent++;
                }
                else if (strT == "}")
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
                                (strT.StartsWith("#") &&
                                 !strT.StartsWithPlusWhiteSpace("#region") &&
                                 strT != "#region" &&
                                 !strT.StartsWithPlusWhiteSpace("#endregion") &&
                                 strT != "#endregion");
                string indent = noIndent ? "" : Indent(curIndent);
                _sb.AppendLine(indent + strT);
            }

            internal void WLs(string[] lines)
            {
                for (int i = 0; i < lines.Length; i++) WL(lines[i]);
            }
        }
    }
}
