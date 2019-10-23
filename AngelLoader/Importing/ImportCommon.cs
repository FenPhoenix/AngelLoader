using System.Collections.Generic;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Common;

namespace AngelLoader.Importing
{
    // Has to be public so it can be passed to a public constructor on a form
    public enum ImportType
    {
        DarkLoader,
        FMSel,
        NewDarkLoader
    }

    //public enum ImportPriority
    //{
    //    NoImport,
    //    DarkLoader,
    //    FMSel,
    //    NewDarkLoader
    //}

    //internal sealed class ImportPriorities_Cascading
    //{
    //    internal ImportPriority[] Title = new ImportPriority[4];
    //    internal ImportPriority[] ReleaseDate = new ImportPriority[4];
    //    internal ImportPriority[] LastPlayed = new ImportPriority[4];
    //    internal ImportPriority[] FinishedOn = new ImportPriority[4];
    //    internal ImportPriority[] Comment = new ImportPriority[4];
    //    internal ImportPriority[] Rating = new ImportPriority[4];
    //    internal ImportPriority[] DisabledMods = new ImportPriority[4];
    //    internal ImportPriority[] Tags = new ImportPriority[4];
    //    internal ImportPriority[] SelectedReadme = new ImportPriority[4];
    //    internal ImportPriority[] Size = new ImportPriority[4];
    //}

    //internal sealed class ImportList
    //{
    //    internal ImportPriority Title;
    //    internal ImportPriority ReleaseDate;
    //    internal ImportPriority LastPlayed;
    //    internal ImportPriority FinishedOn;
    //    internal ImportPriority Comment;
    //    internal ImportPriority Rating;
    //    internal ImportPriority DisabledMods;
    //    internal ImportPriority Tags;
    //    internal ImportPriority SelectedReadme;
    //    internal ImportPriority Size;

    //    internal ImportList DeepCopy()
    //    {
    //        return new ImportList
    //        {
    //            Title = Title,
    //            ReleaseDate = ReleaseDate,
    //            LastPlayed = LastPlayed,
    //            FinishedOn = FinishedOn,
    //            Comment = Comment,
    //            Rating = Rating,
    //            DisabledMods = DisabledMods,
    //            Tags = Tags,
    //            SelectedReadme = SelectedReadme,
    //            Size = Size
    //        };
    //    }
    //}

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

    //internal sealed class PriorityList
    //{
    //    //internal List<FanMission> CurrentFMs = new List<FanMission>();
    //    //internal List<FanMission> DarkLoaderFMs = new List<FanMission>();
    //    //internal List<FanMission> FMSelFMs = new List<FanMission>();
    //    //internal List<FanMission> NewDarkLoaderFMs = new List<FanMission>();
    //    //internal List<ImportList> Priorities = new List<ImportList>();
    //    internal FanMission DarkLoaderFMData = new FanMission();
    //    internal FanMission FMSelFMData = new FanMission();
    //    internal FanMission NewDarkLoaderFMData = new FanMission();
    //    internal readonly ImportList Priority = new ImportList();
    //}

    internal static class ImportCommon
    {
        //internal static readonly Dictionary<FanMission, PriorityList>
        //FMsPriority = new Dictionary<FanMission, PriorityList>();

        //private static ImportPriority ImportTypeToPriority(ImportType importType) => ((ImportPriority)importType) + 1;

        //private static void PriorityAdd(FanMission keyFM, FanMission priorityFMData, ImportType importType, FieldsToImport fields)
        //{
        //    if (!FMsPriority.ContainsKey(keyFM)) FMsPriority.Add(keyFM, new PriorityList());

        //    var pfm = FMsPriority[keyFM];

        //    switch (importType)
        //    {
        //        case ImportType.DarkLoader:
        //            pfm.DarkLoaderFMData = priorityFMData;
        //            break;
        //        case ImportType.FMSel:
        //            pfm.FMSelFMData = priorityFMData;
        //            break;
        //        case ImportType.NewDarkLoader:
        //            pfm.NewDarkLoaderFMData = priorityFMData;
        //            break;
        //    }

