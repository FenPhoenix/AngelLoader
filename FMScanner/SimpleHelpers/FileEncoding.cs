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

    Modified by FenPhoenix 2020-2022
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AL_Common;

namespace FMScanner.SimpleHelpers
{
    public sealed class FileEncoding
    {
        /*
        @CharEncoding: "Knife to the Heart" RUS_readme.txt encoding detection failure
        We flat-out fail to detect the encoding of this readme, which is Win-1251 (Cyrillic). This happens even
        if we bypass this code and just ask the UDE detector directly. However, Notepad++ detects this file's
        encoding correctly, and it's using a C version of the same detector (Mozilla Universal Charset Detector).
        Ude.NetStandard has a bug I guess(?) We should take a look at the code and see if we can fix it...

        UPDATE: 
        Actually Notepad++ is using some version of uchardet (by the looks of things, a really old version -
        https://github.com/BYVoid/uchardet probably, judging by the readme and the "BYvoid" name in NPP source
        /uchardet dir). So we should definitely switch to that, wrap or port as necessary.

        Links of interest:
        
        uchardet:
        https://www.freedesktop.org/wiki/Software/uchardet/
        Claims to be based on the old Mozilla UDE, but updated to be more accurate and support more encodings.
        -Maybe we can wrap this one or port it.

        UTF.Unknown:
        https://github.com/CharsetDetector/UTF-unknown
        Claims to be "based on UDE and uchardet", but based on the release dates it has to be uchardet 0.06 or
        something, rather than 0.07 (latest as of 2021-04-10).
        */

        private const int DEFAULT_BUFFER_SIZE = 128 * 1024;

        private bool _started;
        private bool _done;
        private readonly Dictionary<string, int> _encodingFrequency = new(StringComparer.Ordinal);
        private readonly Ude.CharsetDetector _ude = new();
        private readonly Ude.CharsetDetector _singleUde = new();
        private string? _encodingName;
        // Stupid micro-optimization to reduce GC time
        private readonly byte[] _buffer = new byte[16 * 1024];
        private bool _canBeASCII = true;

        /// <summary>
        /// Tries to detect the file encoding.
        /// </summary>
        /// <param name="inputFilename">The input filename.</param>
        /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
        /// <returns></returns>
        public Encoding? DetectFileEncoding(string inputFilename, Encoding? defaultIfNotDetected = null)
        {
            using var stream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read, FileShare.Read, DEFAULT_BUFFER_SIZE);
            return DetectFileEncoding(stream) ?? defaultIfNotDetected;
        }

        /// <summary>
        /// Tries to detect the file encoding.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
        /// <returns></returns>
        public Encoding? DetectFileEncoding(Stream inputStream, Encoding? defaultIfNotDetected = null)
        {
            Detect(inputStream);
            Encoding? ret = Complete() ?? defaultIfNotDetected;
            Reset();
            return ret;
        }

