using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Forms
{
    internal static class ControlExtensions
    {
        #region Suspend/resume drawing

        internal static void SuspendDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
        }

        internal static void ResumeDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Refresh();
        }

        #endregion

        /// <summary>
        /// Sets the progress bar's value instantly. Avoids the la-dee-dah catch-up-when-I-feel-like-it nature of
        /// the progress bar that makes it look annoying and unprofessional.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="value"></param>
        public static void SetValueInstant(this ProgressBar pb, int value)
        {
            if (value == pb.Maximum)
            {
                pb.Value = pb.Maximum;
            }
            else
            {
                pb.Value = (value + 1).Clamp(pb.Minimum, pb.Maximum);
                pb.Value = value.Clamp(pb.Minimum, pb.Maximum);
            }
        }

        #region Centering

        [PublicAPI]
        internal static void CenterH(this Control control, Control parent)
        {
            control.Location = new Point((parent.Width / 2) - (control.Width / 2), control.Location.Y);
        }

        /*
        [PublicAPI]
        internal static void CenterV(this Control control, Control parent)
        {
            control.Location = new Point(control.Location.X, (parent.Height / 2) - (control.Height / 2));
        }
        */

        [PublicAPI]
        internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
        {
            int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
            int pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
            control.Location = new Point((pWidth / 2) - (control.Width / 2), (pHeight / 2) - (control.Height / 2));
        }

        #endregion

        #region Autosizing

        // PERF_TODO: These are relatively expensive operations (10ms to make 3 calls from SettingsForm)
        // See if we can manually calculate some or all of this and end up with the same result as if we let the
        // layout do the work as we do now.

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes it horizontally to accomodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        /// <param name="preserveHeight"></param>
        internal static void SetTextAutoSize(this Button button, string text, int minWidth = -1, bool preserveHeight = false)
        {
            // Buttons can't be GrowOrShrink because that also shrinks them vertically. So do it manually here.
            // TODO: SetTextAutoSize: SetStyle(FixedHeight, true) looks like it would prevent this?
            // But we want minimum width a lot of the time, so maybe not... Also, we'd need to bloat ourselves up
            // with a whole custom button class just to be able to use SetStyle (it's a protected method).
            button.Text = "";
            button.Width = 2;
            if (!preserveHeight) button.Height = 2;
            button.Text = text;

            if (minWidth > -1 && button.Width < minWidth) button.Width = minWidth;
        }

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes and repositions the <see cref="Button"/> and a
        /// <see cref="TextBox"/> horizontally together to accommodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        internal static void SetTextAutoSize(this Button button, TextBox textBox, string text, int minWidth = -1)
        {
            // Quick fix for this not working if layout is suspended.
            // This will then cause any other controls within the same parent to do their full layout.
            // If this becomes a problem, come up with a better solution here.
            button.Parent.ResumeLayout();

            AnchorStyles oldAnchor = button.Anchor;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            int oldWidth = button.Width;

            button.SetTextAutoSize(text, minWidth);

            int diff =
                button.Width > oldWidth ? -(button.Width - oldWidth) :
                button.Width < oldWidth ? oldWidth - button.Width : 0;

            button.Left += diff;
            // For some reason the diff doesn't work when scaling is > 100% so, yeah
            textBox.Width = button.Left > textBox.Left ? (button.Left - textBox.Left) - 1 : 0;

            button.Anchor = oldAnchor;
        }

        #endregion

        internal static void RemoveAndSelectNearest(this ListBox listBox)
        {
            if (listBox.SelectedIndex == -1) return;

            int oldSelectedIndex = listBox.SelectedIndex;

            listBox.Items.RemoveAt(listBox.SelectedIndex);

            if (oldSelectedIndex < listBox.Items.Count && listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex;
            }
            else if (listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex - 1;
            }
            else if (listBox.Items.Count == 1)
            {
                listBox.SelectedIndex = 0;
            }
        }
    }
}
