﻿// Test offline so as not to hit the server more than is necessary
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
using System.Threading.Tasks;
using System.Xml;
using AL_Common;

namespace AngelLoader;

/*
@TDM: We don't want to download the info every time we scan, or too often in any case. We need a caching scheme.
@TDM: We shouldn't be downloading the entire list of FM details for the scan, only fetching the ones we actually need on the fly.

Note about XML:
InnerText (ie. <tag>Some text</tag>) needs manual unescaping, but Value (ie. <tag thing="Some text"/>)
doesn't. It gets automatically handled somehow.
*/
internal static class TDM_Downloader
{
    private const string _testingPath = @"C:\_al_tdm_testing";
    private static readonly string _detailsPath = Path.Combine(_testingPath, "mission_details");

    private static async Task<Stream> GetAvailableMissionsStream()
    {
#if ENABLE_ONLINE
        HttpResponseMessage request = await GlobalHttpClient.GetAsync("http://missions.thedarkmod.com/get_available_missions.php");
        request.EnsureSuccessStatusCode();
        return await request.Content.ReadAsStreamAsync();
#else
        return await Task.FromResult(File.OpenRead(Path.Combine(_testingPath, "_altdm__available_missions.xml")));
#endif
    }

    internal static async Task<(bool Success, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetMissionsFromServer()
    {
        try
        {
            var fail = (false, (Exception?)null, new List<TDM_ServerFMData>());

            using Stream dataStream = await GetAvailableMissionsStream();

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
                    if (mn.Attributes != null)
                    {
                        TDM_ServerFMData serverFMData = new();
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
                }
            }

            return (true, null, fmsList);
        }
        catch (Exception ex)
        {
            return (false, ex, new List<TDM_ServerFMData>());
        }
    }

    private static async Task<Stream> GetMissionDetailsStream(TDM_ServerFMData serverFMData)
    {
#if ENABLE_ONLINE
        HttpResponseMessage request =

        await GlobalHttpClient.GetAsync("http://missions.thedarkmod.com/get_mission_details.php?id=" + serverFMData.Id);
        request.EnsureSuccessStatusCode();
        return await request.Content.ReadAsStreamAsync();
#else
        return await Task.FromResult(File.OpenRead(Path.Combine(_detailsPath,
            serverFMData.InternalName + "_id=" + serverFMData.Id + ".xml")));
#endif
    }

    internal static async Task<(bool Success, Exception? Ex, TdmFmDetails FmDetails)>
    GetMissionDetails(TDM_ServerFMData serverFMData)
    {
        try
        {
            var fail = (false, (Exception?)null, new TdmFmDetails());

            using Stream dataStream = await GetMissionDetailsStream(serverFMData);

            var xmlDoc = new XmlDocument();

            xmlDoc.Load(dataStream);

            XmlNodeList? tdmNodes = xmlDoc.SelectNodes("tdm");
            if (tdmNodes?.Count != 1) return fail;

            XmlNode tdmNode = tdmNodes[0];

            XmlNodeList missionNodes = tdmNode.ChildNodes;
            if (missionNodes.Count != 1) return fail;

            XmlNode missionNode = missionNodes[0];
            if (missionNode.Name != "mission") return fail;

            TdmFmDetails details = new();
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
                            if (dlNode.Name != "downloadLocation") continue;
                            if (dlNode.Attributes == null) continue;

                            TdmFmDownloadLocation downloadLocation = new(details.InternalName);
                            foreach (XmlAttribute attr in dlNode.Attributes)
                            {
                                switch (attr.Name)
                                {
                                    case "language":
                                        downloadLocation.Language = attr.Value;
                                        break;
                                    case "weight":
                                        downloadLocation.Weight = attr.Value;
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

            return (true, null, details);
        }
        catch (Exception ex)
        {
            return (false, ex, new TdmFmDetails());
        }
    }

    /// <summary>
    /// Gets the unescaped InnerText.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPlainInnerText(this XmlNode node) => WebUtility.HtmlDecode(node.InnerText);


#if ENABLE_ONLINE
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
