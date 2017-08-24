using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Utilities;

namespace KcpClient
{
    /// <summary>
    /// 先弄个简单能用效率低的,调好了服务器再回来优化客户端
    /// </summary>
    public class UdpClient
    {
        protected ConcurrentQueue<byte[]> Incoming;
        protected ConcurrentQueue<byte[]> Outgoing;
        Socket udp;
        IPEndPoint remote_ipep;
        //IPEndPoint local_ipep;
        ServerPackBuilderEx defpb;
        Thread ioThread;
        byte[] applicationData;
        byte[] heartbeatData = new byte[0];

        bool _connected = false;
        public UdpClient(byte a, byte b, byte c, byte d, int sid, byte[] appData)
        {
            defpb = new ServerPackBuilderEx(a, b, c, d, sid);
            SessionId = sid;
            applicationData = appData;
        }

        public UdpClient(byte[] arr, int sid, byte[] appData)
        {
            defpb = new ServerPackBuilderEx(arr, sid);
            SessionId = sid;
            applicationData = appData;
        }

        public static Action<string> debug = (s) =>
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        };

        public Action<ClientErrorCode> OnError = (e) =>
        {
            debug?.Invoke($"{nameof(OnError)}:{e}");
        };
        public Action OnDisconnect = () =>
        {
            debug?.Invoke($"{nameof(OnDisconnect)},server lost");
        };
        public Action<byte[]> OnOperationResponse = (b) =>
        {
            debug?.Invoke("data arrival");
        };


        public Action<int> OnConnected = (sid) =>
        {
            debug?.Invoke($"{nameof(OnConnected)} {sid}");
        };

        public int SessionId { get; private set; }

