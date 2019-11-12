#define FenGen_TypeSource

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses
{
    //internal sealed class ConfigVar
    //{
    //    internal string Name = "";
    //    internal string Command = "";
    //}

    // Startup perf: we don't need to convert them on ini read, we can lazy-load their heavy DateTime? objects
    // later when we go to display them
    internal sealed class ExpandableDate
    {
        private bool _expanded;
        private DateTime? _dateTime;

        internal string UnixDateString { get; set; } = "";

        internal DateTime? DateTime
        {
            get
            {
                if (_expanded)
                {
                    return _dateTime;
                }
                else
                {
                    _dateTime = ExpandDateTime(UnixDateString);
                    _expanded = true;
                    return _dateTime;
                }
            }
            set
            {
                _dateTime = value;
                UnixDateString = value != null
                    ? new DateTimeOffset((DateTime)value).ToUnixTimeSeconds().ToString("X")
                    : "";
                _expanded = true;
            }
        }
    }

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
        DisabledMods,
        Comment
    }

    #endregion

    internal enum GameOrganization { ByTab, OneList }

    // Public for interface use
    public enum RatingDisplayStyle { NewDarkLoader, FMSel }

    internal enum DateFormat { CurrentCultureShort, CurrentCultureLong, Custom }

    [Flags, PublicAPI]
    internal enum FinishedState : uint { Null = 0, Finished = 1, Unfinished = 2 }

    [Flags, PublicAPI]
    internal enum FinishedOn : uint { None = 0, Normal = 1, Hard = 2, Expert = 4, Extreme = 8 }

    // IMPORTANT: Do not rename elements or compatibility will break!
    [Flags]
    internal enum CustomResources
    {
        None = 0,
        Map = 1,
        Automap = 2,
        Scripts = 4,
        Textures = 8,
        Sounds = 16,
        Objects = 32,
        Creatures = 64,
        Motions = 128,
        Movies = 256,
        Subtitles = 512
    }

    internal enum BackupFMData { SavesAndScreensOnly, AllChangedFiles }

    #region Top-right tabs

    internal enum TopRightTab { Statistics, EditFM, Comment, Tags, Patch }

    internal sealed class TopRightTabData
    {
        private int _position;
        internal int Position { get => _position; set => _position = value.Clamp(0, TopRightTabsCount - 1); }

        internal bool Visible = true;
    }

    internal sealed class TopRightTabsData
    {
        internal readonly TopRightTabData[] Tabs = new TopRightTabData[TopRightTabsCount];

        internal TopRightTab SelectedTab = TopRightTab.Statistics;

        internal TopRightTabsData()
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i] = new TopRightTabData();
            ResetAllPositions();
        }

        internal TopRightTabData StatsTab => Tabs[(int)TopRightTab.Statistics];
        internal TopRightTabData EditFMTab => Tabs[(int)TopRightTab.EditFM];
        internal TopRightTabData CommentTab => Tabs[(int)TopRightTab.Comment];
        internal TopRightTabData TagsTab => Tabs[(int)TopRightTab.Tags];
        internal TopRightTabData PatchTab => Tabs[(int)TopRightTab.Patch];

        internal void EnsureValidity()
        {
            #region Fallback if multiple tabs have the same position

            int[] set = { -1, -1, -1, -1, -1 };

            Debug.Assert(set.Length == TopRightTabsCount, nameof(set) + ".Length != " + nameof(TopRightTabsCount));

            // PERF: Unmeasurable. LINQ Distinct().Count() was 6ms. Sheesh.
            for (int i = 0; i < Tabs.Length; i++)
            {
                for (int j = 0; j < set.Length; j++)
                {
                    if (set[j] == Tabs[i].Position)
                    {
                        ResetAllPositions();
                        goto breakout;
                    }
                }

                set[i] = Tabs[i].Position;
            }

            breakout:

            #endregion

            // Fallback if no tabs are visible
            if (NoneVisible()) SetAllVisible(true);

            // Fallback if selected tab is not marked as visible
            if (!Tabs[(int)SelectedTab].Visible)
            {
                for (int i = 0; i < TopRightTabsCount; i++)
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
            for (int i = 0; i < TopRightTabsCount; i++) if (Tabs[i].Visible) return false;
            return true;
        }

        private void SetAllVisible(bool visible)
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i].Visible = visible;
        }

        private void ResetAllPositions()
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i].Position = i;
        }
    }

    #endregion

    #region Filter

    internal sealed class Filter
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
        internal GameTabsState()
        {
            SelectedFMs = new SelectedFM[SupportedGameCount];
            Filters = new Filter[SupportedGameCount];
            for (int i = 0; i < SupportedGameCount; i++)
            {
                SelectedFMs[i] = new SelectedFM();
                Filters[i] = new Filter();
            }
        }

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
            for (int i = 0; i < SelectedFMs.Length; i++) SelectedFMs[i].Clear();
        }

        internal void ClearAllFilters()
        {
            for (int i = 0; i < SupportedGameCount; i++) Filters[i].Clear();
        }
    }
}
