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

internal enum InputState { PureASCII = 0, EscASCII = 1, HighByte = 2 }

public abstract class UniversalDetector
{
    private const float MINIMUM_THRESHOLD = 0.20f;

    private InputState _inputState;
    private bool _start;
    private bool _gotData;
    protected bool _done;
    private byte _lastChar;
    private const int PROBERS_NUM = 3;
    private readonly CharsetProber?[] _charsetProbers = new CharsetProber?[PROBERS_NUM];
    private CharsetProber? _escCharsetProber;
    private Charset _detectedCharset;

    protected UniversalDetector()
    {
        _start = true;
        _inputState = InputState.PureASCII;
        _lastChar = 0x00;
    }

    public void Feed(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        if (_done)
        {
            return;
        }

        if (len > 0)
        {
            _gotData = true;
        }

        // If the data starts with BOM, we know it is UTF
        if (_start)
        {
            _start = false;
            if (len > 3)
            {
                switch (buf[0])
                {
                    case 0xEF:
                        if (buf[1] == 0xBB && buf[2] == 0xBF)
                        {
                            _detectedCharset = Charset.UTF8;
                        }
                        break;
                    case 0xFE:
                        if (buf[1] == 0xFF)
                        {
                            _detectedCharset = Charset.UTF16BE;
                        }
                        break;
                    case 0x00:
                        if (buf[1] == 0x00 && buf[2] == 0xFE && buf[3] == 0xFF)
                        {
                            _detectedCharset = Charset.UTF32BE;
                        }
                        break;
                    case 0xFF:
                        if (buf[1] == 0xFE)
                        {
                            _detectedCharset = buf[2] == 0x00 && buf[3] == 0x00
                                ? Charset.UTF32LE
                                : Charset.UTF16LE;
                        }
                        break;
                }  // switch
            }
            if (_detectedCharset != Charset.Null)
            {
                _done = true;
                return;
            }
        }

        for (int i = 0; i < len; i++)
        {
            // other than 0xa0, if every other character is ascii, the page is ascii
            if ((buf[i] & 0x80) != 0 && buf[i] != 0xA0)
            {
                // we got a non-ascii byte (high-byte)
                if (_inputState != InputState.HighByte)
                {
                    _inputState = InputState.HighByte;

                    // kill EscCharsetProber if it is active
                    _escCharsetProber = null;

                    // start multibyte and singlebyte charset prober
                    _charsetProbers[0] ??= new MBCSGroupProber();
                    _charsetProbers[1] ??= new SBCSGroupProber();
                    _charsetProbers[2] ??= new Latin1Prober();
                }
            }
            else
            {
                if (_inputState == InputState.PureASCII &&
                    (buf[i] == 0x1B || (buf[i] == 0x7B && _lastChar == 0x7E)))
                {
                    // found escape character or HZ "~{"
                    _inputState = InputState.EscASCII;
                }
                _lastChar = buf[i];
            }
        }

        ProbingState st;

        switch (_inputState)
        {
            case InputState.EscASCII:
                _escCharsetProber ??= new EscCharsetProber();
                st = _escCharsetProber.HandleData(buf, offset, len, memoryStream);
                if (st == ProbingState.FoundIt)
                {
                    _done = true;
                    _detectedCharset = _escCharsetProber.GetCharsetName();
                }
                break;
            case InputState.HighByte:
                for (int i = 0; i < PROBERS_NUM; i++)
                {
                    CharsetProber? prober = _charsetProbers[i];
                    if (prober != null)
                    {
                        st = prober.HandleData(buf, offset, len, memoryStream);
                        if (st == ProbingState.FoundIt)
                        {
                            _done = true;
                            _detectedCharset = prober.GetCharsetName();
                            return;
                        }
                    }
                }
                break;
                // default: pure ascii
        }
    }

    /// <summary>
    /// Notify detector that no further data is available.
    /// </summary>
    public void DataEnd()
    {
        if (!_gotData)
        {
            // we haven't got any data yet, return immediately
            // caller program sometimes call DataEnd before anything has
            // been sent to detector
            return;
        }

        if (_detectedCharset != Charset.Null)
        {
            _done = true;
            Report(_detectedCharset, 1.0f);
            return;
        }

        if (_inputState == InputState.HighByte)
        {
            float maxProberConfidence = 0.0f;
            int maxProber = 0;
            for (int i = 0; i < PROBERS_NUM; i++)
            {
                CharsetProber? prober = _charsetProbers[i];
                if (prober != null)
                {
                    float proberConfidence = prober.GetConfidence();
                    if (proberConfidence > maxProberConfidence)
                    {
                        maxProberConfidence = proberConfidence;
                        maxProber = i;
                    }
                }
            }

            if (maxProberConfidence > MINIMUM_THRESHOLD)
            {
                // @Ude: Can we rewrite this in a way that it knows it isn't null?
                Report(_charsetProbers[maxProber]!.GetCharsetName(), maxProberConfidence);
            }
        }
        else if (_inputState == InputState.PureASCII)
        {
            Report(Charset.ASCII, 1.0f);
        }
    }

    /// <summary>
    /// Clear internal state of charset detector.
    /// In the original interface this method is protected.
    /// </summary>
    public virtual void Reset()
    {
        _done = false;
        _start = true;
        _detectedCharset = Charset.Null;
        _gotData = false;
        _inputState = InputState.PureASCII;
        _lastChar = 0x00;
        _escCharsetProber?.Reset();
        for (int i = 0; i < PROBERS_NUM; i++)
        {
            _charsetProbers[i]?.Reset();
        }
    }

    protected abstract void Report(Charset charset, float confidence);
}
