using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.WinAPI;
using Gma.System.MouseKeyHook;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DarkDateTimePicker2 : UserControl
    {
        private sealed class CalendarDropDown : ToolStripDropDown
        {
            internal readonly MonthCalendar Calendar;

            internal CalendarDropDown()
            {
                // Must do this to enable the border-remove hack below
                AutoSize = false;

                Calendar = new MonthCalendar { MaxSelectionCount = 1 };

                var host = new ToolStripControlHost(Calendar);

                Items.Add(host);
            }

            protected override void OnOpened(EventArgs e)
            {
                // Hack to get rid of the border and edges (Padding, Margin, Dock, etc. for any and all subcontrols
                // is futile and does nothing)
                Location = new Point(Location.X + 1, Location.Y - 1);
                Calendar.Location = new Point(-1, -1);
                Size = new Size(Calendar.Width - 2, Calendar.Height - 2);

                // Must do this explicitly otherwise we don't get focus
                Calendar.Focus();

                base.OnOpened(e);
            }
        }

        private readonly CalendarDropDown _calendarDropDown;

        private static CultureInfo _cultureInfo = null!;
        private static bool _forceClassicMode;
        private static int _yearMaxWidth;
        private static int _monthMaxWidth;
        private static int _dayMaxWidth;
        private static int _dateSeparatorWidth;

        [Flags]
        private enum FieldType
        {
            Year = 1,
            Month = 2,
            Day = 4
        }

        public DarkDateTimePicker2()
        {
            InitializeComponent();

            bool SetUpDateFormat()
            {
                _cultureInfo = new CultureInfo(CultureInfo.CurrentCulture.Name, useUserOverride: false);
                int maxNumberWidth = 0;
                for (int i = 0; i < 9; i++)
                {
                    int numWidth = TextRenderer.MeasureText(i.ToString(), Font).Width;
                    if (numWidth > maxNumberWidth) maxNumberWidth = numWidth;
                }
                _yearMaxWidth = maxNumberWidth * 4;
                _monthMaxWidth = maxNumberWidth * 2;
                _dayMaxWidth = maxNumberWidth * 2;

                string sep = _cultureInfo.DateTimeFormat.DateSeparator;

                if (sep.IsEmpty()) return false;

                _dateSeparatorWidth = TextRenderer.MeasureText(sep, Font).Width;
                string dateString = _cultureInfo.DateTimeFormat.ShortDatePattern;

                string[] fields = dateString.Split(new[] { sep },
                    StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < fields.Length; i++)
                {
                    string field = fields[i];
                    string fieldTrimmed = field.Trim();

                    // Account for 'г.' and 'ý.' for bg-BG and tk-TK respectively
                    var m = Regex.Match(fieldTrimmed, @"\s+'.+'$");
                    if (m.Success)
                    {
                        fieldTrimmed = fieldTrimmed.Substring(0, m.Index);
                    }

                    FieldType fieldType = 0;

                    if (fieldTrimmed == "yy" || fieldTrimmed == "yyyy")
                    {
                        if ((fieldType & FieldType.Year) != 0) return false;
                        fieldType |= FieldType.Year;
                        if (m.Success) _yearMaxWidth += TextRenderer.MeasureText(m.Value, Font).Width;
                    }
                    else if (fieldTrimmed == "M" || fieldTrimmed == "MM")
                    {
                        if ((fieldType & FieldType.Month) != 0) return false;
                        fieldType |= FieldType.Month;
                        if (m.Success) _monthMaxWidth += TextRenderer.MeasureText(m.Value, Font).Width;
                    }
                    else if (fieldTrimmed == "d" || fieldTrimmed == "dd")
                    {
                        if ((fieldType & FieldType.Day) != 0) return false;
                        fieldType |= FieldType.Day;
                        if (m.Success) _dayMaxWidth += TextRenderer.MeasureText(m.Value, Font).Width;
                    }

                    if ((fieldType & FieldType.Year) != 0 &&
                        (fieldType & FieldType.Month) != 0 &&
                        (fieldType & FieldType.Day) != 0)
                    {
                        break;
                    }

                    continue;
                }

                return true;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_cultureInfo == null)
            {
                if (!SetUpDateFormat())
                {
                    Trace.WriteLine(nameof(SetUpDateFormat) + " returned false");
                    _forceClassicMode = true;
                }
            }

            _calendarDropDown = new CalendarDropDown();

            DropDownButtonPanel.BackColor = DarkColors.Fen_ControlBackground;

            DropDownButtonPanel.MouseDown += DropDownButtonPanel_MouseDown;

            ControlUtils.MouseHook.MouseDownExt += MouseHook_MouseDownExt;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Trace.WriteLine("FOCUSED");
            base.OnGotFocus(e);
        }

        private void MouseHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (_calendarDropDown.Visible)
            {
                _calendarDropDown.Close();
                e.Handled = true;
            }
        }

        private void DropDownButtonPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_calendarDropDown.Visible)
            {
                _calendarDropDown.Show(DropDownButtonPanel, 0, DropDownButtonPanel.Height);
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                ControlUtils.MouseHook.MouseDownExt -= MouseHook_MouseDownExt;
            }
            base.Dispose(disposing);
        }
    }
}
