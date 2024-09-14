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

// @MT_TASK: Make this resizable somehow? Maybe even make it a window?
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
        MainProgress,
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

            MainProgressLabel.ForeColor = fore;
            MainProgressLabel.BackColor = back;

            ItemsDGV.DarkModeEnabled = _darkModeEnabled;
        }
    }

    public MultiItemProgressBox(MainForm owner)
    {
        _owner = owner;

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        _defaultWidth = Width;

        // ReSharper disable once RedundantExplicitArraySize
        MessageItems = new MessageItem[2]
        {
            new(Message1Label),
            new(MainProgressLabel),
        };

        this.CenterHV(_owner, clientSize: true);
    }

    private void SetCancelButtonText(string text)
    {
        Cancel_Button.Text = text;
        Cancel_Button.CenterH(this);
    }

    private void SetLabelText(MessageItemType item, string text)
    {
        MessageItem messageItem = MessageItems[(int)item];
        if (text == messageItem.Text) return;
        messageItem.Text = text;
        messageItem.Width = -1;
    }

    private void SetProgressBarType(DarkProgressBar progressBar, ProgressType progressType, MessageItemType messageItemType)
    {
        if (progressType == ProgressType.Indeterminate)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            SetLabelText(messageItemType, "");
            _owner.SetTaskBarState(TaskbarStates.Indeterminate);
        }
        else
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            _owner.SetTaskBarState(TaskbarStates.Normal);
        }
    }

    private void SetPercent(int percent, MessageItemType messageItemType, DarkProgressBar progressBar)
    {
        percent = percent.Clamp(0, 100);

        SetLabelText(messageItemType, NonLocalizableText.PercentStrings[percent]);

        progressBar.Value = percent;

        _owner.SetTaskBarValue(0, 100);
    }

    // @MT_TASK: Finish implementing this, add a main progress bar probably etc.
    private int GetRequiredWidth()
    {
        MessageItem messageItemMain1 = MessageItems[(int)MessageItemType.MainMessage1];
        MessageItem messageItemProgress = MessageItems[(int)MessageItemType.MainProgress];
        //MessageItem messageItemMain2 = MessageItems[(int)MessageItemType.MainMessage2];
        //MessageItem messageItemSub = MessageItems[(int)MessageItemType.SubMessage];

        if (messageItemMain1.Width == -1)
        {
            messageItemMain1.Width = TextRenderer.MeasureText(messageItemMain1.Text, messageItemMain1.Label.Font).Width;
        }

        if (messageItemProgress.Width == -1)
        {
            messageItemProgress.Width = TextRenderer.MeasureText(messageItemProgress.Text, messageItemProgress.Label.Font).Width;
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
        int requiredWidth = Math.Max(messageItemMain1.Width, messageItemProgress.Width);
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
            // Hacks to fix various visual glitches
            this.CenterH(_owner, clientSize: true);
            Invalidate();
        }
    }

    /// <summary>
    /// Sets the state of the progress box. A null parameter means no change.
    /// </summary>
    /// <param name="initialRowTexts"></param>
    /// <param name="visible"></param>
    /// <param name="mainMessage1"></param>
    /// <param name="mainProgressMessage"></param>
    /// <param name="cancelButtonMessage"></param>
    /// <param name="cancelAction">Pass <see cref="T:NullAction"/> to hide the cancel button.</param>
    internal void SetState(
        (string Line1, string Line2)[]? initialRowTexts,
        bool? visible,
        string? mainMessage1,
        string? mainProgressMessage,
        string? cancelButtonMessage,
        Action? cancelAction)
    {
        if (mainMessage1 != null)
        {
            SetLabelText(MessageItemType.MainMessage1, mainMessage1);
            AutoSizeWidth();
        }
        if (mainProgressMessage != null)
        {
            SetLabelText(MessageItemType.MainProgress, mainProgressMessage);
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

        switch (visible)
        {
            // Put this last so the localization and whatever else can be right
            case true:
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
                break;
            }
            case false:
                Hide();
                break;
        }

        if (Visible && initialRowTexts != null)
        {
            // This must come after show, or else the scroll bars are broken on second show.
            int rowCount = initialRowTexts.Length;
            ItemsDGV.Rows.Clear();
            ItemsDGV.RowCount = rowCount;
            ItemsDGV.IndeterminateProgressBarsRefCount = 0;
            ItemsDGV.ProgressItems.ClearAndEnsureCapacity(rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                ItemsDGV.ProgressItems.Add(new DGV_ProgressItem.ProgressItemData(
                    line1: initialRowTexts[i].Line1,
                    line2: initialRowTexts[i].Line2,
                    percent: 0,
                    ProgressType.Determinate));
            }
        }
    }

    internal void SetItemData(
        int index,
        string? line1,
        string? line2,
        int? percent,
        ProgressType? progressType)
    {
        DGV_ProgressItem.ProgressItemData item = ItemsDGV.ProgressItems[index];

        bool refreshRequired = false;

        if (line1 != null)
        {
            if (item.Line1 != line1)
            {
                refreshRequired = true;
                item.Line1 = line1;
            }
        }

        if (line2 != null)
        {
            if (item.Line2 != line2)
            {
                refreshRequired = true;
                item.Line2 = line2;
            }
        }

        if (percent is { } percentInt)
        {
            if (item.Percent != percentInt)
            {
                refreshRequired = true;
                item.Percent = percentInt;
            }
        }

        if (progressType is { } progressTypeReal)
        {
            if (item.ProgressType != progressTypeReal)
            {
                refreshRequired = true;
                item.ProgressType = progressTypeReal;
                if (progressTypeReal == ProgressType.Indeterminate)
                {
                    ItemsDGV.IndeterminateProgressBarsRefCount++;
                }
                else
                {
                    ItemsDGV.IndeterminateProgressBarsRefCount--;
                }
            }
        }

        if (refreshRequired)
        {
            ItemsDGV.InvalidateRow(index);
        }
    }

    internal new void Hide()
    {
        ItemsDGV.IndeterminateProgressBarsRefCount = 0;

        _owner.SetTaskBarState(TaskbarStates.NoProgress);

        ItemsDGV.RowCount = 0;

        base.Hide();

        SetLabelText(MessageItemType.MainMessage1, "");
        SetLabelText(MessageItemType.MainProgress, "");

        ItemsDGV.ProgressItems.Clear();

        SetCancelButtonText("");
        Cancel_Button.Hide();
        _cancelAction = NullAction;

        Enabled = false;
        _owner.UIEnabled = true;
    }

    private void Cancel_Button_Click(object sender, EventArgs e)
    {
        if (_cancelAction != NullAction) _cancelAction.Invoke();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_darkModeEnabled && BorderStyle == BorderStyle.FixedSingle)
        {
            e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        }
    }
}
