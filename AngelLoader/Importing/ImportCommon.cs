using System.Collections.Generic;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Importing
{
    internal enum ImportType
    {
        DarkLoader,
        NewDarkLoader,
        FMSel
    }

    internal static class ImportCommon
    {
        internal static List<int> MergeDarkLoaderFMData(List<FanMission> importedFMs, List<FanMission> mainList)
        {
            return MergeImportedFMData(ImportType.DarkLoader, importedFMs, mainList);
        }

        internal static List<int> MergeNDLFMData(List<FanMission> importedFMs, List<FanMission> mainList)
        {
            return MergeImportedFMData(ImportType.NewDarkLoader, importedFMs, mainList);
        }

        internal static List<int> MergeFMSelFMData(List<FanMission> importedFMs, List<FanMission> mainList)
        {
            return MergeImportedFMData(ImportType.FMSel, importedFMs, mainList);
        }

        private static List<int> MergeImportedFMData(ImportType importType, List<FanMission> importedFMs,
            List<FanMission> mainList)
        {
            var checkedList = new List<FanMission>();
            var importedFMIndexes = new List<int>();
            int initCount = mainList.Count;
            int indexPastEnd = initCount - 1;

            for (int impFMi = 0; impFMi < importedFMs.Count; impFMi++)
            {
                var importedFM = importedFMs[impFMi];

                bool existingFound = false;
                for (int mainFMi = 0; mainFMi < initCount; mainFMi++)
                {
                    var mainFM = mainList[mainFMi];

                    if (!mainFM.Checked &&
                        (importType == ImportType.DarkLoader &&
                        mainFM.Archive.EqualsI(importedFM.Archive)) ||
                        (importType == ImportType.NewDarkLoader &&
                         mainFM.InstalledDir.EqualsI(importedFM.InstalledDir)))
                    {
                        if (!importedFM.Title.IsEmpty()) mainFM.Title = importedFM.Title;
                        if (importedFM.ReleaseDate != null) mainFM.ReleaseDate = importedFM.ReleaseDate;
                        mainFM.LastPlayed = importedFM.LastPlayed;
                        mainFM.FinishedOn = importedFM.FinishedOn;
                        mainFM.Comment = importedFM.Comment;

                        if (importType == ImportType.NewDarkLoader ||
                            importType == ImportType.FMSel)
                        {
                            mainFM.Rating = importedFM.Rating;
                            mainFM.DisabledMods = importedFM.DisabledMods;
                            mainFM.DisableAllMods = importedFM.DisableAllMods;
                            mainFM.TagsString = importedFM.TagsString;
                            mainFM.SelectedReadme = importedFM.SelectedReadme;
                        }
                        if (importType == ImportType.NewDarkLoader)
                        {
                            if (mainFM.SizeBytes == 0) mainFM.SizeBytes = importedFM.SizeBytes;
                        }
                        else if (importType == ImportType.FMSel && mainFM.FinishedOn == 0 && !mainFM.FinishedOnUnknown)
                        {
                            mainFM.FinishedOnUnknown = true;
                        }

                        mainFM.Checked = true;

                        // So we only loop through checked FMs when we reset them
                        checkedList.Add(mainFM);

                        importedFMIndexes.Add(mainFMi);

                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    var newFM = new FanMission
                    {
                        Archive = importedFM.Archive,
                        InstalledDir = importedFM.InstalledDir,
                        Title =
                            !importedFM.Title.IsEmpty() ? importedFM.Title :
                            !importedFM.Archive.IsEmpty() ? importedFM.Archive :
                            importedFM.InstalledDir,
                        ReleaseDate = importedFM.ReleaseDate,
                        LastPlayed = importedFM.LastPlayed,
                        FinishedOn = importedFM.FinishedOn,
                        Comment = importedFM.Comment,
                    };

                    if (importType == ImportType.NewDarkLoader ||
                        importType == ImportType.FMSel)
                    {
                        newFM.Rating = importedFM.Rating;
                        newFM.DisabledMods = importedFM.DisabledMods;
                        newFM.DisableAllMods = importedFM.DisableAllMods;
                        newFM.TagsString = importedFM.TagsString;
                        newFM.SelectedReadme = importedFM.SelectedReadme;
                    }
                    if (importType == ImportType.NewDarkLoader)
                    {
                        newFM.SizeBytes = importedFM.SizeBytes;
                    }
                    else if (importType == ImportType.FMSel)
                    {
                        newFM.FinishedOnUnknown = true;
                    }

                    mainList.Add(newFM);
                    indexPastEnd++;
                    importedFMIndexes.Add(indexPastEnd);
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;

            return importedFMIndexes;
        }
    }
}
