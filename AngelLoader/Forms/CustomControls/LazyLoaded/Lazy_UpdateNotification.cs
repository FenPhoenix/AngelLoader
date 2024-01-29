using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;
internal sealed class Lazy_UpdateNotification : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    // @Update: We could make this a PictureBox or whatever later on
    private DarkButton Button = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            Button.DarkModeEnabled = value;
        }
    }

    internal Lazy_UpdateNotification(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        var container = _owner.BottomRightFLP;

        // @Update: Should we make this label/icon more visible/obnoxious colored etc. since it's meant to be a notification?
        Button = new DarkButton
        {
            Tag = LoadType.Lazy,

            AutoSize = false,
            Size = new Size(25, 25),
            Margin = new Padding(0, 9, 0, 0),
            TabIndex = 0,

            DarkModeEnabled = _darkModeEnabled
        };
        Button.PaintCustom += Button_Paint;
        Button.Click += Button_Click;

        // @Update: Should we maybe put it at the top, so it's more visible? It's not so easy to see down at the bottom.
        container.Controls.Add(Button);

        _constructed = true;

        Localize();
    }

    private void Button_Paint(object sender, PaintEventArgs e)
    {
        Images.PaintBitmapButton(Button, e, Images.UpdateIcon, x: 2, y: 2);
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
        // @Update: Localize this
        _owner.MainToolTip.SetToolTip(Button, "Update available");
    }

    internal void SetVisible(bool visible)
    {
        if (visible)
        {
            Construct();
            Button.Show();
        }
        else
        {
            if (_constructed) Button.Hide();
        }
    }
}
