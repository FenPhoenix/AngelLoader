using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace FenGen
{
    internal static class ExcludeResx
    {
        internal static void Generate()
        {
            // no-op until I finish this
            return;

            var excludedResxFiles = Directory.GetFiles(Core.ALProjectPath, "*.resx", SearchOption.AllDirectories)
                .Where(x => !Path.GetFileName(x).EqualsI("Resources.resx"));

            const string embeddedResource = "EmbeddedResource";

            var xml = new XmlDocument();
            xml.Load(Core.ALProjectFile);
            var itemGroups = xml.GetElementsByTagName(embeddedResource);

            var excludeResxNodes = new List<XmlNode>();

            if (itemGroups.Count > 0)
            {
                Trace.WriteLine("found");
                foreach (XmlNode node in itemGroups)
                {
                    XmlNode? itemGroup = node.ParentNode;
                    if (itemGroup != null)
                    {
                        foreach (XmlNode subNode in itemGroup.ChildNodes)
                        {
                            if (subNode.LocalName == embeddedResource)
                            {
                                if (subNode.Attributes?.Count > 0)
                                {
                                    bool conditionFound = false;
                                    bool removeResxFound = false;
                                    foreach (XmlAttribute attr in subNode.Attributes)
                                    {
                                        if (attr.Specified)
                                        {
                                            if (attr.LocalName == "Condition" &&
                                                Regex.Match(attr.Value, @"'\$\(Configuration\)'\s*!=").Success)
                                            {
                                                conditionFound = true;
                                            }
                                            else if (attr.LocalName == "Remove" &&
                                                     attr.Value.EndsWithI(".resx"))
                                            {
                                                removeResxFound = true;
                                            }
                                            if (conditionFound && removeResxFound)
                                            {
                                                excludeResxNodes.Add(subNode);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var sortedIndexes = new List<int>();
                        foreach (XmlNode en in excludeResxNodes)
                        {
                            
                        }
                    }
                }
            }
        }
    }
}
