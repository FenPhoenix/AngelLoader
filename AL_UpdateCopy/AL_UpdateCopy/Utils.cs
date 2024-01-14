using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AL_UpdateCopy;

internal static class Utils
{
    internal static void CenterH(this Control control, Control parent, bool clientSize = false)
    {
        int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
        control.Location = control.Location with { X = (pWidth / 2) - (control.Width / 2) };
    }
}
