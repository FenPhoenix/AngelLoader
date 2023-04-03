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

internal sealed class SBCSGroupProber : CharsetProber
{
    private const int PROBERS_NUM = 13;
    private readonly CharsetProber[] _probers = new CharsetProber[PROBERS_NUM];
    private readonly bool[] _isActive = new bool[PROBERS_NUM];
    private int _bestGuess;
    private int _activeNum;

    internal SBCSGroupProber()
    {
        _probers[0] = new SingleByteCharSetProber(new Win1251Model());
        _probers[1] = new SingleByteCharSetProber(new Koi8rModel());
        _probers[2] = new SingleByteCharSetProber(new Latin5Model());
        _probers[3] = new SingleByteCharSetProber(new MacCyrillicModel());
        _probers[4] = new SingleByteCharSetProber(new Ibm866Model());
        _probers[5] = new SingleByteCharSetProber(new Ibm855Model());
        _probers[6] = new SingleByteCharSetProber(new Latin7Model());
        _probers[7] = new SingleByteCharSetProber(new Win1253Model());
        _probers[8] = new SingleByteCharSetProber(new Latin5BulgarianModel());
        _probers[9] = new SingleByteCharSetProber(new Win1251BulgarianModel());
        var hebrewProber = new HebrewProber();
        _probers[10] = hebrewProber;
        // Logical
        _probers[11] = new SingleByteCharSetProber(new Win1255Model(), false, hebrewProber);
        // Visual
        _probers[12] = new SingleByteCharSetProber(new Win1255Model(), true, hebrewProber);
        hebrewProber.SetModelProbers(_probers[11], _probers[12]);
        // disable latin2 before latin1 is available, otherwise all latin1
        // will be detected as latin2 because of their similarity.
        //probers[13] = new SingleByteCharSetProber(new Latin2HungarianModel());
        //probers[14] = new SingleByteCharSetProber(new Win1250HungarianModel());
        Reset();
    }

    internal override ProbingState HandleData(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        //apply filter to original buffer, and we got new buffer back
        //depend on what script it is, we will feed them the new buffer
        //we got after applying proper filter
        //this is done without any consideration to KeepEnglishLetters
        //of each prober since as of now, there are no probers here which
        //recognize languages with English characters.
        byte[] newBuf = FilterWithoutEnglishLetters(buf, offset, len);
        if (newBuf.Length == 0)
        {
            return _state; // Nothing to see here, move on.
        }

        for (int i = 0; i < PROBERS_NUM; i++)
        {
            if (!_isActive[i])
            {
                continue;
            }

            ProbingState st = _probers[i].HandleData(newBuf, 0, newBuf.Length, memoryStream);

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
        switch (_state)
        {
            case ProbingState.FoundIt:
                return 0.99f; //sure yes
            case ProbingState.NotMe:
                return 0.01f;  //sure no
            default:
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
                break;
        }
        return bestConf;
    }

    internal override void Reset()
    {
        /*
        Fen: This local variable was shadowing the class-level one and stealing the increment intended for
        the class-level one. Removing this line fixes the Cyrillic detection failures.
        */
        //int activeNum = 0;
        for (int i = 0; i < PROBERS_NUM; i++)
        {
            _probers[i].Reset();
            _isActive[i] = true;
            _activeNum++;
        }
        _bestGuess = -1;
        _state = ProbingState.Detecting;
    }

    internal override Charset GetCharsetName()
    {
        //if we have no answer yet
        if (_bestGuess == -1)
        {
            GetConfidence();
            //no charset seems positive
            if (_bestGuess == -1)
            {
                _bestGuess = 0;
            }
        }
        return _probers[_bestGuess].GetCharsetName();
    }
}
