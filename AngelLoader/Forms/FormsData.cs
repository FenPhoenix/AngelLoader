using System;

namespace AngelLoader.Forms;

internal static class FormsData
{
    internal static readonly int ZoomTypesCount = Enum.GetValues(typeof(Zoom)).Length;
}

/// <summary>
/// Set a control's tag to this to tell the darkable control dictionary filler to ignore it.
/// </summary>
public enum LoadType { Lazy }

public enum Direction { Left, Right, Up, Down }

// IMPORTANT: Don't change the order, they're used as indices!
public enum Zoom { In, Out, Reset }
