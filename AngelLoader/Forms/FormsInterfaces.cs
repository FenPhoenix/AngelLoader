﻿using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AngelLoader.Forms;

public interface IUpdateRegion
{
    void BeginUpdate();
    void EndUpdate();
}

[StructLayout(LayoutKind.Auto)]
internal readonly ref struct UpdateRegion
{
    private readonly IUpdateRegion _obj;

    public UpdateRegion(IUpdateRegion obj)
    {
        _obj = obj;
        _obj.BeginUpdate();
    }

    public void Dispose()
    {
        _obj.EndUpdate();
    }
}

#region DisableEvents

/*
 Implement the interface on your form, and put guard clauses on all your event handlers that you want to
 be disableable:

 if (EventsDisabled > 0) return;

 Then whenever you want to disable those event handlers, just make a using block:

 using (new DisableEvents(this))
 {
 }

 Inside this block, put any code that changes the state of the controls in such a way that would normally
 run their event handlers. The guard clauses will exit them before anything happens. Problem solved. And
 much better than a nasty wall of Control.Event1 -= Control_Event1; Control.Event1 += Control_Event1; etc.,
 and has the added bonus of guaranteeing a reset of the value due to the using block.
*/

public interface IEventDisabler
{
    /// <summary>
    /// True if greater than 0.
    /// </summary>
    int EventsDisabled { get; set; }
}

[StructLayout(LayoutKind.Auto)]
internal readonly ref struct DisableEvents
{
    /*
    For some reason VS 17.6+ no longer allows "using ref struct" statements in async methods, even if there is
    no actual async call IN the using block. So we can use these in a manual try-finally statement ourselves to
    still avoid the allocation of a class-based version. Yeesh...
    */
    public static void Open(IEventDisabler obj, bool active = true)
    {
        if (active) obj.EventsDisabled++;
    }

    public static void Close(IEventDisabler obj, bool active = true)
    {
        if (active) obj.EventsDisabled = (obj.EventsDisabled - 1).ClampToZero();
    }

    private readonly bool _active;
    private readonly IEventDisabler _obj;
    public DisableEvents(IEventDisabler obj, bool active = true)
    {
        _active = active;
        _obj = obj;

        if (_active) _obj.EventsDisabled++;
    }

    public void Dispose()
    {
        if (_active) _obj.EventsDisabled = (_obj.EventsDisabled - 1).ClampToZero();
    }
}

#endregion

#region DisableZeroSelectCode

public interface IZeroSelectCodeDisabler
{
    /// <summary>
    /// True if greater than 0.
    /// </summary>
    int ZeroSelectCodeDisabled { get; set; }
}

[StructLayout(LayoutKind.Auto)]
internal readonly ref struct DisableZeroSelectCode
{
    private readonly IZeroSelectCodeDisabler _obj;
    internal DisableZeroSelectCode(IZeroSelectCodeDisabler obj)
    {
        _obj = obj;
        _obj.ZeroSelectCodeDisabled++;
    }

    public void Dispose() => _obj.ZeroSelectCodeDisabled = (_obj.ZeroSelectCodeDisabled - 1).ClampToZero();
}

#endregion

public interface IDarkable
{
    bool DarkModeEnabled { set; }
}

public interface IDarkContextMenuOwner
{
    bool ViewBlocked { get; }
    IContainer GetComponents();
}

public interface IListControlWithBackingItems : IUpdateRegion
{
    void AddFullItem(string backingItem, string item);
    void ClearFullItems();

    #region Disabled until needed

#if false
    int BackingIndexOf(string item);
    string SelectedBackingItem();
    void SelectBackingIndexOf(string item);
#endif

    #endregion
}

public interface IOptionallyLazyTabControl
{
    public bool Enabled { get; set; }
    public TabPage? SelectedTab { get; set; }
    public TabPage? DragTab { get; }
    void ShowTab(TabPage tabPage, bool show);
    int TabCount { get; }
    bool TabPagesContains(TabPage tabPage);
    Rectangle GetTabRect(int index);
    Rectangle GetTabBarRect();
    Rectangle ClientRectangle { get; }
    Point ClientCursorPos();
    int Width { get; }
    int Height { get; }
    int SelectedIndex { get; }
    bool Focused { get; }
    void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds);
    Point PointToClient_Fast(Point point);
    void ResetTempDragData();
    void RestoreBackedUpBackingTabs();
    event TabControlEventHandler? Selected;
}
