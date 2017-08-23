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
        ThreadPoolFiber _fiber = null;
        private DateTime lastPackTime;
        public DateTime LastPackTime { get => lastPackTime; set => lastPackTime = value; }

        public int SessionId { get => this.Context.SessionId; }
        public PeerContext Context { get; internal set; }
        public ThreadPoolFiber Fiber { get => _fiber; /*set => _Fiber = value;*/ }
        public IChannel Channel { get; internal set; }

        protected ConcurrentQueue<byte[]> IncomingData = new ConcurrentQueue<byte[]>();
        protected ConcurrentQueue<byte[]> OutgoingData = new ConcurrentQueue<byte[]>();

        protected ServerPackBuilderEx defpb;
        Codec.CodecBase defEncoder;

        public virtual void OnDisconnect(DateTime lastPackTime, TimeSpan t)
        {
            //do nothing
            Console.WriteLine("connection timeout");
        }
        internal void OnTimeout(DateTime lastPackTime, TimeSpan t)
        {
            this.OnDisconnect(lastPackTime, t);
            var sendbuf = defpb.MakeTimeoutReturn((int)ClientErrorCode.SERVER_TIMEOUT, SessionId);
            this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(sendbuf.Length).WriteBytes(sendbuf), Context.RemoteEP));

            this.Context.Codec.Close();
        }

        /// <summary>
        /// from udp server.
        /// </summary>
        /// <param name="recdata"></param>
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
        public virtual void SendOperationResponse(byte[] data)
        {
            BeforeSendOutgoing(data);
        }

        public PeerBase(PeerContext pc)
        {
            this.Context = pc;
            var fp = pc.ConnectionManager.Workfiberpool;
            defpb = new ServerPackBuilderEx(pc.ConnectionManager.SysId, pc.SessionId);
            defEncoder = pc.Codec;

            this._fiber = new ThreadPoolFiber(fp, this.GetHashCode());
        }

        internal void UpdateInternal()
        {
            while (IncomingData.TryDequeue(out var buf))
            {
                BeforeOperationRequest(buf);

            }
            while (OutgoingData.TryDequeue(out var buf2))
            {
#if PRINTPACK
                Console.WriteLine($"realsend:{buf2.Length}:{string.Join(",", buf2)}");
#endif
                this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(buf2.Length).WriteBytes(buf2), Context.RemoteEP));
            }

            DeriverUpdate();
        }

        protected virtual void DeriverUpdate()
        {
            //do nothing
        }

        protected virtual void BeforeOperationRequest(byte[] buf)
        {
            OnOperationRequest(buf);
        }

        protected virtual void BeforeSendOutgoing(byte[] data)
        {
            var sendbuf = new byte[PackSettings.HEADER_LEN + data.Length];
            defpb.Write(sendbuf, data, 0, data.Length);
            OutgoingData.Enqueue(sendbuf);
        }
    }
}
