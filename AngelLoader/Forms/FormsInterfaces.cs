﻿using System.ComponentModel;
using AL_Common;

namespace AngelLoader.Forms;

#region DisableEvents

/*
 Implement the interface on your form, and put guard clauses on all your event handlers that you want to
 be disableable:

 if (EventsDisabled) return;

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
    bool EventsDisabled { get; }
    int EventsDisabledCount { get; set; }
}

internal readonly ref struct DisableEvents
{
    private readonly bool _active;
    private readonly IEventDisabler Obj;
    public DisableEvents(IEventDisabler obj, bool active = true)
    {
        _active = active;
        Obj = obj;

        if (_active) Obj.EventsDisabledCount++;
    }

    public void Dispose()
    {
        if (_active) Obj.EventsDisabledCount = (Obj.EventsDisabledCount - 1).ClampToZero();
    }
}

#endregion

#region DisableZeroSelectCode

public interface IZeroSelectCodeDisabler
{
    bool ZeroSelectCodeDisabled { get; }
    int ZeroSelectCodeDisabledCount { get; set; }
}

internal readonly ref struct DisableZeroSelectCode
{
    private readonly IZeroSelectCodeDisabler Obj;
    internal DisableZeroSelectCode(IZeroSelectCodeDisabler obj)
    {
        Obj = obj;
        Obj.ZeroSelectCodeDisabledCount++;
    }

    public void Dispose() => Obj.ZeroSelectCodeDisabledCount = (Obj.ZeroSelectCodeDisabledCount - 1).ClampToZero();
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

public interface IListControlWithBackingItems
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

    void BeginUpdate();
    void EndUpdate();
}
