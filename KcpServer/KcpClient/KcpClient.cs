using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace KcpClient
{
    /// <summary>
    /// 先弄个简单能用效率低的,调好了服务器再回来优化客户端
    /// </summary>
    public class KcpClient
    {
        ConcurrentQueue<byte[]> Incoming;
        ConcurrentQueue<byte[]> Outgoing;
        Socket udp;
        IPEndPoint remote_ipep;
        IPEndPoint local_ipep;
        ToServerPackBuilder defpb;
        Thread IOThread;
        byte[] applicationData;
        byte[] heartbeatData = new byte[0];
        public KcpClient(byte a, byte b, byte c, byte d, int sid, byte[] appData)
        {
            defpb = new ToServerPackBuilder(a, b, c, d, sid);
            SessionId = sid;
            applicationData = appData;
        }

        public KcpClient(byte[] arr, int sid, byte[] appData)
        {
            defpb = new ToServerPackBuilder(arr, sid);
            SessionId = sid;
            applicationData = appData;
        }
        public Action OnDisconnect = () =>
        {
#if DEBUG
            Console.WriteLine("断了,服务器不认了");
#endif
        };
        public Action<byte[]> OnOperationResponse = (b) =>
        {
#if DEBUG
            Console.WriteLine("数据来了");
#endif
        };
        public Action<string> debug = (s) =>
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        };

        public ConcurrentQueue<byte[]> Incoming1 { get => Incoming; set => Incoming = value; }
        public int SessionId { get; private set; }

        public void SendOperationRequest(byte[] buff)
        {
            Outgoing.Enqueue(buff);
        }
        public void Service()
        {
            if (udp == null) return;
            while (Incoming.TryDequeue(out var ibuff))
            {
                OnOperationResponse?.Invoke(ibuff);
            }

        }

        static HashSet<int> IOThreads = new HashSet<int>();
        public void Connect(string ip, int port)
        {
            Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        public void Connect(IPEndPoint ipep)
        {
            IOThreads.Clear();
            Incoming = new ConcurrentQueue<byte[]>();
            Outgoing = new ConcurrentQueue<byte[]>();
            udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            defpb = new ToServerPackBuilder(defpb.GetSysIdBuf(), SessionId);
            remote_ipep = ipep;
            udp.Connect(remote_ipep);
            IOThread = new Thread(IOLoop);
            IOThread.IsBackground = true;
            IOThread.Name = $"{nameof(IOThread)}";
            IOThreads.Add(IOThread.ManagedThreadId);
            //Thread.SetData()
            IOThread.Start();
            debug($"start connect");

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
            ToServerPackBuilder tspb = new ToServerPackBuilder(hsbuff, 0);
            byte[] sendbuff = new byte[ToServerPackBuilder.HEADER_LEN + appdata.Length];
            tspb.Write(sendbuff, appdata, 0, appdata.Length);
            udp.SendTo(sendbuff, remote_ipep);
            lastHandshakeTime = DateTime.Now;
            debug($"begin {nameof(Handshake)}");
        }

        DateTime lastHartbeatTime = DateTime.Now;
        protected void Hartbeat()
        {

            if (DateTime.Now.Subtract(lastHartbeatTime).TotalSeconds < 1)
            {
                return;
            }

            byte[] sendbuff = new byte[ToServerPackBuilder.HEADER_LEN + heartbeatData.Length];
            defpb.Write(sendbuff, heartbeatData, 0, heartbeatData.Length);
            udp.SendTo(sendbuff, remote_ipep);
            lastHartbeatTime = DateTime.Now;
        }

        void IOLoop()
        {
            var tid = Thread.CurrentThread.ManagedThreadId;
            debug($"Thread {tid} start");
            EndPoint ep = new IPEndPoint(0, 0);
            SpinWait sw = new SpinWait();
            while (IOThreads.Contains(tid))
            {
                while (udp.Available > 0)
                {
                    byte[] buff = new byte[udp.Available];
                    var cnt = udp.ReceiveFrom(buff, ref ep);
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
                                defpb = new ToServerPackBuilder(defpb.GetSysIdBuf(), this.SessionId);
                                debug($"{nameof(Handshake)}:{nameof(SessionId)}={SessionId}");
                            }
                            else
                            {
                                //error code
                                throw new NotImplementedException($"{nameof(IOLoop)} reserved");
                                var errcode = BitConverter.ToInt32(data, sizeof(int));
                                InternalError(errcode);

                            }
                        }
                        else
                        {
                            Incoming.Enqueue(data);
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
                while (Outgoing.TryDequeue(out var sbuff))
                {
                    var sndbuf = new byte[ToServerPackBuilder.HEADER_LEN + sbuff.Length];
                    defpb.Write(sndbuf, sbuff, 0, sbuff.Length);
                    udp.SendTo(sndbuf, remote_ipep);
                }
                sw.SpinOnce();
                if (this.SessionId == 0)
                {
                    Handshake();
                }
                else
                {
                    //Hartbeat();
                }
            }
            debug($"Thread {tid} exit");
        }

        private void InternalError(int datasize)
        {
            switch (datasize)
            {
                case ToServerPackBuilder.APP_REFUSED:
                case ToServerPackBuilder.APP_REFUSED2:
                    Console.WriteLine("拒绝建立连接");
                    break;
                case ToServerPackBuilder.BAD_SYSID:
                    Console.WriteLine("错误的协议");
                    break;
                case ToServerPackBuilder.SERVER_TIMEOUT:
                    Console.WriteLine("服务器认为你超时了");
                    OnDisconnect();

                    break;
                default:
                    break;
            }
        }
    }
}
