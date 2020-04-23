using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms
{
    public sealed partial class ListMessageBoxForm : Form
    {
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private enum SHSTOCKICONID : uint
        {
            SIID_HELP = 23,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80
        }

        private static SHSTOCKICONID GetSystemIcon(MessageBoxIcon icon)
        {
            return
                  icon == MessageBoxIcon.Error ||
                  icon == MessageBoxIcon.Hand ||
                  icon == MessageBoxIcon.Stop
                ? SHSTOCKICONID.SIID_ERROR
                : icon == MessageBoxIcon.Question
                ? SHSTOCKICONID.SIID_HELP
                : icon == MessageBoxIcon.Exclamation ||
                  icon == MessageBoxIcon.Warning
                ? SHSTOCKICONID.SIID_WARNING
                : icon == MessageBoxIcon.Asterisk ||
                  icon == MessageBoxIcon.Information
                ? SHSTOCKICONID.SIID_INFO
                : throw new ArgumentOutOfRangeException();
        }

        [Flags]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private struct SHSTOCKICONINFO
        {
            internal uint cbSize;
            internal IntPtr hIcon;
            internal int iSysIconIndex;
            internal int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            internal string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public ListMessageBoxForm(string messageTop, string messageBottom, string title, MessageBoxIcon icon,
            string[] choiceStrings)
        {
            InitializeComponent();

            if (choiceStrings == null || choiceStrings.Length == 0)
            {
                throw new ArgumentException(@"Null or empty", nameof(choiceStrings));
            }

            CreateHandle();

            Text = title;
            MessageTopLabel.Text = messageTop;
            MessageBottomLabel.Text = messageBottom;

            const int bottomAreaHeight = 42;

            for (int i = 0; i < choiceStrings.Length; i++)
            {
                ChoiceListBox.Items.Add(choiceStrings[i]);
            }

            ChoiceListBox.Height = ChoiceListBox.GetItemHeight(0) * (ChoiceListBox.Items.Count + 1).Clamp(6, 21);

            ClientSize = new Size(ClientSize.Width, bottomAreaHeight +
                                                    OuterTLP.Margin.Top +
                                                    OuterTLP.Margin.Bottom +
                                                    ContentTLP.Margin.Top +
                                                    ContentTLP.Margin.Bottom +
                                                    MainFLP.Margin.Top +
                                                    MainFLP.Margin.Bottom +
                                                    MessageTopLabel.Margin.Top +
                                                    MessageTopLabel.Margin.Bottom +
                                                    MessageTopLabel.Height +
                                                    ChoiceListBox.Margin.Top +
                                                    ChoiceListBox.Margin.Bottom +
                                                    ChoiceListBox.Height +
                                                    SelectButtonsFLP.Margin.Top +
                                                    SelectButtonsFLP.Margin.Bottom +
                                                    SelectButtonsFLP.Height +
                                                    MessageBottomLabel.Margin.Top +
                                                    MessageBottomLabel.Margin.Bottom +
                                                    MessageBottomLabel.Height);

            int innerControlWidth = MainFLP.Width - 10;
            ChoiceListBox.Width = innerControlWidth;
            SelectButtonsFLP.Width = innerControlWidth + 1;

            ChoiceListBox.SetSelected(0, true);

            if (icon != MessageBoxIcon.None)
            {
                SHSTOCKICONID sysIcon = GetSystemIcon(icon);

                SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
                try
                {
                    sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                    int result = SHGetStockIconInfo(sysIcon, SHGSI.SHGSI_ICON, ref sii);
                    Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

                    IconPictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
                }
                catch
                {
                    // "Wrong style" image (different style from the MessageBox one) but better than nothing if
                    // the above fails
                    IconPictureBox.Image = SystemIcons.Warning.ToBitmap();
                }
                finally
                {
                    DestroyIcon(sii.hIcon);
                }
            }
        }

        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            if (ChoiceListBox.Items.Count > 0)
            {
                for (int i = 0; i < ChoiceListBox.Items.Count; i++)
                {
                    ChoiceListBox.SetSelected(i, true);
                }
            }
        }

        private void ListMessageBoxForm_Load(object sender, EventArgs e)
        {

        }
    }
}
