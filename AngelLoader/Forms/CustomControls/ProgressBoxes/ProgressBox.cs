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
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

// IMPORTANT (ProgressBox layout / size of controls / centering):
// Designer layout is NOT accurate on the right edge! Progress bar dimensions look like 9px left/11px right,
// but in-app it's 9px left/9px right like we want. Ugh.
public sealed partial class ProgressBox : UserControl, IDarkable
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
        MainMessage2,
        MainPercent,
        SubMessage,
        SubPercent,
    }

    private readonly MessageItem[] MessageItems;

    #region Consts

    private const ProgressSizeMode _defaultSizeMode = ProgressSizeMode.Single;
    internal static string DefaultCancelMessage => LText.Global.Cancel;
    internal const ProgressType DefaultProgressType = ProgressType.Determinate;

    private const int _regularHeight = 128;
    private const int _extendedHeight = 192;

    private readonly int _defaultWidth;

    #endregion

    #region Fields

    private readonly MainForm _owner;

    private ProgressSizeMode _sizeModeMode = _defaultSizeMode;

    private Action _cancelAction = NullAction;

    #endregion

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

            MainMessage1Label.ForeColor = fore;
            MainMessage1Label.BackColor = back;

            MainMessage2Label.ForeColor = fore;
            MainMessage2Label.BackColor = back;

            MainPercentLabel.ForeColor = fore;
            MainPercentLabel.BackColor = back;

            MainProgressBar.DarkModeEnabled = _darkModeEnabled;

            SubMessageLabel.ForeColor = fore;
            SubMessageLabel.BackColor = back;

            SubPercentLabel.ForeColor = fore;
            SubPercentLabel.BackColor = back;

            SubProgressBar.DarkModeEnabled = _darkModeEnabled;
        }
    }

    #region Init

    public ProgressBox(MainForm owner)
    {
        _owner = owner;

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        _defaultWidth = Width;

        // ReSharper disable once RedundantExplicitArraySize
        MessageItems = new MessageItem[5]
        {
            new(MainMessage1Label),
            new(MainMessage2Label),
            new(MainPercentLabel),
            new(SubMessageLabel),
            new(SubPercentLabel),
        };
    }

    internal void SetSizeToDefault() => SetSizeMode(_defaultSizeMode, forceChange: true);

    #endregion

    #region Private methods

    private void SetSizeMode(ProgressSizeMode sizeMode, bool forceChange = false)
    {
        if (!forceChange && sizeMode == _sizeModeMode) return;

        bool doubleSize = sizeMode == ProgressSizeMode.Double;

        Size = Size with { Height = doubleSize ? _extendedHeight : _regularHeight };
        SubMessageLabel.Visible = doubleSize;
        SubPercentLabel.Visible = doubleSize;
        SubProgressBar.Visible = doubleSize;

        this.CenterHV(_owner, clientSize: true);

        _sizeModeMode = sizeMode;

        // Necessary otherwise our drawn border doesn't update its size
        Invalidate();
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

    private void SetProgressBarType(DarkProgressBar progressBar, ProgressType progressType, MessageItemType messageItemType, bool updateTaskbar)
    {
        if (progressType == ProgressType.Indeterminate)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            SetLabelText(messageItemType, "");
            if (updateTaskbar)
            {
                _owner.SetTaskBarState(TaskbarStates.Indeterminate);
            }
        }
        else
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            if (updateTaskbar)
            {
                _owner.SetTaskBarState(TaskbarStates.Normal);
            }
        }
    }

    private void SetPercent(int percent, MessageItemType messageItemType, DarkProgressBar progressBar, bool updateTaskbar)
    {
        percent = percent.Clamp(0, 100);

        SetLabelText(messageItemType, NonLocalizableText.PercentStrings[percent]);

        progressBar.Value = percent;

        if (updateTaskbar)
        {
            _owner.SetTaskBarValue(percent, 100);
        }
    }

    #endregion

    #region Internal methods

    internal new void Hide()
    {
        if (!Visible) return;

        _owner.SetTaskBarState(TaskbarStates.NoProgress);

        base.Hide();

        SetLabelText(MessageItemType.MainMessage1, "");
        SetLabelText(MessageItemType.MainMessage2, "");
        SetLabelText(MessageItemType.MainPercent, "");
        MainProgressBar.Value = 0;
        MainProgressBar.Style = ProgressBarStyle.Blocks;

        SetLabelText(MessageItemType.SubMessage, "");
        SetLabelText(MessageItemType.SubPercent, "");
        SubProgressBar.Value = 0;
        SubProgressBar.Style = ProgressBarStyle.Blocks;

        // Necessary so when we show again we can see that text is blank and put the default, otherwise we
        // could have a scenario where we set non-default, hide, then show again without specifying the text,
        // and then it checks for empty and finds false, so it doesn't set the default and keeps whatever was
        // before.
        SetCancelButtonText("");
        Cancel_Button.Hide();
        _cancelAction = NullAction;

        SetSizeMode(_defaultSizeMode);

        Enabled = false;
        _owner.UIEnabled = true;
    }

    private int GetRequiredWidth()
    {
        MessageItem messageItemMain1 = MessageItems[(int)MessageItemType.MainMessage1];
        MessageItem messageItemMain2 = MessageItems[(int)MessageItemType.MainMessage2];
        MessageItem messageItemSub = MessageItems[(int)MessageItemType.SubMessage];

        if (messageItemMain1.Width == -1)
        {
            messageItemMain1.Width = TextRenderer.MeasureText(messageItemMain1.Text, messageItemMain1.Label.Font).Width;
        }

        if (messageItemMain2.Width == -1)
        {
            messageItemMain2.Width = TextRenderer.MeasureText(messageItemMain2.Text, messageItemMain2.Label.Font).Width;
        }

        if (messageItemSub.Width == -1)
        {
            messageItemSub.Width = TextRenderer.MeasureText(messageItemSub.Text, messageItemSub.Label.Font).Width;
        }

        int requiredWidth = MathMax3(messageItemMain1.Width, messageItemMain2.Width, messageItemSub.Width);
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
                MainProgressBar.RefreshDarkModeState(recreateHandleFirstIfDarkMode: true);
                SubProgressBar.RefreshDarkModeState(recreateHandleFirstIfDarkMode: true);
            }
        }
    }

    /// <summary>
    /// Sets the state of the progress box. A null parameter means no change.
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="size"></param>
    /// <param name="mainMessage1"></param>
    /// <param name="mainMessage2"></param>
    /// <param name="mainPercent"></param>
    /// <param name="mainProgressBarType"></param>
    /// <param name="subMessage"></param>
    /// <param name="subPercent"></param>
    /// <param name="subProgressBarType"></param>
    /// <param name="cancelButtonMessage"></param>
    /// <param name="cancelAction">Pass <see cref="T:NullAction"/> to hide the cancel button.</param>
    internal void SetState(
        bool? visible,
        ProgressSizeMode? size,
        string? mainMessage1,
        string? mainMessage2,
        int? mainPercent,
        ProgressType? mainProgressBarType,
        string? subMessage,
        int? subPercent,
        ProgressType? subProgressBarType,
        string? cancelButtonMessage,
        Action? cancelAction)
    {
        if (size != null)
        {
            SetSizeMode((ProgressSizeMode)size);
        }
        if (mainMessage1 != null)
        {
            SetLabelText(MessageItemType.MainMessage1, mainMessage1);
            AutoSizeWidth();
        }
        if (mainMessage2 != null)
        {
            SetLabelText(MessageItemType.MainMessage2, mainMessage2);
            AutoSizeWidth();
        }
        if (mainPercent != null)
        {
            SetPercent((int)mainPercent, MessageItemType.MainPercent, MainProgressBar, updateTaskbar: true);
        }
        if (mainProgressBarType != null)
        {
            SetProgressBarType(MainProgressBar, (ProgressType)mainProgressBarType, MessageItemType.MainPercent, updateTaskbar: true);
        }
        if (subMessage != null)
        {
            SetLabelText(MessageItemType.SubMessage, subMessage);
            AutoSizeWidth();
        }
        if (subPercent != null)
        {
            SetPercent((int)subPercent, MessageItemType.SubPercent, SubProgressBar, updateTaskbar: false);
        }
        if (subProgressBarType != null)
        {
            SetProgressBarType(SubProgressBar, (ProgressType)subProgressBarType, MessageItemType.SubPercent, updateTaskbar: false);
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
            }
            else
            {
                Hide();
            }
        }
    }

    #endregion

    private void ProgressCancelButton_Click(object sender, EventArgs e)
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
