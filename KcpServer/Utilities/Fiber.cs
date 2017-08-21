using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class ThreadPoolFiber : IFiber
    {
        public int fiberid = -1;
        FiberPool fp = null;
        internal void AttachFiberPool(FiberPool fp)
        {
            this.fp = fp;
        }

        public ThreadPoolFiber(FiberPool fp, int fiberidSeed)
        {
            AttachFiberPool(fp);
            this.fiberid = Math.Abs(fiberidSeed) % fp.GetWorkerCount;
        }
        //ConcurrentQueue<Task> _works = new ConcurrentQueue<Task>();
        public void Enqueue(Action t)
        {
            fp.Enqueue(fiberid, t);
        }

        public const int WS_FREE = 0;
        public const int WS_WORKING = 10;
        public const int WS_BUSY = 1000;
        public const int WS_DOS = 1000;

        public WorkingState State
        {
            get
            {
                var cnt = fp.GetWorkingQueueLength(this.fiberid);
                if (cnt <= WS_FREE)
                {
                    return WorkingState.Free;
                }
                if (cnt <= WS_WORKING)
                {
                    return WorkingState.Working;
                }
                if (cnt <= WS_BUSY)
                {
                    return WorkingState.Busy;
                }
                else
                {
                    return WorkingState.DenialOfService;
                }
            }
        }
    }
    /// <summary>
    /// 单线程,所以没有用并行库
    /// </summary>
    public class Fiber : IFiber
    {
        public const int WS_FREE = 0;
        public const int WS_WORKING = 10;
        public const int WS_BUSY = 1000;
        public const int WS_DOS = 1000;
        public Queue<Action> works = new Queue<Action>();
        public WorkingState State
        {
            get
            {
                var cnt = works.Count;
                if (cnt <= WS_FREE)
                {
                    return WorkingState.Free;
                }
                if (cnt <= WS_WORKING)
                {
                    return WorkingState.Working;
                }
                if (cnt <= WS_BUSY)
                {
                    return WorkingState.Busy;
                }
                else
                {
                    return WorkingState.DenialOfService;
                }
            }
        }

        public void Enqueue(Action t)
        {
            works.Enqueue(t);
        }
    }

    public enum WorkingState
    {
        Free = 0,
        Working = 1,
        Busy = 2,

        DenialOfService = 3,
    }
}
