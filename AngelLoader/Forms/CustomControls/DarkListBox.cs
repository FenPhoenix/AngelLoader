using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkListBox : ListBox, IDarkableScrollableNative
    {
        // TODO: @DarkMode(DarkListBox): We have too many problems. Flickering, scroll bars behave wrong.
        // Just switch to a different control. ListBoxes are GARBAGE.

        [PublicAPI]
        public sealed class ListBoxObjectCollectionCustom : ObjectCollection
        {
            private readonly DarkListBox _owner;

            public ListBoxObjectCollectionCustom(DarkListBox owner) : base(owner)
            {
                _owner = owner;
            }

            public ListBoxObjectCollectionCustom(DarkListBox owner, ObjectCollection value) : base(owner, value)
            {
                _owner = owner;
            }

            public ListBoxObjectCollectionCustom(DarkListBox owner, object[] value) : base(owner, value)
            {
                _owner = owner;
            }

            public new void Add(object item)
            {
                _owner.ItemsBase.Add(item);
                _owner.ComputeMaxItemWidth();
            }

            public new void AddRange(object[] items)
            {
                _owner.ItemsBase.AddRange(items);
                _owner.ComputeMaxItemWidth();
            }

            public new void AddRange(ObjectCollection value)
            {
                _owner.ItemsBase.AddRange(value);
                _owner.ComputeMaxItemWidth();
            }

            public new void Clear()
            {
                _owner.ItemsBase.Clear();
                _owner.ComputeMaxItemWidth();
            }

            public new void Insert(int index, object item)
            {
                _owner.ItemsBase.Insert(index, item);
                _owner.ComputeMaxItemWidth();
            }

            public new void Remove(object value)
            {
                _owner.ItemsBase.Remove(value);
                _owner.ComputeMaxItemWidth();
            }

            public new void RemoveAt(int index)
            {
                _owner.ItemsBase.RemoveAt(index);
                _owner.ComputeMaxItemWidth();
            }

            public new object this[int index]
            {
                get => _owner.ItemsBase[index];
                set
                {
                    _owner.ItemsBase[index] = value;
                    _owner.ComputeMaxItemWidth();
                }
            }

            public new int Count => _owner.ItemsBase.Count;

            public IEnumerable<TResult> Cast<TResult>() => _owner.ItemsBase.Cast<TResult>();
        }

        private int _updateCount;

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
                SetUpTheme();
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public new void BeginUpdate()
        {
            base.BeginUpdate();
            _updateCount++;
        }

        public new void EndUpdate()
        {
            base.EndUpdate();
            if (_updateCount <= 0) return;
            _updateCount--;
            if (_updateCount == 0) ComputeMaxItemWidth();
        }

        internal void ComputeMaxItemWidth()
        {
            if (!_darkModeEnabled || _updateCount > 0)
            {
                return;
            }

            // Debug to see if it ever runs multiple times
            // TODO: @DarkMode: Remove this when done
            Trace.WriteLine(nameof(ComputeMaxItemWidth));

            int maxItemWidth = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                int itemWidth = TextRenderer.MeasureText(
                        Items[i].ToString(),
                        Font,
                        new Size(short.MaxValue, short.MaxValue),
                        TextFormatFlags.SingleLine)
                    .Width;
                if (itemWidth > maxItemWidth) maxItemWidth = itemWidth;
            }

            HorizontalExtent = maxItemWidth.ClampToZero();

            RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        public new ListBoxObjectCollectionCustom Items { get; }
        private ObjectCollection ItemsBase => base.Items;

        public DarkListBox()
        {
            base.DoubleBuffered = true;
            Items = new ListBoxObjectCollectionCustom(this);
            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        private void SetUpTheme()
        {
            if (_darkModeEnabled)
            {
                DrawMode = DrawMode.OwnerDrawVariable;

                BackColor = DarkColors.Fen_ControlBackground;
                ForeColor = DarkColors.LightText;
            }
            else
            {
                BackColor = SystemColors.Window;
                ForeColor = SystemColors.WindowText;

                DrawMode = DrawMode.Normal;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnDrawItem(e);
                return;
            }

            if (e.Index == -1) return;

            // TODO: @DarkMode(DarkListBox): We flicker badly when we scroll horizontally, but only when we're focused
            e.Graphics.FillRectangle((e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? DarkColors.BlueSelectionBrush
                : DarkColors.Fen_ControlBackgroundBrush, e.Bounds);

            // No TextAlign property, so leave constant
            const TextFormatFlags textFormat =
                TextFormatFlags.Default |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoClipping |
                TextFormatFlags.SingleLine;

            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), e.Font, e.Bounds, e.ForeColor, textFormat);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Suspended { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control? ClosestAddableParent => Parent;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? DarkModeChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? Scroll;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? RefreshIfNeededForceCorner;

        private void DrawBorder(IntPtr hWnd)
        {
            if (!_darkModeEnabled || BorderStyle == BorderStyle.None) return;

            using var dc = new Native.DeviceContext(hWnd);
            using Graphics g = Graphics.FromHdc(dc.DC);
            g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
            g.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        protected override void WndProc(ref Message m)
        {
            Trace.WriteLine(m.Msg.ToString("x8"));
            switch (m.Msg)
            {
                case Native.WM_PAINT:
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                case Native.WM_ERASEBKGND:
                    base.WndProc(ref m);
                    if (_darkModeEnabled && !Suspended)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case Native.WM_CTLCOLORSCROLLBAR:
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                        if (m.Msg == Native.WM_NCPAINT)
                        {
                            DrawBorder(m.HWnd);
                        }
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
