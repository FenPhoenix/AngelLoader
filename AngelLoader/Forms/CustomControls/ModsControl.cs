using System;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class ModsControl : UserControl
    {
        public ModsControl()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
        }

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsEnableAllButtonClick;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisableNonImportantButtonClick;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsShowUberCheckBoxCheckedChanged;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisabledModsTextBoxTextChanged;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<KeyEventArgs>? ModsDisabledModsTextBoxKeyDown;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisabledModsTextBoxLeave;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<DarkCheckList.DarkCheckListEventArgs>? ModsCheckListItemCheckedChanged;

        private void ModsEnableAllButton_Click(object sender, EventArgs e)
        {
            ModsEnableAllButtonClick?.Invoke(ModsEnableAllButton, e);
        }

        private void ModsDisableNonImportantButton_Click(object sender, EventArgs e)
        {
            ModsDisableNonImportantButtonClick?.Invoke(ModsDisableNonImportantButton, e);
        }

        private void ModsShowUberCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ModsShowUberCheckBoxCheckedChanged?.Invoke(ModsShowUberCheckBox, e);
        }

        private void ModsDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxTextChanged?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsDisabledModsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ModsDisabledModsTextBoxKeyDown?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxLeave?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsCheckList_ItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            ModsCheckListItemCheckedChanged?.Invoke(ModsCheckList, e);
        }
    }
}
