using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;
internal sealed class Lazy_UpdateNotification : IDarkable
{
    private readonly MainForm _owner;

    private bool _constructed;

    // @Update: We could make this a PictureBox or whatever later on
    private DarkLinkLabel Label;

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

        Label = new DarkLinkLabel
        {
            Tag = LoadType.Lazy,

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
        // @Update: This link should not be visible if we have no update infos
        if (Config.UpdateInfosTempCache.Count > 0)
        {
            await CheckUpdates.ShowUpdateAskDialog(Config.UpdateInfosTempCache);
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
