﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms;

public sealed partial class SplashScreenForm : Form, ISplashScreen
{
    /*
    Here's the deal with this:
    We want this form to be fully and properly painted at all times. Normally, we would achieve this by simply
    running all init tasks in the background so that this form's thread is always free. However, part of our
    init time includes initializing the main form (which is heavy, and even more so if we're setting dark mode).
    Since all UI stuff has to run on the same thread, this form would end up in a blocked, and quite likely
    partially-painted, state. We can put this form into another thread via a separate ApplicationContext,
    and that works, except then we have insurmountable focus issues (the main form _usually_, but not always,
    takes focus as it should).
    The other option we have is to bypass the normal way to display a form entirely, and just paint everything
    directly onto its device context. That way, we paint it _once_ and never change it until we repaint the
    message text, and it never gets into an ugly half-painted state, even when its thread is blocked, as it
    will be during main form init. But it does mean we have some very unorthodox code in here. Don't worry
    about it.
    */

    #region Private fields

    private VisualTheme _theme;
    private bool _closingAllowed;

    private const int _messageRectY = 120;
    private readonly Rectangle _messageRect = new Rectangle(1, _messageRectY, 646, 63);

    // Paranoid to make absolutely sure we're not accessing any cross-thread-disallowed Control properties in
    // SetMessage() (thread safety for FM finder)
    private Color _foreColorCached;
    private Color _backColorCached;

    private string _message = "";
    private int _checkMessageWidth;

    #region Disposables

    private readonly Native.GraphicsContext_Ref _graphicsContext;

    // Separate copy in here, so we don't cause an instantiation cascade for all the fields in DarkColors.
    // Special case to be AS FAST as possible.
    private readonly SolidBrush _fen_ControlBackgroundBrush = new SolidBrush(Color.FromArgb(48, 48, 48));

    #endregion

    #endregion

    // Perf
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool VisibleCached { get; private set; }

    public SplashScreenForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        Text = ControlUtils.GetWindowTitleString();

        _graphicsContext = new Native.GraphicsContext_Ref(Handle);
    }

    public void Show(VisualTheme theme)
    {
        _theme = theme;

        // Ultra slim, because a splash screen should come up as quick as possible
        if (theme == VisualTheme.Dark)
        {
            // Again, manual copies of colors so we don't statically initialize the whole of DarkColors.
            BackColor = Color.FromArgb(48, 48, 48); // Fen_ControlBackground
            ForeColor = Color.FromArgb(220, 220, 220); // LightText
        }

        _foreColorCached = ForeColor;
        _backColorCached = BackColor;

        Program.SplashScreenPreloadState.SplashScreenPreloadTask.Wait();

        base.Show();
    }

    private void DrawMain()
    {
        _graphicsContext.G.DrawImage(Preload.AL_Icon_Bmp, 152, 48);

        _graphicsContext.G.DrawImage(
            _theme == VisualTheme.Dark
                ? Preload.AboutDark
                : Preload.About,
            200, 48);

        using Pen pen = new(
            _theme == VisualTheme.Dark
                ? Color.FromArgb(81, 81, 81) // LightBorder
                : SystemColors.ControlDark);

        _graphicsContext.G.DrawRectangle(pen, 0, 0, 647, 183);
    }

    private void DrawMessage()
    {
        if (_message.IsEmpty()) return;

        Brush bgColorBrush = _theme == VisualTheme.Dark
            ? _fen_ControlBackgroundBrush
            : SystemBrushes.Control;

        _graphicsContext.G.FillRectangle(bgColorBrush, _messageRect);

        const TextFormatFlags _messageTextFormatFlags =
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.Top |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping |
            TextFormatFlags.WordBreak;

        TextRenderer.DrawText(_graphicsContext.G, _message, Program.SplashScreenPreloadState.MessageFont, _messageRect, _foreColorCached, _backColorCached, _messageTextFormatFlags);
    }

    public void SetMessage(string message)
    {
        _message = message;
        DrawMessage();
    }

    public void SetCheckMessageWidth(string message)
    {
        const TextFormatFlags _messageTextFormatFlags =
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.Top |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping;

        _checkMessageWidth = TextRenderer.MeasureText(message, Program.SplashScreenPreloadState.MessageFont, Size.Empty, _messageTextFormatFlags).Width;
    }

    public void SetCheckAtStoredMessageWidth()
    {
        if (_checkMessageWidth > Width) return;
        int checkPos = (Width / 2) + (_checkMessageWidth / 2);

        using Pen checkMarkPen = new(_foreColorCached, 1.6f);
        Rectangle outlineBoxRect = new(checkPos, _messageRectY + 4, 12, 12);
        ControlUtils.DrawCheckMark(_graphicsContext.G, checkMarkPen, outlineBoxRect);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Native.WM_WINDOWPOSCHANGED && VisibleCached)
        {
            try
            {
                // This counts as our initial call after Show(), and also further ones to fix the situation where
                // the update copier starts us and then we lose our graphics for some inane reason.
                DrawMain();
            }
            catch
            {
                // Something went wrong, whatever, let's at least not crash
            }
        }

        base.WndProc(ref m);
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        VisibleCached = Visible;
        base.OnVisibleChanged(e);
    }

    public void ProgrammaticClose()
    {
        _closingAllowed = true;
        Close();
    }

    // Don't let the user close the splash screen; that would put us in an unexpected/undefined state.
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_closingAllowed)
        {
            e.Cancel = true;
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            Program.SplashScreenPreloadState.SplashScreenPreloadTask.Wait();

            Program.SplashScreenPreloadState.MessageFont?.Dispose();
            Program.SplashScreenPreloadState.FontCollection?.Dispose();
            _graphicsContext.Dispose();
            _fen_ControlBackgroundBrush.Dispose();

            Program.SplashScreenPreloadState.SplashScreenPreloadTask.Dispose();
        }
        base.Dispose(disposing);
    }
}
