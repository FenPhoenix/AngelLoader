using System;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkListBox2 : DarkDataGridView
    {
        private bool _darkModeEnabled;
        public override bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    BackgroundColor = DarkColors.Fen_DarkBackground;
                }
                else
                {
                    BackgroundColor = SystemColors.Window;
                }
                base.DarkModeEnabled = value;
            }
        }

        public DarkListBox2()
        {
            BorderStyle = BorderStyle.FixedSingle;

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToOrderColumns = true;
            AllowUserToResizeColumns = false;
            AllowUserToResizeRows = false;

            BackgroundColor = SystemColors.Window;
            CellBorderStyle = DataGridViewCellBorderStyle.None;

            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersVisible = false;
            Columns.Add(new DataGridViewColumn());
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            RowTemplate.ReadOnly = true;

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            RowHeadersVisible = false;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            RowsDefaultCellStyle.BackColor = SystemColors.Window;

            Columns[0].CellTemplate = new DataGridViewTextBoxCell();

            StandardTab = true;
        }

        // Hack to get the damn first row unselected on first show
        private bool _loaded;
        protected override void OnParentVisibleChanged(EventArgs e)
        {
            if (!_loaded)
            {
                ClearSelection();
                _loaded = true;
            }
            base.OnParentVisibleChanged(e);
        }

        public string[] GetRowValuesAsStrings()
        {
            var ret = new string[RowCount];
            for (int i = 0; i < RowCount; i++)
            {
                ret[i] = Rows[i].Cells[0].Value?.ToString() ?? "";
            }
            return ret;
        }

        public int SelectedIndex
        {
            get => SelectedRows.Count == 0 ? -1 : SelectedRows[0].Index;
            set
            {
                if (value < -1 || value >= RowCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(SelectedIndex), value, "Index was outside the bounds of the row count");
                }

                if (value == -1)
                {
                    ClearSelection();
                }
                else
                {
                    Rows[value].Selected = true;
                }
            }
        }

        public string SelectedItem => SelectedRows.Count == 0 ? "" : SelectedRows[0].Cells[0].Value?.ToString() ?? "";

        // Hack to keep the cells from being less than the width of the canvas
        protected override void OnClientSizeChanged(EventArgs e)
        {
            Columns[0].MinimumWidth = ClientSize.Width - 2;
            base.OnClientSizeChanged(e);
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            // Hack to get the horizontal scroll bar to disappear if there's no rows
            ScrollBars = ScrollBars.Both;
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                Rows[i].MinimumHeight = Font.Height + 4;
                Rows[i].Height = Font.Height + 4;
            }
            base.OnRowsAdded(e);
        }

        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            // Hack to get the horizontal scroll bar to disappear if there's no rows
            if (Rows.Count == 0) ScrollBars = ScrollBars.Vertical;
            base.OnRowsRemoved(e);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);

            if (_darkModeEnabled && SelectedIndex == e.RowIndex)
            {
                e.Graphics.FillRectangle(DarkColors.BlueSelectionBrush, e.CellBounds);
                e.Paint(e.ClipBounds, DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
            }
        }
    }
}
