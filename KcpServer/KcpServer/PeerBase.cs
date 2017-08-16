using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Utilities;

namespace KcpServer
{
    public abstract class PeerBase
    {
        Fiber _Fiber = null;
        private DateTime lastPackTime;
        public DateTime LastPackTime { get => lastPackTime; set => lastPackTime = value; }

        public int SessionId { get => this.Context.SessionId; }
        public PeerContext Context { get; internal set; }
        public Fiber Fiber { get => _Fiber; /*set => _Fiber = value;*/ }
        public IChannel Channel { get; internal set; }

        ConcurrentQueue<byte[]> IncomingData = new ConcurrentQueue<byte[]>();
        ConcurrentQueue<byte[]> OutgoingData = new ConcurrentQueue<byte[]>();

        ToServerPackBuilder defpb;

        public virtual void OnDisconnect(DateTime lastPackTime, TimeSpan t)
        {
            //do nothing
            Console.WriteLine("连接断了");
        }
        internal void OnTimeout(DateTime lastPackTime, TimeSpan t)
        {
            this.OnDisconnect(lastPackTime, t);
            var sendbuf = defpb.MakeTimeoutReturn(ToServerPackBuilder.SERVER_TIMEOUT,SessionId);
            this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(sendbuf.Length).WriteBytes(sendbuf), Context.RemoteEP));
        }


        internal void AddRecData(byte[] recdata)
        {
            IncomingData.Enqueue(recdata);
        }
        /// <summary>
        /// client sent data to server
        /// </summary>
        /// <param name="data"></param>
        public abstract void OnOperationRequest(byte[] data);
        /// <summary>
        /// send to client
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int SendOperationResponse(byte[] data)
        {
            var sendbuf = new byte[ToServerPackBuilder.HEADER_LEN + data.Length];
            defpb.Write(sendbuf, data, 0, data.Length);
            OutgoingData.Enqueue(sendbuf);
            return 0;
        }

        public PeerBase(PeerContext pc)
        {
            this.Context = pc;
            var fp = pc.ConnectionManager.Workfiberpool;
            defpb = new ToServerPackBuilder(pc.ConnectionManager.SysId, pc.SessionId);
            this._Fiber = new Fiber(fp, this.GetHashCode());
        }

        internal void UpdateInternal()
        {
            while (IncomingData.TryDequeue(out var buf))
            {
                OnOperationRequest(buf);
            }
            while (OutgoingData.TryDequeue(out var buf2))
            {
                this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(buf2.Length).WriteBytes(buf2), Context.RemoteEP));
            }
        }
    }
}
