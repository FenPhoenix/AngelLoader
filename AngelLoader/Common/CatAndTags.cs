using System.Collections.Generic;

namespace AngelLoader.Common
{
    internal sealed class CatAndTags
    {
        internal string Category;
        internal List<string> Tags = new List<string>();
    }

    internal sealed class GlobalCatAndTags
    {
        internal GlobalCatOrTag Category = new GlobalCatOrTag();
        internal List<GlobalCatOrTag> Tags = new List<GlobalCatOrTag>();
    }

    internal sealed class GlobalCatOrTag
    {
        internal string Name;

        /// <summary>
        /// If true, the tag will never be removed from the global list even if no FMs are using it.
        /// </summary>
        internal bool IsPreset;

        /// <summary>
        /// Keeps track of the number of FMs that are using this tag. If a tag is removed from an FM and its
        /// <see cref="UsedCount"/> in the global list is greater than 0, then its <see cref="UsedCount"/> will
        /// be decremented by one and it will not be removed from the global list. This is for performance: it's
        /// much faster to simply keep track of what needs removing than to rebuild the list every time a tag is
        /// removed.
        /// </summary>
        internal int UsedCount;
    }
}
