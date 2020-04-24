using System.IO;
using System.Linq;
using SearchOption = System.IO.SearchOption;

namespace FenGen
{
    internal static class VisLoc
    {
        internal static void Generate()
        {
            const string formsDir = "Forms";
            const string customControlsDir = "CustomControls";
            
            string visLocProjectPath = Path.Combine(Core.ALSolutionPath, "AngelLoaderVisualLocalizer");

            string alFormsPath = Path.Combine(Core.ALProjectPath, formsDir);
            string visLocFormsPath = Path.Combine(visLocProjectPath, formsDir);
            
            string alCustomControlsPath = Path.Combine(Core.ALProjectPath, customControlsDir);
            string visLocCustomControlsPath = Path.Combine(visLocProjectPath, customControlsDir);

            string[] sourceDirs = Directory.GetDirectories(alFormsPath, "*", SearchOption.AllDirectories);

            Directory.CreateDirectory(visLocFormsPath);
            foreach (string d in sourceDirs)
            {
                Directory.CreateDirectory(Path.Combine(visLocFormsPath,
                    d.Substring(alFormsPath.Length + 1).Trim('\\', '/')));
            }

            string[] filesToCopy = Directory.GetFiles(alFormsPath, "*", SearchOption.AllDirectories)
                .Where(x => x.EndsWithI(".Designer.cs") || x.EndsWithI(".resx")).ToArray();

            foreach (string f in filesToCopy)
            {
                File.Copy(f, Path.Combine(visLocFormsPath,
                        Path.Combine(visLocFormsPath, f.Substring(alFormsPath.Length + 1).Trim('\\', '/'))),
                    overwrite: true);
            }
        }
    }
}
