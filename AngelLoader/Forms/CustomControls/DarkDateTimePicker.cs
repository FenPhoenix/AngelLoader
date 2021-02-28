using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using AL_Common;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkDateTimePicker : DateTimePicker, IDarkable
    {
        /*
        TODO: @DarkMode(DarkDateTimePicker): Hoo boy
        We could either:
        -Take the short date format string of the current culture (making sure to construct with
         new CultureInfo(name, useUserOverride: false)) and then take the longest of every field and measure the
         longest possible date string, and if it's longer than the control then just fall back to non-dark.
         Otherwise, use our knowledge of the max length of each field (and since we're ignoring user overrides,
         we know there won't be anything stupid like duplicate fields or whatever) to know what field we should
         select when the user clicks. Also keep track of which field is selected permanently; it doesn't reset
         on de-focus. We need to handle keyboard left+right to select the prev/next fields (note they wrap too).
         Because we're using built-in formats we know we're going to only have m/d/y in some format or other,
         so three valid fields, which simplifies things.
        Or:
        -We could just make our own composite control and slap the dates in there so we have full control of them
         separately. We'd have to duplicate the scrolling-on-too-long behavior of the vanilla control. We would
         also have to programmatically open and close the calendar dropdown somehow.
         For that, we should try this: https://stackoverflow.com/a/56129920

        Notes:
        -There are only two cultures currently (2021-02-26) that have anything other than numbers in their short
         dates:
         ky: d-MMM yy
         ky-KG: d-MMM yy
         That's Kyrgyz and Kyrgyz-Kyrgyzstan. For that we can just fall back to classic-mode I guess, either that
         or shorten the MMM to MM and then we have nice numbers again.
         So actually, presuming we ignore user overrides, we have it really easy here. Just numbers. Oh, except
         for any stuff at the end (see below)... but, we've also left a generous amount of extra space, so maybe
         not really an issue?
        -We should always change any amount of y chars to yyyy (4-digit year).
        -When parsing and cross-referencing the format string, we need to consider only m/d/y, and we should
         display extra stuff like 'г.' (bg-BG) but not consider it a selectable field (this matches vanilla
         behavior).
        */

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                // Original:
                // ControlStyles.UserPaint == false
                // ControlStyles.AllPaintingInWmPaint == true
                // ControlStyles.OptimizedDoubleBuffer == false

                if (_darkModeEnabled)
                {
                    // Re-enable for final
                    //SetStyle(
                    //    ControlStyles.UserPaint
                    //    | ControlStyles.AllPaintingInWmPaint
                    //    | ControlStyles.OptimizedDoubleBuffer
                    //    , true);
                }
                else
                {
                    SetStyle(
                        ControlStyles.AllPaintingInWmPaint
                        , true);
                    SetStyle(
                        ControlStyles.UserPaint
                        | ControlStyles.OptimizedDoubleBuffer
                        , false);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Remove for final
            return;

            if (!_darkModeEnabled) return;

            //var dti = new Native.DATETIMEPICKERINFO { cbSize = (int)Marshal.SizeOf(typeof(Native.DATETIMEPICKERINFO)) };
            //Native.SendMessage(Handle, Native.DTM_GETDATETIMEPICKERINFO, IntPtr.Zero, ref dti);

            //Trace.WriteLine("Edit handle: " + dti.hwndEdit.ToString());
            //Trace.WriteLine(dti.rcButton.left);

            e.Graphics.FillRectangle(DarkColors.LightBackgroundBrush, ClientRectangle);

            string textDate = Format == DateTimePickerFormat.Custom && !CustomFormat.IsEmpty()
                ? CustomFormat
                : Format == DateTimePickerFormat.Long
                ? Value.ToLongDateString()
                : Format == DateTimePickerFormat.Short
                ? Value.ToShortDateString()
                : Format == DateTimePickerFormat.Time
                ? Value.ToShortTimeString()
                : "";

            const TextFormatFlags textFormatFlags =
                TextFormatFlags.Default;

            //CultureInfo.CurrentCulture.DateTimeFormat.

            var dtf = CultureInfo.CurrentCulture.DateTimeFormat;

            //var blah=new DateTime().ToShortDateString()

            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, DarkColors.LightText);
        }

        protected override void OnValueChanged(EventArgs eventargs)
        {
            Invalidate();
            base.OnValueChanged(eventargs);
        }

        private Random _random = new Random();

        private void WmReflectCommand(ref Message m)
        {
            if (!(m.HWnd == this.Handle))
                return;

            int code = ((Native.NMHDR)m.GetLParam(typeof(Native.NMHDR))).code;

            //Trace.WriteLine("code: " + code);

            switch (code)
            {
                case -0x2F7:
                    //Trace.WriteLine(_random.Next() + " thinged?");
                    //this.WmDateTimeChange(ref m);
                    break;
                case -0x2F2:
                    //this.WmDropDown(ref m);
                    break;
                case -0x2F1:
                    //this.WmCloseUp(ref m);
                    break;
            }
        }

        protected override void WndProc(ref Message m)
        {
            //Trace.WriteLine(m.Msg.ToString("X"));
            if (m.Msg == 0x204E)
            {
                //WmReflectCommand(ref m);
            }
            base.WndProc(ref m);
        }
    }
}