        public virtual void SendOperationRequest(byte[] buff)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("not connected");
            }
            ProcessOutgoingData(buff, 0, buff.Length);

        }



        public void Service()
        {
            if (udp == null) return;
            while (Incoming != null && Incoming.TryDequeue(out var ibuff))
            {
                OnOperationResponse?.Invoke(ibuff);
            }
            AfterService();
        }

        protected virtual void AfterService()
        {
            //for derived Class
        }

        static HashSet<int> IOThreads = new HashSet<int>();

        #region IOControl
        const uint IOC_VOID = 0x20000000;/* no parameters */
        const uint IOC_OUT = 0x40000000; /* copy out parameters */
        const uint IOC_IN = 0x80000000;/* copy in parameters */
        const uint IOC_UNIX = 0x00000000;
        const uint IOC_WS2 = 0x08000000;
        const uint IOC_PROTOCOL = 0x10000000;
        const uint IOC_VENDOR = 0x18000000;
        uint _WSAIO(uint x, uint y) => (IOC_VOID | x | y);
        uint _WSAIOR(uint x, uint y) => (IOC_OUT | (x) | (y));
        uint _WSAIOW(uint x, uint y) => (IOC_IN | (x) | (y));
        //uint _WSAIORW(uint x, uint y) => (IOC_INOUT | (x) | (y));
        uint SIO_UDP_CONNRESET => _WSAIOW(IOC_VENDOR, 12);

        #endregion
        public bool Connected { get => _connected; private set => _connected = value; }

        public void Connect(string ip, int port, bool CreateBackgroundThread)
        {
            Connect(new IPEndPoint(IPAddress.Parse(ip), port), CreateBackgroundThread);
        }

        public void Connect(IPEndPoint ipep, bool CreateBackgroundThread)
        {
            IOThreads.Clear();
            lastHandshakeTime = DateTime.MinValue;
            Incoming = new ConcurrentQueue<byte[]>();
            Outgoing = new ConcurrentQueue<byte[]>();
            udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool bNewBehavior = false;
            byte[] dwBytesReturned = new byte[4];
            udp.IOControl((int)SIO_UDP_CONNRESET, BitConverter.GetBytes(bNewBehavior), dwBytesReturned);
            defpb = new ServerPackBuilderEx(defpb.GetSysIdBuf(), SessionId);
            remote_ipep = ipep;
            udp.Connect(remote_ipep);
            if (CreateBackgroundThread)
            {
                ioThread = new Thread(ioLoop);
                ioThread.IsBackground = true;
                ioThread.Name = $"{nameof(ioThread)}";
                IOThreads.Add(ioThread.ManagedThreadId);                
                ioThread.Start();
            }
            debug?.Invoke($"start connect");
        }

        public virtual void Close()
        {
            IOThreads.Clear();
            Incoming = null;
            Outgoing = null;
            defpb = null;
            udp.Close();
        }

        DateTime lastHandshakeTime = DateTime.Now;
        protected void Handshake()
        {
            if (DateTime.Now.Subtract(lastHandshakeTime).TotalSeconds < 1)
            {
                return;
            }
            byte[] hsbuff = defpb.GetSysIdBuf();
            byte[] appdata = applicationData;
            var tspb = new ServerPackBuilderEx(hsbuff, 0);
            byte[] sendbuff = new byte[PackSettings.HEADER_LEN + appdata.Length];
            tspb.Write(sendbuff, appdata, 0, appdata.Length);
            udp.SendTo(sendbuff, remote_ipep);
            lastHandshakeTime = DateTime.Now;
            debug?.Invoke($"begin {nameof(Handshake)}");
            _connected = true;
        }

        DateTime lastHartbeatTime = DateTime.Now;
        protected void Heartbeat()
        {

            if (DateTime.Now.Subtract(lastHartbeatTime).TotalSeconds < 1)
            {
                return;
            }

            byte[] sendbuff = new byte[PackSettings.HEADER_LEN + heartbeatData.Length];
            defpb.Write(sendbuff, heartbeatData, 0, heartbeatData.Length);
            udp.SendTo(sendbuff, remote_ipep);
            lastHartbeatTime = DateTime.Now;
        }

        EndPoint remoteIpep = new IPEndPoint(0, 0);
        void ioLoop()
        {
            var tid = Thread.CurrentThread.ManagedThreadId;
            debug?.Invoke($"Thread {tid} start");
            SpinWait sw = new SpinWait();
            while (IOThreads.Contains(tid))
            {
                DoWork();
                sw.SpinOnce();
            }
            debug?.Invoke($"Thread {tid} exit");
        }

        public void DoWork()
        {
            while (udp.Available > 0)
            {
                byte[] buff = new byte[udp.Available];
                var cnt = udp.ReceiveFrom(buff, ref remoteIpep);
                var datasize = defpb.Read(buff, out var data, out int sid, out var sysbuff);
                if (datasize > 0)
                {
                    //data arrival
                    if (sid == 0)
                    {
                        //握手协议
                        var tmp = BitConverter.ToInt32(data, 0);
                        if (tmp > 0)
                        {
                            this.SessionId = tmp;
                            defpb = new ServerPackBuilderEx(defpb.GetSysIdBuf(), this.SessionId);
                            debug?.Invoke($"{nameof(Handshake)}:{nameof(SessionId)}={SessionId}");
                            OnHandShake();

                        }
                        else
                        {
                            //error code                                
                            var errcode = BitConverter.ToInt32(data, 0);
                            InternalError(errcode);

                        }
                    }
                    else
                    {
                        ProcessIncomingData(data, 0, data.Length);

                    }
                }
                else if (datasize == 0)
                {
                    //heartbeat from server, reserved
                }
                else
                {
                    //error
                    InternalError(datasize);
                }
            }
            while (Outgoing != null && Outgoing.TryDequeue(out var sbuff))
            {
                var sndbuf = new byte[PackSettings.HEADER_LEN + sbuff.Length];
                defpb.Write(sndbuf, sbuff, 0, sbuff.Length);
                udp.SendTo(sndbuf, remote_ipep);
#if PRINTPACK
                    Console.WriteLine($"realsend:{sndbuf.Length}");
#endif
            }
            if (this.SessionId == 0)
            {
                Handshake();
            }
            else
            {
                Heartbeat();
            }
        }

        protected virtual void OnHandShake()
        {
            OnConnected?.Invoke(this.SessionId);

        }

        protected virtual void ProcessIncomingData(byte[] data, int start, int len)
        {
            if (start == 0)
            {
                Incoming.Enqueue(data);
            }
            else
            {
                var buf = new byte[len];
                Array.Copy(data, start, buf, 0, len);
                Incoming.Enqueue(buf);
            }
        }
        protected virtual void ProcessOutgoingData(byte[] data, int start, int len)
        {
            if (start == 0)
            {
                Outgoing.Enqueue(data);
            }
            else
            {
                var buf = new byte[len];
                Array.Copy(data, start, buf, 0, len);
                Outgoing.Enqueue(buf);
            }
        }

        private void InternalError(int datasize)
        {
            Connected = false;
            var cec = (ClientErrorCode)datasize;
            switch (cec)
            {

                case ClientErrorCode.SERVER_TIMEOUT:
                    Console.WriteLine("server think you timeout");
                    Close();
                    OnDisconnect();
                    break;
                default:
                    OnError.Invoke(cec);
                    break;
            }
        }
    }
}
