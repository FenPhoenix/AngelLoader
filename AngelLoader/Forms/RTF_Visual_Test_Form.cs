using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using static AL_Common.CommonUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public partial class RTF_Visual_Test_Form : Form
    {
        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        public RTF_Visual_Test_Form(bool dark)
        {
            InitializeComponent();

            var rtfFiles = Directory
                .GetFiles(@"C:\AngelLoader\Data\FMsCache", "*", SearchOption.AllDirectories)
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

            if (RTFFileComboBox.Items.Count > 0) RTFFileComboBox.SelectedIndex = 0;
        }

        private void SetVisualTheme(VisualTheme theme) => ControlUtils.ChangeFormThemeMode(theme, this, _controlColors);

        private void RTFFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RTFBox.LoadContent(RTFFileComboBox.SelectedItem.ToString(), ReadmeType.RichText);
        }
    }
}
