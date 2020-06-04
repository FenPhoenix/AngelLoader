using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static class Attributes
    {
        // All attributes marked with a conditional based on a define that doesn't exist, so they won't be compiled
        // (we only need these for pre-build code generation)

        /// <summary>
        /// The generator will ignore this field or property and will not generate any code from it.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenIgnoreAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotConvertDateTimeToLocalAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotTrimValueAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenNumericEmptyAttribute : Attribute
        {
            public FenGenNumericEmptyAttribute([UsedImplicitly] long value) { }
        }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenListTypeAttribute : Attribute
        {
            public FenGenListTypeAttribute([UsedImplicitly] string value) { }
        }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenIniNameAttribute : Attribute
        {
            public FenGenIniNameAttribute([UsedImplicitly] string value) { }
        }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenInsertAfterAttribute : Attribute
        {
            public FenGenInsertAfterAttribute([UsedImplicitly] string value) { }
        }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenWriteEmptyValuesAttribute : Attribute
        {
            internal FenGenWriteEmptyValuesAttribute(bool value) { }
        }

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
