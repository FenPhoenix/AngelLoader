using System;
using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable RCS1139 // Add summary element to documentation comment.

namespace AngelLoader;

[PublicAPI]
internal static class FenGenAttributes
{
    /*
    IMPORTANT (Attributes):
    Do NOT change the names of any FenGen attributes without also going in to FenGen and changing their
    names there. Otherwise FenGen will break!

    -All attributes are marked with a conditional based on a define that doesn't exist, so they won't be
     compiled (we only need these for pre-build code generation).
    -Conditionals are literals instead of a constant, because a constant would add something to the exe
     but we don't want anything extra at all.

    -The ones that aren't commented, hopefully you can figure out what they do by looking at where they're
     used or whatever.
    */

    #region Serialization

    /// <summary>
    /// Place this on the class that is to be the source of "FM data" generation (should be the FanMission class).
    /// Only one instance of this attribute should be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenFMDataSourceClassAttribute : Attribute { }

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
    /// written to the ini file.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenNumericEmptyAttribute : Attribute
    {
        internal FenGenNumericEmptyAttribute(long value) { }
    }

    /// <summary>
    /// A perf/alloc optimization that's only for numeric fields where you don't need to parse negatives<br/>
    /// (because it won't work with them). If you specify a maximum number of digits with this attribute,<br/>
    /// the codegen will create code that can parse the value out of the line without taking the extra<br/>
    /// allocation of a substring of the value. If you don't specify this attribute, the value will still<br/>
    /// be read correctly but there will be an extra allocation.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenMaxDigitsAttribute : Attribute
    {
        internal FenGenMaxDigitsAttribute(int value) { }
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
    /// Specifies that the value of this field or property (assumed to be a string) will not have whitespace
    /// trimmed from the end of it on read.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenDoNotTrimValueAttribute : Attribute { }

    /// <summary>
    /// Hack: To be placed on DateTime fields that we don't have want to take the perf hit of doing anything<br/>
    /// with until later.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenDoNotConvertDateTimeToLocalAttribute : Attribute { }

    /// <summary>
    /// Hack: Special case to be placed on the readme encoding field only
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenReadmeEncodingAttribute : Attribute { }

    /// <summary>
    /// Quick hack to tell FenGen not to substring the value from a key-value pair line, because it's going<br/>
    /// to be passed to a method that's designed to work with the entire line, and we can save an allocation.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class FenGenDoNotSubstringAttribute : Attribute { }

    /// <summary>
    /// Hack: Tells FenGen to treat this Flags-enum field as if it's a single-value-at-a-time (non-flags)<br/>
    /// field when it generates read/write code. Only applies to SelectedLang at the moment.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal sealed class FenGenFlagsSingleAssignment : Attribute { }

    #endregion

    #region Localizable text

    /// <summary>
    /// This attribute should be placed on the class that should contain generated getter methods for localized<br/>
    /// per-game strings. Only one instance of this attribute should be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenLocalizedGameNameGetterDestClassAttribute : Attribute { }

    /// <summary>
    /// This attribute should be used on the localization class. Only one instance of this attribute should
    /// be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenLocalizationSourceClassAttribute : Attribute { }

    /// <summary>
    /// Places a comment before the attached field or property.<br/>
    /// For multiple-line comments, each line should be a separate parameter. Concatenating parameters is<br/>
    /// not supported and will not achieve the desired effect.
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
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenGameSupportMainGenDestClassAttribute : Attribute { }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    internal sealed class FenGenGameEnumAttribute : Attribute
    {
        internal FenGenGameEnumAttribute(string gameIndexEnumName) { }
    }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class FenGenGameAttribute : Attribute
    {
        internal FenGenGameAttribute(string prefix, string steamId, string editorName) { }
    }

    #endregion

    #region Language support

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenLanguageSupportDestClassAttribute : Attribute { }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    internal sealed class FenGenLanguageEnumAttribute : Attribute
    {
        internal FenGenLanguageEnumAttribute(string languageIndexEnumName) { }
    }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class FenGenLanguageAttribute : Attribute
    {
        internal FenGenLanguageAttribute(string langCodes, string translatedName) { }
    }

    #endregion

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenBuildDateDestClassAttribute : Attribute { }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FenGenCurrentYearDestClassAttribute : Attribute { }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class FenGenForceRemoveSizeAttribute : Attribute { }
}