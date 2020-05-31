using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.Import;
using static AngelLoader.Misc;

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
        #region Importing

        internal static async Task ImportFromDarkLoader()
        {
            string iniFile;
            bool importFMData,
                importSaves,
                importTitle,
                importSize,
                importComment,
                importReleaseDate,
                importLastPlayed,
                importFinishedOn;
            using (var f = new ImportFromDarkLoaderForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                iniFile = f.DarkLoaderIniFile;
                importFMData = f.ImportFMData;
                importTitle = f.ImportTitle;
                importSize = f.ImportSize;
                importComment = f.ImportComment;
                importReleaseDate = f.ImportReleaseDate;
                importLastPlayed = f.ImportLastPlayed;
                importFinishedOn = f.ImportFinishedOn;
                importSaves = f.ImportSaves;
            }

            if (!importFMData && !importSaves)
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            Core.View.SetRowCount(0);

            var fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Size = importSize,
                Comment = importComment,
                FinishedOn = importFinishedOn
            };

            await ImportDarkLoader.Import(iniFile, importFMData, importSaves, fields);

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await Core.View.SortAndSetFilter(forceDisplayFM: true);
        }

        internal static async Task ImportFromNDLOrFMSel(ImportType importType)
        {
            List<string> iniFiles = new List<string>();
            bool importTitle,
                importReleaseDate,
                importLastPlayed,
                importComment,
                importRating,
                importDisabledMods,
                importTags,
                importSelectedReadme,
                importFinishedOn,
                importSize;
            using (var f = new ImportFromMultipleInisForm(importType))
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                foreach (string file in f.IniFiles) iniFiles.Add(file);
                importTitle = f.ImportTitle;
                importReleaseDate = f.ImportReleaseDate;
                importLastPlayed = f.ImportLastPlayed;
                importComment = f.ImportComment;
                importRating = f.ImportRating;
                importDisabledMods = f.ImportDisabledMods;
                importTags = f.ImportTags;
                importSelectedReadme = f.ImportSelectedReadme;
                importFinishedOn = f.ImportFinishedOn;
                importSize = f.ImportSize;
            }

            if (iniFiles.All(x => x.IsWhiteSpace()))
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            // We're modifying the data that FMsDGV pulls from when it redraws. This will at least prevent a
            // selection changed event from firing while we do it, as that could be really bad potentially.
            Core.View.SetRowCount(0);

            var fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Comment = importComment,
                Rating = importRating,
                DisabledMods = importDisabledMods,
                Tags = importTags,
                SelectedReadme = importSelectedReadme,
                FinishedOn = importFinishedOn,
                Size = importSize
            };

            foreach (string file in iniFiles)
            {
                if (file.IsWhiteSpace()) continue;

                bool success = await (importType == ImportType.FMSel
                    ? ImportFMSel.Import(file, fields)
                    : ImportNDL.Import(file, fields));
            }

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await Core.View.SortAndSetFilter(forceDisplayFM: true);
        }

        // TODO: Finish implementing
        #region ImportFromMultipleLoaders
