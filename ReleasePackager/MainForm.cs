using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ReleasePackager;

public sealed partial class MainForm : Form
{
    private enum Bitness
    {
        X86,
        X64
    }

    public MainForm()
    {
        InitializeComponent();
    }

    // @Update: These paths are output to in the personal post-build bat file
    // We need to make them be accessible to other users. Also make them not hard-coded?
    private const string releaseBasePath = @"C:\AngelLoader_Public_Package";

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Package(Bitness.X64);
        Application.Exit();
    }

    private void Package(Bitness bitness)
    {
        // @Update: Ditto the above with these paths
        string inputPath = Path.Combine(releaseBasePath, bitness == Bitness.X64 ? "x64" : "x86");

        string bitnessString = bitness == Bitness.X64 ? "x64" : "x86";

        string[] files;
        try
        {
            files = Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                MessageBox.Show(this, "No files in '" + inputPath + "'");
                return;
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            MessageBox.Show(this,
                "Directory not found: '" + inputPath + "'.\r\n\r\n" +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
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
                    MessageBox.Show(this,
                        "Error exit code from 7z.exe: " + p.ExitCode);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Exception while running 7z.exe." +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }
    }
}