        //    if (fields.Title) pfm.Priority.Title = ImportTypeToPriority(importType);
        //    if (fields.ReleaseDate) pfm.Priority.ReleaseDate = ImportTypeToPriority(importType);
        //    if (fields.LastPlayed) pfm.Priority.LastPlayed = ImportTypeToPriority(importType);
        //    if (fields.FinishedOn) pfm.Priority.FinishedOn = ImportTypeToPriority(importType);
        //    if (fields.Comment) pfm.Priority.Comment = ImportTypeToPriority(importType);
        //    if (fields.Rating) pfm.Priority.Rating = ImportTypeToPriority(importType);
        //    if (fields.DisabledMods) pfm.Priority.DisabledMods = ImportTypeToPriority(importType);
        //    if (fields.Tags) pfm.Priority.Tags = ImportTypeToPriority(importType);
        //    if (fields.SelectedReadme) pfm.Priority.SelectedReadme = ImportTypeToPriority(importType);
        //    if (fields.Size) pfm.Priority.Size = ImportTypeToPriority(importType);
        //}

        internal static List<FanMission>
        MergeImportedFMData(ImportType importType, List<FanMission> importedFMs, FieldsToImport fields = null
            /*, bool addMergedFMsToPriorityList = false*/)
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
            int initCount = FMDataIniList.Count;
            bool[] checkedArray = new bool[initCount];

            // We can't just send back the list we got in, because we will have deep-copied them to the main list
            var importedFMsInMainList = new List<FanMission>();

