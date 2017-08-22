using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer.Lite
{
    /// <summary>
    /// 自身不开任何线程的UdpServer
    /// </summary>
    public class UdpServerLite
    {
        public Action<string> DebugLog = (s) =>
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        };
        ConnectionManager cm;
        ToServerPackBuilder defpb;
        ApplicationBase App { get => connMan.App; }
        ConnectionManager connMan = null;

        public void Service()
        {
            UpdateSocket();
            cm.CheckTimeout();
        }

        public void Start(ServerConfig sc)
        {
            cm = ConnectionManager.Create(sc.MaxPlayer)
                .SetSysId(sc.SysId)
                .SetApplicationData(sc.AppId)
                .BindApplication(sc.App)
                .SetTimeout(sc.Timeout)
                ;
            defpb = new ToServerPackBuilder(cm._SysId, 0);
            connMan = cm;
            initSocket(sc);
            sc.App.SetLocalEndPoint(udp.LocalEndPoint);
            sc.App.Setup();
        }
        public void Close(TimeSpan ts)
        {
            
            cm.SyncClose(ts);
        }


        #region CreateSocket
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
        Socket udp;
        Socket initSocket(ServerConfig sc)
        {
            if (udp != null) { throw new InvalidOperationException("init twice"); }
            udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool bNewBehavior = false;
            byte[] dwBytesReturned = new byte[4];
            udp.IOControl((int)SIO_UDP_CONNRESET, BitConverter.GetBytes(bNewBehavior), dwBytesReturned);
            udp.Blocking = false;
            udp.Bind(sc.Localipep);
            DebugLog($"udp socket inited {udp.LocalEndPoint}");
            return udp;
        }

        void closeSocket(TimeSpan ts)
        {
            if (udp != null)
            {
                udp.Close((int)ts.TotalMilliseconds);
                udp = null;
            }
        }
        #endregion

        DateTime lastUpdateSocketTime = DateTime.Now;
        Byte[] udprecbuff = new byte[Utilities.ToServerPackBuilder.MAX_RECBUFF_LEN];
        EndPoint recipep = new IPEndPoint(0, 0);
        void UpdateSocket()
        {
            #region update udp
            if (udp == null) return;
            if (udp.Available > 0)
            {
                int udplen = 0;
                do
                {
                    udplen = udp.ReceiveFrom(udprecbuff, ref recipep);
                    if (udplen > 0)
                    {
                        ProcessUdpData(udprecbuff, udplen, recipep);
                    }

                } while (udp.Available > 0);//udplen > 0 &&

            }

            #endregion
            lastUpdateSocketTime = DateTime.Now;
        }

        private void ProcessUdpData(byte[] udprecbuff, int udplen, EndPoint recipep)
        {
            var len = defpb.Read(udprecbuff, out byte[] data, out int sid, out byte[] sysbuff);
            if (len < 0)
            {
                //bad sysid.
                DebugLog("bad sysid:" + string.Join("", sysbuff));
            }
            else if (len >= 0)
            {
                //查看sid是不是0
                if (sid == 0)
                {
                    //请求握手
                    var appdata = new byte[len];
                    Array.Copy(udprecbuff, appdata, len);
                    var x = new PeerContext() { ApplicationData = appdata, RemoteEP = recipep, LocalEP = udp.LocalEndPoint, SessionId = 0, ConnectionManager = connMan };
                    if (!connMan.App.PreCreatePeer(x))
                    {
                        DebugLog("app refuse connect request");
                        byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.APP_REFUSED);
                        //ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                        udp.SendTo(hsbuff, recipep);
                        return;
                    }
                    var newsid = connMan.EnumANewPeerId();
                    if (newsid == ConnectionManager.MAX_CONN_EXCEED)
                    {
                        DebugLog("server refuse connect request");
                        byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.SERVER_REFUSED);
                        //ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                        udp.SendTo(hsbuff, recipep);
                        return;
                    }
                    else
                    {
                        x.SessionId = newsid;
                        BuildCodecsBeforePlayerCreated(x);
                        PeerBase p = connMan.App.CreatePeer(x);
                        if (p == null)
                        {
                            connMan.RecycleSession(newsid);
                            DebugLog("app refuse create player");
                            byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.APP_REFUSED2);
                            //ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                            udp.SendTo(hsbuff, recipep);
                            return;
                        }
                        else
                        {
                            p.LastPackTime = DateTime.Now;
                            p.Context.RemoteEP = recipep;
                            p.Context.LocalEP = udp.LocalEndPoint;
                            p.Channel = new SendChannel(udp);
                            byte[] hsbuff = defpb.MakeHandshakeReturn(newsid);
                            //ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                            udp.SendTo(hsbuff, recipep);
                            connMan.AddConn(p);
                            DebugLog($"{nameof(ConnectionManager)}:new session established {newsid}");
                        }
                    }

                }
                else
                {
                    //data arrival
                    var peer = connMan.FindPeer(sid);
                    if (peer == null)
                    {
                        DebugLog($"cant find peer:{sid}");
                    }
                    else
                    {
                        peer.LastPackTime = DateTime.Now;
                        if (len == 0)
                        {
                            //heartbeat
                            Console.WriteLine($"{peer.Context.SessionId} hb@{peer.LastPackTime}");
                        }
                        else
                        {
                            var recdata = new byte[len];
                            Array.Copy(data, 0, recdata, 0, len);
                            ProcessIncomingData(peer, recdata);
                        }

                    }
                }
            }

        }

        private void ProcessIncomingData(PeerBase peer, byte[] recdata)
        {
            peer.AddRecData(recdata);
        }

        protected virtual void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            x.Codec = new Codec.CodecBase();
        }
    }
}
