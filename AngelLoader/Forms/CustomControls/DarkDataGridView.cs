using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkDataGridView : DataGridView, IDarkable
    {
        private bool _darkModeEnabled;

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    RowsDefaultCellStyle.ForeColor = DarkColors.Fen_DarkForeground;
                    GridColor = Color.FromArgb(64, 64, 64);
                    RowsDefaultCellStyle.BackColor = DarkColors.Fen_DarkBackground;
                }
                else
                {
                    RowsDefaultCellStyle.ForeColor = SystemColors.ControlText;
                    GridColor = SystemColors.ControlDark;
                    RowsDefaultCellStyle.BackColor = SystemColors.Window;
                }
            }
        }

        public DarkDataGridView()
        {
            base.DoubleBuffered = true;
        }

        /// <summary>
        /// If you don't have an actual cell selected (indicated by its header being blue) and you try to move
        /// with the keyboard, it pops back to the top item. This fixes that, and is called wherever appropriate.
        /// </summary>
        internal void SelectProperly(bool suspendResume = true)
        {
            if (Rows.Count == 0 || SelectedRows.Count == 0 || Columns.Count == 0) return;

            // Crappy mitigation for losing horizontal scroll position, not perfect but better than nothing
            int origHSO = HorizontalScrollingOffset;

            try
            {
                // Note: we need to do this null check here, otherwise we get an exception that doesn't get caught(!!!)
                SelectedRows[0].Cells[FirstDisplayedCell?.ColumnIndex ?? 0].Selected = true;
            }
            catch
            {
                // It can't be selected for whatever reason. Oh well.
            }

            try
            {
                if (suspendResume) this.SuspendDrawing();
                if (HorizontalScrollBar.Visible && HorizontalScrollingOffset != origHSO)
                {
                    HorizontalScrollingOffset = origHSO;
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        internal void SendKeyDown(KeyEventArgs e) => OnKeyDown(e);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_darkModeEnabled) return;

            if (BorderStyle == BorderStyle.FixedSingle)
            {
                e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            }

            if (VerticalScrollBar.Visible && HorizontalScrollBar.Visible)
            {
                int vertScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                int horzScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
                e.Graphics.FillRectangle(DarkColors.DarkBackgroundBrush,
                    VerticalScrollBar.Left,
                    HorizontalScrollBar.Top,
                    vertScrollBarWidth,
                    horzScrollBarHeight);
            }
        }
    }
}
