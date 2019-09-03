using System.Collections.Generic;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Importing
{
    // Has to be public so it can be passed to a public constructor on a form
    public enum ImportType
    {
        DarkLoader,
        NewDarkLoader,
        FMSel
    }

    public enum ImportPriority
    {
        NoImport,
        DarkLoader,
        FMSel,
        NewDarkLoader
    }

    internal sealed class ImportList
    {
        internal ImportPriority Title;
        internal ImportPriority ReleaseDate;
        internal ImportPriority LastPlayed;
        internal ImportPriority FinishedOn;
        internal ImportPriority Comment;
        internal ImportPriority Rating;
        internal ImportPriority DisabledMods;
        internal ImportPriority Tags;
        internal ImportPriority SelectedReadme;
        internal ImportPriority Size;

        internal ImportList DeepCopy()
        {
            return new ImportList
            {
                Title = Title,
                ReleaseDate = ReleaseDate,
                LastPlayed = LastPlayed,
                FinishedOn = FinishedOn,
                Comment = Comment,
                Rating = Rating,
                DisabledMods = DisabledMods,
                Tags = Tags,
                SelectedReadme = SelectedReadme,
                Size = Size
            };
        }
    }

    internal sealed class FieldsToImport
    {
        internal bool Title;
        internal bool ReleaseDate;
        internal bool LastPlayed;
        internal bool FinishedOn;
        internal bool Comment;
        internal bool Rating;
        internal bool DisabledMods;
        internal bool Tags;
        internal bool SelectedReadme;
        internal bool Size;
    }

    internal static class ImportCommon
    {
        internal static List<FanMission>
        MergeImportedFMData(ImportType importType, List<FanMission> importedFMs, List<FanMission> mainList, FieldsToImport fields = null)
        {
            if (fields == null)
            {
                fields = new FieldsToImport
                {
                    Title = true,
                    ReleaseDate = true,
                    LastPlayed = true,
                    FinishedOn = true,
                    Comment = true,
                    Rating = true,
                    DisabledMods = true,
                    Tags = true,
                    SelectedReadme = true,
                    Size = true
                };
            }

            // Perf
            var checkedList = new List<FanMission>();
            int initCount = mainList.Count;

            // We can't just send back the list we got in, because we will have deep-copied them to the main list
            var importedFMsInMainList = new List<FanMission>();

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
                        (importType == ImportType.FMSel &&
                         (!importedFM.Archive.IsEmpty() && mainFM.Archive.EqualsI(importedFM.Archive)) ||
                          importedFM.InstalledDir.EqualsI(mainFM.InstalledDir)) ||
                        (importType == ImportType.NewDarkLoader &&
                         mainFM.InstalledDir.EqualsI(importedFM.InstalledDir)))
                    {
                        if (fields.Title && !importedFM.Title.IsEmpty())
                        {
                            mainFM.Title = importedFM.Title;
                        }
                        if (fields.ReleaseDate && importedFM.ReleaseDate != null)
                        {
                            mainFM.ReleaseDate = importedFM.ReleaseDate;
                        }
                        if (fields.LastPlayed)
                        {
                            mainFM.LastPlayed = importedFM.LastPlayed;
                        }
                        if (fields.FinishedOn)
                        {
                            mainFM.FinishedOn = importedFM.FinishedOn;
                            if (importType != ImportType.FMSel) mainFM.FinishedOnUnknown = false;
                        }
                        if (fields.Comment)
                        {
                            mainFM.Comment = importedFM.Comment;
                        }

                        if (importType == ImportType.NewDarkLoader ||
                            importType == ImportType.FMSel)
                        {
                            if (fields.Rating)
                            {
                                mainFM.Rating = importedFM.Rating;
                            }
                            if (fields.DisabledMods)
                            {
                                mainFM.DisabledMods = importedFM.DisabledMods;
                                mainFM.DisableAllMods = importedFM.DisableAllMods;
                            }
                            if (fields.Tags)
                            {
                                mainFM.TagsString = importedFM.TagsString;
                            }
                            if (fields.SelectedReadme)
                            {
                                mainFM.SelectedReadme = importedFM.SelectedReadme;
                            }
                        }
                        if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                        {
                            if (fields.Size && mainFM.SizeBytes == 0)
                            {
                                mainFM.SizeBytes = importedFM.SizeBytes;
                            }
                        }
                        else if (importType == ImportType.FMSel && mainFM.FinishedOn == 0 && !mainFM.FinishedOnUnknown)
                        {
                            if (fields.FinishedOn)
                            {
                                mainFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                            }
                        }

                        mainFM.MarkedScanned = true;

                        mainFM.Checked = true;

                        // So we only loop through checked FMs when we reset them
                        checkedList.Add(mainFM);

                        importedFMsInMainList.Add(mainFM);

                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    var newFM = new FanMission
                    {
                        Archive = importedFM.Archive,
                        InstalledDir = importedFM.InstalledDir
                    };
                    if (fields.Title)
                    {
                        newFM.Title = !importedFM.Title.IsEmpty() ? importedFM.Title :
                            !importedFM.Archive.IsEmpty() ? importedFM.Archive.RemoveExtension() :
                            importedFM.InstalledDir;
                    }
                    if (fields.ReleaseDate)
                    {
                        newFM.ReleaseDate = importedFM.ReleaseDate;
                    }
                    if (fields.LastPlayed)
                    {
                        newFM.LastPlayed = importedFM.LastPlayed;
                    }
                    if (fields.Comment)
                    {
                        newFM.Comment = importedFM.Comment;
                    }

                    if (importType == ImportType.NewDarkLoader ||
                        importType == ImportType.FMSel)
                    {
                        if (fields.Rating)
                        {
                            newFM.Rating = importedFM.Rating;
                        }
                        if (fields.DisabledMods)
                        {
                            newFM.DisabledMods = importedFM.DisabledMods;
                            newFM.DisableAllMods = importedFM.DisableAllMods;
                        }
                        if (fields.Tags)
                        {
                            newFM.TagsString = importedFM.TagsString;
                        }
                        if (fields.SelectedReadme)
                        {
                            newFM.SelectedReadme = importedFM.SelectedReadme;
                        }
                    }
                    if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                    {
                        if (fields.Size)
                        {
                            newFM.SizeBytes = importedFM.SizeBytes;
                        }
                        if (fields.FinishedOn)
                        {
                            newFM.FinishedOn = importedFM.FinishedOn;
                        }
                    }
                    else if (importType == ImportType.FMSel)
                    {
                        if (fields.FinishedOn)
                        {
                            newFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                        }
                    }

                    newFM.MarkedScanned = true;

                    mainList.Add(newFM);
                    importedFMsInMainList.Add(newFM);
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;

            return importedFMsInMainList;
        }
    }
}
