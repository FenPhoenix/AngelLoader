using System;
using System.Collections.Generic;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.CommonUtils;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMTags
    {
        internal static void AddTagToFM(FanMission fm, string catAndTag, bool rebuildGlobalTags = true)
        {
            AddTagsToFMAndGlobalList(catAndTag, fm.Tags, addToGlobalList: false);
            UpdateFMTagsString(fm);
            if (rebuildGlobalTags) RebuildGlobalTags();
        }

        internal static void RebuildGlobalTags()
        {
            PresetTags.DeepCopyTo(GlobalTags);
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                AddTagsToFMAndGlobalList(fm.TagsString, fm.Tags);
            }
        }

        internal static bool RemoveTagFromFM(FanMission fm, string catText, string tagText, bool isCategory)
        {
            if ((isCategory && catText.IsWhiteSpace()) || (!isCategory && tagText.IsWhiteSpace())) return false;

            // Parent node (category)
            if (isCategory)
            {
                // TODO: These messageboxes are annoying, but they prevent accidental deletion.
                // Figure out something better.
                bool cont = Core.View.AskToContinue(LText.TagsTab.AskRemoveCategory, LText.TagsTab.TabText, true);
                if (!cont) return false;

                CatAndTags? cat = fm.Tags.Find(x => x.Category == catText);
                if (cat != null)
                {
                    fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);
                }
            }
            // Child node (tag)
            else
            {
                bool cont = Core.View.AskToContinue(LText.TagsTab.AskRemoveTag, LText.TagsTab.TabText, true);
                if (!cont) return false;

                CatAndTags? cat = fm.Tags.Find(x => x.Category == catText);
                string? tag = cat?.Tags.Find(x => x == tagText);
                if (tag != null)
                {
                    cat!.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);
                }
            }

            RebuildGlobalTags();

            Ini.WriteFullFMDataIni();

            return true;
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
                        if (gCat.Category != "misc") list.Add(gCat.Category + ":");
                    }
                    else
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (!text.Second.IsWhiteSpace() && !gTag.ContainsI(text.Second)) continue;
                            if (gCat.Category == "misc")
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
                if (gCat.Category == "misc")
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

        // Update fm.TagsString here. We keep TagsString around because when we're reading, writing, and merging
        // FMs, we don't want to spend time converting back and forth. So Tags is session-only, and only gets
        // filled out for FMs that will be displayed. TagsString is the one that gets saved and loaded, and must
        // be kept in sync with Tags. This should ONLY be called when a tag is added or removed. Keep it simple
        // so we can see and follow the logic.
        private static void UpdateFMTagsString(FanMission fm)
        {
            var intermediateList = new List<string>();
            foreach (CatAndTags item in fm.Tags)
            {
                if (item.Tags.Count == 0)
                {
                    intermediateList.Add(item.Category);
                }
                else
                {
                    foreach (string tag in item.Tags)
                    {
                        intermediateList.Add(item.Category + ":" + tag);
                    }
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < intermediateList.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(intermediateList[i]);
            }

            fm.TagsString = sb.ToString();
        }

        // Very awkward procedure that accesses global state in the name of only doing one iteration
        // TODO: Test perf when 1000+ FMs each have a bunch of tags
        internal static void AddTagsToFMAndGlobalList(string tagsToAdd, CatAndTagsList existingFMTags,
                                                      bool addToGlobalList = true)
        {
            if (tagsToAdd.IsEmpty()) return;

            string[] tagsArray = tagsToAdd.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in tagsArray)
            {
                string cat, tag;

                int colonCount = item.CountCharsUpToAmount(':', 2);

                // No way josé
                if (colonCount > 1) continue;

                if (colonCount == 1)
                {
                    int index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty() || tag.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                // Note: We've already converted cat to lowercase, so we just do straight == to shave time off

                #region FM tags

                CatAndTags? match = null;
                for (int i = 0; i < existingFMTags.Count; i++)
                {
                    if (existingFMTags[i].Category == cat)
                    {
                        match = existingFMTags[i];
                        break;
                    }
                }
                if (match == null)
                {
                    existingFMTags.Add(new CatAndTags { Category = cat });
                    existingFMTags[existingFMTags.Count - 1].Tags.Add(tag);
                }
                else
                {
                    if (!match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }

                #endregion

                if (!addToGlobalList) continue;

                #region Global tags

                CatAndTags? globalMatch = null;
                for (int i = 0; i < GlobalTags.Count; i++)
                {
                    if (GlobalTags[i].Category == cat)
                    {
                        globalMatch = GlobalTags[i];
                        break;
                    }
                }
                if (globalMatch == null)
                {
                    GlobalTags.Add(new CatAndTags { Category = cat });
                    GlobalTags[GlobalTags.Count - 1].Tags.Add(tag);
                }
                else
                {
                    string? ft = null;
                    for (int i = 0; i < globalMatch.Tags.Count; i++)
                    {
                        if (globalMatch.Tags[i].EqualsI(tag))
                        {
                            ft = globalMatch.Tags[i];
                            break;
                        }
                    }
                    if (ft == null)
                    {
                        globalMatch.Tags.Add(tag);
                    }
                }

                #endregion
            }
        }
    }
}
