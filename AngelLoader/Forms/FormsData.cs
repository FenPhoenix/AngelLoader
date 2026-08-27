using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using AngelLoader.Forms.WinFormsNative;
using Pfim;

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
Images loaded with Image.FromFile() keep the file handle alive for their entire lifetime, insanely. This means
the file is "in use" and will cause delete attempts (like FM uninstallation) to fail. However, images loaded with
Image.FromStream() do NOT keep the file in use. This is completely non-obvious, because every other file API in
the known universe has the path-taking version just construct and pass a stream to the stream-taking version
internally. But this one, alone, calls two completely different Windows API functions internally, and the path-
taking one holds the file handle for the life of the Image.

Because I, quite reasonably, assumed that Image.FromStream() would have the same issue as Image.FromFile(), I
made this class to load the file into a MemoryStream and then pass that to Image.FromStream(), so that when it
"held the stream open" (which it turns out it doesn't), it would only hold the MemoryStream and not the file,
thereby avoiding the file being in use when we're trying to delete it.

But yeah, turns out we can just load the file with Image.FromStream() and everything's fine.

It's possible we might be able to get rid of this class entirely, but it also holds a Path string that gets
cleared on Dispose() but then gets compared to another string later, potentially after disposal, and the whole
thing is nasty and might break if we change it without going through the screenshots tab page code with a fine-
toothed comb. So let's just keep the class for now, but save memory by not keeping a MemoryStream around.

@TDM(TGA format): 2026-08-27: At the time of this writing (TDM 2.14 is the latest), TDM only writes TGA files in
uncompressed true-color format (value 2). Pfim handles RLE and uncompressed, but not "huffman-delta-run-length
encoded color-mapped" or "huffman-delta-run-length-4-pass-quadtree-type process encoded color-mapped", from the
looks of it. If we wanted to be obsessive we could remove the compressed TGA handling code, but we've got Pfim
down to like 18K already so meh.
*/
public sealed class MemoryImage : IDisposable
{
    private GCHandle _pfimHandle;
    public readonly Image Img;
    private readonly Targa? _tgaImg;
    public string Path { get; private set; }
    private readonly bool _isTga;

    public MemoryImage(string path)
    {
        Path = path;
        if (path.EndsWithI(".tga"))
        {
            _isTga = true;

            _tgaImg = Pfimage.FromFile(path);
            PixelFormat format = _tgaImg.Format switch
            {
                Pfim.ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
                Pfim.ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
                Pfim.ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
                Pfim.ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
                Pfim.ImageFormat.R5g5b5a1 => PixelFormat.Format16bppArgb1555,
                Pfim.ImageFormat.Rgb8 => PixelFormat.Format8bppIndexed,
                _ => throw new InvalidDataException("Couldn't load '" + path + "'; pixel format not supported."),
            };
            _pfimHandle = GCHandle.Alloc(_tgaImg.Data, GCHandleType.Pinned);
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(_tgaImg.Data, 0);

            Img = new Bitmap(_tgaImg.Width, _tgaImg.Height, _tgaImg.Stride, format, ptr);
        }
        else
        {
            _isTga = false;

            using FileStream_NET fileStream = File_OpenReadFast(path, FileStreamBufferSize);
            Img = Image.FromStream(fileStream);
        }
    }

    /// <summary>
    /// Disposes and assigns a new one.
    /// </summary>
    /// <param name="memoryImage"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static MemoryImage Recreate(MemoryImage? memoryImage, string path)
    {
        memoryImage?.Dispose();
        return new MemoryImage(path);
    }

    public void Dispose()
    {
        Path = "";
        Img.Dispose();
        if (_isTga)
        {
            try
            {
                if (_pfimHandle.IsAllocated)
                {
                    _pfimHandle.Free();
                }
            }
            catch
            {
                // It might still throw if some weird thread access to the handle happens I guess, so just in case
            }
            finally
            {
                _tgaImg?.Dispose();
            }
        }
    }
}

public sealed class BackingTab
{
    public TabPage TabPage;
    public FMTabVisibleIn VisibleIn;

    public BackingTab(TabPage tabPage)
    {
        TabPage = tabPage;
        VisibleIn = FMTabVisibleIn.Top;
    }

    public BackingTab(TabPage tabPage, FMTabVisibleIn visibleIn)
    {
        TabPage = tabPage;
        VisibleIn = visibleIn;
    }

    public void CopyTo(BackingTab dest)
    {
        dest.TabPage = TabPage;
        dest.VisibleIn = VisibleIn;
    }
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

        if (control is DarkDateTimePicker { Parent: { } parentControl } dtp)
        {
            Point offset = tabControl.PointToClient_Fast(parentControl.PointToScreen_Fast(dtp.Location));
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
