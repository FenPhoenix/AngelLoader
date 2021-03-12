using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public partial class DarkDateTimePicker2 : UserControl
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

        public DarkDateTimePicker2()
        {
            InitializeComponent();
        }
    }
}