#if false
        internal static async Task ImportFromMultipleLoaders()
        {
            ImportList importPriorities;
            string dlIniFile;
            bool dlImportSaves;
            var FMSelIniFiles = new List<string>();
            var NDLIniFiles = new List<string>();
            using (var f = new ImportFromMultipleLoadersForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;

                importPriorities = f.ImportPriorities.DeepCopy();
                dlIniFile = f.DL_IniFile;
                dlImportSaves = f.DL_ImportSaves;
                foreach (var item in f.FMSelIniFiles) FMSelIniFiles.Add(item);
                foreach (var item in f.NDLIniFiles) NDLIniFiles.Add(item);
            }

            var dlFields = new FieldsToImport();
            var fmSelFields = new FieldsToImport();
            var ndlFields = new FieldsToImport();

        #region Fill DL fields

            dlFields.Title = importPriorities.Title == ImportPriority.DarkLoader;
            dlFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.DarkLoader;
            dlFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.DarkLoader;
            dlFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.DarkLoader;
            dlFields.Comment = importPriorities.Comment == ImportPriority.DarkLoader;
            dlFields.Size = importPriorities.Size == ImportPriority.DarkLoader;

        #endregion

        #region Fill FMSel fields

            fmSelFields.Title = importPriorities.Title == ImportPriority.FMSel;
            fmSelFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.FMSel;
            fmSelFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.FMSel;
            fmSelFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.FMSel;
            fmSelFields.Comment = importPriorities.Comment == ImportPriority.FMSel;
            fmSelFields.Rating = importPriorities.Rating == ImportPriority.FMSel;
            fmSelFields.DisabledMods = importPriorities.DisabledMods == ImportPriority.FMSel;
            fmSelFields.Tags = importPriorities.Tags == ImportPriority.FMSel;
            fmSelFields.SelectedReadme = importPriorities.SelectedReadme == ImportPriority.FMSel;
            fmSelFields.Size = importPriorities.Size == ImportPriority.FMSel;

        #endregion

        #region Fill NDL fields

            ndlFields.Title = importPriorities.Title == ImportPriority.NewDarkLoader;
            ndlFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.NewDarkLoader;
            ndlFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.NewDarkLoader;
            ndlFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.NewDarkLoader;
            ndlFields.Comment = importPriorities.Comment == ImportPriority.NewDarkLoader;
            ndlFields.Rating = importPriorities.Rating == ImportPriority.NewDarkLoader;
            ndlFields.DisabledMods = importPriorities.DisabledMods == ImportPriority.NewDarkLoader;
            ndlFields.Tags = importPriorities.Tags == ImportPriority.NewDarkLoader;
            ndlFields.SelectedReadme = importPriorities.SelectedReadme == ImportPriority.NewDarkLoader;
            ndlFields.Size = importPriorities.Size == ImportPriority.NewDarkLoader;

        #endregion

            bool importFromDL = false;
            bool importFromFMSel = false;
            bool importFromNDL = false;

        #region Set import bools

            // There's enough manual twiddling of these fields going on, so using reflection.
            // Not a bottleneck here.

            const BindingFlags bFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var p in dlFields.GetType().GetFields(bFlags))
            {
                if (p.FieldType == typeof(bool) && (bool)p.GetValue(dlFields))
                {
                    importFromDL = true;
                    break;
                }
            }

            foreach (var p in fmSelFields.GetType().GetFields(bFlags))
            {
                if (p.FieldType == typeof(bool) && (bool)p.GetValue(fmSelFields))
                {
                    importFromFMSel = true;
                    break;
                }
            }

            foreach (var p in ndlFields.GetType().GetFields(bFlags))
            {
                if (p.FieldType == typeof(bool) && (bool)p.GetValue(ndlFields))
                {
                    importFromNDL = true;
                    break;
                }
            }

        #endregion

        #region Check for if nothing was selected to import

            if (!dlImportSaves &&
                ((!importFromDL && !importFromFMSel && !importFromNDL) ||
                (dlIniFile.IsEmpty() && FMSelIniFiles.Count == 0 && NDLIniFiles.Count == 0)))
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

        #endregion

            try
            {
                // Must do this
                View.SetRowCount(0);

                // Extremely important!
                ImportCommon.FMsPriority.Clear();

                if (importFromDL || dlImportSaves)
                {
                    bool success = await ImportDarkLoader.Import(dlIniFile, true, dlImportSaves, FMDataIniList, dlFields);
                    if (!success) return;
                }

                if (importFromFMSel)
                {
                    foreach (var f in FMSelIniFiles)
                    {
                        if (f.IsWhiteSpace()) continue;
                        bool success = await ImportFMSel.Import(f, FMDataIniList, fmSelFields);
                        if (!success) return;
                    }
                }

                if (importFromNDL)
                {
                    foreach (var f in NDLIniFiles)
                    {
                        if (f.IsWhiteSpace()) continue;
                        bool success = await ImportNDL.Import(f, FMDataIniList, ndlFields);
                        if (!success) return;
                    }
                }
            }
            finally
            {
                // Must do this
                await View.SortAndSetFilter(forceDisplayFM: true);
            }
        }
#endif
        #endregion

        #endregion

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
        MergeImportedFMData(ImportType importType, List<FanMission> importedFMs, FieldsToImport? fields = null
            /*, bool addMergedFMsToPriorityList = false*/)
        {
            fields ??= new FieldsToImport
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
                        if (fields.ReleaseDate && importedFM.ReleaseDate.DateTime != null)
                        {
                            mainFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                            priorityFMData.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                        }
                        if (fields.LastPlayed)
                        {
                            mainFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                            priorityFMData.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
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
                        newFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                        priorityFMData.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                    }
                    if (fields.LastPlayed)
                    {
                        newFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                        priorityFMData.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
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
