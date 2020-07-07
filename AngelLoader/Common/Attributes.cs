using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static class Attributes
    {
        // IMPORTANT!
        // Do NOT change the names of any FenGen attributes without also going in to FenGen and changing their
        // names there. Otherwise FenGen will break!

        // -All attributes are marked with a conditional based on a define that doesn't exist, so they won't be
        //  compiled (we only need these for pre-build code generation).
        // -Conditionals are literals instead of a constant, because a constant would add something to the exe
        //  but we don't want anything extra at all.

        #region Serialization

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenFMDataSourceClassAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="writeEmptyValues">
            /// Specifies whether empty values will be written to the ini.
            /// What constitutes "empty" will vary depending on type.
            /// </param>
            internal FenGenFMDataSourceClassAttribute([UsedImplicitly] bool writeEmptyValues) { }
        }

        /// <summary>
        /// The generator will ignore this field or property and will not generate any code from it.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenIgnoreAttribute : Attribute { }

        /// <summary>
        /// If this field or property should have a different name in the ini file, you can specify that name here.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenIniNameAttribute : Attribute
        {
            internal FenGenIniNameAttribute([UsedImplicitly] string value) { }
        }

        /// <summary>
        /// Specifies a number to be considered "empty" for the purposes of writeout. Empty values will not be
        /// written to the ini file when <see cref="FenGenFMDataSourceClassAttribute"/>'s writeEmptyValues
        /// parameter is set to <see langword="false"/>.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenNumericEmptyAttribute : Attribute
        {
            internal FenGenNumericEmptyAttribute([UsedImplicitly] long value) { }
        }

        /// <summary>
        /// List type can be "MultipleLines" or "CommaSeparated".
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenListTypeAttribute : Attribute
        {
            /// <param name="value">Can be "MultipleLines" or "CommaSeparated".</param>
            internal FenGenListTypeAttribute([UsedImplicitly] string value) { }
        }

        /// <summary>
        /// List type can be "None", "Exact", or "CaseInsensitive".
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenListDistinctTypeAttribute : Attribute
        {
            /// <param name="value">Can be "None", "Exact", or "CaseInsensitive".</param>
            internal FenGenListDistinctTypeAttribute([UsedImplicitly] string value) { }
        }

        /// <summary>
        /// Specifies that the value of this field or property (assumed to be a string) will not have whitespace
        /// trimmed from the end of it on read.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotTrimValueAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenDoNotConvertDateTimeToLocalAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenInsertAfterAttribute : Attribute
        {
            internal FenGenInsertAfterAttribute([UsedImplicitly] string value) { }
        }

        #endregion

        #region Localizable text

        /// <summary>
        /// This attribute should be used on the localization class. Only one instance of this attribute should
        /// be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationSourceClassAttribute : Attribute { }

        /// <summary>
        /// This attribute should be placed on the localization ini read/write class. Only one instance of this
        /// attribute should be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationDestClassAttribute : Attribute { }

        /// <summary>
        /// Places a comment before the attached field or property.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenCommentAttribute : Attribute
        {
            /// <param name="comments">Each comment will be placed on a separate line.</param>
            internal FenGenCommentAttribute([UsedImplicitly] params string[] comments) { }
        }

        /// <summary>
        /// Cheap and crappy way to specify blank lines that should be written to the lang ini, until I can
        /// figure out a way to detect blank lines properly in FenGen.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class FenGenBlankLineAttribute : Attribute
        {
            internal FenGenBlankLineAttribute() { }
            internal FenGenBlankLineAttribute([UsedImplicitly] int numberOfBlankLines) { }
        }

        #endregion

        #region Game support

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Enum)]
        internal class FenGenGameEnumAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal class FenGenNotAGameTypeAttribute : Attribute { }

        #endregion
    }
}
