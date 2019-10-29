using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace FenGen
{
    internal static class Roslyn_Test
    {
        internal static async Task RunRoslynTest()
        {
            var solutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));

            MSBuildLocator.RegisterDefaults();

            using var ws = MSBuildWorkspace.Create();

            var solution = await ws.OpenSolutionAsync(Path.Combine(solutionPath, "AngelLoader.sln"));

            // Throw if we don't find it, it's kind of important
            var alProj = solution.Projects.First(x => x.AssemblyName.EqualsI("AngelLoader"));

            var c = await alProj.GetCompilationAsync();

            ClassDeclarationSyntax LTextClass = null;

            //var blah = c.GetTypeByMetadataName("AngelLoader.Common.Attributes.FenGenLocalizationClassAttribute");
            //var blah = c.GetTypeByMetadataName("AngelLoader.Forms.MainForm");
            var blah = c.GetTypeByMetadataName("AngelLoader.Common.FenGenLocalizationClassAttribute");
            //var blah=c.GetTypeByMetadataName()
            if (blah != null)
            {
                Trace.WriteLine("***********" + blah.Name);
            }

            Trace.WriteLine("");

            foreach (var t in c.SyntaxTrees)
            {
                //var sm = c.GetSemanticModel(t);
                //var si = sm.GetSymbolInfo((CompilationUnitSyntax)t.GetRoot());
                //var blah = sm.SyntaxTree.GetRoot()

                var nodes = t.GetCompilationUnitRoot().DescendantNodesAndSelf();
                foreach (var n in nodes)
                {
                    if (!n.IsKind(SyntaxKind.ClassDeclaration)) continue;

                    var classItem = (ClassDeclarationSyntax)n;

                    if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0)
                    {
                        foreach (var attr in classItem.AttributeLists[0].Attributes)
                        {
                            var type = c.GetSemanticModel(t).GetTypeInfo(attr).ConvertedType;
                            //Trace.WriteLine("1111***************" + type.MetadataName);
                            //Trace.WriteLine(type.Name);
                            //Trace.WriteLine("");
                            if (type.Name == "FenGenLocalizationClassAttribute")
                            {
                                LTextClass = classItem;
                                goto breakout;
                            }
                        }
                    }

                    //if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0 &&
                    //    classItem.AttributeLists[0].Attributes.Any(x =>
                    //        GetAttributeName(x.Name.ToString(), "FenGenLocalizationClass")))
                    //{
                    //    LTextClass = classItem;
                    //    goto breakout;
                    //}
                }
            }

            breakout:
            if (LTextClass != null)
            {
                Trace.WriteLine(LTextClass.Identifier.Value);
            }
        }
    }
}