        /// <summary>
        /// Detects if contains textual data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        private bool CheckForTextualData(byte[] rawData, int start, int count)
        {
            if (rawData.Length < count || count < 4 || start + 1 >= count)
            {
                return true;
            }

            if (CheckForByteOrderMark(rawData, start))
            {
                return true;
            }

            // http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
            // http://www.gnu.org/software/diffutils/manual/html_node/Binary.html
            // count the number od null bytes sequences
            // considering only sequences of 2 0s: "\0\0" or control characters below 10
            int nullSequences = 0;
            int controlSequences = 0;
            for (int i = start + 1; i < count; i++)
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

        /// <summary>
        /// Detects if data has bytes order mark to indicate its encoding for textual data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        private static bool CheckForByteOrderMark(byte[] rawData, int start = 0)
        {
            if (rawData.Length - start < 4) return false;

            // Detect encoding correctly (from Rick Strahl's blog)
            // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
            if (rawData[start] == 0xef && rawData[start + 1] == 0xbb && rawData[start + 2] == 0xbf)
            {
                // Encoding.UTF8;
                return true;
            }
            else if (rawData[start] == 0xfe && rawData[start + 1] == 0xff)
            {
                // Encoding.Unicode;
                return true;
            }
            else if (rawData[start] == 0 && rawData[start + 1] == 0 && rawData[start + 2] == 0xfe && rawData[start + 3] == 0xff)
            {
                // Encoding.UTF32;
                return true;
            }
            else if (rawData[start] == 0x2b && rawData[start + 1] == 0x2f && rawData[start + 2] == 0x76)
            {
                // Encoding.UTF7;
                return true;
            }
            return false;
        }

        private void Reset()
        {
            _started = false;
            _done = false;
            _encodingFrequency.Clear();
            _ude.Reset();
            _singleUde.Reset();
            _encodingName = null;
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
            const int maxSize = 20 * 1024 * 1024;
            const int bufferSize = 16 * 1024;

            const int maxIterations = maxSize / bufferSize;

            int i = 0;
            _buffer.Clear();
            while (i++ < maxIterations)
            {
                int sz = inputData.ReadAll(_buffer, 0, _buffer.Length);
                if (sz <= 0) break;

                Detect(_buffer, 0, sz);
                if (_done) break;
            }
            Complete();
        }

        /// <summary>
        /// Detects the encoding of textual data of the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <returns>Detected encoding name</returns>
        private void Detect(byte[] inputData, int start, int count)
        {
            if (_done) return;

            if (!_started)
            {
                Reset();
                _started = true;
                if (!CheckForTextualData(inputData, start, count))
                {
                    _done = true;
                    return;
                }
            }

            // execute charset detector                
            _ude.Feed(inputData, start, count);
            _ude.DataEnd();
            if (_ude.IsDone() && !string.IsNullOrEmpty(_ude.Charset))
            {
                IncrementFrequency(_ude.Charset);
                _done = true;
                return;
            }

            // singular buffer detection
            _singleUde.Reset();
            const int udeFeedSize = 4 * 1024;
            int step = count - start < udeFeedSize ? count - start : udeFeedSize;
            for (int pos = start; pos < count; pos += step)
            {
                _singleUde.Feed(inputData, pos, pos + step > count ? count - pos : step);
                _singleUde.DataEnd();
                // update encoding frequency
                if (_singleUde.Confidence > 0.3 && !string.IsNullOrEmpty(_singleUde.Charset))
                {
                    IncrementFrequency(_singleUde.Charset);
                }
            }
            // vote for best encoding
            _encodingName = GetCurrentEncoding();
            // update current encoding name
        }

        /// <summary>
        /// Finalize detection phase and gets detected encoding name.
        /// </summary>
        /// <returns></returns>
        private Encoding? Complete()
        {
            _done = true;
            _ude.DataEnd();
            if (_ude.IsDone() && !string.IsNullOrEmpty(_ude.Charset))
            {
                _encodingName = _ude.Charset;
            }
            // vote for best encoding
            _encodingName = GetCurrentEncoding();

            /*
             Fen's Notes:
             @NET5: GetEncoding(string): string could be "utf-7" and then we throw on .NET 5
             https://docs.microsoft.com/en-us/dotnet/core/compatibility/corefx#utf-7-code-paths-are-obsolete
             Ude.NetStandard v1.2.0 at least does not appear to detect nor ever return the value "utf-7", and
             it doesn't deal with .NET Encoding objects either, it returns encoding names as strings. Still,
             let's put a guard check in here just for robustness in case we change to a different detector or
             whatever.

             There's another UTF-7 reference up in the byte order mark checker, but this is the only place where
             we create an Encoding object and return a value back to the caller (the public methods both get their
             values from here), so this one guard is the only one we need.
            */

            // check result
            return _encodingName.IsEmpty() || _encodingName.EqualsI("utf-7") ? null : Encoding.GetEncoding(_encodingName);
        }

        private void IncrementFrequency(string charset)
        {
            _encodingFrequency.TryGetValue(charset, out int currentCount);
            _encodingFrequency[charset] = ++currentCount;
        }

        private string? GetCurrentEncoding()
        {
            if (_encodingFrequency.Count == 0) return null;
            // ASCII should be the last option, since other encodings often has ASCII included...
            string? ret = _encodingFrequency
                    .OrderByDescending(static i => i.Value * (i.Key != "ASCII" ? 1 : 0))
                    .FirstOrDefault().Key;

            if (ret?.Equals("ASCII") == true)
            {
                // Somewhere along the line, someone is detecting "ASCII" without checking if all our bytes are
                // <=127, so if we've detected "ASCII" but we have byte >127 anywhere, just use what we feel is
                // the "most likely" codepage for 8-bit encodings, which we're just going to say is Windows-1252.
                // Also, if we detected ASCII, just return UTF-8 because that's an exact superset but modern, so
                // why not just get rid of any reference to ancient stuff if we can.
                ret = !_canBeASCII ? "Windows-1252" : "UTF-8";
            }

            return ret;
        }
    }
}
