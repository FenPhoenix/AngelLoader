//#define DESKTOP_MENU

#if DESKTOP_MENU

using System;
using System.IO;
using System.Security;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using Microsoft.Win32;
using static AngelLoader.Logger;
using static AngelLoader.Misc;
namespace AngelLoader
{
    internal static class DesktopMenu
    {
        /*
        TODO(AddUsToWindowsContextMenu):
        -Decide if we want options for "Play with AL", "Install with AL", "Add to list" etc.
        -Test with non-admin, and on Win7.
        -Do we want to always write our stuff to the registry (either add or remove our menu items) on startup?
         Or only on settings change?
        -Finalize strings (ie. "AngelLoader TEST")

        Issues:
        -With our simple registry-based approach, we can't tell Windows to batch multiple files and send them all
         at once. We get one copy of the app started for each file.
         We would need a shell extension for that, and we would want to write it native for perf and lightness
         (official word is that .NET-based shell extensions are not recommended).
         Also shell extensions apparently get perma-loaded by Windows and therefore presumably couldn't be over-
         written(?) So extracting new versions to AL's dir would produce an error for the shell extension file?
         Maybe you're supposed to put it in some central location. Don't know. Not looked into it that much yet.

        -We can't pass strings with our current PostMessage-based first-instance notification system.
         We would have to move to some kind of pipe-based system, or else we just use this as an excuse to move
         off of framework and onto .NET 6+, which has a multi-instance communication system built right in.
        */
        internal static bool AddUsToWindowsContextMenu(bool enable)
        {
            static bool SetRegExt(string ext, bool enable)
            {
                AssertR(ext.Trim('.') == ext, nameof(ext) + " was passed with a dot prefix or suffix");

                string sfaExtShellPath = @"SOFTWARE\Classes\SystemFileAssociations\." + ext + @"\shell";
                const string alKeyName = "open_with_angelloader";
                string alKeyPath = sfaExtShellPath + @"\" + alKeyName;

#region Local functions

                static RegistryKey? CreateSubKey(string path)
                {
                    RegistryKey? key = null;
                    try
                    {
                        key = Registry.CurrentUser.CreateSubKey(path, writable: true);
                    }
                    catch (SecurityException ex)
                    {
                        Log("Could not create key: " + path + "\r\n" +
                            "The user does not have the permissions required to create or open the registry key.",
                            ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log("Could not create key: " + path + "\r\n" +
                            "The registry key cannot be written to. The user may not have the necessary access rights.",
                            ex);
                    }
                    catch (IOException ex)
                    {
                        Log("Could not create key: " + path + "\r\n" +
                            "An I/O exception occurred.",
                            ex);
                    }
                    catch (Exception ex)
                    {
                        Log("Could not create key: " + path, ex);
                    }

                    if (key == null)
                    {
                        Dialogs.ShowError(ErrorText.UnableToAddItemsToExplorerMenu);
                    }

                    return key;
                }

                static bool SetValue(RegistryKey key, string name, object value)
                {
                    try
                    {
                        key.SetValue(name, value);
                    }
                    catch (SecurityException ex)
                    {
                        Log("Could not create Explorer context menu item(s)", ex);
                        Dialogs.ShowError(ErrorText.UnableToAddItemsToExplorerMenu);
                        return false;
                    }

                    return true;
                }

                static bool DeleteKey(string sfaExtShellPath, string alKeyPath, bool silentCleanupMode = false)
                {
                    RegistryKey? alKey = null;
                    try
                    {
                        try
                        {
                            alKey = Registry.CurrentUser.OpenSubKey(sfaExtShellPath, writable: true);
                        }
                        catch (Exception ex)
                        {
                            Log("Could not open key: " + sfaExtShellPath, ex);
                        }
                        if (alKey == null)
                        {
                            if (!silentCleanupMode)
                            {
                                Dialogs.ShowError(ErrorText.UnableToRemoveItemsFromExplorerMenu);
                            }
                            return false;
                        }

                        try
                        {
                            alKey.DeleteSubKeyTree(alKeyName, throwOnMissingSubKey: false);
                        }
                        catch (Exception ex)
                        {
                            Log("Could not delete SubKeyTree: " + alKeyPath, ex);
                            if (!silentCleanupMode)
                            {
                                Dialogs.ShowError(ErrorText.UnableToRemoveItemsFromExplorerMenu);
                            }
                            return false;
                        }

                        return !silentCleanupMode;
                    }
                    finally
                    {
                        alKey?.Dispose();
                    }
                }

                bool Cleanup() => DeleteKey(sfaExtShellPath, alKeyPath, silentCleanupMode: true);

#endregion

                if (enable)
                {
                    string appPathAndExe = Path.Combine(Paths.Startup, "AngelLoader.exe");

                    RegistryKey? alKey = null;
                    try
                    {
                        alKey = CreateSubKey(alKeyPath);
                        if (alKey == null ||
                            !SetValue(alKey, "", "AngelLoader TEST") ||
                            !SetValue(alKey, "Icon", appPathAndExe))
                        {
                            return Cleanup();
                        }
                    }
                    finally
                    {
                        alKey?.Dispose();
                    }

                    RegistryKey? cmdKey = null;
                    try
                    {
                        cmdKey = CreateSubKey(alKeyPath + @"\command");
                        if (cmdKey == null ||
                            !SetValue(cmdKey, "", "\"" + appPathAndExe + "\" -add_fm_archive \"%1\""))
                        {
                            return Cleanup();
                        }

                        return true;
                    }
                    finally
                    {
                        cmdKey?.Dispose();
                    }
                }
                else
                {
                    return DeleteKey(sfaExtShellPath, alKeyPath);
                }
            }

            return SetRegExt("zip", enable) && SetRegExt("7z", enable);
        }
    }
}
#endif
