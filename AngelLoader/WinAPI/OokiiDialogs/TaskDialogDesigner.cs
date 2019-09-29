// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.ComponentModel;
using AngelLoader.WinAPI.OokiiDialogs;

namespace Ookii.Dialogs.WinForms
{
    class TaskDialogDesigner : ComponentDesigner
    {
        public override DesignerVerbCollection Verbs
        {
            get
            {
                DesignerVerbCollection verbs = new DesignerVerbCollection();
                verbs.Add(new DesignerVerb(OokiiResources.Preview, new EventHandler(Preview)));
                return verbs;
            }
        }

        private void Preview(object sender, EventArgs e)
        {
            ((TaskDialog)Component).ShowDialog();
        }
    }
}
