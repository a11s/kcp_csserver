using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer
{
    public abstract class ApplicationBase
    {
        ConnectionManager ConnMan;
        private EndPoint _localEndPoint;

        internal void SetConnMan(ConnectionManager cm)
        {
            ConnMan = cm;
        }

        public bool ApplicationRunning { get; internal set; } = true;
        public EndPoint LocalEndPoint { get => _localEndPoint; /*set => localEndPoint = value;*/ }

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

        internal void SetLocalEndPoint(EndPoint localEndPoint)
        {
            this._localEndPoint = localEndPoint;
        }

        
    }


}
