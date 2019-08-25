using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Forms.Import
{
    internal static class ImportCommon
    {
        internal const string DarkLoaderIni = "DarkLoader.ini";

        internal static string AutodetectDarkLoaderIni()
        {
            // Common locations. Don't go overboard and search the whole filesystem; that would take forever.
            var dlLocations = new[]
            {
                @"DarkLoader",
                @"Games\DarkLoader"
            };

            DriveInfo[] drives;
            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch (Exception ex)
            {
                Log("Exception in GetDrives()", ex);
                return "";
            }

            foreach (var drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;

                try
                {
                    foreach (var loc in dlLocations)
                    {
                        var dlIni = Path.Combine(drive.Name, loc, DarkLoaderIni);
                        if (File.Exists(dlIni)) return dlIni;
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in DarkLoader multi-drive search", ex);
                }
            }

            return "";
        }
    }
}
