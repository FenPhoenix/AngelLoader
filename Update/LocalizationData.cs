using System.Diagnostics.CodeAnalysis;

namespace Update;

[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
internal sealed class LocalizationData
{
    internal static readonly LocalizationData LText = new();

    internal readonly Global_Class Global = new();
    internal readonly AlertMessages_Class AlertMessages = new();
    internal readonly Update_Class Update = new();

    internal sealed class Global_Class
    {
        internal readonly string OK = "OK";
        internal readonly string Cancel = "Cancel";
        internal readonly string Retry = "Retry";
    }

    internal sealed class AlertMessages_Class
    {
        internal readonly string Alert = "Alert";
        internal readonly string Error = "Error";
    }

    internal sealed class Update_Class
    {
        internal readonly string Copying = "Copying...";
        internal readonly string RollingBack = "Could not complete copy; rolling back to old version...";
        internal readonly string RollbackFailed = "The update failed and we tried to restore the old version, but that failed too. It's recommended to download the latest version of AngelLoader and re-install it manually.";
        internal readonly string UnableToStartAngelLoader = "Unable to start AngelLoader. You'll need to start it manually.";
        internal readonly string UnableToCompleteBackup = "Update failed: Unable to complete backup of current app files.";
    }
}
