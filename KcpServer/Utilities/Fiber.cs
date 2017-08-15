using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class Fiber
    {
        public int fiberid = -1;
        FiberPool fp = null;
        internal void AttachFiberPool(FiberPool fp)
        {
            this.fp = fp;
        }

        public Fiber(FiberPool fp, int fiberidSeed)
        {
            AttachFiberPool(fp);
            this.fiberid = Math.Abs(fiberidSeed) % fp.GetWorkerCount;
        }
        //ConcurrentQueue<Task> _works = new ConcurrentQueue<Task>();
        public void Enqueue(Action t)
        {
            fp.Enqueue(fiberid, t);
        }


    }
}
