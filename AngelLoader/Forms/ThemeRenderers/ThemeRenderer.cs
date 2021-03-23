using System;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.ThemeRenderers
{
    [PublicAPI]
    internal abstract class ThemeRenderer : IDisposable
    {
        internal IntPtr HTheme { get; private set; }
        private protected virtual string CLSID => throw new NullReferenceException("No CLSID!");

        internal ThemeRenderer() => Reload();

        internal virtual bool Enabled => true;

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
            in Native.RECT pRect,
            in Native.RECT pClipRect)
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

        protected virtual void Dispose(bool disposing)
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
}
