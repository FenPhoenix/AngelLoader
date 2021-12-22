using System;
using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable RCS1139 // Add summary element to documentation comment.

namespace AngelLoader
{
    [PublicAPI]
    internal static class FenGenAttributes
    {
        // IMPORTANT (Attributes):
        // Do NOT change the names of any FenGen attributes without also going in to FenGen and changing their
        // names there. Otherwise FenGen will break!

        // -All attributes are marked with a conditional based on a define that doesn't exist, so they won't be
        //  compiled (we only need these for pre-build code generation).
        // -Conditionals are literals instead of a constant, because a constant would add something to the exe
        //  but we don't want anything extra at all.

        #region Config

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
        internal sealed class FenGen_ConfigReadAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Method)]
        internal sealed class FenGen_Config_PerGameGetterAttribute : Attribute
        {
            /// <summary>
            /// Specifies that this is a per-game getter.
            /// </summary>
            /// <param name="iniName">If in ini it's "T1UseSteam" then this would be "UseSteam"</param>
            /// <param name="suffix">If true, it will be like "UseSteamT1", otherwise "T1UseSteam"</param>
            internal FenGen_Config_PerGameGetterAttribute(string iniName, bool suffix = false) { }
        }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Method)]
        internal sealed class FenGen_Config_PerGameSetterAttribute : Attribute
        {
            /// <summary>
            /// Specifies that this is a per-game setter.
            /// </summary>
            /// <param name="iniName">If in ini it's "T1UseSteam" then this would be "UseSteam"</param>
            /// <param name="suffix">If true, it will be like "UseSteamT1", otherwise "T1UseSteam"</param>
            internal FenGen_Config_PerGameSetterAttribute(string iniName, bool suffix = false) { }
        }

        #endregion

        #region Serialization

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal sealed class FenGenFMDataSourceClassAttribute : Attribute
        {
            /// <param name="writeEmptyValues">
            /// Specifies whether empty values will be written to the ini.
            /// What constitutes "empty" will vary depending on type.
            /// </param>
            internal FenGenFMDataSourceClassAttribute(bool writeEmptyValues) { }
        }

        /// <summary>
        /// This attribute should be placed on the FMData ini read/write class. Only one instance of this
        /// attribute should be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal sealed class FenGenFMDataDestClassAttribute : Attribute { }

        /// <summary>
        /// The generator will ignore this field or property and will not generate any code from it.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
        internal sealed class FenGenIgnoreAttribute : Attribute { }

        /// <summary>
        /// The generator will create code to read this field, but not to write it.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenDoNotWriteAttribute : Attribute { }

        /// <summary>
        /// If this field or property should have a different name in the ini file, you can specify that name here.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenIniNameAttribute : Attribute
        {
            internal FenGenIniNameAttribute(string value) { }
        }

        /// <summary>
        /// Specifies a number to be considered "empty" for the purposes of writeout. Empty values will not be
        /// written to the ini file when <see cref="FenGenFMDataSourceClassAttribute"/>'s writeEmptyValues
        /// parameter is set to <see langword="false"/>.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenNumericEmptyAttribute : Attribute
        {
            internal FenGenNumericEmptyAttribute(long value) { }
        }

        /// <summary>
        /// List type can be "MultipleLines" or "CommaSeparated".
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenListTypeAttribute : Attribute
        {
            /// <param name="value">Can be "MultipleLines" or "CommaSeparated".</param>
            internal FenGenListTypeAttribute(string value) { }
        }

        /// <summary>
        /// List type can be "None", "Exact", or "CaseInsensitive".
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenListDistinctTypeAttribute : Attribute
        {
            /// <param name="value">Can be "None", "Exact", or "CaseInsensitive".</param>
            internal FenGenListDistinctTypeAttribute(string value) { }
        }

        /// <summary>
        /// Specifies that the value of this field or property (assumed to be a string) will not have whitespace
        /// trimmed from the end of it on read.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenDoNotTrimValueAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenDoNotConvertDateTimeToLocalAttribute : Attribute { }

        #endregion

        #region Localizable text

        /// <summary>
        /// This attribute should be used on the localization class. Only one instance of this attribute should
        /// be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal sealed class FenGenLocalizationSourceClassAttribute : Attribute { }

        /// <summary>
        /// This attribute should be placed on the localization ini read/write class. Only one instance of this
        /// attribute should be used, or else FenGen will throw an error.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal sealed class FenGenLocalizationDestClassAttribute : Attribute { }

        /// <summary>
        /// Places a comment before the attached field or property.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenCommentAttribute : Attribute
        {
            /// <param name="comments">Each comment will be placed on a separate line.</param>
            internal FenGenCommentAttribute(params string[] comments) { }
        }

        /// <summary>
        /// Cheap and crappy way to specify blank lines that should be written to the lang ini, until I can
        /// figure out a way to detect blank lines properly in FenGen.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenBlankLineAttribute : Attribute
        {
            internal FenGenBlankLineAttribute() { }
            internal FenGenBlankLineAttribute(int numberOfBlankLines) { }
        }

        /// <summary>
        /// Notates that the next n entries are per-game fields, where n is the number of entries in the <see cref="GameSupport.GameIndex"/> enum.
        /// </summary>
        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal sealed class FenGenGameSetAttribute : Attribute
        {
            internal FenGenGameSetAttribute(string getterName) { }
        }

        #endregion

        #region Game support

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Enum)]
        internal sealed class FenGenGameEnumAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class FenGenNotAGameTypeAttribute : Attribute { }

        #endregion

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Class)]
        internal sealed class FenGenBuildDateDestClassAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class FenGenDoNotRemoveTextAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class FenGenDoNotRemoveHeaderTextAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class FenGenDoNotRemoveToolTipTextAttribute : Attribute { }

        [Conditional("compile_FenGen_attributes")]
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class FenGenForceRemoveSizeAttribute : Attribute { }
    }
}
