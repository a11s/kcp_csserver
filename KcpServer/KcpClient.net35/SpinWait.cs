using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Threading
{
    internal struct SpinWait
    {
        public void SpinOnce()
        {
            Thread.Sleep(0);
        }
    }
}
