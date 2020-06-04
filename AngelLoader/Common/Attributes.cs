using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static class Attributes
    {
        // All attributes marked with a conditional based on a define that doesn't exist, so they won't be compiled
        // (we only need these for pre-build code generation)

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotSerializeAttribute : Attribute { }

        /// <summary>
        /// This attribute should be used on the localization class. Only one instance of this attribute should
        /// be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationClassAttribute : Attribute { }

        /// <summary>
        /// This attribute should be placed on the localization ini read/write class. Only one instance of this
        /// attribute should be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationReadWriteClassAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Enum)]
        internal class FenGenGameSourceEnumAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenCommentAttribute : Attribute
        {
            internal FenGenCommentAttribute([UsedImplicitly] string comment) { }
        }

        // Yes, Roslyn is so bonkers-idiotic that I have to make an entire attribute just for this. Amazing.
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenBlankLineAttribute : Attribute
        {
            public FenGenBlankLineAttribute() { }
            public FenGenBlankLineAttribute([UsedImplicitly] int numberOfBlankLines) { }
        }
    }
}
