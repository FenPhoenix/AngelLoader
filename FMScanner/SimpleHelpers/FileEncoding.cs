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
 */

// Modified by FenPhoenix 2020 - perf and removal of stuff I'm not using
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FMScanner.SimpleHelpers
{
    public sealed class FileEncoding
    {
        private const int DEFAULT_BUFFER_SIZE = 128 * 1024;

        private bool _started;
        private bool _done;
        private readonly Dictionary<string, int> encodingFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Ude.CharsetDetector _ude = new Ude.CharsetDetector();
        private readonly Ude.CharsetDetector _singleUde = new Ude.CharsetDetector();
        private string? _encodingName;
        // Stupid micro-optimization to reduce GC time
        private readonly byte[] _buffer = new byte[16 * 1024];

        /// <summary>
        /// Tries to detect the file encoding.
        /// </summary>
        /// <param name="inputFilename">The input filename.</param>
        /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
        /// <returns></returns>
        public Encoding? DetectFileEncoding(string inputFilename, Encoding? defaultIfNotDetected = null)
        {
            using var stream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, DEFAULT_BUFFER_SIZE);
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
        private static bool CheckForTextualData(byte[] rawData, int start, int count)
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
            encodingFrequency.Clear();
            _ude.Reset();
            _singleUde.Reset();
            _encodingName = null;
        }

        /// <summary>
        /// Detects the encoding of textual data of the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <param name="maxSize">Size in byte of analysed data, if you want to analysed only a sample. Use 0 to read all stream data.</param>
        /// <param name="bufferSize">Size of the buffer for the stream read.</param>
        /// <returns>Detected encoding name</returns>
        /// <exception cref="ArgumentOutOfRangeException">bufferSize parameter cannot be 0 or less.</exception>
        private string? Detect(Stream inputData, int maxSize = 20 * 1024 * 1024, int bufferSize = 16 * 1024)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size cannot be 0 or less.");
            int maxIterations = maxSize <= 0 ? int.MaxValue : maxSize / bufferSize;
            int i = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
            while (i++ < maxIterations)
            {
                int sz = inputData.Read(_buffer, 0, _buffer.Length);
                if (sz <= 0) break;

                Detect(_buffer, 0, sz);
                if (_done) break;
            }
            Complete();
            return _encodingName;
        }

        /// <summary>
        /// Detects the encoding of textual data of the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <returns>Detected encoding name</returns>
        private string? Detect(byte[] inputData, int start, int count)
        {
            if (_done) return _encodingName;

            if (!_started)
            {
                Reset();
                _started = true;
                if (!CheckForTextualData(inputData, start, count))
                {
                    _done = true;
                    return _encodingName;
                }
            }

            // execute charset detector                
            _ude.Feed(inputData, start, count);
            _ude.DataEnd();
            if (_ude.IsDone() && !string.IsNullOrEmpty(_ude.Charset))
            {
                IncrementFrequency(_ude.Charset);
                _done = true;
                return _encodingName;
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
            return _encodingName;
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
            // check result
            return !string.IsNullOrEmpty(_encodingName) ? Encoding.GetEncoding(_encodingName) : null;
        }

        private void IncrementFrequency(string charset)
        {
            encodingFrequency.TryGetValue(charset, out int currentCount);
            encodingFrequency[charset] = ++currentCount;
        }

        private string? GetCurrentEncoding()
        {
            if (encodingFrequency.Count == 0) return null;
            // ASCII should be the last option, since other encodings often has ASCII included...
            return encodingFrequency
                    .OrderByDescending(i => i.Value * (i.Key != "ASCII" ? 1 : 0))
                    .FirstOrDefault().Key;
        }
    }
}
