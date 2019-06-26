using System;
using System.Collections.Generic;
using System.Linq;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.DataClasses.TopRightTabEnumStatic;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigVar
    {
        internal string Name = "";
        internal string Command = "";
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

    [Flags] internal enum Game : uint { Null = 0, Thief1 = 1, Thief2 = 2, Thief3 = 4, Unsupported = 8 }

    internal enum GameOrganization { ByTab, OneList }

    // Public for interface use
    public enum RatingDisplayStyle { NewDarkLoader, FMSel }

    internal enum DateFormat { CurrentCultureShort, CurrentCultureLong, Custom }

    [Flags] internal enum FinishedState : uint { Null = 0, Finished = 1, Unfinished = 2 }

    [Flags] internal enum FinishedOn : uint { None = 0, Normal = 1, Hard = 2, Expert = 4, Extreme = 8 }

    internal enum BackupFMData { SavesAndScreensOnly, AllChangedFiles }

    #region Top-right tabs

    // Dopey, but for perf so we only have to get it once
    internal static class TopRightTabEnumStatic
    {
        internal static readonly int TopRightTabsCount = Enum.GetValues(typeof(TopRightTab)).Length;
    }

    internal enum TopRightTab { Statistics, EditFM, Comment, Tags, Patch }

    internal sealed class TopRightTabData
    {
        private int _position;
        internal int Position { get => _position; set => _position = value.Clamp(0, TopRightTabsCount); }

        internal bool Visible = true;
    }

    internal sealed class TopRightTabsData
    {
        internal TopRightTabData[] Tabs = new TopRightTabData[TopRightTabsCount];

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
            // Fallback if multiple tabs have the same position
            if (Tabs.Distinct(new TopRightTabPositionComparer()).Count() != TopRightTabsCount) ResetAllPositions();

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
            int count = 0;
            for (int i = 0; i < TopRightTabsCount; i++) if (Tabs[i].Visible) count++;
            return count == 0;
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
            RatingFrom = -1;
            RatingTo = 10;
            ReleaseDateFrom = null;
            ReleaseDateTo = null;
            LastPlayedFrom = null;
            LastPlayedTo = null;
            Finished = FinishedState.Null;
            ShowJunk = false;
        }

        internal bool IsEmpty()
        {
            return Title.IsWhiteSpace() &&
                   Author.IsWhiteSpace() &&
                   Games == Game.Null &&
                   Tags.IsEmpty() &&
                   ReleaseDateFrom == null &&
                   ReleaseDateTo == null &&
                   LastPlayedFrom == null &&
                   LastPlayedTo == null &&
                   RatingFrom == -1 &&
                   RatingTo == 10 &&
                   Finished == FinishedState.Null &&
                   ShowJunk;
        }

        internal string Title = "";
        internal string Author = "";
        internal Game Games = Game.Null;
        internal TagsFilter Tags = new TagsFilter();

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

        #region Release date

        internal void SetReleaseDateFromAndTo(DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from.Value.CompareTo(to) > 0)
            {
                ReleaseDateFrom = to;
                ReleaseDateTo = from;
            }
            else
            {
                ReleaseDateFrom = from;
                ReleaseDateTo = to;
            }
        }

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

        internal void SetLastPlayedFromAndTo(DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from.Value.CompareTo(to) > 0)
            {
                LastPlayedFrom = to;
                LastPlayedTo = from;
            }
            else
            {
                LastPlayedFrom = from;
                LastPlayedTo = to;
            }
        }

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

        internal bool ShowJunk;

        internal void DeepCopyTo(Filter dest)
        {
            dest.Clear();
            dest.Title = Title;
            dest.Author = Author;
            dest.SetRatingFromAndTo(RatingFrom, RatingTo);
            dest.ShowJunk = ShowJunk;

            var relFrom = ReleaseDateFrom == null
                ? (DateTime?)null
                : new DateTime(ReleaseDateFrom.Value.Year, ReleaseDateFrom.Value.Month,
                    ReleaseDateFrom.Value.Day, 0, 0, 0, ReleaseDateFrom.Value.Kind);
            var relTo = ReleaseDateTo == null
                ? (DateTime?)null
                : new DateTime(ReleaseDateTo.Value.Year, ReleaseDateTo.Value.Month,
                    ReleaseDateTo.Value.Day, 0, 0, 0, ReleaseDateTo.Value.Kind);

            dest.SetReleaseDateFromAndTo(relFrom, relTo);

            var lpFrom = LastPlayedFrom == null
                ? (DateTime?)null
                : new DateTime(LastPlayedFrom.Value.Year, LastPlayedFrom.Value.Month,
                    LastPlayedFrom.Value.Day, 0, 0, 0, LastPlayedFrom.Value.Kind);
            var lpTo = LastPlayedTo == null
                ? (DateTime?)null
                : new DateTime(LastPlayedTo.Value.Year, LastPlayedTo.Value.Month,
                    LastPlayedTo.Value.Day, 0, 0, 0, LastPlayedTo.Value.Kind);

            dest.SetLastPlayedFromAndTo(lpFrom, lpTo);

            dest.Finished = Finished;
            dest.Games = Games;
            Tags.DeepCopyTo(dest.Tags);
        }
    }

    internal sealed class TagsFilter
    {
        internal CatAndTagsList AndTags = new CatAndTagsList();
        internal CatAndTagsList OrTags = new CatAndTagsList();
        internal CatAndTagsList NotTags = new CatAndTagsList();

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

    internal sealed class SelectedFM
    {
        internal void DeepCopyTo(SelectedFM dest)
        {
            dest.IndexFromTop = IndexFromTop;
            dest.InstalledName = InstalledName;
        }

        internal void Clear()
        {
            IndexFromTop = 0;
            InstalledName = null;
        }

        private string _installedName;
        internal string InstalledName { get => _installedName; set => _installedName = value.IsEmpty() ? null : value; }

        private int _indexFromTop;
        /// <summary>
        /// The index relative to the first displayed item (not the first item period) in the list.
        /// </summary>
        internal int IndexFromTop { get => _indexFromTop; set => _indexFromTop = value.ClampToZero(); }
    }

    internal sealed class GameTabsState
    {
        internal readonly SelectedFM T1SelFM = new SelectedFM();
        internal readonly SelectedFM T2SelFM = new SelectedFM();
        internal readonly SelectedFM T3SelFM = new SelectedFM();

        internal readonly Filter T1Filter = new Filter();
        internal readonly Filter T2Filter = new Filter();
        internal readonly Filter T3Filter = new Filter();

        internal void DeepCopyTo(GameTabsState dest)
        {
            T1Filter.DeepCopyTo(dest.T1Filter);
            T2Filter.DeepCopyTo(dest.T2Filter);
            T3Filter.DeepCopyTo(dest.T3Filter);
            T1SelFM.DeepCopyTo(dest.T1SelFM);
            T2SelFM.DeepCopyTo(dest.T2SelFM);
            T3SelFM.DeepCopyTo(dest.T3SelFM);
        }

        internal void ClearSelectedFMs()
        {
            T1SelFM.Clear();
            T2SelFM.Clear();
            T3SelFM.Clear();
        }
    }
}
