/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Mozilla Universal charset detector code.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 2001
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *          Shy Shalom <shooshX@gmail.com>
 *          Rudi Pettazzi <rudi.pettazzi@gmail.com> (C# port)
 * 
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

namespace Ude.NetStandard;

// We use gb18030 to replace gb2312, because 18030 is a superset.
internal sealed class GB18030Prober : CharsetProber
{
    private readonly CodingStateMachine _codingSM;
    private readonly GB18030DistributionAnalyser _analyser;
    private readonly byte[] _lastChar;

    internal GB18030Prober()
    {
        _lastChar = new byte[2];
        _codingSM = new CodingStateMachine(new GB18030SMModel());
        _analyser = new GB18030DistributionAnalyser();
        Reset();
    }

    internal override Charset GetCharsetName() => Charset.GB_18030;

    internal override ProbingState HandleData(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        int max = offset + len;

        for (int i = offset; i < max; i++)
        {
            int codingState = _codingSM.NextState(buf[i]);
            if (codingState == SMModel.ERROR)
            {
                _state = ProbingState.NotMe;
                break;
            }
            if (codingState == SMModel.ITSME)
            {
                _state = ProbingState.FoundIt;
                break;
            }
            if (codingState == SMModel.START)
            {
                int charLen = _codingSM.CurrentCharLen;
                if (i == offset)
                {
                    _lastChar[1] = buf[offset];
                    _analyser.HandleOneChar(_lastChar, 0, charLen);
                }
                else
                {
                    _analyser.HandleOneChar(buf, i - 1, charLen);
                }
            }
        }

        _lastChar[0] = buf[max - 1];

        if (_state == ProbingState.Detecting)
        {
            if (_analyser.GotEnoughData() && GetConfidence() > SHORTCUT_THRESHOLD)
            {
                _state = ProbingState.FoundIt;
            }
        }

        return _state;
    }

    internal override float GetConfidence()
    {
        return _analyser.GetConfidence();
    }

    internal override void Reset()
    {
        _codingSM.Reset();
        _state = ProbingState.Detecting;
        _analyser.Reset();
    }
}
