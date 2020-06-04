using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static class Attributes
    {
        // -All attributes marked with a conditional based on a define that doesn't exist, so they won't be
        //  compiled (we only need these for pre-build code generation).
        // -Conditionals are literals instead of a constant, because a constant would add something to the exe
        //  but we don't want anything extra at all.

        /// <summary>
        /// The generator will ignore this field or property and will not generate any code from it.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenIgnoreAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotConvertDateTimeToLocalAttribute : Attribute { }

        /// <summary>
        /// Specifies that the value of this field or property (assumed to be a string) will not have whitespace
        /// trimmed from the end of it on read.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotTrimValueAttribute : Attribute { }

        /// <summary>
        /// Specifies a number to be considered "empty" for the purposes of writeout. Empty values will not be
        /// written to the ini file when <see cref="FenGenWriteEmptyValuesAttribute"/> is set to
        /// <see langword="false"/>.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenNumericEmptyAttribute : Attribute
        {
            public FenGenNumericEmptyAttribute([UsedImplicitly] long value) { }
        }

        /// <summary>
        /// List type can be "MultipleLines" or "CommaSeparated".
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenListTypeAttribute : Attribute
        {
            public FenGenListTypeAttribute([UsedImplicitly] string value) { }
        }

        /// <summary>
        /// If this field or property should have a different name in the ini file, you can specify that name here.
        /// </summary>
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

        /// <summary>
        /// Specifies whether empty values will be written to the ini.
        /// What constitutes "empty" will vary depending on type.
        /// </summary>
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
