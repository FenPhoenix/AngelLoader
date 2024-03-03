using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

// @ViewBusinessLogic: Lots of it in the lazy-loaded FM tabs now.
public class Lazy_TabsBase : DarkTabPageCustom
{
    internal enum ScanSender
    {
        // Statistics
        CustomResources,

        // Edit
        Readmes,
        Title,
        Author,
        ReleaseDate
    }

    private protected MainForm _owner = null!;

    private protected bool _constructed;

    private readonly List<KeyValuePair<Control, ControlUtils.ControlOriginalColors?>> _controlColors = new();

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool DarkModeEnabled
    {
        get => base.DarkModeEnabled;
        set
        {
            if (base.DarkModeEnabled == value) return;
            base.DarkModeEnabled = value;

            if (!_constructed) return;

            RefreshTheme();
        }
    }

    public void SetOwner(MainForm owner) => _owner = owner;

    public void ConstructWithSuspendResume()
    {
        if (_constructed) return;

        try
        {
            this.SuspendDrawing();
            Construct();
        }
        finally
        {
            this.ResumeDrawing();
        }
    }

    public virtual void Construct() { }

    public virtual void Localize() { }

    public virtual void UpdatePage() { }

    private protected void FinishConstruct()
    {
        _constructed = true;
        UpdatePage();
        if (DarkModeEnabled) RefreshTheme();
        Localize();
    }

    private void RefreshTheme()
    {
        ControlUtils.SetTheme(this, _controlColors, base.DarkModeEnabled ? VisualTheme.Dark : VisualTheme.Classic);
    }

    private protected static T ConstructPage<T>() where T : Control, new() => new()
    {
        Dock = DockStyle.Fill,
        Tag = LoadType.Lazy,
        Visible = false
    };
}
