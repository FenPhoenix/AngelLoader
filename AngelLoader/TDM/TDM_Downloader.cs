﻿// Test offline so as not to hit the server more than is necessary
#define ENABLE_ONLINE

#if ENABLE_ONLINE
using System.Net.Http;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AngelLoader;

/*
Note about XML:
InnerText (ie. <tag>Some text</tag>) needs manual unescaping, but Value (ie. <tag thing="Some text"/>)
doesn't. It gets automatically handled somehow.

If we have no internet connection, we fail immediately (thrown exception) and thus fall back to a local-only scan,
so no worries about a too-long timeout.
*/
internal static class TDM_Downloader
{
#if !ENABLE_ONLINE
    private const string _testingPath = @"C:\_al_tdm_testing";
    private static readonly string _detailsPath = Path.Combine(_testingPath, "mission_details");
#endif

    /*
    @TDM_NOTE(available missions urls):
    The game tries these two in order. They both seem to work reliably and appear to lead to the same xml file.
    But let's just match the game.
    */
    private static readonly string[] _availableMissionsUrls =
    {
        "http://missions.thedarkmod.com/get_available_missions.php",
        "http://missions.thedarkmod.com/available_missions.xml",
    };

    internal static async Task<(bool Success, bool Canceled, Exception? Ex, List<TDM_ServerFMData> FMsList)>
    TryGetMissionsFromServer(CancellationToken cancellationToken)
    {
        HttpResponseMessage? request = null;
        try
        {
            var fail = (false, false, (Exception?)null, new List<TDM_ServerFMData>());

#if ENABLE_ONLINE
            bool success = false;
            foreach (string url in _availableMissionsUrls)
            {
                request?.Dispose();
                request = await GlobalHttpClient.Instance.GetAsync(url, cancellationToken);
                if (request.IsSuccessStatusCode)
                {
                    success = true;
                    break;
                }
            }

            if (!success || request == null) return fail;

            using Stream dataStream = await request.Content.ReadAsStreamAsync();
#else
            using Stream dataStream = await Task.FromResult(File.OpenRead(Path.Combine(_testingPath, "_altdm__available_missions.xml")));
#endif

            XmlDocument xmlDoc = new();

            xmlDoc.Load(dataStream);

            using XmlNodeList? tdmNodes = xmlDoc.SelectNodes("tdm");
            if (tdmNodes?.Count != 1) return fail;

            XmlNode? tdmNode = tdmNodes[0];
            if (tdmNode == null) return fail;

            XmlNodeList availableMissionsNodes = tdmNode.ChildNodes;
            if (availableMissionsNodes.Count != 1) return fail;

            XmlNode? availableMissionsNode = availableMissionsNodes[0];
            if (availableMissionsNode == null) return fail;
            if (availableMissionsNode.Name != "availableMissions") return fail;

            XmlNodeList missionNodes = availableMissionsNode.ChildNodes;

            List<TDM_ServerFMData> fmsList = new(missionNodes.Count);

            foreach (XmlNode mn in missionNodes)
            {
                if (mn.Name == "mission" && mn.Attributes != null)
                {
                    string title = "";
                    string releaseDate = "";
                    string version = "";
                    string internalName = "";
                    string author = "";
                    foreach (XmlAttribute attr in mn.Attributes)
                    {
                        switch (attr.Name)
                        {
                            case "title":
                                title = attr.Value;
                                break;
                            case "releaseDate":
                                releaseDate = attr.Value;
                                break;
                            case "version":
                                version = attr.Value;
                                break;
                            case "internalName":
                                internalName = attr.Value;
                                break;
                            case "author":
                                author = attr.Value;
                                break;
                        }
                    }
                    fmsList.Add(new TDM_ServerFMData(title, releaseDate, version, internalName, author));
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
        finally
        {
            request?.Dispose();
        }
    }
}
