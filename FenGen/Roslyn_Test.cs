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
using static FenGen.Methods;

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

            foreach (var t in c.SyntaxTrees)
            {
                //var blah = c.GetSemanticModel(t);
                //blah.

                var nodes = t.GetCompilationUnitRoot().DescendantNodesAndSelf();
                foreach (var n in nodes)
                {
                    if (!n.IsKind(SyntaxKind.ClassDeclaration)) continue;

                    var classItem = (ClassDeclarationSyntax)n;

                    if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0 &&
                        classItem.AttributeLists[0].Attributes.Any(x =>
                            GetAttributeName(x.Name.ToString(), "FenGenLocalizationClass")))
                    {
                        LTextClass = classItem;
                        goto breakout;
                    }
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
