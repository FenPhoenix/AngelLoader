using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    internal class TabControlCustom : TabControl
    {
        private TabPage DragTab;

        public TabControlCustom()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            DragTab = GetTabAtPoint(e.Location);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || DragTab == null || TabCount <= 1) return;

            int dragTabIndex = TabPages.IndexOf(DragTab);
            var dragTabRect = GetTabRect(dragTabIndex);

            if (dragTabIndex < TabPages.Count - 1 &&
                e.Location.X < dragTabRect.Left + GetTabRect(dragTabIndex + 1).Width &&
                dragTabIndex > 0 &&
                e.Location.X > GetTabRect(dragTabIndex - 1).Left + dragTabRect.Width)
            {
                return;
            }

            var newTab = GetTabAtPoint(e.Location);
            if (newTab == null || newTab == DragTab) return;

            int newTabIndex = TabPages.IndexOf(newTab);
            TabPages[dragTabIndex] = newTab;
            TabPages[newTabIndex] = DragTab;

            SelectedTab = DragTab;

            base.OnMouseMove(e);
        }

        private TabPage GetTabAtPoint(Point position)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(position)) return TabPages[i];
            }

            return null;
        }
    }
}
