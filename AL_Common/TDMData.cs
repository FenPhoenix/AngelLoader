using System;
using System.Collections.Generic;
using System.Text;
using static AL_Common.Common;

namespace AL_Common;

public static partial class Common
{
    /*
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
    */

    internal static bool IsValidTDMInternalNameChar(char c)
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
    public string MissionCompleted0 = "";
    public string MissionCompleted1 = "";
    public string MissionCompleted2 = "";
}

public sealed class ScannerTDMContext
{
    public readonly Dictionary<string, TDM_LocalFMData> LocalFMData;
    public readonly Dictionary<string, TDM_ServerFMData> ServerFMData;

    public ScannerTDMContext(List<TDM_LocalFMData> localFMData, List<TDM_ServerFMData> serverFMData)
    {
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

    public ScannerTDMContext()
    {
        LocalFMData = new Dictionary<string, TDM_LocalFMData>();
        ServerFMData = new Dictionary<string, TDM_ServerFMData>();
    }
}
