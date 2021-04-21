using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using static AL_Common.CommonUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class RTF_Visual_Test_Form : Form
    {
        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        private bool _broadcastEnabled = true;

        private const string SaveBaseDir = @"C:\AL_RTF_Color_Accuracy";
        private const string FMsCacheDir = @"C:\AngelLoader\Data\FMsCache";
        private readonly string ConfigDir = Path.Combine(SaveBaseDir, "Config");
        private readonly string ConfigFile;

        public RTF_Visual_Test_Form()
        {
            InitializeComponent();

            ConfigFile = Path.Combine(ConfigDir, "Config.ini");

            var rtfFiles = Directory
                .GetFiles(FMsCacheDir, "*", SearchOption.AllDirectories)
                .Where(x => x.EndsWithI(".rtf") || x.EndsWithI(".txt")).ToList();

            for (int i = 0; i < rtfFiles.Count; i++)
            {
                string file = rtfFiles[i];

                int headerLen = RTFHeaderBytes.Length;
                byte[] buffer = new byte[headerLen];

                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length >= headerLen)
                    {
                        using var br = new BinaryReader(fs, Encoding.ASCII);
                        buffer = br.ReadBytes(headerLen);
                    }
                }

                if (!buffer.SequenceEqual(RTFHeaderBytes))
                {
                    rtfFiles.RemoveAt(i);
                    i--;
                }
            }

            RTFFileComboBox.BeginUpdate();
            foreach (string item in rtfFiles) RTFFileComboBox.Items.Add(item);
            RTFFileComboBox.EndUpdate();

            if (Config.VisualTheme == VisualTheme.Dark)
            {
                NativeHooks.InstallHooks();
                SetVisualTheme(VisualTheme.Dark);
                RTFBox.SetRTFColorStyle(RTFColorStyle.Auto, startup: true);
            }

            if (RTFFileComboBox.Items.Count > 0)
            {
                if (File.Exists(ConfigFile))
                {
                    using var sr = new StreamReader(ConfigFile);
                    string? indexStr = sr.ReadLine();
                    if (indexStr != null && int.TryParse(indexStr, out int index))
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

        private void SetVisualTheme(VisualTheme theme) => ControlUtils.ChangeFormThemeMode(theme, this, _controlColors);

        private void RTFFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RTFBox.LoadContent(RTFFileComboBox.SelectedItem.ToString(), ReadmeType.RichText);

            string notesFile = GetCurrentNotesFile();
            NotesTextBox.Text = File.Exists(notesFile) ? File.ReadAllText(notesFile) : "";

            if (_broadcastEnabled)
            {
                Native.PostMessage((IntPtr)Native.HWND_BROADCAST, Native.WM_CHANGECOMBOBOXSELECTEDINDEX, (IntPtr)AppNum(), (IntPtr)RTFFileComboBox.SelectedIndex);
            }
        }

        private static int AppNum() => Config.VisualTheme == VisualTheme.Classic ? 0 : 1;

        protected override void WndProc(ref Message m)
        {
            static int MsgAppNum(ref Message m) => m.WParam.ToInt32();

            if (m.Msg == Native.WM_CHANGECOMBOBOXSELECTEDINDEX && MsgAppNum(ref m) != AppNum())
            {
                _broadcastEnabled = false;
                RTFFileComboBox.SelectedIndex = m.LParam.ToInt32();
                _broadcastEnabled = true;
            }
            else if (m.Msg == Native.WM_CHANGERICHTEXTBOXSCROLLINFO && MsgAppNum(ref m) != AppNum())
            {
                _broadcastEnabled = false;
                var si = ControlUtils.GetCurrentScrollInfo(RTFBox.Handle, Native.SB_VERT);
                si.nPos = m.LParam.ToInt32();
                ControlUtils.RepositionScroll(RTFBox.Handle, si, Native.SB_VERT);
                _broadcastEnabled = true;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void RTFBox_VScroll(object sender, EventArgs e)
        {
            if (_broadcastEnabled)
            {
                var si = ControlUtils.GetCurrentScrollInfo(RTFBox.Handle, Native.SB_VERT);
                Native.PostMessage((IntPtr)Native.HWND_BROADCAST, Native.WM_CHANGERICHTEXTBOXSCROLLINFO, (IntPtr)AppNum(), (IntPtr)si.nPos);
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
            File.WriteAllText(ConfigFile, RTFFileComboBox.SelectedIndex.ToString());
        }
    }
}
