using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer.Lite
{
    public abstract class PeerBase
    {
        Fiber _fiber = null;
        private DateTime lastPackTime;
        public DateTime LastPackTime { get => lastPackTime; set => lastPackTime = value; }

        public int SessionId { get => this.Context.SessionId; }
        public PeerContext Context { get; internal set; }
        public Fiber Fiber { get => _fiber; /*set => _Fiber = value;*/ }
        public SendChannel Channel { get; internal set; }

        protected Queue<byte[]> IncomingData = new Queue<byte[]>();
        protected Queue<byte[]> OutgoingData = new Queue<byte[]>();

        protected ServerPackBuilder defpb;
        Codec.CodecBase defEncoder;

        public virtual void OnDisconnect(DateTime lastPackTime, TimeSpan t)
        {
            //do nothing
#if DEBUG
            Console.WriteLine("connection timeout");
#endif
        }
        internal void OnTimeout(DateTime lastPackTime, TimeSpan t)
        {
            this.OnDisconnect(lastPackTime, t);
            var sendbuf = defpb.MakeTimeoutReturn((int)ClientErrorCode.SERVER_TIMEOUT, SessionId);
            //this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(sendbuf.Length).WriteBytes(sendbuf), Context.RemoteEP));
            this.Channel.Send(sendbuf, Context.RemoteEP);

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
        public void SendOperationResponse(byte[] data)
        {
            BeforeSendOutgoing(data);

        }

        public PeerBase(PeerContext pc)
        {
            this.Context = pc;
            //var fp = pc.ConnectionManager.Workfiberpool;
            defpb = new ServerPackBuilder(pc.ConnectionManager.SysId, pc.SessionId);
            defEncoder = pc.Codec;

            //this._fiber = new ThreadPoolFiber(fp, this.GetHashCode());
            _fiber = new Fiber();
        }

        internal void UpdateInternal()
        {
            while (IncomingData.Count > 0)
            {
                var buf = IncomingData.Dequeue();
                BeforeOperationRequest(buf);

            }

            #region 这里跟UdpServer不一样
            while (Fiber.works.Count > 0)
            {
                var a = Fiber.works.Dequeue();
                a.Invoke();
            }
            #endregion

            while (OutgoingData.Count > 0)
            {
                var buf2 = OutgoingData.Dequeue();
#if PRINTPACK
                Console.WriteLine($"realsend:{buf2.Length}:{string.Join(",", buf2)}");
#endif
                //this.Channel.WriteAndFlushAsync(new DotNetty.Transport.Channels.Sockets.DatagramPacket(DotNetty.Buffers.Unpooled.Buffer(buf2.Length).WriteBytes(buf2), Context.RemoteEP));
                this.Channel.Send(buf2, Context.RemoteEP);
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
