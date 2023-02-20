using System;
using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.ThemeRenderers;

[PublicAPI]
internal abstract class ThemeRenderer : IDisposable
{
    internal IntPtr HTheme { get; private set; }
    private protected abstract string CLSID { get; }
    internal abstract bool Enabled { get; }

    private protected ThemeRenderer() => Reload();

    internal void Reload()
    {
        Native.CloseThemeData(HTheme);
        using var c = new Control();
        HTheme = Native.OpenThemeData(c.Handle, CLSID);
    }

    internal virtual bool TryDrawThemeBackground(
        IntPtr hTheme,
        IntPtr hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        ref Native.RECT pClipRect)
    {
        return false;
    }

    internal virtual bool TryGetThemeColor(
        IntPtr hTheme,
        int iPartId,
        int iStateId,
        int iPropId,
        out int pColor)
    {
        pColor = 0;
        return false;
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Native.CloseThemeData(HTheme);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
