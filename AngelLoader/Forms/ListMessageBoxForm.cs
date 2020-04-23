using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using JetBrains.Annotations;

namespace AngelLoader.Forms
{
    public sealed partial class ListMessageBoxForm : Form, Misc.ILocalizable
    {
        #region P/Invoke crap

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private enum SHSTOCKICONID : uint
        {
            SIID_HELP = 23,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80
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

        #endregion

        public readonly List<string> SelectedItems = new List<string>();

        public ListMessageBoxForm(string messageTop, string messageBottom, string title, MessageBoxIcon icon,
            string okText, string cancelText, bool okIsDangerous, string[] choiceStrings)
        {
            InitializeComponent();

            if (choiceStrings == null || choiceStrings.Length == 0)
            {
                throw new ArgumentException(@"Null or empty", nameof(choiceStrings));
            }

            #region Set passed-in values

            if (icon != MessageBoxIcon.None) SetIcon(icon);

            Text = title;
            MessageTopLabel.Text = messageTop;
            MessageBottomLabel.Text = messageBottom;

            // Set this first: the list is now populated
            for (int i = 0; i < choiceStrings.Length; i++)
            {
                ChoiceListBox.Items.Add(choiceStrings[i]);
            }

            #endregion

            #region Autosize controls

            // Set this second: the list is now sized based on its content
            ChoiceListBox.Height =
                (ChoiceListBox.ItemHeight * ChoiceListBox.Items.Count.Clamp(5, 20)) +
                ((SystemInformation.BorderSize.Height * 4) + 3);

            // Set this third: all controls sizes are now set, so we can size the window
            const int bottomAreaHeight = 42;
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

            // These can be set anywhere as they don't affect the vertical sizing code
            int innerControlWidth = MainFLP.Width - 10;
            MessageTopLabel.Width = innerControlWidth;
            ChoiceListBox.Width = innerControlWidth;
            SelectButtonsFLP.Width = innerControlWidth + 1;
            MessageBottomLabel.Width = innerControlWidth;

            if (okIsDangerous)
            {
                OKButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                OKButton.ImageAlign = ContentAlignment.MiddleLeft;
                OKButton.Image = Resources.ExclMarkCircleRed_14;
            }

            OKButton.SetTextAutoSize(okText, OKButton.Width);
            Cancel_Button.SetTextAutoSize(cancelText, Cancel_Button.Width);

            #endregion

            ChoiceListBox.SetSelected(0, true);

            Localize();
        }

        private void SetIcon(MessageBoxIcon icon)
        {
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
            try
            {
                SHSTOCKICONID sysIcon =
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

                sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                int result = SHGetStockIconInfo(sysIcon, SHGSI.SHGSI_ICON, ref sii);
                Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

                IconPictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            }
            catch
            {
                // "Wrong style" image (different style from the MessageBox one) but better than nothing if the
                // above fails
                IconPictureBox.Image = SystemIcons.Warning.ToBitmap();
            }
            finally
            {
                DestroyIcon(sii.hIcon);
            }
        }

        public void Localize()
        {
            SelectAllButton.SetTextAutoSize(LText.Global.SelectAll, SelectAllButton.Width);
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

        // Shouldn't happen, but just in case
        private void ChoiceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OKButton.Enabled = ChoiceListBox.SelectedIndex > -1;
        }

        private void ListMessageBoxForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && ChoiceListBox.SelectedIndex > -1)
            {
                foreach (object item in ChoiceListBox.SelectedItems)
                {
                    SelectedItems.Add(item.ToString());
                }
            }
        }
    }
}
