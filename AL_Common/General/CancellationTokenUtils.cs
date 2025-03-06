using System;
using System.Threading;
using JetBrains.Annotations;

namespace AngelLoader;

public static partial class Utils
{
    public static void CancelIfNotDisposed(this CancellationTokenSource value)
    {
        try { value.Cancel(); } catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Disposes and assigns a new one.
    /// </summary>
    /// <param name="cts"></param>
    /// <returns></returns>
    [MustUseReturnValue]
    public static CancellationTokenSource Recreate(this CancellationTokenSource cts)
    {
        cts.Dispose();
        return new CancellationTokenSource();
    }
}
