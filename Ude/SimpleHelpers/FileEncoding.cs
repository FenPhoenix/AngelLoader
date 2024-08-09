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

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AL_Common;
using static AL_Common.Common;

namespace Ude.NetStandard.SimpleHelpers;

public sealed class FileEncoding
{
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

    /// <summary>
    /// Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The detected encoding, or <see langword="null"/> if the detection failed.</returns>
    public Encoding? DetectFileEncoding(Stream inputStream)
    {
        try
        {
            const int maxSize = ByteSize.MB * 20;
            const int maxIterations = maxSize / _bufferSize;
            const int udeFeedSize = ByteSize.KB * 4;

            for (int i = 0; i < maxIterations; i++)
            {
                int bytesRead = inputStream.ReadAll(_buffer, 0, _bufferSize);
                if (bytesRead <= 0) break;

                if (i == 0)
                {
                    Charset bomCharset = CharsetDetector.GetBOMCharset(_buffer, bytesRead);
                    if (bomCharset != Charset.Null)
                    {
                        return CharsetDetector.CharsetToEncoding(bomCharset);
                    }
                }

                // execute charset detector
                _ude.Run(_buffer, 0, bytesRead, _udeContext);
                if (_ude.IsDone() && _ude.Charset != Charset.Null)
                {
                    IncrementFrequency(_ude.Charset);
                }
                else
                {
                    // singular buffer detection
                    _singleUde.Reset();
                    int step = bytesRead < udeFeedSize ? bytesRead : udeFeedSize;
                    for (int pos = 0; pos < bytesRead; pos += step)
                    {
                        _singleUde.Run(_buffer, pos, pos + step > bytesRead ? bytesRead - pos : step, _udeContext);
                        if (_singleUde.Confidence > 0.3 && _singleUde.Charset != Charset.Null)
                        {
                            IncrementFrequency(_singleUde.Charset);
                        }
                    }
                }
            }

            Charset charset = GetCurrentEncoding();
            return charset == Charset.Null ? null : Encoding.GetEncoding(CharsetDetector.GetCharsetCodePage(charset));
        }
        catch
        {
            return null;
        }
        finally
        {
            _encodingFrequency.Clear();
            _encodingFrequencyTouched = false;
            _ude.Reset();
            _singleUde.Reset();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            ret = _ude.DomainSpecificGuess switch
            {
                DomainSpecificGuess.UTF8 => Charset.UTF8,
                DomainSpecificGuess.CannotBeAscii => Charset.Windows1252,
                _ => Charset.UTF8,
            };
        }

        return ret;
    }
}
