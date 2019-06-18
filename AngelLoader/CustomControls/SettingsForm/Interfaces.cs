using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.CustomControls.SettingsForm
{
    internal class Interfaces
    {
        internal interface ISettingsPage
        {
            void SetVScrollPos(int value);
            int GetVScrollPos();
            void ShowPage();
            void HidePage();
        }
    }
}
