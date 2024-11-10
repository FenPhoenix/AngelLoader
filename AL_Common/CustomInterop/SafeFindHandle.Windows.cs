// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AL_Common;

[UsedImplicitly]
internal sealed class SafeFindHandle : SafeHandle
{
    public SafeFindHandle() : base(IntPtr.Zero, true) { }

    protected override bool ReleaseHandle() => Interop.Kernel32.FindClose(handle);

    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);
}
