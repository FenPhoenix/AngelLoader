using System.ComponentModel;
using AL_Common;

namespace AngelLoader.Forms;

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

public interface IDarkContextMenuOwner
{
    bool ViewBlocked { get; }
    IContainer GetComponents();
}

public interface IDarkable
{
    bool DarkModeEnabled { set; }
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
