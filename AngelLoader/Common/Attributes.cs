using System;
using JetBrains.Annotations;

namespace AngelLoader.Common
{
    internal static class Attributes
    {
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationClassAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenCommentAttribute : Attribute
        {
            internal FenGenCommentAttribute([UsedImplicitly] string comment) { }
        }

        // Yes, Roslyn is so bonkers-idiotic that I have to make an entire attribute just for this. Amazing.
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenBlankLineAttribute : Attribute
        {
            public FenGenBlankLineAttribute() { }
            public FenGenBlankLineAttribute([UsedImplicitly] int numberOfBlankLines) { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationReadWriteClass : Attribute { }
    }
}
