using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms;

internal static class FormsData
{
    internal const int ZoomTypesCount = 3;
    internal const int WhichTabCount = 2;
}

/// <summary>
/// Set a control's tag to this to tell the darkable control dictionary filler to ignore it.
/// </summary>
internal enum LoadType { Lazy }

internal enum MenuPos { LeftUp, LeftDown, TopLeft, TopRight, RightUp, RightDown, BottomLeft, BottomRight }

public enum Direction { Left, Right, Up, Down }

// IMPORTANT: Don't change the order, they're used as indices!
public enum Zoom { In, Out, Reset }

public enum WhichTabControl
{
    Top,
    Bottom,
}

internal sealed class FMTabControlGroup(
    IOptionallyLazyTabControl tabControl,
    DarkArrowButton collapseButton,
    Lazy_FMTabsBlocker blocker,
    DarkSplitContainerCustom splitter,
    DarkLabel emptyMessageLabel)
{
    internal readonly IOptionallyLazyTabControl TabControl = tabControl;
    internal readonly DarkArrowButton CollapseButton = collapseButton;
    internal readonly Lazy_FMTabsBlocker Blocker = blocker;
    internal readonly DarkSplitContainerCustom Splitter = splitter;
    internal readonly DarkLabel EmptyMessageLabel = emptyMessageLabel;
}

/*
Images loaded from files keep the file stream alive for their entire lifetime, insanely. This means the file
is "in use" and will cause delete attempts (like FM uninstallation) to fail. So we need to use this workaround
of loading the file into a memory stream first, so it's only the memory stream being kept alive. This does
mean we carry around the full file bytes in memory as well as the displayed image, but since we're only
displaying one at a time and they'll probably be a few megs at most, it's not a big deal.
*/
public sealed class MemoryImage : IDisposable
{
    private readonly MemoryStream _memoryStream;
    public readonly Image Img;
    public string Path { get; private set; }

    public MemoryImage(string path)
    {
        Path = path;
        byte[] bytes = File.ReadAllBytes(path);
        _memoryStream = new MemoryStream(bytes);
        Img = Image.FromStream(_memoryStream);
    }

    public void Dispose()
    {
        Path = "";
        Img.Dispose();
        _memoryStream.Dispose();
    }
}

public sealed class BackingTab(TabPage tabPage)
{
    public TabPage TabPage = tabPage;
    public FMTabVisibleIn VisibleIn = FMTabVisibleIn.Top;
}

public sealed class TabControlImageCursor : IDisposable
{
    /*
    On fail, we're going to set Cursor to Cursors.Default. But we need to make sure we don't dispose it in that
    case, or it will dispose the static default cursor object and make it invisible.
    We could say "Cursor = new Cursor(Cursors.Default.CopyHandle())", but that's another point of failure, so
    let's just set a bool and only dispose the cursor if it's custom and not one of the static built-in ones.
    */
    private readonly bool _cursorIsCustom;
    private readonly Bitmap? _bitmap;
    public readonly Cursor Cursor;

    // Draw the themed DateTimePickers manually onto the image, because their themes don't get fully captured.
    private static void DrawDateTimePickers(
        Control control,
        Graphics g,
        IOptionallyLazyTabControl tabControl,
        int stackCounter = 0)
    {
        stackCounter++;
        if (stackCounter > 100) return;

        if (control is DarkDateTimePicker dtp)
        {
            Point offset = tabControl.PointToClient_Fast(dtp.Parent.PointToScreen_Fast(dtp.Location));
            dtp.PaintCustom(g, offset);
        }

        Control.ControlCollection controls = control.Controls;
        int count = controls.Count;
        for (int i = 0; i < count; i++)
        {
            DrawDateTimePickers(controls[i], g, tabControl, stackCounter);
        }
    }

    public TabControlImageCursor(IOptionallyLazyTabControl tabControl)
    {
        Bitmap? bmpChopped = null;
        try
        {
            using Bitmap bmpPre = new(tabControl.Width, tabControl.Height);
            tabControl.DrawToBitmap(bmpPre, new Rectangle(0, 0, tabControl.Width, tabControl.Height));

            Rectangle tabRect = tabControl.SelectedIndex > -1
                ? tabControl.GetTabRect(tabControl.SelectedIndex)
                : Rectangle.Empty;

            if (tabRect != Rectangle.Empty)
            {
                // Remove all other tabs from the image and show only the selected tab at the left side, for more
                // visual clarity and a clean look
                int tabRectHeight = tabRect.Height + (Global.Config.DarkMode ? 2 : 3);
                int tabRectWidth = tabRect.Width + (Global.Config.DarkMode ? 1 : 4);
                int tabRectLeft = (tabRect.Left - (Global.Config.DarkMode ? 0 : 2)).ClampToZero();

                bmpChopped = new Bitmap(bmpPre.Width, bmpPre.Height, PixelFormat.Format32bppPArgb);
                using Graphics g = Graphics.FromImage(bmpChopped);

                // Main body
                g.DrawImage(
                    image: bmpPre,
                    destRect: new Rectangle(0, tabRectHeight, bmpPre.Width, bmpPre.Height - tabRectHeight),
                    srcX: 0,
                    srcY: tabRectHeight,
                    srcWidth: bmpPre.Width,
                    srcHeight: bmpPre.Height - tabRectHeight,
                    srcUnit: GraphicsUnit.Pixel
                );

                // Top bar
                g.DrawImage(
                    image: bmpPre,
                    destRect: new Rectangle(0, 0, tabRectWidth, tabRectHeight),
                    srcX: tabRectLeft,
                    srcY: 0,
                    srcWidth: tabRectWidth,
                    srcHeight: tabRectHeight,
                    srcUnit: GraphicsUnit.Pixel
                );

                if (Global.Config.DarkMode && tabControl.SelectedTab != null)
                {
                    DrawDateTimePickers(
                        tabControl.SelectedTab,
                        g,
                        tabControl
                    );
                }
            }

            Bitmap? bmpFinal = (bmpChopped ?? bmpPre).CloneWithOpacity(0.88f);
            if (bmpFinal != null &&
                ControlUtils.TryCreateCursor(bmpFinal, 0, 0, out Cursor? cursor))
            {
                _bitmap = bmpFinal;
                Cursor = cursor;
                _cursorIsCustom = true;
            }
            else
            {
                _cursorIsCustom = false;
                bmpFinal?.Dispose();
                _bitmap = null;
                Cursor = Cursors.Default;
            }
        }
        catch
        {
            _cursorIsCustom = false;
            Cursor = Cursors.Default;
            _bitmap = null;
        }
        finally
        {
            bmpChopped?.Dispose();
        }
    }

    public void Dispose()
    {
        if (_cursorIsCustom) Cursor.Dispose();
        _bitmap?.Dispose();
    }
}
