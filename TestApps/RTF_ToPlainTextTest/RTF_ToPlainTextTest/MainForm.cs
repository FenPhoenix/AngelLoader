/*
Unmodified, as-is local testing app. You'll need to modify the paths and maybe some other stuff if you want it
to work. Just adding it for completeness.

Public domain/CC0 cause who cares, there's literally nothing of novelty whatsoever.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FMScanner;
using static AL_Common.Common;

namespace RTF_ToPlainTextTest;

public sealed partial class MainForm : Form
{
    private readonly string _destDirBase = @"C:\rtf_plaintext_test";
    private readonly string _destDirRTFBox;
    private readonly string _destDirCustom;
    private readonly string _originalFullSetFromCacheDir;
    private readonly string _originalFullSetFromCacheDir_NoImages;
    private readonly string _alCacheDir;

    public MainForm()
    {
        InitializeComponent();

        _destDirRTFBox = Path.Combine(_destDirBase, "RichTextBox");
        _destDirCustom = Path.Combine(_destDirBase, "Custom");
        _originalFullSetFromCacheDir = Path.Combine(_destDirBase, "Original_Full_Set_From_Cache");
        _originalFullSetFromCacheDir_NoImages = Path.Combine(_destDirBase, "Original_Full_Set_From_Cache (no images)");
        _alCacheDir = @"C:\AngelLoader\Data\FMsCache";

        Directory.CreateDirectory(_destDirRTFBox);
        Directory.CreateDirectory(_destDirCustom);
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        ConvertOnlyWithCustomButton.Focus();
    }

    private void CopySet()
    {
        var rtfFiles = new List<string>();
        var cacheDirs = Directory.GetDirectories(_alCacheDir, "*", SearchOption.TopDirectoryOnly);
        foreach (string d in cacheDirs)
        {
            rtfFiles.AddRange(Directory.GetFiles(d, "*.rtf", SearchOption.AllDirectories));
            rtfFiles.AddRange(Directory.GetFiles(d, "*.txt", SearchOption.AllDirectories));
        }

        byte[] RTFHeaderBytes = Encoding.ASCII.GetBytes(@"{\rtf1");

        for (int i = 0; i < rtfFiles.Count; i++)
        {
            string f = rtfFiles[i];

            int headerLen = RTFHeaderBytes.Length;

            byte[] buffer = new byte[headerLen];

            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= headerLen)
                {
                    using var br = new BinaryReader(fs, Encoding.ASCII);
                    buffer = br.ReadBytes(headerLen);
                }
            }

            var isRtf = buffer.SequenceEqual(RTFHeaderBytes);

            if (isRtf)
            {
                File.Copy(f, Path.Combine(_originalFullSetFromCacheDir, f.Substring(_alCacheDir.Length).Replace("\\", "__").Replace("/", "__")));
            }
        }
    }

    private string GetOriginalSetFromCacheDir(bool write)
    {
        bool enabled = write ? NoImagesSet_WriteCheckBox.Checked : NoImagesSet_ConvertOnlyCheckBox.Checked;
        return enabled ? _originalFullSetFromCacheDir_NoImages : _originalFullSetFromCacheDir;
    }

    private static string GetMBsString(long totalSize, long elapsedMilliseconds)
    {
        double megs = (double)totalSize / 1024 / 1024;
        double intermediate = megs / elapsedMilliseconds;
        double finalMBs = Math.Round(intermediate * 1000, 2, MidpointRounding.AwayFromZero);
        return finalMBs.ToString(CultureInfo.CurrentCulture) + " MB/s";
    }

    private void WritePlaintextFile(string f, string[] lines, string destDir)
    {
        string ff = f.Substring(GetOriginalSetFromCacheDir(write: true).Length).Replace("\\", "__").Replace("/", "__");
        ff = Path.GetFileNameWithoutExtension(ff) + "_rtf_to_plaintext.txt";
        File.WriteAllLines(Path.Combine(destDir, ff), lines);
    }

    private void ClearPlainTextDir(bool rtf)
    {
        string dir = rtf ? _destDirRTFBox : _destDirCustom;
        foreach (string f in Directory.GetFiles(_destDirRTFBox, "*", SearchOption.TopDirectoryOnly))
        {
            File.Delete(f);
        }
    }

    private static void ShowPerfResults(Stopwatch sw, long totalSize)
    {
        MessageBox.Show(
            sw.Elapsed + "\r\n" +
            GetMBsString(totalSize, sw.ElapsedMilliseconds));
    }

    private void HandleAllWithRichTextBox(bool write)
    {
        string[] rtfFiles = Directory.GetFiles(GetOriginalSetFromCacheDir(write));
        if (write) ClearPlainTextDir(rtf: true);

        using var rtfBox = new RichTextBox();

        MemoryStream[] memStreams = new MemoryStream[rtfFiles.Length];
        try
        {
            long totalSize = 0;

            for (int i = 0; i < rtfFiles.Length; i++)
            {
                string f = rtfFiles[i];
                using var fs = File.OpenRead(f);
                byte[] array = new byte[fs.Length];
                int bytesRead = fs.ReadAll(array, 0, (int)fs.Length);
                memStreams[i] = new MemoryStream(array);
                totalSize += bytesRead;
            }

            if (write)
            {
                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < memStreams.Length; i++)
                {
                    rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                    string f = rtfFiles[i];
                    WritePlaintextFile(f, rtfBox.Lines, _destDirRTFBox);
                }

                sw.Stop();
                ShowPerfResults(sw, totalSize);
            }
            else
            {
                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < memStreams.Length; i++)
                {
                    rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                    _ = rtfBox.Text;
                }

                sw.Stop();
                ShowPerfResults(sw, totalSize);
            }
        }
        finally
        {
            foreach (MemoryStream ms in memStreams)
            {
                ms.Dispose();
            }
        }
    }

    private void HandleAllWithCustom(bool write)
    {
        string[] rtfFiles = Directory.GetFiles(GetOriginalSetFromCacheDir(write));
        if (write) ClearPlainTextDir(rtf: false);

        var rtfReader = new RtfToTextConverter();

        ArrayWithLength<byte>[] byteArrays = new ArrayWithLength<byte>[rtfFiles.Length];

        long totalSize = 0;

        for (int i = 0; i < rtfFiles.Length; i++)
        {
            string f = rtfFiles[i];
            using var fs = File.OpenRead(f);
            byte[] array = new byte[fs.Length];
            int bytesRead = fs.ReadAll(array, 0, (int)fs.Length);
            byteArrays[i] = new ArrayWithLength<byte>(array, bytesRead);
            totalSize += byteArrays[i].Length;
        }

        if (write)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < byteArrays.Length; i++)
            {
                string f = rtfFiles[i];
                Trace.WriteLine(f);
                ArrayWithLength<byte> array = byteArrays[i];
                (_, string text) = rtfReader.Convert(array);
                WritePlaintextFile(f, text.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.None), _destDirCustom);
            }

            sw.Stop();
            ShowPerfResults(sw, totalSize);
        }
        else
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < byteArrays.Length; i++)
            {
                _ = rtfReader.Convert(byteArrays[i]);
            }

            sw.Stop();
            ShowPerfResults(sw, totalSize);
        }
    }

    private void HandleOneWithCustom(bool write)
    {
        if (write) ClearPlainTextDir(rtf: false);

        var rtfreader = new RtfToTextConverter();
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__TDP20AC_theburningbedlam__FMInfo.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2010-08-31_Bathory_campaign_ne__info.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2009-06-22_WickedRelicsV1_1__Wicked Relics.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2006-09-13_WC_SneakingthroughV__Sneaking through Venice.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2006-07-25_DarkMessiah_bassilu__DarkMessiah.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__10Rooms_Hammered_EnglishV1_0__FmInfo-en.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2003-01-25_c4burricksheadinnv2__entry.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2010-04-08_King'sStory_KS__KSreadme.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__7SoM_v11__Seven Shades Readme.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2000-12-30_Uneaffaireenor__Readme.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\!!!!!!custom_testing.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2010-03-09_JourneyIntoTheUnder__A Journey Into The Underdark.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2004-10-17_Ack!TheresaZombiein__Game_Info.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__10Rooms_Cave_v1__readme.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__VaultChronicles__vcse_de.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2010-06-11_WhenStill_1_1__FMInfo-fr.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2004-02-29_c5Summit_The__summit.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\!!!!!!!!!!!!!!!!!!!!!!_custom_2.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__2013-08-08_The_FavourND__The Favour - NewDark.rtf";
        string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__TDP20AC_An_Enigmatic_Treasure___TDP20AC_An_Enigmatic_Treasure_With_A_Recondite_Discovery.rtf";
        //string file = @"C:\rtf_plaintext_test\Original_Full_Set_From_Cache\__UpsideDown__Readme.rtf";
        using var fs = File.OpenRead(file);
        byte[] array = new byte[fs.Length];
        int bytesRead = fs.ReadAll(array, 0, (int)fs.Length);
        (_, string text) = rtfreader.Convert(new ArrayWithLength<byte>(array, bytesRead));
        if (write)
        {
            WritePlaintextFile(file, text.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.None), _destDirCustom);
        }
    }

    private void ConvertAndWriteWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        HandleAllWithRichTextBox(write: true);
    }

    private void ConvertAndWriteWithCustomButton_Click(object sender, EventArgs e)
    {
        HandleAllWithCustom(write: true);
    }

    private void ConvertOnlyWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        HandleAllWithRichTextBox(write: false);
    }

    private void ConvertOnlyWithCustomButton_Click(object sender, EventArgs e)
    {
        HandleAllWithCustom(write: false);
    }

    private void WriteOneButton_Click(object sender, EventArgs e)
    {
        HandleOneWithCustom(write: true);
    }

    private void ConvertOneButton_Click(object sender, EventArgs e)
    {
        HandleOneWithCustom(write: false);
    }

    private void Test1Button_Click(object sender, EventArgs e)
    {
    }
}
