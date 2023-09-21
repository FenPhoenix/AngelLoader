#region *   License     *
/*
    SimpleHelpers - FileEncoding   

    Copyright © 2014 Khalid Salomão

    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the “Software”), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE. 

    License: http://www.opensource.org/licenses/mit-license.php
    Website: https://github.com/khalidsalomao/SimpleHelpers.Net

    Modified by FenPhoenix 2020-2023
 */
#endregion

using System;
using System.IO;
using System.Text;
using AL_Common;
using Ude.NetStandard;
using static AL_Common.Common;

namespace FMScanner.SimpleHelpers;

public sealed class FileEncoding
{
    private bool _started;
    private readonly int[] _encodingFrequency = new int[CharsetDetector.CharsetCount];
    // Stupid micro-optimization
    private bool _encodingFrequencyTouched;
    private readonly CharsetDetector _ude = new();
    private readonly CharsetDetector _singleUde = new();
    private const int _bufferSize = ByteSize.KB * 16;
    // Stupid micro-optimization to reduce GC time
    private readonly byte[] _buffer = new byte[_bufferSize];
    private readonly UdeContext _udeContext;

    public FileEncoding() => _udeContext = new UdeContext(4096);

    public FileEncoding(int contextSize) => _udeContext = new UdeContext(contextSize);

    private static int GetCharsetCodePage(Charset charset) => CharsetDetector.CharsetToCodePage[(int)charset];

    /// <summary>
    /// Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The detected encoding, or <see langword="null"/> if the detection failed.</returns>
    public Encoding? DetectFileEncoding(Stream inputStream)
    {
        try
        {
            Charset bomCharset = Detect(inputStream);
            if (bomCharset != Charset.Null)
            {
                return bomCharset switch
                {
                    Charset.UTF8 => Encoding.UTF8,
                    Charset.UTF16LE => Encoding.Unicode,
                    Charset.UTF16BE => Encoding.BigEndianUnicode,
                    Charset.UTF32LE => Encoding.UTF32,
                    Charset.UTF32BE => Encoding.GetEncoding(GetCharsetCodePage(Charset.UTF32BE)),
                    _ => Encoding.UTF8
                };
            }
            Charset finalCharset = GetCurrentEncoding();
            return finalCharset == Charset.Null ? null : Encoding.GetEncoding(GetCharsetCodePage(finalCharset));
        }
        catch
        {
            return null;
        }
        finally
        {
            Reset();
        }
    }

    private void Reset()
    {
        _started = false;
        _encodingFrequency.Clear();
        _encodingFrequencyTouched = false;
        _ude.Reset();
        _singleUde.Reset();
    }

    /// <summary>
    /// Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <returns>If charset was detected from BOM, returns that charset; otherwise, returns <see cref="Charset.Null"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">bufferSize parameter cannot be 0 or less.</exception>
    private Charset Detect(Stream inputData)
    {
        const int maxSize = ByteSize.MB * 20;
        const int maxIterations = maxSize / _bufferSize;

        int i = 0;
        while (i++ < maxIterations)
        {
            int sz = inputData.ReadAll(_buffer, 0, _bufferSize);
            if (sz <= 0) break;

            Charset bomCharset = Detect(_buffer, sz);
            if (bomCharset != Charset.Null)
            {
                return bomCharset;
            }
        }

        return Charset.Null;
    }

    /// <summary>
    /// Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <param name="count">The count.</param>
    /// <returns>If charset was detected from BOM, returns that charset; otherwise, returns <see cref="Charset.Null"/>.</returns>
    private Charset Detect(byte[] inputData, int count)
    {
        if (!_started)
        {
            _started = true;
            Charset charSet = CharsetDetector.GetBOMCharset(inputData, count);
            if (charSet != Charset.Null)
            {
                return charSet;
            }
        }

        // execute charset detector
        _ude.Run(inputData, 0, count, _udeContext);
        if (_ude.IsDone() && _ude.Charset != Charset.Null)
        {
            IncrementFrequency(_ude.Charset);
            return Charset.Null;
        }

        // singular buffer detection
        _singleUde.Reset();
        const int udeFeedSize = ByteSize.KB * 4;
        int step = count < udeFeedSize ? count : udeFeedSize;
        for (int pos = 0; pos < count; pos += step)
        {
            _singleUde.Run(inputData, pos, pos + step > count ? count - pos : step, _udeContext);
            if (_singleUde.Confidence > 0.3 && _singleUde.Charset != Charset.Null)
            {
                IncrementFrequency(_singleUde.Charset);
            }
        }

        return Charset.Null;
    }

    private void IncrementFrequency(Charset charset)
    {
        // Fen: Matching original behavior with ranking ASCII the lowest always
        _encodingFrequency[(int)charset] = charset == Charset.ASCII ? -1 : _encodingFrequency[(int)charset] + 1;
        _encodingFrequencyTouched = true;
    }

    private Charset GetCurrentEncoding()
    {
        if (!_encodingFrequencyTouched) return Charset.Null;

        // ASCII should be the last option, since other encodings often has ASCII included...
        // Fen: Matching original behavior of pushing ASCII to the bottom
        int maxFreq = 0;
        int maxFreqIndex = -1;
        int foundCount = 0;
        bool foundAscii = false;
        for (int i = 0; i < _encodingFrequency.Length; i++)
        {
            int freq = _encodingFrequency[i];
            // For future debug
            //Trace.WriteLine(((Charset)i) + " (count: " + freq + ")");
            if (freq != 0)
            {
                if (freq == -1)
                {
                    foundAscii = true;
                }
                else if (freq > maxFreq)
                {
                    maxFreq = freq;
                    maxFreqIndex = i;
                }
                foundCount++;
            }
        }

        Charset ret = foundCount == 1 && foundAscii ? Charset.ASCII : (Charset)maxFreqIndex;

        if (ret == Charset.ASCII)
        {
            /*
            Fen: Somewhere along the line, someone is detecting "ASCII" without checking if all our bytes are
            <=127, so if we've detected "ASCII" but we have byte >127 anywhere, just use what we feel is the
            "most likely" codepage for 8-bit encodings, which we're just going to say is Windows-1252. Also,
            if we detected ASCII, just return UTF-8 because that's an exact superset but modern, so why not
            just get rid of any reference to ancient stuff if we can.
            */
            ret = !_ude.CanBeASCII ? Charset.Windows1252 : Charset.UTF8;
        }

        return ret;
    }
}
