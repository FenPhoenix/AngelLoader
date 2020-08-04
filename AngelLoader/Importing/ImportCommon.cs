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

                await ImportNDLOrFMSel.Import(importType, file, fields);
            }

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await Core.View.SortAndSetFilter(forceDisplayFM: true);
        }

        #endregion

        internal static List<FanMission>
        MergeImportedFMData(ImportType importType, List<FanMission> importedFMs, FieldsToImport? fields = null)
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
                        newFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                    }
                    if (fields.LastPlayed)
                    {
                        newFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
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

                    FMDataIniList.Add(newFM);
                    importedFMsInMainList.Add(newFM);
                }
            }

            return importedFMsInMainList;
        }
    }
}
