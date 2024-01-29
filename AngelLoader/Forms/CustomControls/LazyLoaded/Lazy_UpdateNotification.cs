using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;
internal sealed class Lazy_UpdateNotification : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    internal ToolStripButtonCustom Button = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            Button.Image = Images.UpdateIcon;
        }
    }

    internal Lazy_UpdateNotification(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        // @Update: Should we make this label/icon more visible/obnoxious colored etc. since it's meant to be a notification?
        Button = new ToolStripButtonCustom
        {
            Tag = LoadType.Lazy,

            AutoSize = false,
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            Image = Images.UpdateIcon,
            Margin = new Padding(0)
        };
        Button.Click += Button_Click;

        _owner.RefreshAreaToolStrip.Items.Insert(0, Button);

        _constructed = true;

        Localize();
    }

    private async void Button_Click(object sender, EventArgs e)
    {
        bool success;
        List<CheckUpdates.UpdateInfo> updateInfos;
        try
        {
            _owner.SetWaitCursor(true);
            (success, updateInfos) = await CheckUpdates.GetUpdateDetails();
        }
        finally
        {
            _owner.SetWaitCursor(false);
        }

        if (success && updateInfos.Count > 0)
        {
            await CheckUpdates.ShowUpdateAskDialog(updateInfos);
        }
        else
        {
            // @Update: If we couldn't access the internet, we need to say something different than if it's some other error
            Core.Dialogs.ShowAlert("Update error description goes here", "Update");
        }
    }

    internal void Localize()
    {
        if (!_constructed) return;
        Button.Text = LText.Update.UpdateAvailable;
        RefreshSize();
    }

    // Plain AutoSize makes the height go off the bottom, so do it manually. There's no MinimumSize for ToolStrip
    // stuff either.
    private void RefreshSize()
    {
        Button.AutoSize = true;
        Button.AutoSize = false;
        Button.Size = Button.Size with { Height = 25 };
        _owner.SetFilterBarWidth();
    }

    internal void SetVisible(bool visible)
    {
        if (visible)
        {
            Construct();
            Button.Visible = true;
            _owner.SetFilterBarWidth();
        }
        else
        {
            if (!_constructed) return;

            Button.Visible = false;
            _owner.SetFilterBarWidth();
        }
    }
}
