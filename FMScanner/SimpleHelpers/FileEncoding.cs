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
    private const int DEFAULT_BUFFER_SIZE = ByteSize.KB * 128;

    private bool _started;
    private readonly int[] _encodingFrequency = new int[CharsetDetector.CharsetCount];
    // Stupid micro-optimization
    private bool _encodingFrequencyTouched;
    private readonly CharsetDetector _ude = new();
    private readonly CharsetDetector _singleUde = new();
    private Charset _encodingCharset;
    // Stupid micro-optimization to reduce GC time
    private readonly byte[] _buffer = new byte[ByteSize.KB * 16];
    private readonly byte[] _fileStreamBuffer = new byte[DEFAULT_BUFFER_SIZE];
    private bool _canBeASCII = true;
    // Biggest known FM readme as of 2023/03/28 is 56KB, so 100KB is way more than enough to not reallocate
    private readonly MemoryStreamFast _memoryStream = new(ByteSize.KB * 100);

    private static int GetCharsetCodePage(Charset charset) => CharsetDetector.CharsetToCodePage[(int)charset];

    /// <summary>
    /// Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputFilename">The input filename.</param>
    /// <returns>The detected encoding, or <see langword="null"/> if the detection failed.</returns>
    public Encoding? DetectFileEncoding(string inputFilename)
    {
        using var stream = GetReadModeFileStreamWithCachedBuffer(inputFilename, _fileStreamBuffer);
        return DetectFileEncoding(stream);
    }

    /// <summary>
    /// Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The detected encoding, or <see langword="null"/> if the detection failed.</returns>
    public Encoding? DetectFileEncoding(Stream inputStream)
    {
        try
        {
            Detect(inputStream);
            _encodingCharset = GetCurrentEncoding();
            return _encodingCharset == Charset.Null ? null : Encoding.GetEncoding(GetCharsetCodePage(_encodingCharset));
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

    /// <summary>
    /// Detects if contains textual data.
    /// </summary>
    /// <param name="rawData">The raw data.</param>
    /// <param name="count">The count.</param>
    private bool CheckForTextualData(byte[] rawData, int count)
    {
        if (rawData.Length < count || count < 4)
        {
            return true;
        }

        if (CharsetDetector.GetBOMCharset(rawData, rawData.Length) != Charset.Null)
        {
            return true;
        }

        // http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
        // http://www.gnu.org/software/diffutils/manual/html_node/Binary.html
        // count the number of null bytes sequences
        // considering only sequences of 2 0s: "\0\0" or control characters below 10
        int nullSequences = 0;
        int controlSequences = 0;
        for (int i = 1; i < count; i++)
        {
            // Fix(Fen): Any bytes >127 mean we can't be ASCII, period. But somewhere along the line, we're
            // deciding we can detect "ASCII" anyway, even if we have bytes >127. So set a bool to force us
            // to reject "ASCII" encoding if it's impossible.
            if (rawData[i - 1] > 127 || rawData[i] > 127)
            {
                _canBeASCII = false;
            }

            if (rawData[i - 1] == 0 && rawData[i] == 0)
            {
                if (++nullSequences > 1) break;
            }
            else if (rawData[i - 1] == 0 && rawData[i] < 10)
            {
                ++controlSequences;
            }
        }

        // is text if there is no null byte sequences or less than 10% of the buffer has control characters
        return nullSequences == 0 && controlSequences <= rawData.Length / 10;
    }

    private void Reset()
    {
        _started = false;
        _encodingFrequency.Clear();
        _encodingFrequencyTouched = false;
        _ude.Reset();
        _singleUde.Reset();
        _encodingCharset = Charset.Null;
        _canBeASCII = true;
    }

    /// <summary>
    /// Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <returns>Detected encoding name</returns>
    /// <exception cref="ArgumentOutOfRangeException">bufferSize parameter cannot be 0 or less.</exception>
    private void Detect(Stream inputData)
    {
        const int maxSize = ByteSize.MB * 20;
        const int bufferSize = ByteSize.KB * 16;

        const int maxIterations = maxSize / bufferSize;

        int i = 0;
        _buffer.Clear();
        while (i++ < maxIterations)
        {
            int sz = inputData.ReadAll(_buffer, 0, _buffer.Length);
            if (sz <= 0) break;

            Detect(_buffer, sz);
        }
    }

    /// <summary>
    /// Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <param name="count">The count.</param>
    /// <returns>Detected encoding name</returns>
    private void Detect(byte[] inputData, int count)
    {
        if (!_started)
        {
            Reset();
            _started = true;
            if (!CheckForTextualData(inputData, count))
            {
                return;
            }
        }

        // execute charset detector
        _ude.Run(inputData, 0, count, _memoryStream);
        if (_ude.IsDone() && _ude.Charset != Charset.Null)
        {
            IncrementFrequency(_ude.Charset);
            return;
        }

        // singular buffer detection
        _singleUde.Reset();
        const int udeFeedSize = ByteSize.KB * 4;
        int step = count < udeFeedSize ? count : udeFeedSize;
        for (int pos = 0; pos < count; pos += step)
        {
            _singleUde.Run(inputData, pos, pos + step > count ? count - pos : step, _memoryStream);
            if (_singleUde.Confidence > 0.3 && _singleUde.Charset != Charset.Null)
            {
                IncrementFrequency(_singleUde.Charset);
            }
        }
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
            ret = !_canBeASCII ? Charset.Windows1252 : Charset.UTF8;
        }

        return ret;
    }
}