            for (int impFMi = 0; impFMi < importedFMs.Count; impFMi++)
            {
                var importedFM = importedFMs[impFMi];

                bool existingFound = false;
                for (int mainFMi = 0; mainFMi < initCount; mainFMi++)
                {
                    var mainFM = FMDataIniList[mainFMi];

                    if (!checkedArray[mainFMi] &&
                        (importType == ImportType.DarkLoader &&
                         mainFM.Archive.EqualsI(importedFM.Archive)) ||
                        (importType == ImportType.FMSel &&
                         (!importedFM.Archive.IsEmpty() && mainFM.Archive.EqualsI(importedFM.Archive)) ||
                          importedFM.InstalledDir.EqualsI(mainFM.InstalledDir)) ||
                        (importType == ImportType.NewDarkLoader &&
                         mainFM.InstalledDir.EqualsI(importedFM.InstalledDir)))
                    {
                        var priorityFMData = new FanMission();

                        if (fields.Title && !importedFM.Title.IsEmpty())
                        {
                            mainFM.Title = importedFM.Title;
                            priorityFMData.Title = importedFM.Title;
                        }
                        if (fields.ReleaseDate && importedFM.ReleaseDate != null)
                        {
                            mainFM.ReleaseDate = importedFM.ReleaseDate;
                            priorityFMData.ReleaseDate = importedFM.ReleaseDate;
                        }
                        if (fields.LastPlayed)
                        {
                            mainFM.LastPlayed = importedFM.LastPlayed;
                            priorityFMData.LastPlayed = importedFM.LastPlayed;
                        }
                        if (fields.FinishedOn)
                        {
                            mainFM.FinishedOn = importedFM.FinishedOn;
                            priorityFMData.FinishedOn = importedFM.FinishedOn;
                            if (importType != ImportType.FMSel)
                            {
                                mainFM.FinishedOnUnknown = false;
                                priorityFMData.FinishedOnUnknown = false;
                            }
                        }
                        if (fields.Comment)
                        {
                            mainFM.Comment = importedFM.Comment;
                            priorityFMData.Comment = importedFM.Comment;
                        }

                        if (importType == ImportType.NewDarkLoader ||
                            importType == ImportType.FMSel)
                        {
                            if (fields.Rating)
                            {
                                mainFM.Rating = importedFM.Rating;
                                priorityFMData.Rating = importedFM.Rating;
                            }
                            if (fields.DisabledMods)
                            {
                                mainFM.DisabledMods = importedFM.DisabledMods;
                                priorityFMData.DisabledMods = importedFM.DisabledMods;
                                mainFM.DisableAllMods = importedFM.DisableAllMods;
                                priorityFMData.DisableAllMods = importedFM.DisableAllMods;
                            }
                            if (fields.Tags)
                            {
                                mainFM.TagsString = importedFM.TagsString;
                                priorityFMData.TagsString = importedFM.TagsString;
                            }
                            if (fields.SelectedReadme)
                            {
                                mainFM.SelectedReadme = importedFM.SelectedReadme;
                                priorityFMData.SelectedReadme = importedFM.SelectedReadme;
                            }
                        }
                        if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                        {
                            if (fields.Size && mainFM.SizeBytes == 0)
                            {
                                mainFM.SizeBytes = importedFM.SizeBytes;
                                priorityFMData.SizeBytes = importedFM.SizeBytes;
                            }
                        }
                        else if (importType == ImportType.FMSel && mainFM.FinishedOn == 0 && !mainFM.FinishedOnUnknown)
                        {
                            if (fields.FinishedOn)
                            {
                                mainFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                                priorityFMData.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                            }
                        }

                        mainFM.MarkedScanned = true;

                        checkedArray[mainFMi] = true;

                        importedFMsInMainList.Add(mainFM);

                        //PriorityAdd(mainFM, priorityFMData, importType, fields);

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

                    var priorityFMData = new FanMission();

                    if (fields.Title)
                    {
                        newFM.Title = !importedFM.Title.IsEmpty() ? importedFM.Title :
                            !importedFM.Archive.IsEmpty() ? importedFM.Archive.RemoveExtension() :
                            importedFM.InstalledDir;
                        priorityFMData.Title = !importedFM.Title.IsEmpty() ? importedFM.Title :
                            !importedFM.Archive.IsEmpty() ? importedFM.Archive.RemoveExtension() :
                            importedFM.InstalledDir;
                    }
                    if (fields.ReleaseDate)
                    {
                        newFM.ReleaseDate = importedFM.ReleaseDate;
                        priorityFMData.ReleaseDate = importedFM.ReleaseDate;
                    }
                    if (fields.LastPlayed)
                    {
                        newFM.LastPlayed = importedFM.LastPlayed;
                        priorityFMData.LastPlayed = importedFM.LastPlayed;
                    }
                    if (fields.Comment)
                    {
                        newFM.Comment = importedFM.Comment;
                        priorityFMData.Comment = importedFM.Comment;
                    }

                    if (importType == ImportType.NewDarkLoader ||
                        importType == ImportType.FMSel)
                    {
                        if (fields.Rating)
                        {
                            newFM.Rating = importedFM.Rating;
                            priorityFMData.Rating = importedFM.Rating;
                        }
                        if (fields.DisabledMods)
                        {
                            newFM.DisabledMods = importedFM.DisabledMods;
                            priorityFMData.DisabledMods = importedFM.DisabledMods;
                            newFM.DisableAllMods = importedFM.DisableAllMods;
                            priorityFMData.DisableAllMods = importedFM.DisableAllMods;
                        }
                        if (fields.Tags)
                        {
                            newFM.TagsString = importedFM.TagsString;
                            priorityFMData.TagsString = importedFM.TagsString;
                        }
                        if (fields.SelectedReadme)
                        {
                            newFM.SelectedReadme = importedFM.SelectedReadme;
                            priorityFMData.SelectedReadme = importedFM.SelectedReadme;
                        }
                    }
                    if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                    {
                        if (fields.Size)
                        {
                            newFM.SizeBytes = importedFM.SizeBytes;
                            priorityFMData.SizeBytes = importedFM.SizeBytes;
                        }
                        if (fields.FinishedOn)
                        {
                            newFM.FinishedOn = importedFM.FinishedOn;
                            priorityFMData.FinishedOn = importedFM.FinishedOn;
                        }
                    }
                    else if (importType == ImportType.FMSel)
                    {
                        if (fields.FinishedOn)
                        {
                            newFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                            priorityFMData.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                        }
                    }

                    newFM.MarkedScanned = true;

                    FMDataIniList.Add(newFM);
                    importedFMsInMainList.Add(newFM);

                    //PriorityAdd(newFM, priorityFMData, importType, fields);
                }
            }

            return importedFMsInMainList;
        }

        //internal static void
        //MergeMultipleSets(List<FanMission> dlFMs, List<FanMission> fmSelFMs, List<FanMission> ndlFMs,
        //                  ImportPriorities_Cascading ipc)
        //{

        //}
    }
}
