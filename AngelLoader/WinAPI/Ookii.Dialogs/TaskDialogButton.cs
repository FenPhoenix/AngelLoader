// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// A button on a <see cref="TaskDialog"/>.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    //[PublicAPI]
    public class TaskDialogButton : Component//: TaskDialogItem
    {
        private ButtonType _type;

        private int _id;

        //private TaskDialog? _owner;

        public TaskDialogButton()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class with the specified button type.
        /// </summary>
        /// <param name="type">The type of the button.</param>
        public TaskDialogButton(ButtonType type)
        {
            // The item cannot have an owner at this point, so it's not needed to check for duplicates,
            // which is why we can safely use the field and not the property, avoiding the virtual method call.
            _id = (int)type;
            _type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class with the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string text) => Text = text;


        public string Text { get; set; } = "";

        public ButtonType ButtonType
        {
            get => _type;
            set
            {
                _type = value;
                //if (value != ButtonType.Custom)
                //{
                //    _type = value;
                //    Id = (int)value;
                //}
                //else
                //{
                //    _type = value;
                //    AutoAssignId();
                //}
            }
        }

        public string CommandLinkNote { get; set; } = "";

        public bool Default { get; set; }

        internal int Id
        {
            get => _id;
            set
            {
                _id = value;
                //if (_id != value)
                //{
                //    if (_type != ButtonType.Custom)
                //    {
                //        throw new InvalidOperationException(OokiiResources.NonCustomTaskDialogButtonIdError);
                //    }

                //    _id = value;
                //}
            }
        }

        private void AutoAssignId()
        {
            //if (_type == ButtonType.Custom)
            //{
            //    if (ItemCollection == null) return;

            //    int highestId = 9;
            //    foreach (TaskDialogButton item in ItemCollection)
            //    {
            //        if (item.Id > highestId) highestId = item.Id;
            //    }
            //    Id = highestId + 1;
            //}
        }

        internal NativeMethods.TaskDialogCommonButtonFlags ButtonFlag => _type switch
        {
            ButtonType.Ok => NativeMethods.TaskDialogCommonButtonFlags.OkButton,
            ButtonType.Yes => NativeMethods.TaskDialogCommonButtonFlags.YesButton,
            ButtonType.No => NativeMethods.TaskDialogCommonButtonFlags.NoButton,
            ButtonType.Cancel => NativeMethods.TaskDialogCommonButtonFlags.CancelButton,
            ButtonType.Retry => NativeMethods.TaskDialogCommonButtonFlags.RetryButton,
            ButtonType.Close => NativeMethods.TaskDialogCommonButtonFlags.CloseButton,
            _ => 0
        };
    }
}
