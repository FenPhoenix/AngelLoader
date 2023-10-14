// Test offline so as not to hit the server more than is necessary
// #define ENABLE_ONLINE

#if ENABLE_ONLINE
using System.Net.Http;
using static AL_Common.Common;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace AngelLoader;

internal static class TDM_Downloader
{
    internal sealed class TdmFmInfo
    {
        // Change these to their appropriate types later
        internal string Title = "";

        // DateTime
        // This is always in the format yyyy-mm-dd
        internal string ReleaseDate = "";

        // float, or double for safety?
        internal string Size = "";

        // probably int
        internal string Version = "";

        internal string InternalName = "";

        // Type is either "single" or "multi", but we scan for exact mission count, so we won't use this normally.
        // But if we have a downloader, we could display it there, as the closest thing to mission count we have.
        internal string Type = "";

        internal string Author = "";

        // probably int
        internal string Id = "";

        public TdmFmInfo()
        {

        }

        public TdmFmInfo(
            string title,
            string releaseDate,
            string size,
            string version,
            string internalName,
            string type,
            string author,
            string id)
        {
            Title = title;
            ReleaseDate = releaseDate;
            Size = size;
            Version = version;
            InternalName = internalName;
            Type = type;
            Author = author;
            Id = id;
        }

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

    private static async Task<Stream> GetAvailableMissionsStream()
    {
#if ENABLE_ONLINE
        HttpResponseMessage request = await GlobalHttpClient.GetAsync("http://missions.thedarkmod.com/get_available_missions.php");
        request.EnsureSuccessStatusCode();
        return await request.Content.ReadAsStreamAsync();
#else
        return await Task.FromResult(File.OpenRead(@"C:\_altdm__available_missions.xml"));
#endif
    }

    internal static async Task<(bool Success, Exception? Ex, List<TdmFmInfo> FMsList)>
    TryGetMissionsFromServer()
    {
        try
        {
            var fail = (false, (Exception?)null, new List<TdmFmInfo>());

            using var dataStream = await GetAvailableMissionsStream();

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

            var fmsList = new List<TdmFmInfo>(missionNodes.Count);

            foreach (XmlNode mn in missionNodes)
            {
                if (mn.Name == "mission")
                {
                    if (mn.Attributes != null)
                    {
                        TdmFmInfo fmInfo = new();
                        foreach (XmlAttribute attr in mn.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "title":
                                    fmInfo.Title = attr.Value;
                                    break;
                                case "releaseDate":
                                    fmInfo.ReleaseDate = attr.Value;
                                    break;
                                case "size":
                                    fmInfo.Size = attr.Value;
                                    break;
                                case "version":
                                    fmInfo.Version = attr.Value;
                                    break;
                                case "internalName":
                                    fmInfo.InternalName = attr.Value;
                                    break;
                                case "type":
                                    fmInfo.Type = attr.Value;
                                    break;
                                case "author":
                                    fmInfo.Author = attr.Value;
                                    break;
                                case "id":
                                    fmInfo.Id = attr.Value;
                                    break;
                            }
                        }
                        fmsList.Add(fmInfo);
                    }
                }
            }

            return (true, null, fmsList);
        }
        catch (Exception ex)
        {
            return (false, ex, new List<TdmFmInfo>());
        }
    }
}
