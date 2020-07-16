// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    [UsedImplicitly]
    internal class ActivationContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ActivationContextSafeHandle() : base(true) { }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            NativeMethods.ReleaseActCtx(handle);
            return true;
        }
    }
}
