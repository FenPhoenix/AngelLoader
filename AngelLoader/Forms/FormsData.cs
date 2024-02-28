using System;
using System.Drawing;
using System.IO;

namespace AngelLoader.Forms;

internal static class FormsData
{
    internal const int ZoomTypesCount = 3;
}

/// <summary>
/// Set a control's tag to this to tell the darkable control dictionary filler to ignore it.
/// </summary>
internal enum LoadType { Lazy }

internal enum MenuPos { LeftUp, LeftDown, TopLeft, TopRight, RightUp, RightDown, BottomLeft, BottomRight }

public enum Direction { Left, Right, Up, Down }

// IMPORTANT: Don't change the order, they're used as indices!
public enum Zoom { In, Out, Reset }

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
