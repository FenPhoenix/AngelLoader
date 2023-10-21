// Test offline so as not to hit the server more than is necessary
//#define ENABLE_ONLINE

#if ENABLE_ONLINE
using System.Net.Http;
using static AL_Common.Common;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AL_Common;
using static AngelLoader.GameSupport;

namespace AngelLoader;

/*
@TDM: We don't want to download the info every time we scan, or too often in any case. We need a caching scheme.

Note about XML:
InnerText (ie. <tag>Some text</tag>) needs manual unescaping, but Value (ie. <tag thing="Some text"/>)
doesn't. It gets automatically handled somehow.
*/
internal static class TDM_Downloader
{
    private const string _testingPath = @"C:\_al_tdm_testing";
    private static readonly string _detailsPath = Path.Combine(_testingPath, "mission_details");

    internal static Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetMissionsFromServer()
    {
        return TryGetMissionsFromServer(CancellationToken.None);
    }

    internal static async Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetMissionsFromServer(CancellationToken cancellationToken)
    {
        try
        {
            var fail = (false, false, (Exception?)null, new List<TDM_ServerFMData>());

#if ENABLE_ONLINE
            using HttpResponseMessage request = await GlobalHttpClient.GetAsync(
                "http://missions.thedarkmod.com/get_available_missions.php",
                cancellationToken);
            request.EnsureSuccessStatusCode();
            using Stream dataStream = await request.Content.ReadAsStreamAsync();
#else
            using Stream dataStream = await Task.FromResult(File.OpenRead(Path.Combine(_testingPath, "_altdm__available_missions.xml")));
#endif

            var xmlDoc = new XmlDocument();

            xmlDoc.Load(dataStream);

            XmlNodeList? tdmNodes = xmlDoc.SelectNodes("tdm");
            if (tdmNodes?.Count != 1) return fail;

            XmlNode tdmNode = tdmNodes[0];

            XmlNodeList availableMissionsNodes = tdmNode.ChildNodes;
            if (availableMissionsNodes.Count != 1) return fail;

            XmlNode availableMissionsNode = availableMissionsNodes[0];
            if (availableMissionsNode.Name != "availableMissions") return fail;

            XmlNodeList missionNodes = availableMissionsNode.ChildNodes;

            var fmsList = new List<TDM_ServerFMData>(missionNodes.Count);

            foreach (XmlNode mn in missionNodes)
            {
                if (mn.Name == "mission")
                {
                    TDM_ServerFMData serverFMData = new();
                    if (mn.Attributes != null)
                    {
                        foreach (XmlAttribute attr in mn.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "title":
                                    serverFMData.Title = attr.Value;
                                    break;
                                case "releaseDate":
                                    serverFMData.ReleaseDate = attr.Value;
                                    break;
                                case "size":
                                    serverFMData.Size = attr.Value;
                                    break;
                                case "version":
                                    serverFMData.Version = attr.Value;
                                    break;
                                case "internalName":
                                    serverFMData.InternalName = attr.Value;
                                    break;
                                case "type":
                                    serverFMData.Type = attr.Value;
                                    break;
                                case "author":
                                    serverFMData.Author = attr.Value;
                                    break;
                                case "id":
                                    serverFMData.Id = attr.Value;
                                    break;
                            }
                        }
                        fmsList.Add(serverFMData);
                    }
                    foreach (XmlNode dlNode in mn.ChildNodes)
                    {
                        if (dlNode.Attributes == null) continue;
                        if (dlNode.Name == "downloadLocation")
                        {
                            TDM_FMDownloadLocation downloadLocation = new(serverFMData.InternalName);
                            foreach (XmlAttribute attr in dlNode.Attributes)
                            {
                                switch (attr.Name)
                                {
                                    case "language":
                                        downloadLocation.Language = attr.Value;
                                        break;
                                    case "weight":
                                        if (float.TryParse(attr.Value, out float weight))
                                        {
                                            downloadLocation.Weight = weight;
                                        }
                                        break;
                                    case "sha256":
                                        downloadLocation.SHA256 = attr.Value;
                                        break;
                                    case "url":
                                        downloadLocation.Url = attr.Value;
                                        break;
                                }
                            }
                            serverFMData.DownloadLocations.Add(downloadLocation);
                        }
                        else if (dlNode.Name == "localisationPack")
                        {
                            TDM_FMLocalizationPack l10nPack = new(serverFMData.InternalName);
                            foreach (XmlAttribute attr in dlNode.Attributes)
                            {
                                switch (attr.Name)
                                {
                                    case "weight":
                                        if (float.TryParse(attr.Value, out float weight))
                                        {
                                            l10nPack.Weight = weight;
                                        }
                                        break;
                                    case "sha256":
                                        l10nPack.SHA256 = attr.Value;
                                        break;
                                    case "url":
                                        l10nPack.Url = attr.Value;
                                        break;
                                }
                            }
                            serverFMData.LocalizationPacks.Add(l10nPack);
                        }
                    }
                }
            }

            return (true, false, null, fmsList);
        }
        catch (OperationCanceledException)
        {
            return (false, true, null, new List<TDM_ServerFMData>());
        }
        catch (Exception ex)
        {
            return (false, false, ex, new List<TDM_ServerFMData>());
        }
    }

    internal static Task<(bool Success, bool Canceled, Exception? Ex, TDM_ServerFMDetails ServerFMDetails)>
    GetMissionDetails(TDM_ServerFMData serverFMData)
    {
        return GetMissionDetails(serverFMData, CancellationToken.None);
    }

    internal static async Task<(bool Success, bool Canceled, Exception? Ex, TDM_ServerFMDetails ServerFMDetails)>
    GetMissionDetails(TDM_ServerFMData serverFMData, CancellationToken cancellationToken)
    {
        try
        {
            var fail = (false, false, (Exception?)null, new TDM_ServerFMDetails());

#if ENABLE_ONLINE
            using HttpResponseMessage request = await GlobalHttpClient.GetAsync(
                "http://missions.thedarkmod.com/get_mission_details.php?id=" + serverFMData.Id,
                cancellationToken);
            request.EnsureSuccessStatusCode();
            using Stream dataStream = await request.Content.ReadAsStreamAsync();
#else
            using Stream dataStream = await Task.FromResult(File.OpenRead(Path.Combine(_detailsPath,
                serverFMData.InternalName + "_id=" + serverFMData.Id + ".xml")));
#endif

            var xmlDoc = new XmlDocument();

            xmlDoc.Load(dataStream);

            XmlNodeList? tdmNodes = xmlDoc.SelectNodes("tdm");
            if (tdmNodes?.Count != 1) return fail;

            XmlNode tdmNode = tdmNodes[0];

            XmlNodeList missionNodes = tdmNode.ChildNodes;
            if (missionNodes.Count != 1) return fail;

            XmlNode missionNode = missionNodes[0];
            if (missionNode.Name != "mission") return fail;

            TDM_ServerFMDetails details = new();
            XmlNodeList detailsNodes = missionNode.ChildNodes;
            foreach (XmlNode dn in detailsNodes)
            {
                switch (dn.Name)
                {
                    case "id":
                        details.Id = dn.GetPlainInnerText();
                        break;
                    case "title":
                        details.Title = dn.GetPlainInnerText();
                        break;
                    case "releaseDate":
                        details.ReleaseDate = dn.GetPlainInnerText();
                        break;
                    case "size":
                        details.Size = dn.GetPlainInnerText();
                        break;
                    case "version":
                        details.Version = dn.GetPlainInnerText();
                        break;
                    case "internalName":
                        details.InternalName = dn.GetPlainInnerText();
                        break;
                    case "type":
                        details.Type = dn.GetPlainInnerText();
                        break;
                    case "author":
                        details.Author = dn.GetPlainInnerText();
                        break;
                    case "description":
                        details.Description = dn.GetPlainInnerText();
                        break;
                    case "downloadLocations":
                    {
                        foreach (XmlNode dlNode in dn.ChildNodes)
                        {
                            if (dlNode.Attributes == null) continue;
                            if (dlNode.Name == "downloadLocation")
                            {
                                TDM_FMDownloadLocation downloadLocation = new(details.InternalName);
                                foreach (XmlAttribute attr in dlNode.Attributes)
                                {
                                    switch (attr.Name)
                                    {
                                        case "language":
                                            downloadLocation.Language = attr.Value;
                                            break;
                                        case "weight":
                                            if (float.TryParse(attr.Value, out float weight))
                                            {
                                                downloadLocation.Weight = weight;
                                            }
                                            break;
                                        case "sha256":
                                            downloadLocation.SHA256 = attr.Value;
                                            break;
                                        case "url":
                                            downloadLocation.Url = attr.Value;
                                            break;
                                    }
                                }
                                details.DownloadLocations.Add(downloadLocation);
                            }
                            else if (dlNode.Name == "localisationPack")
                            {
                                TDM_FMLocalizationPack l10nPack = new(details.InternalName);
                                foreach (XmlAttribute attr in dlNode.Attributes)
                                {
                                    switch (attr.Name)
                                    {
                                        case "weight":
                                            if (float.TryParse(attr.Value, out float weight))
                                            {
                                                l10nPack.Weight = weight;
                                            }
                                            break;
                                        case "sha256":
                                            l10nPack.SHA256 = attr.Value;
                                            break;
                                        case "url":
                                            l10nPack.Url = attr.Value;
                                            break;
                                    }
                                }
                                details.LocalizationPacks.Add(l10nPack);
                            }
                        }
                        break;
                    }
                    case "screenshots":
                    {
                        foreach (XmlNode dlNode in dn.ChildNodes)
                        {
                            if (dlNode.Name != "screenshot") continue;
                            if (dlNode.Attributes == null) continue;

                            foreach (XmlAttribute attr in dlNode.Attributes)
                            {
                                if (attr.Name == "path")
                                {
                                    details.Screenshots.Add(attr.Value);
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return (true, false, null, details);
        }
        catch (OperationCanceledException)
        {
            return (false, true, null, new TDM_ServerFMDetails());
        }
        catch (Exception ex)
        {
            return (false, false, ex, new TDM_ServerFMDetails());
        }
    }

    /// <summary>
    /// Gets the unescaped InnerText.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPlainInnerText(this XmlNode node) => WebUtility.HtmlDecode(node.InnerText);

    internal static async Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetServerDataWithUpdateInfo(CancellationToken cancellationToken)
    {
        var fail = (false, false, (Exception?)null, new List<TDM_ServerFMData>());

        try
        {
            string baseFMsPath = Global.Config.GetFMInstallPath(GameIndex.TDM);
            List<string> fmDirs = FastIO.GetDirsTopOnly(baseFMsPath, "*", returnFullPaths: false);
            var fmDirsDict = new Dictionary<string, string>();
            for (int i = 0; i < fmDirs.Count; i++)
            {
                string fmDir = fmDirs[i];
                if (fmDir != Paths.TDMMissionShots)
                {
                    fmDirsDict[fmDir] = Path.Combine(baseFMsPath, fmDir);
                }
            }

            // @TDM: Pass dicts along where needed, rather than recreate them all the time in different places

            (bool success, bool canceled, _, List<TDM_ServerFMData> serverDataList) =
                await TryGetMissionsFromServer(cancellationToken);
            if (success)
            {
                List<TDM_LocalFMData> localDataList = TDMParser.ParseMissionsInfoFile();
                var serverDataDict = new Dictionary<string, TDM_ServerFMData>();
                foreach (TDM_ServerFMData item in serverDataList)
                {
                    serverDataDict[item.InternalName] = item;
                }

                foreach (TDM_LocalFMData localData in localDataList)
                {
                    if (serverDataDict.TryGetValue(localData.InternalName, out TDM_ServerFMData serverData))
                    {
                        if (int.TryParse(serverData.Version, out int serverVersionInt) &&
                            int.TryParse(localData.DownloadedVersion, out int localVersionInt) &&
                            // @TDM: Should we compare higher only?
                            serverVersionInt != localVersionInt)
                        {
                            serverData.IsUpdate = true;
                        }

                        if (serverData.LocalizationPacks.Count > 0)
                        {
                            string pk4File = serverData.InternalName + ".pk4";
                            string pk4L10nFile = serverData.InternalName + "_l10n.pk4";

                            if (fmDirsDict.TryGetValue(serverData.InternalName, out string fmPath))
                            {
                                if (File.Exists(Path.Combine(fmPath, pk4L10nFile)) ||
                                    !File.Exists(Path.Combine(baseFMsPath, pk4L10nFile)))
                                {
                                    serverData.HasAvailableLanguagePack = true;
                                }
                            }
                            else if (File.Exists(Path.Combine(baseFMsPath, pk4File)) &&
                                     !File.Exists(Path.Combine(baseFMsPath, pk4L10nFile)))
                            {
                                serverData.HasAvailableLanguagePack = true;
                            }
                        }
                    }
                }

                return (true, false, null, serverDataList);
            }
            else if (canceled)
            {
                return (false, true, null, new List<TDM_ServerFMData>());
            }
            else
            {
                return fail;
            }
        }
        catch (OperationCanceledException)
        {
            return (false, true, null, new List<TDM_ServerFMData>());
        }
        catch (Exception ex)
        {
            return (false, false, ex, new List<TDM_ServerFMData>());
        }
    }


#if false
    internal static async Task SaveAllMissionDetailsXmlFiles(List<TDM_ServerFMData> infos)
    {
        foreach (TDM_ServerFMData info in infos)
        {
            HttpResponseMessage request = await GlobalHttpClient.GetAsync("http://missions.thedarkmod.com/get_mission_details.php?id=" + info.Id);
            request.EnsureSuccessStatusCode();
            byte[] bytes = await request.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(Path.Combine(_detailsPath, info.InternalName + "_id=" + info.Id + ".xml"), bytes);
        }
    }
#endif
}
