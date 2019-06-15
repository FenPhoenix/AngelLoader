using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls
{
    internal sealed class ToolStripCustom : ToolStrip
    {
        /// <summary>
        /// Fiddle this around to get the right-side garbage line to disappear again when Padding is set to something.
        /// </summary>
        [Browsable(true)] public int PaddingDrawNudge { get; set; } = 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Hack in order to be able to have ManagerRenderMode, but also get rid of any garbage around the
            // edges that may be drawn. In particular, there's an ugly visual-styled vertical line at the right
            // side if you don't do this.
            var rect1 = new Rectangle(0, 0, Items[0].Bounds.X, Height);
            var last = Items[Items.Count - 1];
            var rect2Start = last.Bounds.X + last.Bounds.Width;
            var rect2 = new Rectangle(rect2Start - PaddingDrawNudge, 0, (Width - rect2Start) + PaddingDrawNudge, Height);

            e.Graphics.FillRectangle(SystemBrushes.Control, rect1);
            e.Graphics.FillRectangle(SystemBrushes.Control, rect2);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripButtonCustom : ToolStripButton
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            // Use the mouseover BackColor when it's checked, for a more visible checked experience
            if (Checked) e.Graphics.FillRectangle(Brushes.LightSkyBlue, 0, 0, Width, Height);
            base.OnPaint(e);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripSeparatorCustom : ToolStripSeparator
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            // These ToolStrip abominations just won't behave. They claim to have their color set to "Control",
            // but what they really mean is "almost Control but not quite". Same with any other hue. Have to set
            // it here just to get them to listen.
            e.Graphics.FillRectangle(SystemBrushes.Control, 0, 0, Width, Height);
            base.OnPaint(e);
        }
    }

    #region Dropdown working

    public class ToolStripDropDownCustom : ToolStripDropDownMenu
    {
        private ToolStripDropDownButton Owner;
        private string oldToolTipText;

        public ToolStripDropDownCustom()
        {
            //AutoClose = false;
        }

        public void Inject(ToolStripDropDownButton owner) => Owner = owner;

        // This AutoClose toggling thing is necessary because if we leave AutoClose on, it will close when the
        // mouse merely moves over another ToolStrip item. So we want it off when we're open, but when we want
        // to close our dropdown, we have to turn it on again.
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Owner != null)
            {
                if (Visible)
                {
                    oldToolTipText = Owner.ToolTipText;
                    Owner.ToolTipText = "";
                }
                else
                {
                    Owner.ToolTipText = oldToolTipText;
                }
            }

            if (Visible)
            {
                //AutoClose = false;
            }
            base.OnVisibleChanged(e);
        }

        public void HideThis()
        {
            //AutoClose = true;
            Hide();
        }

        public bool CursorOutsideDropDown()
        {
            Rectangle? cbDDRect = null;
            foreach (ToolStripItem tsItem in Items)
            {
                if (tsItem is ToolStripComboBox cb && cb.ComboBox != null && cb.ComboBox.DroppedDown)
                {
                    // It should only be possible to have one combobox dropped-down at a time, so just break on
                    // the first dropped-down one found
                    cbDDRect = cb.ComboBox.GetDropDownRect();
                    break;
                }
            }

            var curPos = PointToClient(Cursor.Position);
            return !ClientRectangle.Contains(curPos) && (cbDDRect == null || !((Rectangle)cbDDRect).Contains(curPos));
        }
    }

    // Welp, it finally happened. I finally used inheritance on one of my own classes. *shudder*
    public sealed class FilterByRatingDropDown : ToolStripDropDownCustom, ILocalizable
    {
        public ToolStripLabel FromLabel = new ToolStripLabel();
        public ToolStripLabel ToLabel = new ToolStripLabel();
        public ToolStripComboBoxCustom FromComboBox = new ToolStripComboBoxCustom { AutoSize = false, Width = 150 };
        public ToolStripComboBoxCustom ToComboBox = new ToolStripComboBoxCustom { AutoSize = false, Width = 150 };
        public ToolStripNormalButtonCustom ResetButton = new ToolStripNormalButtonCustom();

        public FilterByRatingDropDown()
        {
            AutoSize = true;
            ShowCheckMargin = false;
            ShowImageMargin = false;

            //FromComboBox.ComboBox.Width = 150;
            //ToComboBox.ComboBox.Width = 150;

            Items.Add(FromLabel);
            Items.Add(FromComboBox);
            Items.Add(ToLabel);
            Items.Add(ToComboBox);
            Items.Add(ResetButton);
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            FromLabel.Text = LText.RatingFilterBox.From;
            ToLabel.Text = LText.RatingFilterBox.To;

            ResetButton.Text = LText.Global.Reset;
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripCheckBoxCustom : ToolStripControlHost
    {
        public CheckBox CheckBox => Control as CheckBox;

        public ToolStripCheckBoxCustom() : base(new CheckBox()) { }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripComboBoxCustom : ToolStripControlHost
    {
        public ComboBox ComboBox => Control as ComboBox;

        public ToolStripComboBoxCustom() : base(new ComboBox()) { }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripNormalButtonCustom : ToolStripControlHost
    {
        public Button Button => Control as Button;

        public ToolStripNormalButtonCustom() : base(new Button()) { }
    }

    #endregion
}
