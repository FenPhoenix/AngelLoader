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

internal sealed class EscCharsetProber
{
    private const int CHARSETS_NUM = 3;
    internal Charset DetectedCharset;
    private readonly CodingStateMachine[] _codingSM;
    private int _activeSM;

    private ProbingState _state;

    internal EscCharsetProber()
    {
        _codingSM = new CodingStateMachine[CHARSETS_NUM];
        _codingSM[0] = new CodingStateMachine(new HZSMModel());
        _codingSM[1] = new CodingStateMachine(new ISO2022JPSMModel());
        _codingSM[2] = new CodingStateMachine(new ISO2022KRSMModel());
        Reset();
    }

    internal void Reset()
    {
        _state = ProbingState.Detecting;
        for (int i = 0; i < CHARSETS_NUM; i++)
        {
            _codingSM[i].Reset();
        }

        _activeSM = CHARSETS_NUM;
        DetectedCharset = Charset.Null;
    }

    internal ProbingState HandleData(byte[] buf, int offset, int len)
    {
        int max = offset + len;

        for (int i = offset; i < max && _state == ProbingState.Detecting; i++)
        {
            for (int j = _activeSM - 1; j >= 0; j--)
            {
                // byte is feed to all active state machine
                int codingState = _codingSM[j].NextState(buf[i]);
                if (codingState == SMModel.ERROR)
                {
                    // got negative answer for this state machine, make it inactive
                    _activeSM--;
                    if (_activeSM == 0)
                    {
                        _state = ProbingState.NotMe;
                        return _state;
                    }
                    else if (j != _activeSM)
                    {
                        (_codingSM[_activeSM], _codingSM[j]) = (_codingSM[j], _codingSM[_activeSM]);
                    }
                }
                else if (codingState == SMModel.ITSME)
                {
                    _state = ProbingState.FoundIt;
                    DetectedCharset = _codingSM[j].Model.Name;
                    return _state;
                }
            }
        }
        return _state;
    }
}
