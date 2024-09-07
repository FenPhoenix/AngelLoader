using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative.Taskbar;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;
public sealed partial class MultiItemProgressBox : UserControl, IDarkable
{
    // Cache text and width to minimize expensive calls to Control.Text property (getter) and text measurer.
    // Controls have a cache-text option but it's weird and causes weird issues sometimes that are not always
    // immediately obvious, so let's just do it ourselves and not bother with the whole crap.
    private sealed class MessageItem
    {
        internal readonly DarkLabel Label;
        private string _text;
        internal string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _text = value;
                Label.Text = value;
            }
        }
        internal int Width;

        internal MessageItem(DarkLabel label)
        {
            Label = label;
            Label.Text = "";
            _text = "";
            Width = -1;
        }
    }

    private enum MessageItemType
    {
        MainMessage1,
        //MainMessage2,
        //MainPercent,
        //SubMessage,
        //SubPercent,
    }

    private readonly MessageItem[] MessageItems;

    private readonly int _defaultWidth;

    internal static string DefaultCancelMessage => LText.Global.Cancel;

    private readonly MainForm _owner;

    private Action _cancelAction = NullAction;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            Cancel_Button.DarkModeEnabled = _darkModeEnabled;

            (Color fore, Color back) =
                _darkModeEnabled
                    // Use a lighter background to make it easy to see we're supposed to be in front and modal
                    ? (fore: DarkColors.LightText, back: DarkColors.LightBackground)
                    : (fore: SystemColors.ControlText, back: SystemColors.Control);

            ForeColor = fore;
            BackColor = back;

            Message1Label.ForeColor = fore;
            Message1Label.BackColor = back;

            ItemsDGV.DarkModeEnabled = _darkModeEnabled;
        }
    }

    public MultiItemProgressBox(MainForm owner)
    {
        _owner = owner;

        InitializeComponent();

        _defaultWidth = Width;

        // ReSharper disable once RedundantExplicitArraySize
        MessageItems = new MessageItem[1]
        {
            new(Message1Label),
        };

        this.CenterHV(_owner, clientSize: true);
    }

    private void SetCancelButtonText(string text)
    {
        Cancel_Button.Text = text;
        Cancel_Button.CenterH(this);
    }

    private void SetLabelText(string text)
    {
        Message1Label.Text = text;
    }

    // @MT_TASK: Finish implementing this, add a main progress bar probably etc.
    private int GetRequiredWidth()
    {
        MessageItem messageItemMain1 = MessageItems[(int)MessageItemType.MainMessage1];
        //MessageItem messageItemMain2 = MessageItems[(int)MessageItemType.MainMessage2];
        //MessageItem messageItemSub = MessageItems[(int)MessageItemType.SubMessage];

        if (messageItemMain1.Width == -1)
        {
            messageItemMain1.Width = TextRenderer.MeasureText(messageItemMain1.Text, messageItemMain1.Label.Font).Width;
        }

        //if (messageItemMain2.Width == -1)
        //{
        //    messageItemMain2.Width = TextRenderer.MeasureText(messageItemMain2.Text, messageItemMain2.Label.Font).Width;
        //}

        //if (messageItemSub.Width == -1)
        //{
        //    messageItemSub.Width = TextRenderer.MeasureText(messageItemSub.Text, messageItemSub.Label.Font).Width;
        //}

        //int requiredWidth = MathMax3(messageItemMain1.Width, messageItemMain2.Width, messageItemSub.Width);
        int requiredWidth = messageItemMain1.Width;
        return requiredWidth;
    }

    private void AutoSizeWidth()
    {
        bool widthChanged = false;

        // Perf so as not to change width if we don't have to
        if (Width > _defaultWidth)
        {
            Width = Math.Max(GetRequiredWidth(), _defaultWidth);
            widthChanged = true;
        }
        else
        {
            int requiredWidth = GetRequiredWidth();
            if (requiredWidth > _defaultWidth)
            {
                Width = requiredWidth;
                widthChanged = true;
            }
        }

        if (widthChanged)
        {
            // Hacks to fix various visual glitches, and the progress bar partially losing its dark mode state
            this.CenterH(_owner, clientSize: true);
            Invalidate();
            if (_darkModeEnabled)
            {
                //MainProgressBar.RefreshDarkModeState(recreateHandleFirstIfDarkMode: true);
                //SubProgressBar.RefreshDarkModeState(recreateHandleFirstIfDarkMode: true);
            }
        }
    }

    /// <summary>
    /// Sets the state of the progress box. A null parameter means no change.
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="visible"></param>
    /// <param name="mainMessage1"></param>
    /// <param name="cancelButtonMessage"></param>
    /// <param name="cancelAction">Pass <see cref="T:NullAction"/> to hide the cancel button.</param>
    internal void SetState(
        int? rows,
        bool? visible,
        string? mainMessage1,
        string? cancelButtonMessage,
        Action? cancelAction)
    {
        if (mainMessage1 != null)
        {
            SetLabelText(mainMessage1);
            AutoSizeWidth();
        }
        if (cancelButtonMessage != null)
        {
            SetCancelButtonText(cancelButtonMessage);
        }
        if (cancelAction != null)
        {
            _cancelAction = cancelAction;
            Cancel_Button.Visible = cancelAction != NullAction;
        }

        // Put this last so the localization and whatever else can be right
        if (visible != null)
        {
            if (visible == true)
            {
                _owner.UIEnabled = false;
                Enabled = true;

                if (Cancel_Button.Text.IsEmpty())
                {
                    SetCancelButtonText(DefaultCancelMessage);
                }

                BringToFront();
                Show();
                Cancel_Button.Focus();

                // This must come after show, or else the scroll bars are broken on second show.
                if (rows is { } rowsInt)
                {
                    ItemsDGV.Rows.Clear();
                    ItemsDGV.RowCount = rowsInt;
                    ItemsDGV.ProgressItems.ClearAndEnsureCapacity(rowsInt);
                    for (int i = 0; i < rowsInt; i++)
                    {
                        ItemsDGV.ProgressItems.Add(new DGV_ProgressItem.ProgressItemData("", "", 0));
                    }
                }
            }
            else
            {
                Hide();
            }
        }
    }

    internal void SetItemData(int index, string? line1, string? line2, int? percent)
    {
        DGV_ProgressItem.ProgressItemData item = ItemsDGV.ProgressItems[index];

        if (line1 != null) item.Line1 = line1;
        if (line2 != null) item.Line2 = line2;
        if (percent != null) item.Percent = (int)percent;

        ItemsDGV.InvalidateRow(index);
    }

    internal new void Hide()
    {
        if (_owner.IsHandleCreated) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.NoProgress);

        ItemsDGV.RowCount = 0;
        base.Hide();

        SetLabelText("");
        ItemsDGV.ProgressItems.Clear();

        SetCancelButtonText("");
        Cancel_Button.Hide();
        _cancelAction = NullAction;

        Enabled = false;
        _owner.UIEnabled = true;
    }
}
