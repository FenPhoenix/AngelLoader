using System;
using System.Collections.Generic;
using System.Text;
using static AL_Common.Common;

namespace AL_Common;

public static partial class Common
{
    /*
    @TDM(manual install filenames)
    If you go to an FM's download page, the pk4 is the identifying name plus an underscore and then a sha256
    hash code truncated to 16 characters. The game doesn't special-case this and doesn't strip the hash code or
    anything. The FM is just treated as having the name it has, so it will end up a separate entry. The game thus
    doesn't find it in the server's list and doesn't note the server version as being an update of the installed
    one, even if it is. So we can just ignore this whole issue and we match the game.

    @TDM(internalName validation and conversion-to-valid):
    TDM does some checking and processing on internalNames from the server, to wit:
    -Converts it to lowercase if not already
    -Strips trailing .pk4 if it exists
    -Converts to underscores any chars that are NOT:
     -ASCII alphabetical; or
     -ASCII numeric; or
     -"Western European high-ascii chars" (0xC0 to 0xFF in Win1252 / ISO 8859-1 - both encodings are the same in
      that char range)
    
    There don't appear to be any nonconforming internalNames on the server as of 2023-10-16. I suspect they're
    also doing the conversion server-side in the first place. Should we match the game on this?
    Converting names would lead to all manner of nasty potential corner cases.

    The wiki doesn't appear to say anything specifically about this, it just says you should make your FM's
    internal name conform to this logic (lowercase, ascii letters and numbers) and make sure it doesn't conflict
    with any other. It states that FMs are uploaded manually by a staff member, and so that's where the trail
    ends. I don't know if the server automatically processes/disambiguates/corrects the internal names.

    We should probably just do what the game does, and if we have any problems then so will the game, so meh.

    @TDM: The game also does this when reading from disk...
    Specifically, it's for pk4s in the base fms dir, it converts the name when it moves the pk4 into its own dir.
    However, if an fm dir is named eg. "bakery;job" ,then it leaves the name alone and writes "bakery;job" out to
    missions.tdminfo as a separate entry from "bakery_job".

    So to match it, we need to consider "bakery;job.pk4" equal to "bakery_job" when reading the on-disk FMs.
    */

    private static bool IsValidTDMInternalNameChar(char c)
    {
        return
            c.IsAsciiAlpha() || c.IsAsciiNumeric() ||
            // "Western European high-ascii chars" (0xC0 to 0xFF in Win1252 / ISO 8859-1 - both encodings are the
            // same in that char range)
            (c >= 0xC0 && c <= 0xFF);
    }

    private static bool IsValidTDMInternalName(this string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if ((!c.IsAsciiAlpha() && !c.IsAsciiNumeric() && !(c >= 0xC0 && c <= 0xFF) && c != '_') || char.IsUpper(c))
            {
                return false;
            }
        }
        if (value.EndsWithO(".pk4"))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// If no conversion is necessary, returns the current instance unchanged.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ConvertToValidTDMInternalName(this string value)
    {
        if (value.IsValidTDMInternalName()) return value;

        value = value.ToLowerInvariant();
        if (value.EndsWithO(".pk4"))
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
        set => _internalName = value.ConvertToValidTDMInternalName();
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
