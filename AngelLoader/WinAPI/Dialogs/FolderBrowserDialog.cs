using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Dialogs
{
    [PublicAPI]
    public sealed class AutoFolderBrowserDialog : IDisposable
    {
        /// <summary>
        /// The good folder browser dialog if we're on Vista or above, or the crappy one if we're not
        /// </summary>
        public AutoFolderBrowserDialog() => Reset();

        public DialogResult ShowDialog()
        {
            DialogResult result;

            // Windows Vista is version 6.0
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= new Version(6, 0))
            {
                using var d = new VistaFolderBrowserDialog { InitialDirectory = InitialDirectory, MultiSelect = true };

                if (d.ShowDialog() == DialogResult.OK)
                {
                    DirectoryName = d.DirectoryName;
                    DirectoryNames.Clear();
                    foreach (string dir in d.DirectoryNames) DirectoryNames.Add(dir);
                    result = DialogResult.OK;
                }
                else
                {
                    result = DialogResult.Cancel;
                }
            }
            else
            {
                // Fallback for pre-Vista, probably won't ever be hit seeing as we require Windows 7 or above,
                // and even if we went back to .NET 4.6 we would still require Vista. But hey.
                using var d = new FolderBrowserDialog { SelectedPath = InitialDirectory, ShowNewFolderButton = true };

                if (d.ShowDialog() == DialogResult.OK)
                {
                    DirectoryName = d.SelectedPath;
                    DirectoryNames.Clear();
                    DirectoryNames.Add(d.SelectedPath);
                    result = DialogResult.OK;
                }
                else
                {
                    result = DialogResult.Cancel;
                }
            }

            return result;
        }

        #region Public properties

        /// <summary>
        /// Gets or sets the initial directory displayed when the dialog is shown.
        /// If null, empty, or otherwise invalid, the default directory will be used.
        /// </summary>
        public string InitialDirectory { get; set; } = "";

        /// <summary>
        /// Gets the selected directory. If <see cref="MultiSelect"/> is true, this will be the first selected directory.
        /// </summary>
        public string DirectoryName { get; private set; } = "";

        /// <summary>
        /// Gets the selected directories. If <see cref="MultiSelect"/> is false, this will contain only one directory.
        /// </summary>
        public List<string> DirectoryNames { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a value that determines whether the user can select more than one file.
        /// The default is <see langword="false"/>.
        /// </summary>
        public bool MultiSelect { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public void Reset()
        {
            InitialDirectory = "";
            DirectoryName = "";
            MultiSelect = false;
        }

        #endregion

        public void Dispose() { }
    }

    // Prevents the file from opening in design view when there's nothing to design
    [System.ComponentModel.DesignerCategory("")]
    public sealed class VistaFolderBrowserDialog : CommonDialog
    {
        public VistaFolderBrowserDialog() => Reset();

        #region Public properties

        /// <summary>
        /// Gets or sets the initial directory displayed when the dialog is shown.
        /// If null, empty, or otherwise invalid, the default directory will be used.
        /// </summary>
        public string InitialDirectory { get; set; } = "";

        /// <summary>
        /// Gets the selected directory. If <see cref="MultiSelect"/> is true, this will be the first selected directory.
        /// </summary>
        public string DirectoryName { get; private set; } = "";

        /// <summary>
        /// Gets the selected directories. If <see cref="MultiSelect"/> is false, this will contain only one directory.
        /// </summary>
        public List<string> DirectoryNames { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a value that determines whether the user can select more than one file.
        /// The default is <see langword="false"/>.
        /// </summary>
        public bool MultiSelect { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public override void Reset()
        {
            InitialDirectory = "";
            DirectoryName = "";
            MultiSelect = false;
        }

        #endregion

        #region Protected methods

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            NativeFolderBrowserDialog? dialog = null;
            try
            {
                dialog = new NativeFolderBrowserDialog();

                SetDialogProperties(dialog);

                HResult result = dialog.Show(hwndOwner);
                if (result != HResult.Ok)
                {
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
            var flags = NativeMethods.FOS.FOS_PICKFOLDERS |
                        NativeMethods.FOS.FOS_FORCEFILESYSTEM |
                        NativeMethods.FOS.FOS_PATHMUSTEXIST |
                        NativeMethods.FOS.FOS_FILEMUSTEXIST |
                        NativeMethods.FOS.FOS_NOVALIDATE;

            if (MultiSelect) flags |= NativeMethods.FOS.FOS_ALLOWMULTISELECT;

            dialog.SetOptions(flags);

            if (!string.IsNullOrWhiteSpace(InitialDirectory))
            {
                if (!Directory.Exists(InitialDirectory))
                {
                    // C:\Folder\File.exe becomes C:\Folder
                    InitialDirectory = Path.GetDirectoryName(InitialDirectory);
                    if (!Directory.Exists(InitialDirectory)) return;
                }

                var guid = new Guid(Guids.IShellItem);

                HResult result = NativeMethods.SHCreateItemFromParsingName(
                    InitialDirectory,
                    IntPtr.Zero,
                    ref guid,
                    out object item);

                if (result != HResult.Ok) throw Marshal.GetExceptionForHR((int)result);

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

                HResult hr = result.GetDisplayName(NativeMethods.SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out IntPtr ppszName);

                if (hr == HResult.Ok && ppszName != IntPtr.Zero)
                {
                    DirectoryNames.Add(Marshal.PtrToStringAuto(ppszName));
                }

                Marshal.FreeCoTaskMem(ppszName);
            }

            if (DirectoryNames.Count > 0) DirectoryName = DirectoryNames[0];
        }

        #endregion
    }
}
