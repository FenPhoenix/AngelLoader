using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FenGen
{
    internal static class ExcludeResx
    {
        internal static void Generate()
        {
            var resxFilesToExclude = Directory.GetFiles(Core.ALProjectPath, "*.resx", SearchOption.AllDirectories)
                .Where(x => !Path.GetFileName(x).EqualsI("Resources.resx")).ToArray();

            const string embeddedResourceName = "EmbeddedResource";
            const string conditionString = "'$(Configuration)' != 'Debug'";
            const string conditionName = "Condition";
            const string removeName = "Remove";

            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.Load(Core.ALProjectFile);

            var itemGroups = xml.GetElementsByTagName(embeddedResourceName);
            if (itemGroups.Count <= 0) return;

            var excludeResxNodes = new List<XmlNode>();
            bool found = false;

            /*
             TODO(FenGen:ExcludeResx.Generate()): This logic works for our specific case, but is not quite correct.
             For instance, it's finding the group by seeing if there's an <EmbeddedResource> element in it. In
             our case, we're finding it either by this:

             <EmbeddedResource Condition="'$(Configuration)' != 'Debug'" Remove="[filename]" />

             or this, which in our case is in the same group:

             <EmbeddedResource Update="Properties\Resources.resx">
               <Generator>ResXFileCodeGenerator</Generator>
               <LastGenOutput>Resources.Designer.cs</LastGenOutput>
             </EmbeddedResource>

             If that last one wasn't in the same group as the rest are supposed to be, AND if the rest had ALL
             been removed, it would find nothing and return without generating. Vanishingly unlikely in our case,
             but we should handle that situation!
            */
            foreach (XmlNode node in itemGroups)
            {
                XmlNode? itemGroup = node.ParentNode;
                if (itemGroup == null) continue;

                for (int i = 0; i < itemGroup.ChildNodes.Count; i++)
                {
                    XmlNode childNode = itemGroup.ChildNodes[i];

                    if (!(childNode.Attributes?.Count > 0)) continue;

                    bool conditionFound = false;
                    bool removeResxFound = false;
                    foreach (XmlAttribute attr in childNode.Attributes)
                    {
                        if (!attr.Specified) continue;

                        if (attr.LocalName == conditionName &&
                            Regex.Match(attr.Value, @"'\$\(Configuration\)'\s*!=").Success)
                        {
                            conditionFound = true;
                        }
                        else if (attr.LocalName == removeName &&
                                 attr.Value.EndsWithI(".resx"))
                        {
                            removeResxFound = true;
                        }

                        if (conditionFound && removeResxFound)
                        {
                            excludeResxNodes.Add(childNode);
                            break;
                        }
                    }
                }

                var newNodes = new List<XmlNode>();

                foreach (XmlNode en in excludeResxNodes)
                {
                    itemGroup.RemoveChild(en);
                }

                foreach (string f in resxFilesToExclude)
                {
                    XmlElement elem = xml.CreateElement(embeddedResourceName);

                    XmlAttribute conditionAttr = xml.CreateAttribute(conditionName);
                    conditionAttr.Value = conditionString;
                    XmlAttribute removeAttr = xml.CreateAttribute(removeName);
                    removeAttr.Value = f.Substring(Core.ALProjectPath.Length).TrimStart('/', '\\');

                    elem.SetAttributeNode(conditionAttr);
                    elem.SetAttributeNode(removeAttr);

                    newNodes.Add(elem);
                }

                for (int i = 0; i < newNodes.Count; i++)
                {
                    XmlNode n = newNodes[i];
                    itemGroup.PrependChild(n);
                    // We have to manually add linebreaks and indents
                    itemGroup.InsertBefore(xml.CreateWhitespace("    "), n);
                    itemGroup.InsertAfter(xml.CreateWhitespace("\r\n"), n);
                }
                // Add initial linebreak (prepending so we do it at the end)
                itemGroup.PrependChild(xml.CreateWhitespace("\r\n"));

                found = true;

                break;
            }

            if (!found) return;

            List<string> lines;
            using (var strW = new StringWriter())
            {
                var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, OmitXmlDeclaration = true };
                using (var xmlWriter = XmlWriter.Create(strW, settings))
                {
                    xml.Save(xmlWriter);
                }

                lines = strW
                    .ToString()
                    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                    .ToList();
            }

            // Remove consecutive whitespace lines (leaving only one-in-a-row at most).
            // This gets rid of the garbage left behind from removing the old nodes (whitespace lines).
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsWhiteSpace())
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        if (lines[j].IsWhiteSpace())
                        {
                            lines.RemoveAt(j);
                            j--;
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            using var sw = new StreamWriter(Core.ALProjectFile, append: false, Encoding.UTF8);
            for (int i = 0; i < lines.Count; i++)
            {
                if (i == lines.Count - 1)
                {
                    sw.Write(lines[i]);
                }
                else
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }
    }
}
