using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

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

    @TDM: Eventually remove this test and add the actual conversion code with this logic (if we decide to).
    */
    public static void Test_ValidateInternalName(string value)
    {
        bool error = false;

        foreach (char c in value)
        {
            if (c.IsAsciiAlpha() || c.IsAsciiNumeric() || (c >= 0xC0 && c <= 0xFF))
            {
                continue;
            }
            if (c != '_')
            {
                error = true;
                break;
            }
        }
        if (value.EndsWith(".pk4", StringComparison.OrdinalIgnoreCase))
        {
            error = true;
        }

        foreach (char c in value)
        {
            if (char.IsUpper(c))
            {
                error = true;
                break;
            }
        }

        if (error)
        {
            Trace.WriteLine("************************* Bad internalName: " + value);
        }
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
                if (DateTime.TryParseExact(ReleaseDate, "yyyy-M-d",
                        DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime result))
                {
                    _releaseDateDT = result;
                }
            }
            return _releaseDateDT;
        }
    }

    // float, or double for safety?
    public string Size = "";

    // probably int
    public string Version = "";

    // string
    private string _internalName = "";
    public string InternalName
    {
        get => _internalName;
        set
        {
            Common.Test_ValidateInternalName(value);
            _internalName = value;
        }
    }

    // string
    // Type is either "single" or "multi", but we scan for exact mission count, so we won't use this normally.
    // But if we have a downloader, we could display it there, as the closest thing to mission count we have.
    public string Type = "";

    // string
    public string Author = "";

    // probably int
    public string Id = "";

    public override string ToString()
    {
        return
            nameof(InternalName) + ": " + InternalName + Environment.NewLine +
            "\t" + nameof(Title) + ": " + Title + Environment.NewLine +
            "\t" + nameof(ReleaseDate) + ": " + ReleaseDate + Environment.NewLine +
            "\t" + nameof(Size) + ": " + Size + Environment.NewLine +
            "\t" + nameof(Version) + ": " + Version + Environment.NewLine +
            "\t" + nameof(Type) + ": " + Type + Environment.NewLine +
            "\t" + nameof(Author) + ": " + Author + Environment.NewLine +
            "\t" + nameof(Id) + ": " + Id + Environment.NewLine;
    }
}

public interface IWeighted
{
    float Weight { get; }
}

public interface IChecksum
{
    string SHA256 { get; }
}

public sealed class TDM_FMDownloadLocation : IWeighted, IChecksum
{
    // Not part of the xml item, but we store it for convenience
    public readonly string FMInternalName;

    // probably string (could be mapped to enum or something?)
    public string Language = "";

    // float/double
    public float Weight { get; set; }

    // string
    public string SHA256 { get; set; } = "";

    // string
    public string Url = "";

    public TDM_FMDownloadLocation(string fmInternalName)
    {
        FMInternalName = fmInternalName;
    }

    public override string ToString()
    {
        return
            FMInternalName + " Download Location:" + Environment.NewLine +
            "\t\t" + nameof(Language) + ": " + Language + Environment.NewLine +
            "\t\t" + nameof(Weight) + ": " + Weight + Environment.NewLine +
            "\t\t" + nameof(SHA256) + ": " + SHA256 + Environment.NewLine +
            "\t\t" + nameof(Url) + ": " + Url + Environment.NewLine;
    }
}

public sealed class TDM_FMLocalizationPack : IWeighted, IChecksum
{
    // Not part of the xml item, but we store it for convenience
    public readonly string FMInternalName;

    // float/double
    public float Weight { get; set; }

    // string
    public string SHA256 { get; set; } = "";

    // string
    public string Url = "";

    public TDM_FMLocalizationPack(string fmInternalName)
    {
        FMInternalName = fmInternalName;
    }

    public override string ToString()
    {
        return
            FMInternalName + " Localization Pack:" + Environment.NewLine +
            "\t\t" + nameof(Weight) + ": " + Weight + Environment.NewLine +
            "\t\t" + nameof(SHA256) + ": " + SHA256 + Environment.NewLine +
            "\t\t" + nameof(Url) + ": " + Url + Environment.NewLine;
    }
}

