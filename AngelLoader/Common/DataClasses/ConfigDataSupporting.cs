﻿#define FenGen_TypeSource

using System;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses
{
    #region Columns

    internal sealed class ColumnData
    {
        internal Column Id;
        internal int DisplayIndex = -1;
        internal int Width = Defaults.ColumnWidth;
        internal bool Visible = true;
    }

    // Public for interface use
    public enum Column
    {
        Game,
        Installed,
        Title,
        Archive,
        Author,
        Size,
        Rating,
        Finished,
        ReleaseDate,
        LastPlayed,
        DateAdded,
        DisabledMods,
        Comment
    }

    #endregion

    public enum HideableFilterControls
    {
        Title,
        Author,
        ReleaseDate,
        LastPlayed,
        Tags,
        FinishedState,
        Rating,
        ShowUnsupported,
        ShowRecentAtTop
    }

    internal enum GameOrganization { ByTab, OneList }

    // Public for interface use
    public enum RatingDisplayStyle { NewDarkLoader, FMSel }

    internal enum DateFormat { CurrentCultureShort, CurrentCultureLong, Custom }

    [Flags]
    internal enum FinishedState : uint { Null = 0, Finished = 1, Unfinished = 2 }

    internal enum BackupFMData { SavesAndScreensOnly, AllChangedFiles }

    #region Top-right tabs

    internal enum TopRightTab { Statistics, EditFM, Comment, Tags, Patch }

    internal sealed class TopRightTabData
    {
        private int _displayIndex;
        internal int DisplayIndex { get => _displayIndex; set => _displayIndex = value.Clamp(0, TopRightTabsData.Count - 1); }

        internal bool Visible = true;
    }

    internal sealed class TopRightTabsData
    {
        // Perf: so we only have to get it once
        /// <summary>
        /// Returns the number of tabs that have been defined in the <see cref="TopRightTab"/> enum.
        /// </summary>
        internal static readonly int Count = Enum.GetValues(typeof(TopRightTab)).Length;

        internal readonly TopRightTabData[] Tabs = InitializedArray<TopRightTabData>(Count);

        internal TopRightTab SelectedTab = TopRightTab.Statistics;

        internal TopRightTabsData() => ResetAllDisplayIndexes();

        internal TopRightTabData StatsTab => Tabs[(int)TopRightTab.Statistics];
        internal TopRightTabData EditFMTab => Tabs[(int)TopRightTab.EditFM];
        internal TopRightTabData CommentTab => Tabs[(int)TopRightTab.Comment];
        internal TopRightTabData TagsTab => Tabs[(int)TopRightTab.Tags];
        internal TopRightTabData PatchTab => Tabs[(int)TopRightTab.Patch];

        internal void EnsureValidity()
        {
            #region Fallback if multiple tabs have the same display index

            int[] set = InitializedArray(Count, -1);

            // PERF: Unmeasurable. LINQ Distinct().Count() was 6ms. Sheesh.
            for (int i = 0; i < Tabs.Length; i++)
            {
                for (int j = 0; j < set.Length; j++)
                {
                    if (set[j] == Tabs[i].DisplayIndex)
                    {
                        ResetAllDisplayIndexes();
                        goto breakout;
                    }
                }

                set[i] = Tabs[i].DisplayIndex;
            }

            breakout:

            #endregion

            // Fallback if no tabs are visible
            if (NoneVisible()) SetAllVisible(true);

            // Fallback if selected tab is not marked as visible
            if (!Tabs[(int)SelectedTab].Visible)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Tabs[i].Visible)
                    {
                        SelectedTab = (TopRightTab)i;
                        break;
                    }
                }
            }
        }

        private bool NoneVisible()
        {
            for (int i = 0; i < Count; i++) if (Tabs[i].Visible) return false;
            return true;
        }

        private void SetAllVisible(bool visible)
        {
            for (int i = 0; i < Count; i++) Tabs[i].Visible = visible;
        }

        private void ResetAllDisplayIndexes()
        {
            for (int i = 0; i < Count; i++) Tabs[i].DisplayIndex = i;
        }
    }

    #endregion

    #region Filter

    public sealed class Filter
    {
        internal void Clear(bool clearGames = true)
        {
            Title = "";
            Author = "";
            if (clearGames) Games = Game.Null;
            Tags.Clear();
            _ratingFrom = -1;
            _ratingTo = 10;
            _releaseDateFrom = null;
            _releaseDateTo = null;
            _lastPlayedFrom = null;
            _lastPlayedTo = null;
            Finished = FinishedState.Null;
            ShowUnsupported = false;
        }

        internal string Title = "";
        internal string Author = "";
        internal Game Games = Game.Null;
        internal readonly TagsFilter Tags = new TagsFilter();

        #region Rating

        internal void SetRatingFromAndTo(int from, int to)
        {
            RatingFrom = Math.Min(from, to);
            RatingTo = Math.Max(from, to);
        }
        private int _ratingFrom = -1;
        internal int RatingFrom { get => _ratingFrom; set => _ratingFrom = value.Clamp(-1, 10); }
        private int _ratingTo = 10;
        internal int RatingTo { get => _ratingTo; set => _ratingTo = value.Clamp(-1, 10); }

        #endregion

        internal void SetDateFromAndTo(bool lastPlayed, DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from.Value.CompareTo((DateTime)to) > 0)
            {
                if (lastPlayed)
                {
                    LastPlayedFrom = to;
                    LastPlayedTo = from;
                }
                else
                {
                    ReleaseDateFrom = to;
                    ReleaseDateTo = from;
                }
            }
            else
            {
                if (lastPlayed)
                {
                    LastPlayedFrom = from;
                    LastPlayedTo = to;
                }
                else
                {
                    ReleaseDateFrom = from;
                    ReleaseDateTo = to;
                }
            }
        }

        #region Release date

        // TODO(ReleaseDateFrom/To): Why am I setting these clamped to min/max? Surely they can't be beyond those anyway?!

        private DateTime? _releaseDateFrom;
        internal DateTime? ReleaseDateFrom
        {
            get => _releaseDateFrom;
            set => _releaseDateFrom = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }
        private DateTime? _releaseDateTo;
        internal DateTime? ReleaseDateTo
        {
            get => _releaseDateTo;
            set => _releaseDateTo = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }

        #endregion

        #region Last played

        private DateTime? _lastPlayedFrom;
        internal DateTime? LastPlayedFrom
        {
            get => _lastPlayedFrom;
            set => _lastPlayedFrom = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }
        private DateTime? _lastPlayedTo;
        internal DateTime? LastPlayedTo
        {
            get => _lastPlayedTo;
            set => _lastPlayedTo = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }

        #endregion

        internal FinishedState Finished = FinishedState.Null;

        internal bool ShowUnsupported;

        internal void DeepCopyTo(Filter dest)
        {
            dest.Clear();
            dest.Title = Title;
            dest.Author = Author;
            dest.SetRatingFromAndTo(RatingFrom, RatingTo);
            dest.ShowUnsupported = ShowUnsupported;

            // DateTime is a struct (value type), so we can just assign and it copies. Phew.
            dest.SetDateFromAndTo(lastPlayed: false, ReleaseDateFrom, ReleaseDateTo);
            dest.SetDateFromAndTo(lastPlayed: true, LastPlayedFrom, LastPlayedTo);

            dest.Finished = Finished;
            dest.Games = Games;
            Tags.DeepCopyTo(dest.Tags);
        }
    }

    internal sealed class TagsFilter
    {
        internal readonly CatAndTagsList AndTags = new CatAndTagsList();
        internal readonly CatAndTagsList OrTags = new CatAndTagsList();
        internal readonly CatAndTagsList NotTags = new CatAndTagsList();

        internal void Clear()
        {
            AndTags.Clear();
            OrTags.Clear();
            NotTags.Clear();
        }

        internal bool IsEmpty() => AndTags.Count == 0 &&
                                   OrTags.Count == 0 &&
                                   NotTags.Count == 0;

        internal void DeepCopyTo(TagsFilter dest)
        {
            AndTags.DeepCopyTo(dest.AndTags);
            OrTags.DeepCopyTo(dest.OrTags);
            NotTags.DeepCopyTo(dest.NotTags);
        }
    }

    #endregion

    internal enum SettingsTab { Paths, FMDisplay, Other }

    // TODO: This name is confusing, it sounds like it refers to an entire FanMission object or something
    // Naming this is brutally difficult. If we call it SelectedFM, it sounds like it's encapsulating an entire
    // FM object, and var names ("selectedFM" / "selFM") sound like they're of type FanMission. FMSelectionData
    // or FMSelectionInfo is likely to be shortened to fmSelData/fmSelInfo, and then it sounds like we're talking
    // about FMSel the application. Calling it FMInstNameAndScrollIndex is clear and descriptive but annoyingly
    // long and shortening it is impossible (fmINASI?!). Also, we have multiple of them (one per game tab and then
    // one global), so that would now be FMInstNameAndScrollIndexes which is an awkward half-plural.
    // SelectionMetadata? SelectedFMTag/Handle? Maybe we put an underscore like fm_SelData?
    // Maybe FMInstNameAndScrollIndex really is the least worst name here.
    public sealed class SelectedFM
    {
        internal void DeepCopyTo(SelectedFM dest)
        {
            dest.IndexFromTop = IndexFromTop;
            dest.InstalledName = InstalledName;
        }

        internal void Clear()
        {
            IndexFromTop = 0;
            InstalledName = "";
        }

        internal string InstalledName = "";

        private int _indexFromTop;
        /// <summary>
        /// The index relative to the first displayed item (not the first item period) in the list.
        /// </summary>
        internal int IndexFromTop { get => _indexFromTop; set => _indexFromTop = value.ClampToZero(); }
    }

    internal sealed class GameTabsState
    {
        internal GameTabsState() => InitializeArrays(SupportedGameCount, out SelectedFMs, out Filters);

        #region Selected FMs

        private readonly SelectedFM[] SelectedFMs;

        internal SelectedFM GetSelectedFM(GameIndex index) => SelectedFMs[(uint)index];

        #endregion

        #region Filters

        private readonly Filter[] Filters;

        internal Filter GetFilter(GameIndex index) => Filters[(uint)index];

        #endregion

        // TODO: Add sorted column / sort order as a per-tab thing

        internal void DeepCopyTo(GameTabsState dest)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                SelectedFMs[i].DeepCopyTo(dest.SelectedFMs[i]);
                Filters[i].DeepCopyTo(dest.Filters[i]);
            }
        }

        internal void ClearAllSelectedFMs()
        {
            for (int i = 0; i < SupportedGameCount; i++) SelectedFMs[i].Clear();
        }

        internal void ClearAllFilters()
        {
            for (int i = 0; i < SupportedGameCount; i++) Filters[i].Clear();
        }
    }
}
