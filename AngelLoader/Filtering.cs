using System;
using System.Collections.Generic;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

internal static class Filtering
{
    /// <summary>
    /// Very simple "fuzzy" search, only finds exact strings or strings present with other chars in between.
    /// <br/>
    /// Does no Levenshtein or anything fancy.
    /// </summary>
    /// <param name="hay"></param>
    /// <param name="needle"></param>
    /// <returns></returns>
    private static (bool Matched, bool ExactMatch)
    ContainsI_Subsequence(this string hay, string needle)
    {
        var fail = (false, false);

        int hayLength = hay.Length;
        int needleLength = needle.Length;

        if (needleLength == 0) return fail;
        // Don't do a needle > hay length check, because we want to support many duplicate chars (possibly
        // beyond the hay length) in the needle

        /*
        This algo sometimes rejects results that have the actual exact string in them, and if you try to
        tune it so it doesn't, then it gets other problems. It's just too simplistic to really work that
        well, so do a strict check first to cover that case.
        */
        if (hay.ContainsI(needle)) return (true, true);

        // Repetition everywhere so that we make sure only the ascii path runs if it's ascii, because with
        // big if((this and that) or that) statements, the non-ascii path always runs even if we're ascii and
        // blah.

        int needleUsed = 0;
        int skippedInARow = 0;

        int startIndex = -1;
        for (int i = 0; i < hayLength; i++)
        {
            if (BothAreAscii(hay[i], needle[0]))
            {
                if (hay[i].EqualsIAscii(needle[0]))
                {
                    startIndex = i;
                    break;
                }
            }
            else if (hay[i].EqualsIAscii(needle[0]) || hay[i].ToString().EqualsI(needle[0].ToString()))
            {
                startIndex = i;
                break;
            }
        }

        if (startIndex == -1) return fail;

        for (int i = startIndex; i < hayLength; ++i)
        {
            if (needleUsed == needleLength)
            {
                return (true, false);
            }

            if (skippedInARow > 2) return fail;

            char hayChar = hay[i];
            char needleChar = needle[needleUsed];

            // Don't allocate unless we need to...
            if (BothAreAscii(hayChar, needleChar))
            {
                if (hayChar.EqualsIAscii(needleChar))
                {
                    skippedInARow = 0;
                    char lastChar;
                    char currentChar;
                    do
                    {
                        ++needleUsed;
                        lastChar = needleChar;
                    } while (needleUsed < needleLength - 1 && ((currentChar = needle[needleUsed]).EqualsIAscii(lastChar) || char.IsWhiteSpace(currentChar)));
                }
                else if (!char.IsWhiteSpace(hayChar))
                {
                    ++skippedInARow;
                }
            }
            else
            {
                if (hayChar.EqualsIAscii(needleChar) || hayChar.ToString().EqualsI(needleChar.ToString()))
                {
                    skippedInARow = 0;
                    char lastChar;
                    do
                    {
                        ++needleUsed;
                        lastChar = needleChar;
                    } while (needleUsed < needleLength - 1 && needle[needleUsed].EqualsIAscii(lastChar));
                }
                else if (!char.IsWhiteSpace(hayChar))
                {
                    ++skippedInARow;
                }
            }
        }

        return (needleUsed == needleLength, false);
    }

    internal static (bool Matched, bool ExactMatch)
    ContainsI_TextFilter(this string hay, string needle)
    {
        if (Config.EnableFuzzySearch)
        {
            return hay.ContainsI_Subsequence(needle);
        }
        else
        {
            bool contains = hay.ContainsI(needle);
            return (contains, contains);
        }
    }

    internal static (bool Matched, bool ExactMatch)
    FMTitleContains_AllTests(FanMission fm, string title, string titleTrimmed)
    {
        bool extIsArchive = fm.Archive.ExtIsArchive();
        if (extIsArchive &&
            // @RAR: Rule of three, let's pack these away in an array or something
            (titleTrimmed.EqualsI(".zip") ||
             titleTrimmed.EqualsI(".7z") ||
             titleTrimmed.EqualsI(".rar")))
        {
            bool matched = fm.Archive.EndsWithI(titleTrimmed);
            return (matched, matched);
        }
        else
        {
            var titleContains = fm.Title.ContainsI_TextFilter(title);
            return titleContains.Matched
                ? titleContains
                : extIsArchive
                    ? (fm.Archive.IndexOf(title, 0, fm.Archive.LastIndexOf('.'), StringComparison.OrdinalIgnoreCase) > -1, true)
                    : fm.Game == Game.TDM ? (fm.DisplayArchive.IndexOf(title, 0, StringComparison.OrdinalIgnoreCase) > -1, true)
                    : (false, false);
        }
    }

