﻿using System;
using System.Collections.Generic;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMTags
    {
        #region Add tag

        internal static void AddTagOperation(FanMission fm, string catAndTag)
        {
            if (!catAndTag.CharCountIsAtLeast(':', 2) && !catAndTag.IsWhiteSpace())
            {
                AddTagToFM(fm, catAndTag);
                Ini.WriteFullFMDataIni();
                Core.View.DisplayFMTags(fm.Tags);
            }

            Core.View.ClearTagsSearchBox();
        }

        internal static void AddTagToFM(FanMission fm, string catAndTag, bool rebuildGlobalTags = true)
        {
            AddTagsToFMAndGlobalList(catAndTag, fm.Tags, addToGlobalList: false);
            UpdateFMTagsString(fm);
            if (rebuildGlobalTags) RebuildGlobalTags();
        }

        #endregion

        #region Remove tag

        internal static void RemoveTagOperation()
        {
            var fm = Core.View.GetSelectedFMOrNull();
            if (fm == null) return;

            (string catText, string tagText) = Core.View.SelectedCategoryAndTag();
            if (catText.IsEmpty() && tagText.IsEmpty()) return;

            bool isCategory = tagText.IsEmpty();
            bool success = RemoveTagFromFM(fm, catText, tagText, isCategory);
            if (success)
            {
                Core.View.DisplayFMTags(fm.Tags);
            }
        }

        private static bool RemoveTagFromFM(FanMission fm, string catText, string tagText, bool isCategory)
        {
            if ((isCategory && catText.IsWhiteSpace()) || (!isCategory && tagText.IsWhiteSpace())) return false;

            // Parent node (category)
            if (isCategory)
            {
                // TODO: These messageboxes are annoying, but they prevent accidental deletion.
                // Figure out something better.
                bool cont = Dialogs.AskToContinue(LText.TagsTab.AskRemoveCategory, LText.TagsTab.TabText, true);
                if (!cont) return false;

                if (fm.Tags.ContainsKey(catText))
                {
                    fm.Tags.Remove(catText);
                    UpdateFMTagsString(fm);
                }
            }
            // Child node (tag)
            else
            {
                bool cont = Dialogs.AskToContinue(LText.TagsTab.AskRemoveTag, LText.TagsTab.TabText, true);
                if (!cont) return false;

                if (fm.Tags.TryGetValue(catText, out FMTagsCollection tagsList) &&
                    tagsList.Contains(tagText))
                {
                    tagsList.Remove(tagText);
                    if (tagsList.Count == 0) fm.Tags.Remove(catText);
                    UpdateFMTagsString(fm);
                }
            }

            RebuildGlobalTags();

            Ini.WriteFullFMDataIni();

            return true;
        }

        #endregion

        internal static void RebuildGlobalTags()
        {
            PresetTags.DeepCopyTo(GlobalTags);
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                AddTagsToFMAndGlobalList(fm.TagsString, fm.Tags);
            }
        }

        internal static List<string> GetMatchingTagsList(string searchText)
        {
            // Smartasses who try to break it get nothing
            if (searchText.CharCountIsAtLeast(':', 2) || searchText.IsWhiteSpace()) return new List<string>();

            (string First, string Second) text;

            int index = searchText.IndexOf(':');
            if (index > -1)
            {
                text.First = searchText.Substring(0, index).Trim();
                text.Second = searchText.Substring(index + 1).Trim();
            }
            else
            {
                text.First = searchText.Trim();
                text.Second = "";
            }

            // Shut up, it works
            var list = new List<string>();
            foreach (var gCat in GlobalTags)
            {
                if (gCat.Category.ContainsI(text.First))
                {
                    if (gCat.Tags.Count == 0)
                    {
                        if (gCat.Category != PresetTags.MiscCategory) list.Add(gCat.Category + ":");
                    }
                    else
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (!text.Second.IsWhiteSpace() && !gTag.ContainsI(text.Second)) continue;
                            if (gCat.Category == PresetTags.MiscCategory)
                            {
                                if (text.Second.IsWhiteSpace() && !gCat.Category.ContainsI(text.First))
                                {
                                    list.Add(gTag);
                                }
                            }
                            else
                            {
                                list.Add(gCat.Category + ": " + gTag);
                            }
                        }
                    }
                }
                // if, not else if - we want to display found tags both categorized and uncategorized
                if (gCat.Category == PresetTags.MiscCategory)
                {
                    foreach (var gTag in gCat.Tags)
                    {
                        if (gTag.ContainsI(searchText)) list.Add(gTag);
                    }
                }
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);

            return list;
        }

        internal static string TagsToString(FMCategoriesCollection tagsList, bool writeEmptyCategories)
        {
            var intermediateTagsList = new List<string>();
            foreach (CatAndTagsList item in tagsList)
            {
                if (item.Tags.Count == 0 && writeEmptyCategories)
                {
                    intermediateTagsList.Add(item.Category + ":");
                }
                else
                {
                    foreach (string tag in item.Tags)
                    {
                        intermediateTagsList.Add(item.Category + ":" + tag);
                    }
                }
            }

            return string.Join(",", intermediateTagsList);
        }

        // Update fm.TagsString here. We keep TagsString around because when we're reading, writing, and merging
        // FMs, we don't want to spend time converting back and forth. So Tags is session-only, and only gets
        // filled out for FMs that will be displayed. TagsString is the one that gets saved and loaded, and must
        // be kept in sync with Tags. This should ONLY be called when a tag is added or removed. Keep it simple
        // so we can see and follow the logic.
        private static void UpdateFMTagsString(FanMission fm)
        {
            fm.TagsString = TagsToString(fm.Tags, writeEmptyCategories: false);
        }

        internal static bool TryGetCatAndTag(string item, out string cat, out string tag)
        {
            switch (item.CountCharsUpToAmount(':', 2))
            {
                case > 1:
                    cat = "";
                    tag = "";
                    return false;
                case 1:
                    int index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    break;
                default:
                    cat = PresetTags.MiscCategory;
                    tag = item.Trim();
                    break;
            }

            return true;
        }

        // Very awkward procedure that accesses global state in the name of only doing one iteration
        internal static void AddTagsToFMAndGlobalList(
            string tagsToAdd,
            FMCategoriesCollection existingFMTags,
            bool addToGlobalList = true)
        {
            if (tagsToAdd.IsWhiteSpace()) return;

            string[] tagsArray = tagsToAdd.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tagsArray.Length; i++)
            {
                if (!TryGetCatAndTag(tagsArray[i], out string cat, out string tag) ||
                    cat.IsEmpty() || tag.IsEmpty())
                {
                    continue;
                }

                #region FM tags

                if (existingFMTags.TryGetValue(cat, out FMTagsCollection tagsList))
                {
                    tagsList.Add(tag);
                }
                else
                {
                    existingFMTags.Add(cat, new FMTagsCollection { tag });
                }

                #endregion

                if (!addToGlobalList) continue;

                #region Global tags

                if (GlobalTags.TryGetValue(cat, out FMTagsCollection globalTagsList))
                {
                    globalTagsList.Add(tag);
                }
                else
                {
                    GlobalTags.Add(cat, new FMTagsCollection { tag });
                }

                #endregion
            }
        }
    }
}
