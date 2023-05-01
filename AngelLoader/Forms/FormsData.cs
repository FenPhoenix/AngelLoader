namespace AngelLoader.Forms;

internal static class FormsData
{
    internal const int ZoomTypesCount = 3;
}

/// <summary>
/// Set a control's tag to this to tell the darkable control dictionary filler to ignore it.
/// </summary>
public enum LoadType { Lazy }

public enum Direction { Left, Right, Up, Down }

// IMPORTANT: Don't change the order, they're used as indices!
public enum Zoom { In, Out, Reset }
