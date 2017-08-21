using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Lite
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

        public abstract void Setup();

        public abstract void TearDown();

        internal void Close()
        {
            if (ApplicationRunning)
            {                
                TearDown();
                ApplicationRunning = false;
            }
        }
    }
}
