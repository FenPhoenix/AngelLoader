#define FenGen_UpdaterLocalizationSource

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Update;

#region Attributes

/// <summary>
/// This attribute should be used on the localization class. Only one instance of this attribute should
/// be used, or else FenGen will throw an error.
/// </summary>
[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Class)]
file sealed class FenGenLocalizationSourceClassAttribute : Attribute { }

[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Field)]
file sealed class FenGenPlaceAfterKeyAttribute(string key) : Attribute { }

/// <summary>
/// Cheap and crappy way to specify blank lines that should be written to the lang ini, until I can
/// figure out a way to detect blank lines properly in FenGen.
/// </summary>
[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
file sealed class FenGenBlankLineAttribute : Attribute
{
    public FenGenBlankLineAttribute() { }
    public FenGenBlankLineAttribute(int numberOfBlankLines) { }
}

#endregion

[FenGenLocalizationSourceClass]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
internal sealed class LocalizationData
{
    internal readonly Global_Class Global = new();
    internal readonly AlertMessages_Class AlertMessages = new();
    internal readonly Update_Class Update = new();

    internal sealed class Global_Class
    {
        internal readonly string OK = "OK";
        internal readonly string Cancel = "Cancel";
        [FenGenPlaceAfterKey("Cancel")]
        internal readonly string Retry = "Retry";
    }

    internal sealed class AlertMessages_Class
    {
        internal readonly string Alert = "Alert";
        internal readonly string Error = "Error";
    }

    internal sealed class Update_Class
    {
        [FenGenBlankLine]
        internal readonly string Copying = "Copying...";
        internal readonly string RollingBack = "Could not complete copy; rolling back to old version...";
        internal readonly string RollbackFailed = "The update failed and we tried to restore the old version, but that failed too. It's recommended to download the latest version of AngelLoader and re-install it manually.";
        internal readonly string UnableToStartAngelLoader = "Unable to start AngelLoader. You'll need to start it manually.";
        internal readonly string UnableToCompleteBackup = "Update failed: Unable to complete backup of current app files.";
    }
}
