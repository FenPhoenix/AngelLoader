using System;
using System.Diagnostics;
using JetBrains.Annotations;
#pragma warning disable CS9113 // Parameter is unread.

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060 // Remove unused parameter.
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable RCS1139 // Add summary element to documentation comment.

namespace AL_Common;

[PublicAPI]
public static class FenGenAttributes
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
    public sealed class FenGenFMDataSourceClassAttribute : Attribute;

    /// <summary>
    /// This attribute should be placed on the FMData ini read/write class. Only one instance of this
    /// attribute should be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenFMDataDestClassAttribute : Attribute;

    /// <summary>
    /// The generator will ignore this field or property and will not generate any code from it.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class FenGenIgnoreAttribute : Attribute;

    /// <summary>
    /// The generator will create code to read this field, but not to write it.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenDoNotWriteAttribute : Attribute;

    /// <summary>
    /// If this field or property should have a different name in the ini file, you can specify that name here.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenIniNameAttribute(string value) : Attribute;

    /// <summary>
    /// Specifies a number to be considered "empty" for the purposes of writeout. Empty values will not be
    /// written to the ini file.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenNumericEmptyAttribute(long value) : Attribute;

    /// <summary>
    /// A perf/alloc optimization that's only for numeric fields where you don't need to parse negatives<br/>
    /// (because it won't work with them). If you specify a maximum number of digits with this attribute,<br/>
    /// the codegen will create code that can parse the value out of the line without taking the extra<br/>
    /// allocation of a substring of the value. If you don't specify this attribute, the value will still<br/>
    /// be read correctly but there will be an extra allocation.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenMaxDigitsAttribute(int value) : Attribute;

    /// <summary>
    /// List type can be "MultipleLines" or "CommaSeparated".
    /// </summary>
    /// <param name="value">Can be "MultipleLines" or "CommaSeparated".</param>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenListTypeAttribute(string value) : Attribute;

    /// <summary>
    /// Specifies that the value of this field or property (assumed to be a string) will not have whitespace
    /// trimmed from the end of it on read.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenDoNotTrimValueAttribute : Attribute;

    /// <summary>
    /// Hack: Special case to be placed on the readme encoding field only
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenReadmeEncodingAttribute : Attribute;

    /// <summary>
    /// Quick hack to tell FenGen not to substring the value from a key-value pair line, because it's going<br/>
    /// to be passed to a method that's designed to work with the entire line, and we can save an allocation.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenDoNotSubstringAttribute : Attribute;

    /// <summary>
    /// Hack: Tells FenGen to treat this Flags-enum field as if it's a single-value-at-a-time (non-flags)<br/>
    /// field when it generates read/write code. Only applies to SelectedLang at the moment.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class FenGenFlagsSingleAssignmentAttribute : Attribute;

    #endregion

    #region Localizable text

    /// <summary>
    /// This attribute should be placed on the class that should contain generated getter methods for localized<br/>
    /// per-game strings. Only one instance of this attribute should be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenLocalizedGameNameGetterDestClassAttribute : Attribute;

    /// <summary>
    /// This attribute should be used on the localization class. Only one instance of this attribute should
    /// be used, or else FenGen will throw an error.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenLocalizationSourceClassAttribute : Attribute;

    /// <summary>
    /// Places a comment before the attached field or property.<br/>
    /// For multiple-line comments, each line should be a separate parameter. Concatenating parameters is<br/>
    /// not supported and will not achieve the desired effect.
    /// </summary>
    /// <param name="comments">Each comment will be placed on a separate line.</param>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenCommentAttribute(params string[] comments) : Attribute;

    /// <summary>
    /// Cheap and crappy way to specify blank lines that should be written to the lang ini, until I can
    /// figure out a way to detect blank lines properly in FenGen.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenBlankLineAttribute : Attribute
    {
        public FenGenBlankLineAttribute() { }
        public FenGenBlankLineAttribute(int numberOfBlankLines) { }
    }

    /// <summary>
    /// Notates that the next n entries are per-game fields, where n is the number of entries in the GameIndex enum.
    /// </summary>
    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenGameSetAttribute(string getterName) : Attribute;

    #endregion

    #region Game support

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenGameSupportMainGenDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class FenGenGameEnumAttribute(string gameIndexEnumName) : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FenGenGameAttribute(
        string prefix,
        string steamId,
        string editorName,
        bool isDarkEngine,
        bool supportsMods,
        bool supportsImport,
        bool supportsLanguages,
        bool supportsResourceDetection,
        bool requiresBackupPath) : Attribute;

    #endregion

    #region Language support

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenLanguageSupportDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class FenGenLanguageEnumAttribute(string languageIndexEnumName) : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FenGenLanguageAttribute(string langCodes, string translatedName) : Attribute;

    #endregion

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenBuildDateDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenCurrentYearDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FenGenForceRemoveSizeAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class FenGenEnumCountAttribute : Attribute
    {
        public FenGenEnumCountAttribute() { }
        public FenGenEnumCountAttribute(int plusOrMinus) { }
    }

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class FenGenEnumNamesAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenEnumDataDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenRtfDuplicateSourceClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FenGenRtfDuplicateDestClassAttribute : Attribute;

    [Conditional("compile_FenGen_attributes")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FenGenTreatAsListAttribute(string type) : Attribute;
}
