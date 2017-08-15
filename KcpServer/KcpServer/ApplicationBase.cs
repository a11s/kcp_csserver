using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer
{
    public abstract class ApplicationBase
    {
         ConnectionManager ConnMan;
        internal void SetConnMan(ConnectionManager cm)
        {
            ConnMan = cm;
        }

        public bool ApplicationRunning { get; internal set; } = true;

        public virtual bool PreCreatePeer(PeerContext peerContext)
        {
            return true;
        }
        /// <summary>
        /// if eject return null.
        /// </summary>
        /// <returns></returns>
        public abstract PeerBase CreatePeer(PeerContext peerContext);

        internal virtual void Setup()
        {
            Console.WriteLine($"{nameof(ApplicationBase)} {nameof(Setup)}");
        }

        internal virtual void TearDown()
        {
            this.ConnMan.SyncClose(TimeSpan.FromSeconds(2));
            Console.WriteLine($"{nameof(ApplicationBase)} {nameof(TearDown)}");
        }
    }

    
}
