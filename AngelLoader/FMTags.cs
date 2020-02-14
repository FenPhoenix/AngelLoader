using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMTags
    {
        internal static void AddTagToFM(FanMission fm, string catAndTag)
        {
            AddTagsToFMAndGlobalList(catAndTag, fm.Tags);
            UpdateFMTagsString(fm);
            Ini.WriteFullFMDataIni();
        }

        internal static bool RemoveTagFromFM(FanMission fm, string catText, string tagText)
        {
            if (tagText.IsEmpty()) return false;

            // Parent node (category)
            if (catText.IsEmpty())
            {
                // TODO: These messageboxes are annoying, but they prevent accidental deletion.
                // Figure out something better.
                bool cont = Core.View.AskToContinue(LText.TagsTab.AskRemoveCategory, LText.TagsTab.TabText, true);
                if (!cont) return false;

                CatAndTags? cat = fm.Tags.FirstOrDefault(x => x.Category == tagText);
                if (cat != null)
                {
                    fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    // TODO: Profile the FirstOrDefaults and see if I should make them for loops
                    GlobalCatAndTags? globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    if (globalCat != null && !globalCat.Category.IsPreset)
                    {
                        if (globalCat.Category.UsedCount > 0) globalCat.Category.UsedCount--;
                        if (globalCat.Category.UsedCount == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }
            // Child node (tag)
            else
            {
                bool cont = Core.View.AskToContinue(LText.TagsTab.AskRemoveTag, LText.TagsTab.TabText, true);
                if (!cont) return false;

                CatAndTags? cat = fm.Tags.FirstOrDefault(x => x.Category == catText);
                string? tag = cat?.Tags.FirstOrDefault(x => x == tagText);
                if (tag != null)
                {
                    cat!.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    GlobalCatAndTags? globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    GlobalCatOrTag? globalTag = globalCat?.Tags.FirstOrDefault(x => x.Name == tagText);
                    if (globalTag != null && !globalTag.IsPreset)
                    {
                        if (globalTag.UsedCount > 0) globalTag.UsedCount--;
                        if (globalTag.UsedCount == 0) globalCat!.Tags.Remove(globalTag);
                        if (globalCat!.Tags.Count == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }

            Ini.WriteFullFMDataIni();

            return true;
        }

        internal static List<string> GetMatchingTagsList(string searchText)
        {
            // Smartasses who try to break it get nothing
            if (searchText.CountChars(':') > 1 || searchText.IsWhiteSpace()) return new List<string>();

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
                if (gCat.Category.Name.ContainsI(text.First))
                {
                    if (gCat.Tags.Count == 0)
                    {
                        if (gCat.Category.Name != "misc") list.Add(gCat.Category.Name + ":");
                    }
                    else
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (!text.Second.IsWhiteSpace() && !gTag.Name.ContainsI(text.Second)) continue;
                            if (gCat.Category.Name == "misc")
                            {
                                if (text.Second.IsWhiteSpace() && !gCat.Category.Name.ContainsI(text.First))
                                {
                                    list.Add(gTag.Name);
                                }
                            }
                            else
                            {
                                list.Add(gCat.Category.Name + ": " + gTag.Name);
                            }
                        }
                    }
                }
                // if, not else if - we want to display found tags both categorized and uncategorized
                if (gCat.Category.Name == "misc")
                {
                    foreach (var gTag in gCat.Tags)
                    {
                        if (gTag.Name.ContainsI(searchText)) list.Add(gTag.Name);
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
        internal static void UpdateFMTagsString(FanMission fm)
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
        internal static void AddTagsToFMAndGlobalList(string tagsToAdd, CatAndTagsList existingFMTags)
        {
            if (tagsToAdd.IsEmpty()) return;

            string[] tagsArray = tagsToAdd.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in tagsArray)
            {
                string cat, tag;

                int colonCount = item.CountChars(':');

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

                #region Global tags

                GlobalCatAndTags? globalMatch = null;
                for (int i = 0; i < GlobalTags.Count; i++)
                {
                    if (GlobalTags[i].Category.Name == cat)
                    {
                        globalMatch = GlobalTags[i];
                        break;
                    }
                }
                if (globalMatch == null)
                {
                    GlobalTags.Add(new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = cat, UsedCount = 1 } });
                    GlobalTags[GlobalTags.Count - 1].Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                }
                else
                {
                    globalMatch.Category.UsedCount++;

                    GlobalCatOrTag? ft = null;
                    for (int i = 0; i < globalMatch.Tags.Count; i++)
                    {
                        if (globalMatch.Tags[i].Name.EqualsI(tag))
                        {
                            ft = globalMatch.Tags[i];
                            break;
                        }
                    }
                    if (ft == null)
                    {
                        globalMatch.Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                    }
                    else
                    {
                        ft.UsedCount++;
                    }
                }

                #endregion
            }
        }
    }
}
