﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Xml;

namespace AngelLoader;

internal static class TDM_Downloader
{
    internal sealed class TdmFmInfo
    {
        // Change these to their appropriate types later
        internal string Title = "";

        // DateTime
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

    internal static bool TryGetMissionsFromServer([NotNullWhen(true)] out List<TdmFmInfo>? fmsList)
    {
        fmsList = null;

        try
        {
            using var wc = new WebClient();
            byte[] data = wc.DownloadData("http://missions.thedarkmod.com/get_available_missions.php");
            using var dataStream = new MemoryStream(data);

            var xmlDoc = new XmlDocument();

            xmlDoc.Load(dataStream);

            XmlNodeList? tdmNodes = xmlDoc.SelectNodes("tdm");
            if (tdmNodes?.Count != 1) return false;

            XmlNode tdmNode = tdmNodes[0];

            XmlNodeList availableMissionsNodes = tdmNode.ChildNodes;
            if (availableMissionsNodes.Count != 1) return false;

            XmlNode availableMissionsNode = availableMissionsNodes[0];
            if (availableMissionsNode.Name != "availableMissions") return false;

            XmlNodeList missionNodes = availableMissionsNode.ChildNodes;

            fmsList = new List<TdmFmInfo>();

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

            return true;
        }
        catch
        {
            fmsList = null;
            return false;
        }
    }
}
