using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static kcpwarpper.KCP;
namespace KcpServer.Lite.Codec
{
    public unsafe class KcpCodec : CodecBase
    {
        private PeerContext x;
        public KcpCodec(PeerContext x)
        {
            this.x = x;
            CreateKcp(x);

        }
        public virtual int MaxKcpPackSize { get => Utilities.PackSettings.MAX_DATA_LEN; }
        public void CreateKcp(PeerContext x)
        {
            if (x.EncoderData == null)
            {
                var errno = ikcp_create((uint)x.SessionId, (void*)0);
                if ((int)errno != -1)
                {
                    x.EncoderData = errno;
#if PRINTPACK
                    Console.WriteLine($"create kcp {(int)x.EncoderData}");
#endif
                    var kcp = x.EncoderData;
                    /*该调用将会设置协议的最大发送窗口和最大接收窗口大小，默认为32. 这个可以理解为 TCP的 SND_BUF 和 RCV_BUF，只不过单位不一样 SND/RCV_BUF 单位是字节，这个单位是包。*/

                    ikcp_wndsize(kcp, 128, 128);
                    /*
                    nodelay ：是否启用 nodelay模式，0不启用；1启用。
                    interval ：协议内部工作的 interval，单位毫秒，比如 10ms或者 20ms
                    resend ：快速重传模式，默认0关闭，可以设置2（2次ACK跨越将会直接重传）
                    nc ：是否关闭流控，默认是0代表不关闭，1代表关闭。
                    普通模式：`ikcp_nodelay(kcp, 0, 40, 0, 0);
                    极速模式： ikcp_nodelay(kcp, 1, 10, 2, 1);
                     */
                    ikcp_nodelay(kcp, 1, 10, 2, 1);

                    /*最大传输单元：纯算法协议并不负责探测 MTU，默认 mtu是1400字节，可以使用ikcp_setmtu来设置该值。该值将会影响数据包归并及分片时候的最大传输单元。*/
                    ikcp_setmtu(kcp, MaxKcpPackSize);//可能还要浪费几个字节

                    /*最小RTO：不管是 TCP还是 KCP计算 RTO时都有最小 RTO的限制，即便计算出来RTO为40ms，由于默认的 RTO是100ms，协议只有在100ms后才能检测到丢包，快速模式下为30ms，可以手动更改该值：*/
                    kcp->rx_minrto = 100;
                }
                else
                {
                    x.EncoderData = null;
                    throw new InvalidCastException($"kcp create failed {(int)errno}");
                }
            }
        }


        public override void Close()
        {
            if (x.EncoderData != null)
            {
                Console.WriteLine($"release kcp {(int)x.EncoderData}");
                ikcp_release(x.EncoderData);
                x.EncoderData = null;
            }
            Console.WriteLine("kcp encoder close");
        }
    }
}
