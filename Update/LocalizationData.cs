#define FenGen_UpdaterLocalizationSource

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Update;

#region Attributes

#pragma warning disable CS9113 // Parameter is unread.
/// <summary>
/// This attribute should be used on the localization class. Only one instance of this attribute should
/// be used, or else FenGen will throw an error.
/// </summary>
[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Class)]
file sealed class FenGenLocalizationSourceClassAttribute : Attribute;

[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Field)]
file sealed class FenGenPlaceAfterKeyAttribute(string key) : Attribute;

/// <summary>
/// Cheap and crappy way to specify blank lines that should be written to the lang ini, until I can
/// figure out a way to detect blank lines properly in FenGen.
/// </summary>
[Conditional("compile_FenGen_attributes")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
file sealed class FenGenBlankLineAttribute : Attribute
{
    public FenGenBlankLineAttribute() { }
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedParameter.Local
    public FenGenBlankLineAttribute(int numberOfBlankLines) { }
}
#pragma warning restore CS9113 // Parameter is unread.

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
        internal readonly string Error_ViewLog = "View log";
    }

    internal sealed class Update_Class
    {
        [FenGenBlankLine]
        internal readonly string DoNotRunUpdateExeManually = "This executable is not meant to be run on its own. Please update from within AngelLoader.";
        internal readonly string PreparingToUpdate = "Preparing to update...";
        internal readonly string CopyingFiles = "Copying files...";
        internal readonly string RestoringOldFiles = "Restoring old files...";
        internal readonly string UpdateFailed = "The update failed. AngelLoader will remain at its current version.";
        internal readonly string UpdateCanceled = "The update was canceled. AngelLoader will remain at its current version.";
        internal readonly string RollbackFailed = "The update failed, and the old app files couldn't be restored.";
        internal readonly string CanceledAndRollbackFailed = "The update was canceled, but the old app files couldn't be restored.";
        internal readonly string RecommendManualUpdate = "It's recommended to download the latest version of AngelLoader and install it manually.";
        internal readonly string UnableToStartAngelLoader = "AngelLoader couldn't be restarted. You'll need to start it manually.";
        internal readonly string FileCopy_CouldNotCopyFile = "Couldn't copy the following file:";
        internal readonly string FileCopy_Source = "Source:";
        internal readonly string FileCopy_Destination = "Destination:";
        internal readonly string FileCopy_CloseAngelLoader = "If AngelLoader is running, close it and try again.";
    }
}
