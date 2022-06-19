﻿using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkContextMenu : ContextMenuStrip
    {
        private bool _preventClose;
        private ToolStripMenuItemCustom[]? _preventCloseItems;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                RefreshDarkModeState();
            }
        }

        #region Constructors

        public DarkContextMenu(IContainer container) : base(container) { }

        #endregion

        internal void SetPreventCloseOnClickItems(params ToolStripMenuItemCustom[] items) => _preventCloseItems = items;

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (_preventCloseItems?.Length > 0)
            {
                _preventClose = _preventCloseItems.Contains(e.ClickedItem) && ((ToolStripMenuItemCustom)e.ClickedItem).CheckOnClick;
            }

            base.OnItemClicked(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (_preventCloseItems?.Length > 0 && _preventClose)
            {
                _preventClose = false;
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        internal void RefreshDarkModeState()
        {
            void SetMenuTheme(ToolStripDropDown menu)
            {
                if (_darkModeEnabled)
                {
                    menu.Renderer = new DarkMenuRenderer();
                }
                else
                {
                    menu.RenderMode = ToolStripRenderMode.ManagerRenderMode;

                    // Prevents wrong back color on separators
                    menu.ResetBackColor();
                    menu.ResetForeColor();

                    // Prevents wrong back/fore color on items
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.ResetBackColor();
                        item.ResetForeColor();
                    }
                }

                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.DropDown != null)
                    {
                        SetMenuTheme(menuItem.DropDown);
                    }
                }
            }

            SetMenuTheme(this);
        }
    }
}
