/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2019 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FMScanner.SimpleHelpers;
using static FMScanner.Regexes;

namespace FMScanner
{
    internal static class Methods
    {
        internal static bool StringToDate(string dateString, out DateTime dateTime)
        {
            dateString = dateString.Replace(",", " ");
            dateString = Regex.Replace(dateString, @"\s+", @" ");
            dateString = Regex.Replace(dateString, @"\s+-\s+", "-");
            dateString = Regex.Replace(dateString, @"\s+/\s+", "/");

            // Remove "st", "nd", "rd, "th" if present, as DateTime.TryParse() will choke on them
            var match = DaySuffixesRegex.Match(dateString);
            if (match.Success)
            {
                var suffix = match.Groups["Suffix"];
                dateString = dateString.Substring(0, suffix.Index) +
                             dateString.Substring(suffix.Index + suffix.Length);
            }

            // We pass specific date formats to ensure that no field will be inferred: if there's no year, we
            // want to fail, and not assume the current year.
            var success = DateTime.TryParseExact(dateString, FMConstants.DateFormats,
                DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result);
            if (success)
            {
                dateTime = result;
                return true;
            }
            else
            {
                dateTime = new DateTime();
                return false;
            }
        }

        #region ReadAllLines / ReadAllText

        /// <summary>
        /// Reads all the lines in a stream, auto-detecting its encoding. Ensures non-ASCII characters show up
        /// correctly.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="length">The length of the stream in bytes.</param>
        /// <param name="streamIsSeekable">If true, the stream is used directly rather than copied, and is left
        /// open.</param>
        /// <returns></returns>
        internal static string[] ReadAllLinesE(Stream stream, long length, bool streamIsSeekable = false)
        {
            var lines = new List<string>();

            // Quick hack
            if (streamIsSeekable)
            {
                stream.Position = 0;

                var enc = FileEncoding.DetectFileEncoding(stream);

                stream.Position = 0;

                // Code page 1252 = Western European (using instead of Encoding.Default)
                using (var sr = new StreamReader(stream, enc ?? Encoding.GetEncoding(1252), false, 1024,
                    leaveOpen: true))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            else
            {
                // Detecting the encoding of a stream reads it forward some amount, and I can't seek backwards in an
                // archive stream, so I have to copy it to a seekable MemoryStream. Blah.
                using (var memStream = new MemoryStream((int)length))
                {
                    stream.CopyTo(memStream);
                    stream.Dispose();
                    memStream.Position = 0;
                    var enc = FileEncoding.DetectFileEncoding(memStream);
                    memStream.Position = 0;

                    using (var sr = new StreamReader(memStream, enc ?? Encoding.GetEncoding(1252), false))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                }
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Reads all the lines in a file, auto-detecting its encoding. Ensures non-ASCII characters show up
        /// correctly.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns></returns>
        internal static string[] ReadAllLinesE(string file)
        {
            var enc = FileEncoding.DetectFileEncoding(file);

            return File.ReadAllLines(file, enc ?? Encoding.GetEncoding(1252));
        }

        #endregion
    }
}
