using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AngelLoader.Forms.WinFormsNative.Dialogs;

// Prevents the file from opening in design view when there's nothing to design
[System.ComponentModel.DesignerCategory("")]
public sealed class VistaFolderBrowserDialog : CommonDialog
{
    public VistaFolderBrowserDialog() => Reset();

    #region Public properties

    public string Title = "";

    /// <summary>
    /// Gets or sets the initial directory displayed when the dialog is shown.
    /// If null, empty, or otherwise invalid, the default directory will be used.
    /// </summary>
    public string InitialDirectory = "";

    /// <summary>
    /// Gets the selected directory. If <see cref="MultiSelect"/> is true, this will be the first selected directory.
    /// </summary>
    public string DirectoryName { get; private set; } = "";

    /// <summary>
    /// Gets the selected directories. If <see cref="MultiSelect"/> is false, this will contain only one directory.
    /// </summary>
    public readonly List<string> DirectoryNames = new();

    /// <summary>
    /// Gets or sets a value that determines whether the user can select more than one file.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool MultiSelect;

    #endregion

    #region Public methods

    /// <summary>
    /// Resets all properties to their default values.
    /// </summary>
    public override void Reset()
    {
        Title = "";
        InitialDirectory = "";
        DirectoryName = "";
        MultiSelect = false;
    }

    #endregion

    #region Protected methods

    protected override bool RunDialog(nint hwndOwner)
    {
        NativeFolderBrowserDialog? dialog = null;
        try
        {
            dialog = new NativeFolderBrowserDialog();

            SetDialogProperties(dialog);

            HResult result = dialog.Show(hwndOwner);
            if (result != HResult.Ok)
            {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (result == HResult.Canceled)
                {
                    return false;
                }
                else
                {
                    // I think I want to just ignore exceptions and cancel if anything at all went wrong, but
                    // just in case I change my mind, here's the line to use:
                    // throw Marshal.GetExceptionForHR((int)result);
                    return false;
                }
            }

            SetPropertiesFromDialogResult(dialog);

            return true;
        }
        finally
        {
            if (dialog != null) Marshal.FinalReleaseComObject(dialog);
        }
    }

    #endregion

    #region Private methods

    private void SetDialogProperties(NativeFolderBrowserDialog dialog)
    {
        NativeMethods.FOS flags =
            NativeMethods.FOS.FOS_PICKFOLDERS |
            NativeMethods.FOS.FOS_FORCEFILESYSTEM |
            NativeMethods.FOS.FOS_PATHMUSTEXIST |
            NativeMethods.FOS.FOS_FILEMUSTEXIST |
            NativeMethods.FOS.FOS_NOVALIDATE;

        if (MultiSelect) flags |= NativeMethods.FOS.FOS_ALLOWMULTISELECT;

        dialog.SetOptions(flags);

        if (!Title.IsEmpty())
        {
            dialog.SetTitle(Title);
        }

        if (!InitialDirectory.IsWhiteSpace())
        {
            if (!Directory.Exists(InitialDirectory))
            {
                try
                {
                    // C:\Folder\File.exe becomes C:\Folder
                    InitialDirectory = Path.GetDirectoryName(InitialDirectory) ?? "";
                    if (!Directory.Exists(InitialDirectory)) return;
                }
                // Fix: we weren't checking for invalid path names
                catch
                {
                    return;
                }
            }

            Guid guid = new(Guids.IShellItem);

            HResult result = NativeMethods.SHCreateItemFromParsingName(
                InitialDirectory,
                0,
                ref guid,
                out object item);

            if (result != HResult.Ok)
            {
                throw Marshal.GetExceptionForHR((int)result) ?? new Exception($"Folder browser dialog exception:{NL}" + result);
            }

            dialog.SetFolder((IShellItem)item);
        }
    }

    private void SetPropertiesFromDialogResult(NativeFolderBrowserDialog dialog)
    {
        dialog.GetResults(out IShellItemArray resultsArray);

        DirectoryNames.Clear();

        resultsArray.GetCount(out uint count);
        for (uint i = 0; i < count; i++)
        {
            resultsArray.GetItemAt(i, out IShellItem result);

            HResult hr = result.GetDisplayName(NativeMethods.SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out nint ppszName);

            if (hr == HResult.Ok && ppszName != 0)
            {
                DirectoryNames.Add(Marshal.PtrToStringAuto(ppszName));
            }

            Marshal.FreeCoTaskMem(ppszName);
        }

        if (DirectoryNames.Count > 0) DirectoryName = DirectoryNames[0];
    }

    #endregion
}