    private static void FilterTags(
        FMCategoriesCollection fmTags,
        FMCategoriesCollection andTags,
        FMCategoriesCollection orTags,
        FMCategoriesCollection notTags,
        ref bool shownInFilter)
    {
        if (andTags.Count > 0 ||
            orTags.Count > 0 ||
            notTags.Count > 0)
        {
            if (fmTags.Count == 0 && notTags.Count == 0)
            {
                shownInFilter = false;
                return;
            }

            // I don't ever want to see these damn things again

            #region And

            if (andTags.Count > 0)
            {
                bool andPass = true;
                foreach (CatAndTagsList andTag in andTags)
                {
                    if (!fmTags.TryGetValue(andTag.Category, out FMTagsCollection match))
                    {
                        andPass = false;
                        break;
                    }

                    if (andTag.Tags.Count > 0)
                    {
                        foreach (string andTagTag in andTag.Tags)
                        {
                            if (!match.Contains(andTagTag))
                            {
                                andPass = false;
                                break;
                            }
                        }

                        if (!andPass) break;
                    }
                }

                if (!andPass)
                {
                    shownInFilter = false;
                    return;
                }
            }

            #endregion

            #region Or

            if (orTags.Count > 0)
            {
                bool orPass = false;
                foreach (CatAndTagsList orTag in orTags)
                {
                    if (!fmTags.TryGetValue(orTag.Category, out FMTagsCollection match))
                    {
                        continue;
                    }

                    if (orTag.Tags.Count > 0)
                    {
                        foreach (string orTagTag in orTag.Tags)
                        {
                            if (match.Contains(orTagTag))
                            {
                                orPass = true;
                                break;
                            }
                        }

                        if (orPass) break;
                    }
                    else
                    {
                        orPass = true;
                    }
                }

                if (!orPass)
                {
                    shownInFilter = false;
                    return;
                }
            }

            #endregion

            #region Not

            if (notTags.Count > 0)
            {
                bool notPass = true;
                foreach (CatAndTagsList notTag in notTags)
                {
                    if (!fmTags.TryGetValue(notTag.Category, out FMTagsCollection match))
                    {
                        continue;
                    }

                    if (notTag.Tags.Count == 0)
                    {
                        notPass = false;
                        continue;
                    }

                    if (notTag.Tags.Count > 0)
                    {
                        foreach (string notTagTag in notTag.Tags)
                        {
                            if (match.Contains(notTagTag))
                            {
                                notPass = false;
                                break;
                            }
                        }

                        if (!notPass) break;
                    }
                }

                if (!notPass)
                {
                    shownInFilter = false;
                    // Explicit continue for safety in case the order of these gets changed or another
                    // gets added
                    // ReSharper disable once RedundantJumpStatement
                    return;
                }
            }

            #endregion
        }
    }

