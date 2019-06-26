using System.Collections.Generic;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.Common
{
    internal static class Common
    {
        internal static readonly ConfigData Config = new ConfigData();

        // These are the FMSel preset tags. Conforming to standards here.
        internal static readonly GlobalCatAndTagsList PresetTags = new GlobalCatAndTagsList(6)
        {
            new GlobalCatAndTags {Category = new GlobalCatOrTag {Name = "author", IsPreset = true}},
            new GlobalCatAndTags {Category = new GlobalCatOrTag {Name = "contest", IsPreset = true}},
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag {Name = "genre", IsPreset = true},
                Tags = new List<GlobalCatOrTag>(5)
                {
                    new GlobalCatOrTag {Name = "action", IsPreset = true},
                    new GlobalCatOrTag {Name = "crime", IsPreset = true},
                    new GlobalCatOrTag {Name = "horror", IsPreset = true},
                    new GlobalCatOrTag {Name = "mystery", IsPreset = true},
                    new GlobalCatOrTag {Name = "puzzle", IsPreset = true}
                }
            },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag {Name = "language", IsPreset = true},
                Tags = new List<GlobalCatOrTag>(11)
                {
                    new GlobalCatOrTag {Name = "English", IsPreset = true},
                    new GlobalCatOrTag {Name = "Czech", IsPreset = true},
                    new GlobalCatOrTag {Name = "Dutch", IsPreset = true},
                    new GlobalCatOrTag {Name = "French", IsPreset = true},
                    new GlobalCatOrTag {Name = "German", IsPreset = true},
                    new GlobalCatOrTag {Name = "Hungarian", IsPreset = true},
                    new GlobalCatOrTag {Name = "Italian", IsPreset = true},
                    new GlobalCatOrTag {Name = "Japanese", IsPreset = true},
                    new GlobalCatOrTag {Name = "Polish", IsPreset = true},
                    new GlobalCatOrTag {Name = "Russian", IsPreset = true},
                    new GlobalCatOrTag {Name = "Spanish", IsPreset = true}
                }
            },
            new GlobalCatAndTags {Category = new GlobalCatOrTag {Name = "series", IsPreset = true}},
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag {Name = "misc", IsPreset = true},
                Tags = new List<GlobalCatOrTag>(6)
                {
                    new GlobalCatOrTag {Name = "campaign", IsPreset = true},
                    new GlobalCatOrTag {Name = "demo", IsPreset = true},
                    new GlobalCatOrTag {Name = "long", IsPreset = true},
                    new GlobalCatOrTag {Name = "other protagonist", IsPreset = true},
                    new GlobalCatOrTag {Name = "short", IsPreset = true},
                    new GlobalCatOrTag {Name = "unknown author", IsPreset = true}
                }
            }
        };

        // Don't say this = PresetTags; that will make it a reference and we don't want that. It will be deep
        // copied later.
        internal static readonly GlobalCatAndTagsList GlobalTags = new GlobalCatAndTagsList();
    }
}
