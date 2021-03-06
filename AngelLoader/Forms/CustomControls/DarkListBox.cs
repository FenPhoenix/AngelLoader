using System;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkListBox : DarkDataGridView
    {
        private bool _loaded;

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

        public DarkListBox()
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

            // Full-width item hack part un: Set the column to accomodate to the longest item.
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            RowHeadersVisible = false;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            RowsDefaultCellStyle.BackColor = SystemColors.Window;

            Columns[0].CellTemplate = new DataGridViewTextBoxCell();

            StandardTab = true;

            ShowCellToolTips = false;
        }

        #region Public methods

        public string[] GetRowValuesAsStrings()
        {
            var ret = new string[RowCount];
            for (int i = 0; i < RowCount; i++)
            {
                ret[i] = Rows[i].Cells[0].Value?.ToString() ?? "";
            }
            return ret;
        }

        #endregion

        #region Public properties

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

        public string[] SelectedItems
        {
            get
            {
                string[] ret = new string[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    ret[i] = SelectedRows[i].Cells[0].Value?.ToString() ?? "";
                }
                return ret;
            }
        }

        public int ItemHeight => Font.Height + 4;

        #endregion

        #region Event overrides

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (!_loaded) return;
            base.OnSelectionChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // Hack to get the damn first row unselected on first show
            if (Visible && !_loaded)
            {
                ClearSelection();
                _loaded = true;
            }

            base.OnVisibleChanged(e);
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

            if (Rows[e.RowIndex].Selected)
            {
                // Full-width item hack part deux: The AllCells autosize mode will still end up making the column
                // shorter than the canvas if the longest ITEM is shorter than the canvas. We want it to be max
                // the longest item, and min the client width. But we can't. Even when we try to set the minimum
                // width in OnClientSize(), it doesn't work. By which I mean, it sets the size the FIRST time
                // it's run, but not any subsequent run. I don't have time to deal with this shit, so:
                // Just draw the selection rectangle the width of the client. That takes care of the visual part.
                var selRect = new Rectangle(
                    e.CellBounds.X,
                    e.CellBounds.Y,
                    ClientRectangle.Width - e.CellBounds.X,
                    e.CellBounds.Height
                );

                Brush brush = _darkModeEnabled ? DarkColors.BlueSelectionBrush : SystemBrushes.Highlight;

                e.Graphics.FillRectangle(brush, selRect);
                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Full-width item hack part trois: Even though we're drawing the selection full-width, the mouse
            // down will still do nothing if it's outside the actual column rectangle. So just set the X coord
            // to way over on the left all the time, and that makes it work like you'd expect. Repulsive, but
            // there you are.
            base.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, 2, e.Y, e.Delta));
        }

        #endregion
    }
}
