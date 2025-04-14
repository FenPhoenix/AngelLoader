//#define ENABLE_RTF_VISUAL_TEST_FORM

#if ENABLE_RTF_VISUAL_TEST_FORM && (DEBUG || Release_Testing)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using static AL_Common.Common;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed partial class RTF_Visual_Test_Form : DarkFormBase, IWaitCursorSettable
{
    private const string AppGuid = "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader";

    internal sealed class RTF_Dark_Test_AppContext : ApplicationContext
    {
        internal RTF_Dark_Test_AppContext(bool dark)
        {
            Config.VisualTheme = dark ? VisualTheme.Dark : VisualTheme.Classic;

            using RTF_Visual_Test_Form f = new();
            f.ShowDialogDark();
            Environment.Exit(1);
        }
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    internal static void LoadIfCommandLineArgsArePresent()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length > 1 && args[1].StartsWithO("-rtf_test_"))
        {
            bool dark = args[1] switch
            {
                "-rtf_test_light" => false,
                "-rtf_test_dark" => true,
                _ => false,
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RTF_Dark_Test_AppContext(dark));
        }
    }

    [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int RegisterWindowMessageW(string message);

    private const int HWND_BROADCAST = 0xffff;
    private static readonly int WM_CHANGECOMBOBOXSELECTEDINDEX = RegisterWindowMessageW(nameof(WM_CHANGECOMBOBOXSELECTEDINDEX) + "|" + AppGuid);
    private static readonly int WM_CHANGERICHTEXTBOXSCROLLINFO = RegisterWindowMessageW(nameof(WM_CHANGERICHTEXTBOXSCROLLINFO) + "|" + AppGuid);

    private bool _broadcastEnabled = true;

    private const string SaveBaseDir = @"C:\AL_RTF_Color_Accuracy";
    private const string FMsCacheDir = @"C:\AngelLoader\Data\FMsCache";
    private readonly string ConfigDir = Path.Combine(SaveBaseDir, "Config");
    private readonly string ConfigFile;

    public RTF_Visual_Test_Form()
    {
        InitializeComponent();

        RTFBox.SetOwner(this);

        ConfigFile = Path.Combine(ConfigDir, "Config.ini");

        List<string> rtfFiles = Directory
            .GetFiles(FMsCacheDir, "*", SearchOption.AllDirectories)
            .Where(static x => x.EndsWithI(".rtf") || x.EndsWithI(".txt")).ToList();

        byte[] rtfHeaderBuffer = new byte[RTFHeaderBytes.Length];

        for (int i = 0; i < rtfFiles.Count; i++)
        {
            string file = rtfFiles[i];

            int headerLen = RTFHeaderBytes.Length;

            using (FileStream fs = File.OpenRead(file))
            {
                if (fs.Length >= headerLen)
                {
                    using BinaryReader br = new(fs, Encoding.ASCII);
                    _ = br.BaseStream.ReadAll(rtfHeaderBuffer, 0, headerLen);
                }
            }

            if (!rtfHeaderBuffer.SequenceEqual(RTFHeaderBytes))
            {
                rtfFiles.RemoveAt(i);
                i--;
            }
        }

        RTFFileComboBox.BeginUpdate();
        foreach (string item in rtfFiles)
        {
            RTFFileComboBox.Items.Add(item);
        }
        RTFFileComboBox.EndUpdate();

        if (Config.VisualTheme == VisualTheme.Dark)
        {
            Win32ThemeHooks.InstallHooks();
            SetThemeBase(VisualTheme.Dark);
        }

        if (RTFFileComboBox.Items.Count > 0)
        {
            if (File.Exists(ConfigFile))
            {
                using StreamReader sr = new(ConfigFile);
                string? indexStr = sr.ReadLine();
                if (indexStr != null && Int_TryParseInv(indexStr, out int index))
                {
                    RTFFileComboBox.SelectedIndex = index;
                }
            }
            else
            {
                RTFFileComboBox.SelectedIndex = 0;
            }
        }
    }

    private void RTFFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        RTFBox.LoadContent(RTFFileComboBox.SelectedItem.ToString(), ReadmeType.RichText, null);

        string notesFile = GetCurrentNotesFile();
        NotesTextBox.Text = File.Exists(notesFile) ? File.ReadAllText(notesFile) : "";

        if (_broadcastEnabled)
        {
            Native.PostMessageW(HWND_BROADCAST, WM_CHANGECOMBOBOXSELECTEDINDEX, AppNum(), RTFFileComboBox.SelectedIndex);
        }
    }

    private static int AppNum() => Config.VisualTheme == VisualTheme.Classic ? 0 : 1;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_CHANGECOMBOBOXSELECTEDINDEX && MsgAppNum(ref m) != AppNum())
        {
            _broadcastEnabled = false;
            RTFFileComboBox.SelectedIndex = m.LParam.ToInt32();
            _broadcastEnabled = true;
        }
        else if (m.Msg == WM_CHANGERICHTEXTBOXSCROLLINFO && MsgAppNum(ref m) != AppNum())
        {
            _broadcastEnabled = false;
            Native.SCROLLINFO si = ControlUtils.GetCurrentScrollInfo(RTFBox.Handle, Native.SB_VERT);
            si.nPos = m.LParam.ToInt32();
            ControlUtils.RepositionScroll(RTFBox.Handle, si, Native.SB_VERT);
            _broadcastEnabled = true;
        }
        else
        {
            base.WndProc(ref m);
        }

        return;

        static int MsgAppNum(ref Message m) => m.WParam.ToInt32();
    }

    private void RTFBox_VScroll(object sender, EventArgs e)
    {
        if (_broadcastEnabled)
        {
            Native.SCROLLINFO si = ControlUtils.GetCurrentScrollInfo(RTFBox.Handle, Native.SB_VERT);
            Native.PostMessageW(HWND_BROADCAST, WM_CHANGERICHTEXTBOXSCROLLINFO, AppNum(), si.nPos);
        }
    }

    private string GetCurrentNotesFile() =>
        Path.Combine(SaveBaseDir, RTFFileComboBox
            .SelectedItem.ToString()
            .Substring(FMsCacheDir.Length + 1)
            .ToForwardSlashes()
            .Replace("/", "___") + ".txt");

    private void SaveButton_Click(object sender, EventArgs e)
    {
        Directory.CreateDirectory(SaveBaseDir);
        if (NotesTextBox.Text.IsWhiteSpace())
        {
            File.Delete(GetCurrentNotesFile());
        }
        else
        {
            File.WriteAllText(GetCurrentNotesFile(), NotesTextBox.Text);
        }
    }

    private void RTF_Visual_Test_Form_FormClosing(object sender, FormClosingEventArgs e)
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(ConfigFile, RTFFileComboBox.SelectedIndex.ToStrInv());
    }

    public void SetWaitCursor(bool value) => Cursor = value ? Cursors.WaitCursor : Cursors.Default;
}
#endif
