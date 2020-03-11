/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;
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
            Match match = DaySuffixesRegex.Match(dateString);
            if (match.Success)
            {
                Group suffix = match.Groups["Suffix"];
                dateString = dateString.Substring(0, suffix.Index) +
                             dateString.Substring(suffix.Index + suffix.Length);
            }

            // We pass specific date formats to ensure that no field will be inferred: if there's no year, we
            // want to fail, and not assume the current year.
            bool success = DateTime.TryParseExact(dateString, FMConstants.DateFormats,
                DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime result);
            dateTime = success ? result : new DateTime();
            return success;
        }
    }
}
