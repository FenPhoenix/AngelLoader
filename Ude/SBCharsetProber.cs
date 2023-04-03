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

internal sealed class SingleByteCharSetProber : CharsetProber
{
    private const int SAMPLE_SIZE = 64;
    private const int SB_ENOUGH_REL_THRESHOLD = 1024;
    private const float POSITIVE_SHORTCUT_THRESHOLD = 0.95f;
    private const float NEGATIVE_SHORTCUT_THRESHOLD = 0.05f;
    private const int SYMBOL_CAT_ORDER = 250;
    private const int NUMBER_OF_SEQ_CAT = 4;
    private const int POSITIVE_CAT = NUMBER_OF_SEQ_CAT - 1;

    private readonly SequenceModel _model;

    // true if we need to reverse every pair in the model lookup
    private readonly bool _reversed;

    // char order of last character
    private byte _lastOrder;

    private int _totalSeqs;
    private int _totalChar;
    private readonly int[] _seqCounters = new int[NUMBER_OF_SEQ_CAT];

    // characters that fall in our sampling range
    private int _freqChar;

    // Optional auxiliary prober for name decision. created and destroyed by the GroupProber
    private readonly CharsetProber? _nameProber;

    internal SingleByteCharSetProber(SequenceModel model)
        : this(model, false, null)
    {
    }

    internal SingleByteCharSetProber(SequenceModel model, bool reversed,
        CharsetProber? nameProber)
    {
        _model = model;
        _reversed = reversed;
        _nameProber = nameProber;
        Reset();
    }

    internal override ProbingState HandleData(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        int max = offset + len;

        for (int i = offset; i < max; i++)
        {
            byte order = _model.GetOrder(buf[i]);

            if (order < SYMBOL_CAT_ORDER)
            {
                _totalChar++;
            }

            if (order < SAMPLE_SIZE)
            {
                _freqChar++;

                if (_lastOrder < SAMPLE_SIZE)
                {
                    _totalSeqs++;
                    if (!_reversed)
                    {
                        ++_seqCounters[_model.GetPrecedence((_lastOrder * SAMPLE_SIZE) + order)];
                    }
                    else // reverse the order of the letters in the lookup
                    {
                        ++_seqCounters[_model.GetPrecedence((order * SAMPLE_SIZE) + _lastOrder)];
                    }
                }
            }
            _lastOrder = order;
        }

        if (_state == ProbingState.Detecting)
        {
            if (_totalSeqs > SB_ENOUGH_REL_THRESHOLD)
            {
                float cf = GetConfidence();
                if (cf > POSITIVE_SHORTCUT_THRESHOLD)
                {
                    _state = ProbingState.FoundIt;
                }
                else if (cf < NEGATIVE_SHORTCUT_THRESHOLD)
                {
                    _state = ProbingState.NotMe;
                }
            }
        }
        return _state;
    }

    internal override float GetConfidence()
    {
        /*
        NEGATIVE_APPROACH
        if (totalSeqs > 0) {
            if (totalSeqs > seqCounters[NEGATIVE_CAT] * 10)
                return (totalSeqs - seqCounters[NEGATIVE_CAT] * 10)/totalSeqs * freqChar / mTotalChar;
        }
        return 0.01f;
        */
        // POSITIVE_APPROACH

        if (_totalSeqs > 0)
        {
            float r = 1.0f * _seqCounters[POSITIVE_CAT] / _totalSeqs / _model.TypicalPositiveRatio;
            r = r * _freqChar / _totalChar;
            if (r >= 1.0f)
            {
                r = 0.99f;
            }

            return r;
        }
        return 0.01f;
    }

    internal override void Reset()
    {
        _state = ProbingState.Detecting;
        _lastOrder = 255;
        for (int i = 0; i < NUMBER_OF_SEQ_CAT; i++)
        {
            _seqCounters[i] = 0;
        }

        _totalSeqs = 0;
        _totalChar = 0;
        _freqChar = 0;
    }

    internal override Charset GetCharsetName() => _nameProber?.GetCharsetName() ?? _model.CharsetName;
}
