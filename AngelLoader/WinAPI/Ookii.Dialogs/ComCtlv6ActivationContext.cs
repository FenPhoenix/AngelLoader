﻿// NULL_TODO
#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal sealed class ComCtlv6ActivationContext : IDisposable
    {
        // Private data
        private IntPtr _cookie;
        private static NativeMethods.ACTCTX _enableThemingActivationContext;
        private static ActivationContextSafeHandle _activationContext;
        private static bool _contextCreationSucceeded;
        private static readonly object _contextCreationLock = new object();

        internal ComCtlv6ActivationContext(bool enable)
        {
            if (enable && NativeMethods.IsWindowsXPOrLater)
            {
                if (EnsureActivateContextCreated())
                {
                    if (!NativeMethods.ActivateActCtx(_activationContext, out _cookie))
                    {
                        // Be sure cookie always zero if activation failed
                        _cookie = IntPtr.Zero;
                    }
                }
            }
        }

        ~ComCtlv6ActivationContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_cookie != IntPtr.Zero)
            {
                if (NativeMethods.DeactivateActCtx(0, _cookie))
                {
                    // deactivation succeeded...
                    _cookie = IntPtr.Zero;
                }
            }
        }

        private static bool EnsureActivateContextCreated()
        {
            lock (_contextCreationLock)
            {
                if (!_contextCreationSucceeded)
                {
                    // Pull manifest from the .NET Framework install
                    // directory

                    // TODO: Fen's note: This looks sketchy. Step through and see if this works on Core 3...
                    var assemblyLoc = typeof(object).Assembly.Location;

                    string manifestLoc = null;
                    string installDir = null;
                    if (assemblyLoc != null)
                    {
                        installDir = Path.GetDirectoryName(assemblyLoc);
                        const string manifestName = "XPThemes.manifest";
                        manifestLoc = Path.Combine(installDir, manifestName);
                    }

                    if (manifestLoc != null && installDir != null)
                    {
                        _enableThemingActivationContext = new NativeMethods.ACTCTX
                        {
                            cbSize = Marshal.SizeOf(typeof(NativeMethods.ACTCTX)),
                            lpSource = manifestLoc,
                            // Set the lpAssemblyDirectory to the install
                            // directory to prevent Win32 Side by Side from
                            // looking for comctl32 in the application
                            // directory, which could cause a bogus dll to be
                            // placed there and open a security hole.
                            lpAssemblyDirectory = installDir,
                            dwFlags = NativeMethods.ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID
                        };

                        // Note this will fail gracefully if file specified
                        // by manifestLoc doesn't exist.
                        _activationContext = NativeMethods.CreateActCtx(ref _enableThemingActivationContext);
                        _contextCreationSucceeded = !_activationContext.IsInvalid;
                    }
                }

                // If we return false, we'll try again on the next call into
                // EnsureActivateContextCreated(), which is fine.
                return _contextCreationSucceeded;
            }
        }
    }
}
