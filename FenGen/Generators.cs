using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                else if (strT.StartsWith("switch "))
                {
                    _inSwitchStatement = true;
                    _inFirstCaseStatement = true;
                }
                else if (strT.StartsWith("case "))
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

                string indent = strT.IsWhiteSpace() || strT.StartsWith("#") ? "" : Indent(curIndent);
                _sb.AppendLine(indent + strT);
            }
        }

        internal sealed class SectionedIniFile
        {
            private int _indent = 0;

            internal SectionedIniFile(StringBuilder sb, int startingIndent)
            {
                _indent = startingIndent;
            }
        }
    }
}
