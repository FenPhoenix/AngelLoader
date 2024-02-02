﻿#define FenGen_UpdaterLocalizationSource

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
        internal readonly string Error_ViewLog = "View log";
    }

    internal sealed class Update_Class
    {
        [FenGenBlankLine]
        // @Update: Make these lines clearer and more succinct...
        internal readonly string PreparingToUpdate = "Preparing to update...";
        internal readonly string CopyingFiles = "Copying files...";
        internal readonly string RestoringOldFiles = "Restoring old files...";
        internal readonly string UpdateFailed = "The update failed. AngelLoader will remain at its current version.";
        internal readonly string UpdateCanceled = "The update was canceled. AngelLoader will remain at its current version.";
        // @Update(Rollback failed): Improve phrasing
        internal readonly string RollbackFailed = "The update failed, and the old app files couldn't be restored.";
        internal readonly string CanceledAndRollbackFailed = "The update was canceled, but the old app files couldn't be restored.";
        // @Update: Put a link to download the latest version here?
        internal readonly string RecommendManualUpdate = "It's recommended to download the latest version of AngelLoader and install it manually.";
        internal readonly string UnableToStartAngelLoader = "AngelLoader couldn't be restarted. You'll need to start it manually.";
    }
}
