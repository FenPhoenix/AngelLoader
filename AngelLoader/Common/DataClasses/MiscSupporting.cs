using System;
using System.Collections.Generic;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigVar
    {
        internal string Name = "";
        internal string Command = "";
    }

    internal sealed class ColumnData
    {
        internal Column Id;
        internal int DisplayIndex = -1;
        internal int Width = 100;
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

    internal enum DateFormat
    {
        CurrentCultureShort,
        CurrentCultureLong,
        Custom
    }

    internal enum Game
    {
        Thief1,
        Thief2,
        Thief3,
        Unsupported
    }

    internal enum FinishedState
    {
        Finished,
        Unfinished
    }

    internal enum BackupFMData
    {
        SavesAndScreensOnly,
        AllChangedFiles
    }

    internal enum GameOrganization
    {
        ByTab,
        OneList
    }

    public enum RatingDisplayStyle
    {
        NewDarkLoader,
        FMSel
    }

    [Flags]
    internal enum FinishedOn
    {
        None = 0,
        Normal = 1,
        Hard = 2,
        Expert = 4,
        Extreme = 8
    }

    internal sealed class TopRightTabOrder
    {
        private const int MaxIndex = 4;

        private int _statsTabPosition = 0;
        internal int StatsTabPosition { get => _statsTabPosition; set => _statsTabPosition = value.Clamp(0, MaxIndex); }

        private int _editFMTabPosition = 1;
        internal int EditFMTabPosition { get => _editFMTabPosition; set => _editFMTabPosition = value.Clamp(0, MaxIndex); }

        private int _commentTabPosition = 2;
        internal int CommentTabPosition { get => _commentTabPosition; set => _commentTabPosition = value.Clamp(0, MaxIndex); }

        private int _tagsTabPosition = 3;
        internal int TagsTabPosition { get => _tagsTabPosition; set => _tagsTabPosition = value.Clamp(0, MaxIndex); }

        private int _patchTabPosition = 4;
        internal int PatchTabPosition { get => _patchTabPosition; set => _patchTabPosition = value.Clamp(0, MaxIndex); }
    }

    internal sealed class Filter
    {
        internal void Clear(bool clearGames = true)
        {
            Title = "";
            Author = "";
            if (clearGames) Games.Clear();
            Tags.Clear();
            RatingFrom = -1;
            RatingTo = 10;
            ReleaseDateFrom = null;
            ReleaseDateTo = null;
            LastPlayedFrom = null;
            LastPlayedTo = null;
            Finished.Clear();
            ShowJunk = false;
        }

        internal bool IsEmpty()
        {
            return Title.IsWhiteSpace() &&
                   Author.IsWhiteSpace() &&
                   Games.Count == 0 &&
                   Tags.IsEmpty() &&
                   ReleaseDateFrom == null &&
                   ReleaseDateTo == null &&
                   LastPlayedFrom == null &&
                   LastPlayedTo == null &&
                   RatingFrom == -1 &&
                   RatingTo == 10 &&
                   Finished.Count == 0 &&
                   ShowJunk;
        }

        internal string Title = "";
        internal string Author = "";
        internal List<Game> Games = new List<Game>();
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

        internal List<FinishedState> Finished = new List<FinishedState>();

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

            foreach (var finished in Finished) dest.Finished.Add(finished);
            foreach (var game in Games) dest.Games.Add(game);
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

    internal enum TopRightTab
    {
        Statistics,
        EditFM,
        Comment,
        Tags,
        Patch
    }

    internal enum SettingsTab
    {
        Paths,
        FMDisplay,
        Other
    }

    internal sealed class SelectedFM
    {
        private int _indexFromTop;
        private string _installedName;

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

        internal string InstalledName
        {
            get => _installedName;
            set => _installedName = value.IsEmpty() ? null : value;
        }
        /// <summary>
        /// The index relative to the first displayed item (not the first item period) in the list.
        /// </summary>
        internal int IndexFromTop
        {
            get => _indexFromTop;
            set => _indexFromTop = value.ClampToZero();
        }
    }

    internal sealed class GameTabsState
    {
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

        internal SelectedFM T1SelFM { get; } = new SelectedFM();
        internal SelectedFM T2SelFM { get; } = new SelectedFM();
        internal SelectedFM T3SelFM { get; } = new SelectedFM();

        internal Filter T1Filter { get; } = new Filter();
        internal Filter T2Filter { get; } = new Filter();
        internal Filter T3Filter { get; } = new Filter();
    }
}
