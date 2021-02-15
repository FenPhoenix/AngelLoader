using System;

namespace DarkUI.Controls
{
    public class DarkModeChangedEventArgs : EventArgs
    {
        public bool DarkModeEnabled { get; private set; }

        public DarkModeChangedEventArgs(bool darkModeEnabled)
        {
            DarkModeEnabled = darkModeEnabled;
        }
    }
}
