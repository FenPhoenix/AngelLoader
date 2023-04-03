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

/// <summary>
/// Multi-byte charsets probers
/// </summary>
internal sealed class MBCSGroupProber : CharsetProber
{
    private const int PROBERS_NUM = 6;
    private readonly CharsetProber[] _probers = new CharsetProber[PROBERS_NUM];
    private readonly bool[] _isActive = new bool[PROBERS_NUM];
    private int _bestGuess;
    private int _activeNum;

    internal MBCSGroupProber()
    {
        _probers[0] = new UTF8Prober();
        _probers[1] = new SJISProber();
        _probers[2] = new EUCJPProber();
        _probers[3] = new GB18030Prober();
        _probers[4] = new EUCKRProber();
        _probers[5] = new Big5Prober();
        Reset();
    }

    internal override Charset GetCharsetName()
    {
        if (_bestGuess == -1)
        {
            GetConfidence();
            if (_bestGuess == -1)
            {
                _bestGuess = 0;
            }
        }
        return _probers[_bestGuess].GetCharsetName();
    }

    internal override void Reset()
    {
        _activeNum = 0;
        for (int i = 0; i < _probers.Length; i++)
        {
            _probers[i].Reset();
            _isActive[i] = true;
            ++_activeNum;
        }
        _bestGuess = -1;
        _state = ProbingState.Detecting;
    }

    internal override ProbingState HandleData(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        // do filtering to reduce load to probers
        // @Ude: Byte array allocation
        byte[] highbyteBuf = new byte[len];
        int hptr = 0;
        //assume previous is not ascii, it will do no harm except add some noise
        bool keepNext = true;
        int max = offset + len;

        for (int i = offset; i < max; i++)
        {
            if ((buf[i] & 0x80) != 0)
            {
                highbyteBuf[hptr++] = buf[i];
                keepNext = true;
            }
            else
            {
                //if previous is highbyte, keep this even it is a ASCII
                if (keepNext)
                {
                    highbyteBuf[hptr++] = buf[i];
                    keepNext = false;
                }
            }
        }

        for (int i = 0; i < _probers.Length; i++)
        {
            if (!_isActive[i])
            {
                continue;
            }

            ProbingState st = _probers[i].HandleData(highbyteBuf, 0, hptr, memoryStream);
            if (st == ProbingState.FoundIt)
            {
                _bestGuess = i;
                _state = ProbingState.FoundIt;
                break;
            }
            else if (st == ProbingState.NotMe)
            {
                _isActive[i] = false;
                _activeNum--;
                if (_activeNum <= 0)
                {
                    _state = ProbingState.NotMe;
                    break;
                }
            }
        }
        return _state;
    }

    internal override float GetConfidence()
    {
        float bestConf = 0.0f;

        if (_state == ProbingState.FoundIt)
        {
            return 0.99f;
        }
        else if (_state == ProbingState.NotMe)
        {
            return 0.01f;
        }
        else
        {
            for (int i = 0; i < PROBERS_NUM; i++)
            {
                if (!_isActive[i])
                {
                    continue;
                }

                float cf = _probers[i].GetConfidence();
                if (bestConf < cf)
                {
                    bestConf = cf;
                    _bestGuess = i;
                }
            }
        }
        return bestConf;
    }
}
