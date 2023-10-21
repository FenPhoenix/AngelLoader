// Test offline so as not to hit the server more than is necessary
#define ENABLE_ONLINE

#if ENABLE_ONLINE
using System.Net.Http;
using static AL_Common.Common;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AL_Common;

namespace AngelLoader;

/*
@TDM: We don't want to download the info every time we scan, or too often in any case. We need a caching scheme.

Note about XML:
InnerText (ie. <tag>Some text</tag>) needs manual unescaping, but Value (ie. <tag thing="Some text"/>)
doesn't. It gets automatically handled somehow.
*/
internal static class TDM_Downloader
{
#if !ENABLE_ONLINE
    private const string _testingPath = @"C:\_al_tdm_testing";
    private static readonly string _detailsPath = Path.Combine(_testingPath, "mission_details");
#endif

    internal static Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetMissionsFromServer()
    {
        return TryGetMissionsFromServer(CancellationToken.None);
    }

    private static async Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
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
                                case "version":
                                    serverFMData.Version = attr.Value;
                                    break;
                                case "internalName":
                                    serverFMData.InternalName = attr.Value;
                                    break;
                                case "author":
                                    serverFMData.Author = attr.Value;
                                    break;
                            }
                        }
                        fmsList.Add(serverFMData);
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
}
