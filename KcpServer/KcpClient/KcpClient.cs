
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using k = kcpwarpper;
using static kcpwarpper.KCP;
using System.Runtime.InteropServices;
namespace KcpClient
{
    public unsafe class KcpClient : UdpClient
    {
#if PRINTPACK
        void printpack(string s)
        {
            Console.WriteLine(s);
        }
#endif
        k.IKCPCB* kcp = null;
        public KcpClient(byte[] arr, int sid, byte[] appData) : base(arr, sid, appData)
        {

        }
        k.d_output realsend;
        void initKcp()
        {
            if (kcp != null)
            {
                ikcp_release(kcp);
                kcp = null;
            }
            var errno = ikcp_create((uint)this.SessionId, (void*)0);
            if ((int)errno == -1)
            {
                throw new NullReferenceException("init codec failed");
            }
            kcp = errno;
            realsend = new k.d_output(udp_output);
            kcp->output = Marshal.GetFunctionPointerForDelegate(realsend);
            ikcp_wndsize(kcp, 128, 128);
            ikcp_nodelay(kcp, 1, 10, 2, 1);
            kcp->rx_minrto = 10;
            kcp->fastresend = 1;
            kcp->mtu = Utilities.ToServerPackBuilder.MAX_DATA_LEN;//可能还要浪费几个字节
        }
        /// <summary>
        /// real send
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <param name="kcp"></param>
        /// <param name="user"></param>
        /// <returns></returns>

        int udp_output(byte* buf, int len, k.IKCPCB* kcp, void* user)
        {
            byte[] buff = new byte[len];
            Marshal.Copy(new IntPtr(buf), buff, 0, len);
#if PRINTPACK
            printpack($"kcp_output:{buff.Length}:{string.Join(",",buff)}");
#endif
            Outgoing.Enqueue(buff);
            return 0;
        }
        protected override void ProcessIncomingData(byte[] data)
        {
#if PRINTPACK
            printpack($"ikcp_input:{data.Length}:{string.Join(",", data)}");
#endif
            fixed (byte* p = &data[0])
            {
                ikcp_input(kcp, p, data.Length);
            }
        }

        protected override void ProcessOutgoingData(byte[] buff)
        {
            //give to kcp to determin how to send
            if (kcp == null)
            {
                throw new NullReferenceException();
            }
            fixed (byte* p = &buff[0])
            {
                var ret = ikcp_send(kcp, p, buff.Length);
            }
        }
        byte[] kb = new byte[1400];
        protected override void AfterService()
        {
            if (kcp == null)
            {
                return;
            }
            ikcp_update(kcp, (uint)Environment.TickCount);
            fixed (byte* p = &kb[0])
            {
                int kcnt = 0;
                do
                {
                    kcnt = ikcp_recv(kcp, p, kb.Length);
                    if (kcnt > 0)
                    {
                        var data = new byte[kcnt];
                        Array.Copy(kb, data, kcnt);
                        Incoming.Enqueue(data);
                    }
                } while (kcnt > 0);
            }
        }
        protected override void OnHandShake()
        {
            base.OnHandShake();
            initKcp();
        }
    }
}
