//#define ENABLE_TEST

#if ENABLE_TEST

using System.IO;
using System.Threading.Tasks;
using AL_Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class TEST_
    {
        internal static async Task BackupAndRestoreTest()
        {
            FanMission fm = Core.View.GetMainSelectedFMOrNull();
            if (fm == null)
            {
                Core.Dialogs.ShowAlert("Safety - FM is null!", "Alert");
                return;
            }

            if (!fm.Archive.EqualsI("TROTB2v1.22.zip"))
            //if (!fm.Archive.EqualsI("RabenbachV2.7z"))
            {
                Core.Dialogs.ShowAlert("Safety - not TROTB2!", "Alert");
                return;
            }

            string fmInstalledPath = Path.Combine(Config.GetFMInstallPath(GameIndex.Thief2), fm.InstalledDir);
            string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive);

            try
            {
                Core.View.ShowProgressBox_Single("Working...");

                await FMBackupAndRestore.BackupFM(fm, fmInstalledPath, fmArchivePath);
            }
            finally
            {
                Core.View.HideProgressBox();
            }
        }
    }
}

#endif