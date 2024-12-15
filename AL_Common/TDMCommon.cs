using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AL_Common;

public static partial class Common
{
    /*
    @TDM_NOTE(manual install filenames)
    If you go to an FM's download page, the pk4 is the identifying name plus an underscore and then a sha256
    hash code truncated to 16 characters. The game doesn't special-case this and doesn't strip the hash code or
    anything. The FM is just treated as having the name it has, so it will end up a separate entry. The game thus
    doesn't find it in the server's list and doesn't note the server version as being an update of the installed
    one, even if it is. So we can just ignore this whole issue and we match the game.

    @TDM_NOTE: FM name validation and conversion-to-valid notes:
    TDM does some checking and processing on FM names, to wit:
    -Converts it to lowercase if not already
    -Strips trailing .pk4 if it exists
    -Converts to underscores any chars that are NOT:
     -ASCII alphabetical; or
     -ASCII numeric; or
     -"Western European high-ascii chars" (0xC0 to 0xFF in Win1252 / ISO 8859-1 - both encodings are the same in
      that char range)
    
    -There don't appear to be any nonconforming internalNames on the server as of 2023-10-16.
     The wiki doesn't appear to say anything specifically about this, it just says you should make your FM's
     internal name conform to this logic (lowercase, ascii letters and numbers) and make sure it doesn't conflict
     with any other. It states that FMs are uploaded manually by a staff member, and so that's where the trail
     ends. I don't know if the server automatically processes/disambiguates/corrects the internal names.

    -These are the places the game does this conversion:
     -On getting the names from the server
     -On moving pk4s into directories (the directory and pk4 are converted to valid names)

    However, if an FM dir itself is named invalidly, it DOESN'T convert it, but treats it as a separate FM, new
    entry in mission.tdminfo and all.

    Also, when loading an FM, it appears to just load whatever pk4 is in the specified fm folder, no matter its
    name. So in all the following scenarios, the game will load Bakery Job just fine:
    -C:\darkmod\fms\bakery_job\bakery_job.pk4
    -C:\darkmod\fms\bakery_job\bakery;job.pk4
    -C:\darkmod\fms\bakery_job\totally_irrelevant_name.pk4

    @TDM_NOTE(pk4/zip):
    TDM supports zip files in the base FMs dir too, it moves them into their folder and renames them to .pk4.
    But it does NOT support zip files in FM folders! It will fail to find/load those. So we only need to support
    zips in the base FMs dir.

    -TDM finds files in order of pk4, then zip. It only takes files that have darkmod.txt in them (problematic
    for us - perf issue). It then moves them in order, overwriting previous ones if they exist. So, that means
    zips take priority over pk4s, although they all end up named .pk4 once in the FM folder, the pk4 there will
    have come from a zip if one existed.
    */

    private static bool IsValidTDMInternalNameChar(char c)
    {
        return
            char.IsAsciiLetter(c) || char.IsAsciiDigit(c) ||
            // "Western European high-ascii chars" (0xC0 to 0xFF in Win1252 / ISO 8859-1 - both encodings are the
            // same in that char range)
            (c >= 0xC0 && c <= 0xFF);
    }

    private static bool IsValidTDMInternalName(this string value, string extension)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if ((!char.IsAsciiLetter(c) && !char.IsAsciiDigit(c) && !(c >= 0xC0 && c <= 0xFF) && c != '_') || char.IsUpper(c))
            {
                return false;
            }
        }
        if (value.EndsWithO(extension))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// If no conversion is necessary, returns the current instance unchanged.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string ConvertToValidTDMInternalName(this string value, string extension)
    {
        if (value.IsValidTDMInternalName(extension)) return value;

        value = value.ToLowerInvariant();
        if (value.EndsWithO(extension))
        {
            value = value.RemoveExtension();
        }

        var sb = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            sb.Append(IsValidTDMInternalNameChar(c) ? c : '_');
        }

        return sb.ToString();
    }

    public static bool TryParseTDMDate(string dateString, out DateTime dateTime)
    {
        return DateTime.TryParseExact(
            dateString,
            "yyyy-M-d",
            DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None,
            out dateTime);
    }
}

public sealed class TDM_ServerFMData
{
    // string
    public string Title = "";

    // DateTime
    // This is always in the format yyyy-mm-dd
    public string ReleaseDate = "";

    private DateTime? _releaseDateDT;
    public DateTime? ReleaseDateDT
    {
        get
        {
            if (_releaseDateDT == null)
            {
                if (TryParseTDMDate(ReleaseDate, out DateTime result))
                {
                    _releaseDateDT = result;
                }
            }
            return _releaseDateDT;
        }
    }

    // probably int
    public string Version = "";

    // string
    private string _internalName = "";
    public string InternalName
    {
        get => _internalName;
        set => _internalName = value.ConvertToValidTDMInternalName(".pk4");
    }

    // string
    public string Author = "";
}

public sealed class TDM_LocalFMData
{
    public readonly string InternalName;

    public TDM_LocalFMData(string internalName)
    {
        InternalName = internalName;
    }

    public string DownloadedVersion = "";
    public string LastPlayDate = "";
    public bool MissionCompletedOnNormal;
    public bool MissionCompletedOnHard;
    public bool MissionCompletedOnExpert;
}

// @TDM_CASE: Case-sensitive dictionaries
public sealed class ScannerTDMContext
{
    public readonly string FMsPath;
    // @TDM_CASE: Case-insensitive dictionary
    public readonly DictionaryI<string> BaseFMsDirPK4Files;
    public readonly Dictionary<string, TDM_LocalFMData> LocalFMData;
    public readonly Dictionary<string, TDM_ServerFMData> ServerFMData;

    public ScannerTDMContext(
        string fmsPath,
        DictionaryI<string> baseFMsDirPK4Files,
        List<TDM_LocalFMData> localFMData,
        List<TDM_ServerFMData> serverFMData)
    {
        FMsPath = fmsPath;
        BaseFMsDirPK4Files = baseFMsDirPK4Files;

        LocalFMData = new Dictionary<string, TDM_LocalFMData>();
        ServerFMData = new Dictionary<string, TDM_ServerFMData>();

        foreach (TDM_LocalFMData item in localFMData)
        {
            LocalFMData[item.InternalName] = item;
        }
        foreach (TDM_ServerFMData item in serverFMData)
        {
            ServerFMData[item.InternalName] = item;
        }
    }

    public ScannerTDMContext(string fmsPath)
    {
        FMsPath = fmsPath;
        BaseFMsDirPK4Files = new DictionaryI<string>();

        LocalFMData = new Dictionary<string, TDM_LocalFMData>();
        ServerFMData = new Dictionary<string, TDM_ServerFMData>();
    }
}
