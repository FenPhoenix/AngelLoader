using System;
using System.Collections.Generic;

namespace AL_Common;

public sealed class TdmFmInfo
{
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
    public string InternalName = "";

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

public sealed class TdmFmDownloadLocation
{
    // Not part of the xml item, but we store it for convenience
    public readonly string FMInternalName;

    // probably string (could be mapped to enum or something?)
    public string Language = "";

    // float/double
    public string Weight = "";

    // string
    public string SHA256 = "";

    // string
    public string Url = "";

    public TdmFmDownloadLocation(string fmInternalName)
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

public sealed class TdmFmDetails
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
    public string InternalName = "";

    // string
    // Type is either "single" or "multi", but we scan for exact mission count, so we won't use this normally.
    // But if we have a downloader, we could display it there, as the closest thing to mission count we have.
    public string Type = "";

    // string
    public string Author = "";

    // string
    public string Description = "";

    public List<TdmFmDownloadLocation> DownloadLocations = new();

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

        ret += "\t" + nameof(Screenshots) + ":" + Environment.NewLine;
        for (int i = 0; i < Screenshots.Count; i++)
        {
            ret += "\t\t" + Screenshots[i] + Environment.NewLine;
        }

        return ret;
    }
}

public sealed class MissionInfoEntry
{
    public readonly string InternalName;

    public MissionInfoEntry(string internalName)
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
