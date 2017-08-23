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
    /// <summary>
    /// 这个支持不可靠协议
    /// </summary>
    public unsafe abstract class KcpPeerEx : KcpPeerBase
    {
        public KcpPeerEx(PeerContext pc) : base(pc) { }
        protected override unsafe int udp_output(byte* buf, int len, k.IKCPCB* kcp, void* user)
        {
            //一定是reliable
            byte[] kcppack = new byte[len + 1];
            Marshal.Copy(new IntPtr(buf), kcppack, 1, len);
            kcppack[0] = (byte)PackType.Kcp;
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
            if (buf.Length > 0)
            {
                var type = (Utilities.PackType)buf[0];
                switch (type)
                {
                    case PackType.Udp:
                        var data = new byte[buf.Length - 1];
                        Array.Copy(buf, 1, data, 0, data.Length);
                        OnOperationRequest(data);
                        break;
                    case PackType.Kcp:
                        fixed (byte* p = &buf[1])
                        {
                            ikcp_input(this.Context.EncoderData, p, buf.Length - 1);
                        }
                        break;
                    default:
                        throw new UnknownTypeException("unknown packtype");
                        break;
                }
            }
        }

        protected override void BeforeSendOutgoing(byte[] data)
        {
            //这里的data已经不是用户数据,已经包含了reliable
            var type = (Utilities.PackType)data[0];
            switch (type)
            {
                case PackType.Udp:
                    var sendbuf = new byte[PackSettings.HEADER_LEN + data.Length];
                    defpb.Write(sendbuf, data, 0, data.Length);
                    OutgoingData.Enqueue(sendbuf);
                    break;
                case PackType.Kcp:
                    //交给kcp需要去掉第一个字节，不是直接发
                    fixed (byte* b = &data[1])
                    {
                        ikcp_send(this.Context.EncoderData, b, data.Length-1);
                    }
                    break;
                default:
                    throw new UnknownTypeException("unknown packtype");
                    break;
            }
        }

        public override void SendOperationResponse(byte[] data)
        {
            SendOperationResponse(data, false);
        }
        public void SendOperationResponse(byte[] data, bool unreliable)
        {
            var senddata = new byte[data.Length + 1];
            Array.Copy(data, 0, senddata, 1, data.Length);
            if (unreliable)
            {
                senddata[0] = (byte)Utilities.PackType.Udp;
            }
            else
            {
                senddata[0] = (byte)Utilities.PackType.Kcp;
            }
            BeforeSendOutgoing(senddata);
        }
    }

    public class UnknownTypeException : Exception
    {
        public UnknownTypeException()
        {

        }
        public UnknownTypeException(string s) : base(s)
        {

        }
    }
}
