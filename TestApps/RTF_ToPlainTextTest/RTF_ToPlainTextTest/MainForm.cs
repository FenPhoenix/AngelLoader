/*
Unmodified, as-is local testing app. You'll need to modify the paths and maybe some other stuff if you want it
to work. Just adding it for completeness.

Public domain/CC0 cause who cares, there's literally nothing of novelty whatsoever.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly string _rtfFilesListFile;
    //private readonly string _originalRTFSetDir;
    private readonly string _originalFullSetFromCacheDir;
    private readonly string _alCacheDir;

    public MainForm()
    {
        InitializeComponent();

        _destDirRTFBox = Path.Combine(_destDirBase, "RichTextBox");
        _destDirCustom = Path.Combine(_destDirBase, "Custom");
        _rtfFilesListFile = Path.Combine(_destDirBase, "rtfFilesList.txt");
        //_originalRTFSetDir = Path.Combine(_destDirBase, "Original_RTF_Set");
        //_originalFullSetFromCacheDir = Path.Combine(_destDirBase, "Original_RTF_Set");
        _originalFullSetFromCacheDir = Path.Combine(_destDirBase, "Original_Full_Set_From_Cache");
        _alCacheDir = @"C:\AngelLoader\Data\FMsCache";

        Directory.CreateDirectory(_destDirRTFBox);
        Directory.CreateDirectory(_destDirCustom);
    }

    private enum ConversionType
    {
        RichTextBox,
        Custom
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

    private void WriteRTFToPlaintextConvertedFiles(ConversionType conversionType)
    {
        //string[] rtfFiles = File.ReadAllLines(_rtfFilesListFile);

        string[] rtfFiles = Directory.GetFiles(_originalFullSetFromCacheDir);

        void WritePlaintextFile(string f, string[] lines, string destDir)
        {
            string ff = f.Substring(_originalFullSetFromCacheDir.Length).Replace("\\", "__").Replace("/", "__");
            ff = Path.GetFileNameWithoutExtension(ff) + "_rtf_to_plaintext.txt";
            //string ff = Path.GetFileNameWithoutExtension(f) + "_rtf_to_plaintext.txt";
            File.WriteAllLines(Path.Combine(destDir, ff), lines);
            //File.WriteAllLines(Path.Combine(destDir, ff), lines,Encoding.Unicode);
        }

        if (conversionType == ConversionType.RichTextBox)
        {
            foreach (string f in Directory.GetFiles(_destDirRTFBox, "*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(f);
            }

            using var rtfBox = new RichTextBox();

            MemoryStream[] memStreams = new MemoryStream[rtfFiles.Length];
            try
            {
                for (int i = 0; i < rtfFiles.Length; i++)
                {
                    string f = rtfFiles[i];
                    using var fs = File.OpenRead(f);
                    byte[] array = new byte[fs.Length];
                    fs.ReadAll(array, 0, (int)fs.Length);
                    memStreams[i] = new MemoryStream(array);
                }

                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < memStreams.Length; i++)
                {
                    rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                    string f = rtfFiles[i];
                    WritePlaintextFile(f, rtfBox.Lines, _destDirRTFBox);
                }

                sw.Stop();
                MessageBox.Show(sw.Elapsed + "\r\n");
            }
            finally
            {
                foreach (MemoryStream ms in memStreams)
                {
                    ms.Dispose();
                }
            }
        }
        else
        {
            foreach (string f in Directory.GetFiles(_destDirCustom, "*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(f);
            }

            //bool one = true;
            bool one = false;

            if (one)
            {
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
                WritePlaintextFile(file, text.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.None), _destDirCustom);
            }
            else
            {
                var rtfReader = new RtfToTextConverter();

#if true
                ArrayWithLength<byte>[] byteArrays = new ArrayWithLength<byte>[rtfFiles.Length];

                for (int i = 0; i < rtfFiles.Length; i++)
                {
                    string f = rtfFiles[i];
                    using var fs = File.OpenRead(f);
                    byte[] array = new byte[fs.Length];
                    int bytesRead = fs.ReadAll(array, 0, (int)fs.Length);
                    byteArrays[i] = new ArrayWithLength<byte>(array, bytesRead);
                }

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
                MessageBox.Show(sw.Elapsed + "\r\n");

                return;
#endif
                foreach (string f in rtfFiles)
                {
                    Trace.WriteLine(f);
                    using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None, 81920);
                    byte[] array = new byte[fs.Length];
                    int bytesRead = fs.ReadAll(array, 0, (int)fs.Length);
                    (_, string text) = rtfReader.Convert(new ArrayWithLength<byte>(array, bytesRead));
                    WritePlaintextFile(f, text.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.None), _destDirCustom);
                }

                //Trace.WriteLine(rtfReader._stopwatch.Elapsed);
                //MessageBox.Show(rtfReader._stopwatch.Elapsed.ToString());
                //Trace.WriteLine("-----------");
                //Trace.WriteLine(rtfReader._returnSB.Capacity);
            }

            //var mono_rtfBox = new RichTextBox_Custom();
            //foreach (string f in rtfFiles)
            //{
            //    mono_rtfBox.LoadFile(f, RichTextBoxStreamType.RichText);
            //    WritePlaintextFile(f, mono_rtfBox.Lines, _destDirCustom);
            //}

            //foreach (string f in rtfFiles)
            //{
            //    if (f.Contains("Forgotten Tomb"))
            //    {
            //        Debugger.Break();
            //    }
            //    try
            //    {
            //        var ConversionText = File.ReadAllText(f);
            //        IRtfGroup rtfStructure = RtfParserTool.Parse(ConversionText);
            //        RtfTextConverter textConverter = new RtfTextConverter();
            //        RtfInterpreterTool.Interpret(rtfStructure, textConverter);
            //        //RtfInterpreterTool.Interpret("",new IRtfInterpreterListener[1]);
            //        //textBox.Text = textConverter.PlainText;
            //    }
            //    catch (Exception exception)
            //    {
            //        Trace.WriteLine(f);
            //        Trace.WriteLine("Error " + exception.Message);
            //        MessageBox.Show(this, "Error " + exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
        }
    }

    private void ConvertWithRichTextBoxButton_Click(object sender, EventArgs e) => WriteRTFToPlaintextConvertedFiles(ConversionType.RichTextBox);

    private void ConvertWithCustomButton_Click(object sender, EventArgs e)
    {
#if true
        WriteRTFToPlaintextConvertedFiles(ConversionType.Custom);
#else
            var rtfReader = new RtfToTextConverter();

            string[] rtfFiles = Directory.GetFiles(_originalFullSetFromCacheDir);
            foreach (string f in rtfFiles)
            {
                Trace.WriteLine(f);
                using var stream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None, 81920);
                (_, _) = rtfReader.Convert(stream, stream.Length);
            }
#endif
    }

    private void Test1Button_Click(object sender, EventArgs e)
    {
        //CopySet();
        return;

        var dict = new Dictionary<string, bool>();
        var files = Directory.GetFiles(_originalFullSetFromCacheDir, "*", SearchOption.TopDirectoryOnly);
        foreach (var f in files)
        {
            using (var sr = new StreamReader(f, Encoding.ASCII))
            {
                string content = sr.ReadToEnd();
                bool hasAnsi = content.Contains(@"\ansi");
                dict.Add(Path.GetFileName(f), hasAnsi);
            }
        }

        using (var sw = new StreamWriter(@"C:\rtf_has_ansi.txt", append: false))
        {
            foreach (var item in dict)
            {
                sw.WriteLine(item.Key + @": has \ansi: " + item.Value);
            }
        }

        //var mono_rtfBox = new System_Mono.Windows.Forms.RichTextBox();
        //{
        //    mono_rtfBox.LoadFile(@"C:\nonexistent.rtf", System_Mono.Windows.Forms.RichTextBoxStreamType.RichText);
        //    var text = mono_rtfBox.Text;
        //    var lines = mono_rtfBox.Lines;
        //}
    }
}
