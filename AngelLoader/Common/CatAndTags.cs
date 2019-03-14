using System.Collections.Generic;

namespace AngelLoader.Common
{
    public sealed class CatAndTags
    {
        public string Category;
        public List<string> Tags = new List<string>();
    }

    public sealed class GlobalCatAndTags
    {
        public GlobalCatOrTag Category = new GlobalCatOrTag();
        public List<GlobalCatOrTag> Tags = new List<GlobalCatOrTag>();
    }

    public sealed class GlobalCatOrTag
    {
        public string Name;

        /// <summary>
        /// If true, the tag will never be removed from the global list even if no FMs are using it.
        /// </summary>
        public bool IsPreset;

        /// <summary>
        /// Keeps track of the number of FMs that are using this tag. If a tag is removed from an FM and its
        /// <see cref="UsedCount"/> in the global list is greater than 0, then its <see cref="UsedCount"/> will
        /// be decremented by one and it will not be removed from the global list. This is for performance: it's
        /// much faster to simply keep track of what needs removing than to rebuild the list every time a tag is
        /// removed.
        /// </summary>
        public int UsedCount;
    }
}
