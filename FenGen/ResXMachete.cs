using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FenGen
{
    internal static class ResXMachete
    {
        internal static void Generate(string resxFile)
        {
            File.Copy(resxFile, resxFile + ".temp", overwrite: true);

            var xr = new XmlDocument();
            xr.Load(resxFile);

            var root = xr.DocumentElement;
            var nodes = root?.SelectNodes("data[substring(@name, string-length(@name) - string-length('.ImageStream') + 1) = '.ImageStream']");
            if (nodes == null || nodes.Count <= 0) return;

            foreach (XmlNode node in nodes) root.RemoveChild(node);
            xr.Save(resxFile);
        }
    }
}
