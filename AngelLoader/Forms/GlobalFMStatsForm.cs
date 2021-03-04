/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class GlobalFMStatsForm : DarkForm
    {
        public GlobalFMStatsForm()
        {
            InitializeComponent();

            CalculateStats();

            Localize();
        }

        private void Localize()
        {
            Text = LText.GlobalFMStats.TitleText;

            FMsInDatabaseLabel.Text = LText.GlobalFMStats.FMsInDatabase;
            AvailableFMsLabel.Text = LText.GlobalFMStats.AvailableFMs;
            T1Label.Text = LText.Global.Thief1_Colon;
            T2Label.Text = LText.Global.Thief2_Colon;
            T3Label.Text = LText.Global.Thief3_Colon;
            SS2Label.Text = LText.Global.SystemShock2_Colon;
            UnscannedLabel.Text = LText.GlobalFMStats.Unscanned;
            UnsupportedLabel.Text = LText.GlobalFMStats.Unsupported;

            OKButton.Text = LText.Global.OK;
        }

        private void CalculateStats()
        {
            FMsInDatabaseTextBox.Text = FMDataIniList.Count.ToString();
            AvailableFMsTextBox.Text = FMsViewList.Count.ToString();

            int t1FMs = 0;
            int t2FMs = 0;
            int t3FMs = 0;
            int ss2FMs = 0;
            int unscannedFMs = 0;
            int unsupportedFMs = 0;
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                switch (fm.Game)
                {
                    case Game.Thief1:
                        t1FMs++;
                        break;
                    case Game.Thief2:
                        t2FMs++;
                        break;
                    case Game.Thief3:
                        t3FMs++;
                        break;
                    case Game.SS2:
                        ss2FMs++;
                        break;
                    case Game.Null:
                        unscannedFMs++;
                        break;
                    case Game.Unsupported:
                        unsupportedFMs++;
                        break;
                }
            }

            T1TextBox.Text = t1FMs.ToString();
            T2TextBox.Text = t2FMs.ToString();
            T3TextBox.Text = t3FMs.ToString();
            SS2TextBox.Text = ss2FMs.ToString();
            UnscannedTextBox.Text = unscannedFMs.ToString();
            UnsupportedTextBox.Text = unsupportedFMs.ToString();
        }
    }
}
*/