    internal static (FanMission? TitleExactMatch, FanMission? AuthorExactMatch)
    SetFilter()
    {
#if DEBUG || (Release_Testing && !RT_StartupOnly)
        Core.View.SetDebug2Text(Int_TryParseInv(Core.View.GetDebug2Text(), out int result) ? (result + 1).ToStrInv() : "1");
#endif

        (FanMission? titleExactMatch, FanMission? authorExactMatch) ret = (null, null);

        Filter viewFilter = Core.View.GetFilter();

        #region Set filters that are stored in control state

        viewFilter.Title = Core.View.GetTitleFilter();
        viewFilter.Author = Core.View.GetAuthorFilter();

        viewFilter.Games = Core.View.GetGameFiltersEnabled();

        viewFilter.Finished = FinishedState.Null;
        if (Core.View.GetFinishedFilter()) viewFilter.Finished |= FinishedState.Finished;
        if (Core.View.GetUnfinishedFilter()) viewFilter.Finished |= FinishedState.Unfinished;

        #endregion

        List<int> filterShownIndexList = Core.View.GetFilterShownIndexList();

        filterShownIndexList.Clear();

        // Only ever add to the filter shown index list, don't subtract, as that requires constantly copying list
        // items and is therefore like eight trillion times slower.

        bool titleIsWhitespace = viewFilter.Title.IsWhiteSpace();
        string titleTrimmed = viewFilter.Title.Trim();
        bool authorIsWhitespace = viewFilter.Author.IsWhiteSpace();
        bool showUnsupported = Core.View.GetShowUnsupportedFilter();
        FMCategoriesCollection andTags = viewFilter.Tags.AndTags;
        FMCategoriesCollection orTags = viewFilter.Tags.OrTags;
        FMCategoriesCollection notTags = viewFilter.Tags.NotTags;
        bool ratingIsSet = viewFilter.RatingIsSet();
        int filterRatingFrom = viewFilter.RatingFrom;
        int filterRatingTo = viewFilter.RatingTo;
        DateTime? filterReleaseDateFrom = viewFilter.ReleaseDateFrom;
        DateTime? filterReleaseDateTo = viewFilter.ReleaseDateTo;
        bool releaseDateFilterSet = filterReleaseDateFrom != null || filterReleaseDateTo != null;
        DateTime? filterLastPlayedFrom = viewFilter.LastPlayedFrom;
        DateTime? filterLastPlayedTo = viewFilter.LastPlayedTo;
        bool lastPlayedFilterSet = filterLastPlayedFrom != null || filterLastPlayedTo != null;
        bool showAvailable = Core.View.GetShowUnavailableFMsFilter();

        for (int i = 0; i < FMsViewList.Count; i++)
        {
            FanMission fm = FMsViewList[i];

            bool shownInFilter = true;

            if (!fm.IsTopped())
            {
                (bool Match, bool ExactMatch) match;

                #region Title

                if (!titleIsWhitespace)
                {
                    if (!(match = FMTitleContains_AllTests(fm, viewFilter.Title, titleTrimmed)).Match)
                    {
                        shownInFilter = false;
                    }
                    else if (match.ExactMatch && ret.titleExactMatch == null)
                    {
                        ret.titleExactMatch = fm;
                    }
                }

                #endregion

                if (!shownInFilter) continue;

                #region Author

                if (!authorIsWhitespace)
                {
                    if (!(match = fm.Author.ContainsI_TextFilter(viewFilter.Author)).Match)
                    {
                        shownInFilter = false;
                    }
                    else if (match.ExactMatch && ret.authorExactMatch == null)
                    {
                        ret.authorExactMatch = fm;
                    }
                }

                #endregion

                if (!shownInFilter) continue;

                FilterTags(fm.Tags, andTags, orTags, notTags, ref shownInFilter);

                if (!shownInFilter) continue;

                #region Rating

                if (ratingIsSet)
                {
                    if ((fm.Rating < filterRatingFrom || fm.Rating > filterRatingTo))
                    {
                        shownInFilter = false;
                    }
                }

                #endregion

                if (!shownInFilter) continue;

                #region Release date

                if (releaseDateFilterSet)
                {
                    if ((fm.ReleaseDate.DateTime == null ||
                         (filterReleaseDateFrom != null &&
                          fm.ReleaseDate.DateTime.Value.Date.CompareTo(filterReleaseDateFrom.Value.Date) < 0) ||
                         (filterReleaseDateTo != null &&
                          fm.ReleaseDate.DateTime.Value.Date.CompareTo(filterReleaseDateTo.Value.Date) > 0)))
                    {
                        shownInFilter = false;
                    }
                }

                #endregion

                if (!shownInFilter) continue;

                #region Last played

                if (lastPlayedFilterSet)
                {
                    if ((fm.LastPlayed.DateTime == null ||
                         (filterLastPlayedFrom != null &&
                          fm.LastPlayed.DateTime.Value.Date.CompareTo(filterLastPlayedFrom.Value.Date) < 0) ||
                         (filterLastPlayedTo != null &&
                          fm.LastPlayed.DateTime.Value.Date.CompareTo(filterLastPlayedTo.Value.Date) > 0)))
                    {
                        shownInFilter = false;
                    }
                }

                #endregion

                if (!shownInFilter) continue;

                #region Finished

                if (viewFilter.Finished > FinishedState.Null)
                {
                    uint fmFinished = fm.FinishedOn;
                    bool fmFinishedOnUnknown = fm.FinishedOnUnknown;

                    if ((((fmFinished > 0 || fmFinishedOnUnknown) &&
                          !viewFilter.Finished.HasFlagFast(FinishedState.Finished)) ||
                         (fmFinished == 0 && !fmFinishedOnUnknown &&
                          !viewFilter.Finished.HasFlagFast(FinishedState.Unfinished))))
                    {
                        shownInFilter = false;
                    }
                }

                #endregion

                if (!shownInFilter) continue;
            }

            #region Marked unavailable

            if (!showAvailable)
            {
                if (fm.MarkedUnavailable)
                {
                    shownInFilter = false;
                }
            }
            else
            {
                if (!fm.MarkedUnavailable)
                {
                    shownInFilter = false;
                }
            }

            #endregion

            if (!shownInFilter) continue;

            #region Show unsupported

            if (!showUnsupported)
            {
                if (fm.Game == Game.Unsupported)
                {
                    shownInFilter = false;
                }
            }

            #endregion

            if (!shownInFilter) continue;

            #region Games

            if (viewFilter.Games > Game.Null)
            {
                if (GameIsKnownAndSupported(fm.Game) &&
                    (Config.GameOrganization == GameOrganization.ByTab || !fm.IsTopped()) &&
                    !viewFilter.Games.HasFlagFast(fm.Game))
                {
                    shownInFilter = false;
                }
            }

            #endregion

            if (!shownInFilter) continue;

            filterShownIndexList.Add(i);
        }

        return ret;
    }
}
