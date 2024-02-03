using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ReleasePackager;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        View = new MainForm();
        Application.Run(View);
    }

    private static MainForm View = null!;

    private sealed class LineItem(string text, bool isHeader, bool isListLine, int listIndent)
    {
        internal readonly string Text = text;
        internal readonly bool IsHeader = isHeader;
        internal readonly bool IsListLine = isListLine;
        internal readonly int ListIndent = listIndent;
    }

    internal static void UpdateTexts()
    {
        List<LineItem> lineItems = BuildLineItems();

        View.SetMarkdownText(GetMarkdownRawText(lineItems));
        View.SetTTLGText(GetTTLGText(lineItems));
    }

    private static string GetTTLGText(List<LineItem> lineItems)
    {
        List<string> ttlgLines = new();
        for (int i = 0; i < lineItems.Count; i++)
        {
            LineItem lineItem = lineItems[i];
            // This whole crap is just trial-and-error smashing the keyboard until it appears to finally do what
            // I @!#$!@#$! want. ARGH!
            if (lineItem.IsListLine)
            {
                if (i == 0 ||
                    (i > 0 && !lineItems[i - 1].IsListLine) ||
                    (i > 0 && lineItems[i - 1].IsListLine && lineItem.IsListLine && lineItems[i - 1].ListIndent < lineItem.ListIndent))
                {
                    ttlgLines.Add("[LIST]");
                }
                else if (i > 0 &&
                    ((lineItems[i - 1].IsListLine && !lineItem.IsListLine) ||
                     (lineItems[i - 1].IsListLine && lineItem.IsListLine && lineItems[i - 1].ListIndent > lineItem.ListIndent)))
                {
                    if (lineItems.Count > 0)
                    {
                        for (int indentI = 0; indentI < lineItems[i - 1].ListIndent; indentI++)
                        {
                            ttlgLines.Add("[/LIST]");
                        }
                    }
                    else
                    {
                        ttlgLines.Add("[/LIST]");
                    }
                }

                ttlgLines.Add("[*]" + lineItem.Text);

                if (i == lineItems.Count - 1)
                {
                    if (lineItems.Count > 0)
                    {
                        for (int indentI = 0; indentI < lineItems[i - 1].ListIndent + 1; indentI++)
                        {
                            ttlgLines.Add("[/LIST]");
                        }
                    }
                    else
                    {
                        ttlgLines.Add("[/LIST]");
                    }
                }

            }
            else
            {
                if (i > 0 &&
                   ((lineItems[i - 1].IsListLine && !lineItem.IsListLine) ||
                    (lineItems[i - 1].IsListLine && lineItem.IsListLine && lineItems[i - 1].ListIndent > lineItem.ListIndent)))
                {
                    ttlgLines.Add("[/LIST]");
                }

                (string prefix, string suffix) = lineItem.IsHeader ? ("[B]", "[/B]") : ("", "");
                ttlgLines.Add(prefix + lineItem.Text + suffix);
            }
        }

        return string.Join("\r\n", ttlgLines);
    }

    private static string GetMarkdownRawText(List<LineItem> lineItems)
    {
        List<string> markDownRawLines = new();
        foreach (LineItem lineItem in lineItems)
        {
            if (lineItem.IsHeader)
            {
                markDownRawLines.Add("#### " + lineItem.Text);
            }
            else if (lineItem.IsListLine)
            {
                markDownRawLines.Add(Indent(lineItem.ListIndent, 2) + "- " + lineItem.Text);
            }
            else
            {
                markDownRawLines.Add(lineItem.Text);
            }
        }

        return string.Join("\r\n", markDownRawLines);

        static string Indent(int levels, int width) => new(' ', width * levels);
    }

    private static List<LineItem> BuildLineItems()
    {
        List<LineItem> lineItems = new();
        string[] lines = View.GetReleaseNotesText().Split(new[] { "\r\n" }, StringSplitOptions.None);
        int currentIndent = 0;
        foreach (string line in lines)
        {
            int listCharIndex = ListCharIndex(line);
            if (listCharIndex > -1)
            {
                if (listCharIndex > currentIndent)
                {
                    currentIndent++;
                }
                else if (listCharIndex < currentIndent)
                {
                    currentIndent--;
                }
                var listItem = new LineItem(line.Substring(listCharIndex + 1).TrimStart(), false, true, currentIndent);

                lineItems.Add(listItem);
            }
            else
            {
                currentIndent = 0;
                lineItems.Add(Regex.Match(line.TrimEnd(), ":$").Success
                    ? new LineItem(line.Trim(), isHeader: true, isListLine: false, 0)
                    : new LineItem(line.Trim(), isHeader: false, isListLine: false, currentIndent));
            }
        }

        return lineItems;
    }

    private static int ListCharIndex(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (char.IsWhiteSpace(c)) continue;
            if (c == '-') return i;
        }
        return -1;
    }

    // @Update: When writing the file to the server, write with UTF8 no BOM for size minimization

    #region Package

    // @Update: These paths are output to in the personal post-build bat file
    // We need to make them be accessible to other users. Also make them not hard-coded?
    private const string releaseBasePath = @"C:\AngelLoader_Public_Package";

    internal enum Bitness
    {
        X86,
        X64
    }

    private static readonly string[] _bitnessStrings = { "x86", "x64" };

    private static string GetBitnessString(Bitness bitness) => _bitnessStrings[(int)bitness];

    internal static void Package(Bitness bitness)
    {
        string bitnessString = GetBitnessString(bitness);

        // @Update: Ditto the above with these paths
        string inputPath = Path.Combine(releaseBasePath, bitnessString);

        string[] files;
        try
        {
            files = Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                MessageBox.Show(View, "No files in '" + inputPath + "'");
                return;
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            MessageBox.Show(View,
                "Directory not found: '" + inputPath + "'.\r\n\r\n" +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(View,
                "Error while trying to get the list of files in '" + inputPath + "'.\r\n\r\n" +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }

        try
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = Path.Combine(Application.StartupPath, "7z.exe");
                p.StartInfo.WorkingDirectory = Application.StartupPath;
                // @Update: Have AL's post-build batch file pass bitness and version to us
                string outputArchive = Path.Combine(releaseBasePath,
                    "AngelLoader_v1.7.X_PACKAGE_TEST_" + bitnessString + ".zip");

                try
                {
                    File.Delete(outputArchive);
                }
                catch
                {
                    // ignore
                }

                p.StartInfo.Arguments =
                    "a \"" + outputArchive + "\" \"" + Path.Combine(inputPath, "*.*") + "\" "
                    // -r        = Recurse subdirectories
                    // -y        = Say yes to all prompts automatically
                    // -mx=9     = Compression level Ultra (maximum)
                    // -mfb=257  = Max fast bytes (max compression)
                    // -mpass=15 = Max passes (max compression)
                    // -mcu=on   = Always use UTF-8 for non-ASCII file names
                    + "-r -y -mx=9 -mfb=257 -mpass=15 -mcu=on";
                Trace.WriteLine(p.StartInfo.Arguments);
                p.StartInfo.CreateNoWindow = false;

                p.Start();
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    MessageBox.Show(View,
                        "Error exit code from 7z.exe: " + p.ExitCode);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(View,
                "Exception while running 7z.exe." +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }
    }

    #endregion
}
