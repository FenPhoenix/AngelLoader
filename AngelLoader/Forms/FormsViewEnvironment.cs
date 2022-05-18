using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.Forms
{
    public sealed class FormsViewEnvironment : IViewEnvironment
    {
        public IDialogs GetDialogs() => new Dialogs();
    }
}