public sealed class TDM_ServerFMDetails
{
    // probably int
    public string Id = "";

    // Change these to their appropriate types later
    public string Title = "";

    // DateTime
    // This is always in the format yyyy-mm-dd
    public string ReleaseDate = "";

    // float, or double for safety?
    public string Size = "";

    // probably int
    public string Version = "";

    // string
    private string _internalName = "";
    public string InternalName
    {
        get => _internalName;
        set
        {
            Common.Test_ValidateInternalName(value);
            _internalName = value;
        }
    }

    // string
    // Type is either "single" or "multi", but we scan for exact mission count, so we won't use this normally.
    // But if we have a downloader, we could display it there, as the closest thing to mission count we have.
    public string Type = "";

    // string
    public string Author = "";

    // string
    public string Description = "";

    public List<TDM_FMDownloadLocation> DownloadLocations = new();
    public List<TDM_FMLocalizationPack> LocalizationPacks = new();

    public List<string> Screenshots = new();

    public override string ToString()
    {
        string ret =
            "\t" + nameof(Id) + ": " + Id + Environment.NewLine +
            "\t" + nameof(Title) + ": " + Title + Environment.NewLine +
            "\t" + nameof(ReleaseDate) + ": " + ReleaseDate + Environment.NewLine +
            "\t" + nameof(Size) + ": " + Size + Environment.NewLine +
            "\t" + nameof(Version) + ": " + Version + Environment.NewLine +
            "\t" + nameof(InternalName) + ": " + InternalName + Environment.NewLine +
            "\t" + nameof(Type) + ": " + Type + Environment.NewLine +
            "\t" + nameof(Author) + ": " + Author + Environment.NewLine +
            "\t" + nameof(Description) + ": " + Description + Environment.NewLine +
            "\t" + nameof(DownloadLocations) + ":" + Environment.NewLine;

        for (int i = 0; i < DownloadLocations.Count; i++)
        {
            ret += "\t" + DownloadLocations[i] + Environment.NewLine;
        }

        ret += "\t" + nameof(LocalizationPacks) + ":" + Environment.NewLine;
        for (int i = 0; i < LocalizationPacks.Count; i++)
        {
            ret += "\t" + LocalizationPacks[i] + Environment.NewLine;
        }

        ret += "\t" + nameof(Screenshots) + ":" + Environment.NewLine;
        for (int i = 0; i < Screenshots.Count; i++)
        {
            ret += "\t\t" + Screenshots[i] + Environment.NewLine;
        }

        return ret;
    }
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
    public string MissionLootCollected0 = "";
    public string MissionLootCollected1 = "";
    public string MissionLootCollected2 = "";

    public override string ToString()
    {
        return
            nameof(InternalName) + ": " + InternalName + Environment.NewLine +
            "\t" + nameof(DownloadedVersion) + ": " + DownloadedVersion + Environment.NewLine +
            "\t" + nameof(LastPlayDate) + ": " + LastPlayDate + Environment.NewLine +
            "\t" + nameof(MissionCompleted0) + ": " + MissionCompleted0 + Environment.NewLine +
            "\t" + nameof(MissionCompleted1) + ": " + MissionCompleted1 + Environment.NewLine +
            "\t" + nameof(MissionCompleted2) + ": " + MissionCompleted2 + Environment.NewLine +
            "\t" + nameof(MissionLootCollected0) + ": " + MissionLootCollected0 + Environment.NewLine +
            "\t" + nameof(MissionLootCollected1) + ": " + MissionLootCollected1 + Environment.NewLine +
            "\t" + nameof(MissionLootCollected2) + ": " + MissionLootCollected2 + Environment.NewLine;
    }
}

public sealed class ScannerTDMContext
{
    /*
    @TDM(Case-sensitivity in filenames):
    Since TDM also has a Linux version, there may be a question of how it treats casing of fm names. Do we need
    case-sensitivity here (and everywhere else) or should we do case-insensitive since we're Windows?
    */
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
