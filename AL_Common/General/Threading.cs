using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AL_Common;

public static partial class Common
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ParallelForData<T>
    {
        public readonly ConcurrentQueue<T> CQ;
        public readonly ParallelOptions PO;

        public ParallelForData(ConcurrentQueue<T> cq, ParallelOptions po)
        {
            CQ = cq;
            PO = po;
        }

        public ParallelForData()
        {
            CQ = new ConcurrentQueue<T>();
            PO = new ParallelOptions();
        }
    }

    public static bool TryGetParallelForData<T>(
        int threadCount,
        IEnumerable<T> items,
        CancellationToken cancellationToken,
        out ParallelForData<T> pd)
    {
        if (threadCount is 0 or < -1)
        {
            pd = new ParallelForData<T>();
            return false;
        }

        try
        {
            pd = new ParallelForData<T>(
                cq: new ConcurrentQueue<T>(items),
                po: new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = threadCount,
                }
            );

            return true;
        }
        catch
        {
            pd = new ParallelForData<T>();
            return false;
        }
    }
}
