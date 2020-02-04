/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

namespace FMScanner
{
    internal static class Ini
    {
        internal static FMIniData DeserializeFmIniLines(string[] lines)
        {
            var fmIni = new FMIniData();

            bool inDescr = false;

            // Quick n dirty, works fine and is fast
            foreach (string line in lines)
            {
                if (line.StartsWithI(nameof(fmIni.NiceName) + "="))
                {
                    inDescr = false;
                    fmIni.NiceName = line.Substring(line.IndexOf('=') + 1).Trim();
                }
                else if (line.StartsWithI(nameof(fmIni.ReleaseDate) + "="))
                {
                    inDescr = false;
                    fmIni.ReleaseDate = line.Substring(line.IndexOf('=') + 1).Trim();
                }
                else if (line.StartsWithI(nameof(fmIni.Tags) + "="))
                {
                    inDescr = false;
                    fmIni.Tags = line.Substring(line.IndexOf('=') + 1).Trim();
                }
                // Sometimes Descr values are literally multi-line. DON'T. DO. THAT. Use \n.
                // But I have to deal with it anyway.
                else if (line.StartsWithI(nameof(fmIni.Descr) + "="))
                {
                    inDescr = true;
                    fmIni.Descr = line.Substring(line.IndexOf('=') + 1).Trim();
                }
                else if (inDescr)
                {
                    fmIni.Descr += "\r\n" + line;
                }
            }

            if (!string.IsNullOrEmpty(fmIni.Descr)) fmIni.Descr = fmIni.Descr.Trim();

            return fmIni;
        }
    }
}
