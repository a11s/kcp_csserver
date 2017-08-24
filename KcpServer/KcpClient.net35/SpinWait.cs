using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Threading
{
    internal class SpinWait
    {
        public void SpinOnce()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {

            }
            Thread.Sleep(0);
        }
        ISpinOnce spin = null;
        public SpinWait()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    this.spin = new WinSpin();
                    break;
                case PlatformID.Unix:
                    this.spin = new LinuxSpin();
                    break;
                case PlatformID.MacOSX:
                    throw new NotImplementedException($"not ready for {Environment.OSVersion.Platform}");
                    break;
                default:
                    throw new NotImplementedException($"not support for {Environment.OSVersion.Platform}");
                    break;
            }
        }

        interface ISpinOnce
        {
            void SpinOnce();
        }
        class WinSpin : ISpinOnce
        {
            public void SpinOnce()
            {
                Thread.Sleep(0);
            }
        }
        class LinuxSpin : ISpinOnce
        {
            public void SpinOnce()
            {
                throw new NotImplementedException();
            }
        }
    }
}
