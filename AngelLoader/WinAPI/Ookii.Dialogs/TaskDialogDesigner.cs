// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.ComponentModel.Design;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    [PublicAPI]
    public class TaskDialogDesigner : ComponentDesigner
    {
        public override DesignerVerbCollection Verbs => new DesignerVerbCollection { new DesignerVerb(OokiiResources.Preview, Preview) };

        private void Preview(object sender, EventArgs e) => ((TaskDialog)Component).ShowDialog();
    }
}
