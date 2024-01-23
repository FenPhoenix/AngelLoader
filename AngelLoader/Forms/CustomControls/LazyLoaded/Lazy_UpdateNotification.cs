using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;
internal sealed class Lazy_UpdateNotification : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    // @Update: We could make this a PictureBox or whatever later on
    private DarkLinkLabel Label = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            Label.DarkModeEnabled = value;
        }
    }

    internal Lazy_UpdateNotification(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        var container = _owner.BottomRightFLP;

        // @Update: Should we make this label/icon more visible/obnoxious colored etc. since it's meant to be a notification?
        Label = new DarkLinkLabel
        {
            Tag = LoadType.Lazy,

            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0),
            TabIndex = 0,

            DarkModeEnabled = _darkModeEnabled
        };
        Label.LinkClicked += Label_LinkClicked;

        container.Controls.Add(Label);

        _constructed = true;

        Localize();
    }

    private async void Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        bool success;
        List<CheckUpdates.UpdateInfo> updateInfos;
        try
        {
            _owner.SetWaitCursor(true);
            (success, updateInfos) = await CheckUpdates.Check2024();
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
        Label.Text = "Update";
    }

    internal void SetVisible(bool visible)
    {
        if (visible)
        {
            Construct();
            Label.Show();
        }
        else
        {
            if (_constructed) Label.Hide();
        }
    }
}
