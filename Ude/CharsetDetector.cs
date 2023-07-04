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

public enum Charset
{
    Null,
    ASCII,
    Big5,
    EUC_JP,
    EUC_KR,
    HZ_GB_2312,
    IBM855,
    IBM866,
    ISO_2022_JP,
    ISO_2022_KR,
    ISO_8859_5,
    ISO_8859_7,
    ISO_8859_8, // Visual Hebrew
    KOI8_R,
    ShiftJIS,
    UTF16BE,
    UTF16LE,
    UTF32BE,
    UTF32LE,
    UTF8,
    GB_18030,
    Windows1251,
    Windows1252,
    Windows1253,
    Windows1255, // Logical Hebrew
    X_MAC_CYRILLIC
}

/// <summary>
/// <para>
/// Default implementation of charset detection interface.
/// The detector can be fed by a System.IO.Stream:
/// <example>
/// <code>
/// using (FileStream fs = File.OpenRead(filename)) {
///    CharsetDetector cdet = new CharsetDetector();
///    cdet.Feed(fs);
///    cdet.DataEnd();
///    Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
/// </code>
/// </example>
/// </para>
/// <para> or by a byte a array:</para>
/// <para>
/// <example>
/// <code>
/// byte[] buff = new byte[1024];
/// int read;
/// while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
///     Feed(buff, 0, read);
/// cdet.DataEnd();
/// Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
/// </code>
/// </example>
/// </para>
/// </summary>
public sealed class CharsetDetector
{
    // @Ude: Static array, probably we want to keep it static because we use it all the time
    // IMPORTANT: These must be in the exact same order as the Ude.Charset enum members! (Done for perf)
    public static readonly int[] CharsetToCodePage =
    {
        -1,
        20127, // "ASCII"
        950,   // "Big5"
        51932, // "EUC-JP"
        51949, // "EUC-KR"
        52936, // "HZ-GB-2312"
        855,   // "IBM855"
        866,   // "IBM866"
        50220, // "ISO-2022-JP"
        50225, // "ISO-2022-KR"
        28595, // "ISO-8859-5"
        28597, // "ISO-8859-7"
        28598, // "ISO-8859-8" // Visual Hebrew
        20866, // "KOI8-R"
        932,   // "Shift-JIS"
        1201,  // "UTF-16BE"
        1200,  // "UTF-16LE"
        12001, // "UTF-32BE"
        12000, // "UTF-32LE"
        65001, // "UTF-8"
        54936, // "gb18030"
        1251,  // "windows-1251"
        1252,  // "windows-1252"
        1253,  // "windows-1253"
        1255,  // "windows-1255" // Logical Hebrew
        10007  // "x-mac-cyrillic"
    };

    public static readonly int CharsetCount = CharsetToCodePage.Length;

    private enum InputState { PureASCII = 0, EscASCII = 1, HighByte = 2 }

    private const float MINIMUM_THRESHOLD = 0.20f;

    private InputState _inputState = InputState.PureASCII;
    private bool _start = true;
    private bool _gotData;
    private bool _done;
    private byte _lastChar;
    private const int PROBERS_NUM = 3;
    private readonly CharsetProber?[] _charsetProbers = new CharsetProber?[PROBERS_NUM];
    private EscCharsetProber? _escCharsetProber;
    private Charset _detectedCharset;

#if false
    public void Feed(Stream stream)
    {
        byte[] buff = new byte[1024];
        int read;
        while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !_done)
        {
            Feed(buff, 0, read);
        }
    }
#endif

    public bool IsDone() => _done;

    public void Reset()
    {
        Charset = Charset.Null;
        Confidence = 0.0f;

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

    public Charset Charset;

    public float Confidence;

    private void Report(Charset charset, float confidence)
    {
        Charset = charset;
        Confidence = confidence;
    }

    public static Charset GetBOMCharset(byte[] buf, int len)
    {
        return len >= 2
            ? buf[0] switch
            {
                0xFE when buf[1] == 0xFF => Charset.UTF16BE,
                0xFF when buf[1] == 0xFE => len < 4 || buf[2] != 0x00 || buf[3] != 0x00
                    ? Charset.UTF16LE
                    : Charset.UTF32LE,
                0xEF when len >= 3 && buf[1] == 0xBB && buf[2] == 0xBF => Charset.UTF8,
                0x00 when len >= 4 && buf[1] == 0x00 && buf[2] == 0xFE && buf[3] == 0xFF => Charset.UTF32BE,
                _ => Charset.Null
            }
            : Charset.Null;
    }

    public void Run(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        Feed(buf, offset, len, memoryStream);
        DataEnd();
    }

    private void Feed(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
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

            Charset bomCharset = GetBOMCharset(buf, len);
            if (bomCharset != Charset.Null) _detectedCharset = bomCharset;

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

        switch (_inputState)
        {
            case InputState.EscASCII:
            {
                _escCharsetProber ??= new EscCharsetProber();
                if (_escCharsetProber.HandleData(buf, offset, len) == ProbingState.FoundIt)
                {
                    _done = true;
                    _detectedCharset = _escCharsetProber.DetectedCharset;
                }
                break;
            }
            case InputState.HighByte:
            {
                for (int i = 0; i < PROBERS_NUM; i++)
                {
                    CharsetProber? prober = _charsetProbers[i];
                    if (prober?.HandleData(buf, offset, len, memoryStream) == ProbingState.FoundIt)
                    {
                        _done = true;
                        _detectedCharset = prober.GetCharsetName();
                        return;
                    }
                }
                break;
            }
            // default: pure ascii
        }
    }

    /// <summary>
    /// Notify detector that no further data is available.
    /// </summary>
    private void DataEnd()
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
}
