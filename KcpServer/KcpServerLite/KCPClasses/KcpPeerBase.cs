using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using k = kcpwarpper;
using static kcpwarpper.KCP;
using System.Runtime.InteropServices;
using Utilities;

namespace KcpServer.Lite
{
    public unsafe abstract class KcpPeerBase : PeerBase
    {
        k.IKCPCB* kcp
        {
            get
            {
                return pc.EncoderData;
            }
        }
        k.d_output realsend;
        PeerContext pc;
        public KcpPeerBase(PeerContext pc) : base(pc)
        {
            this.pc = pc;
            //kcp create放在这里比较合适
            if (kcp == null)
            {
                throw new NullReferenceException("kcp codec lost");
            }
            realsend = new k.d_output(udp_output);
            pc.EncoderData->output = Marshal.GetFunctionPointerForDelegate(realsend);
        }
        protected virtual int udp_output(byte* buf, int len, k.IKCPCB* kcp, void* user)
        {
            byte[] kcppack = new byte[len];
            Marshal.Copy(new IntPtr(buf), kcppack, 0, len);
#if PRINTPACK
            Console.WriteLine($"kcp_output:size{kcppack.Length}:{string.Join(",", kcppack)}");
#endif
            var sendbuf = new byte[PackSettings.HEADER_LEN + kcppack.Length];
            defpb.Write(sendbuf, kcppack, 0, kcppack.Length);
            OutgoingData.Enqueue(sendbuf);
            return 0;
        }
        protected override void BeforeOperationRequest(byte[] buf)
        {
#if PRINTPACK
            Console.WriteLine($"ikcp_input:size{buf.Length}:{string.Join(",", buf)}");
            Console.WriteLine($"rec:{buf.Length}");
#endif
            fixed (byte* p = &buf[0])
            {
                ikcp_input(this.Context.EncoderData, p, buf.Length);
            }
        }

        protected override void BeforeSendOutgoing(byte[] data)
        {
            if (kcp == null) return;
            fixed (byte* b = &data[0])
            {
                ikcp_send(this.Context.EncoderData, b, data.Length);
            }
        }
        byte[] recbuf = new byte[Utilities.PackSettings.MAX_RECBUFF_LEN];
        protected override void DeriverUpdate()
        {
            if (kcp == null) return;
            fixed (byte* b = &recbuf[0])
            {
                int kcnt = 0;
                do
                {
                    kcnt = ikcp_recv(kcp, b, recbuf.Length);
                    if (kcnt > 0)
                    {
                        byte[] data = new byte[kcnt];
                        Array.Copy(recbuf, data, kcnt);
                        OnOperationRequest(data);

                    }
                } while (kcnt > 0);
            }
            ikcp_update(kcp, (uint)Environment.TickCount);
        }


    }
}
