using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public partial class GameInfoForm : Form
    {
        private readonly (Label Label, TextBox TextBox)[] GameVersionItems;

        public GameInfoForm()
        {
            InitializeComponent();

            // @GENGAMES (GameInfoForm): Begin
            GameVersionItems = new[]
            {
                (T1VersionLabel, T1VersionTextBox),
                (T2VersionLabel, T2VersionTextBox),
                (T3VersionLabel, T3VersionTextBox),
                (SS2VersionLabel, SS2VersionTextBox)
            };
            // @GENGAMES (GameInfoForm): End
        }

        private void GameInfoForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;

                string ver = "";

                if (!Config.GetGameExe(gameIndex).IsEmpty())
                {
                    Error error = GameVersions.TryGetGameVersion((GameIndex)i, out ver);
                    if (error == Error.GameVersionNotFound)
                    {
                        ver = "not found";
                    }
                    else if (error != Error.None)
                    {
                        ver = "error getting version";
                    }
                }
                else
                {
                    ver = "Game not set";
                }

                GameVersionItems[i].TextBox.Text = ver;
            }
        }
    }
}
