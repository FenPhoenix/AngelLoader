using System;
using System.Collections.Generic;
using System.Text;
using AngelLoader.DataClasses;
using SpanExtensions;
using static AL_Common.Common;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

internal static class FMTags
{
    #region Add tag

    internal static void AddTagsToFM(FanMission fm, string tags, bool rebuildGlobalTags = true, StringBuilder? sb = null)
    {
        AddTagsToFMAndGlobalList(tags, fm.Tags, addToGlobalList: false);
        UpdateFMTagsString(fm, sb);
        if (rebuildGlobalTags) RebuildGlobalTags();
    }

    #endregion

    #region Remove tag

    internal static bool RemoveTagFromFM(FanMission fm, string catText, string tagText, bool isCategory)
    {
        if (isCategory ? catText.IsWhiteSpace() : tagText.IsWhiteSpace()) return false;

        // TODO: These messageboxes are annoying, but they prevent accidental deletion.
        // Figure out something better.
        (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
            message: isCategory ? LText.TagsTab.AskRemoveCategory : LText.TagsTab.AskRemoveTag,
            title: LText.TagsTab.TabText,
            icon: MBoxIcon.None,
            yes: LText.Global.Yes,
            no: LText.Global.No
        );
        if (result == MBoxButton.No) return false;

        // Parent node (category)
        if (isCategory)
        {
            if (fm.Tags.ContainsKey(catText))
            {
                fm.Tags.Remove(catText);
                UpdateFMTagsString(fm);
            }
        }
        // Child node (tag)
        else
        {
            if (fm.Tags.TryGetValue(catText, out FMTagsCollection? tagsList) &&
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
        foreach (CatAndTagsList gCat in GlobalTags)
        {
            if (gCat.Category.ContainsI(text.First))
            {
                if (gCat.Tags.Count == 0)
                {
                    if (gCat.Category != PresetTags.MiscCategory) list.Add(gCat.Category + ":");
                }
                else
                {
                    foreach (string gTag in gCat.Tags)
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
                foreach (string gTag in gCat.Tags)
                {
                    if (gTag.ContainsI(searchText)) list.Add(gTag);
                }
            }
        }

        list.Sort(StringComparer.OrdinalIgnoreCase);

        return list;
    }

    public const int TagsToStringSBInitialCapacity = 100;
    internal static string TagsToString(FMCategoriesCollection tagsList, bool writeEmptyCategories, StringBuilder? sb = null)
    {
        if (sb == null)
        {
            sb = new StringBuilder(TagsToStringSBInitialCapacity);
        }
        else
        {
            sb.Clear();
        }

        if (sb.Capacity > ByteSize.KB * 10)
        {
            sb.Capacity = TagsToStringSBInitialCapacity;
        }

        for (int i = 0; i < tagsList.Count; i++)
        {
            CatAndTagsList item = tagsList[i];
            if (item.Tags.Count == 0 && writeEmptyCategories)
            {
                sb.Append(item.Category).Append(':').Append(',');
            }
            else
            {
                for (int j = 0; j < item.Tags.Count; j++)
                {
                    string tag = item.Tags[j];
                    sb.Append(item.Category).Append(':').Append(tag).Append(',');
                }
            }
        }

        // Cheap and easy to understand
        if (sb.Length > 0 && sb[^1] == ',')
        {
            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }

    // Update fm.TagsString here. We keep TagsString around because when we're reading, writing, and merging
    // FMs, we don't want to spend time converting back and forth. So Tags is session-only, and only gets
    // filled out for FMs that will be displayed. TagsString is the one that gets saved and loaded, and must
    // be kept in sync with Tags. This should ONLY be called when a tag is added or removed. Keep it simple
    // so we can see and follow the logic.
    private static void UpdateFMTagsString(FanMission fm, StringBuilder? sb = null)
    {
        fm.TagsString = TagsToString(fm.Tags, writeEmptyCategories: false, sb);
    }

    internal static bool TryGetCatAndTag(ReadOnlySpan<char> item, out string cat, out string tag)
    {
        switch (item.CountCharsUpToAmount(':', 2))
        {
            case > 1:
                cat = "";
                tag = "";
                return false;
            case 1:
                int index = item.IndexOf(':');
                cat = item[..index].Trim().ToString();
                // Save an alloc if we're ascii lowercase already (case conversion always allocs, even if
                // the new string is the same as the old)
                if (!cat.IsAsciiLower()) cat = cat.ToLowerInvariant();
                tag = item[(index + 1)..].Trim().ToString();
                break;
            default:
                cat = PresetTags.MiscCategory;
                tag = item.Trim().ToString();
                break;
        }

        return true;
    }

    /*
    Very awkward procedure that accesses global state in the name of only doing one iteration
    TODO(AddTagsToFMAndGlobalList): The set of tags adding stuff in here is very awkwardly coupled
    We did it so we can keep this one loop instead of two, but that's a stupid micro-optimization that almost
    certainly gains us nothing perceptible but makes the logic in here very difficult to follow.
    We should pull these apart and put them back together in a better way.

    @PerfScale: This method takes ~2.6s out of ~7.25s FM-find time for the huge set.
    
    (From Framework - we're not doing some of this work in .NET 8)
    
    string.Split(): 853ms
    List.Add(): 691ms (it's all EnsureCapacity() - at least one call per FM!)
    HashSet.Add(): 390ms
    TryGetCatAndTag(): 210ms
    FMTagsCollection.ctor: 144ms (almost all GC - where's the garbage coming from?)

    Everything else takes negligible time in the grand scheme of things. 78ms on down, which is fine given the
    size of the huge set.

    We should switch to a much more lightweight way of storing the tags. Maybe just ensure proper format on the
    string and then wrap it in a struct with a list of indices for cats/tags? Would the string processing take
    too much time? Could we work with even a crappily-formatted string?
    */
    internal static void AddTagsToFMAndGlobalList(
        string tagsToAdd,
        FMCategoriesCollection existingFMTags,
        bool addToGlobalList = true)
    {
        if (tagsToAdd.IsWhiteSpace()) return;

        // @SpanExtOverflow: RemoveEmptyEntries uses un-optimized tail call recursion (stack overflow possible!)
        foreach (ReadOnlySpan<char> item in ReadOnlySpanExtensions.SplitAny(tagsToAdd, CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!TryGetCatAndTag(item, out string cat, out string tag) ||
                cat.IsEmpty() || tag.IsEmpty())
            {
                continue;
            }

            #region FM tags

            if (existingFMTags.TryGetValue(cat, out FMTagsCollection? tagsList))
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

            if (GlobalTags.TryGetValue(cat, out FMTagsCollection? globalTagsList))
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
